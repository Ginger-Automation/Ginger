using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.DiameterLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.DataSource;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using static Amdocs.Ginger.CoreNET.DiameterLib.DiameterEnums;

namespace Amdocs.Ginger.CoreNET.ActionsLib.Webservices.Diameter
{
    public class ActDiameter : Act
    {
        public override string ActionType { get { return ActionDescription; } }

        public override string ActionDescription { get { return "Diameter Action"; } }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }

        public override bool ValueConfigsNeeded { get { return false; } }

        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.WebServices);
                }
                return mPlatforms;
            }
        }
        public override string ActionEditPage { get { return "WebServices.ActDiameterEditPage"; } }

        public override string ActionUserDescription { get { return "Performs Diameter action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you want to send Diameter messages");
        }
        public eDiameterMessageType DiameterMessageType
        {
            get
            {
                return GetOrCreateInputParam(nameof(DiameterMessageType), eDiameterMessageType.None);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(DiameterMessageType), value.ToString());
                OnPropertyChanged(nameof(DiameterMessageType));
            }
        }
        public int CommandCode
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(CommandCode), 0.ToString()).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(CommandCode), value.ToString());
                OnPropertyChanged(nameof(CommandCode));
            }
        }
        public int ApplicationId
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(ApplicationId), 0.ToString()).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ApplicationId), value.ToString());
                OnPropertyChanged(nameof(ApplicationId));
            }
        }
        public int HopByHopIdentifier
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(HopByHopIdentifier), 1.ToString()).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(HopByHopIdentifier), value.ToString());
                OnPropertyChanged(nameof(HopByHopIdentifier));
            }
        }
        public int EndToEndIdentifier
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(EndToEndIdentifier), 1.ToString()).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(EndToEndIdentifier), value.ToString());
                OnPropertyChanged(nameof(EndToEndIdentifier));
            }
        }

        public bool SetProxiableBit
        {
            get
            {
                bool value;
                bool.TryParse(GetOrCreateInputParam(nameof(SetProxiableBit), false.ToString()).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SetProxiableBit), value.ToString());
                OnPropertyChanged(nameof(SetProxiableBit));
            }
        }

        public bool SetRequestBit
        {
            get
            {
                bool value;
                bool.TryParse(GetOrCreateInputParam(nameof(SetRequestBit), false.ToString()).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SetRequestBit), value.ToString());
                OnPropertyChanged(nameof(SetRequestBit));
            }
        }
        public bool SetErrorBit
        {
            get
            {
                bool value;
                bool.TryParse(GetOrCreateInputParam(nameof(SetErrorBit), false.ToString()).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SetErrorBit), value.ToString());
                OnPropertyChanged(nameof(SetErrorBit));
            }
        }
        public bool SetRetransmitBit
        {
            get
            {
                bool value;
                bool.TryParse(GetOrCreateInputParam(nameof(SetRetransmitBit), false.ToString()).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SetRetransmitBit), value.ToString());
                OnPropertyChanged(nameof(SetRetransmitBit));
            }
        }
        private ObservableList<ActDiameterAvp> mRequestAvpList = new ObservableList<ActDiameterAvp>();
        [IsSerializedForLocalRepository]
        public ObservableList<ActDiameterAvp> RequestAvpList
        {
            get
            {
                return mRequestAvpList;
            }
            set
            {
                if (mRequestAvpList != value)
                {
                    mRequestAvpList = value;
                    OnPropertyChanged(nameof(mRequestAvpList));
                }
            }
        }
        public override List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing()
        {
            List<ObservableList<ActInputValue>> list = new List<ObservableList<ActInputValue>>();
            list.Add(FormDataToAIVConverter());

            return list;
        }
        private ObservableList<ActInputValue> FormDataToAIVConverter()
        {
            ObservableList<ActInputValue> fa = new ObservableList<ActInputValue>();
            foreach (ActDiameterAvp reqAvp in RequestAvpList)
            {
                fa.Add((ActInputValue)reqAvp);
            }
            return fa;
        }
    }
}
