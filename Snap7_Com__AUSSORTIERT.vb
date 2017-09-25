Imports Snap7
Imports Snap7.S7Client
Imports Snap7.S7Consts

Module Snap7_Com
    Dim Client As S7Client                                                                          'Deklaration des Clienten
    Property ConnectionState As Boolean = False                                                     'Verbindungsstatus
    Dim Result As Integer = 0                                                                       'Letzte Ergebnismeldung
    Dim Buffer(65536) As Byte                                                                       'Buffer  

#Region "Eventbehandlung"
    Public Event ClientConnected()                                                                  'Client ist verbunden
    Public Event ClientDisconnected()                                                               'Client ist getrennt
    Public Event MessageRaised(ByVal Number As Integer, ByVal Message As String)                    'Meldung wurde ausgelöst
#End Region
    'Löst die Meldung aus
    Private Sub ShowResult(ByVal Result As Integer)
        If Result <> 0 Then

            RaiseEvent MessageRaised(Result, Client.ErrorText(Result))
        End If

    End Sub

    'Verbinden
    Sub Connect(ByVal Host As String, ByVal Rack As Integer, ByVal Slot As Integer, ByVal ConnectionType As Integer)
        Client = New S7Client

        Client.SetConnectionType(ConnectionType)

        Result = Client.ConnectTo(Host, Rack, Slot)                 'Verbindet mit den angegebenen Parametern

        ShowResult(Result)

        If Result = 0 Then
            ConnectionState = True                                  'Setze Verbindungsstatus auf Verbunden
            RaiseEvent ClientConnected()                            'Löse Event aus
        End If
    End Sub
    'Trennen
    Sub Disconnect()
        Result = Client.Disconnect()
        ShowResult(Result)

        If Result = 0 Then
            ConnectionState = False                                  'Setze Verbindungsstatus auf getrennt
            RaiseEvent ClientConnected()                            'Löse Event aus
        End If
    End Sub

    Sub ReadValue(ByVal Area As String, ByVal Pos_Byte As Integer, Pos_Bit As Integer, ByVal Typ As String, ByRef CurrentVal As String, Optional Size As Integer = 1)



        Select Case Area                                      'Filter Nach Bereich Eingang, Ausgang etc.


#Region "Eingänge"


#Region "E_Bit"
            Case "E"
                'Bereich E-Bit

                Dim Result As Integer = Client.EBRead(Pos_Byte, 1, Buffer)                                                          'Lesen des kompleten Bytes

                ShowResult(Result)                                                                                                  'Anzeige des Ergebnisses vom Vorgang
#End Region

#Region "E-Byte"
            Case "EB"
                'Bereich E-Byte


                Dim Result As Integer = Client.EBRead(Pos_Byte, 1, Buffer)                                                           'Lesen des kompleten Bytes

                ShowResult(Result)                                                                                                  'Anzeige des Ergebnisses vom Vorgang

#End Region

#Region "E-Word"
            Case "EW"
                'Bereich E-Word

                Dim Result As Integer = Client.EBRead(Pos_Byte, 2, Buffer)                                                           'Lesen des kompleten Bytes

                ShowResult(Result)                                                                                                  'Anzeige des Ergebnisses vom Vorgang

#End Region


#End Region



#Region "Ausgänge"


#Region "A-Bit"
            Case "A"
                'Bereich A-Bit



                Dim Result As Integer = Client.ABRead(Pos_Byte, 1, Buffer)                                                          'Lesen des kompleten Bytes

                ShowResult(Result)                                                                                                  'Anzeige des Ergebnisses vom Vorgang

#End Region

#Region "A-Byte"
            Case "AB"
                'Bereich A-Byte


                Dim Result As Integer = Client.ABRead(Pos_Byte, 1, Buffer)                                                           'Lesen des kompleten Bytes

                ShowResult(Result)                                                                                                  'Anzeige des Ergebnisses vom Vorgang



#End Region

#Region "A-Word"
            Case "AW"
                'Bereich A-Word

                Dim Result As Integer = Client.ABRead(Pos_Byte, 2, Buffer)                                                           'Lesen des kompleten Bytes

                ShowResult(Result)                                                                                                  'Anzeige des Ergebnisses vom Vorgang


#End Region


#End Region



#Region "Merker"


#Region "M-Bit"
            Case "M"
                'Bereich M-Bit



                Dim Result As Integer = Client.MBRead(Pos_Byte, 1, Buffer)                                                          'Lesen des kompleten Bytes

                ShowResult(Result)                                                                                                  'Anzeige des Ergebnisses vom Vorgang


