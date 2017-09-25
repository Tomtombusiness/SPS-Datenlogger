Public Class Datalogging_Settings_Form
    Dim ini As INI_Connector.INI_Connector

    'Symboltabelle öffnen(Pfad suchen)
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        OpenFileSymbolInputs.ShowDialog()
        If DialogResult <> DialogResult.Abort Then
            My.Settings.SymbolInput = OpenFileSymbolInputs.FileName
        End If
    End Sub
    'Log Protokoll suchen(Pfad)
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        SaveFileLog.ShowDialog()
        If DialogResult <> DialogResult.Abort Then
            My.Settings.LogOutput = SaveFileLog.FileName
        End If
    End Sub
    'Debug Protokoll suchen(Pfad)
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        SaveFileDebug.ShowDialog()
        If DialogResult <> DialogResult.Abort Then
            My.Settings.DebugOutput = SaveFileDebug.FileName
        End If
    End Sub

    'Autostart Datei beschreiben
    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click


        'Datalogging_Form.WriteIni()
    End Sub


End Class