Imports System.Threading
Imports System.Data
Imports System.Windows.Forms.DataVisualization.Charting

Public Class Datalogging_Form
    Dim CycleState As Boolean = False           'Zyklus Unterbrecher zum sauberen Zyklus beenden

    WithEvents Plugin_SPS_Connector As New Interfaces_Driver.Class_Interface_SPS_Control
    Dim Ini As INI_Connector.INI_Connector

    WithEvents Memory As New Memory_Connector.Controller

    Dim MessageCount As Integer = 0             'Anzahl der Meldungen 
    Dim ErrorExcepted As Boolean = False        'Schwerwigender Fehler(TabControl bekommt ! als marker)

#Region "Fehlerbehandlung"
    'Fehlerbehandlung Komunikation zur Steuerung
    Sub MemoryErrRaised(ByVal sender As Object, ByVal e As Memory_Connector.ErrorRaisedEventArgs) Handles Memory.ErrorRaised
        Select Case e.ErrNumber
            Case 1000
                Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Speicher", 3, "Fehler " & e.ErrNumber.ToString & " - " & e.ErrMessage)
            Case 1001
                Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Speicher", 3, "Fehler " & e.ErrNumber.ToString & " - " & e.ErrMessage)
            Case 1002
                Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Speicher", 3, "Fehler " & e.ErrNumber.ToString & " - " & e.ErrMessage)
            Case 1003
                Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Speicher", 3, "Fehler " & e.ErrNumber.ToString & " - " & e.ErrMessage)
            Case 1004
                Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Speicher", 3, "Fehler " & e.ErrNumber.ToString & " - " & e.ErrMessage)
            Case 1007
                Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Speicher", 3, "Fehler " & e.ErrNumber.ToString & " - " & e.ErrMessage)
            Case 1008
                Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Speicher", 3, "Fehler " & e.ErrNumber.ToString & " - " & e.ErrMessage)
            Case Else
                Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Speicher", 2, "Fehler " & e.ErrNumber.ToString & " - " & e.ErrMessage)
        End Select

    End Sub
    Sub SPSErrRaised(ByVal sender As Object, ByVal e As Interfaces_Driver.ErrorRaisedEventArgs) Handles Plugin_SPS_Connector.ErrorRaised
        Select Case e.ErrNumber
            Case Else
                Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Steuerung", 2, "Fehler " & e.ErrNumber.ToString & " - " & e.ErrMessage)
        End Select
    End Sub
    Sub ClientReconnect()
        'Automatischer Verbindungsversuch
        Dim Tries As Integer = 1
        While Tries <= NumericUpDown1.Value
            Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Steuerung", 2, "Automatischer Verbindungsaufbau. Versuch " & Tries.ToString & " von " & NumericUpDown1.Value.ToString)
            Plugin_SPS_Connector.Connect(TextBox1.Text, TextBox2.Text, TextBox3.Text, TextBox4.Text)
            If Plugin_SPS_Connector.Connected = True Then
                Exit While
            End If

            'Pause zwischen den Verbindungsversuchen
            Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Steuerung", 2, "Automatischer Verbindungsaufbau fehlgeschlagen. Warte " & NumericUpDown2.Value.ToString & " Sekunden")
            Tries = Tries + 1
            Thread.Sleep(CInt(NumericUpDown2.Value) * 1000)    'Konventierung von Sekunden in Milisekunden
        End While

        'Verbindungsversuch nicht erfolgreich
        If Plugin_SPS_Connector.Connected = False And Tries = NumericUpDown1.Value Then
            Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Steuerung", 3, "Verbindung konnte nicht hergestellt werden")
            CycleState = False
        ElseIf Plugin_SPS_Connector.Connected = True Then
            Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Steuerung", 1, "Verbindung wiederhergestellt")
        End If
    End Sub


    Public Delegate Sub WriteErrThread(ByVal Area As String, ByVal Priority As Integer, ByVal Message As String)
    'In Meldepuffer schreiben
    Sub WriteErrMessage(ByVal Area As String, ByVal Priority As Integer, ByVal Message As String)
        Dim RowIndex As Integer = DataGridView1.Rows.Add
        DataGridView1.Rows(RowIndex).Cells("D_Date").Value = Now.ToString
        DataGridView1.Rows(RowIndex).Cells("D_Priority").Value = Priority.ToString
        DataGridView1.Rows(RowIndex).Cells("D_Area").Value = Area
        DataGridView1.Rows(RowIndex).Cells("D_Message").Value = Message
        Select Case Priority
            Case 1  'Meldung
                DataGridView1.Rows(RowIndex).DefaultCellStyle.BackColor = Color.White
            Case 2  'Warnung
                DataGridView1.Rows(RowIndex).DefaultCellStyle.BackColor = Color.Orange
            Case 3  'Fehler
                DataGridView1.Rows(RowIndex).DefaultCellStyle.BackColor = Color.Red
                ErrorExcepted = True
                MessageBox.Show("Bereich: " & Area & vbCrLf & "Meldung: " & Message, "Fehler führte zum Stillstand", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Select
        MessageCount = MessageCount + 1
        If ErrorExcepted = True Then
            TabPage3.Text = "Meldungen!!!(" & MessageCount.ToString & ")!!!"
        Else
            TabPage3.Text = "Meldungen(" & MessageCount.ToString & ")"
        End If

    End Sub
    'Meldepuffer löschen
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        DataGridView1.Rows.Clear()
        TabPage3.Text = "Meldungen"
        MessageCount = 0
        ErrorExcepted = False
    End Sub
    'Zyklischer Ablauf Statusänderung
    Private Sub BGW_Cycle_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BGW_Cycle.RunWorkerCompleted
        If e.Cancelled Then ToolStripStatusLabel3.Text = "Prozess(Zyklisch); abgebrochen"
        If e.Result Then ToolStripStatusLabel3.Text = "Prozess(Zyklisch); abgebrochen"
        If e.Error IsNot Nothing Then
            ToolStripStatusLabel3.Text = "Prozess(Zyklisch); FEHLER"
            Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Prozess(Zyklisch)", 3, e.Error.Message)
        ElseIf e.Cancelled Then
            ToolStripStatusLabel3.Text = "Prozess(Zyklisch); abgebrochen"
            Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Prozess(Zyklisch)", 2, "Von Benutzer abebrochen")
        Else
            ToolStripStatusLabel3.Text = "Prozess(Zyklisch); beendet"
            Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Prozess(Zyklisch)", 2, "Von Benutzer beendet")
        End If

    End Sub

#End Region


#Region "Sonderfunktionen"
    Function GetConnectionType(ByVal Typ As String) As Integer
        Select Case Typ
            Case "PC/PG"
                Return 1
            Case "OP Panel"
                Return 2
            Case "Basic"
                Return 3
            Case Else
                Return 1        'Standardwert
        End Select
    End Function


#End Region

#Region "Form Steuerung"
    'Projekt Öffnen
    Private Sub ÖffnenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ÖffnenToolStripMenuItem.Click
        OpenSettings.ShowDialog()
        If DialogResult <> DialogResult.Cancel Then
            Dim Ini As New INI_Connector.INI_Connector(OpenSettings.FileName)
#Region "Verbindungseinstellungen"
            WriteErrMessage("Einstellungen laden", 1, "Verbindung")
            'Verbindung
            TextBox1.Text = Ini.ReadString("Connection", "Host", "127.0.0.1")
            TextBox2.Text = Ini.ReadString("Connection", "Rack", "0")
            TextBox3.Text = Ini.ReadString("Connection", "Slot", "2")
            ComboBox1.Text = Ini.ReadString("Connection", "Typ", "PG/PC")
            'Verbindungparameter
            CheckBox1.Checked = Ini.ReadBoolean("Connection", "Auto_Reconnect", True)
            NumericUpDown1.Text = Ini.ReadInteger("Connection", "Max_Tries_Reconnection", 10).ToString
            NumericUpDown2.Text = Ini.ReadInteger("Connection", "Intervall_Reconnection", 10).ToString

            'Speicher
            ComboBox2.Text = Ini.ReadString("Database", "Typ", "")
            TextBox4.Text = Ini.ReadString("Database", "Host", "")
            TextBox5.Text = Ini.ReadString("Database", "User", "")
            TextBox6.Text = Ini.ReadString("Database", "Passwort", "")
            TextBox7.Text = Ini.ReadString("Database", "Table", "")
            'Speicherparameter
            CheckBox1.Checked = Ini.ReadBoolean("Database", "Auto_Reconnect", True)
            NumericUpDown1.Text = Ini.ReadInteger("Database", "Max_Tries_Reconnection", 10).ToString
            NumericUpDown2.Text = Ini.ReadInteger("Database", "Intervall_Reconnection", 10).ToString
#End Region

#Region "Triggereinstellungen"
            'Zyklisch
            WriteErrMessage("Einstellungen laden", 1, "Trigger")
            RadioButton1.Checked = Ini.ReadBoolean("Trigger", "Cycle_Activated", False)
            NumericUpDown5.Text = Ini.ReadInteger("Trigger", "Cycle_Sampling_Rate", 100).ToString
            NumericUpDown6.Text = Ini.ReadInteger("Trigger", "Cycle_Time", 1000).ToString
            NumericUpDown7.Text = Ini.ReadInteger("Trigger", "Cycle_Pause", 5000).ToString
            'Getriggert
            RadioButton2.Checked = Ini.ReadBoolean("Trigger", "Trigger_Activated", False)
            NumericUpDown10.Text = Ini.ReadInteger("Trigger", "Trigger_Sampling_Rate", 100).ToString
            'Trigger

            Dim Trigger_Area As String() = Ini.ReadString("Trigger", "Area").Split(";")
            Dim Trigger_Adress As String() = Ini.ReadString("Trigger", "Adress").Split(";")
            Dim Trigger_Size As String() = Ini.ReadString("Trigger", "Size").Split(";")
            Dim Trigger_Format As String() = Ini.ReadString("Trigger", "Format").Split(";")
            Dim Trigger_Operator As String() = Ini.ReadString("Trigger", "Operator").Split(";")
            Dim Trigger_Value As String() = Ini.ReadString("Trigger", "Value").Split(";")
            Dim Trigger_Operation As String() = Ini.ReadString("Trigger", "Operation").Split(";")

            If Trigger_Area.Count = Trigger_Adress.Count And
               Trigger_Operation.Count = Trigger_Format.Count And
               Trigger_Operator.Count = Trigger_Value.Count Then

                DataGridView2.Rows.Clear()

                For i As Integer = 0 To Trigger_Area.Count - 2
                    Dim index As Integer = DataGridView2.Rows.Add
                    With DataGridView2.Rows(index)
                        .Cells("T_Area").Value = Trigger_Area(i)
                        .Cells("T_Adress").Value = Trigger_Adress(i)
                        .Cells("T_Size").Value = Trigger_Size(i)
                        .Cells("T_Format").Value = Trigger_Format(i)
                        .Cells("T_Operator").Value = Trigger_Operator(i)
                        .Cells("T_Value").Value = Trigger_Value(i)
                        .Cells("T_Operation").Value = Trigger_Operation(i)
                    End With
                Next


                WriteErrMessage("Einstellungen laden", 1, "Trigger erfolgreich geladen")
            Else
                WriteErrMessage("Einstellungen laden(Trigger)", 2, "ungleiche Anzahl. Laden nicht möglich" & vbCrLf &
                                "Anzahl Area:" & Trigger_Area.Count.ToString & vbCrLf &
                                "Anzahl Adress:" & Trigger_Adress.Count.ToString & vbCrLf &
                                "Anzahl Size:" & Trigger_Size.Count.ToString & vbCrLf &
                                "Anzahl Format:" & Trigger_Format.Count.ToString & vbCrLf &
                                "Anzahl Operator:" & Trigger_Operator.Count.ToString & vbCrLf &
                                "Anzahl Value:" & Trigger_Value.Count.ToString & vbCrLf &
                                "Anzahl Operation:" & Trigger_Operation.Count.ToString)
            End If
#End Region

#Region "Ein- / Ausgänge"
            WriteErrMessage("Einstellungen laden", 1, "IOs")
            Dim IO_Labels As String() = Ini.ReadString("IO", "Label").Split(";")
            Dim IO_Area As String() = Ini.ReadString("IO", "Area").Split(";")
            Dim IO_Adress As String() = Ini.ReadString("IO", "Adress").Split(";")
            Dim IO_Size As String() = Ini.ReadString("IO", "Size").Split(";")
            Dim IO_Format As String() = Ini.ReadString("IO", "Format").Split(";")

            If IO_Labels.Count = IO_Area.Count And IO_Adress.Count = IO_Format.Count Then
                DataGridView3.Rows.Clear()

                For i As Integer = 0 To IO_Labels.Count - 2
                    Dim Index As Integer = DataGridView3.Rows.Add
                    With DataGridView3.Rows(Index)
                        .Cells("IO_Label").Value = IO_Labels(i)
                        .Cells("IO_Area").Value = IO_Area(i)
                        .Cells("IO_Adress").Value = IO_Adress(i)
                        .Cells("IO_Size").Value = IO_Size(i)
                        .Cells("IO_Format").Value = IO_Format(i)
                    End With
                Next
                WriteErrMessage("Einstellungen laden", 1, "IOs erfolgreich geladen")
            Else
                WriteErrMessage("Einstellungen laden(IO)", 2, "ungleiche Anzahl. Laden nicht möglich" & vbCrLf &
                                "Anzahl Labels:" & IO_Labels.Count.ToString & vbCrLf &
                                "Anzahl Area:" & IO_Area.Count.ToString & vbCrLf &
                                "Anzahl Adress:" & IO_Adress.Count.ToString & vbCrLf &
                                "Anzahl Size:" & IO_Size.Count.ToString & vbCrLf &
                                "Anzahl Format:" & IO_Format.Count.ToString)
            End If
#End Region
        End If
    End Sub
    'Projekt Speichern
    Private Sub SpeichernToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SpeichernToolStripMenuItem.Click
        SaveSettings.ShowDialog()
        If DialogResult <> DialogResult.Cancel Then
            Dim Ini As New INI_Connector.INI_Connector(SaveSettings.FileName)

#Region "Verbindungseinstellungen"
            WriteErrMessage("Einstellungen speichern", 1, "Verbindung")
            'Verbindung
            Ini.Write("Connection", "Host", TextBox1.Text)
            Ini.Write("Connection", "Rack", TextBox2.Text)
            Ini.Write("Connection", "Slot", TextBox3.Text)
            Ini.Write("Connection", "Typ", ComboBox1.Text)
            'Verbindungparameter
            Ini.Write("Connection", "Auto_Reconnect", CheckBox1.Checked)
            Ini.Write("Connection", "Max_Tries_Reconnection", NumericUpDown1.Text)
            Ini.Write("Connection", "Intervall_Reconnection", NumericUpDown2.Text)

            'Speicher
            Ini.Write("Database", "Typ", ComboBox2.Text)
            Ini.Write("Database", "Host", TextBox4.Text)
            Ini.Write("Database", "User", TextBox5.Text)
            Ini.Write("Database", "Passwort", TextBox6.Text)
            Ini.Write("Database", "Table", TextBox7.Text)
            'Speicherparameter
            Ini.Write("Database", "Auto_Reconnect", CheckBox1.Checked)
            Ini.Write("Database", "Max_Tries_Reconnection", NumericUpDown1.Text)
            Ini.Write("Database", "Intervall_Reconnection", NumericUpDown2.Text)
#End Region

#Region "Triggereinstellungen"
            'Zyklisch
            WriteErrMessage("Einstellungen speichern", 1, "Trigger")
            Ini.Write("Trigger", "Cycle_Activated", RadioButton1.Checked)
            Ini.Write("Trigger", "Cycle_Sampling_Rate", NumericUpDown5.Text)
            Ini.Write("Trigger", "Cycle_Time", NumericUpDown6.Text)
            Ini.Write("Trigger", "Cycle_Pause", NumericUpDown7.Text)
            'Getriggert
            Ini.Write("Trigger", "Trigger_Activated", RadioButton2.Checked)
            Ini.Write("Trigger", "Trigger_Sampling_Rate", NumericUpDown10.Text)
            'Trigger

            DataGridView2.AllowUserToAddRows = False    'DGW Sperren um Letzte Spalte auszublenden

            Dim T_Area As String
            Dim T_Adress As String
            Dim T_Size As String
            Dim T_Format As String
            Dim T_Operator As String
            Dim T_Value As String
            Dim T_Operation As String



            For Each r As DataGridViewRow In DataGridView2.Rows
                T_Area = T_Area & r.Cells("T_Area").Value & ";"
                T_Adress = T_Adress & r.Cells("T_Adress").Value & ";"
                T_Size = T_Size & r.Cells("T_Size").Value & ";"
                T_Format = T_Format & r.Cells("T_Format").Value & ";"
                T_Operator = T_Operator & r.Cells("T_Operator").Value & ";"
                T_Value = T_Value & r.Cells("T_Value").Value & ";"
                T_Operation = T_Operation & r.Cells("T_Operation").Value & ";"
            Next
            Ini.Write("Trigger", "Area", T_Area)
            Ini.Write("Trigger", "Adress", T_Adress)
            Ini.Write("Trigger", "Size", T_Size)
            Ini.Write("Trigger", "Format", T_Format)
            Ini.Write("Trigger", "Operator", T_Operator)
            Ini.Write("Trigger", "Value", T_Value)
            Ini.Write("Trigger", "Operation", T_Operation)

            DataGridView2.AllowUserToAddRows = True     'DGW Freigeben
            WriteErrMessage("Einstellungen speichern", 1, "Trigger erfolgreich gespeichert")

#End Region

#Region "Ein- / Ausgänge"
            DataGridView3.AllowUserToAddRows = False    'DGW Sperren um Letzte Spalte auszublenden

            Dim IO_Labels As String
            Dim IO_Area As String
            Dim IO_Adress As String
            Dim IO_Size As String
            Dim IO_Format As String

            For Each r As DataGridViewRow In DataGridView3.Rows
                IO_Labels = IO_Labels & r.Cells("IO_Label").Value & ";"
                IO_Area = IO_Area & r.Cells("IO_Area").Value & ";"
                IO_Adress = IO_Adress & r.Cells("IO_Adress").Value & ";"
                IO_Size = IO_Size & r.Cells("IO_Size").Value & ";"
                IO_Format = IO_Format & r.Cells("IO_Format").Value & ";"
            Next

            Ini.Write("IO", "Label", IO_Labels)
            Ini.Write("IO", "Area", IO_Area)
            Ini.Write("IO", "Adress", IO_Adress)
            Ini.Write("IO", "Size", IO_Size)
            Ini.Write("IO", "Format", IO_Format)

            DataGridView3.AllowUserToAddRows = True     'DGW Freigeben
            WriteErrMessage("Einstellungen speichern", 1, "IOs erfolgreich gespeichert")
#End Region


        End If
    End Sub

    'Projekt starten
    Private Sub StartenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles StartenToolStripMenuItem.Click
        DataGridView3.AllowUserToAddRows = False    'Eingabe sperren
        StartCycle()
    End Sub
    'Projekt stoppen
    Private Sub StoppenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles StoppenToolStripMenuItem.Click
        CycleState = False

    End Sub
    'Anwendung startet
    Private Sub Datalogging_Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ComboBox3.Items.Clear()
        If Plugin_SPS_Connector.GetPluginList.Count <> 0 Then
            For Each Item As String In Plugin_SPS_Connector.GetPluginList
                ComboBox3.Items.Add(Item)
            Next
        End If
    End Sub
    'Anwendung beenden
    Private Sub BeendenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BeendenToolStripMenuItem.Click

        Me.Close()
    End Sub
    'Anwendung beenden(Form Schließen)
    Private Sub Datalogging_Form_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        CycleState = False
        'Warteschleife bis Zyklischer Zyklus ordendlich beendet ist
        Thread.Sleep(5000)
    End Sub

#End Region

#Region "Verbindung Steuerung"
    'Verbindungstest - Steuerungsverbindung
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If Plugin_SPS_Connector.Connect(TextBox1.Text, CInt(TextBox2.Text), CInt(TextBox3.Text), GetConnectionType(ComboBox1.Text)) Then
            Label4.Text = "Verbindung erfolgreich"
            Plugin_SPS_Connector.Disconnect()
        Else
            Label4.Text = "Verbindung erfolglos"
        End If
    End Sub

    'Auswahl Reconnect Steuerung
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked Then
            NumericUpDown1.ReadOnly = False
            NumericUpDown2.ReadOnly = False
        Else
            NumericUpDown1.ReadOnly = True
            NumericUpDown2.ReadOnly = True
        End If
    End Sub

#End Region
#Region "Verbindung Speicher"
    'Verbindungstest - Speicherverbindung
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        Memory.C_Format = ComboBox2.Text
        Memory.C_Adresses = New Memory_Connector.Adress(TextBox4.Text, TextBox7.Text, TextBox8.Text)
        Memory.C_Credientals = New Memory_Connector.Crediental(TextBox5.Text, TextBox6.Text)

        If Memory.Connect Then
            Label9.Text = "Verbindung erfolgreich"

            Memory.Disconnect()
        Else
            Label9.Text = "Verbindung erfolglos"
        End If
    End Sub
    'Auswahl Verbindungstyp des Speichers
    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox2.SelectedIndexChanged
        Select Case ComboBox2.Text
            Case "XML"
                TextBox4.Enabled = True     '
                TextBox5.Enabled = False
                TextBox6.Enabled = False
                TextBox7.Enabled = True
                TextBox8.Enabled = True
            Case Else
                MessageBox.Show("Dieses Format wird noch nicht unterstützt." & vbCrLf &
                                "Bitte wählen sie ein anderes Format aus", "Nicht unterstütztes Format", MessageBoxButtons.OK)
        End Select

    End Sub
    'Speicher suchen
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Memory.C_Format = ComboBox2.Text
        Dim MemoryPath As String = Memory.SearchPath()
        If MemoryPath <> "" Then
            TextBox4.Text = MemoryPath
        End If

        Exit Sub

        Select Case ComboBox2.Text

            Case "CSV"
                SaveFileSpeicher.Filter = "CSV Datenspeicher|*.csv"
                SaveFileSpeicher.ShowDialog()
                If DialogResult <> DialogResult.Cancel Then
                    If IO.File.Exists(SaveFileSpeicher.FileName) Then
                    Else    'Datei anlegen
                        IO.File.AppendAllText(SaveFileSpeicher.FileName, "")
                        WriteErrMessage("Speicher - anlegen", 1, "Datei erfolgreich angelegt")
                    End If
                    TextBox4.Text = SaveFileSpeicher.FileName
                End If
            Case "SqLite"
                SaveFileSpeicher.Filter = "Lokaler Datenspeicher|*.SqLite, *.mdf"
                SaveFileSpeicher.ShowDialog()
                If DialogResult <> DialogResult.Cancel Then
                    If IO.File.Exists(SaveFileSpeicher.FileName) Then
                    Else    'Datei anlegen
                        IO.File.AppendAllText(SaveFileSpeicher.FileName, "")
                        WriteErrMessage("Speicher - anlegen", 1, "Datei erfolgreich angelegt")
                    End If
                    TextBox4.Text = SaveFileSpeicher.FileName
                End If
        End Select
    End Sub

#End Region

#Region "Datensätze"
    'Datenbank laden
    Private Sub TabControl1_Selected(sender As Object, e As TabControlEventArgs) Handles TabControl1.Selected
        If e.TabPage.Text = TabPage4.Text Then

            TreeView1.Nodes.Clear()
            Try
                For Each TB As DataTable In Memory.C_DS.Tables
                    TreeView1.Nodes.Add(TB.TableName)
                Next
            Catch ex As NullReferenceException

            End Try




        End If

    End Sub
    'Tabelle Laden
    Private Sub TreeView1_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterSelect
        DataGridView4.DataSource = Memory.C_DS.Tables(TreeView1.SelectedNode.Text)
        Chart1.Series.Clear()
        Chart1.ChartAreas.Clear()
        Chart1.ChartAreas.Add("Allgemein")
        Chart1.DataSource = Memory.C_DS.Tables(TreeView1.SelectedNode.Text)
        For Each Ser As DataColumn In Memory.C_DS.Tables(TreeView1.SelectedNode.Text).Columns
            Dim clm As Series = Chart1.Series.Add(Ser.ColumnName)
            clm.ChartArea = "Allgemein"
            clm.YValueMembers = Ser.ColumnName
        Next
    End Sub
#End Region

#Region "Ablaufsteuerung"
    'Zyklische abtastung starten
    Sub StartCycle()
        If BGW_Cycle.IsBusy Then
            WriteErrMessage("Zyklus", 3, "Zyklus Zyklus läuft schon")
            Exit Sub
        End If

        CycleState = True
        WriteErrMessage("Zyklus", 1, "Zyklus wird gestartet")

        '--------------------------------
        'Verbindung zur Steuerung starten
        '--------------------------------

        Plugin_SPS_Connector.Connect(TextBox1.Text, CInt(TextBox2.Text), CInt(TextBox3.Text), GetConnectionType(ComboBox1.Text))

        '--------------------------------
        'Verbindung zum Speicher starten
        '--------------------------------
        Memory.C_Format = ComboBox2.Text
        Memory.C_Adresses = New Memory_Connector.Adress(TextBox4.Text, TextBox7.Text, TextBox8.Text)
        Memory.C_Credientals = New Memory_Connector.Crediental(TextBox5.Text, TextBox6.Text)

        Memory.Connect()

        If Plugin_SPS_Connector.Connected And Memory.C_ConnState Then

            If RadioButton1.Checked Then    'Zyklischer ablauf
                ' CycleAction()
                BGW_Cycle.RunWorkerAsync()
            End If
        Else
            If Plugin_SPS_Connector.Connected = False Then
                WriteErrMessage("Zyklus", 3, "Kann keine Verbindung zur Steuerung aufbauen(Verbindungstest durchführen!)")
            End If
            If Memory.C_ConnState = False Then
                WriteErrMessage("Zyklus", 3, "Kann keine Verbindung zum Speicher aufbauen(Verbindungstest durchführen!)")
            End If
        End If

    End Sub

    'Zyklischer Zyklus
    Private Sub BGW_Cycle_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BGW_Cycle.DoWork
        While CycleState = True             'Zyklus Unterbrecher
            While Plugin_SPS_Connector.Connected          'Solange Client verbunden
                While Memory.C_ConnState      'Solange Server verbunden 

                    'Eingaben sperren
                    DataGridView3.AllowUserToAddRows = False     'Eingabe Freigeben

                    '-----------------


                    Dim Endtime As DateTime = Now.AddMilliseconds(NumericUpDown6.Value)     'Zykluszeit Endzeit
                    While Now <= Endtime
                        If Memory.NewRow = True Then  'Neue Zeile starten
                            For Each Row As DataGridViewRow In DataGridView3.Rows

                                '--------------Deklaration aller Werte--------------
                                Dim Area As String = Row.Cells("IO_Area").Value.ToString
                                Dim Pos_Byte As Integer = 0
                                Dim Pos_Bit As Integer = 0
                                Dim DB_Size As Integer = 0
                                Dim Typ As String = Row.Cells("IO_Format").Value.ToString

                                'Prüfen ob Zelle leer, Wenn Ja nehme 0
                                If String.IsNullOrEmpty(Row.Cells("IO_Size").Value.ToString) = False Then DB_Size = CInt(Row.Cells("IO_Size").Value.ToString)

                                If Row.Cells("IO_Adress").Value.ToString.Contains(".") = True Then
                                    Pos_Byte = Row.Cells("IO_Adress").Value.Split(".")(0)
                                    Pos_Bit = Row.Cells("IO_Adress").Value.Split(".")(1)
                                Else
                                    Pos_Byte = CInt(Row.Cells("IO_Adress").Value)
                                End If
                                '--------------Deklaration aller Werte--------------


                                'Verarbeite Eingabe
                                Dim Val As String = Plugin_SPS_Connector.Read(Area, Pos_Byte, Typ, Pos_Bit, DB_Size).ToString

                                Memory.SetRow(Row.Cells("IO_Label").Value.ToString, Val)
                            Next
                        Else
                            Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Zyklus - Speicher", 2, "Problem beim erstellen einer Zeile")
                        End If


                        If Memory.CommitRow() = True Then 'Spalte zum abspeicher Freigeben und neue Beantragen
                        Else
                            Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Zyklus - Speicher(Commit)", 2, "Problem beim abspeichern einer Zeile")
                            CycleState = False
                        End If







                        'DataGridView4.DataSource = Server.GetDataTable() 'DGW aktualisieren
                        Thread.Sleep(CInt(NumericUpDown5.Value))                                  'Abtastrate

                    End While
                    Thread.Sleep(CInt(NumericUpDown7.Value))                                      'Zyklus Pause

                    'Fehlerbehandlung von Verbindungsabbrüchen
                    If CycleState = False Then
                        GoTo Zyklus_Stop
                    End If
                    If Plugin_SPS_Connector.Connected = False Then
                        GoTo Client_Stop
                    End If
                    If Memory.C_ConnState = False Then
                        GoTo Database_Stop
                    End If
                End While

Database_Stop:
                'Speicher Verbindung verloren
                Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Zyklus - Speicher", 2, "Verbindung verloren")
                While CycleState = True And Memory.C_ConnState = False
                End While
            End While

Client_Stop:
            'Steuerung Verbindung verloren
            Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Zyklus - Steuerung", 2, "Verbindung verloren")
            While CycleState = True And Plugin_SPS_Connector.Connected = False
            End While
        End While
        'Zyklus stop
Zyklus_Stop:
        Memory.Disconnect()
        Plugin_SPS_Connector.Disconnect()
        Me.Invoke(New WriteErrThread(AddressOf WriteErrMessage), "Zyklus", 2, "Vom Benutzer abgebrochen")

        DataGridView3.AllowUserToAddRows = True     'Eingabe Freigeben

        BGW_Cycle.CancelAsync()
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        If Memory.C_ConnState = False Then
            Memory.C_Format = ComboBox2.Text
            Memory.C_Adresses = New Memory_Connector.Adress(TextBox4.Text, TextBox7.Text, TextBox8.Text)
            Memory.C_Credientals = New Memory_Connector.Crediental(TextBox5.Text, TextBox6.Text)
            If Memory.Connect = True Then
                Button5.Text = "Trennen"
            Else
                Button5.Text = "Verbinden"
            End If
        Else
            Memory.Disconnect()
            Button5.Text = "Verbinden"
        End If
    End Sub

    'Automatische Aktualisierung der Datensätze
    Private Sub TAutoActualLoad_Tick(sender As Object, e As EventArgs) Handles TAutoActualLoad.Tick
        Try
            DataGridView4.DataSource = Memory.C_DS.Tables(TreeView1.SelectedNode.Text)
        Catch ex As NullReferenceException      'KEine Tabelle ausgewählt

        End Try

    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox3.CheckedChanged
        If CheckBox3.Checked Then
            TAutoActualLoad.Start()
        Else
            TAutoActualLoad.Stop()
        End If

    End Sub

    Private Sub ComboBox3_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox3.SelectedIndexChanged
        Plugin_SPS_Connector.SelectPlugin(ComboBox3.Text)
    End Sub





#End Region

End Class