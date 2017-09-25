Public Class ConnectionChangedEventArgs
    Inherits EventArgs

    Public ConnectionState As Boolean
    Public ConnectionEvent As ConnectionType
    Enum ConnectionType
        Connected = 0
        Connection_Lost = 1
        Reconnected = 2
    End Enum

End Class