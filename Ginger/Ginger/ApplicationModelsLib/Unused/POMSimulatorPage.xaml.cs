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

using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.POMsLib
{
    /// <summary>
    /// Interaction logic for POMSimulatorPage.xaml
    /// </summary>
    public partial class POMSimulatorPage : Page
    {
        //ApplicationPOMModel mPOM;

        bool mRecording = false;
        //ScreenShotViewPage mScreenShotViewPage;
        public POMSimulatorPage(ApplicationPOMModel POM)
        {
            InitializeComponent();

            //mPOM = POM;

            

            //mScreenShotViewPage = new ScreenShotViewPage(mPOM.Name, mPOM.ScreenShot);
            //ScreenshotFrame.Content = mScreenShotViewPage;

            //mScreenShotViewPage.MouseClickOnScreenshot += MouseClickOnScreenShot;

            ////ActionsPage ap = new ActionsPage(App.BusinessFlow.CurrentActivity);
            ////ActionsFrame.Content = ap;

            //InitActionsGrid();
        }


        private void InitActionsGrid()
        {
            //GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            //defView.GridColsView = new ObservableList<GridColView>();
            //defView.GridColsView.Add(new GridColView() { Field = nameof(Act.Description), WidthWeight = 10 });            
            //AvailableControlActionsGrid.SetAllColumnsDefaultView(defView);
            //AvailableControlActionsGrid.InitViewItems();            

            //AvailableControlActionsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
        }

        private void MouseClickOnScreenShot(object arg1, MouseclickonScrenshot arg2)
        {
            ShowElem(arg2.X, arg2.Y);
        }

        private void ShowElem(double x, double y)
        {
            // ******************************************************************************
            //DO NOT  DELETE Temp commented for moving to GingerCoreCommon
            // ******************************************************************************

            ////locate the elem in the POM

            //foreach (ElementInfo EI in mPOM.UIElements)
            //{
            //    if (EI.Active)
            //    {
            //        if (x >= EI.X && x<= EI.X + EI.Width && y>= EI.Y && y<= EI.Y+EI.Height)
            //        {
            //            // TODO: we can find more then one elem, keep all and go based on the one with less area X*Y
            //            //meanhwile we take the first we find
            //            mScreenShotViewPage.HighLight(EI.X, EI.Y, EI.Width, EI.Height);

            //            ShowControlActions(EI);

            //            break;
            //        }
            //    }
            //}


        }

        private void ShowControlActions(ElementInfo EI)
        {
            //AvailableControlActionsGrid.DataSourceList = EI.GetAvailableActions();
        }

        private void CreateControlsButton_Click(object sender, RoutedEventArgs e)
        {
            // ******************************************************************************
            //DO NOT  DELETE Temp commented for moving to GingerCoreCommon
            // ******************************************************************************

            //// mScreenShotViewPage.ClearControls();
            //foreach (ElementInfo ei in mPOM.UIElements)
            //{
            //    if (ei.Active)
            //    {
            //        // TODO: based onthe type create the correct control
            //        //TODO: we need to keep also the tab order
            //        //TODO: attach handler fo intersting events: text changed, click etc.

            //        //TODO: temp hard coded fix me
            //        //TODO: use switch in IWindowEx/EI to return control
            //        if (ei.ElementType == "INPUT.TEXT")
            //        {
            //            TextBox c = new TextBox();
            //            c.Width = ei.Width;
            //            c.Height = ei.Height;
            //            c.Tag = ei;
            //            // c.TextChanged += C_TextChanged;
            //            c.LostFocus += C_LostFocus;
            //            mScreenShotViewPage.AddControl(c, ei.X, ei.Y);
            //            continue;
            //        }

            //        if (ei.ElementType == "INPUT.BUTTON")
            //        {
            //            Button c = new Button();
            //            c.Width = ei.Width;
            //            c.Height = ei.Height;
            //            c.Content = ei.ElementTitle ;  // TODO: fix me 
            //            c.Click += Button_Click;
            //            c.Tag = ei;
            //            mScreenShotViewPage.AddControl(c, ei.X, ei.Y);
            //            continue;
            //        }

            //        if (ei.ElementType == "SELECT")
            //        {
            //            ComboBox c = new ComboBox();
            //            c.Width = ei.Width;
            //            c.Height = ei.Height;
            //            c.Items.Add("A"); // temo, TODO: fix me
            //            c.Items.Add("B");
            //            c.Items.Add("C");
            //            c.Items.Add("d");
            //            c.SelectionChanged += ComboBox_SelectionChanged;
            //            c.Tag = ei;
            //            mScreenShotViewPage.AddControl(c, ei.X, ei.Y);
            //            continue;
            //        }


            //    }
            //}
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (!mRecording) return;

            //ComboBox CB = (ComboBox)sender;
            //ElementInfo ei = (ElementInfo)CB.Tag;            

            //ActUIElement act = new ActUIElement();
            //act.Description = "Set ComboBox Value " + ei.ElementTitle;
            //act.ElementLocateBy = eLocateBy.ByModelName;
            //act.ElementType = eElementType.ComboBox;
            //act.ElementLocateValue = @"\POM\App\v1\Login\text";  //TODO: fix me
            //act.ElementAction = ActUIElement.eElementAction.SetText;
            //act.Value = (string)CB.SelectedValue;
            //App.BusinessFlow.AddAct(act);
        }

        private void C_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (!mRecording) return;

            //TextBox TB = (TextBox)sender;
            //ElementInfo ei = (ElementInfo)TB.Tag;
            //// MessageBox.Show("You changed text of - " + ei.ElementTitle);

            //ActUIElement act = new ActUIElement();
            //act.Description = "Set Value " + ei.ElementTitle;
            //act.ElementLocateBy = eLocateBy.ByModelName;  
            //act.ElementType = eElementType.TextBox;
            //act.ElementLocateValue = @"\POM\App\v1\Login\text";  //TODO: fix me
            //act.ElementAction = ActUIElement.eElementAction.SetText;
            //act.Value = TB.Text;
            //App.BusinessFlow.AddAct(act);
        }

        private void C_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //if (!mRecording) return;

            //Button b = (Button)sender;
            //ElementInfo ei = (ElementInfo)b.Tag;
            //// MessageBox.Show("You Clicked - " + ei.ElementTitle);

            //ActUIElement act = new ActUIElement();
            //act.Description = "Click button " + ei.ElementTitle;
            //act.ElementLocateBy = eLocateBy.ByModelName; 
            //act.ElementType = eElementType.Button;
            //act.ElementLocateValue = @"\POM\App\v1\Login\loginbutton";  //TODO: fix me
            //act.ElementAction = ActUIElement.eElementAction.Click;
            
            //App.BusinessFlow.AddAct(act);
        }

       
        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            mRecording = !mRecording;
            if (mRecording)
            {
                RecordButton.Content = "Stop Recording";
            }
            else
            {
                RecordButton.Content = "Start Recording";
            }
        }
    }
}
