Imports System.Windows.Forms

Public Class Controller
    Private _C_Format As String
    Property C_Format() As String
        Set(value As String)
            Select Case value
                Case "XML"
                Case "Live"
                Case Else
                    _C_Format = Nothing
                    RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1003, "Format wird nicht unterstützt"))
            End Select
            _C_Format = value
        End Set
        Get
            Return _C_Format
        End Get
    End Property
    Private _C_Credientals As Crediental
    Property C_Credientals() As Crediental
        Get
            Return _C_Credientals
        End Get
        Set(value As Crediental)
            _C_Credientals = value
        End Set
    End Property
    Private _C_Adresses As Adress
    Property C_Adresses() As Adress
        Get
            Return _C_Adresses
        End Get
        Set(value As Adress)
            _C_Adresses = value
        End Set
    End Property
    Private _C_ConnState As Boolean
    ReadOnly Property C_ConnState() As Boolean     'Zum Überprüfen ob noch mit anderem Speicher verbunden
        Get
            Return _C_ConnState
        End Get
    End Property

    '___________Interne Datenbank___________
    Private _C_DS As DataSet
    ReadOnly Property C_DS As DataSet
        Get
            Return _C_DS
        End Get
    End Property
    Private _C_Row As DataRow
    Private _C_RowNew As Boolean = False
    Private _C_RowChanged As Boolean = False

    '___________Sonstiges___________
    Dim Live_Form As Live_View

    Public Event ErrorRaised(ByVal sender As Object, ByVal e As ErrorRaisedEventArgs)

    Sub New()
        Me._C_Format = Nothing
        Me._C_Credientals = Nothing
        Me._C_Adresses = Nothing
        Me._C_ConnState = Nothing
        Me._C_DS = New DataSet
    End Sub

    ''' <summary>
    ''' Sucht das jeweilige Ziel zb durch eine Adresse
    ''' </summary>
    ''' <returns>Gibt die Adresse zurück</returns>
    Function SearchPath() As String
        Dim DLG As New SaveFileDialog
        Select Case _C_Format
            Case "XML"
                DLG.Filter = "Tabelle XML|*.XML"
                If DLG.ShowDialog <> DialogResult.Cancel Then
                    If IO.File.Exists(DLG.FileName) = False Then
                        IO.File.AppendAllText(DLG.FileName, "")
                        RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1017, "Angegebenen Host erstellt"))
                    End If
                    Return DLG.FileName
                Else
                    Return ""
                End If
            Case "Live"
                Return ""
            Case Else           'Kein Format ausgewählt
                RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1002, "Kein Format ausgewählt"))
                Return ""
        End Select
    End Function

    ''' <summary>
    ''' Verbindet zum angegebenen Host
    ''' </summary>
    ''' <returns>Ergebnis des Verbindungsversuches</returns>
    Function Connect() As Boolean
        If _C_ConnState = False Then
            If _C_Format <> Nothing Then
                Dim Result As Boolean = False


                Select Case _C_Format


                    Case "XML"
                        If _C_Adresses.Path <> Nothing Then
                            Result = ReadIn()
                        Else
                            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1000, "Adressen nicht definiert"))
                            Return False
                        End If


                    Case "Live"
                        Live_Form = New Live_View
                        Live_Form.Chart1.DataSource = _C_DS.Tables(_C_Adresses.Table)
                        Live_Form.Show()
                        Result = True
                    Case Else
                        Return False
                End Select

                _C_ConnState = Result
                Return Result
            Else
                RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1000, "Kein Format ausgewählt"))
                Return False
            End If
        Else
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1005, "Verbindung zum Host nicht getrennt!"))
            Return False
        End If

    End Function
    ''' <summary>
    ''' Trennt vom angegebenen Host
    ''' </summary>
    ''' <returns>Ergebnis des Trennungsversuches</returns>
    Function Disconnect()
        If _C_ConnState = True Then

            Dim Result As Boolean = False

            Select Case _C_Format
                Case "XML"
                    Result = WriteOut()
                Case "Live"
                    Live_Form.Close()
                    Result = True
                Case Else
                    Return False
            End Select
            _C_DS.Clear()
            _C_RowNew = False
            _C_RowChanged = False
            _C_ConnState = False
            Return Result
        Else
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1006, "Verbindung schon getrennt gewesen"))
            Return True
        End If
    End Function

    ''' <summary>
    ''' Legt eine neue Zeile bereit
    ''' </summary>
    ''' <returns></returns>
    Function NewRow() As Boolean
        If _C_ConnState = True Then
            If _C_Format <> Nothing Then
                If C_Adresses.Path <> Nothing Then
                    If _C_RowNew = False Then
                        If _C_RowChanged = False Then

                            'Prüfen ob Tabelle existiert
                            Dim TbSearchResult As Boolean = False
                            For Each Tb As DataTable In _C_DS.Tables
                                If Tb.TableName = _C_Adresses.Table Then
                                    TbSearchResult = True
                                End If
                            Next


                            If TbSearchResult = True Then       'Tabelle vorhanden
                                _C_Row = _C_DS.Tables(_C_Adresses.Table).NewRow
                                _C_RowNew = True
                                Return True
                            Else                                'Tabelle nicht vorhanden(Erstellen)
                                _C_DS.Tables.Add(_C_Adresses.Table)
                                _C_Row = _C_DS.Tables(_C_Adresses.Table).NewRow
                                _C_RowNew = True
                                RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1009, "Tabelle nicht vorhanden! Neu erstellt"))
                                Return True
                            End If
                        Else
                            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1016, "Zeile enthällt Werte wurde aber noch nicht gespeichert"))
                            Return False
                        End If
                    Else
                        RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1013, "Zeile schon Initialisiert"))
                        _C_RowNew = True
                        Return True
                    End If
                Else
                    RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1000, "Adressen nicht definiert"))
                    Return False
                End If
            Else
                RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1002, "Kein Format ausgewählt"))
                Return False
            End If
        Else
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1007, "Nicht mit Host verbunden"))
            Return False
        End If
    End Function
    ''' <summary>
    ''' Legt Werte in die aktuelle Zeile ab
    ''' </summary>
    ''' <param name="Key">Spaltenname</param>
    ''' <param name="Value">Wert</param>
    ''' <returns></returns>
    Function SetRow(ByVal Key As String, ByVal Value As String) As Boolean
        If _C_ConnState = True Then
            If _C_Format <> Nothing Then
                If C_Adresses.Path <> Nothing Then
                    If _C_RowNew = True Then

                        'Prüfen ob Spalte vorhanden
                        Dim CLSearchSesult As Boolean = False
                        For Each Cl As DataColumn In _C_DS.Tables(_C_Adresses.Table).Columns
                            If Cl.ColumnName = Key Then
                                CLSearchSesult = True
                            End If
                        Next

                        If CLSearchSesult = True Then
                            _C_Row(Key) = Value                 'Abfrage ob spalte vorhanden
                            _C_RowChanged = True
                            Return True
                        Else
                            _C_DS.Tables(_C_Adresses.Table).Columns.Add(Key)
                            _C_Row(Key) = Value                 'Abfrage ob spalte vorhanden
                            _C_RowChanged = True
                            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1010, "Spalte nicht vorhanden! Neu erstellt"))
                            Return True
                        End If

                    Else
                        RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1014, "Zeile nicht initialisiert"))
                        Return False
                    End If
                Else
                    RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1000, "Adressen nicht definiert"))
                    Return False
                End If
            Else
                RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1002, "Kein Format ausgewählt"))
                Return False
            End If
        Else
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1007, "Nicht mit Host verbunden"))
            Return False
        End If

    End Function
    ''' <summary>
    ''' Gibt die Zeile frei zum Speichern
    ''' </summary>
    ''' <returns></returns>
    Function CommitRow() As Boolean
        If _C_ConnState = True Then
            If _C_Format <> Nothing Then
                If C_Adresses.Path <> Nothing Then
                    If _C_RowNew = True Then
                        If _C_RowChanged = True Then
                            _C_DS.Tables(C_Adresses.Table).Rows.Add(_C_Row)
                            _C_RowNew = False
                            _C_RowChanged = False

                            Return WriteOut()
                        Else
                            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1015, "Zeile enthällt keine Werte. Nicht gespeichert"))
                            _C_RowNew = False
                            _C_RowChanged = False
                            Return True
                        End If

                    Else
                        RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1014, "Zeile nicht initialisiert"))
                        Return False
                    End If
                Else
                    RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1000, "Adressen nicht definiert"))
                    Return False
                End If
            Else
                RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1002, "Kein Format ausgewählt"))
                Return False
            End If
        Else
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1007, "Nicht mit Host verbunden"))
            Return False
        End If
    End Function

    ''' <summary>
    ''' Liest Daten vom angegebenen Speicher ein
    ''' </summary>
    ''' <returns></returns>
    Private Function ReadIn()
        If _C_Format <> Nothing Then
            If C_Adresses.Path <> Nothing Then
                Select Case _C_Format
                    Case "XML"
                        If IO.File.Exists(_C_Adresses.Path) = True Then
                            Try
                                _C_DS.ReadXml(_C_Adresses.Path)
                            Catch ex As Xml.XmlException

                            End Try

                        Else
                            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(104, "Angegebenen Host nicht gefunden"))
                        End If

                        Return True
                    Case "Live"
                        Return True
                    Case Else
                        Return False
                End Select
            Else
                RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1000, "Adressen nicht definiert"))
                Return False
            End If
        Else
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1002, "Kein Format ausgewählt"))
            Return False
        End If
    End Function
    ''' <summary>
    ''' Schreibt Daten in den angegebenen Speicher rein
    ''' </summary>
    ''' <returns></returns>
    Private Function WriteOut() As Boolean
        If _C_ConnState = True Then
            If _C_Format <> Nothing Then
                If C_Adresses.Path <> Nothing Then
                    Select Case _C_Format
                        Case "XML"
                            _C_DS.WriteXml(_C_Adresses.Path)
                            Return True
                        Case "Live"
                            Live_Form.Chart1.DataSource = _C_DS.Tables(_C_Adresses.Table)
                            Return True
                        Case Else
                            Return False
                    End Select
                Else
                    RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1000, "Adressen nicht definiert"))
                    Return False
                End If
            Else
                RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1002, "Kein Format ausgewählt"))
                Return False
            End If
        Else
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(1007, "Nicht mit Host verbunden"))
            Return False
        End If
    End Function