#End Region

#Region "M-Byte"
            Case "MB"
                'Bereich A-Byte


                Dim Result As Integer = Client.MBRead(Pos_Byte, 1, Buffer)                                                           'Lesen des kompleten Bytes

                ShowResult(Result)                                                                                                  'Anzeige des Ergebnisses vom Vorgang



#End Region

#Region "M-Word"
            Case "MW"
                'Bereich M-Word

                Dim Result As Integer = Client.MBRead(Pos_Byte, 2, Buffer)                                                           'Lesen des kompleten Bytes

                ShowResult(Result)                                                                                                  'Anzeige des Ergebnisses vom Vorgang


#End Region


#End Region



#Region "Datenbausteine"


            Case "DB"
                Dim Result As Integer = Client.AsDBRead(Pos_Byte, Pos_Bit, Size, Buffer)                                            'Lesen des kompleten Bytes
                ShowResult(Result)                                                                                                  'Anzeige des Ergebnisses vom Vorgang

#End Region


        End Select




        Select Case Typ                                     'Buffer nach angegebenen Typ Conventieren

#Region "Bit"
            Case "Bit"
                If S7.GetBitAt(Buffer, 0, Pos_Bit) = True Then CurrentVal = "1"
                If S7.GetBitAt(Buffer, 0, Pos_Bit) = False Then CurrentVal = "0"
#End Region

#Region "Byte"
            Case "Byte"
                CurrentVal = ""
                For i As Integer = 0 To 7
                    If S7.GetBitAt(Buffer, 0, i) = True Then CurrentVal = CurrentVal & "1"
                    If S7.GetBitAt(Buffer, 0, i) = False Then CurrentVal = CurrentVal & "0"
                Next
#End Region

#Region "Word"
            Case "Word"
                CurrentVal = ""
                For i As Integer = 0 To 7
                    If S7.GetBitAt(Buffer, 0, i) = True Then CurrentVal = CurrentVal & "1"
                    If S7.GetBitAt(Buffer, 0, i) = False Then CurrentVal = CurrentVal & "0"
                Next

                For i As Integer = 0 To 7
                    If S7.GetBitAt(Buffer, 1, i) = True Then CurrentVal = CurrentVal & "1"
                    If S7.GetBitAt(Buffer, 1, i) = False Then CurrentVal = CurrentVal & "0"
                Next
#End Region

#Region "DWord"
            Case "DWord"
                CurrentVal = ""
                For i As Integer = 0 To 7
                    If S7.GetBitAt(Buffer, 0, i) = True Then CurrentVal = CurrentVal & "1"
                    If S7.GetBitAt(Buffer, 0, i) = False Then CurrentVal = CurrentVal & "0"
                Next
                For i As Integer = 0 To 7
                    If S7.GetBitAt(Buffer, 1, i) = True Then CurrentVal = CurrentVal & "1"
                    If S7.GetBitAt(Buffer, 1, i) = False Then CurrentVal = CurrentVal & "0"
                Next
                For i As Integer = 0 To 7
                    If S7.GetBitAt(Buffer, 2, i) = True Then CurrentVal = CurrentVal & "1"
                    If S7.GetBitAt(Buffer, 2, i) = False Then CurrentVal = CurrentVal & "0"
                Next
                For i As Integer = 0 To 7
                    If S7.GetBitAt(Buffer, 3, i) = True Then CurrentVal = CurrentVal & "1"
                    If S7.GetBitAt(Buffer, 3, i) = False Then CurrentVal = CurrentVal & "0"
                Next
#End Region

#Region "String"
            Case "String"
                CurrentVal = S7.GetCharsAt(Buffer, 0, Size)
#End Region

#Region "Int"
            Case "Int"
                CurrentVal = S7.GetIntAt(Buffer, 0)
#End Region

#Region "DInt"
            Case "DInt"
                CurrentVal = S7.GetDIntAt(Buffer, 0)
#End Region

#Region "Real"
            Case "Real"
                CurrentVal = S7.GetRealAt(Buffer, 0)
#End Region

#Region "DateTime"
            Case "DateTime"
                CurrentVal = S7.GetDateTimeAt(Buffer, 0)
#End Region

#Region "S1200_DTLA"
            Case "S1200_DTLA"
                CurrentVal = S7.GetS1200_DTLAt(Buffer, 0)
#End Region

        End Select

    End Sub



End Module
