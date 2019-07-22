#region License
/*
Copyright © 2014-2019 European Support Limited

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

using Amdocs.Ginger.Common.InterfacesLib;
using Ginger.UserControlsLib.VisualFlow;
using GingerCore;
using GingerCore.Actions;
using GingerCore.FlowControlLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Ginger.BusinessFlowLib
{
    /// <summary>
    /// Interaction logic for BusinessFlowDiagramPage.xaml
    /// </summary>
    public partial class BusinessFlowDiagramPage : Page
    {
        BusinessFlow mBusinessFlow;
        FlowDiagramPage mFlowDiagram;
        FlowElement StartFlowElement;
        FlowElement EndFlowElement;

        public BusinessFlowDiagramPage(BusinessFlow BF)
        {
            mBusinessFlow = BF;
            InitializeComponent();
            int i = BF.Activities.Count;   //temp code remove later !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            
            //TODO: if big flow takes time to load then show loading message

            CreateBFFlowDiagram();            
        }

        private void CreateBFFlowDiagram()
        {
            mFlowDiagram = new FlowDiagramPage();

            double x = 50;
            double y = 50;
            // Show start elem
            StartFlowElement = new FlowElement(FlowElement.eElementType.Start, "Start", x, y);
            x += StartFlowElement.Width + 50;
            mFlowDiagram.AddFlowElem(StartFlowElement);
            y += 100;

            FlowElement LastActivity = StartFlowElement;
            foreach (Activity activity in mBusinessFlow.Activities)
            {
                FlowElement ActivityFE = new FlowElement(FlowElement.eElementType.Activity, activity, nameof(Activity.ActivityName), x, y);
                y += ActivityFE.Height + 50;

                //TODO: fixme to see image
                //ActivityFE.SetImage(activity.Image);

                ActivityFE.Object = activity;  // keep ref to the Activity for later use
                ActivityFE.BindStatusLabel(activity, nameof(Activity.Status));
                mFlowDiagram.AddFlowElem(ActivityFE);

                //Add connector to last activity
                FlowLink FL = new FlowLink(LastActivity, ActivityFE);
                FL.LinkStyle = FlowLink.eLinkStyle.Arrow;
                FL.SourcePosition = FlowLink.eFlowElementPosition.bottom;
                FL.DestinationPosition = FlowLink.eFlowElementPosition.Top;
                mFlowDiagram.AddConnector(FL);
                LastActivity = ActivityFE;

            }

            // Add 'End' Flow elem
            x += LastActivity.Width + 50;
            EndFlowElement = new FlowElement(FlowElement.eElementType.End, "End", x, y);
            mFlowDiagram.AddFlowElem(EndFlowElement);

            // connect last Activity to 'End' Flow Elem
            FlowLink FLEnd = new FlowLink(LastActivity, EndFlowElement);
            FLEnd.LinkStyle = FlowLink.eLinkStyle.Arrow;
            FLEnd.SourcePosition = FlowLink.eFlowElementPosition.bottom;
            FLEnd.DestinationPosition = FlowLink.eFlowElementPosition.Top;
            mFlowDiagram.AddConnector(FLEnd);

            AddFlowControls();

            MainFrame.Content = mFlowDiagram;
        }
        
        void AddFlowControls()
        {
            foreach (Activity a in mBusinessFlow.Activities)
            {
                foreach (Act act in a.Acts)
                {
                    foreach (FlowControl FC in act.FlowControls)
                    {   
                        
                        FlowElement FlowControlFE = new FlowElement(FlowElement.eElementType.FlowControl, act.Description + Environment.NewLine + FC.Condition + "?", 200, 300);

                        //TODO: fix me cause problem when status = 0 !!!!!!!!!!!!!!!!
                        // FlowControlFE.BindStatusLabel(FC, FlowControl.Fields.Status);
                        
                        mFlowDiagram.AddFlowElem(FlowControlFE);
                        
                        // Create Line connector to the activity this FC is in
                        FlowLink FL2 = new FlowLink(GetFlowElemOfActivity(a), FlowControlFE);
                        FL2.LinkStyle = FlowLink.eLinkStyle.Line;
                        FL2.SourcePosition = FlowLink.eFlowElementPosition.Right;
                        FL2.DestinationPosition = FlowLink.eFlowElementPosition.Left;
                        mFlowDiagram.AddConnector(FL2);

                        //TODO: update the FlowControlFE to be on same y of the Activity and x + 150
                        
                        if (FC.FlowControlAction == eFlowControlAction.GoToActivity)
                        {
                            // Add Link from FC to target Activity if condition met
                            
                            FlowElement TargetActivityFE = (from x in mFlowDiagram.GetAllFlowElem() where x.Object is Activity && ((Activity)x.Object).Guid == FC.GetGuidFromValue() select x).FirstOrDefault();
                            
                            FlowLink FL3 = new FlowLink(FlowControlFE, TargetActivityFE);                            
                            FL3.LinkStyle = FlowLink.eLinkStyle.DottedArrow;
                            FL3.SourcePosition = FlowLink.eFlowElementPosition.Top;  // TODO: find best connector - if we go up or down
                            FL3.DestinationPosition = FlowLink.eFlowElementPosition.Right; // TODO: find best connector- if we go up or down
                            mFlowDiagram.AddConnector(FL3);
                        }

                        if (FC.FlowControlAction == eFlowControlAction.StopBusinessFlow || FC.FlowControlAction == eFlowControlAction.StopRun)
                        {                        
                            FlowLink FL3 = new FlowLink(FlowControlFE, EndFlowElement);                         
                            FL3.LinkStyle = FlowLink.eLinkStyle.DottedArrow;
                            FL3.SourcePosition = FlowLink.eFlowElementPosition.Top;  // TODO: find best connector
                            FL3.DestinationPosition = FlowLink.eFlowElementPosition.Right; // TODO: find best connector
                            mFlowDiagram.AddConnector(FL3);
                        }
                        //TODO: handle all other FC actions

                        //if (FC.FlowControlAction == eFlowControlAction.SetVariableValue )
                        //{
                        //    FlowLink FL3 = new FlowLink(FlowControlFE, EndFlowElement);
                        //    FL3.LinkStyle = FlowLink.eLinkStyle.DottedArrow;
                        //    FL3.SourcePosition = FlowLink.eFlowElementPosition.Top;  // TODO: find best connector
                        //    FL3.DestinationPosition = FlowLink.eFlowElementPosition.Right; // TODO: find best connector
                        //    mFlowDiagram.AddConnector(FL3);
                        //}
                    }
                }
            }
        }

        private FlowElement GetFlowElemOfActivity(Activity a)
        {
            IEnumerable<FlowElement> list =  mFlowDiagram.GetAllFlowElem();
            foreach(FlowElement FE in list)
            {
                if (FE.Object is Activity)
                {
                    if ((Activity)FE.Object == a)
                    {
                        return FE;
                    }
                }
            }
            return null;
        }

        public void ShowAsWindow()
        {
        }
    }
}
