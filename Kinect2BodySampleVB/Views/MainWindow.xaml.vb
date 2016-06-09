Class MainWindow
    Private ViewModel As New MainViewModel

    Public Sub New()
        ' この呼び出しはデザイナーで必要です。
        InitializeComponent()

        ' InitializeComponent() 呼び出しの後で初期化を追加します。
        Me.DataContext = Me.ViewModel
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Me.ViewModel.StartCommand()
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        Me.ViewModel.StopCommand()
    End Sub
End Class