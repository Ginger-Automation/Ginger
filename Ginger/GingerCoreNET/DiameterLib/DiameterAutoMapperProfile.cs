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

using AutoMapper;

namespace Amdocs.Ginger.CoreNET.DiameterLib
{
    public class DiameterAutoMapperProfile : Profile
    {
        public DiameterAutoMapperProfile()
        {
            CreateMap<DiameterAvpDictionaryItem, DiameterAVP>()
                .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.AvpDataType))
                .ForMember(dest => dest.VendorId, opt => opt.Ignore())
                .ForMember(dest => dest.Length, opt => opt.Ignore())
                .ForMember(dest => dest.NestedAvpList, opt => opt.Ignore())
                .ForMember(dest => dest.ParentName, opt => opt.Ignore())
                .ForMember(dest => dest.ParentAvpGuid, opt => opt.Ignore());

            CreateMap<DiameterAVP, DiameterAvpDictionaryItem>()
                .ForMember(dest => dest.AvpDataType, opt => opt.MapFrom(src => src.DataType))
                .ReverseMap(); // Enable reverse mapping from DiameterAvpDictionaryItem to DiameterAVP
        }
    }
}
