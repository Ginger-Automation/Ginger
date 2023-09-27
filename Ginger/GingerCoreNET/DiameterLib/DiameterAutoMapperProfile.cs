using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
