Imports System.Net
Imports System.Net.Sockets
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Runtime.InteropServices

Module ChatNetworking

    ' Declare client / server vars
    Dim tl As TcpListener
    Dim clientSock(33792) As TcpClient
    Dim serverStream(33792) As NetworkStream
    Dim serverSock(33792) As Socket
    Dim activeClient As Integer = 1
    Dim tmpClientID As Integer = 0
    Dim tmpClientHandle As String = String.Empty
    Dim srvMotd As String = ""
    Dim serverID As String = ""
    Dim firstData = True
    Public srvSettings As Ini
    Public cliSettings As Ini
    ' Initialize Garbage Collector timer
    Dim garbageCollector As New System.Timers.Timer

    ' Client error counter in case of overflow
    Dim errCounter As Integer = 0
    Dim errSpan As Integer = 0
    Dim lastError As DateTime

    ' Some constants
    Dim dsctErrs As New List(Of Integer) From {&H80131620, &H80131622}
    Dim rfsdErrs As New List(Of Integer) From {&H80004005}
    Dim crLf = Environment.NewLine
    Const encryptBadge As String = "🔒 "

    ' Stored RSA encryptors for each socket
    Dim cliRSA(32768) As SimpleRSA
    Dim srvRSA(32768) As SimpleRSA
    Dim cliAES(32768) As SimpleAes
    Dim srvAES(32768) As SimpleAes
    Dim cliVerify(32768) As String


    Function Sleep(interval As Integer)
        Dim currentTime = Now()
        While Now() < currentTime.AddMilliseconds(interval)
            Application.DoEvents()
        End While
        Return True
    End Function
    Function IsCommonPort(port As Integer)
        Dim commonPortList As New List(Of Integer) From
                {1, 5, 7, 9, 11, 13, 17, 18, 19, 20, 21,
                 22, 23, 25, 26, 35, 37, 38, 39, 41, 42,
                 43, 49, 53, 57, 67, 68, 69, 70, 79, 80,
                 81, 82, 83, 88, 101, 102, 107, 109, 110,
                 111, 113, 115, 117, 118, 119, 123, 135,
                 137, 138, 139, 143, 152, 153, 156, 157,
                 158, 159, 160, 161, 162, 170, 179, 190,
                 191, 192, 194, 201, 209, 213, 218, 220,
                 259, 264, 311, 318, 323, 383, 366, 369,
                 371, 384, 387, 389, 401, 411, 427, 443,
                 444, 445, 464, 465, 500, 512, 513, 514,
                 515, 517, 518, 520, 524, 525, 530, 531,
                 532, 533, 540, 543, 544, 546, 547, 548,
                 550, 554, 556, 560, 561, 563, 587, 591,
                 593, 604, 631, 636, 639, 646, 647, 648,
                 652, 654, 665, 666, 674, 691, 692, 695,
                 666, 674, 691, 692, 695, 698, 699, 700,
                 701, 702, 706, 711, 712, 720, 749, 750,
                 782, 829, 860, 873, 901, 902, 911, 981,
                 989, 990, 991, 992, 993, 995}
        If commonPortList.Contains(port) Then Return True
        Return False
    End Function

    Function GetSetActiveClient(Optional cli As Integer = -1)
        If cli = -1 Then
            Return activeClient
        Else
            activeClient = cli
            Return activeClient
        End If
    End Function
    'Function GetSetActiveRoom(Optional room As String = "")
    '    If room = "" Then
    '        Return activeRoom
    '    Else
    '        activeRoom = room
    '        Return activeRoom
    '    End If
    'End Function
    Sub SrvWaitThenDie()
        Dim peers = GetSrvConnectedPeers(srvDb)
        While peers.Count > 1
            GracefulClean(peers.Last)
            peers = GetSrvConnectedPeers(srvDb)
            Application.DoEvents()
        End While
        DeinitListener()
    End Sub

    Public Function LocalIPs() As String()
        Dim nic = GetObject("winmgmts:{impersonationLevel=impersonate}!\\.\root\cimv2")
        Dim colNicConfigs = nic.ExecQuery("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True")
        Dim IPs As String = ""
        Dim IPv4 As String = ""
        For Each nic In colNicConfigs
            For Each IPv4 In nic.IPAddress
                If IPs.IndexOf(IPv4) < 0 Then
                    If Not IPs = "" Then IPs += ","
                    IPs += IPv4
                End If
            Next
        Next
        Return IPs.Split(",")
    End Function

    Function InitListener(ip As IPAddress, port As Integer, name As String, motd As String)
        If IsNothing(tl) Then
            Try
                serverID = name
                srvMotd = motd
                tl = New TcpListener(ip, port)
                tl.Start()
                srvRSA(0) = New SimpleRSA
                If Not IO.File.Exists(IO.Path.Combine(Application.StartupPath, "server", "rsakeypair.xml")) Then
                    Try
                        AddConLine("Warning: RSA keypair not found. If this is your first time starting the server, this is normal. Generating new RSA keypair...")
                        IO.File.WriteAllText(IO.Path.Combine(Application.StartupPath, "server", "rsakeypair.xml"), srvRSA(0).ExportXML(True))
                        AddConLine("RSA keypair generated.")
                    Catch
                        AddConLine("Fatal error creating RSA keypair: could not write file.", " critical")
                        Return False
                    End Try
                Else
                    Try
                        Dim keySet = IO.File.ReadAllText(IO.Path.Combine(Application.StartupPath, "server", "rsakeypair.xml"))
                        srvRSA(0) = New SimpleRSA(keySet)
                    Catch
                        AddConLine("Fatal error reading RSA keypair: key pair may be corrupt. Delete the rsakeypair.xml file and restart the server.", " critical")
                        Return False
                    End Try
                End If
                AddConLine("Server initialized")
                AddConLine("Server name: " & name & crLf)
                AddConLine("Server MOTD: " & motd & crLf)
                garbageCollector.Interval = 40000
                AddHandler garbageCollector.Elapsed, AddressOf GCollectTimer
                garbageCollector.Start()
                CreateYbServerDB()
                Listen()
                Return True
            Catch ex As Exception
                ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
                Return False
            End Try
        Else
            Return False
            AddConLine("You cannot initialize the server twice. Please deinitialize the server first.", " critical")
        End If
        Return False
    End Function

    Sub DeinitListener()
        If Not IsNothing(tl) Then
            tl.Stop()
            tl = Nothing
            srvDb.Close()
            garbageCollector.Stop()
        End If
    End Sub

    Sub SendMessage(sockID, dataString)
        WriteClientData(sockID, "MESG•" & TrimWhite(Sanitize(dataString)))
    End Sub

    Function InitClient(sockID As Integer, ip As String, port As String, handle As String, encrypted As Integer)
        Try
            If Not IsNothing(clientSock(sockID)) Then
                If clientSock(sockID).Connected = True Then
                    AddConLine("Fatal error occurred when initializing the client: a client is already initialized on socket ID " & sockID & ". Please restart Yardbird and try again.")
                    DeinitClient(sockID)
                End If
            End If
            clientSock(sockID) = New TcpClient
            clientSock(sockID).SendTimeout = 1
            clientSock(sockID).ReceiveTimeout = 1
            Dim realIP As IPAddress = Dns.GetHostAddresses(ip)(0)
            Dim realPort As Integer = CInt(port)
            Dim realEnd As New IPEndPoint(realIP, realPort)
            Dim localAddresses = LocalIPs()
            firstData = True
            Try
                clientSock(sockID).Connect(realEnd)
            Catch ex As Exception
                If rfsdErrs.Contains(ex.HResult) = False Then
                    ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
                End If
                Return False
            End Try
            Try
                If IsNothing(GetSetCliIdentityAttribute(cliDb, sockID, "id")) Then
                    AddCliIdentitytoDB(cliDb, sockID, handle, ip & ":" & port, encrypted)
                End If
                frmMain.ChangeStatusText(sockID, "Connecting...")
                CreateNewChatWindow(sockID)
                If IsNothing(GetChatWindow(sockID)) Then
                    Throw New SystemException("The chat window could not be initialized.")
                End If
                If LoadTheme(cliSettings.GetKey("General", "theme", "default"), GetActiveChatWindow(), True, False) = False Then
                    Throw New SystemException("The theme could not be loaded.")
                End If
                If Not frmMain.tvwServers.Nodes.ContainsKey(sockID) Then
                    Dim node = frmMain.tvwServers.Nodes.Add(sockID, ip)
                    node.ContextMenu = CreateClientNodeContextMenu(sockID)
                End If
            Catch ex As Exception
                DeinitClient(sockID)
                MsgBox(ex.HResult & vbCrLf & ex.Message & vbCrLf & ex.StackTrace)
                ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
                Return False
            End Try
            'Dim evaluator = New Threading.Thread(Sub() 
            ReadClientData(sockID)
            'evaluator.Start()
            frmMain.ChangeStatusText(sockID, "Connected")
            GetSetCliIdentityAttribute(cliDb, sockID, "isOnline", "1")
            AddConLine("Connected to " & realIP.ToString & ":" & realPort & vbCrLf)
            ChatAppend(GetActiveChatWindow(), "alert", " passive", "Connected to " & realEnd.ToString)
            frmMain.tvwServers.SelectedNode = frmMain.tvwServers.Nodes(sockID)
            frmMain.ShowClientSavedState(sockID)
        Catch ex As Exception
            ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
            Return False
        End Try
        Return True
    End Function

    Sub DeinitClient(sockID As Integer)
        ChatAppend(GetChatWindow(sockID), "alert", " warning", "You have been disconnected from the server.")
        ContextMenuHandleClientAction("Disconnect", sockID)
        If Not IsNothing(clientSock(sockID)) Then
            clientSock(sockID).Close()
            clientSock(sockID) = Nothing
        End If
        PeerClear(sockID)
        If Not IsNothing(cliAES(sockID)) Then
            cliAES(sockID).Dispose()
            cliAES(sockID) = Nothing
        End If
        If Not IsNothing(cliAES(sockID)) Then
            cliRSA(sockID).Dispose()
            cliRSA(sockID) = Nothing
        End If
        frmMain.PopulatePeersInList(sockID)
        GetSetCliIdentityAttribute(cliDb, sockID, "identified", 0)
        GetSetCliIdentityAttribute(cliDb, sockID, "encryptionActive", 0)
        GetSetCliIdentityAttribute(cliDb, sockID, "encryptionStage", 0)
        GetSetCliIdentityAttribute(cliDb, sockID, "aesKey", "NULL")
        GetSetCliIdentityAttribute(cliDb, sockID, "isOnline", 0)
        GetSetCliIdentityAttribute(cliDb, sockID, "messageText", "")
        frmMain.ChangeStatusText(sockID, "Disconnected")
        frmMain.ShowClientSavedState(sockID)
    End Sub

    Async Sub ReadClientData(sockID As Integer)
        If Not IsNothing(clientSock(sockID)) Then
            Try
                If clientSock(sockID).Connected = True Then
                    Dim readBuffer(32767) As Byte
                    Await clientSock(sockID).GetStream.ReadAsync(readBuffer, 0, 32767)
                    Dim bnl = Split(TrimWhite(ToStr(readBuffer)), crLf)
                    For i = 0 To bnl.Count - 1
                        Dim outString = bnl(i)
                        If TrimWhite(outString) = String.Empty Then
                            Dim incrementor = CliIdentIncrementValue(cliDb, sockID, "emptyDataCount")
                            If incrementor > 10 Or incrementor = 0 Then
                                GetSetCliIdentityAttribute(cliDb, sockID, "emptyDataCount", 0)
                                DeinitClient(sockID)
                                Exit Sub
                            End If
                        Else
                            GetSetCliIdentityAttribute(cliDb, sockID, "emptyDataCount", 0)
                            ProcessClientData(sockID, outString)
                        End If
                    Next
                    ReadClientData(sockID)
                End If
            Catch ex As Exception
                If Not dsctErrs.Contains(ex.HResult) Then
                    ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
                End If
                DeinitClient(sockID)
            End Try
        End If
    End Sub
    Sub WriteClientData(sockID As Integer, data As String, Optional async As Boolean = True)
        If async = True Then
            Dim evaluator = New Threading.Thread(Sub() Wcd_(sockID, data))
            evaluator.Start()
        Else
            Wcd_(sockID, data)
        End If
    End Sub

    Sub Wcd_(sockID As Integer, data As String)
        If Not IsNothing(clientSock(sockID)) Then
            If clientSock(sockID).Connected = True Then
                Try
                    Dim outString
                    Dim prefix = ""
                    If GetSetCliIdentityAttribute(cliDb, sockID, "encryptionActive") = 1 Then
                        outString = cliAES(sockID).EncryptData(data) & crLf
                        prefix = encryptBadge
                    Else
                        outString = data & crLf
                    End If
                    Dim writeBuffer As Byte() = ToBytes(outString)
                    clientSock(sockID).GetStream.Write(writeBuffer, 0, writeBuffer.Length)
                    AddConLine(prefix & "[CLIENT " & sockID & "] SAID: " & TrimWhite(data) & crLf)
                Catch ex As Exception
                    If dsctErrs.Contains(ex.HResult) Then
                        DeinitClient(sockID)
                    Else
                        ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
                    End If
                End Try
            End If
        End If
    End Sub

    Sub AddConLine(data As String, Optional classes As String = " passive")
        ChatAppend(GetChatWindow(0), "alert", classes, data)
    End Sub
    Async Sub Listen()
        If Not IsNothing(tl) Then
            Try
                Dim newSocketID = NextAvailServerSlot()
                If newSocketID = 0 Then
                    Throw New SystemException("No available server slots.")
                    DeinitListener()
                    Exit Sub
                End If
                Dim maxPeers As Integer = 32768
                Dim maxOverflow As Integer = 1024
                If srvSettings.GetKey("Listener", "maxpeers", 32768) < 32768 Then
                    maxPeers = srvSettings.GetKey("Listener", "maxpeers", 32768)
                End If
                If srvSettings.GetKey("Listener", "maxoverflow", 1024) < 1024 Then
                    maxOverflow = srvSettings.GetKey("Listener", "maxpeers", 1024)
                End If
                If (newSocketID > maxPeers) Then
                    If (newSocketID < (maxPeers + maxOverflow)) Then
                        serverSock(newSocketID) = Await tl.AcceptSocketAsync
                        serverStream(newSocketID) = New NetworkStream(serverSock(newSocketID))
                        WriteServerData(newSocketID, "FULL•" & serverID)
                        GracefulClean(newSocketID)
                        Dim evaluator1 As New Threading.Thread(Sub() Listen())
                        evaluator1.Start()
                    Else
                        Dim evaluator1 As New Threading.Thread(Sub() Listen())
                        evaluator1.Start()
                    End If
                Else
                    If GetSrvPeerFromAttribute(srvDb, "id", newSocketID) <> 0 Then
                        Throw New SystemException("Slot was not successfully assigned in the database.")
                        DeinitListener()
                    Else
                        serverSock(newSocketID) = Await tl.AcceptSocketAsync
                        serverStream(newSocketID) = New NetworkStream(serverSock(newSocketID))
                        AddSrvPeertoDB(srvDb, newSocketID, serverSock(newSocketID).RemoteEndPoint.ToString)
                        WriteServerData(newSocketID, "REDY•" & serverID)
                        Dim evaluator As New Threading.Thread(Sub() ReadServerData(newSocketID))
                        evaluator.Start()
                        Dim evaluator1 As New Threading.Thread(Sub() Listen())
                        evaluator1.Start()
                    End If
                End If
            Catch ex As Exception
                If dsctErrs.Contains(ex.HResult) Then
                    AddConLine("Server stopped.", " critical")
                Else
                    AddConLine("A fatal error occurred when attempting to assign a new socket to the listener: " & ex.Message, " critical")
                End If
            End Try
        End If
    End Sub
    Function ListenerActive()
        If Not IsNothing(tl) Then
            Return True
        Else
            Return False
        End If
    End Function

    Function ClientActive(sockID As Integer)
        If Not IsNothing(clientSock(sockID)) Then
            If clientSock(sockID).Connected = True Then
                Return True
            Else
                Return False
            End If
        Else
            Return False
        End If
    End Function

    Sub SendPeers(sockID)
        WriteServerData(sockID, "PCLR•")
        Dim peerList = SqlQuery(srvDb, "SELECT id, handle FROM ybPeers;")
        Dim buildString = "PLST•{"
        If Not (IsNothing(peerList)) Then
            For Each Item In peerList
                If Item("id") = peerList.Last.Item("id") Then
                    buildString = buildString & Item("id") & "|" & Item("handle") & "}"
                Else
                    buildString = buildString & Item("id") & "|" & Item("handle") & ","
                End If
            Next
        End If
        WriteServerData(sockID, buildString)
    End Sub

    Sub SendHandle(sockID, peerID)
        Try
            WriteServerData(sockID, "PEER•" & peerID & "•" & GetSrvPeerAttribute(srvDb, peerID, "handle"))
        Catch ex As Exception
            WriteServerData(sockID, "ERRS•111•Invalid peer.")
        End Try
    End Sub

    Sub ReadServerData(sockID As Integer)
        If Not IsNothing(serverSock(sockID)) Then
            Dim readBuffer(32767) As Byte
            Try
                If Not IsNothing(serverStream(sockID)) Then
                    serverStream(sockID).ReadTimeout = 80000
                    serverStream(sockID).Read(readBuffer, 0, 32767)
                    Dim outstring = TrimWhite(ToStr(readBuffer))
                    If outstring = String.Empty Then
                        If SrvIncrementValue(srvDb, sockID, "emptyDataCount") > 10 Then
                            GracefulClean(sockID)
                            Exit Sub
                        End If
                    Else
                        SetSrvPeerAttribute(srvDb, sockID, "emptyDataCount", 0)
                        ProcessServerData(sockID, TrimWhite(outstring))
                        ReadServerData(sockID)
                    End If
                End If
            Catch ex As Exception
                If dsctErrs.Contains(ex.HResult) Then
                    GracefulClean(sockID)
                Else
                    ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
                    ReadServerData(sockID)
                End If
            End Try
        End If
    End Sub

    Sub WriteServerData(sockID As Integer, data As String, Optional threaded As Boolean = True)
        If threaded = True Then
            Dim evaluator = New Threading.Thread(Sub() Wsd_(sockID, data))
            evaluator.Start()
        Else
            Wsd_(sockID, data)
        End If
    End Sub

    Sub Wsd_(sockID As Integer, data As String)
        If Not IsNothing(serverSock(sockID)) Then
            Dim outString
            Dim prefix = ""
            If GetSrvPeerAttribute(srvDb, sockID, "encrypted") = 1 Then
                outString = srvAES(sockID).EncryptData(data) & crLf
                prefix = encryptBadge
            Else
                outString = data & crLf
            End If
            Dim writeBuffer As Byte() = ToBytes(outString)
            Try
                serverStream(sockID).Write(writeBuffer, 0, writeBuffer.Length)
            Catch ex As Exception
                If dsctErrs.Contains(ex.HResult) Then
                    GracefulClean(sockID)
                Else
                    ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
                End If
            End Try
            AddConLine(prefix & "[SERVER " & sockID & "] SAID: " & data & crLf)
        End If
    End Sub

    Function Broadcast(dataString As String, Optional except As List(Of Integer) = Nothing)
        Try
            For Each Item In GetSrvConnectedPeers(srvDb)
                If Not IsNothing(serverSock(Item)) Then
                    If serverSock(Item).Connected = True And GetSrvPeerAttribute(srvDb, Item, "identified") = 1 Then
                        If IsNothing(except) Then
                            WriteServerData(Item, dataString)
                        Else
                            If Not (except.Contains(Item)) Then
                                WriteServerData(Item, dataString)
                            End If
                        End If
                    End If
                End If
            Next
            Return True
        Catch ex As Exception
            ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
            Return False
        End Try
    End Function


    Function GracefulClean(sockID As Integer)
        Try
            DeleteSrvPeerFromDB(srvDb, sockID)
            If Not IsNothing(serverStream(sockID)) Then
                Broadcast("RCVD•DSCT•" & sockID)
                WriteServerData(sockID, "GDBY")
                serverStream(sockID).Close()
                serverStream(sockID).Dispose()
                serverStream(sockID) = Nothing
            End If
            If Not IsNothing(serverSock(sockID)) Then
                serverSock(sockID).Close()
                serverSock(sockID).Dispose()
                serverSock(sockID) = Nothing
            End If
            If Not IsNothing(srvAES(sockID)) Then
                srvAES(sockID).Dispose()
                srvAES(sockID) = Nothing
            End If
            If Not IsNothing(srvRSA(sockID)) Then
                srvRSA(sockID).Dispose()
                srvRSA(sockID) = Nothing
            End If
            Return True
        Catch ex As Exception
            ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
            Return False
        End Try
        Return False
    End Function

    Function NextAvailServerSlot()
        Try
            Dim slots As List(Of Integer) = GetSrvConnectedPeers(srvDb)
            If slots.Count = 0 Then

                Return 1
            Else
                Dim i = 1
                slots.Add(slots.Last + 2)
                For Each Item In slots
                    If Item <> i Then
                        Return i
                    End If
                    i = i + 1
                Next
            End If
        Catch ex As Exception
            Return 0
        End Try
        Return 0
    End Function

    Function NextAvailClientSlot()
        Try
            Dim slots As List(Of Integer) = GetCliIdentities(cliDb)
            If slots.Count = 0 Then
                Return 1
            Else
                Dim i = 1
                slots.Add(slots.Last + 2)
                For Each Item In slots
                    If Item <> i Then
                        Return i
                    End If
                    i = i + 1
                Next
            End If
        Catch ex As Exception
            Return 0
        End Try
        Return 0
    End Function

    Function IsClientConnected(sockID)
        Try
            If Not IsNothing(clientSock(sockID)) Then
                Return clientSock(sockID).Connected
            Else
                Return False
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function

    Function ToBytes(str As String)
        Return Text.Encoding.UTF8.GetBytes(str)
    End Function


    Function ToStr(bytes As Byte())
        Return Text.Encoding.UTF8.GetString(bytes)
    End Function

    Sub GarbageCollect()
        For Each Item In GetSrvConnectedPeers(srvDb)
            Try
                If Not IsNothing(serverSock(Item)) Then
                    If serverSock(Item).Connected = False Then
                        GracefulClean(Item)
                    Else
                        If GetSrvPeerAttribute(srvDb, Item, "identified") = 0 And ((BespokeTimestamp(Now) - GetSrvPeerAttribute(srvDb, Item, "joinedAt")) > 40000) Then
                            WriteServerData(Item, "ERRS•102•Your client has been inactive for too long and will be disconnected.", False)
                            GracefulClean(Item)
                        Else
                            Dim maxAfkPeriod = srvSettings.GetKey("FISC", "MaxAFKPeriod", 0) * 1000 * 60
                            Dim afkPeriod = (BespokeTimestamp(Now) - GetSrvPeerAttribute(srvDb, Item, "lastMessageSent"))
                            Debug.Print(afkPeriod)
                            Debug.Print(maxAfkPeriod)
                            If Not maxAfkPeriod = 0 And afkPeriod > maxAfkPeriod Then
                                WriteServerData(Item, "ERRS•203•Your client has been inactive for too long and will be disconnected.", False)
                                GracefulClean(Item)
                            Else
                                Try
                                    serverSock(Item).SendTimeout = 1
                                    Heartbeat(Item)
                                Catch ex As Exception
                                    ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
                                    If ex.HResult = &H80004005 Then
                                        GracefulClean(Item)
                                    End If
                                End Try
                            End If
                        End If

                    End If
                End If
            Catch ex As Exception
                ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
            End Try
        Next
    End Sub

    Sub Heartbeat(sockID)
        WriteServerData(sockID, "PING•")
    End Sub

    Sub KickServerPeers(Optional alert As String = "")
        Dim enumeratedPeers As New List(Of Integer)
        For Each Item As Integer In GetSrvConnectedPeers(srvDb)
            If Not (alert = "") Then WriteServerData(Item, "RCVD•ALERT•" & alert)
            WriteServerData(Item, "ACKD•QUIT•" & Item)
            enumeratedPeers.Add(Item)
        Next
        For Each Item As Integer In enumeratedPeers
            GracefulClean(Item)
        Next
    End Sub

    Function ClientConnected(sockID)
        If IsNothing(clientSock(sockID)) Then
            Return False
        Else
            Return clientSock(sockID).Connected
        End If
    End Function

    Sub GCollectTimer(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs)
        GarbageCollect()
        Debug.Print("Automatic garbage collection started")
    End Sub


    Sub ProcessServerData(sockID As Integer, dataString As String)
        Try
            Dim cleanDataString = ""
            Dim prefix = ""
            Dim stage As Integer = GetSrvPeerAttribute(srvDb, sockID, "encryptionStage")
            Select Case stage
                Case 0, 1
                    cleanDataString = TrimWhite(dataString)
                Case 2
                    cleanDataString = srvRSA(0).decryptdata(TrimWhite(dataString))
                    If IsNothing(cleanDataString) Then
                        WriteServerData(sockID, "ERRS•403•Error during encryption handshake. You will be disconnected.", False)
                        GracefulClean(sockID)
                    End If
                Case 3, 4
                    If GetSrvPeerAttribute(srvDb, sockID, "encrypted") = 1 Then
                        cleanDataString = srvAES(sockID).DecryptData(TrimWhite(dataString))
                        If IsNothing(cleanDataString) Then
                            WriteServerData(sockID, "ERRS•403•Error decrypting client data. You will be disconnected.", False)
                            GracefulClean(sockID)
                        End If
                        prefix = encryptBadge
                    Else
                        cleanDataString = TrimWhite(dataString)
                    End If
            End Select
            If cleanDataString = "" Then Exit Sub
            AddConLine(prefix & "[SERVER " & sockID & "] HEARD: " & cleanDataString & crLf)
            cleanDataString.Replace(crLf, "")
            Dim parsedData = Split(cleanDataString, "•")
            If Right(Left(cleanDataString, 5), 1) <> "•" Then
                WriteServerData(sockID, "ERRS•100•Invalid input")
            Else
                Select Case Left(cleanDataString, 4)
                    Case "HELO"
                        If GetSrvPeerAttribute(srvDb, sockID, "identified") = 1 Then
                            WriteServerData(sockID, "ERRS•105•You already have a handle.")
                        Else
                            Dim maxBadIdents = srvSettings.GetKey("FISC", "maxbadidents", 0)
                            If (Not maxBadIdents = 0) And (CInt(GetSrvPeerAttribute(srvDb, sockID, "badIdentCount")) = maxBadIdents) Then
                                WriteServerData(sockID, "ERRS•109•You have exceeded the maximum number of identification attempts. You will now be disconnected.")
                                SetSrvPeerAttribute(srvDb, sockID, "badIdentCount", 0)
                                GracefulClean(sockID)
                                Exit Sub
                            End If
                            Dim isTaken = False
                            For Each Item As String In GetSrvPeerHandles(srvDb)
                                If Item.ToLower = parsedData(1).ToLower Then
                                    LogImpersonationAttempt(sockID, GetSrvPeerAttribute(srvDb, sockID, "endpoint"), parsedData(1), GetSrvPeerFromAttribute(srvDb, "handle", Item))
                                    SetSrvPeerAttribute(srvDb, sockID, "badIdentCount", CInt(GetSrvPeerAttribute(srvDb, sockID, "badIdentCount")) + 1)
                                    WriteServerData(sockID, "ERRS•107•A user already has this handle." & parsedData(1))
                                    isTaken = True
                                    Exit For
                                End If
                            Next
                            If isTaken = False Then
                                If Regex.Match(parsedData(1), "\A[A-Z|a-z|0-9|\-|_]+\z").Success = False Or parsedData(1).Length > 20 Then
                                    WriteServerData(sockID, "ERRS•108•The specified handle is invalid.•" & parsedData(1))
                                    SetSrvPeerAttribute(srvDb, sockID, "badIdentCount", CInt(GetSrvPeerAttribute(srvDb, sockID, "badIdentCount")) + 1)
                                Else
                                    WriteServerData(sockID, "ACKD•HANDLE•" & sockID & "•" & TrimWhite(parsedData(1)) & "•" & serverSock(sockID).RemoteEndPoint.ToString)
                                    Broadcast("RCVD•HANDLE•" & sockID & "•" & TrimWhite(parsedData(1)) & "•" & serverSock(sockID).RemoteEndPoint.ToString, New List(Of Integer) From {sockID})
                                    SetSrvPeerAttribute(srvDb, sockID, "handle", TrimWhite(parsedData(1)))
                                    SetSrvPeerAttribute(srvDb, sockID, "identified", "1")
                                End If
                            End If
                        End If
                    Case "MESG"
                        If GetSrvPeerAttribute(srvDb, sockID, "identified") = 1 Then
                            If BespokeTimestamp(Now) - CLng(GetSrvPeerAttribute(srvDb, sockID, "lastMessageSent")) < srvSettings.GetKey("FISC", "minmessagerate", 0) Then
                                WriteServerData(sockID, "ERRS•202•You must wait to do that.")
                                Dim maxViol = srvSettings.GetKey("FISC", "maxmessagerateviolations", 0)
                                If (Not maxViol = 0) And (SrvIncrementValue(srvDb, sockID, "badMessageCount") > maxViol) Then
                                    WriteServerData(sockID, "ERRS•200•You are doing that too much. You will now be disconnected.")
                                    GracefulClean(sockID)
                                    Exit Sub
                                End If
                            Else
                                WriteServerData(sockID, "ACKD•MESG•" & parsedData(1))
                                Broadcast("RCVD•MESG•" & sockID & "•" & parsedData(1), New List(Of Integer) From {sockID})
                            End If
                            SetSrvPeerAttribute(srvDb, sockID, "lastMessageSent", BespokeTimestamp(Now))
                        Else
                            WriteServerData(sockID, "ERRS•106•You need to identify yourself to send messages.")
                        End If
                    Case "CLID"
                        WriteServerData(sockID, "ACKD•CLID•" & sockID)
                    Case "EBGN"
                        If Not IsNothing(stage) Then
                            If stage = 0 Then
                                SetSrvPeerAttribute(srvDb, sockID, "encrypted", 0)
                                srvRSA(sockID) = New SimpleRSA
                                WriteServerData(sockID, "ACKD•EBGN•" & srvRSA(0).ExportPublicKey, False)
                                SetSrvPeerAttribute(srvDb, sockID, "encryptionStage", 1)
                            Else
                                WriteServerData(sockID, "ERRS•401•Incorrect stage in encryption process. Expected: 0 or 2, got: " & stage, False)
                                GracefulClean(sockID)
                            End If
                        Else
                            GracefulClean(sockID)
                            AddConLine("Fatal database error while initializing encryption provider.", " critical")
                        End If
                    Case "EPBK"
                        If Not IsNothing(stage) Then
                            If stage = 1 Then
                                srvRSA(sockID) = New SimpleRSA(BerToXML(TrimWhite(parsedData(1))))
                                WriteServerData(sockID, "ACKD•EPBK", False)
                                SetSrvPeerAttribute(srvDb, sockID, "encryptionStage", 2)
                            Else
                                WriteServerData(sockID, "ERRS•401•Incorrect stage in encryption process. Expected: 1, got: " & stage, False)
                                GracefulClean(sockID)
                            End If
                        Else
                            GracefulClean(sockID)
                            AddConLine("Fatal database error while initializing server encryption provider.", " critical")
                        End If
                    Case "EVRF"
                        If Not IsNothing(stage) Then
                            If stage = 2 Then
                                WriteServerData(sockID, srvRSA(sockID).EncryptData("ACKD•EVRF•" & parsedData(1) & "•" & GetNewRandom16Bit()))
                            Else
                                WriteServerData(sockID, "ERRS•401•Incorrect stage in encryption process. Expected: 1, got: " & stage, False)
                                GracefulClean(sockID)
                            End If
                        Else
                            GracefulClean(sockID)
                            AddConLine("Fatal database error while initializing server encryption provider.", " critical")
                        End If
                    Case "EAES"
                        If Not IsNothing(stage) Then
                            If stage = 2 Then
                                Dim ekey = GetNewAESEncryptionKey()
                                Dim output = srvRSA(sockID).EncryptData("ACKD•EAES•" & ekey & "•" & GetNewRandom16Bit())
                                srvAES(sockID) = New SimpleAes(ekey)
                                WriteServerData(sockID, output, False)
                                SetSrvPeerAttribute(srvDb, sockID, "aesKey", ekey)
                                SetSrvPeerAttribute(srvDb, sockID, "encryptionStage", 3)
                                SetSrvPeerAttribute(srvDb, sockID, "encrypted", 1)
                            Else
                                WriteServerData(sockID, "ERRS•401•Incorrect stage in encryption process. Expected: 2, got: " & stage, False)
                                GracefulClean(sockID)
                            End If
                        Else
                            GracefulClean(sockID)
                            AddConLine("Fatal database error while initializing server encryption provider.", " critical")
                        End If
                    Case "EEND"
                        If Not IsNothing(stage) Then
                            If stage = 3 Then
                                WriteServerData(sockID, "ACKD•EEND•", False)
                                SetSrvPeerAttribute(srvDb, sockID, "encryptionStage", 4)
                                srvRSA(sockID).Dispose()
                                srvRSA(sockID) = Nothing
                            Else
                                WriteServerData(sockID, "ERRS•401•Incorrect stage in encryption process. Expected: 3, got: " & stage, False)
                                GracefulClean(sockID)
                            End If
                        Else
                            GracefulClean(sockID)
                            AddConLine("Fatal database error while initializing server encryption provider.", " critical")
                        End If
                    Case "PLST"
                        SendPeers(sockID)
                    Case "PING"
                        WriteServerData(sockID, "ACKD•PING•PONG")
                    Case "QUIT"
                        Dim reasonString = ""
                        If parsedData.Count > 1 Then
                            reasonString = "•" & parsedData(1)
                        End If
                        WriteServerData(sockID, "ACKD•QUIT•" & sockID)
                        Broadcast("RCVD•QUIT•" & sockID & reasonString, New List(Of Integer) From {sockID})
                    Case "GDBY"
                        GracefulClean(sockID)
                    Case "MOTD"
                        Dim tmpMotd = srvMotd
                        tmpMotd = Replace(tmpMotd, "%n", serverID)
                        tmpMotd = Replace(tmpMotd, "%a", GetSrvPeerCount(srvDb))
                        tmpMotd = Replace(tmpMotd, "%t", Now.ToLongTimeString)
                        tmpMotd = Replace(tmpMotd, "%d", Now.ToLongDateString)
                        WriteServerData(sockID, "RCVD•MESG•0•" & tmpMotd)
                    Case "ACKD"
                        Select Case parsedData(1)
                            Case "PING"
                                If parsedData(2) = "PONG" Then

                                End If
                            Case Else
                                WriteServerData(sockID, "ERRS•110•Invalid acknowledgement.")
                        End Select
                    Case Else
                        WriteServerData(sockID, "ERRS•101•Not yet implemented.•" & Left(cleanDataString, 4))
                End Select
            End If
        Catch ex As Exception
            ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
        End Try
    End Sub

    Sub PeerRemove(serverID, peerID)
        DeleteCliPeerFromDB(cliDb, serverID, peerID)
    End Sub

    Sub PeerClear(serverID)
        SqlNonQuery(cliDb, "DELETE FROM ybPeers WHERE serverID = '" & serverID & "';")
        SqlNonQuery(cliDb, "INSERT INTO ybPeers (serverID, id, handle) VALUES ('" & serverID & "', '0', 'SERVER');")
    End Sub

    Sub PeerAdd(serverID As Integer, peerID As Integer, handle As String)
        Try
            If Not (peerID = 0) Then
                If IsNothing(GetSetCliPeerAttribute(cliDb, serverID, peerID, "id")) Then
                    AddCliPeertoDB(cliDb, serverID, peerID, handle)
                End If
            End If
        Catch ex As Exception
            ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
        End Try
    End Sub

    Sub ProcessClientData(sockID As Integer, dataString As String)
        Try
            Dim stage As Integer = GetSetCliIdentityAttribute(cliDb, sockID, "encryptionStage")
            Dim cleanDataString = ""
            Dim prefix = ""
            Select Case stage
                Case 0, 1
                    cleanDataString = TrimWhite(dataString)
                Case 2
                    cleanDataString = cliRSA(sockID).decryptdata(TrimWhite(dataString))
                    If IsNothing(cleanDataString) Then
                        ChatAppend(GetActiveChatWindow(), "alert", " critical", "Fatal error occurred during the handshake process: the server sent invalid data during the handshake.")
                        DeinitClient(sockID)
                        Exit Sub
                    End If
                Case 3, 4
                    If GetSetCliIdentityAttribute(cliDb, sockID, "encryptionActive") = 1 Then
                        cleanDataString = cliAES(sockID).DecryptData(dataString)
                        If IsNothing(cleanDataString) Then
                            ChatAppend(GetActiveChatWindow(), "alert", " critical", "Fatal error occurred during secure encryption: the server sent invalid data.")
                            DeinitClient(sockID)
                            Exit Sub
                        End If
                        prefix = encryptBadge
                    Else
                        cleanDataString = TrimWhite(dataString)
                    End If
            End Select
            If cleanDataString = "" Then Exit Sub
            AddConLine(prefix & "[CLIENT " & sockID & " HEARD]: " & cleanDataString & crLf)
            Dim parsedData = Split(cleanDataString, "•")
            If Right(Left(cleanDataString, 5), 1) <> "•" Then
                WriteClientData(sockID, "ERRS•100•Invalid input")
                errCounter = errCounter + 1
                lastError = Now()
                errSpan = errSpan + (Now() - lastError).Seconds
                If errCounter > 20 And errSpan > 5 Then
                    clientSock(sockID).Close()
                    MsgBox("Yardbird has encountered a memory leak condition. Please report to the developers.")
                End If
                If firstData = True Then
                    If Not Left(cleanDataString, 4) = "REDY" Then
                        DeinitClient(sockID)
                        ChatAppend(GetChatWindow(sockID), "alert", " warning", "The server did not return a valid response. Are you sure this is a Yardbird server?")
                        firstData = False
                    End If
                End If
            Else
                firstData = False
                Select Case Left(cleanDataString, 4)
                    Case "REDY"
                        ChatAppend(GetChatWindow(sockID), "alert", " passive", "Welcome to server '" & parsedData(1) & "'")
                        WriteClientData(sockID, "CLID•")
                    Case "FULL"
                        ChatAppend(GetChatWindow(sockID), "alert", " warning", "Server " & parsedData(1) & " is full. Please try again later.")
                        WriteClientData(sockID, "GDBY•")
                    Case "PING"
                        WriteClientData(sockID, "ACKD•PING•PONG")
                    Case "PLST"
                        Dim matches = System.Text.RegularExpressions.Regex.Match(parsedData(1), "{(.*?)}")
                        If matches.Groups.Count > 1 Then
                            Dim peers = Split(matches.Groups(1).Captures(0).Value, ",")
                            For Each Item In peers
                                Dim delim = Split(Item, "|")
                                Dim tmpID = delim(0)
                                Dim tmpHandle = delim(1)
                                PeerAdd(sockID, tmpID, tmpHandle)
                            Next
                            frmMain.PopulatePeersInList(sockID)
                        Else
                            ChatAppend(GetActiveChatWindow, "alert", " warning", "Error retrieving peers. You will now be disconnected.")
                            DeinitClient(sockID)
                        End If
                    Case "PCLR"
                        PeerClear(sockID)
                    Case "ACKD"
                        Select Case parsedData(1)
                            Case "CLID"
                                GetSetCliIdentityAttribute(cliDb, sockID, "id", parsedData(2))
                                If GetSetCliIdentityAttribute(cliDb, sockID, "encrypted") = 1 Then
                                    WriteClientData(sockID, "EBGN•")
                                Else
                                    WriteClientData(sockID, "HELO•" & GetSetCliIdentityAttribute(cliDb, sockID, "handle"))
                                End If
                            Case "HANDLE"
                                GetSetCliIdentityAttribute(cliDb, sockID, "handle", parsedData(3))
                                GetSetCliIdentityAttribute(cliDb, sockID, "identified", 1)
                                frmMain.tvwServers.Nodes.Item(sockID.ToString).Text = parsedData(3) & "@" & GetSetCliIdentityAttribute(cliDb, sockID, "remoteEndpoint")
                                ChatAppend(GetChatWindow(sockID), "alert", " information", "" & GetSetCliIdentityAttribute(cliDb, sockID, "handle") & " (peer ID " & GetSetCliIdentityAttribute(cliDb, sockID, "id") & ") has joined the room.")
                                WriteClientData(sockID, "PLST•")
                                WriteClientData(sockID, "MOTD•")
                            Case "MESG"
                                ChatAppend(GetChatWindow(sockID), "message", " sent", parsedData(2), GetSetCliIdentityAttribute(cliDb, sockID, "handle"))
                            Case "QUIT"
                                If GetSetCliIdentityAttribute(cliDb, sockID, "id") = CInt(parsedData(2)) Then
                                    WriteClientData(sockID, "GDBY•", False)
                                    DeinitClient(sockID)
                                End If
                            Case "EBGN"
                                If Not IsNothing(stage) Then
                                    If stage = 0 Or stage = 5 Then
                                        GetSetCliIdentityAttribute(cliDb, sockID, "encryptionActive", 0)
                                        GetSetCliIdentityAttribute(cliDb, sockID, "encryptionStage", 0)
                                        cliRSA(0) = New SimpleRSA(BerToXML(parsedData(2)))
                                        If Not IO.File.Exists(IO.Path.Combine(Application.StartupPath, "client", "rsakeypair.xml")) Then
                                            Try
                                                cliRSA(sockID) = New SimpleRSA
                                                AddConLine("Warning: Client RSA keypair not found. If this is your first time starting the client, this is normal. Generating new RSA keypair...")
                                                IO.File.WriteAllText(IO.Path.Combine(Application.StartupPath, "client", "rsakeypair.xml"), cliRSA(sockID).ExportXML(True))
                                                AddConLine("Client RSA keypair generated.")
                                            Catch
                                                AddConLine("Fatal error creating client RSA keypair: could not write file.", " critical")
                                            End Try
                                        Else
                                            Try
                                                Dim keySet = IO.File.ReadAllText(IO.Path.Combine(Application.StartupPath, "client", "rsakeypair.xml"))
                                                cliRSA(sockID) = New SimpleRSA(keySet)
                                            Catch
                                                AddConLine("Fatal error reading client RSA keypair: key pair may be corrupt. Delete the rsakeypair.xml file and retry the connection.", " critical")
                                            End Try
                                        End If
                                        Dim ber64 = cliRSA(sockID).ExportPublicKey()
                                        WriteClientData(sockID, "EPBK•" & ber64, False)
                                        GetSetCliIdentityAttribute(cliDb, sockID, "encryptionStage", 1)
                                    Else
                                        AddConLine("Incorrect stage in encryption handshake. Expected stage 1, got " & stage, " critical")
                                        DeinitClient(sockID)
                                    End If
                                Else
                                    DeinitClient(sockID)
                                    AddConLine("Fatal database error while initializing encryption provider.", " critical")
                                End If
                            Case "EPBK"
                                cliVerify(sockID) = GetNewRandom128Bit()
                                WriteClientData(sockID, cliRSA(0).EncryptData("EVRF•" & cliVerify(sockID) & "•" & GetNewRandom16Bit()), False)
                                GetSetCliIdentityAttribute(cliDb, sockID, "encryptionStage", 2)
                            Case "EVRF"
                                If parsedData(2) = cliVerify(sockID) Then
                                    WriteClientData(sockID, cliRSA(0).EncryptData("EAES•" & GetNewRandom16Bit()), False)
                                Else
                                    ChatAppend(GetActiveChatWindow(), "alert", " critical", "Fatal error: server was unable to verify that it held the keypair it specified during the handshake.")
                                End If
                            Case "EAES"
                                If Not IsNothing(stage) Then
                                    If stage = 2 Then
                                        GetSetCliIdentityAttribute(cliDb, sockID, "aesKey", TrimWhite(parsedData(2)))
                                        GetSetCliIdentityAttribute(cliDb, sockID, "encryptionActive", 1)
                                        cliAES(sockID) = New SimpleAes(TrimWhite(parsedData(2)))
                                        cliRSA(sockID).Dispose()
                                        cliRSA(sockID) = Nothing
                                        WriteClientData(sockID, "EEND•", False)
                                        GetSetCliIdentityAttribute(cliDb, sockID, "encryptionStage", 3)
                                    Else
                                        AddConLine("Incorrect stage in encryption handshake. Expected stage 1, got " & stage, " critical")
                                        DeinitClient(sockID)
                                    End If
                                Else
                                    DeinitClient(sockID)
                                    AddConLine("Fatal database error while initializing encryption provider.", " critical")
                                End If
                            Case "EEND"
                                If Not IsNothing(stage) Then
                                    If stage = 3 Then
                                        If GetSetCliIdentityAttribute(cliDb, sockID, "identified") = 0 Then
                                            WriteClientData(sockID, "HELO•" & GetSetCliIdentityAttribute(cliDb, sockID, "handle"), False)
                                        End If
                                        GetSetCliIdentityAttribute(cliDb, sockID, "encryptionStage", 4)
                                        ChatAppend(GetChatWindow(sockID), "alert", " information", "A new encryption key has been negotiated. Your messages are now secure.")
                                    Else
                                        AddConLine("Incorrect stage in encryption handshake. Expected stage 3, got " & stage, " critical")
                                        DeinitClient(sockID)
                                    End If
                                Else
                                    DeinitClient(sockID)
                                    AddConLine("Fatal database error while initializing encryption provider.", " critical")
                                End If
                            Case Else
                                WriteClientData(sockID, "ERRS•110•Invalid acknowledgement.")
                        End Select
                    Case "RCVD"
                        If GetSetCliIdentityAttribute(cliDb, sockID, "sounds") = 1 Then
                            Dim evaluator As New Threading.Thread(Sub() My.Computer.Audio.Play(IO.Path.Combine(Application.StartupPath, "assets", "sounds", "pop.wav")))
                            evaluator.Start()
                        End If
                        Select Case parsedData(1)
                            Case "ALERT"
                                ChatAppend(GetChatWindow(sockID), "alert", " passive", parsedData(2))
                            Case "MESG"
                                Dim tempHndl = GetSetCliPeerAttribute(cliDb, sockID, CInt(parsedData(2)), "handle")
                                ChatAppend(GetChatWindow(sockID), "message", "", parsedData(3), tempHndl)
                            Case "HANDLE"
                                Dim tmpID = parsedData(2)
                                Dim tmpHandle = parsedData(3)
                                PeerAdd(sockID, tmpID, tmpHandle)
                                ChatAppend(GetChatWindow(sockID), "alert", " information", "" & tmpHandle & " (peer ID " & parsedData(2) & ") has joined the room.")
                            Case "QUIT"
                                If GetCliConnectedPeerIDs(cliDb, sockID).Contains(CInt(parsedData(2))) Then
                                    Dim reasonString = ""
                                    If parsedData.Count > 3 Then
                                        reasonString = " Reason: " & parsedData(3)
                                    End If
                                    Dim tmpHandle = GetSetCliPeerAttribute(cliDb, sockID, CInt(parsedData(2)), "handle")
                                    ChatAppend(GetChatWindow(sockID), "alert", " warning", "" & tmpHandle & " (peer ID " & parsedData(2) & ") is leaving." & reasonString)
                                End If
                            Case "DSCT"
                                If GetCliConnectedPeerIDs(cliDb, sockID).Contains(CInt(parsedData(2))) Then
                                    Dim tmpHandle = GetSetCliPeerAttribute(cliDb, sockID, CInt(parsedData(2)), "handle")
                                    ChatAppend(GetChatWindow(sockID), "alert", " warning", "" & tmpHandle & " (peer ID " & parsedData(2) & ") has been disconnected.")
                                    WriteClientData(sockID, "PLST•")
                                End If
                            Case Else
                        End Select
                    Case "ERRS"
                        If parsedData.Count > 3 Then
                            ChatAppend(GetChatWindow(sockID), "alert", " warning", "" & CliErrHndl(sockID, parsedData(1), parsedData(2), parsedData(3)))
                        Else
                            ChatAppend(GetChatWindow(sockID), "alert", " warning", "" & CliErrHndl(sockID, parsedData(1), parsedData(2)))
                        End If
                    Case Else
                        WriteClientData(sockID, "ERRS•101•Not yet implemented.")
                End Select
            End If
        Catch ex As Exception
            ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
        End Try
    End Sub
End Module
