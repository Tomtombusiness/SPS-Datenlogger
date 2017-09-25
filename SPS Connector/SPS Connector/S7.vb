Imports Snap7
'Imports Snap7.S7Client
Imports Snap7.S7
Imports SPS_Connector
Imports S7_DataLogging_Manager

Public Class S7
    Implements Interface_SPS_Control

    Dim Client As S7Client                      'Client Object
    Dim Buffer(65536) As Byte                   ' Buffer  

    Public ReadOnly Property Name As String Implements Interface_SPS_Control.Name
        Get
            Return "Snap7 Driver"
        End Get
    End Property

    Public ReadOnly Property Version As String Implements Interface_SPS_Control.Version
        Get
            Return "0.0.0.1 Beta"
        End Get
    End Property

    Public ReadOnly Property Author As String Implements Interface_SPS_Control.Author
        Get
            Return "Tom Schorn"
        End Get
    End Property

    Public ReadOnly Property Website As String Implements Interface_SPS_Control.Website
        Get
            Return "www.tom-schorn.de"
        End Get
    End Property

    Public ReadOnly Property E_Mail As String Implements Interface_SPS_Control.E_Mail
        Get
            Return "Keine Angaben"
        End Get
    End Property

    Sub New()
        Client = New S7Client
    End Sub

#Region "Events"
    Public Event ErrorRaised(ByVal sender As Object, ByVal e As ErrorRaisedEventArgs)
    Private Event ErrorRaised_2 As Interface_SPS_Control.ErrorRaisedEventHandler Implements Interface_SPS_Control.ErrorRaised
    Public Event ConnectionChanged As INterface_SPS_Control.ConnectionChangedEventHandler Implements INterface_SPS_Control.ConnectionChanged
