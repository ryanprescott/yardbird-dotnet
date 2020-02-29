Imports System.ComponentModel
Public Class frmMain
    Dim LastGoodbye = False
    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            CreateYbClientDB()
            Dim cliIniPath = IO.Path.Combine(Application.StartupPath, "client", "config.ini")
            If IO.File.Exists(cliIniPath) Then
                cliSettings = New Ini(cliIniPath)
            Else
                Throw New Exception("Client configuration file could not be found.")
            End If
            Dim cw = CreateNewChatWindow(0)
            If Not IsNothing(cw) Then
                If LoadTheme(cliSettings.GetKey("General", "theme", "default"), cw, True, False) = False Then
                    Throw New SystemException("The default theme could not be loaded. It may be corrupted or non-existent.")
                Else
                    While Not cw.ReadyState = WebBrowserReadyState.Complete
                        Application.DoEvents()
                    End While
                End If
                tvwServers.SelectedNode = tvwServers.Nodes.Add(0, "Console")
                AddConLine("Welcome to " & My.Application.Info.AssemblyName & " " & My.Application.Info.Version.ToString & "!  Join a server, or start your own!", " information")
            Else
                Throw New SystemException("The console window could not be initialized.")
            End If
        Catch ex As Exception
            MsgBox("Fatal error on startup: " & ex.Message & vbCrLf & vbCrLf & ex.StackTrace, MsgBoxStyle.Critical)
            End
        End Try
    End Sub

    Public Sub PopulatePeersInList(sockID)
        lstPeers.Items.Clear()
        For Each Item In GetCliConnectedPeers(cliDb, sockID)
            lstPeers.Items.Add(Item("handle"))
        Next
    End Sub

    Sub ChangeStatusText(socket As Integer, value As String)
        GetSetCliIdentityAttribute(cliDb, socket, "statusText", value)
        If GetSetActiveClient() = socket Then
            lblNetStatus.Text = value
        End If
    End Sub

    Sub SaveMessageText(socket As Integer, value As String)
        GetSetCliIdentityAttribute(cliDb, socket, "messageText", value)
    End Sub

    Sub ShowClientSavedState(socket As Integer)
        Dim st = GetSetCliIdentityAttribute(cliDb, socket, "statusText")
        Dim mt = GetSetCliIdentityAttribute(cliDb, socket, "messageText")
        Dim io = GetSetCliIdentityAttribute(cliDb, socket, "isOnline")
        If Not IsNothing(io) Then
            If io = 1 Then
                txtMessage.Enabled = True
                btnFmt.Enabled = True
            Else
                SaveMessageText(socket, "")
                txtMessage.Enabled = False
                btnFmt.Enabled = False
            End If
        Else
            SaveMessageText(socket, "")
            txtMessage.Enabled = False
            btnFmt.Enabled = False
        End If
        If Not IsNothing(st) Then
            lblNetStatus.Text = st
        Else
            lblNetStatus.Text = ""
        End If
        If Not IsNothing(mt) Then
            txtMessage.Text = mt
        Else
            txtMessage.Text = ""
        End If
    End Sub

    Private Sub txtMessage_TextChanged(sender As Object, e As EventArgs) Handles txtMessage.TextChanged
        If txtMessage.Text.Length > 1 Then
            If txtMessage.Text.Substring(0, 2) = ">>" Then
                txtMessage.MaxLength = 0
                lblCharLmt.ForeColor = Color.ForestGreen
                lblCharLmt.Text = "CMD MODE"
                btnFmt.Enabled = False
            Else
                btnFmt.Enabled = True
                txtMessage.MaxLength = 500
                lblCharLmt.Text = txtMessage.Text.Length & "/500"
                lblCharLmt.ForeColor = Color.Black
            End If
        Else
            btnFmt.Enabled = True
            txtMessage.MaxLength = 500
            lblCharLmt.Text = txtMessage.Text.Length & "/500"
        End If
        If txtMessage.Text.Length = 500 And txtMessage.MaxLength = 500 Then
            lblCharLmt.ForeColor = Color.Red
        Else
            lblCharLmt.ForeColor = Color.Black
        End If
        SaveMessageText(GetSetActiveClient, txtMessage.Text)
    End Sub

    Private Sub txtMessage_KeyDown(sender As Object, e As KeyEventArgs) Handles txtMessage.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            If txtMessage.Text.Length > 0 Then
                If txtMessage.Text.Length > 1 Then

                    If txtMessage.Text.Substring(0, 2) = ">>" Then
                        WriteClientData(GetSetActiveClient, txtMessage.Text.Substring(2, txtMessage.Text.Length - 2))
                        txtMessage.Text = ""
                    Else
                        SendMessage(GetSetActiveClient, Replace(txtMessage.Text, "vbCrLf", ""))
                        txtMessage.Text = ""
                    End If
                Else
                    SendMessage(GetSetActiveClient, Replace(txtMessage.Text, "vbCrLf", ""))
                    txtMessage.Text = ""
                End If
            Else
                e.SuppressKeyPress = True
                Beep()
            End If
        End If
    End Sub


    Private Sub frmMain_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If ListenerActive() Then
            Dim areusure = MsgBox("Are you sure you want to quit Yardbird? This will kick all clients connected to your server, as well as disconnecting you from all servers you're connected to.", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo, "Exit Yardbird")
            If areusure = vbYes Then
                For Each Item In GetConnectedIdentities(cliDb)
                    WriteClientData(Item, "QUIT•Disconnecting from server", False)
                Next
                KickServerPeers("The server is shutting down.")
                SrvWaitThenDie()
                DeinitListener()
                LastGoodbye = True
            End If
        ElseIf GetConnectedIdentities(cliDb).Count > 0 Then
            Dim areusure = MsgBox("Are you sure you want to quit Yardbird? This will disconnect you from all servers you're connected to.", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo, "Exit Yardbird")
            If areusure = vbYes Then
                For Each Item In GetConnectedIdentities(cliDb)
                    WriteClientData(Item, "QUIT•Disconnecting from server", False)
                    DeinitClient(Item)
                Next
                LastGoodbye = True
                End
            End If
        Else
            LastGoodbye = True
            End
        End If
        e.Cancel = Not LastGoodbye
    End Sub



    Private Sub LoadThemeToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LoadThemeToolStripMenuItem.Click
        Dim themeName = InputBox("Please enter the theme name.", "default")
        If themeName <> "" Then LoadTheme(themeName, GetActiveChatWindow(), False, True)
    End Sub

    Private Sub RunGarbageCollectionToolStripMenuItem_Click(sender As Object, e As EventArgs)
        GarbageCollect()
    End Sub


    Private Sub btnFmt_Click(sender As Object, e As EventArgs) Handles btnFmt.Click
        If txtMessage.SelectionLength > 0 Then
            Dim formatSubstring = txtMessage.Text.Substring(txtMessage.SelectionStart, txtMessage.SelectionLength)
            txtMessage.Text = Replace(txtMessage.Text, formatSubstring, FormatDlg(formatSubstring, txtMessage.SelectionStart, txtMessage.SelectionLength))
        Else
            txtMessage.Text = FormatDlg(txtMessage.Text)
        End If
        txtMessage.Select()
    End Sub


    Private Sub AboutYardbirdToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutYardbirdToolStripMenuItem.Click
        frmAbout.ShowDialog()
    End Sub

    Private Sub tvwServers_ControlRemoved(sender As Object, e As ControlEventArgs) Handles tvwServers.ControlRemoved
        tvwServers.SelectedNode = tvwServers.Nodes(tvwServers.Nodes.Count - 1)
    End Sub


    Private Sub tvwServers_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles tvwServers.AfterSelect
        Dim regex = System.Text.RegularExpressions.Regex.Match(e.Node.Name, "\A\(([0-9]+)#([A-Z|a-z|0-9|-|_]+)\)\z")
        If regex.Groups.Count > 1 Then
            GetSetActiveClient(regex.Groups(1).Captures(0).Value)
            'GetSetActiveRoom(regex.Groups(2).Captures(0).Value)
            ActivateChatWindow(GetSetActiveClient())
        Else
            GetSetActiveClient(e.Node.Name)
            'GetSetActiveRoom("#lobby")
            ActivateChatWindow(GetSetActiveClient())
        End If
    End Sub

    Private Sub SearchForPeersToolStripMenuItem_Click(sender As Object, e As EventArgs)
        MsgBox("Not yet implemented")
    End Sub

    Private Sub ConnectToolStripMenuItem_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub ConnectToolStripMenuItem_MouseDown(sender As Object, e As MouseEventArgs)

    End Sub

    Private Sub LoadThemeToolStripMenuItem_Click_1(sender As Object, e As EventArgs)

    End Sub

    Private Sub HelpToolStripMenuItem_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub MenuStrip1_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs) Handles MenuStrip1.ItemClicked

    End Sub

    Private Sub StatusStrip1_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs) Handles StatusStrip1.ItemClicked

    End Sub

    Private Sub tvwServers_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles tvwServers.NodeMouseClick
        If e.Button = MouseButtons.Right Then
            tvwServers.SelectedNode = e.Node
        End If
    End Sub

    Private Sub ConnectToServerToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ConnectToServerToolStripMenuItem.Click
        frmConnect.Owner = Me
        frmConnect.ShowDialog()
    End Sub

    Private Sub ControlPanelToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ControlPanelToolStripMenuItem.Click
        frmServer.Owner = Me
        frmServer.ShowDialog()
    End Sub

    Private Sub frmMain_ResizeEnd(sender As Object, e As EventArgs) Handles Me.ResizeEnd
        GetActiveChatWindow().Document.Body.Children.Item(GetActiveChatWindow().Document.Body.Children.Count - 1).ScrollIntoView(False)
    End Sub

    Private Sub ToHTMLToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ToHTMLToolStripMenuItem.Click
        Dim fd As New SaveFileDialog
        fd.CheckPathExists = True
        fd.Filter = "HTML Web Page|*.htm, *.html"
        Dim result As DialogResult = fd.ShowDialog
        If result <> DialogResult.Cancel Then
            If fd.FileName <> "" Then
                IO.File.WriteAllText(fd.FileName, GetActiveChatWindow().Document.GetElementsByTagName("HTML")(0).OuterHtml)
            End If
        End If
    End Sub

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click
        Dim pp As New PuzzleProvider
        PictureBox1.Image = pp.Image
    End Sub
End Class
