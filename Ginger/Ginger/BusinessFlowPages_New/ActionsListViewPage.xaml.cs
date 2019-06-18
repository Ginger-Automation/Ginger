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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.Repository;
using Ginger.ApiModelsFolder;
using Ginger.BusinessFlowPages.ListViewItems;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers.Common;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.DragDropLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for ActivityActionsPage.xaml
    /// </summary>
    public partial class ActionsListViewPage : Page
    {
        Activity mActivity;
        Context mContext;

        ActionsListHelper mActionsListHelper;
        UcListView mActionsListView;
        ActionEditPage mActionEditPage;

        public UcListView ListView
        {
            get { return mActionsListView; }
        }

        public ActionsListViewPage(Activity Activity, Context context)
        {
            InitializeComponent();

            mActivity = Activity;
            mContext = context;

            SetListView();
            SetSharedRepositoryMark();
            ShowHideEditPage(null);
        }

        private void ShowHideEditPage(Act actionToEdit)
        {
            if (actionToEdit != null)
            {
                xBackToListPnl.Visibility = Visibility.Visible;
                mActionEditPage = new ActionEditPage(actionToEdit, Ginger.General.RepositoryItemPageViewMode.Automation);//need to pass Context?
                xMainFrame.Content = mActionEditPage;
            }
            else
            {
                xBackToListPnl.Visibility = Visibility.Collapsed;
                mActionEditPage = null;
                xMainFrame.Content = mActionsListView;
            }
        }

        private void SetListView()
        {
            mActionsListView = new UcListView();
            mActionsListView.Title = "Actions";
            mActionsListView.ListImageType = Amdocs.Ginger.Common.Enums.eImageType.Action;

            mActionsListHelper = new ActionsListHelper(mContext);
            mActionsListHelper.ActionListItemEvent += MActionListItemInfo_ActionListItemEvent;
            mActionsListView.SetDefaultListDataTemplate(mActionsListHelper);

            mActionsListView.DataSourceList = mActivity.Acts;

            mActionsListView.PreviewDragItem += listActions_PreviewDragItem;
            mActionsListView.ItemDropped += listActions_ItemDropped;

            mActionsListView.List.MouseDoubleClick += ActionsListView_MouseDoubleClick;
        }

        private void ActionsListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (mActionsListView.CurrentItem != null)
            {
                (mActionsListView.CurrentItem as Act).Context = mContext;
                ShowHideEditPage((Act)mActionsListView.CurrentItem);
            }
        }

        private void MActionListItemInfo_ActionListItemEvent(ActionListItemEventArgs EventArgs)
        {
            switch (EventArgs.EventType)
            {
                case ActionListItemEventArgs.eEventType.ShowActionEditPage:
                    ShowHideEditPage((Act)EventArgs.EventObject);
                    break;
            }
        }

        public void UpdateActivity(Activity activity)
        {
            if (mActivity != activity)
            {
                mActivity = activity;
                if (mActivity != null)
                {
                    mActionsListView.DataSourceList = mActivity.Acts;
                    SetSharedRepositoryMark();
                }
                else
                {
                    mActionsListView.DataSourceList = null;
                }
                ShowHideEditPage(null);
            }
        }

        // Drag Drop handlers
        private void listActions_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DragInfo.DataIsAssignableToType(typeof(Act))
                || DragDrop2.DragInfo.DataIsAssignableToType(typeof(ApplicationPOMModel))
                    || DragDrop2.DragInfo.DataIsAssignableToType(typeof(ElementInfo))
                        || DragDrop2.DragInfo.DataIsAssignableToType(typeof(ApplicationAPIModel)))
            {
                // OK to drop                         
                DragDrop2.DragInfo.DragIcon = GingerWPF.DragDropLib.DragInfo.eDragIcon.Copy;
            }
        }

        private void listActions_ItemDropped(object sender, EventArgs e)
        {
            object droppedItem = ((DragInfo)sender).Data as object;
            if (droppedItem != null)
            {
                Act instance = null;
                if (droppedItem is Act)
                {
                    Act a = droppedItem as Act;
                    instance = (Act)a.CreateInstance(true);
                }
                else if (droppedItem is ElementInfo)
                {
                    ElementInfo elementInfo = droppedItem as ElementInfo;
                    instance = GenerateRelatedAction(elementInfo);
                }
                else if (droppedItem is ApplicationPOMModel)
                {
                    ApplicationPOMModel currentPOM = ((DragInfo)sender).Data as ApplicationPOMModel;
                    foreach (ElementInfo elemInfo in currentPOM.MappedUIElements)
                    {
                        HTMLElementInfo htmlElementInfo = elemInfo as HTMLElementInfo;
                        instance = GenerateRelatedAction(htmlElementInfo);
                        if (instance != null)
                        {
                            instance.Active = true;
                            AddGeneratedAction(instance);
                        }
                    }
                    instance = null;
                }
                else if (droppedItem is ApplicationAPIModel)
                {
                    ApplicationAPIModel currentAPIModel = ((DragInfo)sender).Data as ApplicationAPIModel;
                    AddApiModelActionWizardPage APIModelWizPage = new AddApiModelActionWizardPage(mContext);
                    APIModelWizPage.AAMList.Add(currentAPIModel);
                    APIModelWizPage.Pages.RemoveAt(0);
                    WizardWindow wizWindow = new WizardWindow(APIModelWizPage);
                    //APIModelWizPage.Pages.Insert(0, new WizardPage());
                    APIModelWizPage.GetCurrentPage().Page.WizardEvent(new WizardEventArgs(APIModelWizPage, EventType.Active));
                    wizWindow.Show();

                    //APIModelParamsWizardPage
                    //APIModelWizPage.AAMList.Add(currentAPIModel);
                    //APIModelWizPage.Next();
                    //instance = new GingerCore.Actions.WebServices.WebAPI.ActWebAPIModel();
                }

                if (instance != null)
                {
                    AddGeneratedAction(instance);
                }
            }
        }

        private void AddGeneratedAction(Act instance)
        {
            mActivity.Acts.Add(instance);

            int selectedActIndex = -1;
            if (mActivity.Acts.CurrentItem != null)
            {
                selectedActIndex = mActivity.Acts.IndexOf((Act)mActivity.Acts.CurrentItem);
            }
            if (selectedActIndex >= 0)
            {
                mActivity.Acts.Move(mActivity.Acts.Count - 1, selectedActIndex + 1);
            }
        }

        private static Act GenerateRelatedAction(ElementInfo elementInfo)
        {
            Act instance;
            IPlatformInfo mPlatform = PlatformInfoBase.GetPlatformImpl(ePlatformType.Web);
            ElementActionCongifuration actionConfigurations = new ElementActionCongifuration
            {
                Description = "UIElement Action : " + elementInfo.ItemName,
                Operation = ActUIElement.eElementAction.NotExist.ToString(),
                LocateBy = eLocateBy.POMElement,
                LocateValue = elementInfo.ParentGuid.ToString() + "_" + elementInfo.Guid.ToString(),
                ElementValue = "",
                AddPOMToAction = true,
                POMGuid = elementInfo.ParentGuid.ToString(),
                ElementGuid = elementInfo.Guid.ToString(),
                LearnedElementInfo = elementInfo,
            };

            switch (elementInfo.ElementTypeEnum)
            {
                case eElementType.Button:
                case eElementType.CheckBox:
                case eElementType.RadioButton:
                case eElementType.HyperLink:
                case eElementType.Span:
                case eElementType.Div:
                    actionConfigurations.Operation = ActUIElement.eElementAction.Click.ToString();
                    break;

                case eElementType.TextBox:
                    actionConfigurations.Operation = ActUIElement.eElementAction.SetText.ToString();
                    break;

                default:
                    actionConfigurations.Operation = ActUIElement.eElementAction.NotExist.ToString();
                    break;
            }

            instance = mPlatform.GetPlatformAction(elementInfo, actionConfigurations);
            return instance;
        }

        private void xGoToActionsList_Click(object sender, RoutedEventArgs e)
        {
            ShowHideEditPage(null);
        }

        private void SetSharedRepositoryMark()
        {
            ObservableList<Act> sharedActions = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();
            SharedRepositoryOperations.MarkSharedRepositoryItems((IEnumerable<object>)mActivity.Acts, (IEnumerable<object>)sharedActions);
        }
    }
}
