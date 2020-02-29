Imports System.ComponentModel
Imports System.Text.RegularExpressions

Public Class frmFmt

    Public textToEdit As String
    Public editedText As String
    Dim saved As Boolean = False
    Private Sub frmFmt_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim colorPal As New ToolStripDropDown
        htmlText.Navigate("about:blank")
        For Each Item As KeyValuePair(Of Integer, Color) In ColorDictionary
            Dim colorName As String = Item.Value.Name
            Dim colorMenuItem As New ToolStripMenuItem
            Dim tmpClr As Color = Item.Value
            Dim tmpBmp As New Bitmap(16, 16)
            Dim tmpGfx As Graphics = Graphics.FromImage(tmpBmp)
            Dim tmpBrush = New SolidBrush(Color.Black)
            Dim tmpPen = New Pen(tmpBrush, 1)
            tmpGfx.Clear(tmpClr)
            tmpGfx.DrawRectangle(tmpPen, 0, 0, 15, 15)
            tmpGfx.Dispose()
            colorMenuItem.Image = tmpBmp
            colorMenuItem.Text = colorName
            colorMenuItem.DisplayStyle = ToolStripItemDisplayStyle.Image
            colorMenuItem.Name = "btn" & colorName
            colorMenuItem.ToolTipText = colorName
            AddHandler colorMenuItem.Click, AddressOf ColorPicked
            colorPal.Items.Add(colorMenuItem)
        Next
        Dim moreMenuItem As New ToolStripMenuItem
        moreMenuItem.Text = "..."
        moreMenuItem.DisplayStyle = ToolStripItemDisplayStyle.Text
        moreMenuItem.Name = "btnMore"
        moreMenuItem.ToolTipText = "More..."
        AddHandler moreMenuItem.Click, AddressOf ColorPicked
        colorPal.Items.Add(moreMenuItem)
        btnFontColor.DropDown = colorPal
    End Sub

    Sub ColorPicked(sender As ToolStripMenuItem, e As EventArgs)
        Try
            Dim tmpClr As Color
            If sender.Name = "btnMore" Then
                Dim clrDialog As New ColorDialog
                clrDialog.ShowDialog()
                tmpClr = clrDialog.Color
            Else
                tmpClr = Color.FromName(Replace(sender.Name, "btn", ""))
            End If
            Dim hexCode As String = "#" & tmpClr.R.ToString("X2") & tmpClr.G.ToString("X2") & tmpClr.B.ToString("X2")
            htmlText.Document.ExecCommand("ForeColor", False, hexCode)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub htmlText_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles htmlText.DocumentCompleted
        htmlText.Document.Body.Style = "font-family: 'Segoe UI Emoji'; font-size: 9pt;"
        htmlText.Document.Body.InnerHtml = "<P>" & FormatSubstitute(textToEdit, False) & "</P>"
        htmlText.Document.Body.SetAttribute("contenteditable", "true")
    End Sub

    Private Sub btnBold_Click(sender As Object, e As EventArgs) Handles btnBold.Click
        htmlText.Document.ExecCommand("Bold", False, Nothing)
    End Sub
    Private Sub btnItalic_Click(sender As Object, e As EventArgs) Handles btnItalic.Click
        htmlText.Document.ExecCommand("Italic", False, Nothing)
    End Sub
    Private Sub btnUnder_Click(sender As Object, e As EventArgs) Handles btnUnder.Click
        htmlText.Document.ExecCommand("Underline", False, Nothing)
    End Sub
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        saved = True
        Me.Close()
    End Sub

    Private Sub SettingsToolStripButton_Click(sender As Object, e As EventArgs) Handles SettingsToolStripButton.Click

    End Sub

    Private Sub btnFontColor_Click(sender As Object, e As EventArgs) Handles btnFontColor.Click

    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        saved = False
        Me.Close()
    End Sub

    Private Sub frmFmt_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If saved = True Then
            editedText = HTMLtoYardbird(htmlText.Document.Body, AliasColorsToolStripMenuItem.Checked)
        Else
            editedText = textToEdit
        End If
    End Sub
End Class