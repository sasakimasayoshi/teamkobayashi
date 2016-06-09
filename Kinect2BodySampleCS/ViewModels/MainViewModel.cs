using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Kinect2BodySampleCS
{
    class MainViewModel : INotifyPropertyChanged
    {
        private KinectModel Model = new KinectModel();

        public MainViewModel()
        {
            this.Model.PropertyChanged += Model_PropertyChanged;
        }

        public string StatusElement
        {
            get { return this.Model.StatusElement; }
        }

        public ImageSource BodyImageElement
        {
            get { return this.Model.BodyImageElement; }
            set { this.Model.BodyImageElement = value; }
        }

        #region "command"
        public void StartCommand()
        {
            this.Model.Start();
        }

        public void StopCommand()
        {
            this.Model.Stop();
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }
    }
}
