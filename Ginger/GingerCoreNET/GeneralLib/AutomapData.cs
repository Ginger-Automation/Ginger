using AutoMapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using GingerCore.Variables;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.CoreNET.GeneralLib
{
    public static class AutomapData
    {
        public static void AutoMapVariableData(VariableBase customizedVar, ref VariableBase originalVar, bool skipType)
        {
            string varType = string.Empty;
            if(!skipType)
            {
               varType = customizedVar.GetType().Name;
            }                
            
            switch (varType)
            {
                case "VariableDateTime":
                    CreateMapper<VariableDateTime>().Map<VariableDateTime, VariableDateTime>((VariableDateTime)customizedVar, (VariableDateTime)originalVar);
                    break;
                case "VariableDynamic":
                    CreateMapper<VariableDynamic>().Map<VariableDynamic, VariableDynamic>((VariableDynamic)customizedVar, (VariableDynamic)originalVar);
                    break;
                case "VariableNumber":
                    CreateMapper<VariableNumber>().Map<VariableNumber, VariableNumber>((VariableNumber)customizedVar, (VariableNumber)originalVar);
                    break;
                case "VariableString":
                    CreateMapper<VariableString>().Map<VariableString, VariableString>((VariableString)customizedVar, (VariableString)originalVar);
                    break;
                case "VariablePasswordString":
                    CreateMapper<VariablePasswordString>().Map<VariablePasswordString, VariablePasswordString>((VariablePasswordString)customizedVar, (VariablePasswordString)originalVar);
                    break;
                case "VariableRandomNumber":
                    CreateMapper<VariableRandomNumber>().Map<VariableRandomNumber, VariableRandomNumber>((VariableRandomNumber)customizedVar, (VariableRandomNumber)originalVar);
                    break;
                case "VariableRandomString":
                    CreateMapper<VariableRandomString>().Map<VariableRandomString, VariableRandomString>((VariableRandomString)customizedVar, (VariableRandomString)originalVar);
                    break;
                case "VariableSequence":
                    CreateMapper<VariableSequence>().Map<VariableSequence, VariableSequence>((VariableSequence)customizedVar, (VariableSequence)originalVar);
                    break;
                case "VariableTimer":
                    CreateMapper<VariableTimer>().Map<VariableTimer, VariableTimer>((VariableTimer)customizedVar, (VariableTimer)originalVar);
                    break;                
                case "VariableSelectionList":
                    CreateMapper<VariableSelectionList>().Map<VariableSelectionList, VariableSelectionList>((VariableSelectionList)customizedVar, (VariableSelectionList)originalVar);
                    break;
                default:
                    CreateMapper<VariableBase>().Map<VariableBase, VariableBase>(customizedVar, originalVar);
                    break;
            }
        }

        private static IMapper CreateMapper<T>()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<List<string>, List<string>>().ConvertUsing(new IgnoringNullValuesTypeConverter<List<string>>());
                cfg.CreateMap<List<OptionalValue>, List<OptionalValue>>().ConvertUsing(new IgnoringNullValuesTypeConverter<List<OptionalValue>>());
                cfg.CreateMap<List<Guid>, List<Guid>>().ConvertUsing(new IgnoringNullValuesTypeConverter<List<Guid>>());
                cfg.CreateMap<T, T>()
               .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            });
            return config.CreateMapper();
        }

        private class IgnoringNullValuesTypeConverter<T> : ITypeConverter<T, T> where T : class
        {
            public T Convert(T source, T destination, ResolutionContext context)
            {
                if (source is IList && ((IList)source).Count == 0)
                {
                    return destination;
                }
                else
                {
                    return source;
                }
            }
        }
    }
}
