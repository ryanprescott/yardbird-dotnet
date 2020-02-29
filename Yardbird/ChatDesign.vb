Imports System.Text.RegularExpressions
Module ChatDesign

    Public ColorDictionary As New Dictionary(Of Integer, Color) From {{0, Color.Black}, {1, Color.Navy}, {2, Color.DarkGreen},
                                                                        {3, Color.Teal}, {4, Color.Maroon}, {5, Color.Purple},
                                                                        {6, Color.Olive}, {7, Color.Silver}, {8, Color.Gray},
                                                                        {9, Color.Blue}, {10, Color.Green}, {11, Color.Cyan},
                                                                        {12, Color.Red}, {13, Color.Fuchsia}, {14, Color.Yellow},
                                                                        {15, Color.White}}
    Dim chatWindows(256) As WebBrowser
    ' Color aliasing algorithm
    Public baseString As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghjiklmnopqrstuvwxyz0123456789-=+"
    Dim colorMap As Bitmap = My.Resources.am
    Dim colorMapBytes As Byte()
    Dim colorMapBMData As Imaging.BitmapData
    Dim bitLocked As Boolean = False
    Dim previouslyAliased As New Dictionary(Of Color, String)
    Dim aCmdHandles As List(Of Integer)

    ' Invoking errors to dismiss if the control disappears during a chat append
    Dim invkErrs As New List(Of Integer) From {&H80070057, &H80131509}
    Public emoticonDictionary As New Dictionary(Of String(), String) From {
                                                                                  {{":)", ":-)"}, "🙂"},
                                                                                  {{":D", ":-D"}, "😃"},
                                                                                  {{":(", ":-("}, "😞"},
                                                                                  {{";)", ";-)"}, "😉"},
                                                                                  {{":/", ":\"}, "😏"},
                                                                                  {{";P", ";-P"}, "😜"},
                                                                                  {{":P", ":-P"}, "😛"},
                                                                                  {{"ToT"}, "😭"},
                                                                                  {{":'(", ":'-("}, "😢"},
                                                                                  {{":')", ":'-)"}, "😂"},
                                                                                  {{":O", ":-O", ":o", ":-o"}, "😮"},
                                                                                  {{":*", ":-*", ":x", ":-x"}, "😙"},
                                                                                  {{";*", ";-*", ";x", ";-x"}, "😘"},
                                                                                  {{"^_^", "^-^"}, "😊"}}
    Function CreateNewChatWindow(i As Integer) As WebBrowser
        If IsNothing(chatWindows(i)) Then
            Dim cw As New WebBrowser
            cw.Dock = DockStyle.Fill
            cw.ScriptErrorsSuppressed = True
            cw.ScrollBarsEnabled = True
            cw.Name = "room" & i
            cw.IsWebBrowserContextMenuEnabled = False
            AddHandler cw.Navigated, AddressOf chatWindowNavigated
            chatWindows(i) = cw
            frmMain.SplitContainer4.Panel1.Controls.Add(chatWindows(i))
            ActivateChatWindow(i)
            Return chatWindows(i)
        End If
        Return Nothing
    End Function

    Function DisposeChatWindow(i As Integer) As Boolean
        If Not IsNothing(chatWindows(i)) Then
            chatWindows(i).Dispose()
            chatWindows(i) = Nothing
            Return True
        Else
            Return True
        End If
        Return Nothing
    End Function
    Sub ActivateChatWindow(sockID As Integer)
        For i = 0 To chatWindows.Count - 1
            If Not IsNothing(chatWindows(i)) Then
                If Not i = sockID Then
                    chatWindows(i).Visible = False
                End If
            End If
        Next
        If Not IsNothing(chatWindows(sockID)) Then
            chatWindows(sockID).Visible = True
            GetSetActiveClient(sockID)
            frmMain.Invoke(Sub() frmMain.tvwServers.SelectedNode = frmMain.tvwServers.Nodes(sockID.ToString))
            frmMain.Invoke(Sub() frmMain.ShowClientSavedState(sockID))
            frmMain.Invoke(Sub() frmMain.PopulatePeersInList(sockID))
        End If
    End Sub
    Function GetChatWindow(sockID As Integer) As WebBrowser
        Return chatWindows(sockID)
    End Function
    Function GetActiveChatWindow() As WebBrowser
        Return chatWindows(GetSetActiveClient)
    End Function
    Sub chatWindowNavigated(sender As WebBrowser, e As WebBrowserNavigatedEventArgs)
        sender.Document.Window.AttachEventHandler("onscroll", Sub() OnScrollEventHandler(sender, EventArgs.Empty))
        If Split(e.Url.ToString, "#").Count > 1 Then
            If Split(Split(e.Url.ToString, "#")(1), "/").Count > 1 Then
                Dim action = Split(Split(e.Url.ToString, "#")(1), "[")(0)
                Dim value = Split(Split(e.Url.ToString, "#")(1), "[")(1)
                Select Case action
                    Case "aOpen"
                        Dim r = MsgBox("This external link points to " & value & "." & vbCrLf & "Are you sure you want to open this link in a web browser?", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo, "External Link")
                        If r = vbYes Then
                            Process.Start(value)
                        End If
                    Case "aBlock"
                        Dim r = MsgBox("This link is invalid and you have been blocked from opening it." & vbCrLf & vbCrLf &
                                       "Possible reasons include:" & vbCrLf &
                                       "- The link was formatted incorrectly." & vbCrLf &
                                       "- The link contained malicious content." & vbCrLf, MsgBoxStyle.Critical, "Invalid Link")
                End Select
            End If
        End If
    End Sub

    Sub OnScrollEventHandler(sender As WebBrowser, e As EventArgs)
        Dim container = sender.Document.GetElementById("container")
        Dim scrollTop As Integer = sender.Document.GetElementsByTagName("HTML")(0).ScrollTop
        Dim scrollBottom As Integer = sender.Document.GetElementsByTagName("HTML")(0).ScrollTop + sender.Height
        If scrollTop = 0 Then
            If container.Children.Count > 0 Then
                If container.Children(0).GetAttribute("id") = String.Empty Then
                    MsgBox("Fatal error: the theme you are using does not support backlogging." & vbCrLf, MsgBoxStyle.Critical, "Fatal Error")
                    Exit Sub
                Else
                    Dim messageID = CInt(container.Children(0).GetAttribute("id"))
                    Dim messagesToLoad As String = vbCrLf
                    For i = (messageID - cliSettings.GetKey("Backlog", "BacklogRetrievalAmount", 10)) To (messageID - 1)
                        If Not i < 1 Then
                            Dim messageText = SqlQuerySingle(cliDb, "SELECT content FROM ybBacklog WHERE id = '" & i & "' and serverid = '" & Replace(sender.Name, "room", "") & "';")
                            If Not IsNothing(messageText) Then
                                messagesToLoad = messagesToLoad & messageText & vbCrLf
                            End If
                        End If
                    Next
                    Dim oldHeight = container.Document.Window.Size.Height
                    container.InnerHtml = messagesToLoad & container.InnerHtml
                    sender.Document.GetElementsByTagName("HTML")(0).ScrollTop = container.Document.Window.Size.Height - oldHeight
                End If
            End If
        End If
        If scrollBottom > (sender.Document.Window.Size.Height * 0.9) Then
            Dim oldScrollTop = scrollTop
            Dim oldHeight = container.Document.Window.Size.Height
            Dim pElements = container.GetElementsByTagName("P")
            Dim chatElements As New List(Of HtmlElement)
            For Each Item In pElements
                If Item.GetAttribute("ClassName").ToString.Contains("chatElement") Then
                    chatElements.Add(Item)
                End If
            Next
            If chatElements.Count > cliSettings.GetKey("Backlog", "VisibleItemLimit", 100) Then
                For i = 0 To (chatElements.Count - cliSettings.GetKey("Backlog", "VisibleItemLimit", 100) - 1)
                    chatElements(i).OuterHtml = ""
                Next
            End If
            sender.Document.GetElementsByTagName("HTML")(0).ScrollTop = oldScrollTop + (container.Document.Window.Size.Height - oldHeight)
        End If
    End Sub
    Function FormatSubstitute(markupStr As String, Optional obfuscateLinks As Boolean = True)
        Dim ss = markupStr
        ' Style formatting
        Dim formattingRegex = "@([A-Z|a-z|1-7]{1})(\[(?:[^\[\]]|(?<c>\[)|(?<-c>\]))+(?(c)(?!))\])"
        While Regex.Matches(ss, formattingRegex).Count > 0
            Dim formattingMatches = Regex.Matches(ss, formattingRegex)
            For Each Item As Match In formattingMatches
                Dim formatCode = Item.Groups(1).Captures.Item(0).Value.ToUpper
                Dim text = Item.Groups(2).Captures.Item(0).Value
                If text.Chars(0) = "[" Then
                    text = text.Substring(1, text.Length - 1)
                End If
                If text.Chars(text.Length - 1) = "]" Then
                    text = text.Substring(0, text.Length - 1)
                End If
                Select Case formatCode
                    Case "B"
                        ss = Replace(ss, Item.Value, "<strong>" & text & "</strong>")
                    Case "I"
                        ss = Replace(ss, Item.Value, "<em>" & text & "</em>")
                    Case "U"
                        ss = Replace(ss, Item.Value, "<u>" & text & "</u>")
                    Case "A"
                        Dim linkValues = Split(System.Net.WebUtility.HtmlDecode(text), "|")
                        If linkValues.Count > 1 Then
                            If obfuscateLinks = True Then
                                If Uri.IsWellFormedUriString(linkValues(1), UriKind.Absolute) Then
                                    If linkValues(1).Substring(0, 4) = "http" Then
                                        ss = Replace(ss, Item.Value, "<a title='" & linkValues(1) & "' href='#aOpen[" & linkValues(1) & "'>" & linkValues(0) & "</a>")
                                    Else
                                        ss = Replace(ss, Item.Value, "<a title='This link is invalid.' class='disabledlink' href='#aBlock['>" & linkValues(0) & "</a>")
                                    End If
                                Else
                                    ss = Replace(ss, Item.Value, "<a title='This link is invalid.' class='disabledlink' href='#aBlock['>" & linkValues(0) & "</a>")
                                End If
                            Else
                                ss = Replace(ss, Item.Value, "<a title='" & linkValues(1) & "' href='" & linkValues(1) & "'>" & linkValues(0) & "</a>")
                            End If
                        Else
                            ss = Replace(ss, Item.Value, "<a href='#aOpen['>" & text & "</a>")
                        End If
                        ss = Replace(ss, Item.Value, "<u>" & text & "</u>")
                    Case "X"
                        ss = Replace(ss, Item.Value, "<strike>" & text & "</strike>")
                    Case "1", "2", "3", "4", "5", "6", "7"
                        ss = Replace(ss, Item.Value, "<FONT SIZE=" & formatCode & ">" & text & "</FONT>")

                End Select
            Next
        End While
        ' Default color formatting
        Dim dftColorRegex = "\$(b|B|f|F)([A-F|a-f|0-9])(\[(?:[^\[\]]|(?<c>\[)|(?<-c>\]))+(?(c)(?!))\])"
        Dim dftColorMatches = Regex.Matches(ss, dftColorRegex)
        While dftColorMatches.Count > 0
            Dim fcolormatches = Regex.Matches(ss, dftColorRegex)
            For Each Item As Match In fcolormatches
                Dim setType = Item.Groups(1).Captures.Item(0).Value.ToUpper
                Dim colorCode = Convert.ToInt32(Item.Groups(2).Captures.Item(0).Value, 16)
                Dim colorName = ColorDictionary(colorCode).Name
                Dim text = Item.Groups(3).Captures.Item(0).Value
                If text.Chars(0) = "[" Then
                    text = text.Substring(1, text.Length - 1)
                End If
                If text.Chars(text.Length - 1) = "]" Then
                    text = text.Substring(0, text.Length - 1)
                End If
                Select Case setType
                    Case "B"
                        ss = Replace(ss, Item.Value, "<span style='background-color: " & colorName & "'>" & text & "</span>")
                    Case "F"
                        ss = Replace(ss, Item.Value, "<font color=" & colorName & ">" & text & "</font>")
                End Select
                dftColorMatches = Regex.Matches(ss, dftColorRegex)
            Next
        End While

        ' Aliased (compressed) color formatting
        Dim alsColorRegex = "\^(b|B|f|F)([A-Z|a-z|0-9|\-|+|=]{2})(\[(?:[^\[\]]|(?<c>\[)|(?<-c>\]))+(?(c)(?!))\])"
        Dim alsColorMatches = Regex.Matches(ss, alsColorRegex)
        While alsColorMatches.Count > 0
            For Each Item As Match In alsColorMatches
                Dim cAlias = "^" & Item.Groups(2).Captures.Item(0).Value
                Dim hexCode = "#000000"
                If Not IsNothing(DealiasColor(cAlias)) Then
                    hexCode = ColortoHexCode(DealiasColor(cAlias))
                End If
                Dim setType = Item.Groups(1).Captures.Item(0).Value.ToUpper
                Dim text = Item.Groups(3).Captures.Item(0).Value
                If text.Chars(0) = "[" Then
                    text = text.Substring(1, text.Length - 1)
                End If
                If text.Chars(text.Length - 1) = "]" Then
                    text = text.Substring(0, text.Length - 1)
                End If
                Select Case setType
                    Case "B"
                        ss = Replace(ss, Item.Value, "<span style='background-color: " & hexCode & "'>" & text & "</span>")
                    Case "F"
                        ss = Replace(ss, Item.Value, "<font color=" & hexCode & ">" & text & "</font>")
                End Select
                alsColorMatches = Regex.Matches(ss, alsColorRegex)
            Next
        End While
        ' Hex color formatting
        Dim hexColorRegex = "\#(b|B|f|F)([A-F|a-f|0-9|\-|+|=]{6})(\[(?:[^\[\]]|(?<c>\[)|(?<-c>\]))+(?(c)(?!))\])"
        Dim hexColorMatches = Regex.Matches(ss, hexColorRegex)
        While hexColorMatches.Count > 0
            For Each Item As Match In hexColorMatches
                Dim setType = Item.Groups(1).Captures.Item(0).Value.ToUpper
                Dim hexCode = Item.Groups(2).Captures.Item(0).Value.ToUpper
                Dim text = Regex.Replace(Item.Groups(3).Captures.Item(0).Value, "\[(.*?)\]", "$1")
                If text.Chars(0) = "[" Then
                    text = text.Substring(1, text.Length - 1)
                End If
                If text.Chars(text.Length - 1) = "]" Then
                    text = text.Substring(0, text.Length - 1)
                End If
                Select Case setType
                    Case "B"
                        ss = Replace(ss, Item.Value, "<span style='background-color: " & hexCode & "'>" & text & "</span>")
                    Case "F"
                        ss = Replace(ss, Item.Value, "<font color=" & hexCode & ">" & text & "</font>")
                End Select
                hexColorMatches = Regex.Matches(ss, hexColorRegex)
            Next
        End While
        'ss = Replace(ss, Item.Value, "<span class='" & ClassBuilder & "'>" & DelimitedItem(1) & "</span>")
        ss = Replace(ss, "\n", "<br />")
        Return ss
    End Function
    Function GetTheme(themeName As String, Optional uri As Boolean = True)
        Try
            Dim themePath = IO.Path.Combine(Application.StartupPath, "assets", "themes", themeName)
            Dim contentPath = IO.Path.Combine(themePath, "content.html")
            Dim stylePath = IO.Path.Combine(themePath, "default.css")
            If IO.Directory.Exists(themePath) And IO.File.Exists(contentPath) And IO.File.Exists(stylePath) Then
                If uri = True Then
                    Return RealPathtoWebPath(themePath)
                Else
                    Return themePath
                End If
                Return ""
            End If
        Catch ex As Exception
            ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
            Return ""
        End Try
        Return ""
    End Function
    Function HexCodetoColorCode(hexCode As String, Optional h As Boolean = False)
        If hexCode.Length = 7 Then
            If hexCode.Substring(0, 1) = "#" Then
                Dim R, G, B As Integer
                Try
                    R = Convert.ToInt32(hexCode.Substring(1, 2), 16)
                    G = Convert.ToInt32(hexCode.Substring(3, 2), 16)
                    B = Convert.ToInt32(hexCode.Substring(5, 2), 16)

                Catch
                    Return Nothing
                End Try
                If R >= 0 And R <= 255 And
               G >= 0 And G <= 255 And
               B >= 0 And B <= 255 Then
                    Return ColorToCode(Color.FromArgb(R, G, B), h)
                Else
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
        Else
            Return Nothing
        End If
    End Function
    Function HexCodetoColor(hexCode As String)
        If hexCode.Length = 7 Then
            If hexCode.Substring(0, 1) = "#" Then
                Dim R, G, B As Integer
                Try
                    R = Convert.ToInt32(hexCode.Substring(1, 2), 16)
                    G = Convert.ToInt32(hexCode.Substring(3, 2), 16)
                    B = Convert.ToInt32(hexCode.Substring(5, 2), 16)

                Catch
                    Return Nothing
                End Try
                If R >= 0 And R <= 255 And
               G >= 0 And G <= 255 And
               B >= 0 And B <= 255 Then
                    Return Color.FromArgb(R, G, B)
                Else
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
        Else
            Return Nothing
        End If
    End Function

    Function HexCodeValidate(hexCode As String)
        If hexCode.Length = 7 Then
            If hexCode.Substring(0, 1) = "#" Then
                Dim R, G, B As Integer
                Try
                    R = Convert.ToInt32(hexCode.Substring(1, 2), 16)
                    G = Convert.ToInt32(hexCode.Substring(3, 2), 16)
                    B = Convert.ToInt32(hexCode.Substring(5, 2), 16)

                Catch
                    Return False
                End Try
                If R >= 0 And R <= 255 And
               G >= 0 And G <= 255 And
               B >= 0 And B <= 255 Then
                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If
        Else
            Return False
        End If
    End Function

    Function ColortoHexCode(c As Color)
        Return "#" & c.R.ToString("X2") & c.G.ToString("X2") & c.B.ToString("X2")
    End Function

    Function ColorToCode(c As Color, Optional h As Boolean = False)
        Dim tmpKey = Nothing
        For Each Item As KeyValuePair(Of Integer, Color) In ColorDictionary
            If Convert.ToInt32(Item.Value.ToArgb) = Convert.ToInt32(c.ToArgb) Then
                tmpKey = Item.Key
                Exit For
            End If
        Next
        If h = True Then
            If IsNothing(tmpKey) Then
                Return Nothing
            End If
            Return Hex(tmpKey).ToUpper
        Else
            Return tmpKey
        End If
    End Function

    Function CreateClientNodeContextMenu(sockID As Integer)
        Dim ctxt = New ContextMenu
        ctxt.Name = "ctxt" & sockID
        Dim iHeading = ctxt.MenuItems.Add("Options for " & GetSetCliIdentityAttribute(cliDb, sockID, "remoteendpoint").ToString)
        iHeading.Name = "mnuHeading" & sockID
        iHeading.Enabled = False
        Dim iSep = ctxt.MenuItems.Add("-")
        iSep.Name = "mnuSep" & sockID
        Dim iConnect = ctxt.MenuItems.Add("Connect")
        iConnect.Name = "mnuConnect" & sockID
        AddHandler iConnect.Click, AddressOf NodeContextMenuClick
        iConnect.Enabled = False
        Dim iDisconnect = ctxt.MenuItems.Add("Disconnect")
        iDisconnect.Name = "mnuDisconnect" & sockID
        AddHandler iDisconnect.Click, AddressOf NodeContextMenuClick
        Dim iRemove = ctxt.MenuItems.Add("Remove")
        iRemove.Name = "mnuRemove" & sockID
        AddHandler iRemove.Click, AddressOf NodeContextMenuClick
        iRemove.Enabled = False
        Dim iSounds = ctxt.MenuItems.Add("Message Sounds")
        iSounds.Name = "mnuSounds" & sockID
        AddHandler iSounds.Click, AddressOf NodeContextMenuClick
        iSounds.Checked = True
        iSounds.Enabled = True
        Return ctxt
    End Function

    Sub NodeContextMenuClick(sender As MenuItem, e As EventArgs)
        Dim getActionAndSocket = Regex.Match(sender.Name, "\Amnu([A-Z|a-z]+)([0-9]{1,3})\z")
        Dim action As String = String.Empty
        Dim socket As Integer = 0
        If getActionAndSocket.Groups.Count > 1 Then
            action = getActionAndSocket.Groups(1).Captures(0).Value.ToString
            socket = CInt(getActionAndSocket.Groups(2).Captures(0).Value.ToString)
        Else
            Exit Sub
        End If
        ContextMenuHandleClientAction(action, socket, sender.GetContextMenu)
    End Sub

    Sub ContextMenuHandleClientAction(action As String, socket As Integer, Optional sender As ContextMenu = Nothing)
        If IsNothing(sender) Then
            For Each Item As TreeNode In frmMain.tvwServers.Nodes
                If Not IsNothing(Item.ContextMenu) Then
                    If Item.ContextMenu.Name = "ctxt" & socket Then
                        sender = Item.ContextMenu
                        Exit For
                    End If
                End If
            Next
        End If
        If IsNothing(sender) Then
            Exit Sub
        End If
        Select Case action
            Case "Connect"
                Dim ipandport = GetSetCliIdentityAttribute(cliDb, socket, "remoteEndpoint")
                Dim handle = GetSetCliIdentityAttribute(cliDb, socket, "handle")
                Dim encrypted = GetSetCliIdentityAttribute(cliDb, socket, "encrypted")
                InitClient(socket, Split(ipandport, ":")(0), Split(ipandport, ":")(1), handle, encrypted)
                sender.MenuItems(2).Enabled = False
                sender.MenuItems(3).Enabled = True
                sender.MenuItems(4).Enabled = False
            Case "Disconnect"
                ActivateChatWindow(socket)
                WriteClientData(socket, "QUIT•Disconnecting from server", False)
                sender.MenuItems(3).Enabled = False
                sender.MenuItems(2).Enabled = True
                sender.MenuItems(4).Enabled = True
                frmMain.ChangeStatusText(socket, "Disconnecting...")
            Case "Remove"
                DeinitClient(socket)
                frmMain.tvwServers.Nodes.RemoveByKey(socket)
                DeleteCliIdentityFromDB(cliDb, socket)
                DisposeChatWindow(socket)
            Case "Sounds"
                sender.MenuItems(5).Checked = Not sender.MenuItems(5).Checked
                GetSetCliIdentityAttribute(cliDb, socket, "sounds", Math.Abs(CInt(sender.MenuItems(5).Checked)))
        End Select
    End Sub

    Function LoadTheme(themeName As String, browser As WebBrowser, Optional silent As Boolean = False, Optional sideload As Boolean = True)
        If Not GetTheme(themeName) = "" Then
            If sideload = False Then
                browser.Navigate(GetTheme(themeName) & "/content.html")
            Else
                Dim containerBackup As String = browser.Document.GetElementById("container").InnerHtml
                browser.Navigate(GetTheme(themeName) & "/content.html")
                Dim iterator = 0
                While Not (browser.ReadyState = WebBrowserReadyState.Complete)
                    If iterator = 100 Then
                        Throw New SystemException("Chat window never completed navigating.")
                    End If
                    Application.DoEvents()
                    iterator += 1
                End While
                browser.Document.GetElementById("container").InnerHtml = browser.Document.GetElementById("container").InnerHtml & containerBackup
            End If
            If silent = False Then
                ChatAppend(GetActiveChatWindow(), "alert", " information", "Theme '" & themeName & "' loaded successfully!")
            End If
            Return True
        Else
            Return False
        End If
    End Function
    Function Sanitize(text As String)
        Dim tempText = System.Net.WebUtility.HtmlEncode(text)
        tempText = Replace(tempText, "•", "&#149;")
        Return tempText
    End Function
    Function ChatAppend(browser As WebBrowser, struct As String, classes As String, text As String, Optional handle As String = "")
        If Not IsNothing(browser) Then
            Try
                browser.Invoke(Sub()
                                   Try
                                       Dim iterator = 0
                                       While Not (browser.ReadyState = WebBrowserReadyState.Complete) And iterator < 100
                                           Application.DoEvents()
                                           iterator += 1
                                       End While
                                       With browser
                                           Dim container = .Document.GetElementById("container")
                                           If IsNothing(container) Then
                                               Throw New System.ArgumentNullException("Missing container element.")
                                           Else
                                               If IsNothing(.Document.GetElementsByTagName("structures")(0).GetElementsByTagName(struct)(0)) Then
                                                   Throw New System.ArgumentNullException("Missing structure element.")
                                               Else
                                                   Dim messageText = FormatSubstitute(text)

                                                   If True Then
                                                       Try
                                                           For Each Item In emoticonDictionary
                                                               For Each Emote In Item.Key
                                                                   If TrimWhite(messageText) = Emote Then
                                                                       messageText = Item.Value
                                                                       Exit For
                                                                   End If
                                                                   If Regex.Match(messageText, "(?>\s|\A)(" & Regex.Escape(Emote) & ")(?>\s|\z)").Groups.Count > 1 Then
                                                                       messageText = Replace(messageText, Emote, Item.Value)
                                                                   End If
                                                               Next
                                                           Next
                                                       Catch
                                                       End Try
                                                   End If
                                                   Dim serverID = Replace(browser.Name, "room", "")
                                                   Dim newMsgID = SqlQuerySingle(cliDb, "SELECT MAX(id) FROM ybBacklog WHERE serverID = '" & serverID & "';") + 1
                                                   If newMsgID > cliSettings.GetKey("Backlog", "BacklogTotalLimit", 1000) Then
                                                       Dim smallestID = SqlQuerySingle(cliDb, "SELECT MIN(id) FROM ybBacklog WHERE serverID = '" & serverID & "';")
                                                       SqlNonQuery(cliDb, "DELETE FROM ybBacklog WHERE id = '" & smallestID & "' AND serverID = '" & serverID & "';")
                                                   End If
                                                   Dim htmlToReplace = .Document.GetElementsByTagName("structures")(0).GetElementsByTagName(struct)(0).InnerHtml
                                                   htmlToReplace = Replace(htmlToReplace, "%h", handle)
                                                   htmlToReplace = Replace(htmlToReplace, "%c", classes)
                                                   htmlToReplace = Replace(htmlToReplace, "%i", newMsgID)
                                                   htmlToReplace = Replace(htmlToReplace, "%t", TimeString())
                                                   htmlToReplace = Replace(htmlToReplace, "%m", messageText)
                                                   container.InnerHtml = container.InnerHtml & htmlToReplace
                                                   SqlNonQuery(cliDb, "INSERT INTO ybBacklog (serverID, id, content) VALUES ('" & Replace(browser.Name, "room", "") & "','" & newMsgID & "','" & TrimWhite(Replace(htmlToReplace, "'", "''")) & "');")
                                                   .Document.Body.Children.Item(.Document.Body.Children.Count - 1).ScrollIntoView(False)
                                               End If
                                               End If
                                       End With
                                   Catch ex As Exception
                                       MsgBox("An error has occurred in rendering the current theme: " & Hex(ex.HResult) & ": " & ex.Message & " Possible reasons that this may occur:" & vbCrLf & vbCrLf &
"- The theme did not load correctly. Please reload the theme and reconnect to the server." & vbCrLf & vbCrLf &
"- The theme is not structured correctly. The theme body should contain a container element." & vbCrLf & vbCrLf &
"- The theme is corrupted.", MsgBoxStyle.Exclamation, "Theme Rendering Error")
                                   End Try
                               End Sub)
                Return True
            Catch ex As Exception
                If invkErrs.Contains(ex.HResult) = False Then
                    MsgBox("An error has occurred in rendering the current theme: " & Hex(ex.HResult) & ": " & ex.Message & " Possible reasons that this may occur:" & vbCrLf & vbCrLf &
"- The theme did not load correctly. Please reload the theme and reconnect to the server." & vbCrLf & vbCrLf &
"- The theme is not structured correctly. The theme body should contain a container element." & vbCrLf & vbCrLf &
"- The theme is corrupted.", MsgBoxStyle.Exclamation, "Theme Rendering Error")
                End If
                Return False
            End Try
        Else
            Return False
        End If
        Return False
    End Function

    Function CliErrHndl(sockID As Integer, code As Integer, str As String, Optional errorReason As String = "")
        Dim errText As String = "An error has occurred. The server provided the following message: " & str
        Select Case code
            Case 100
                errText = "The client has sent invalid data to the server."
            Case 101
                If errorReason.Length > 0 Then
                    errText = "The command '" & errorReason & "' is not implemented in the Yardbird protocol."
                Else
                    errText = "This command is not implemented in the Yardbird protocol."
                End If
            Case 102
                errText = "You are being disconnected due to inactivity."
                dlgNewHndl.GracefulBye()
            Case 105
                If errorReason.Length > 0 Then
                    errText = "You have already chosen handle '" & errorReason & "'; you don't need to identify again."
                Else
                    errText = "You have already chosen a handle; you don't need to identify again."
                End If
            Case 107
                Dim dlg = New dlgNewHndl
                If errorReason.Length > 0 Then
                    errText = "The handle '" & errorReason & "' is already in use.  If you do not identify before the next garbage collection, you will be disconnected."
                    Dim hndlText = errorReason & "_"
                    If (hndlText.Length > 20) Then
                        hndlText = hndlText.Substring(0, 19) & "_"
                    End If
                    dlg.hndlText = hndlText
                Else
                    errText = "The specified handle is already in use.  If you do not identify before the next garbage collection, you will be disconnected."
                End If
                dlg.ShowDialog()
                If dlg.hndlText <> "" Then
                    WriteClientData(sockID, "HELO•" & dlg.hndlText)
                Else
                    WriteClientData(sockID, "QUIT•" & dlg.hndlText)
                End If
                dlg.Dispose()
            Case 108
                Dim dlg = New dlgNewHndl
                If errorReason.Length > 0 Then
                    Dim hndlText = errorReason
                    hndlText = Regex.Replace(hndlText, "[^A-Z|a-z-|0-9|\-|_]", "")
                    If (hndlText.Length > 20) Then
                        hndlText = hndlText.Substring(0, 20)
                    End If
                    dlg.hndlText = hndlText
                    errText = "The handle '" & errorReason & "' is invalid.  If you do not identify before the next garbage collection, you will be disconnected."
                Else
                    errText = "The specified handle is invalid.  If you do not identify before the next garbage collection, you will be disconnected."
                End If
                dlg.dlgText = "The specified handle is invalid. Please specify a new handle."
                dlg.ShowDialog()
                If dlg.hndlText <> "" Then
                    WriteClientData(sockID, "HELO•" & dlg.hndlText)
                Else
                    WriteClientData(sockID, "QUIT•" & dlg.hndlText)
                End If
                dlg.Dispose()
            Case 109
                errText = "You have exceeded the maximum amount of identification attempts. You will now be disconnected."
            Case 200
                errText = "You are doing that too much. You will now be disconnected."
            Case 201
                errText = "You tried to impersonate the SERVER user. You will now be disconnected."
            Case 202
                errText = "You must wait to do that again."
            Case 203
                errText = "You have been AFK for too long."
            Case 200 To 299
                If errorReason.Length > 0 Then
                    errText = "Flood and Intrusion Control: " & errorReason
                Else
                    errText = "Flood and Intrusion Control: Unspecified error."
                End If
            Case 111
        End Select
        Return errText
    End Function

    Function Cls(browser As WebBrowser)
        If Not IsNothing(browser) Then
            If Not IsNothing(browser.Document) Then
                If Not IsNothing(browser.Document.GetElementById("container")) Then
                    Try
                        browser.Document.GetElementById("container").InnerHtml = ""
                        Return True
                    Catch ex As Exception
                        Return False
                    End Try
                Else
                    Return False
                End If
            Else
                Return False
            End If
        Else
            Return False
        End If
    End Function

    Function FormatDlg(text As String, Optional start As Integer = 0, Optional length As Integer = 0)
        Dim dlg As New frmFmt
        dlg.textToEdit = text
        dlg.Owner = frmMain
        dlg.ShowDialog()
        Return dlg.editedText
        dlg.Dispose()
    End Function

    Function HTMLtoYardbird(html As HtmlElement, Optional aliasColors As Boolean = False)
        Dim paragraphs As HtmlElementCollection = html.GetElementsByTagName("P")
        Dim outString As String = ""
        If paragraphs.Count = 0 Then
            Return ""
        Else
            For Each paragraph As HtmlElement In paragraphs
                outString = outString & HTMLIteratePBlocks(paragraph, aliasColors) & vbCrLf
            Next
            outString = System.Net.WebUtility.HtmlDecode(Replace(TrimWhite(outString), vbCrLf, "\n"))
            Return outString
        End If
    End Function
    Function HTMLIteratePBlocks(html As HtmlElement, aliasColors As Boolean)
        If html.InnerText <> "" Then
            If html.Children.Count > 0 Then
                While html.Children.Count > 0
                    For Each Child As HtmlElement In html.Children
                        Dim reformattedChild = ""
                        Select Case Child.TagName
                            Case "B", "STRONG"
                                reformattedChild = "@B[" & Child.InnerHtml & "]"
                                html.InnerHtml = Replace(html.InnerHtml, Child.OuterHtml, reformattedChild)
                            Case "I", "EM"
                                reformattedChild = "@I[" & Child.InnerHtml & "]"
                                html.InnerHtml = Replace(html.InnerHtml, Child.OuterHtml, reformattedChild)
                            Case "U"
                                reformattedChild = "@U[" & Child.InnerHtml & "]"
                                html.InnerHtml = Replace(html.InnerHtml, Child.OuterHtml, reformattedChild)
                            Case "FONT", "SPAN"
                                reformattedChild = FontFormatter(Child, aliasColors)
                                html.InnerHtml = Replace(html.InnerHtml, Child.OuterHtml, reformattedChild)
                            Case "A"
                                reformattedChild = LinkFormatter(Child, aliasColors)
                                html.InnerHtml = Replace(html.InnerHtml, Child.OuterHtml, reformattedChild)
                            Case Else
                                reformattedChild = Child.InnerHtml
                                html.InnerHtml = Replace(html.InnerHtml, Child.OuterHtml, reformattedChild)
                        End Select
                    Next
                End While
            End If
        End If
        Return html.InnerHtml
    End Function

    Function FontFormatter(Child As HtmlElement, Optional aliasColors As Boolean = False)
        Dim reformattedChild = ""
        Select Case Child.TagName
            Case "FONT", "SPAN", "A"
                If Not (IsNothing(Child.GetAttribute("COLOR")) Or Child.GetAttribute("COLOR") = "") Then
                    reformattedChild = ColorFormatter(Child, Child.GetAttribute("COLOR"), aliasColors)
                Else
                    If Not (IsNothing(Child.Style) Or Child.Style = "") Then
                        Dim css = GetStyles(Child.Style)
                        If css.ContainsKey("COLOR") Then
                            reformattedChild = ColorFormatter(Child, css("COLOR"), aliasColors)
                        End If
                    Else
                        Return Child.InnerHtml
                    End If
                End If
            Case Else
                Return Child.InnerHtml
        End Select
        Return reformattedChild
    End Function
    Function LinkFormatter(Child As HtmlElement, Optional aliasColors As Boolean = False)
        Dim reformattedChild = ""
        Select Case Child.TagName
            Case "A"
                If Not (IsNothing(Child.GetAttribute("HREF")) Or Child.GetAttribute("HREF") = "") Then
                    reformattedChild = "@a[" & Child.InnerHtml & "|" & Child.GetAttribute("HREF") & "]"
                Else
                    If Not (IsNothing(Child.Style) Or Child.Style = "") Then
                        Dim css = GetStyles(Child.Style)
                        If css.ContainsKey("COLOR") Then
                            reformattedChild = ColorFormatter(Child, css("COLOR"), aliasColors)
                        End If
                    Else
                        Return Child.InnerHtml
                    End If
                End If
            Case Else
                Return Child.InnerHtml
        End Select
        Return reformattedChild
    End Function

    Function ColorFormatter(Child As HtmlElement, htmlColor As String, Optional aliasColors As Boolean = False)

        ' Declare variables
        Dim reformattedChild = ""
        Dim newColor As Color = Color.Black()

        ' Color validation and conversion

        If HexCodeValidate(htmlColor) Then
            newColor = HexCodetoColor(htmlColor)
        Else
            Dim rgbRegex = "(?i)rgb\(([0-9]{0,3})\,([0-9]{0,3})\,([0-9]{0,3})\)\z"
            Dim argbRegex = "(?i)rgba\(([0-9]{0,3})\,([0-9]{0,3})\,([0-9]{0,3})\,([0-9]|[0-9].[0-9])\)\z"
            If Regex.Match(htmlColor, rgbRegex).Success Then
                Dim r = CInt(Regex.Match(htmlColor, rgbRegex).Groups(1).Captures(0).Value)
                Dim g = CInt(Regex.Match(htmlColor, rgbRegex).Groups(2).Captures(0).Value)
                Dim b = CInt(Regex.Match(htmlColor, rgbRegex).Groups(3).Captures(0).Value)
                If (r >= 0 Or r <= 255) And (g >= 0 Or b <= 255) And (b >= 0 Or b <= 255) Then
                    newColor= Color.FromArgb(r, g, b)
                End If
            Else
                If Regex.Match(htmlColor, argbRegex).Success Then
                    Dim r = CInt(Regex.Match(htmlColor, rgbRegex).Groups(1).Captures(0).Value)
                    Dim g = CInt(Regex.Match(htmlColor, rgbRegex).Groups(2).Captures(0).Value)
                    Dim b = CInt(Regex.Match(htmlColor, rgbRegex).Groups(3).Captures(0).Value)
                    Dim a = CInt(((Convert.ToDouble(Regex.Match(htmlColor, rgbRegex).Groups(2).Captures(0).Value) * 100) / 100) * 255)
                    If (r >= 0 Or r <= 255) And (g >= 0 Or b <= 255) And (b >= 0 Or b <= 255) And (a >= 0 Or b <= 255) Then
                        newColor = Color.FromArgb(a, r, g, b)
                    End If
                Else
                    newColor = Color.FromName(htmlColor)
                End If
            End If
        End If

        ' Color formatting return
        If Not (IsNothing(ColorToCode(newColor, True))) Then
            reformattedChild = "$f" & ColorToCode(newColor, True) & "[" & Child.InnerHtml & "]"
        Else
            If aliasColors = True Then
                If Not IsNothing(AliasColor(newColor)) Then
                    reformattedChild = "^f" & Replace(AliasColor(newColor), "^", "") & "[" & Child.InnerHtml & "]"
                Else
                    reformattedChild = "#f" & Replace(ColortoHexCode(newColor), "#", "") & "[" & Child.InnerHtml & "]"
                End If
            Else
                reformattedChild = "#f" & Replace(ColortoHexCode(newColor), "#", "") & "[" & Child.InnerHtml & "]"
            End If
        End If
        Return reformattedChild

    End Function
    Function GetStyles(css As String)
        Dim styles As New Dictionary(Of String, String)
        Dim elementStyles = Split(css.ToUpper, ";")
        For Each Item In elementStyles
            Dim attribute = Split(Item, ":")
            If attribute.Count > 1 Then
                styles.Add(TrimWhite(attribute(0)), TrimWhite(attribute(1)))
            End If
        Next
        Return styles
    End Function

    Function AliasColor(c As Color) As String

        If ColorToCode(c, True) = Nothing Then ' checks to see if the color even needs to be aliased
            If bitLocked = False Then ' Lock the bits in the color map to save processing power
                Dim rect As Rectangle = New Rectangle(0, 0, colorMap.Height, colorMap.Width)
                colorMapBMData = My.Resources.am.LockBits(rect, Imaging.ImageLockMode.ReadOnly, colorMap.PixelFormat)
                Dim ptr As IntPtr = colorMapBMData.Scan0
                Dim bytes As Integer = Math.Abs(colorMapBMData.Stride) * colorMap.Height
                colorMapBytes = New Byte(bytes - 1) {}
                System.Runtime.InteropServices.Marshal.Copy(ptr, colorMapBytes, 0, bytes)
                bitLocked = True
            End If
            If previouslyAliased.ContainsKey(c) Then ' Checks to see if the dictionary of previously aliased colors already contains this color
                Return previouslyAliased(c)

            End If
            ' Set up color iterator
            Dim lastDiff = 999999999
            Dim bestDiff = 0
            Dim finalX = 0
            Dim finalY = 0
            Dim r As Integer = 0
            Dim g As Integer = 0
            Dim b As Integer = 0
            Dim baseOffset As Integer = 0
            Dim addOffset As Integer = 0
            For y = 0 To colorMap.Height - 1
                For x = 0 To colorMap.Width - 1
                    baseOffset = colorMapBMData.Stride * y
                    addOffset = x * 4
                    r = colorMapBytes(baseOffset + addOffset + 2)
                    g = colorMapBytes(baseOffset + addOffset + 1)
                    b = colorMapBytes(baseOffset + addOffset)
                    ' Get Euclidian color distance
                    Dim diff = Math.Floor((((r - c.R) ^ 2) + ((g - c.G) ^ 2) + ((b - c.B) ^ 2)) ^ 0.5)
                    If diff = 0 Then
                        bestDiff = diff
                        finalX = x
                        finalY = y
                        Exit For
                    End If
                    If diff <= lastDiff Then
                        bestDiff = diff
                        finalX = x
                        finalY = y
                        lastDiff = diff
                    End If
                Next
            Next
            Dim aliasedColorValue = "^" & baseString.Chars(finalX) & baseString.Chars(finalY)
            previouslyAliased.Add(c, aliasedColorValue)
            Return aliasedColorValue
        Else
            Return ColorToCode(c, True)
        End If
    End Function
    Function DealiasColor(aliasColor As String)
        Try
            If aliasColor.Length = 3 And aliasColor.Chars(0) = "^" Then
                Dim bmp As Bitmap = My.Resources.am
                Dim x = baseString.IndexOf(aliasColor.Chars(1))
                Dim y = baseString.IndexOf(aliasColor.Chars(2))
                Return bmp.GetPixel(x, y)
            Else
                If Regex.Match(aliasColor, "[A-F|a-f|0-9]\z").Captures.Count = 1 Then
                    If ColorDictionary.ContainsKey(Convert.ToInt32(aliasColor, 16)) Then
                        Return ColorDictionary(Convert.ToInt32(aliasColor, 16))
                    Else
                        Return Color.Black
                    End If
                Else
                    Return Color.Black
                End If
            End If
        Catch ex As Exception
            ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
        End Try
        Return Color.Black
    End Function
End Module
