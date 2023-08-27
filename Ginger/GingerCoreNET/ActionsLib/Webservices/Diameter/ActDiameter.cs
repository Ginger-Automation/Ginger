using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.DiameterLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.DataSource;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Collections.Generic;
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
                return GetOrCreateInputParam(nameof(DiameterMessageType), eDiameterMessageType.CapabilitiesExchange);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(DiameterMessageType), value.ToString());
                //OnPropertyChanged(nameof(DiameterMessageType));
            }
        }
        public int CommandCode
        {
            get
            {
                return GetOrCreateInputParam(nameof(CommandCode), 0);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(CommandCode), value.ToString());
            }
        }
        public int ApplicationId
        {
            get
            {
                return GetOrCreateInputParam(nameof(ApplicationId), 0);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ApplicationId), value.ToString());
            }
        }
        public int HopByHopIdentifier
        {
            get
            {
                return GetOrCreateInputParam(nameof(HopByHopIdentifier), 1);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(HopByHopIdentifier), value.ToString());
            }
        }
        public int EndToEndIdentifier
        {
            get
            {
                return GetOrCreateInputParam(nameof(EndToEndIdentifier), 1);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(EndToEndIdentifier), value.ToString());
            }
        }

        public bool SetProxiableBit
        {
            get
            {
                return bool.Parse(GetOrCreateInputParam(nameof(SetProxiableBit), false.ToString()).Value);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SetProxiableBit), value.ToString());
            }
        }

        public bool SetRequestBit
        {
            get
            {
                return bool.Parse(GetOrCreateInputParam(nameof(SetRequestBit), false.ToString()).Value);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SetRequestBit), value.ToString());
            }
        }
        public bool SetErrorBit
        {
            get
            {
                return bool.Parse(GetOrCreateInputParam(nameof(SetErrorBit), false.ToString()).Value);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SetErrorBit), value.ToString());
            }
        }
        public bool SetRetransmitBit
        {
            get
            {
                return bool.Parse(GetOrCreateInputParam(nameof(SetRetransmitBit), false.ToString()).Value);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SetRetransmitBit), value.ToString());
            }
        }
        private ObservableList<DiameterAVP> mRequestAvpList;
        public ObservableList<DiameterAVP> RequestAvpList
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
    }
}