#End Region

    '    Sub SetPlcSystemDateTime()
    '   Dim Result As Integer = Client.SetPlcSystemDateTime()
    '  If Result <> 0 Then
    ' RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(Result, Client.ErrorText(Result)))
    'End If
    'End Sub




    Private Function Interface_SPS_Control_Connect(Host As String, Rack As Integer, Slot As Integer, ConnectionType As Integer) As Boolean Implements INterface_SPS_Control.Connect
        Client.SetConnectionType(ConnectionType)
        Dim Result As Integer = Client.ConnectTo(Host, Rack, Slot)
        If Result = 0 Then
            Return True
        Else
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(Result, Client.ErrorText(Result)))
            Return False
        End If
    End Function

    Private Function Interface_SPS_Control_Disconnect() As Boolean Implements INterface_SPS_Control.Disconnect
        Dim Result As Integer = Client.Disconnect()
        If Result = 0 Then
            Return True
        Else
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(Result, Client.ErrorText(Result)))
            Return False
        End If
    End Function

    Private Function Interface_SPS_Control_Connected() As Boolean Implements INterface_SPS_Control.Connected
        Return Client.Connected
    End Function

    Private Sub Interface_SPS_Control_PLCHotStart() Implements INterface_SPS_Control.PLCHotStart
        Dim Result As Integer = Client.PlcHotStart()
        If Result <> 0 Then
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(Result, Client.ErrorText(Result)))
        End If
    End Sub

    Private Sub Interface_SPS_Control_PLCColdStart() Implements INterface_SPS_Control.PLCColdStart
        Dim Result As Integer = Client.PlcColdStart
        If Result <> 0 Then
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(Result, Client.ErrorText(Result)))
        End If
    End Sub

    Private Sub Interface_SPS_Control_PLCStop() Implements INterface_SPS_Control.PLCStop
        Dim Result As Integer = Client.PlcStop
        If Result <> 0 Then
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(Result, Client.ErrorText(Result)))
        End If
    End Sub

    Private Function Interface_SPS_Control_PLCGetStatus() As Boolean Implements INterface_SPS_Control.PLCGetStatus
        Dim Buffer As Integer = 0
        Dim Result As Integer = Client.PlcGetStatus(Buffer)
        If Result = 4 Then
            Return False
        ElseIf Result = 8 Then
            Return True
        Else
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(Result, Client.ErrorText(Result)))
            Return False
        End If
    End Function

    Public Function PLCGetTime() As Date Implements INterface_SPS_Control.PLCGetTime
        Dim Buffer As Date
        Dim Result As Integer = Client.GetPlcDateTime(Buffer)
        If Result <> 0 Then
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(Result, Client.ErrorText(Result)))
        End If
        Return Buffer
    End Function

    Private Function Interface_SPS_Control_Read(Area As String, Pos_Byte As Integer, Format As String, Optional Pos_Bit As Integer = 0, Optional DB_Size As Integer = 0) As Object Implements INterface_SPS_Control.Read
        Dim Result As Integer = 0
        'Lese aus SPS in Buffer
        Select Case Area

                'Eingänge
            Case "E"
                Result = Client.EBRead(Pos_Byte, 1, Buffer)

                'Ausgänge
            Case "A"
                Result = Client.ABRead(Pos_Byte, 1, Buffer)

                'Merker
            Case "M"
                Result = Client.MBRead(Pos_Byte, 1, Buffer)

                'Datenbausteine
            Case "DB"
                Result = Client.DBRead(Pos_Byte, Pos_Bit, DB_Size, Buffer)
        End Select

        If Result <> 0 Then
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(Result, Client.ErrorText(Result)))
        End If

        If Result = 0 Then



            'Starte Conventierung ins Format und gebe Wert zurück
            Select Case Format
                Case "Bit"
                    Dim Val As Boolean = GetBitAt(Buffer, 0, Pos_Bit)
                    If Val = True Then Return "1"
                    If Val = False Then Return "0"

                Case "Byte"
                    Dim LoadedVal As String

                    For i As Integer = 0 To 7

                        If GetBitAt(Buffer, 0, i) = True Then LoadedVal = LoadedVal & "1"
                        If GetBitAt(Buffer, 0, i) = False Then LoadedVal = LoadedVal & "0"
                    Next
                    Return LoadedVal
                Case "Word"
                    Return GetDWordAt(Buffer, 0)
                Case "String"
                    Return GetCharsAt(Buffer, 0, DB_Size)
                Case "Int"
                    Return GetIntAt(Buffer, 0)
                Case "DInt"
                    Return GetDIntAt(Buffer, 0)
                Case "Real"
                    Return GetRealAt(Buffer, 0)
                Case "DateTime"
                    Return GetDateTimeAt(Buffer, 0)
                Case "S1200_DTLA"
                    Return GetS1200_DTLAt(Buffer, 0)
            End Select

            If Result <> 0 Then
                RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(Result, Client.ErrorText(Result)))
            End If
        End If
    End Function

    Private Sub Interface_SPS_Control_Write(Area As String, Format As String, Pos_Byte As Integer, Pos_Bit As Integer, Value As String, Optional DB_Size As Integer = 0) Implements INterface_SPS_Control.Write

        Dim Result As Integer = 0

        Select Case Format

            Case "Bit"
                If Value = "1" Then SetBitAt(Buffer, 0, Pos_Bit, True) Else SetBitAt(Buffer, 0, Pos_Bit, False)
            Case "Byte"
                For i As Integer = 0 To 7
                    If ((Value.ToString.ToCharArray)(i)).ToString = "1" Then SetBitAt(Buffer, 0, i, True) Else SetBitAt(Buffer, 0, i, False)
                Next
            Case "Word"
                SetDWordAt(Buffer, 0, Value)
            Case "String"
                SetCharsAt(Buffer, 0, Value)
            Case "Int"
                SetIntAt(Buffer, 0, Value)
            Case "DInt"
                SetDIntAt(Buffer, 0, Value)
            Case "Real"
                SetRealAt(Buffer, 0, Value)
            Case "DateTime"
                SetDateTimeAt(Buffer, 0, Value)
            Case "S1200_DTLA"
                SetS1200_DTLAt(Buffer, 0, Value)
        End Select

        If Result <> 0 Then
            RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(Result, Client.ErrorText(Result)))
        End If

        If Result = 0 Then
            'Starte Conventierung ins Angegebene Format und schiebe sie in die SPS
            Select Case Area
            'Eingänge
                Case "E"
                    Select Case Format
                        Case "Bit"
                            Result = Client.EBWrite(Pos_Byte, 1, Buffer)
                        Case "Byte"
                            Result = Client.EBWrite(Pos_Byte, 1, Buffer)
                        Case "Word"
                            Result = Client.EBWrite(Pos_Byte, 2, Buffer)
                        Case "String"
                            Result = Client.EBWrite(Pos_Byte, DB_Size, Buffer)
                        Case "Int"
                            Result = Client.EBWrite(Pos_Byte, 1, Buffer)
                        Case "DInt"
                            Result = Client.EBWrite(Pos_Byte, 2, Buffer)
                        Case "Real"
                            Result = Client.EBWrite(Pos_Byte, 2, Buffer)
                        Case "DateTime"
                            Result = Client.EBWrite(Pos_Byte, 2, Buffer)
                        Case "S1200_DTLA"
                            Result = Client.EBWrite(Pos_Byte, 2, Buffer)
                    End Select


                'Ausgänge
                Case "A"
                    Select Case Format
                        Case "Bit"
                            Result = Client.ABWrite(Pos_Byte, 1, Buffer)
                        Case "Byte"
                            Result = Client.ABWrite(Pos_Byte, 1, Buffer)
                        Case "Word"
                            Result = Client.ABWrite(Pos_Byte, 2, Buffer)
                        Case "String"
                            Result = Client.ABWrite(Pos_Byte, DB_Size, Buffer)
                        Case "Int"
                            Result = Client.ABWrite(Pos_Byte, 1, Buffer)
                        Case "DInt"
                            Result = Client.ABWrite(Pos_Byte, 2, Buffer)
                        Case "Real"
                            Result = Client.ABWrite(Pos_Byte, 2, Buffer)
                        Case "DateTime"
                            Result = Client.ABWrite(Pos_Byte, 2, Buffer)
                        Case "S1200_DTLA"
                            Result = Client.ABWrite(Pos_Byte, 2, Buffer)
                    End Select
                'Merker
                Case "M"
                    Select Case Format
                        Case "Bit"
                            Result = Client.MBWrite(Pos_Byte, 1, Buffer)
                        Case "Byte"
                            Result = Client.MBWrite(Pos_Byte, 1, Buffer)
                        Case "Word"
                            Result = Client.MBWrite(Pos_Byte, 2, Buffer)
                        Case "String"
                            Result = Client.MBWrite(Pos_Byte, DB_Size, Buffer)
                        Case "Int"
                            Result = Client.MBWrite(Pos_Byte, 1, Buffer)
                        Case "DInt"
                            Result = Client.MBWrite(Pos_Byte, 2, Buffer)
                        Case "Real"
                            Result = Client.MBWrite(Pos_Byte, 2, Buffer)
                        Case "DateTime"
                            Result = Client.MBWrite(Pos_Byte, 2, Buffer)
                        Case "S1200_DTLA"
                            Result = Client.MBWrite(Pos_Byte, 2, Buffer)
                    End Select
                'Datenbausteine
                Case "DB"
                    Result = Client.DBWrite(Pos_Byte, Pos_Bit, DB_Size, Buffer)
            End Select
            If Result <> 0 Then
                RaiseEvent ErrorRaised(Me, New ErrorRaisedEventArgs(Result, Client.ErrorText(Result)))
            End If
        End If


    End Sub

    Public Sub SetPlcSystemDateTime() Implements INterface_SPS_Control.SetPlcSystemDateTime
        Throw New NotImplementedException()
    End Sub
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