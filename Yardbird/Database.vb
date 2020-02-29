Imports System.Data.SQLite

Module Database
    Public srvDb As SQLiteConnection
    Public srvUsrDb As SQLiteConnection
    Public cliDb As SQLiteConnection
    Function CreateDbInMem(filename As String) As SQLiteConnection
        Return New SQLiteConnection("FullUri=file:" & filename & "?mode=memory;Version=3;New=True;", False)
    End Function

    Function BespokeTimestamp(time As DateTime) As Long
        Return (DateTime.UtcNow - New DateTime(2005, 1, 1, 0, 0, 0)).TotalMilliseconds
    End Function

    Function CreateDbInFile(filename As String) As SQLiteConnection
        Return New SQLiteConnection("Data Source=" & IO.Path.Combine(Application.StartupPath, "server", filename & ".db") & ";Version=3")
    End Function

    Sub CreateYbServerDB()
        Try
            srvDb = CreateDbInMem("memsrv")
            srvUsrDb = CreateDbInFile("users")
            srvDb.Open()
            srvUsrDb.Open()
            SqlNonQuery(srvDb, "CREATE TABLE ybPeers (id integer primary key,
                      handle varchar(20),
                      endpoint varchar(51),
                      identified tinyint(1) NOT NULL DEFAULT '0',
                      badIdentCount Integer,
                      dataCount Integer,
                      badMessageCount integer,
                      emptyDataCount integer,
                      lastMessageSent integer,
                      encrypted integer(1) default '0', 
                      aesKey varchar(2048),
                      encryptionStage integer(1) default '0',
                      joinedAt integer);")
            SqlNonQuery(srvDb, "INSERT INTO ybPeers VALUES ('0',
                      'SERVER',
                      NULL,
                      '1',
                       '0',
                       '0',
                       '0',
                       '0',
                       '" & BespokeTimestamp(Now) & "', '0', NULL, '0', '" & BespokeTimestamp(Now) & "');")
            SqlNonQuery(srvUsrDb, "CREATE TABLE IF NOT EXISTS ybUsers (id integer primary key, handle varchar(20) UNIQUE, pwdHash varchar(256))")
        Catch ex As Exception
            MsgBox("Fatal error creating server database. Error code: " & ex.HResult & ": " & ex.Message & vbCrLf & vbCrLf & ex.StackTrace & vbCrLf & vbCrLf & "Please report to the developers.", MsgBoxStyle.Critical, "Fatal Error")
            End
        End Try
    End Sub
    Sub CreateYbClientDB()
        Try
            cliDb = CreateDbInMem("memcli")
            cliDb.Open()
            SqlNonQuery(cliDb, "CREATE TABLE ybPeers (row_id INTEGER PRIMARY KEY AUTOINCREMENT,
                      serverID integer(5),
                      id integer,
                      handle varchar(20));")
            SqlNonQuery(cliDb, "CREATE TABLE ybIdentities (serverID integer(5) primary key,
                      id integer,
                      handle varchar(20),
                      remoteendpoint varchar(51),
                      identified integer(1) DEFAULT '0',
                      sounds INTEGER(1) DEFAULT '1',
                      emptyDataCount INTEGER,
                      encrypted INTEGER(1) DEFAULT '0',
                      aesKey varchar(2048),
                      encryptionActive INTEGER(1) DEFAULT '0',
                      encryptionStage integer(1) default '0',
                      statusText varchar(40),
                      messageText varchar(32767),
                      isOnline INTEGER(1) DEFAULT '0');")
            SqlNonQuery(cliDb, "CREATE TABLE ybBacklog (row_id INTEGER PRIMARY KEY AUTOINCREMENT, serverID integer(5),
                      id integer,
                      content varchar(1024))")
        Catch ex As Exception
            MsgBox("Fatal error creating client database. Error code: " & ex.HResult & ": " & ex.Message & vbCrLf & vbCrLf & ex.StackTrace & vbCrLf & vbCrLf & "Please report to the developers.", MsgBoxStyle.Critical, "Fatal Error")
            End
        End Try
    End Sub

    Function SqlQuery(db As SQLiteConnection, query As String) As List(Of Specialized.NameValueCollection)
        Try
            Dim cmd As SQLiteCommand = db.CreateCommand()
            cmd.CommandText = query
            Dim reader = cmd.ExecuteReader()
            Dim coll As New List(Of Specialized.NameValueCollection)
            While (reader.Read())
                coll.Add(reader.GetValues)
            End While
            reader.Close()
            reader.Dispose()
            Return coll
        Catch
            Return Nothing
        End Try
    End Function

    Function SqlQuerySingle(db As SQLiteConnection, query As String) As Object
        Try
            Dim cmd As SQLiteCommand = db.CreateCommand()
            cmd.CommandText = query
            Dim value = cmd.ExecuteScalar()
            If IsDBNull(value) Then
                Return Nothing
            End If
            If IsNothing(value) Then
                Return Nothing
            End If
            Dim int As Long
            If Long.TryParse(value, int) = True Then
                Return int
            End If
            If value.ToLower = "false" Then
                Return False
            End If
            If value.ToLower = "true" Then
                Return True
            End If
            If value = "" Then
                Return Nothing
            End If
            Return value
        Catch
            Return Nothing
        End Try
    End Function
    Function SqlNonQuery(db As SQLiteConnection, command As String) As Boolean
        Try
            Dim cmd As SQLiteCommand = db.CreateCommand()
            cmd.CommandText = command
            cmd.ExecuteNonQuery()
            cmd.Dispose()
            Return True
        Catch
            Return False
        End Try
    End Function

    Function AddSrvPeertoDB(db As SQLiteConnection, sockID As Integer, endpoint As String)
        Try
            SqlNonQuery(db, "INSERT INTO ybPeers VALUES ('" & sockID & "',
                      NULL,
                      '" & endpoint & "',
                       '0',
                       '0',
                       '0',
                       '0',
                       '0',
                       '" & BespokeTimestamp(Now) & "', '0', NULL, '0', '" & BespokeTimestamp(Now) & "');")
            Return True
        Catch
            Return False
        End Try
    End Function


    Function AddCliPeertoDB(db As SQLiteConnection, serverID As Integer, peerID As Integer, handle As String)
        Try
            SqlNonQuery(db, "INSERT INTO ybPeers (serverID, id, handle) VALUES ('" & serverID & "',
                      '" & peerID & "',
                      '" & handle & "');")
            Return True
        Catch
            Return False
        End Try
    End Function


    Function AddCliIdentitytoDB(db As SQLiteConnection, serverID As Integer, handle As String, remoteEndpoint As String, encrypted As Integer) As Boolean
        Try
            SqlNonQuery(db, "INSERT INTO ybIdentities VALUES ('" & serverID & "',
                      '-1',
                      '" & handle & "',
                      '" & remoteEndpoint & "', '0', '1', '0',
                      '" & encrypted & "', NULL, '0', '0', NULL, NULL, '0');")
            Return True
        Catch
            Return False
        End Try
    End Function

    Function GetConnectedIdentities(db As SQLiteConnection) As List(Of Integer)
        Try
            Dim peers = SqlQuery(db, "SELECT serverID from ybIdentities")
            Dim peerList As New List(Of Integer)
            For Each item In peers
                peerList.Add(item("serverID"))
            Next
            Return peerList
        Catch
            Return Nothing
        End Try
    End Function

    Function DeleteSrvPeerFromDB(db As SQLiteConnection, sockID As Integer)
        Try
            SqlNonQuery(db, "DELETE FROM ybPeers WHERE id = '" & sockID & "';")
            Return True
        Catch
            Return False
        End Try
    End Function

    Function DeleteCliPeerFromDB(db As SQLiteConnection, serverID As Integer, sockID As Integer)
        Try
            SqlNonQuery(db, "DELETE FROM ybPeers WHERE id = '" & sockID & "' AND serverID = '" & serverID & "';")
            Return True
        Catch
            Return False
        End Try
    End Function

    Function DeleteCliIdentityFromDB(db As SQLiteConnection, serverID As Integer)
        Try
            SqlNonQuery(db, "DELETE FROM ybIdentities WHERE serverID = '" & serverID & "';")
            SqlNonQuery(db, "DELETE FROM ybBacklog WHERE serverID = '" & serverID & "';")
            Return True
        Catch
            Return False
        End Try
    End Function

    Function SetSrvPeerAttribute(db As SQLiteConnection, sockID As Integer, column As String, value As Object) As Boolean
        Return SqlNonQuery(db, "UPDATE ybPeers SET " & TrimWhite(column) & " = '" & value.ToString & "' WHERE id = '" & sockID & "';")
    End Function


    Function GetSrvPeerAttribute(db As SQLiteConnection, sockID As Integer, column As String) As Object
        Return SqlQuerySingle(db, "SELECT " & TrimWhite(column) & " FROM ybPeers WHERE id = '" & sockID & "';")
    End Function

    Function GetCliPeerAttribute(db As SQLiteConnection, serverID As Integer, sockID As Integer, column As String) As Object
        Return SqlQuerySingle(db, "SELECT " & TrimWhite(column) & " FROM ybPeers WHERE id = '" & sockID & "' AND serverID = '" & serverID & "';")
    End Function

    Function SetCliPeerAttribute(db As SQLiteConnection, serverID As Integer, sockID As Integer, column As String, value As Object) As Boolean
        Return SqlNonQuery(db, "UPDATE ybPeers SET " & TrimWhite(column) & " = '" & value.ToString & "' WHERE id = '" & sockID & "' AND serverID = '" & serverID & "';")
    End Function

    Function GetSetCliIdentityAttribute(db As SQLiteConnection, serverID As Integer, column As String, Optional value As Object = Nothing) As Object
        If IsNothing(value) Then
            Return SqlQuerySingle(db, "SELECT " & TrimWhite(column) & " FROM ybIdentities WHERE serverID = '" & serverID & "';")
        Else
            Return SqlNonQuery(db, "UPDATE ybIdentities SET " & TrimWhite(column) & " = '" & Replace(value.ToString, "'", "''") & "' WHERE serverID = '" & serverID & "';")
        End If
    End Function

    Function GetSetCliPeerAttribute(db As SQLiteConnection, serverID As Integer, peerID As Integer, column As String, Optional value As Object = Nothing) As Object
        If IsNothing(value) Then
            Return SqlQuerySingle(db, "SELECT " & TrimWhite(column) & " FROM ybPeers WHERE serverID = '" & serverID & "' and id = '" & peerID & "';")
        Else
            Return SqlNonQuery(db, "UPDATE ybIdentities SET " & TrimWhite(column) & " = '" & Replace(value.ToString, "'", "''") & "' WHERE serverID = '" & serverID & "';")
        End If
    End Function

    Function GetSrvPeerFromAttribute(db As SQLiteConnection, attribute As String, value As String) As Integer
        Return SqlQuerySingle(db, "SELECT id FROM ybPeers WHERE " & TrimWhite(attribute) & " = '" & TrimWhite(Replace(value.ToString, "'", "''")) & "';")
    End Function

    Function GetSrvPeerHandles(db) As List(Of String)
        Try
            Dim hndls As New List(Of String)
            Dim results = SqlQuery(db, "SELECT handle from ybPeers;")
            If Not IsNothing(results) Then
                For Each item In results
                    hndls.Add(item("handle"))
                Next
            End If
            Return hndls
        Catch
            Return Nothing
        End Try
    End Function

    Function GetSrvPeerCount(db) As Integer
        Dim peers = SqlQuery(db, "SELECT id from ybPeers")
        If IsNothing(peers) Then
            Return 0
        Else
            Return SqlQuery(db, "SELECT id from ybPeers").Count - 1
        End If
    End Function

    Function GetSrvConnectedPeers(db) As List(Of Integer)
        Dim peers = SqlQuery(db, "SELECT id from ybPeers")
        Dim peerList As New List(Of Integer)
        If Not IsNothing(peers) Then
            For Each Item In peers
                If Not (Item("id") = 0) Then
                    peerList.Add(Item("id"))
                End If
            Next
        End If
        Return peerList
    End Function

    Function GetCliIdentities(db) As List(Of Integer)
        Dim peers = SqlQuery(db, "SELECT serverID from ybIdentities")
        Dim peerList As New List(Of Integer)
        If Not IsNothing(peers) Then
            For Each Item In peers
                If Not (Item("serverID") = 0) Then
                    peerList.Add(Item("serverID"))
                End If
            Next
        End If
        Return peerList
    End Function

    Function GetCliConnectedPeers(db, serverID) As List(Of Specialized.NameValueCollection)
        Dim peers = SqlQuery(db, "SELECT * from ybPeers WHERE serverID = '" & serverID & "';")
        Dim peerList As New List(Of Specialized.NameValueCollection)
        If Not IsNothing(peers) Then
            For Each Item In peers
                If Not Item("id") = 0 Then
                    peerList.Add(Item)
                End If
            Next
        End If
        Return peerList
    End Function

    Function GetCliConnectedPeerIDs(db, serverID) As List(Of Integer)
        Dim peers = SqlQuery(db, "SELECT * from ybPeers WHERE serverID = '" & serverID & "';")
        Dim peerList As New List(Of Integer)
        If Not IsNothing(peers) Then
            For Each Item In peers
                peerList.Add(Item("id"))
            Next
        End If
        Return peerList
    End Function

    Function SrvIncrementValue(db As SQLiteConnection, sockID As Integer, column As String, Optional incrementBy As Integer = 1)
        SqlNonQuery(db, "UPDATE ybPeers SET " & column & " = " & column & " + " & incrementBy & " WHERE id = '" & sockID & "';")
        Return SqlQuerySingle(db, "SELECT " & column & " FROM ybPeers WHERE id = '" & sockID & "';")
    End Function

    Function CliIdentIncrementValue(db As SQLiteConnection, serverID As Integer, column As String, Optional incrementBy As Integer = 1)
        SqlNonQuery(db, "UPDATE ybIdentities SET " & column & " = " & column & " + " & incrementBy & " WHERE id = '" & serverID & "';")
        Return SqlQuerySingle(db, "SELECT " & column & " FROM ybIdentities WHERE id = '" & serverID & "';")
    End Function
End Module

