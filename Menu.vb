Public Class Menu
    Private Sub BtnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        If txtPlayer.Text = "" Then
            MessageBox.Show("Please enter a player name")
            txtPlayer.Focus()
            Exit Sub
        End If
        Dim player_Name As String = txtPlayer.Text
        Dim gameInterface As New Game_Interface(player_Name)
        Hide()
        gameInterface.Show()
        End
    End Sub
End Class