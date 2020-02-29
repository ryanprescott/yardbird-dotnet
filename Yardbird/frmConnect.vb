Imports System.Text.RegularExpressions
Public Class frmConnect
    Dim localAddresses As String() = LocalIPs()
    Dim svdSrvDict As New Dictionary(Of String, Dictionary(Of String, String))
    Private Sub btnConnect_Click(sender As Object, e As EventArgs) Handles btnConnect.Click
        For Each Item As Control In Me.Controls
            Item.Enabled = False
        Next
        txtServer.Text = TrimWhite(txtServer.Text)
        txtPort.Text = TrimWhite(txtPort.Text)
        txtHandle.Text = TrimWhite(txtHandle.Text)
        If txtServer.TextLength = 0 Then
            MsgBox("You must enter a server address.", MsgBoxStyle.Critical, "Invalid Server Address")
            GoTo ReEnable
        End If
        If txtPort.TextLength = 0 Then
            MsgBox("You must enter a port.", MsgBoxStyle.Critical, "Invalid Port")
            GoTo ReEnable
        End If
        If txtHandle.TextLength = 0 Then
            MsgBox("You must enter a user handle.", MsgBoxStyle.Critical, "Invalid Handle")
            GoTo ReEnable
        End If
        If Regex.Match(txtPort.Text, "\A[0-9]{1,5}\z").Success = False Then
            MsgBox("The specified port is invalid." & Environment.NewLine & "Acceptable values are 1-65535.", MsgBoxStyle.Critical, "Invalid Port")
            GoTo ReEnable
        Else
            If CInt(txtPort.Text) > 65535 Or CInt(txtPort.Text) < 1 Then
                MsgBox("The specified port is invalid." & Environment.NewLine & "Acceptable values are 1-65535.", MsgBoxStyle.Critical, "Invalid Port")
                Exit Sub
            End If
        End If
        If IsCommonPort(CInt(txtPort.Text)) Then
            Dim r = MsgBox("The specified port is a well-known port. The server you are connecting to may not be a Yardbird server. Do you wish to continue?", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo, "Well-Known Port")
            If r = vbNo Then
                GoTo ReEnable
            End If
        End If
        Try
            Net.Dns.GetHostAddresses(txtServer.Text)
        Catch ex As Exception
            MsgBox("The specified host does not exist. This can happen for the following reasons:" & vbCrLf & vbCrLf &
                   "- The server address is incorrect. Please check that you have entered it correctly." & vbCrLf & vbCrLf &
                   "- You are not connected to the Internet. Please check your Internet connection settings." & vbCrLf & vbCrLf, MsgBoxStyle.Critical, "Invalid Server Address")
            GoTo ReEnable
        End Try
        If Regex.Match(txtHandle.Text, "\A[A-Z|a-z|0-9|\-|_]+\z").Success = False Then
            MsgBox("The specified user handle is invalid." & Environment.NewLine & "Acceptable characters are: A-Z, a-z, 0-9, -_", MsgBoxStyle.Critical, "Invalid Handle")
            GoTo ReEnable
        End If
        If txtPwd.TextLength > 0 And chkEncrypted.Checked = False Then
            MsgBox("You are about to send your password over an unencrypted connection. It is recommend to use encryption if you are signing in with a registered account. Are you sure you wish to do this?")
        End If
        If InitClient(NextAvailClientSlot(), txtServer.Text, txtPort.Text, txtHandle.Text, Math.Abs(CInt(chkEncrypted.Checked))) = True Then
            Me.Hide()
        Else
            MsgBox("The server at " & txtServer.Text & " could not be contacted. This can happen for the following reasons:" & vbCrLf & vbCrLf &
                   "- The server address and/or port is incorrect. Please check that you have entered them correctly." & vbCrLf & vbCrLf &
                   "- You are not connected to the Internet. Please check your Internet connection settings." & vbCrLf & vbCrLf &
                   "- There is no Yardbird server at the specified address.", MsgBoxStyle.Critical, "Could Not Connect")
        End If
