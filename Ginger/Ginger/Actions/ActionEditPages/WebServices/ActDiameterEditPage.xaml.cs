#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.ActionsLib.Webservices.Diameter;
using Amdocs.Ginger.CoreNET.DiameterLib;
using Amdocs.Ginger.Repository;
using Ginger.Actions.ActionConversion;
using Ginger.UserControls;
using Ginger.UserControlsLib.TextEditor;
using GingerCore.GeneralLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Amdocs.Ginger.CoreNET.DiameterLib.DiameterEnums;

namespace Ginger.Actions.WebServices
{
    /// <summary>
    /// Interaction logic for ActDiameterEditPage.xaml
    /// </summary>
    public partial class ActDiameterEditPage : Page
    {
        private enum eGridType
        {
            RequestGrid,
            ResponseGrid
        }
        ActDiameter mAct;
        ObservableList<DiameterAVP> groupedAvpsList = [];

        public ActDiameterEditPage(ActDiameter act)
        {
            mAct = act;
            InitializeComponent();
            BindControls();
            SetGroupedAvpList();
            SetRequestAvpsGrid();
            SetCustomResponseAvpsGrid();
        }

        private void SetCustomResponseAvpsGrid()
        {
            xCustomResponseAvpListGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddAvpToCustomResponseAvpGrid));
            xCustomResponseAvpListGrid.SetTitleLightStyle = true;

            GridViewDef view = CreateGridView();
            CreateAvpGridColumns(view, eGridType.ResponseGrid);

