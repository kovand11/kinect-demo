﻿using System;
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
using System.Collections.ObjectModel;
using Microsoft.Kinect;
using KinectDemo.UIElements;
using System.ComponentModel;
using MathNet.Numerics.LinearAlgebra;
using KinectDemo.Util;
using System.Windows.Media.Media3D;
using MathNet.Numerics.LinearAlgebra.Double;
namespace KinectDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private WorkspaceControl workspaceControl;

        private WorkspaceView activeWorkspace;

        // Displays color image on which to select workspaces
        private CameraWorkspace cameraWorkspace;

        // Displays point clouds
        private CloudView workspaceCloudView;

        // Displays skeleton model and indicates active workspace
        private BodyView bodyView;

        private List<Workspace> workspaceList = new List<Workspace>();

        private const int MARGIN = 5;

        private string statusText = null;

        private KinectSensor kinectSensor;

        public MainWindow()
        {
            InitializeComponent();

            addWorkspaceControl();

            addCameraWorkspace();

            this.kinectSensor = KinectSensor.GetDefault();

            workspaceCloudView = new CloudView(this.kinectSensor);

            workspacePointCloudHolder.Children.Add(workspaceCloudView);

            bodyView = new BodyView(this.kinectSensor);

            handCheck_BodyViewHolder.Children.Add(bodyView);


            
        }

        private void addCameraWorkspace()
        {
            cameraWorkspace = new CameraWorkspace(KinectSensor.GetDefault());
            cameraWorkspace.MouseLeftButtonDown += cameraWorkspace_MouseLeftButtonDown;

            cameraHolder.Children.Add(cameraWorkspace);
        }

        private void addWorkspaceControl()
        {
            workspaceControl = new WorkspaceControl();
            workspaceControl.AddButton.Click += addButton_Click;
            workspaceControl.DeleteButton.Click += deleteButton_Click;
            workspaceControl.setSource(new Workspace());
            workspaceControl.Margin = new Thickness(MARGIN);

            listHolder.Children.Add(workspaceControl);
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            listHolder.Children.Remove(activeWorkspace);
            workspaceControl.Mode = WorkspaceControl.WorkspaceControlMode.Add;
            workspaceControl.setSource(new Workspace());
        }


        void addButton_Click(object sender, RoutedEventArgs e)
        {
            
            // New Workspace added
            if (workspaceControl.Mode == WorkspaceControl.WorkspaceControlMode.Add)
            {
                workspaceList.Add(workspaceControl.Workspace);
                WorkspaceView workspaceView = new WorkspaceView(workspaceControl.Workspace);
                workspaceView.wsName.MouseDown += selectWorkspace;
                workspaceView.Margin = new Thickness(MARGIN);
                workspaceView.DeleteButton.Click += deleteWorkspace;
                this.listHolder.Children.Add(workspaceView);
            }
            // Existing Workspace edited
            else
            {
                workspaceControl.Mode = WorkspaceControl.WorkspaceControlMode.Add;
                workspaceCloudView.updatePointCloudAndCenter();
            }
            workspaceControl.setSource(new Workspace());
        }

       

        private void deleteWorkspace(object sender, RoutedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent((Button) sender);
            while (!(parent is WorkspaceView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            WorkspaceView workspaceView = ((WorkspaceView)parent);
            ((Panel)(workspaceView.Parent)).Children.Remove(workspaceView);

            workspaceList.Remove(activeWorkspace.Workspace);

            workspaceControl.Mode = WorkspaceControl.WorkspaceControlMode.Add;
            workspaceControl.setSource(new Workspace());

        }

        void selectWorkspace(object sender, MouseButtonEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent((TextBlock)sender);
            while (!(parent is WorkspaceView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            activeWorkspace = (WorkspaceView)parent;
            workspaceControl.setSource(activeWorkspace.Workspace);

            workspaceControl.Mode = WorkspaceControl.WorkspaceControlMode.Edit;

            this.workspaceCloudView.setWorkspace(activeWorkspace.Workspace);
        }

        private void cameraWorkspace_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (FocusManager.GetFocusedElement(this) is TextBox)
            {
                TextBox focusedTextBox = (TextBox)FocusManager.GetFocusedElement(this);
                if (focusedTextBox != null)
                {
                    // Get Depth coordinates from clicked point
                    double actualWidth = this.cameraWorkspace.ActualWidth;
                    double actualHeight = this.cameraWorkspace.ActualHeight;
                    
                    double x = e.GetPosition(this.cameraWorkspace).X;
                    double y = e.GetPosition(this.cameraWorkspace).Y;

                    int depthWidth = cameraWorkspace.depthFrameSize[0];
                    int depthHeight = cameraWorkspace.depthFrameSize[1];

                    focusedTextBox.Text = (int)((x / actualWidth) * depthWidth) + "," + (int)((y / actualHeight) * depthHeight);
                }
                
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                focusedTextBox.MoveFocus(tRequest);
            }
        }

        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        private void HandCheck_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bodyView.workspaceList = this.workspaceList;
        }
    }
}