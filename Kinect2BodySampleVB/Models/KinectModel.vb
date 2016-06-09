Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports Microsoft.Kinect

Public Class KinectModel
    Implements INotifyPropertyChanged

#Region "メンバー変数"
    Private WithEvents Kinect As KinectSensor
    Private WithEvents Reader As BodyFrameReader

    Private ReadOnly BodyBrushes As Brush() = New Brush() {Brushes.White,
                                                            Brushes.Crimson,
                                                            Brushes.Indigo,
                                                            Brushes.DodgerBlue,
                                                            Brushes.Yellow,
                                                            Brushes.Pink}
    Private Bodies As Body()
    Private BodyDrawingGroup As New DrawingGroup
    Private BodyImageBitmapRect As Rect
#End Region

#Region "プロパティ"
    Private _StatusElement As String
    Public Property StatusElement As String
        Get
            Return Me._StatusElement
        End Get
        Set(value As String)
            Me._StatusElement = value
            OnPropertyChanged()
        End Set
    End Property

    Private _BodyImageElement As ImageSource
    Public Property BodyImageElement As ImageSource
        Get
            Return Me._BodyImageElement
        End Get
        Set(value As ImageSource)
            Me._BodyImageElement = value
            OnPropertyChanged()
        End Set
    End Property
#End Region

#Region "メソッド"
    Public Sub Start()
        DiscoverKinectSensor()
    End Sub

    Public Sub [Stop]()
        If Me.Kinect IsNot Nothing Then
            Me.StatusElement = "Stop"
            Me.Reader.Dispose()
            Me.BodyImageElement = Nothing
            Me.Kinect = Nothing
        End If
    End Sub
#End Region

#Region "イベント"
    Public Event PropertyChanged(sender As Object,
                     e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged
    Protected Sub OnPropertyChanged(<CallerMemberName> Optional propertyName As String = Nothing)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
#End Region

    Private Sub DiscoverKinectSensor()
        Me.Kinect = KinectSensor.KinectSensors.FirstOrDefault(
            Function(x)
                Return x.Status = KinectStatus.Connected
            End Function)
        If Me.Kinect IsNot Nothing Then
            Dim depthDesc = Me.Kinect.DepthFrameSource.FrameDescription

            Me.Kinect.Open()
            Me.Reader = Me.Kinect.BodyFrameSource.OpenReader
            ReDim Me.Bodies(Me.Kinect.BodyFrameSource.BodyCount - 1)
            Me.BodyImageBitmapRect = New Rect(0,
                                              0,
                                              depthDesc.Width,
                                              depthDesc.Height)
        End If
    End Sub

    Private Sub Reader_FrameArrived(sender As Object,
                                    e As BodyFrameArrivedEventArgs) Handles Reader.FrameArrived
        Dim frameReference = e.FrameReference()

        Using frame = frameReference.AcquireFrame
            If frame IsNot Nothing Then
                Using dc = Me.BodyDrawingGroup.Open
                    dc.DrawRectangle(Brushes.Black, Nothing, Me.BodyImageBitmapRect)
                    frame.GetAndRefreshBodyData(Me.Bodies)
                    For index As Integer = 0 To Me.Bodies.Length - 1
                        If Me.Bodies(index).IsTracked Then
                            Dim joints As IReadOnlyDictionary(Of JointType, Joint) = Me.Bodies(index).Joints
                            Dim points As New Dictionary(Of JointType, Point)
                            For Each joint In joints.Keys
                                Dim pos = Me.Kinect.CoordinateMapper.MapCameraPointToDepthSpace(joints(joint).Position)
                                points(joint) = New Point(pos.X, pos.Y)
                            Next
                            DrawBody(joints, points, BodyBrushes(index), dc)
                        End If
                    Next
                    Me.BodyDrawingGroup.ClipGeometry = New RectangleGeometry(Me.BodyImageBitmapRect)
                    Me.BodyImageElement = New DrawingImage(Me.BodyDrawingGroup)
                End Using
            End If
        End Using
    End Sub

    Private Sub DrawBody(joints As IReadOnlyDictionary(Of JointType, Joint),
                         jointPoints As Dictionary(Of JointType, Point),
                         bodyBrash As Brush,
                         dc As DrawingContext)
        ' Draw the bones

        ' Torso
        Me.DrawBone(joints, jointPoints, JointType.Head, JointType.Neck, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.Neck, JointType.SpineShoulder, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.SpineMid, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.SpineMid, JointType.SpineBase, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.ShoulderRight, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.ShoulderLeft, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.SpineBase, JointType.HipRight, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.SpineBase, JointType.HipLeft, bodyBrash, dc)

        ' Right Arm    
        Me.DrawBone(joints, jointPoints, JointType.ShoulderRight, JointType.ElbowRight, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.ElbowRight, JointType.WristRight, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.WristRight, JointType.HandRight, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.HandRight, JointType.HandTipRight, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.WristRight, JointType.ThumbRight, bodyBrash, dc)

        ' Left Arm
        Me.DrawBone(joints, jointPoints, JointType.ShoulderLeft, JointType.ElbowLeft, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.ElbowLeft, JointType.WristLeft, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.WristLeft, JointType.HandLeft, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.HandLeft, JointType.HandTipLeft, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.WristLeft, JointType.ThumbLeft, bodyBrash, dc)

        ' Right Leg
        Me.DrawBone(joints, jointPoints, JointType.HipRight, JointType.KneeRight, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.KneeRight, JointType.AnkleRight, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.AnkleRight, JointType.FootRight, bodyBrash, dc)

        ' Left Leg
        Me.DrawBone(joints, jointPoints, JointType.HipLeft, JointType.KneeLeft, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.KneeLeft, JointType.AnkleLeft, bodyBrash, dc)
        Me.DrawBone(joints, jointPoints, JointType.AnkleLeft, JointType.FootLeft, bodyBrash, dc)

        ' Draw the joints
        For Each jointType As JointType In joints.Keys
            Dim drawBrush As Brush = Nothing
            Dim state As TrackingState = joints(jointType).TrackingState

            If (state = TrackingState.Tracked) Then
                drawBrush = Brushes.Red
            ElseIf (state = TrackingState.Inferred) Then
                drawBrush = Brushes.Red
            End If

            If (drawBrush IsNot Nothing) Then
                dc.DrawEllipse(drawBrush,
                               Nothing,
                               jointPoints(jointType),
                               3,
                               3)
            End If
        Next
    End Sub

    Private Sub DrawBone(joints As IReadOnlyDictionary(Of JointType, Joint),
                         jointPoints As Dictionary(Of JointType, Point),
                         startJointType As JointType,
                         endJointType As JointType,
                         bodyBrash As Brush,
                         dc As DrawingContext)
        Dim startJoint = joints(startJointType)
        Dim endJoint = joints(endJointType)

        If (startJoint.TrackingState = TrackingState.NotTracked OrElse
            endJoint.TrackingState = TrackingState.NotTracked) Then
            Exit Sub
        End If

        If (startJoint.TrackingState = TrackingState.Inferred AndAlso
                endJoint.TrackingState = TrackingState.Inferred) Then
            Exit Sub
        End If

        dc.DrawLine(New Pen(bodyBrash, 6), jointPoints(startJointType), jointPoints(endJointType))
    End Sub
End Class

