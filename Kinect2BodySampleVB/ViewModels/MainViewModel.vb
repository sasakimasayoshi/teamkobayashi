Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Class MainViewModel
    Implements INotifyPropertyChanged

    Private WithEvents Model As New KinectModel

    Public ReadOnly Property StatusElement As String
        Get
            Return Me.Model.StatusElement
        End Get
    End Property

    Public Property BodyImageElement As ImageSource
        Get
            Return Me.Model.BodyImageElement
        End Get
        Set(value As ImageSource)
            Me.Model.BodyImageElement = value
        End Set
    End Property

#Region "Command"
    Public Sub StartCommand()
        Me.Model.Start()
    End Sub

    Public Sub StopCommand()
        Me.Model.Stop()
    End Sub
#End Region

    Public Event PropertyChanged(sender As Object,
                         e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged
    Protected Sub OnPropertyChanged(<CallerMemberName> Optional propertyName As String = Nothing)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Private Sub Model_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles Model.PropertyChanged
        OnPropertyChanged(e.PropertyName)
    End Sub
End Class
