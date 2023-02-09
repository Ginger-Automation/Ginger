#region License
/*
Copyright © 2014-2023 European Support Limited

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

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace GingerCore.NoSqlBase
{
    namespace DataAccess
    {
        public class MyClassBuilder
        {
            AssemblyName asemblyName;

            public MyClassBuilder()
            {
            }

            public MyClassBuilder(string ClassName)
            {
                this.asemblyName = new AssemblyName(ClassName);
            }

            public object CreateObject(string[] PropertyNames, Type[] Types)
            {
                if (PropertyNames.Length != Types.Length)
                {
                    Console.WriteLine("The number of property names should match their corresponding types number");
                }

                TypeBuilder DynamicClass = this.CreateClass();
                this.CreateConstructor(DynamicClass);
                for (int ind = 0; ind < PropertyNames.Count(); ind++)
                    CreateProperty(DynamicClass, PropertyNames[ind], Types[ind]);
                Type type = DynamicClass.CreateTypeInfo();
                
                return Activator.CreateInstance(type);
            }

            private TypeBuilder CreateClass()
            {
                AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(this.asemblyName, AssemblyBuilderAccess.Run);
                ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
                TypeBuilder typeBuilder = moduleBuilder.DefineType(this.asemblyName.FullName
                                    , TypeAttributes.Public |
                                    TypeAttributes.Class |
                                    TypeAttributes.AutoClass |
                                    TypeAttributes.AnsiClass |
                                    TypeAttributes.BeforeFieldInit |
                                    TypeAttributes.AutoLayout
                                    , null);
                return typeBuilder;
            }
            private void CreateConstructor(TypeBuilder typeBuilder)
            {
                typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            }
            private void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
            {
                FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
                MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
                ILGenerator getIl = getPropMthdBldr.GetILGenerator();

                getIl.Emit(OpCodes.Ldarg_0);
                getIl.Emit(OpCodes.Ldfld, fieldBuilder);
                getIl.Emit(OpCodes.Ret);

                MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod("set_" + propertyName,
                      MethodAttributes.Public |
                      MethodAttributes.SpecialName |
                      MethodAttributes.HideBySig,
                      null, new[] { propertyType });

                ILGenerator setIl = setPropMthdBldr.GetILGenerator();
                Label modifyProperty = setIl.DefineLabel();
                Label exitSet = setIl.DefineLabel();

                setIl.MarkLabel(modifyProperty);
                setIl.Emit(OpCodes.Ldarg_0);
                setIl.Emit(OpCodes.Ldarg_1);
                setIl.Emit(OpCodes.Stfld, fieldBuilder);

                setIl.Emit(OpCodes.Nop);
                setIl.MarkLabel(exitSet);
                setIl.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getPropMthdBldr);
                propertyBuilder.SetSetMethod(setPropMthdBldr);
            }
        }
    }
}