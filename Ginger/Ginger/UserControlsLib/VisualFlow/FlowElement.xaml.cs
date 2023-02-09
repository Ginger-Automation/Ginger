#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.UserControls;
using GingerCore.Actions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Ginger.UserControlsLib.VisualFlow
{
    /// <summary>
    /// Interaction logic for FlowElem.xaml
    /// </summary>
    public partial class FlowElement : UserControl
    {
        // Any object this element represent
        public object Object { get; set; }
        public eElementType ElementType { get; set; }
        public Visibility OtherInfoVisibility
        {
            get { return OtherInfo.Visibility; }
            set { OtherInfo.Visibility = value; }
        }
        public enum eElementType
        {
            Activity,
            Action,
            FlowControl,
            Start,
            End,
            Screenshot,
            CustomeShape//TODO : This is not the correct element type, need to make it in more generic way.
            //TODO: add Data - when we save a var
            // Add Action
            // Add Activity of error handler
        }

        public Frame GetCustomeShape()
        {
             return CustomeShape; 
        }

        public FlowElement(eElementType ElementType, string Caption, double left, double top)
        {
            this.ElementType = ElementType;
            Init(left, top);
            CaptionLabel.Content = Caption;
        }

        public FlowElement(eElementType ElementType, object obj, string prop, double left, double top)
        {
            this.ElementType = ElementType;
            Init(left, top);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(CaptionLabel, ContentProperty, obj, prop, BindingMode.OneWay);
        }
        public FlowElement(eElementType ElementType, Page f,double left, double top,double width,double height)
        {
            this.ElementType = ElementType;
            Init(left, top,width,height);
            CustomeShape.Content = f;
        }

        void Init(double left, double top,double width=300,double height=150)
        {
            InitializeComponent();

            FEImage.Source = ImageMakerControl.GetImageSource(eImageType.Refresh, width: 25);
            Width = width;
            Height = height;
            Margin = new Thickness(left, top, 0, 0);
            Highlighter.Visibility = Visibility.Collapsed;

            IDLabel.Visibility = Visibility.Collapsed;
            HoverInfoLabel.Visibility = Visibility.Collapsed;

            ActivityShape.Visibility = System.Windows.Visibility.Collapsed;
            ActionShape.Visibility = System.Windows.Visibility.Collapsed;
            FlowControlShape.Visibility = System.Windows.Visibility.Collapsed;
            StartEndShape.Visibility = System.Windows.Visibility.Collapsed;
            ScreenShotShape.Visibility = System.Windows.Visibility.Collapsed;
            CustomeShape.Visibility = System.Windows.Visibility.Collapsed;
            switch (ElementType)
            {
                case eElementType.Activity:
                    ActivityShape.Visibility = System.Windows.Visibility.Visible;
                    break;
                case eElementType.Action:
                    ActionShape.Visibility = System.Windows.Visibility.Visible;
                    break;
                case eElementType.FlowControl:
                    FlowControlShape.Visibility = System.Windows.Visibility.Visible;
                    break;
                case eElementType.Start:
                case eElementType.End:
                    StartEndShape.Visibility = System.Windows.Visibility.Visible;
                    break;
                case eElementType.Screenshot:
                    ScreenShotShape.Visibility = Visibility.Visible;
                    break;
                case eElementType.CustomeShape:
                    SetHighLight(false);
                    CustomeShape.Visibility = Visibility.Visible;                    
                    break;
                default:
                    ActivityShape.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
        }

        //TODO: need to use bind
        private string mID;
        public string ID { get { return mID; } set { mID = value; IDLabel.Content = value; IDLabel.Visibility = Visibility.Visible; } }

        public void BindStatusLabel(object obj, string prop)
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(StatusLabel, ContentProperty, obj, prop, BindingMode.OneWay);
        }

        public void SetImage(System.Drawing.Image img)
        {            
            BitmapImage logo = new BitmapImage();
            logo.BeginInit();            
            //TODO: use the image from above
            logo.UriSource = new Uri(@"/Images/" + "@Agent_32x32.png", UriKind.RelativeOrAbsolute);
            logo.EndInit();

            FEImage.Source = logo;
        }

        public Point TopConnectorPoint;

        public void SetHighLight(bool visible)
        {
            if (visible)
            {
                Highlighter.Visibility = Visibility.Visible;
                EditButton.Visibility = Visibility.Visible;
                RunButton.Visibility = Visibility.Visible;

                HoverInfoLabel.Visibility = Visibility.Visible;
                SetConnectorsVisibility(Visibility.Visible);
                SetResizersVisibility(Visibility.Visible);            
            }
            else
            {
                Highlighter.Visibility = Visibility.Collapsed;
                EditButton.Visibility = Visibility.Collapsed;
                RunButton.Visibility = Visibility.Collapsed;

                HoverInfoLabel.Visibility = Visibility.Collapsed;
                SetConnectorsVisibility(Visibility.Collapsed);
                SetResizersVisibility(Visibility.Collapsed);             
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
        }

        internal Point GetConnectorPointByTagName(string destinationConnectorName)
        {
            foreach (UIElement e in MainGrid.Children)
            {
                if (e is System.Windows.Shapes.Rectangle)
                {
                    System.Windows.Shapes.Rectangle r = (System.Windows.Shapes.Rectangle)e;
                    if ((string)r.Tag == destinationConnectorName)
                    {
                        // Get X,Y of r relative to the FlowElemnet we are on    
                        //TODO: replace with grid col 0 width and grid row 0 height
                        double left = LeftConnector.Width  + r.Margin.Left + r.Width / 2;
                        double top = TopConnector.Height + r.Margin.Top + r.Height /2;
                        return new Point(left, top);
                    }
                }
            }

            throw new Exception("Connector not found - " + destinationConnectorName);
        }

        private void RunActionButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: if typeof
            Act act = (Act)this.Object;
        }

        private void SetConnectorsVisibility(Visibility visible)
        {
            TopConnector.Visibility = visible;
            LeftConnector.Visibility = visible;
            BottomConnector.Visibility = visible;
            RightConnector.Visibility = visible;
        }

        private void SetResizersVisibility(Visibility visible)
        {
            TopLeftResizer.Visibility = visible;
            TopRightResizer.Visibility = visible;
            BottomLeftResizer.Visibility = visible;
            BottomRightResizer.Visibility = visible;
        }
    }
}
