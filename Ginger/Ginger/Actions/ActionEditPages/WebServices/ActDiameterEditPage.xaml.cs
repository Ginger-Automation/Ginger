using Amdocs.Ginger.CoreNET.ActionsLib.Webservices.Diameter;
using Amdocs.Ginger.CoreNET.DiameterLib;
using System.Windows.Controls;
using System.Collections.Generic;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.GeneralLib;
using static Amdocs.Ginger.CoreNET.ActionsLib.Webservices.Diameter.ActDiameter;
using System.Windows;
using System;
using Ginger.UserControls;
using GingerCore.DataSource;
using System.Data;
using System.Reflection;
using System.IO;
using Amdocs.Ginger.CoreNET.DiameterLib;
using System.Linq;
using Ginger.UserControlsLib.TextEditor;
using GingerCore.Actions.WebAPI;

namespace Ginger.Actions.WebServices
{
    /// <summary>
    /// Interaction logic for ActDiameterEditPage.xaml
    /// </summary>
    public partial class ActDiameterEditPage : Page
    {
        ActDiameter mAct;

        public ActDiameterEditPage(ActDiameter act)
        {
            mAct = act;
            InitializeComponent();
            BindControls();
            SetRequestAvpsGrid();
        }

        private ObservableList<ActDiameterAvp> LoadAvpForMessage(DiameterEnums.eDiameterMessageType diameterMessageType)
        {
            return DiameterUtils.GetMandatoryAVPForMessage(diameterMessageType);
        }

        private void BindControls()
        {
            ActInputValue messageType = mAct.GetOrCreateInputParam(nameof(ActDiameter.DiameterMessageType), DiameterEnums.eDiameterMessageType.None.ToString());
            xMessageTypeComboBox.Init(messageType, typeof(DiameterEnums.eDiameterMessageType), false, xMessageTypeComboBox_SelectionChanged);
            xCommandCodeTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActDiameter.CommandCode)));
            xApplicationIdTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActDiameter.ApplicationId)));
            xHopByHopIdTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActDiameter.HopByHopIdentifier)));
            xEndToEndIdTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActDiameter.EndToEndIdentifier)));

            BindingHandler.ObjFieldBinding(xIsRequestCheckBox, CheckBox.IsCheckedProperty, mAct, nameof(ActDiameter.SetRequestBit));
            BindingHandler.ObjFieldBinding(xProxiableCheckBox, CheckBox.IsCheckedProperty, mAct, nameof(ActDiameter.SetProxiableBit));
            BindingHandler.ObjFieldBinding(xErrorCheckBox, CheckBox.IsCheckedProperty, mAct, nameof(ActDiameter.SetErrorBit));
            BindingHandler.ObjFieldBinding(xRetransmitMessageCheckBox, CheckBox.IsCheckedProperty, mAct, nameof(ActDiameter.SetRetransmitBit));
        }

        private void SetRequestAvpsGrid()
        {
            xRequestAvpListGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddAvpToGrid));
            xRequestAvpListGrid.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteAvpToGrid));
            xRequestAvpListGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshRequestAvpGrid));
            xRequestAvpListGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            List<ComboEnumItem> avpDataTypeList = GingerCore.General.GetEnumValuesForCombo(typeof(DiameterEnums.eDiameterAvpDataType));
            view.GridColsView.Add(new GridColView() { Field = nameof(ActDiameterAvp.Name), Header = "Avp Name", WidthWeight = 35, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(nameof(ActDiameterAvp.AvpNamesList), nameof(ActDiameterAvp.Name), allowEdit: true, comboSelectionChangedHandler: xAvpNameComboBox_SelectionChanged) });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActDiameterAvp.Code), Header = "Code", WidthWeight = 10 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActDiameterAvp.DataType), Header = "Data Type", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = avpDataTypeList });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActDiameterAvp.IsMandatory), Header = "Mandatory", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActDiameterAvp.IsVendorSpecific), Header = "Vendor Specific", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActDiameterAvp.Value), Header = "Value", WidthWeight = 15 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xRequestAVPPanel.Resources["ValueExpressionButton"] });
            xRequestAvpListGrid.SetAllColumnsDefaultView(view);
            xRequestAvpListGrid.InitViewItems();
            xRequestAvpListGrid.DataSourceList = mAct.RequestAvpList;
        }

        private void RefreshRequestAvpGrid(object sender, RoutedEventArgs e)
        {
            UpdateRequestAvpsGridDataSource();
        }

        private void xAvpNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox? avpNameCB = sender as ComboBox;
            if (avpNameCB != null && avpNameCB.IsDropDownOpen)
            {
                DiameterAVP? avp = DiameterUtils.AvpDictionaryList.Select(avp => avp).FirstOrDefault(avp => avp.Name == avpNameCB.SelectedItem.ToString());
                if (avp != null)
                {
                    ActDiameterAvp? selectedAvp = (ActDiameterAvp)xRequestAvpListGrid.grdMain.SelectedItem;
                    if (selectedAvp != null)
                    {
                        selectedAvp = new ActDiameterAvp(avp);
                        int selectedItemIndex = xRequestAvpListGrid.grdMain.SelectedIndex;
                        mAct.RequestAvpList[selectedItemIndex] = selectedAvp;
                        UpdateRequestAvpsGridDataSource();
                    }
                }
            }
        }

        private void DeleteAvpToGrid(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void AddAvpToGrid(object sender, RoutedEventArgs e)
        {
            mAct.RequestAvpList.Add(new ActDiameterAvp());
            UpdateRequestAvpsGridDataSource();
        }

        private void xViewRawRequestBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string requestContent = DiameterUtils.GetRawRequestContentPreview(mAct);
            if (requestContent != string.Empty)
            {
                string tempFilePath = GingerCoreNET.GeneralLib.General.CreateTempTextFile(requestContent);
                if (System.IO.File.Exists(tempFilePath))
                {
                    DocumentEditorPage docPage = new DocumentEditorPage(tempFilePath, enableEdit: false, UCTextEditorTitle: string.Empty);
                    docPage.Width = 800;
                    docPage.Height = 800;
                    docPage.ShowAsWindow("Raw Request Preview");
                    System.IO.File.Delete(tempFilePath);
                    return;
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

        private void SetMessageDetails(DiameterEnums.eDiameterMessageType messageType)
        {
            if (messageType == DiameterEnums.eDiameterMessageType.CapabilitiesExchangeRequest)
            {
                mAct.CommandCode = 257;
                mAct.ApplicationId = 0;
                mAct.SetRequestBit = true;
                mAct.RequestAvpList = LoadAvpForMessage(messageType);
            }
            else if (messageType == DiameterEnums.eDiameterMessageType.CreditControlRequest)
            {
                mAct.CommandCode = 272;
                mAct.ApplicationId = 4;
                mAct.SetRequestBit = true;
            }

            UpdateRequestAvpsGridDataSource();
        }

        private void GridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActDiameterAvp diameterAVP = (ActDiameterAvp)xRequestAvpListGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(diameterAVP, nameof(ActDiameterAvp.Value), Context.GetAsContext(mAct.Context));
            VEEW.ShowAsWindow();
        }

        private void UpdateRequestAvpsGridDataSource()
        {
            if (mAct != null && xRequestAvpListGrid != null)
            {
                xRequestAvpListGrid.DataSourceList = mAct.RequestAvpList;
            }
        }
    }
}
