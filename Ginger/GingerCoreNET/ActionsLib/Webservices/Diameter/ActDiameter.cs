using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.DiameterLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
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

        public bool IsProxiableBitSet
        {
            get
            {
                bool value;
                bool.TryParse(GetOrCreateInputParam(nameof(IsProxiableBitSet), false.ToString()).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(IsProxiableBitSet), value.ToString());
                OnPropertyChanged(nameof(IsProxiableBitSet));
            }
        }

        public bool IsRequestBitSet
        {
            get
            {
                bool value;
                bool.TryParse(GetOrCreateInputParam(nameof(IsRequestBitSet), false.ToString()).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(IsRequestBitSet), value.ToString());
                OnPropertyChanged(nameof(IsRequestBitSet));
            }
        }
        public bool IsErrorBitSet
        {
            get
            {
                bool value;
                bool.TryParse(GetOrCreateInputParam(nameof(IsErrorBitSet), false.ToString()).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(IsErrorBitSet), value.ToString());
                OnPropertyChanged(nameof(IsErrorBitSet));
            }
        }
        private ObservableList<DiameterAVP> mRequestAvpList = new ObservableList<DiameterAVP>();
        [IsSerializedForLocalRepository]
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

        private ObservableList<DiameterAVP> mCustomResponseAvpList = new ObservableList<DiameterAVP>();
        [IsSerializedForLocalRepository]
        public ObservableList<DiameterAVP> CustomResponseAvpList
        {
            get
            {
                return mCustomResponseAvpList;
            }
            set
            {
                if (mCustomResponseAvpList != value)
                {
                    mCustomResponseAvpList = value;
                    OnPropertyChanged(nameof(mCustomResponseAvpList));
                }
            }
        }
        public override List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing()
        {
            List<ObservableList<ActInputValue>> list = new List<ObservableList<ActInputValue>>
            {
                AVPToAIVConverter()
            };

            return list;
        }
        private ObservableList<ActInputValue> AVPToAIVConverter()
        {
            ObservableList<ActInputValue> AIVList = new ObservableList<ActInputValue>();
            foreach (DiameterAVP requestAvp in RequestAvpList)
            {
                AIVList.Add(requestAvp);
            }
            foreach (DiameterAVP customResponseAvp in CustomResponseAvpList)
            {
                AIVList.Add(customResponseAvp);
            }
            return AIVList;
        }
    }
}