ReEnable:
        For Each Item As Control In Me.Controls
            Item.Enabled = True
        Next
    End Sub

    Sub LoadSvrsIntoList()
        cliSettings.Reload()
        svdSrvDict.Clear()
        Dim serverInfoPattern As String = "\A{([A-Z|a-z|0-9|_|-|.|,|\s]+)}\z"
        For Each Item As KeyValuePair(Of String, Object) In cliSettings.GetSection("SavedServers")
            Dim rm = Regex.Match(Item.Value.ToString, serverInfoPattern)
            If rm.Success = True Then
                Dim splitter = Split(rm.Groups(1).Captures(0).Value, ",")
                Try
                    svdSrvDict.Add(splitter(0), New Dictionary(Of String, String) From {{"key", Item.Key}, {"ipAddress", splitter(1)}, {"port", splitter(2)}, {"username", splitter(3)}, {"password", splitter(4)}, {"encrypted", splitter(5)}})
                Catch
                End Try
            End If
        Next
        svdSrvDict.Add("+ New Server...", New Dictionary(Of String, String) From {{"key", ""}, {"ipAddress", ""}, {"port", 49620}, {"username", ""}, {"password", ""}, {"encrypted", True}})
    End Sub
    Private Sub frmConnect_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        LoadSvrsIntoList()
        If svdSrvDict.Count > 0 Then
            With lstSaved
                .DataSource = New BindingSource(svdSrvDict, Nothing)
                .DisplayMember = "Key"
                .ValueMember = "Value"
            End With
        End If
        PopulateTextboxes()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Private Sub TableLayoutPanel1_Paint(sender As Object, e As PaintEventArgs)

    End Sub

    Private Sub txtServer_TextChanged(sender As Object, e As EventArgs)

    End Sub


    Private Sub frmConnect_Load(sender As Object, e As EventArgs) Handles MyBase.Load


    End Sub
    Sub PopulateTextboxes()
        If Not IsNothing(TryCast(lstSaved.SelectedValue, Dictionary(Of String, String))) Then
            Dim dict As Dictionary(Of String, String) = lstSaved.SelectedValue
            cliSettings.SetKey("SavedServers", "selected", dict("key"))
            txtServer.Text = dict("ipAddress")
            txtPort.Text = dict("port")
            txtHandle.Text = dict("username")
            txtPwd.Text = dict("password")
            chkEncrypted.Checked = CBool(dict("encrypted"))
        End If
    End Sub
    Private Sub lstSaved_DoubleClick(sender As Object, e As EventArgs) Handles lstSaved.DoubleClick

    End Sub

    Private Sub txtServer_TextChanged_1(sender As Object, e As EventArgs) Handles txtServer.TextChanged

    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        For Each Item As Control In Me.Controls
            Item.Enabled = False
        Next
        txtServer.Text = TrimWhite(txtServer.Text)
        txtPort.Text = TrimWhite(txtPort.Text)
        txtHandle.Text = TrimWhite(txtHandle.Text)
        If txtServer.TextLength = 0 Then
            MsgBox("You must enter a server address.", MsgBoxStyle.Critical, "Invalid Server Address")
            GoTo ReEnable
        End If
        If txtPort.TextLength = 0 Then
            MsgBox("You must enter a port.", MsgBoxStyle.Critical, "Invalid Port")
            GoTo ReEnable
        End If
        If Regex.Match(txtPort.Text, "\A[0-9]{1,5}\z").Success = False Then
            MsgBox("The specified port is invalid." & Environment.NewLine & "Acceptable values are 1-65535.", MsgBoxStyle.Critical, "Invalid Port")
            GoTo ReEnable
        Else
            If CInt(txtPort.Text) > 65535 Or CInt(txtPort.Text) < 1 Then
                MsgBox("The specified port is invalid." & Environment.NewLine & "Acceptable values are 1-65535.", MsgBoxStyle.Critical, "Invalid Port")
                GoTo ReEnable
            End If
        End If
        If IsCommonPort(CInt(txtPort.Text)) Then
            Dim r = MsgBox("The specified port is a well-known port. The server you are connecting to may not be a Yardbird server. Do you wish to continue?", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo, "Well-Known Port")
            If r = vbNo Then
                GoTo ReEnable
            End If
        End If
        Try
            Net.Dns.GetHostAddresses(txtServer.Text)
        Catch ex As Exception
            MsgBox("The specified host does not exist. This can happen for the following reasons:" & vbCrLf & vbCrLf &
                   "- The server address is incorrect. Please check that you have entered it correctly." & vbCrLf & vbCrLf &
                   "- You are not connected to the Internet. Please check your Internet connection settings." & vbCrLf & vbCrLf, MsgBoxStyle.Critical, "Invalid Server Address")
            GoTo ReEnable
        End Try
        If Regex.Match(txtHandle.Text, "\A[A-Z|a-z|0-9|\-|_]+\z").Success = False And txtHandle.Text <> "" Then
            MsgBox("The specified user handle is invalid." & Environment.NewLine & "Acceptable characters are: A-Z, a-z, 0-9, -_", MsgBoxStyle.Critical, "Invalid Handle")
            GoTo ReEnable
        End If
        Dim nextNumber = CInt(cliSettings.GetSection("SavedServers").Keys.Last.Substring(cliSettings.GetSection("SavedServers").Keys.Last.Length - 1, 1)) + 1
        If Not IsNothing(TryCast(lstSaved.SelectedValue, Dictionary(Of String, String))) Then
            Dim dict As Dictionary(Of String, String) = lstSaved.SelectedValue
            If dict("key") = "" Then
                If IsNothing(cliSettings.GetKey("SavedServer", "s" & nextNumber)) Then
                    Dim serverName As String = InputBox("Enter a name for the server.", "Server Name", "New Server")
                    Dim params As New List(Of String) From {serverName, txtServer.Text, txtPort.Text, txtHandle.Text, txtPwd.Text, chkEncrypted.Checked.ToString}
                    Dim svrString = "{" & String.Join(",", params) & "}"
                    If cliSettings.AddKey("SavedServers", "s" & nextNumber, svrString) = True Then
                        cliSettings.WriteOut()
                        LoadSvrsIntoList()
                        If svdSrvDict.Count > 0 Then
                            With lstSaved
                                .DataSource = New BindingSource(svdSrvDict, Nothing)
                                .DisplayMember = "Key"
                                .ValueMember = "Value"
                            End With
                        End If
                    Else
                        MsgBox("Error saving server settings: AddKey returned False.")
                    End If
                Else
                    MsgBox("Error saving server settings: key is not unique")
                End If
            Else
                Dim params As New List(Of String) From {lstSaved.SelectedItem.Key, txtServer.Text, txtPort.Text, txtHandle.Text, txtPwd.Text, chkEncrypted.Checked.ToString}
                Dim svrString = "{" & String.Join(",", params) & "}"
                cliSettings.SetKey("SavedServers", dict("key"), svrString)
                cliSettings.WriteOut()
                LoadSvrsIntoList()
                If svdSrvDict.Count > 0 Then
                    With lstSaved
                        .DataSource = New BindingSource(svdSrvDict, Nothing)
                        .DisplayMember = "Key"
                        .ValueMember = "Value"
                    End With
                End If
            End If
        Else
                MsgBox("Error saving server settings: could not cast item to dictionary.")
        End If
ReEnable:
        For Each Item As Control In Me.Controls
            Item.Enabled = True
        Next
    End Sub

    Private Sub lstSaved_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstSaved.SelectedIndexChanged
        PopulateTextboxes()
    End Sub
End Class