Imports System.Text.RegularExpressions
Public Class frmServer
    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnEdit.Click
        Process.Start(IO.Path.Combine(Application.StartupPath, "server", "config.ini"))
    End Sub

    Private Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        btnStart.Enabled = False
        btnStop.Enabled = False
        btnRestart.Enabled = False
        Dim supposedIniPath = IO.Path.Combine(Application.StartupPath, "server", "config.ini")
        If Not IO.File.Exists(supposedIniPath) Then
            IO.File.WriteAllText(supposedIniPath, My.Resources.config)
        End If
        srvSettings = New Ini(IO.Path.Combine(Application.StartupPath, "server", "config.ini"))
        Dim tmpIP As String = srvSettings.GetKey("Listener", "IPAddress")
        Dim tmpPort As Integer = srvSettings.GetKey("Listener", "Port")
        Dim tmpName As String = srvSettings.GetKey("General", "Name")
        Dim tmpMotd As String = srvSettings.GetKey("General", "MOTD")
        If InitListener(Net.IPAddress.Parse(tmpIP), tmpPort, tmpName, tmpMotd) = True Then
            btnStart.Enabled = False
            btnStop.Enabled = True
            btnRestart.Enabled = True
        End If
    End Sub

    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        btnStart.Enabled = False
        btnStop.Enabled = False
        btnRestart.Enabled = False
        KickServerPeers("The server is shutting down.")
        SrvWaitThenDie()
        DeinitListener()
        btnStart.Enabled = True
        btnStop.Enabled = False
        btnRestart.Enabled = False
    End Sub

    Private Sub btnReload_Click(sender As Object, e As EventArgs)
        srvSettings.Reload()
    End Sub

    Private Sub btnRestart_Click(sender As Object, e As EventArgs) Handles btnRestart.Click
        btnStart.Enabled = False
        btnStop.Enabled = False
        btnRestart.Enabled = False
        KickServerPeers("The server is restarting.")
        SrvWaitThenDie()
        srvSettings.Reload()
        Sleep(500)
        Dim tmpIP As String = srvSettings.GetKey("Listener", "IPAddress")
        Dim tmpPort As Integer = srvSettings.GetKey("Listener", "Port")
        Dim tmpName As String = srvSettings.GetKey("General", "Name")
        Dim tmpMotd As String = srvSettings.GetKey("General", "MOTD")
        InitListener(Net.IPAddress.Parse(tmpIP), tmpPort, tmpName, tmpMotd)
        btnStart.Enabled = False
        btnStop.Enabled = True
        btnRestart.Enabled = True
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs)

    End Sub
End Class