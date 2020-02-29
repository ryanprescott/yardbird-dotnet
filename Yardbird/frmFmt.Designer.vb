<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmFmt
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmFmt))
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.htmlText = New System.Windows.Forms.WebBrowser()
        Me.btnBold = New System.Windows.Forms.ToolStripButton()
        Me.btnItalic = New System.Windows.Forms.ToolStripButton()
        Me.btnUnder = New System.Windows.Forms.ToolStripButton()
        Me.btnFontColor = New System.Windows.Forms.ToolStripDropDownButton()
        Me.cbxColorPick = New System.Windows.Forms.ToolStripComboBox()
        Me.btnFontSize = New System.Windows.Forms.ToolStripDropDownButton()
        Me.ToolStripComboBox1 = New System.Windows.Forms.ToolStripComboBox()
        Me.btnSave = New System.Windows.Forms.ToolStripButton()
        Me.btnCancel = New System.Windows.Forms.ToolStripButton()
        Me.SettingsToolStripButton = New System.Windows.Forms.ToolStripDropDownButton()
        Me.AliasColorsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ToolStrip1
        '
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.btnBold, Me.btnItalic, Me.btnUnder, Me.btnFontColor, Me.btnFontSize, Me.btnSave, Me.btnCancel, Me.SettingsToolStripButton})
        Me.ToolStrip1.Location = New System.Drawing.Point(0, 0)
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.Size = New System.Drawing.Size(674, 25)
        Me.ToolStrip1.TabIndex = 2
        Me.ToolStrip1.Text = "ToolStrip1"
        '
        'htmlText
        '
        Me.htmlText.Dock = System.Windows.Forms.DockStyle.Fill
        Me.htmlText.Location = New System.Drawing.Point(0, 25)
        Me.htmlText.MinimumSize = New System.Drawing.Size(20, 20)
        Me.htmlText.Name = "htmlText"
        Me.htmlText.Size = New System.Drawing.Size(674, 90)
        Me.htmlText.TabIndex = 3
        '
        'btnBold
        '
        Me.btnBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.btnBold.Image = Global.Yardbird.My.Resources.Resources.Bold_11689_32
        Me.btnBold.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.btnBold.Name = "btnBold"
        Me.btnBold.Size = New System.Drawing.Size(23, 22)
        Me.btnBold.Text = "ToolStripButton1"
        '
        'btnItalic
        '
        Me.btnItalic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.btnItalic.Image = Global.Yardbird.My.Resources.Resources.Italic_11693_32
        Me.btnItalic.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.btnItalic.Name = "btnItalic"
        Me.btnItalic.Size = New System.Drawing.Size(23, 22)
        Me.btnItalic.Text = "ToolStripButton2"
        '
        'btnUnder
        '
        Me.btnUnder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.btnUnder.Image = Global.Yardbird.My.Resources.Resources.Underline_11700_32
        Me.btnUnder.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.btnUnder.Name = "btnUnder"
        Me.btnUnder.Size = New System.Drawing.Size(23, 22)
        Me.btnUnder.Text = "ToolStripButton3"
        '
        'btnFontColor
        '
        Me.btnFontColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.btnFontColor.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.cbxColorPick})
        Me.btnFontColor.Image = Global.Yardbird.My.Resources.Resources.FontColor_11146_24
        Me.btnFontColor.ImageTransparentColor = System.Drawing.Color.White
        Me.btnFontColor.Name = "btnFontColor"
        Me.btnFontColor.Size = New System.Drawing.Size(29, 22)
        Me.btnFontColor.Text = "ToolStripDropDownButton1"
        '
        'cbxColorPick
        '
        Me.cbxColorPick.Name = "cbxColorPick"
        Me.cbxColorPick.Size = New System.Drawing.Size(121, 23)
        '
        'btnFontSize
        '
        Me.btnFontSize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.btnFontSize.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripComboBox1})
        Me.btnFontSize.Image = Global.Yardbird.My.Resources.Resources.FontSize_5701_32
        Me.btnFontSize.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.btnFontSize.Name = "btnFontSize"
        Me.btnFontSize.Size = New System.Drawing.Size(29, 22)
        Me.btnFontSize.Text = "ToolStripDropDownButton2"
        '
        'ToolStripComboBox1
        '
        Me.ToolStripComboBox1.Name = "ToolStripComboBox1"
        Me.ToolStripComboBox1.Size = New System.Drawing.Size(121, 23)
        '
        'btnSave
        '
        Me.btnSave.Image = Global.Yardbird.My.Resources.Resources.CheckMark_6909_24
        Me.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(51, 22)
        Me.btnSave.Text = "Save"
        '
        'btnCancel
        '
        Me.btnCancel.Image = Global.Yardbird.My.Resources.Resources.action_Cancel_16xMD
        Me.btnCancel.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(63, 22)
        Me.btnCancel.Text = "Cancel"
        '
        'SettingsToolStripButton
        '
        Me.SettingsToolStripButton.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AliasColorsToolStripMenuItem})
        Me.SettingsToolStripButton.Image = CType(resources.GetObject("SettingsToolStripButton.Image"), System.Drawing.Image)
        Me.SettingsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.SettingsToolStripButton.Name = "SettingsToolStripButton"
        Me.SettingsToolStripButton.Size = New System.Drawing.Size(78, 22)
        Me.SettingsToolStripButton.Text = "Settings"
        '
        'AliasColorsToolStripMenuItem
        '
        Me.AliasColorsToolStripMenuItem.Checked = True
        Me.AliasColorsToolStripMenuItem.CheckOnClick = True
        Me.AliasColorsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.AliasColorsToolStripMenuItem.Name = "AliasColorsToolStripMenuItem"
        Me.AliasColorsToolStripMenuItem.Size = New System.Drawing.Size(164, 22)
        Me.AliasColorsToolStripMenuItem.Text = "Compress Colors"
        Me.AliasColorsToolStripMenuItem.ToolTipText = "Saves space in your message by approximating colors instead of using the full hex" &
    " value."
        '
        'frmFmt
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(674, 115)
        Me.Controls.Add(Me.htmlText)
        Me.Controls.Add(Me.ToolStrip1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Name = "frmFmt"
        Me.Text = "Format Text"
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ToolStrip1 As ToolStrip
    Friend WithEvents btnBold As ToolStripButton
    Friend WithEvents btnItalic As ToolStripButton
    Friend WithEvents btnUnder As ToolStripButton
    Friend WithEvents btnFontColor As ToolStripDropDownButton
    Friend WithEvents btnFontSize As ToolStripDropDownButton
    Friend WithEvents btnSave As ToolStripButton
    Friend WithEvents btnCancel As ToolStripButton
    Friend WithEvents cbxColorPick As ToolStripComboBox
    Friend WithEvents ToolStripComboBox1 As ToolStripComboBox
    Friend WithEvents htmlText As WebBrowser
    Friend WithEvents SettingsToolStripButton As ToolStripDropDownButton
    Friend WithEvents AliasColorsToolStripMenuItem As ToolStripMenuItem
End Class
