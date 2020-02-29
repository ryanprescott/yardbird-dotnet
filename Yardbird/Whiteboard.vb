Public Class Whiteboard
    Dim secondPoint = True
    Dim wbGraphics As Graphics
    Private Sub PictureBox1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseMove
        If e.Button = MouseButtons.Left Then
            PictureBox1.
        End If
    End Sub

    Private Sub Whiteboard_Load(sender As Object, e As EventArgs) Handles Me.Load
        wbGraphics = Graphics.FromImage(PictureBox1.Image)
    End Sub
End Class