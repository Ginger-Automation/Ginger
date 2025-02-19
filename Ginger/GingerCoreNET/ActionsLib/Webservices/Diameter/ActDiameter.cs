#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common.Enums;
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
        public ActDiameter()
        {
            //Disable Auto Screenshot on failure by default. User can override it if needed
            AutoScreenShotOnFailure = false;
        }

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

        public override eImageType Image { get { return eImageType.Exchange; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText($"Use this action in case you want to send messages using the Diameter protocol.{System.Environment.NewLine}");
            TBH.AddText($"Diameter protocol is a robust and extensible protocol used primarily in telecommunications and networking contexts and is designed to provide various authentication, authorization, and accounting (AAA) services.{System.Environment.NewLine}");
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
        private ObservableList<DiameterAVP> mRequestAvpList = [];
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

        private ObservableList<DiameterAVP> mCustomResponseAvpList = [];
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
            List<ObservableList<ActInputValue>> list =
            [
                AVPToAIVConverter()
            ];

            return list;
        }
        private ObservableList<ActInputValue> AVPToAIVConverter()
        {
            ObservableList<ActInputValue> AIVList = [.. RequestAvpList, .. CustomResponseAvpList];
            return AIVList;
        }
    }
}
