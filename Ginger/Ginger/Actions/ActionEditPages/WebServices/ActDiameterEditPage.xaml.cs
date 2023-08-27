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
            //DiameterAVP avp1 = new DiameterAVP() { Name = "Origin-Host", Code = 264 };
            //AvpList.Add(avp1);
            BindControls();
        }

        private void LoadAvpDictionary(DiameterEnums.eDiameterMessageType diameterMessageType)
        {
        }

        private void BindControls()
        {
            ActInputValue messageType = mAct.GetOrCreateInputParam(nameof(ActDiameter.DiameterMessageType), DiameterEnums.eDiameterMessageType.CapabilitiesExchange.ToString());
            xMessageTypeComboBox.Init(messageType, typeof(DiameterEnums.eDiameterMessageType), false, xMessageTypeComboBox_SelectionChanged);
            xCommandCodeTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActDiameter.CommandCode)));
            xApplicationIdTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActDiameter.ApplicationId)));
            xHopByHopIdTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActDiameter.HopByHopIdentifier)));
            xEndToEndIdTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActDiameter.EndToEndIdentifier)));
            
            BindingHandler.ObjFieldBinding(xIsRequestCheckBox, CheckBox.IsCheckedProperty, mAct, nameof(ActDiameter.SetRequestBit));
            BindingHandler.ObjFieldBinding(xProxiableCheckBox, CheckBox.IsCheckedProperty, mAct, nameof(ActDiameter.SetProxiableBit));
            BindingHandler.ObjFieldBinding(xErrorCheckBox, CheckBox.IsCheckedProperty, mAct, nameof(ActDiameter.SetErrorBit));
            BindingHandler.ObjFieldBinding(xRetransmitMessageCheckBox, CheckBox.IsCheckedProperty, mAct, nameof(ActDiameter.SetRetransmitBit));
            
            xRequestAvpListGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddAvpToGrid));
            xRequestAvpListGrid.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteAvpToGrid));
            xRequestAvpListGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(DiameterAVP.Name), Header = "Avp Name", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(nameof(DiameterUtils.AvpDictionaryList), nameof(DiameterAVP.Name), allowEdit: true) });
            view.GridColsView.Add(new GridColView() { Field = nameof(DiameterAVP.Code), Header = "Code", WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = nameof(DiameterAVP.Value), Header = "Value", WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xRequestAVPPanel.Resources["ValueExpressionButton"] });
            xRequestAvpListGrid.SetAllColumnsDefaultView(view);
            xRequestAvpListGrid.InitViewItems();
            xRequestAvpListGrid.DataSourceList = mAct.RequestAvpList;
        }

        private void DeleteAvpToGrid(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void AddAvpToGrid(object sender, RoutedEventArgs e)
        {
            mAct.RequestAvpList.Add(new DiameterAVP());
        }

        private void xViewRawRequestBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void xMessageTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(mAct != null)
            {
                SetMessageDetails(mAct.DiameterMessageType);
            }
        }

        private void SetMessageDetails(DiameterEnums.eDiameterMessageType messageType)
        {
            if (messageType == DiameterEnums.eDiameterMessageType.CapabilitiesExchange)
            {
                mAct.CommandCode = 257;
                mAct.ApplicationId = 0;
                mAct.SetRequestBit = true;
                mAct.RequestAvpList = DiameterUtils.LoadDictionary();
            }
            else if (messageType == DiameterEnums.eDiameterMessageType.CreditControl)
            {
                mAct.CommandCode = 272;
                mAct.ApplicationId = 4;
                mAct.SetRequestBit = true;
            }
        }

        private void GridVEButton_Click(object sender, RoutedEventArgs e)
        {
            DiameterAVP diameterAVP = (DiameterAVP)xRequestAvpListGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(diameterAVP, nameof(DiameterAVP.Value), Context.GetAsContext(mAct.Context));
            VEEW.ShowAsWindow();
        }
    }
}
