<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.lblNetStatus = New System.Windows.Forms.ToolStripStatusLabel()
        Me.lblCharLmt = New System.Windows.Forms.ToolStripStatusLabel()
        Me.SplitContainer3 = New System.Windows.Forms.SplitContainer()
        Me.txtMessage = New System.Windows.Forms.TextBox()
        Me.btnFmt = New System.Windows.Forms.Button()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.SplitContainer2 = New System.Windows.Forms.SplitContainer()
        Me.tvwServers = New System.Windows.Forms.TreeView()
        Me.SplitContainer4 = New System.Windows.Forms.SplitContainer()
        Me.lstPeers = New System.Windows.Forms.ListBox()
        Me.LoadThemeToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HelpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AboutYardbirdToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExportChatLogToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToHTMLToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.NetworkToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ClientToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ConnectToServerToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.Server1ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ControlPanelToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.StatusStrip1.SuspendLayout()
        CType(Me.SplitContainer3, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer3.Panel1.SuspendLayout()
        Me.SplitContainer3.Panel2.SuspendLayout()
        Me.SplitContainer3.SuspendLayout()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer2.Panel1.SuspendLayout()
        Me.SplitContainer2.Panel2.SuspendLayout()
        Me.SplitContainer2.SuspendLayout()
        CType(Me.SplitContainer4, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer4.Panel2.SuspendLayout()
        Me.SplitContainer4.SuspendLayout()
        Me.MenuStrip1.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'StatusStrip1
        '
        Me.StatusStrip1.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.lblNetStatus, Me.lblCharLmt})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 535)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(784, 26)
        Me.StatusStrip1.TabIndex = 10
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'lblNetStatus
        '
        Me.lblNetStatus.Name = "lblNetStatus"
        Me.lblNetStatus.Size = New System.Drawing.Size(43, 21)
        Me.lblNetStatus.Text = "Offline"
        '
        'lblCharLmt
        '
        Me.lblCharLmt.AutoSize = False
        Me.lblCharLmt.Name = "lblCharLmt"
        Me.lblCharLmt.Size = New System.Drawing.Size(726, 21)
        Me.lblCharLmt.Spring = True
        Me.lblCharLmt.Text = "0/500"
        Me.lblCharLmt.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'SplitContainer3
        '
        Me.SplitContainer3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.SplitContainer3.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer3.Name = "SplitContainer3"
        '
        'SplitContainer3.Panel1
        '
        Me.SplitContainer3.Panel1.Controls.Add(Me.txtMessage)
        '
        'SplitContainer3.Panel2
        '
        Me.SplitContainer3.Panel2.Controls.Add(Me.btnFmt)
        Me.SplitContainer3.Size = New System.Drawing.Size(784, 25)
        Me.SplitContainer3.SplitterDistance = 752
        Me.SplitContainer3.TabIndex = 13
        '
        'txtMessage
        '
        Me.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtMessage.Enabled = False
        Me.txtMessage.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtMessage.Location = New System.Drawing.Point(0, 0)
        Me.txtMessage.Name = "txtMessage"
        Me.txtMessage.Size = New System.Drawing.Size(752, 23)
        Me.txtMessage.TabIndex = 6
        '
        'btnFmt
        '
        Me.btnFmt.Enabled = False
        Me.btnFmt.Image = Global.Yardbird.My.Resources.Resources.FontDialogControl_679_24
        Me.btnFmt.Location = New System.Drawing.Point(1, 1)
        Me.btnFmt.Name = "btnFmt"
        Me.btnFmt.Size = New System.Drawing.Size(26, 23)
        Me.btnFmt.TabIndex = 9
        Me.btnFmt.Text = "..."
        Me.btnFmt.UseVisualStyleBackColor = True
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.SplitContainer1.IsSplitterFixed = True
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 24)
        Me.SplitContainer1.Name = "SplitContainer1"
        Me.SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.SplitContainer2)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.SplitContainer3)
        Me.SplitContainer1.Size = New System.Drawing.Size(784, 511)
        Me.SplitContainer1.SplitterDistance = 482
        Me.SplitContainer1.TabIndex = 13
        '
        'SplitContainer2
        '
        Me.SplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.SplitContainer2.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer2.Name = "SplitContainer2"
        '
        'SplitContainer2.Panel1
        '
        Me.SplitContainer2.Panel1.Controls.Add(Me.tvwServers)
        '
        'SplitContainer2.Panel2
        '
        Me.SplitContainer2.Panel2.Controls.Add(Me.SplitContainer4)
        Me.SplitContainer2.Size = New System.Drawing.Size(784, 482)
        Me.SplitContainer2.SplitterDistance = 166
        Me.SplitContainer2.TabIndex = 0
        '
        'tvwServers
        '
        Me.tvwServers.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tvwServers.Location = New System.Drawing.Point(0, 0)
        Me.tvwServers.Name = "tvwServers"
        Me.tvwServers.Size = New System.Drawing.Size(166, 482)
        Me.tvwServers.TabIndex = 0
        '
        'SplitContainer4
        '
        Me.SplitContainer4.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.SplitContainer4.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer4.Name = "SplitContainer4"
        '
        'SplitContainer4.Panel2
        '
        Me.SplitContainer4.Panel2.Controls.Add(Me.PictureBox1)
        Me.SplitContainer4.Panel2.Controls.Add(Me.lstPeers)
        Me.SplitContainer4.Size = New System.Drawing.Size(614, 482)
        Me.SplitContainer4.SplitterDistance = 450
        Me.SplitContainer4.TabIndex = 0
        '
        'lstPeers
        '
        Me.lstPeers.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstPeers.FormattingEnabled = True
        Me.lstPeers.IntegralHeight = False
        Me.lstPeers.ItemHeight = 15
        Me.lstPeers.Location = New System.Drawing.Point(0, 0)
        Me.lstPeers.Name = "lstPeers"
        Me.lstPeers.ScrollAlwaysVisible = True
        Me.lstPeers.Size = New System.Drawing.Size(160, 482)
        Me.lstPeers.TabIndex = 0
        '
        'LoadThemeToolStripMenuItem
        '
        Me.LoadThemeToolStripMenuItem.Name = "LoadThemeToolStripMenuItem"
        Me.LoadThemeToolStripMenuItem.Size = New System.Drawing.Size(56, 20)
        Me.LoadThemeToolStripMenuItem.Text = "Theme"
        '
        'HelpToolStripMenuItem
        '
        Me.HelpToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AboutYardbirdToolStripMenuItem})
        Me.HelpToolStripMenuItem.Name = "HelpToolStripMenuItem"
        Me.HelpToolStripMenuItem.Size = New System.Drawing.Size(44, 20)
        Me.HelpToolStripMenuItem.Text = "Help"
        '
        'AboutYardbirdToolStripMenuItem
        '
        Me.AboutYardbirdToolStripMenuItem.Name = "AboutYardbirdToolStripMenuItem"
        Me.AboutYardbirdToolStripMenuItem.Size = New System.Drawing.Size(154, 22)
        Me.AboutYardbirdToolStripMenuItem.Text = "About Yardbird"
        '
        'MenuStrip1
        '
        Me.MenuStrip1.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem, Me.NetworkToolStripMenuItem, Me.LoadThemeToolStripMenuItem, Me.HelpToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(784, 24)
        Me.MenuStrip1.TabIndex = 9
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ExportChatLogToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
        Me.FileToolStripMenuItem.Text = "File"
        '
        'ExportChatLogToolStripMenuItem
        '
        Me.ExportChatLogToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToHTMLToolStripMenuItem})
        Me.ExportChatLogToolStripMenuItem.Name = "ExportChatLogToolStripMenuItem"
        Me.ExportChatLogToolStripMenuItem.Size = New System.Drawing.Size(167, 22)
        Me.ExportChatLogToolStripMenuItem.Text = "Export Chat Log..."
        '
        'ToHTMLToolStripMenuItem
        '
        Me.ToHTMLToolStripMenuItem.Name = "ToHTMLToolStripMenuItem"
        Me.ToHTMLToolStripMenuItem.Size = New System.Drawing.Size(123, 22)
        Me.ToHTMLToolStripMenuItem.Text = "To HTML"
        '
        'NetworkToolStripMenuItem
        '
        Me.NetworkToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ClientToolStripMenuItem, Me.Server1ToolStripMenuItem})
        Me.NetworkToolStripMenuItem.Name = "NetworkToolStripMenuItem"
        Me.NetworkToolStripMenuItem.Size = New System.Drawing.Size(64, 20)
        Me.NetworkToolStripMenuItem.Text = "Network"
        '
        'ClientToolStripMenuItem
        '
        Me.ClientToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ConnectToServerToolStripMenuItem})
        Me.ClientToolStripMenuItem.Name = "ClientToolStripMenuItem"
        Me.ClientToolStripMenuItem.Size = New System.Drawing.Size(106, 22)
        Me.ClientToolStripMenuItem.Text = "Client"
        '
        'ConnectToServerToolStripMenuItem
        '
        Me.ConnectToServerToolStripMenuItem.Name = "ConnectToServerToolStripMenuItem"
        Me.ConnectToServerToolStripMenuItem.Size = New System.Drawing.Size(177, 22)
        Me.ConnectToServerToolStripMenuItem.Text = "Connect to Server..."
        '
        'Server1ToolStripMenuItem
        '
        Me.Server1ToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ControlPanelToolStripMenuItem})
        Me.Server1ToolStripMenuItem.Name = "Server1ToolStripMenuItem"
        Me.Server1ToolStripMenuItem.Size = New System.Drawing.Size(106, 22)
        Me.Server1ToolStripMenuItem.Text = "Server"
        '
        'ControlPanelToolStripMenuItem
        '
        Me.ControlPanelToolStripMenuItem.Name = "ControlPanelToolStripMenuItem"
        Me.ControlPanelToolStripMenuItem.Size = New System.Drawing.Size(146, 22)
        Me.ControlPanelToolStripMenuItem.Text = "Control Panel"
        '
        'PictureBox1
        '
        Me.PictureBox1.Location = New System.Drawing.Point(3, 426)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(131, 38)
        Me.PictureBox1.TabIndex = 1
        Me.PictureBox1.TabStop = False
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(784, 561)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.MinimumSize = New System.Drawing.Size(530, 400)
        Me.Name = "frmMain"
        Me.Text = "Yardbird"
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.SplitContainer3.Panel1.ResumeLayout(False)
        Me.SplitContainer3.Panel1.PerformLayout()
        Me.SplitContainer3.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer3, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer3.ResumeLayout(False)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.SplitContainer2.Panel1.ResumeLayout(False)
        Me.SplitContainer2.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer2.ResumeLayout(False)
        Me.SplitContainer4.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer4, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer4.ResumeLayout(False)
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents lblNetStatus As ToolStripStatusLabel
    Friend WithEvents lblCharLmt As ToolStripStatusLabel
    Friend WithEvents SplitContainer3 As SplitContainer
    Friend WithEvents txtMessage As TextBox
    Friend WithEvents btnFmt As Button
    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents SplitContainer2 As SplitContainer
    Friend WithEvents tvwServers As TreeView
    Friend WithEvents SplitContainer4 As SplitContainer
    Friend WithEvents lstPeers As ListBox
    Friend WithEvents LoadThemeToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents HelpToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AboutYardbirdToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents NetworkToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ClientToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents Server1ToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ConnectToServerToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ControlPanelToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents FileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ExportChatLogToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ToHTMLToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents PictureBox1 As PictureBox
End Class
