Public Class DVDMakerGuide

    Private Sub DVDMakerGuide_FormClosing(sender As System.Object, e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        Form1.Option12.Visible = Not IO.Directory.Exists(Form1.windrive + "Program Files\DVD Maker\codecs")
    End Sub
End Class