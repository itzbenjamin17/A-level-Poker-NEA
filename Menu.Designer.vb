<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Menu
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
        btnStart = New Button()
        Label1 = New Label()
        txtPlayer = New TextBox()
        SuspendLayout()
        ' 
        ' btnStart
        ' 
        btnStart.Anchor = AnchorStyles.None
        btnStart.Location = New Point(907, 546)
        btnStart.Name = "btnStart"
        btnStart.Size = New Size(112, 34)
        btnStart.TabIndex = 0
        btnStart.Text = "Start"
        btnStart.UseVisualStyleBackColor = True
        ' 
        ' Label1
        ' 
        Label1.Anchor = AnchorStyles.None
        Label1.AutoSize = True
        Label1.Location = New Point(870, 456)
        Label1.Name = "Label1"
        Label1.Size = New Size(195, 25)
        Label1.TabIndex = 1
        Label1.Text = "Enter your name below"
        ' 
        ' txtPlayer
        ' 
        txtPlayer.Anchor = AnchorStyles.None
        txtPlayer.Location = New Point(870, 497)
        txtPlayer.Name = "txtPlayer"
        txtPlayer.Size = New Size(182, 31)
        txtPlayer.TabIndex = 2
        ' 
        ' Menu
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1924, 1024)
        Controls.Add(txtPlayer)
        Controls.Add(Label1)
        Controls.Add(btnStart)
        Name = "Menu"
        Text = "Menu"
        WindowState = FormWindowState.Maximized
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents btnStart As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents txtPlayer As TextBox
End Class
