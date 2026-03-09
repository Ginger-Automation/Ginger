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

using Amdocs.Ginger.Repository;
using AutoMapper;
using GingerCore.Variables;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Amdocs.Ginger.CoreNET.GeneralLib
{
    public static class AutomapData
    {
        public static void AutoMapVariableData(VariableBase customizedVar, ref VariableBase originalVar, bool skipType)
        {
            string varType = string.Empty;
            if (!skipType)
            {
                varType = customizedVar.GetType().Name;
            }

            switch (varType)
            {
                case nameof(VariableDateTime):
                    CreateMapper<VariableDateTime>().Map<VariableDateTime, VariableDateTime>((VariableDateTime)customizedVar, (VariableDateTime)originalVar);
                    break;
                case nameof(VariableDynamic):
                    CreateMapper<VariableDynamic>().Map<VariableDynamic, VariableDynamic>((VariableDynamic)customizedVar, (VariableDynamic)originalVar);
                    break;
                case nameof(VariableNumber):
                    CreateMapper<VariableNumber>().Map<VariableNumber, VariableNumber>((VariableNumber)customizedVar, (VariableNumber)originalVar);
                    break;
                case nameof(VariableString):
                    CreateMapper<VariableString>().Map<VariableString, VariableString>((VariableString)customizedVar, (VariableString)originalVar);
                    break;
                case nameof(VariablePasswordString):
                    CreateMapper<VariablePasswordString>().Map<VariablePasswordString, VariablePasswordString>((VariablePasswordString)customizedVar, (VariablePasswordString)originalVar);
                    break;
                case nameof(VariableRandomNumber):
                    CreateMapper<VariableRandomNumber>().Map<VariableRandomNumber, VariableRandomNumber>((VariableRandomNumber)customizedVar, (VariableRandomNumber)originalVar);
                    break;
                case nameof(VariableRandomString):
                    CreateMapper<VariableRandomString>().Map<VariableRandomString, VariableRandomString>((VariableRandomString)customizedVar, (VariableRandomString)originalVar);
                    break;
                case nameof(VariableSequence):
                    CreateMapper<VariableSequence>().Map<VariableSequence, VariableSequence>((VariableSequence)customizedVar, (VariableSequence)originalVar);
                    break;
                case nameof(VariableTimer):
                    CreateMapper<VariableTimer>().Map<VariableTimer, VariableTimer>((VariableTimer)customizedVar, (VariableTimer)originalVar);
                    break;
                case nameof(VariableSelectionList):
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
