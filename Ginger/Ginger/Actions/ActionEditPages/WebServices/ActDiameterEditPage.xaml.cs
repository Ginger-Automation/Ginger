using Amdocs.Ginger.CoreNET.ActionsLib.Webservices.Diameter;
using Amdocs.Ginger.CoreNET.DiameterLib;
using System.Windows.Controls;
using System.Collections.Generic;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.GeneralLib;
using System.Windows;
using Ginger.UserControls;
using Ginger.UserControlsLib.TextEditor;

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

        private ObservableList<DiameterAVP> LoadAvpForMessage(DiameterEnums.eDiameterMessageType diameterMessageType)
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
            xRequestAvpListGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshRequestAvpGrid));
            xRequestAvpListGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            List<ComboEnumItem> avpDataTypeList = GingerCore.General.GetEnumValuesForCombo(typeof(DiameterEnums.eDiameterAvpDataType));
            view.GridColsView.Add(new GridColView()
            {
                Field = nameof(DiameterAVP.Name),
                Header = "Avp Name",
                WidthWeight = 35,
                StyleType = GridColView.eGridColStyleType.Template,
                CellTemplate = ucGrid.GetGridComboBoxTemplate<DiameterAVP>(
                    comboValuesList: DiameterUtils.AvpDictionaryList,
                    displayMemberPath: nameof(DiameterAVP.Name),
                    selectedValuePath: nameof(DiameterAVP.Name),
                    selectedValueField: nameof(DiameterAVP.Name),
                    allowEdit: true,
                    comboSelectionChangedHandler: xAvpNameComboBox_SelectionChanged)
            });
            view.GridColsView.Add(new GridColView() { Field = nameof(DiameterAVP.Code), Header = "Code", WidthWeight = 10 });
            view.GridColsView.Add(new GridColView() { Field = nameof(DiameterAVP.DataType), Header = "Data Type", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = avpDataTypeList });
            view.GridColsView.Add(new GridColView() { Field = nameof(DiameterAVP.IsMandatory), Header = "Mandatory", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(DiameterAVP.IsVendorSpecific), Header = "Vendor Specific", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView()
            {
                Field = nameof(DiameterAVP.Value),
                Header = "Value",
                WidthWeight = 15,
                StyleType = GridColView.eGridColStyleType.Template,
                CellTemplate = ucGrid.getDataColValueExpressionTemplate(nameof(DiameterAVP.ValueVE), (Context)mAct.Context)
            });

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
                int selectedItemIndex = xRequestAvpListGrid.grdMain.SelectedIndex;
                if (selectedItemIndex != -1)
                {
                    mAct.RequestAvpList[selectedItemIndex] = (DiameterAVP)avpNameCB.SelectedItem;
                    UpdateRequestAvpsGridDataSource();
                }
            }
        }

        private void AddAvpToGrid(object sender, RoutedEventArgs e)
        {
            mAct.RequestAvpList.Add(new DiameterAVP());
        }

        // TODO: display the raw request(diameter)
        private void xViewRawRequestBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //string requestContent = DiameterUtils.GetRawRequestContentPreview(mAct);
            //if (requestContent != string.Empty)
            //{
            //    string tempFilePath = GingerCoreNET.GeneralLib.General.CreateTempTextFile(requestContent);
            //    if (System.IO.File.Exists(tempFilePath))
            //    {
            //        DocumentEditorPage docPage = new DocumentEditorPage(tempFilePath, enableEdit: false, UCTextEditorTitle: string.Empty);
            //        docPage.Width = 800;
            //        docPage.Height = 800;
            //        docPage.ShowAsWindow("Raw Request Preview");
            //        System.IO.File.Delete(tempFilePath);
            //        return;
            //    }
            //}
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
            DiameterAVP diameterAVP = (DiameterAVP)xRequestAvpListGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(diameterAVP, nameof(DiameterAVP.Value), Context.GetAsContext(mAct.Context));
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