End Class


''' <summary>
''' Fehlerbehandlung - Die Bibliothek gibt Codes raus
''' Quasi ein Converter
''' </summary>
Public Class ErrorRaisedEventArgs
        Inherits EventArgs
        'das was man später unter e sehen kann
        Public ErrNumber As Integer
        Public ErrMessage As String

        'Zum übergeben der Parameter
        Public Sub New(ByVal Number As Integer, ByVal Message As String)
            ErrNumber = Number
            ErrMessage = Message
        End Sub
    End Class


Public Class Crediental
    Private _Username As String
    Property Username() As String
        Get
            Return _Username
        End Get
        Set(value As String)
            _Username = value
        End Set
    End Property
    Private _Password As String
    Property Password() As String
        Get
            Return _Password
        End Get
        Set(value As String)
            _Password = value
        End Set
    End Property
    Sub New(ByVal Usrnme As String, ByVal Pwd As String)
        _Username = Usrnme
        _Password = Pwd
    End Sub

End Class

Public Class Adress
    Private _Path As String
    Property Path As String
        Get
            Return _Path
        End Get
        Set(value As String)
            _Path = value
        End Set
    End Property
    Private _Database As String
    Property Database As String
        Get
            Return _Database
        End Get
        Set(value As String)
            _Database = value
        End Set
    End Property
    Private _Table As String
    Property Table As String
        Get
            Return _Table
        End Get
        Set(value As String)
            _Table = value
        End Set
    End Property

    Sub New(ByVal Path As String, ByVal Database As String, ByVal Table As String)
        _Path = Path
        _Database = Database
        _Table = Table
    End Sub
End Class
