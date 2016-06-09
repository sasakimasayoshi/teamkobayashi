using System.ComponentModel;
using System.Windows;

namespace Kinect2BodySampleCS
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.StartCommand();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.ViewModel.StopCommand();
        }
    }
}
