Public Interface INterface_SPS_Control


#Region "Events"
    Event ErrorRaised(ByVal sender As Object, ByVal e As ErrorRaisedEventArgs)
    Event ConnectionChanged(ByVal sender As Object, ByVal e As ConnectionChangedEventArgs)
#End Region

#Region "Plugin Informationen"
    ReadOnly Property Name() As String
    ReadOnly Property Version() As String
    ReadOnly Property Author() As String
    ReadOnly Property Website() As String
    ReadOnly Property E_Mail() As String
#End Region

#Region "Verbindung"
    Function Connect(ByVal Host As String, ByVal Rack As Integer, ByVal Slot As Integer, ByVal ConnectionType As Integer) As Boolean
    Function Disconnect() As Boolean
    Function Connected() As Boolean
#End Region

#Region "Steuerungsabfrage"
    Sub PLCHotStart()
    Sub PLCColdStart()
    Sub PLCStop()

    Function PLCGetStatus() As Boolean
    Function PLCGetTime() As Date
    Sub SetPlcSystemDateTime()
#End Region

#Region "IO-Control"
    Function Read(ByVal Area As String, ByVal Pos_Byte As Integer, ByVal Format As String, Optional Pos_Bit As Integer = 0, Optional DB_Size As Integer = 0)
    Sub Write(ByVal Area As String, ByVal Format As String, ByVal Pos_Byte As Integer, ByVal Pos_Bit As Integer, ByVal Value As String, Optional DB_Size As Integer = 0)
#End Region

End Interface