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