            xCustomResponseAvpListGrid.SetAllColumnsDefaultView(view);
            xCustomResponseAvpListGrid.InitViewItems();
            xCustomResponseAvpListGrid.DataSourceList = mAct.CustomResponseAvpList;
        }

        private ObservableList<DiameterAVP> LoadAvpForMessage(eDiameterMessageType diameterMessageType)
        {
            return DiameterUtils.GetMandatoryAVPForMessage(diameterMessageType);
        }

        private void BindControls()
        {
            ActInputValue messageType = mAct.GetOrCreateInputParam(nameof(ActDiameter.DiameterMessageType), eDiameterMessageType.None.ToString());
            xMessageTypeComboBox.Init(messageType, typeof(eDiameterMessageType), false, xMessageTypeComboBox_SelectionChanged);
            xCommandCodeTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActDiameter.CommandCode)));
            xApplicationIdTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActDiameter.ApplicationId)));
            xHopByHopIdTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActDiameter.HopByHopIdentifier)));
            xEndToEndIdTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActDiameter.EndToEndIdentifier)));

            BindingHandler.ObjFieldBinding(xIsRequestCheckBox, CheckBox.IsCheckedProperty, mAct, nameof(ActDiameter.IsRequestBitSet));
            BindingHandler.ObjFieldBinding(xProxiableCheckBox, CheckBox.IsCheckedProperty, mAct, nameof(ActDiameter.IsProxiableBitSet));
            BindingHandler.ObjFieldBinding(xErrorCheckBox, CheckBox.IsCheckedProperty, mAct, nameof(ActDiameter.IsErrorBitSet));
        }

        private void SetRequestAvpsGrid()
        {
            xRequestAvpListGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddAvpToRequestAvpGrid));
            xRequestAvpListGrid.SetTitleLightStyle = true;

            GridViewDef view = CreateGridView();
            CreateAvpGridColumns(view, eGridType.RequestGrid);

            xRequestAvpListGrid.SetAllColumnsDefaultView(view);
            xRequestAvpListGrid.InitViewItems();
            xRequestAvpListGrid.DataSourceList = mAct.RequestAvpList;
        }
        private void CreateAvpGridColumns(GridViewDef view, eGridType gridType)
        {
            List<GridColView> colViews = GetGridColViewList(gridType);
            foreach (var colView in colViews)
            {
                view.GridColsView.Add(colView);
            }
        }
        private List<GridColView> GetGridColViewList(eGridType gridType)
        {
            List<GridColView> colViews = [];

            var nameColView = new GridColView()
            {
                Header = "Name",
                Field = nameof(DiameterAVP.Name),
                WidthWeight = 200,
                StyleType = GridColView.eGridColStyleType.Template,
                CellTemplate = ucGrid.GetGridComboBoxTemplate<DiameterAvpDictionaryItem>(DiameterUtils.AvpDictionaryList,
                displayMemberPath: nameof(DiameterAVP.Name),
                selectedValuePath: nameof(DiameterAVP.Name),
                selectedValueField: nameof(DiameterAVP.Name),
                allowEdit: true,
                comboSelectionChangedHandler: xAvpNameComboBox_SelectionChanged,
                comboBoxTag: gridType.ToString())
            };

            var codeColView = new GridColView()
            {
                Header = "Code",
                WidthWeight = 45,
                MaxWidth = 45,
                Field = nameof(DiameterAVP.Code)
            };

            var dataTypeColView = new GridColView()
            {
                Header = "Data Type",
                WidthWeight = 100,
                Field = nameof(DiameterAVP.DataType),
                StyleType = GridColView.eGridColStyleType.Template,
                CellTemplate = ucGrid.GetGridComboBoxTemplate(GingerCore.General.GetEnumValuesForCombo(typeof(DiameterEnums.eDiameterAvpDataType)), nameof(DiameterAVP.DataType), comboSelectionChangedHandler: xDataTypeComboBox_SelectionChanged)
            };

            colViews.Add(nameColView);
            colViews.Add(codeColView);
            colViews.Add(dataTypeColView);


            if (gridType == eGridType.RequestGrid)
            {
                var mandatoryColView = new GridColView()
                {
                    Header = "Mandatory",
                    WidthWeight = 60,
                    MaxWidth = 75,
                    Field = nameof(DiameterAVP.IsMandatory),
                    StyleType = GridColView.eGridColStyleType.CheckBox
                };
                var vendorSpecificColView = new GridColView()
                {
                    Header = "Vendor Specific",
                    WidthWeight = 80,
                    MaxWidth = 95,
                    Field = nameof(DiameterAVP.IsVendorSpecific),
                    StyleType = GridColView.eGridColStyleType.CheckBox
                };
                var vendorIdColView = new GridColView()
                {
                    Header = "Vendor Id",
                    WidthWeight = 110,
                    Field = nameof(DiameterAVP.VendorId),
                    StyleType = GridColView.eGridColStyleType.Template,
                    CellTemplate = GetTextBoxCellTemplate(valuePath: nameof(DiameterAVP.VendorId), isEnabledPath: nameof(DiameterAVP.IsVendorSpecific))
                };
                var parentAvpColView = new GridColView()
                {
                    Header = "Parent AVP",
                    WidthWeight = 150,
                    Field = nameof(DiameterAVP.ParentName),
                    StyleType = GridColView.eGridColStyleType.Template,
                    CellTemplate = GetParentAVPComboBoxDataTemplate(groupedAvpsList, valuePath: nameof(DiameterAVP.ParentAvpGuid), isEnabledPath: nameof(DiameterAVP.DataType), xParentAVP_SelectionChanged, gridType)
                };
                var valueColView = new GridColView()
                {
                    Field = nameof(DiameterAVP.Value),
                    Header = "Value",
                    StyleType = GridColView.eGridColStyleType.Template,
                    CellTemplate = ucGrid.getDataColValueExpressionTemplate(nameof(DiameterAVP.ValueVE), (Context)mAct.Context)
                };

                colViews.Add(mandatoryColView);
                colViews.Add(vendorSpecificColView);
                colViews.Add(vendorIdColView);
                colViews.Add(parentAvpColView);
                colViews.Add(valueColView);
            }

            return colViews;
        }

        private GridViewDef CreateGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView = []
            };
            return view;
        }
        private void AddAvpToCustomResponseAvpGrid(object sender, RoutedEventArgs e)
        {
            mAct.CustomResponseAvpList.Add(new DiameterAVP());
        }
        private void xAvpNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox avpNameCB && avpNameCB.IsDropDownOpen)
            {
                if ((string)avpNameCB.Tag == eGridType.RequestGrid.ToString())
                {
                    HandleRequestGridAvpNameChanged(avpNameCB);
                }
                else if ((string)avpNameCB.Tag == eGridType.ResponseGrid.ToString())
                {
                    HandleResponseGridAvpNameChanged(avpNameCB);
                }

            }
        }
        private void HandleResponseGridAvpNameChanged(ComboBox avpNameCB)
        {
            int selectedItemIndex = xCustomResponseAvpListGrid.grdMain.SelectedIndex;
            if (selectedItemIndex != -1)
            {
                var sourceAVP = (DiameterAvpDictionaryItem)avpNameCB.SelectedItem;
                DiameterAVP currentAVP = mAct.CustomResponseAvpList[selectedItemIndex];
                UpdateAVP(currentAVP, sourceAVP);
                UpdateResponseAvpsGridDataSource();
                SetGroupedAvpList();
            }
        }
        private void HandleRequestGridAvpNameChanged(ComboBox avpNameCB)
        {
            int selectedItemIndex = xRequestAvpListGrid.grdMain.SelectedIndex;
            if (selectedItemIndex != -1)
            {
                var sourceAVP = (DiameterAvpDictionaryItem)avpNameCB.SelectedItem;
                DiameterAVP currentAVP = mAct.RequestAvpList[selectedItemIndex];
                UpdateAVP(currentAVP, sourceAVP);
                UpdateRequestAvpsGridDataSource();
                SetGroupedAvpList();
            }
        }
        private void UpdateAVP(DiameterAVP avpToUpdate, DiameterAvpDictionaryItem sourceAvp)
        {
            var mapperConfig = new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile<DiameterAutoMapperProfile>());
            var mapper = mapperConfig.CreateMapper();

            mapper.Map(sourceAvp, avpToUpdate);

            avpToUpdate.ParentName = null;
            avpToUpdate.ParentAvpGuid = System.Guid.Empty;
            avpToUpdate.NestedAvpList.Clear();
        }
        private void xDataTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox dataTypeCB && dataTypeCB.IsDropDownOpen)
            {
                SetGroupedAvpList();
            }
        }

        private void SetGroupedAvpList()
        {
            ValidateGroupedAvpList();
            AddGroupedAvpToListFromRequestGrid();
            AddGroupedAvpToListFromCustomResponseGrid();
        }

        private void AddGroupedAvpToListFromCustomResponseGrid()
        {
            if (mAct.CustomResponseAvpList != null)
            {
                var responseGroupedAvps = mAct.CustomResponseAvpList.Where(x => x.DataType == DiameterEnums.eDiameterAvpDataType.Grouped);
                foreach (DiameterAVP groupedAVP in responseGroupedAvps)
                {
                    if (!groupedAvpsList.Contains(groupedAVP))
                    {
                        groupedAvpsList.Add(groupedAVP);
                    }
                }
            }
        }
        private void AddGroupedAvpToListFromRequestGrid()
        {
            if (mAct.RequestAvpList != null)
            {
                var requestGroupedAvps = mAct.RequestAvpList.Where(x => x.DataType == DiameterEnums.eDiameterAvpDataType.Grouped);
                foreach (DiameterAVP groupedAVP in requestGroupedAvps)
                {
                    if (!groupedAvpsList.Contains(groupedAVP))
                    {
                        groupedAvpsList.Add(groupedAVP);
                    }
                }
            }
        }
        private void ValidateGroupedAvpList()
        {
            if (groupedAvpsList != null)
            {
                if (groupedAvpsList.Any())
                {
                    List<DiameterAVP> itemsToRemove = [];

                    foreach (DiameterAVP avp in groupedAvpsList)
                    {
                        if (avp.DataType != DiameterEnums.eDiameterAvpDataType.Grouped)
                        {
                            ClearChildAVP(avp);
                            itemsToRemove.Add(avp);
                        }
                    }

                    foreach (DiameterAVP avpToRemove in itemsToRemove)
                    {
                        groupedAvpsList.Remove(avpToRemove);
                    }
                }
                else
                {
                    groupedAvpsList.Add(new DiameterAVP() { Name = "N/A", DataType = DiameterEnums.eDiameterAvpDataType.Grouped });
                }
            }

        }
        private void ClearChildAVP(DiameterAVP avp)
        {
            foreach (DiameterAVP childAvp in avp.NestedAvpList)
            {
                if (childAvp != null)
                {
                    childAvp.ParentName = null;
                    childAvp.ParentAvpGuid = System.Guid.Empty;
                }
            }
            avp.NestedAvpList.Clear();
        }
        private void xParentAVP_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox parentAvpCB && parentAvpCB.IsDropDownOpen)
            {
                if (parentAvpCB.SelectedItem is DiameterAVP selectedAvp)
                {
                    HandleRequestParentAvpChanged(selectedAvp);
                }
            }
        }
        private void HandleRequestParentAvpChanged(DiameterAVP selectedAvp)
        {
            int selectedItemIndex = xRequestAvpListGrid.grdMain.SelectedIndex;
            if (selectedItemIndex != -1)
            {
                var avp = (DiameterAVP)xRequestAvpListGrid.grdMain.Items[selectedItemIndex];
                if (avp != null)
                {
                    avp.ParentName = selectedAvp.Name;
                }
            }
        }
        private void AddAvpToRequestAvpGrid(object sender, RoutedEventArgs e)
        {
            mAct.RequestAvpList.Add(new DiameterAVP());
        }
        private void xViewRawRequestBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mAct != null)
            {
                string requestContent = DiameterUtils.GetRawRequestContentPreview(mAct);
                if (requestContent != string.Empty)
                {
                    string tempFilePath = GingerCoreNET.GeneralLib.General.CreateTempTextFile(requestContent);
                    if (System.IO.File.Exists(tempFilePath))
                    {
                        DocumentEditorPage docPage = new DocumentEditorPage(tempFilePath, enableEdit: false, UCTextEditorTitle: string.Empty)
                        {
                            Width = 800,
                            Height = 800
                        };
                        docPage.ShowAsWindow("Raw Request Preview");
                        System.IO.File.Delete(tempFilePath);
                        return;
                    }
                }
            }
            Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to load raw request preview, see log for details.");
        }
        private void xMessageTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xMessageTypeComboBox.ComboBox.IsDropDownOpen)
            {
                if (mAct != null)
                {
                    SetMessageDetails(mAct.DiameterMessageType);
                }
            }
        }
        private void SetMessageDetails(eDiameterMessageType messageType)
        {
            ResetMessageDetails();
            if (messageType == eDiameterMessageType.CapabilitiesExchangeRequest)
            {
                mAct.CommandCode = 257;
                mAct.ApplicationId = 0;
                mAct.IsRequestBitSet = true;
                GetMessageAvpByMessageType(messageType);
            }
            else if (messageType == eDiameterMessageType.CreditControlRequest)
            {
                mAct.CommandCode = 272;
                mAct.ApplicationId = 4;
                mAct.IsRequestBitSet = true;
                mAct.IsProxiableBitSet = true;
                GetMessageAvpByMessageType(messageType);
            }

            UpdateRequestAvpsGridDataSource();
        }

        private void GetMessageAvpByMessageType(eDiameterMessageType messageType)
        {
            var avpList = LoadAvpForMessage(messageType);
            foreach (DiameterAVP avp in avpList)
            {
                if (avp != null)
                {
                    mAct.RequestAvpList.Add(avp);
                }
            }
        }

        private void ResetMessageDetails()
        {
            // Reset Diameter Action to default values
            mAct.CommandCode = 0;
            mAct.ApplicationId = 0;
            mAct.EndToEndIdentifier = 0;
            mAct.HopByHopIdentifier = 0;
            mAct.IsRequestBitSet = false;
            mAct.IsProxiableBitSet = false;
            mAct.IsErrorBitSet = false;
            mAct.RequestAvpList?.Clear();
            mAct.CustomResponseAvpList?.Clear();
        }

        private void UpdateRequestAvpsGridDataSource()
        {
            if (mAct != null && xRequestAvpListGrid != null)
            {
                xRequestAvpListGrid.DataSourceList = mAct.RequestAvpList;
            }
        }
        private void UpdateResponseAvpsGridDataSource()
        {
            if (mAct != null && xCustomResponseAvpListGrid != null)
            {
                xCustomResponseAvpListGrid.DataSourceList = mAct.CustomResponseAvpList;
            }
        }
        private DataTemplate GetParentAVPComboBoxDataTemplate(ObservableList<DiameterAVP> groupedAvpList, string valuePath, string isEnabledPath, SelectionChangedEventHandler changedEventHandler, eGridType gridType)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory combo = new FrameworkElementFactory(typeof(ComboBox));

            combo.SetValue(ComboBox.ItemsSourceProperty, groupedAvpList);
            combo.SetValue(ComboBox.DisplayMemberPathProperty, nameof(DiameterAVP.Name));
            combo.SetValue(ComboBox.SelectedValuePathProperty, nameof(DiameterAVP.Guid));

            SetDataTemplateValueBinding(combo, valuePath, ComboBox.SelectedValueProperty);

            combo.SetValue(TagProperty, gridType);

            combo.AddHandler(ComboBox.SelectionChangedEvent, changedEventHandler);

            template.VisualTree = combo;
            return template;
        }
        private DataTemplate GetTextBoxCellTemplate(string valuePath, string isEnabledPath)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory textBox = new FrameworkElementFactory(typeof(TextBox));

            textBox.SetValue(TextBox.TextProperty, valuePath);

            SetDataTemplateValueBinding(textBox, valuePath, TextBox.TextProperty);
            SetDataTemplateIsEnableBinding(textBox, isEnabledPath, new BooleanToEnabledConverter());

            template.VisualTree = textBox;

            return template;
        }
        private void SetDataTemplateValueBinding(FrameworkElementFactory frameworkElement, string valuePath, DependencyProperty dependencyProperty, BindingMode bindingMode = BindingMode.TwoWay, UpdateSourceTrigger updateSourceTrigger = UpdateSourceTrigger.PropertyChanged)
        {
            Binding valueBinding = new Binding(valuePath)
            {
                Mode = bindingMode,
                UpdateSourceTrigger = updateSourceTrigger
            };
            frameworkElement.SetBinding(dependencyProperty, valueBinding);
        }
        private void SetDataTemplateIsEnableBinding(FrameworkElementFactory frameworkElement, string isEnablePath, IValueConverter valueConverter)
        {
            Binding isEnableBinding = new Binding(isEnablePath)
            {
                Converter = valueConverter
            };
            frameworkElement.SetBinding(IsEnabledProperty, isEnableBinding);
        }
    }
}
