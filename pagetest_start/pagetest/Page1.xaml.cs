using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Wpf.Controls;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;


namespace pagetest
{
    /// <summary>
    /// Page1.xaml の相互作用ロジック
    /// </summary>
    /// 

    public partial class Page1 : Page
    {
        int i=0;
        private System.Media.SoundPlayer player = null;
        string SoundFile = "futta-dream.wav";
        private void PlaySound()
        {
            player = new System.Media.SoundPlayer(SoundFile);
            player.Play();
        }
        void InitializeKinectControls()
        {
            KinectRegion.SetKinectRegion(this, kinectRegion);
            this.kinectRegion.KinectSensor = KinectSensor.GetDefault();
        }
        public Page1()
        {
            InitializeComponent();
        }

        
        private void btnNavigateWithObject_Click(object sender, RoutedEventArgs e)
        {
            
            if(button1.Background== System.Windows.Media.Brushes.Blue || button2.Background == System.Windows.Media.Brushes.Blue || button3.Background == System.Windows.Media.Brushes.Blue || button4.Background == System.Windows.Media.Brushes.Blue)
            {
                var nextPage = new Page2(i);
                NavigationService.Navigate(nextPage);
                
            }
            else
            {
                System.Windows.MessageBox.Show("人数が選択されてないよ！！！！");
                PlaySound();
            }
                    
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            button1.Background = System.Windows.Media.Brushes.Blue ;
            button2.Background = System.Windows.Media.Brushes.LightGray;
            button3.Background = System.Windows.Media.Brushes.LightGray;
            button4.Background = System.Windows.Media.Brushes.LightGray;
            i = 1;
        }
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            button2.Background = System.Windows.Media.Brushes.Blue;
            button1.Background = System.Windows.Media.Brushes.LightGray;
            button3.Background = System.Windows.Media.Brushes.LightGray;
            button4.Background = System.Windows.Media.Brushes.LightGray;
            i = 2;
        }
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            button3.Background = System.Windows.Media.Brushes.Blue;
            button2.Background = System.Windows.Media.Brushes.LightGray;
            button1.Background = System.Windows.Media.Brushes.LightGray;
            button4.Background = System.Windows.Media.Brushes.LightGray;
            i = 3;
        }
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            button4.Background = System.Windows.Media.Brushes.Blue;
            button2.Background = System.Windows.Media.Brushes.LightGray;
            button3.Background = System.Windows.Media.Brushes.LightGray;
            button1.Background = System.Windows.Media.Brushes.LightGray;
            i = 4;
        }
    }

    public class DragDropDecorator : Decorator, IKinectControl
    {
        public bool IsManipulatable
        {
            get
            {
                return true;
            }
        }

        public bool IsPressable
        {
            get
            {
                return false;
            }
        }

        public IKinectController CreateController(IInputModel inputModel, KinectRegion kinectRegion)
        {
            return new DragDropDecoratorController(inputModel, kinectRegion);
        }
    }

    class DragDropDecoratorController : IKinectManipulatableController
    {
        private ManipulatableModel _inputModel;
        private KinectRegion _kinectRegion;
        private DragDropDecorator _dragDropDecorator;
        bool _disposed = false;

        //コンストラクタ
        public DragDropDecoratorController(IInputModel inputModel, KinectRegion kinectRegion)
        {
            _inputModel = inputModel as ManipulatableModel;
            _kinectRegion = kinectRegion;
            _dragDropDecorator = inputModel.Element as DragDropDecorator;
            _inputModel.ManipulationUpdated += inputModel_ManipulationUpdated;
        }

        //DragDropDecoratorのCanvas.Top,Canvas.Leftプロパティを更新
        void inputModel_ManipulationUpdated(object sender, Microsoft.Kinect.Input.KinectManipulationUpdatedEventArgs e)
        {
            //DragDropDecoratorの親はCanvas
            Canvas canvas = _dragDropDecorator.Parent as Canvas;
            if (canvas != null)
            {
                double y = Canvas.GetTop(_dragDropDecorator);
                double x = Canvas.GetLeft(_dragDropDecorator);
                if (double.IsNaN(x) || double.IsNaN(y))
                    return;
                Microsoft.Kinect.PointF delta = e.Delta.Translation;
                //deltaは-1.0..1.0の相対値で表されているので
                //KinectRegionに合わせて拡大
                var Dy = delta.Y * _kinectRegion.ActualHeight;
                var Dx = delta.X * _kinectRegion.ActualWidth;
                Canvas.SetTop(_dragDropDecorator, y + Dy);
                Canvas.SetLeft(_dragDropDecorator, x + Dx);
            }
        }

        FrameworkElement IKinectController.Element
        {
            get
            {
                return _inputModel.Element as FrameworkElement;
            }
        }

        ManipulatableModel IKinectManipulatableController.ManipulatableInputModel
        {
            get
            {
                return _inputModel;
            }
        }

        void System.IDisposable.Dispose()
        {
            if (!_disposed)
            {
                _kinectRegion = null;
                _inputModel = null;
                _dragDropDecorator = null;
                _inputModel.ManipulationUpdated -= inputModel_ManipulationUpdated;
                _disposed = true;
            }
        }
    }
}