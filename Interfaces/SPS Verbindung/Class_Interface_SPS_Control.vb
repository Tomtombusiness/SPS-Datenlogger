Imports System.ComponentModel.Composition
Imports System.ComponentModel.Composition.Hosting
Imports System.Reflection

Public Class Class_Interface_SPS_Control
    <ImportMany(GetType(INterface_SPS_Control))>
    Public Plugins As INterface_SPS_Control()

    Dim SelectedPlugin As String = ""

    Sub New()
        Try
            Dim catalog = New DirectoryCatalog("./Addons/") 'Eine Import-Quelle angeben (gibt noch andere)
            ' catalog gibt an, dass die Plugins aus dem Verzeichnis „Plugin“ geladen werden sollen
            Dim container = New CompositionContainer(catalog) 'Quelle zum Container hinzufügen
            container.ComposeParts(Me) ' Hier passiert die Magie!
        Catch ex As ReflectionTypeLoadException
            MsgBox(ex.LoaderExceptions(0).Message)
        End Try

    End Sub

    Function GetPluginList() As List(Of String)
        Dim L As List(Of String) = New List(Of String)
        For Each Plugin As INterface_SPS_Control In Plugins
            L.Add(Plugin.Name)

        Next
        Return L
    End Function
    Sub SelectPlugin(ByVal Name As String)
        SelectedPlugin = Name
    End Sub

#Region "Events"
    Event ErrorRaised(ByVal sender As Object, ByVal e As ErrorRaisedEventArgs)
    Sub ErrorHandler(ByVal sender As Object, ByVal e As ErrorRaisedEventArgs) Handles Me.ErrorRaised
        RaiseEvent ErrorRaised(sender, e)
    End Sub
    Event ConnectionChanged(ByVal sender As Object, ByVal e As ConnectionChangedEventArgs)
    Sub ConnectionHandler(ByVal sender As Object, ByVal e As ConnectionChangedEventArgs)
        RaiseEvent ConnectionChanged(sender, e)
    End Sub

#End Region

#Region "Plugin Information"
    Function GetVersion() As String
        For Each Plugin As INterface_SPS_Control In Plugins
            Dim Result As Boolean = False
            If Plugin.Name = SelectedPlugin Then
                Return Plugin.Version
            End If
        Next
        Return "Error 0200"
    End Function
    Function GetAuthor() As String
        For Each Plugin As INterface_SPS_Control In Plugins
            Dim Result As Boolean = False
            If Plugin.Name = SelectedPlugin Then
                Return Plugin.Author
            End If
        Next
        Return "Error 0201"
    End Function
    Function GetWebsite() As String
        For Each Plugin As INterface_SPS_Control In Plugins
            Dim Result As Boolean = False
            If Plugin.Name = SelectedPlugin Then
                Return Plugin.Website
            End If
        Next
        Return "Error 0202"
    End Function
    Function GetE_Mail() As String
        For Each Plugin As INterface_SPS_Control In Plugins
            Dim Result As Boolean = False
            If Plugin.Name = SelectedPlugin Then
                Return Plugin.E_Mail
            End If
        Next
        Return "Error 0203"
    End Function
#End Region

#Region "Verbindung"
    Function Connect(ByVal Host As String, ByVal Rack As Integer, ByVal Slot As Integer, ByVal ConnectionType As Integer) As Boolean
        For Each Plugin As INterface_SPS_Control In Plugins
            Dim Result As Boolean = False
            If Plugin.Name = SelectedPlugin Then
                Return Plugin.Connect(Host, Rack, Slot, ConnectionType)
            End If
        Next
        Return "Error 0204"
    End Function
    Function Disconnect() As Boolean
        For Each Plugin As INterface_SPS_Control In Plugins
            Dim Result As Boolean = False
            If Plugin.Name = SelectedPlugin Then
                Return Plugin.Disconnect
            End If
        Next
        Return "Error 025"
    End Function
    Function Connected() As Boolean
        For Each Plugin As INterface_SPS_Control In Plugins
            Dim Result As Boolean = False
            If Plugin.Name = SelectedPlugin Then
                Return Plugin.Connected
            End If
        Next
        Return "Error 0206"
    End Function
#End Region

#Region "Statusabfrage"
    Sub PLCHotStart()
        For Each Plugin As INterface_SPS_Control In Plugins
            Dim Result As Boolean = False
            If Plugin.Name = SelectedPlugin Then
                Plugin.PLCHotStart()
            End If
        Next

    End Sub
    Sub PLCColdStart()
        For Each Plugin As INterface_SPS_Control In Plugins
            Dim Result As Boolean = False
            If Plugin.Name = SelectedPlugin Then
                Plugin.PLCColdStart()
            End If
        Next
    End Sub
    Sub PLCStop()
        For Each Plugin As INterface_SPS_Control In Plugins
            Dim Result As Boolean = False
            If Plugin.Name = SelectedPlugin Then
                Plugin.PLCStop()
            End If
        Next
    End Sub
    Function PLCGetStatus() As Boolean
        For Each Plugin As INterface_SPS_Control In Plugins
            Dim Result As Boolean = False
            If Plugin.Name = SelectedPlugin Then
                Return Plugin.PLCGetStatus
            End If
        Next
        Return False
    End Function
    Function PLCGetTime() As Date
        For Each Plugin As INterface_SPS_Control In Plugins
            Dim Result As Boolean = False
            If Plugin.Name = SelectedPlugin Then
                Return Plugin.PLCGetTime
            End If
        Next
        Return "00:00:00"
    End Function
    Sub SetPlcSystemDateTime()
        For Each Plugin As INterface_SPS_Control In Plugins
            Dim Result As Boolean = False
            If Plugin.Name = SelectedPlugin Then
                Plugin.SetPlcSystemDateTime()
            End If
        Next
    End Sub
#End Region

#Region "IO-Control"
    Function Read(ByVal Area As String, ByVal Pos_Byte As Integer, ByVal Format As String, Optional Pos_Bit As Integer = 0, Optional DB_Size As Integer = 0)
        For Each Plugin As INterface_SPS_Control In Plugins
            Dim Result As Boolean = False
            If Plugin.Name = SelectedPlugin Then
                Return Plugin.Read(Area, Pos_Byte, Format, Pos_Bit, DB_Size)
            End If
        Next
        Return "Error 0205"
    End Function
    Sub Write(ByVal Area As String, ByVal Format As String, ByVal Pos_Byte As Integer, ByVal Pos_Bit As Integer, ByVal Value As String, Optional DB_Size As Integer = 0)
        For Each Plugin As INterface_SPS_Control In Plugins
            Dim Result As Boolean = False
            If Plugin.Name = SelectedPlugin Then
                Plugin.Write(Area, Format, Pos_Byte, Pos_Bit, Value, DB_Size)
            End If
        Next
    End Sub
#End Region

End Class