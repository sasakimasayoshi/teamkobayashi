using Microsoft.Kinect;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Kinect2BodySampleCS
{
    class KinectModel : INotifyPropertyChanged
    {
        #region "メンバー変数"
        private KinectSensor Kinect;
        private BodyFrameReader Reader;

        private readonly Brush[] BodyBrushes = new Brush[] {Brushes.White,
                                                            Brushes.Crimson,
                                                            Brushes.Indigo,
                                                            Brushes.DodgerBlue,
                                                            Brushes.Yellow,
                                                            Brushes.Pink};
        private Body[] Bodies;
        private DrawingGroup BodyDrawingGroup = new DrawingGroup();
        private Rect BodyImageBitmapRect;
        #endregion

        #region "プロパティ"
        private string _StatusElement;
        public string StatusElement
        {
            get { return this._StatusElement; }
            set
            {
                this._StatusElement = value;
                OnPropertyChanged();
            }
        }

        private ImageSource _BodyImageElement;
        public ImageSource BodyImageElement
        {
            get { return this._BodyImageElement; }
            set
            {
                this._BodyImageElement = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region "メソッド"
        public void Start()
        {
            DiscoverKinectSensor();
        }

        public void Stop()
        {
            if (this.Kinect != null)
            {
                this.StatusElement = "Stop";
                this.Reader.Dispose();
                this.BodyImageElement = null;
                this.Kinect = null;
            }
        }
        #endregion

        #region "イベント"
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void DiscoverKinectSensor()
        {
            this.Kinect = KinectSensor.GetDefault();
            //this.Kinect = KinectSensor.KinectSensors.FirstOrDefault((x) =>
            //{
            //    return x.Status == KinectStatus.Connected;
            //});

            if (this.Kinect != null)
            {
                var depthDesc = this.Kinect.DepthFrameSource.FrameDescription;

                this.Kinect.Open();
                this.Reader = this.Kinect.BodyFrameSource.OpenReader();
                this.Reader.FrameArrived += Reader_FrameArrived;
                this.Bodies = new Body[this.Kinect.BodyFrameSource.BodyCount];
                this.BodyImageBitmapRect = new Rect(0,
                                                    0,
                                                    depthDesc.Width,
                                                    depthDesc.Height);
            }
        }

        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            var frameReference = e.FrameReference;

            using (var frame = frameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    using (var dc = this.BodyDrawingGroup.Open())
                    {
                        dc.DrawRectangle(Brushes.Black, null, this.BodyImageBitmapRect);
                        frame.GetAndRefreshBodyData(this.Bodies);
                        for (int index = 0; index <= this.Bodies.Length - 1; index++)
                        {
                            if (this.Bodies[index].IsTracked)
                            {
                                IReadOnlyDictionary<JointType, Joint> joints = this.Bodies[index].Joints;
                                Dictionary<JointType, Point> points = new Dictionary<JointType, Point>();
                                foreach (var joint in joints.Keys)
                                {
                                    var pos = this.Kinect.CoordinateMapper.MapCameraPointToDepthSpace(joints[joint].Position);
                                    points[joint] = new Point(pos.X, pos.Y);
                                }
                                DrawBody(joints, points, BodyBrushes[index], dc);
                            }
                        }
                        this.BodyDrawingGroup.ClipGeometry = new RectangleGeometry(this.BodyImageBitmapRect);
                        this.BodyImageElement = new DrawingImage(this.BodyDrawingGroup);
                    }
                }
            }
        }

        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints,
                              Dictionary<JointType, Point> jointPoints,
                              Brush bodyBrash,
                              DrawingContext dc)
        {
            /* Draw the bones */

            /* Torso */
            this.DrawBone(joints, jointPoints, JointType.Head, JointType.Neck, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.Neck, JointType.SpineShoulder, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.SpineMid, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.SpineMid, JointType.SpineBase, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.ShoulderRight, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.ShoulderLeft, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.SpineBase, JointType.HipRight, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.SpineBase, JointType.HipLeft, bodyBrash, dc);

            /* Right Arm    */
            this.DrawBone(joints, jointPoints, JointType.ShoulderRight, JointType.ElbowRight, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.ElbowRight, JointType.WristRight, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.WristRight, JointType.HandRight, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.HandRight, JointType.HandTipRight, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.WristRight, JointType.ThumbRight, bodyBrash, dc);

            /* Left Arm */
            this.DrawBone(joints, jointPoints, JointType.ShoulderLeft, JointType.ElbowLeft, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.ElbowLeft, JointType.WristLeft, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.WristLeft, JointType.HandLeft, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.HandLeft, JointType.HandTipLeft, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.WristLeft, JointType.ThumbLeft, bodyBrash, dc);

            /* Right Leg */
            this.DrawBone(joints, jointPoints, JointType.HipRight, JointType.KneeRight, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.KneeRight, JointType.AnkleRight, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.AnkleRight, JointType.FootRight, bodyBrash, dc);

            /* Left Leg */
            this.DrawBone(joints, jointPoints, JointType.HipLeft, JointType.KneeLeft, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.KneeLeft, JointType.AnkleLeft, bodyBrash, dc);
            this.DrawBone(joints, jointPoints, JointType.AnkleLeft, JointType.FootLeft, bodyBrash, dc);

            /* Draw the joints */
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;
                TrackingState state = joints[jointType].TrackingState;

                if (state == TrackingState.Tracked)
                {
                    drawBrush = Brushes.Red;
                }
                else if (state == TrackingState.Inferred)
                {
                    drawBrush = Brushes.Red;
                }

                if (drawBrush != null)
                {
                    dc.DrawEllipse(drawBrush,
                                   null,
                                   jointPoints[jointType],
                                   3,
                                   3);
                }
            }
        }

        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints,
                              Dictionary<JointType, Point> jointPoints,
                              JointType startJointType,
                              JointType endJointType,
                              Brush bodyBrash,
                              DrawingContext dc)
        {
            var startJoint = joints[startJointType];
            var endJoint = joints[endJointType];

            if (startJoint.TrackingState == TrackingState.NotTracked ||
                endJoint.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            if (startJoint.TrackingState == TrackingState.Inferred &&
                    endJoint.TrackingState == TrackingState.Inferred)
            {
                return;
            }

            dc.DrawLine(new Pen(bodyBrash, 6), jointPoints[startJointType], jointPoints[endJointType]);
        }
    }
}
