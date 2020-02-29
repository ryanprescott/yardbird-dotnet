Imports System.Windows.Forms
Imports System.Text.RegularExpressions

Public Class dlgNewHndl
    Public dlgText = "The handle you have selected is already taken. Please specify a new handle."
    Public hndlText = ""
    Dim ticker As New Timer
    Dim interval As Integer = 40
    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        If Regex.Match(txtHandle.Text, "\A[A-Z|a-z|0-9|\-|_]+\z").Success = False Or txtHandle.TextLength > 20 Then
            MsgBox("The specified user handle is invalid." & Environment.NewLine & "Acceptable characters are: A-Z, a-z, 0-9, -_" & Environment.NewLine & "Your handle cannot be more than 20 characters.", MsgBoxStyle.Critical, "Invalid Handle")
        Else
            hndlText = txtHandle.Text
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            ticker.Enabled = False
            Me.Close()
        End If
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        ticker.Enabled = False
        Me.Close()
    End Sub

    Public Sub GracefulBye()
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub dlgNewHndl_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblDlgText.Text = dlgText & " (" & interval & "s)"
        txtHandle.Text = hndlText
        ticker.Interval = 1000
        ticker.Enabled = True
        AddHandler ticker.Tick, AddressOf Decrement
    End Sub

    Sub Decrement()
        interval += -1
        lblDlgText.Text = dlgText & " (" & interval & "s)"
        If interval = 0 Then
            ticker.Enabled = False
            Me.Close()
        End If

    End Sub

    Private Sub txtHandle_TextChanged(sender As Object, e As EventArgs) Handles txtHandle.TextChanged

    End Sub

End Class
