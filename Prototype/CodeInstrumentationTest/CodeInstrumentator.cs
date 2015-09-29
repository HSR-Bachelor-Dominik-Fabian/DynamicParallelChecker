using System;
using System.Linq;
using System.Diagnostics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace CodeInstrumentationTest
{
    class CodeInstrumentator
    {
        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            string fileName = @"TestProgram.exe";
            PrintTypes(fileName);
            Process process = new Process {StartInfo = {FileName = fileName}};
            process.Start();
            Console.WriteLine("Enter to stop");
            Console.ReadLine();
        }

        public static void PrintTypes(string fileName)
        {
            ModuleDefinition refModul = ModuleDefinition.ReadModule("DPCLibrary.dll");
            TypeDefinition typeDefinition =refModul.Types.First(x => x.Name == "DpcLibrary");
            MethodDefinition readAccessDef = typeDefinition.Methods.Single(x => x.Name == "ReadAccess");
            MethodDefinition writeAccessDef = typeDefinition.Methods.Single(x => x.Name == "WriteAccess");
            MethodDefinition lockObjectDef = typeDefinition.Methods.Single(x => x.Name == "LockObject");

            ModuleDefinition module = ModuleDefinition.ReadModule(fileName);
            MethodReference referencedReadAccessMethod = module.Import(readAccessDef);
            MethodReference referencedWriteAccessMethod = module.Import(writeAccessDef);
            MethodReference referencedLockObjectMethod = module.Import(lockObjectDef);
            foreach (TypeDefinition type in module.Types)
            {
                if (type.HasMethods)
                {
                    foreach(MethodDefinition method in type.Methods)
                    {
                        ArrayList tempList = new ArrayList(method.Body.Instructions.ToList());
                        foreach (Instruction ins in tempList)
                        {
                            if (ins.OpCode.Equals(OpCodes.Ldsfld))
                            {
                                FieldDefinition fieldDefinition = (FieldDefinition)ins.Operand;
                                var processor = method.Body.GetILProcessor();
                                var loadAddressInstruction = processor.Create(OpCodes.Ldsflda, fieldDefinition);
                                var readAccessLibraryCall = processor.Create(OpCodes.Call, referencedReadAccessMethod);
                                processor.InsertAfter(ins, loadAddressInstruction);
                                processor.InsertAfter(loadAddressInstruction, readAccessLibraryCall);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Ldsflda) || ins.OpCode.Equals(OpCodes.Ldflda) || ins.OpCode.Equals(OpCodes.Ldelema))
                            {
                                var processor = method.Body.GetILProcessor();
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var readAccessLibraryCall = processor.Create(OpCodes.Call, referencedReadAccessMethod);
                                processor.InsertAfter(ins, dupInstruction);
                                processor.InsertAfter(dupInstruction, readAccessLibraryCall);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Stsfld))
                            {
                                FieldDefinition fieldDefinition = (FieldDefinition) ins.Operand;
                                var processor = method.Body.GetILProcessor();
                                var loadAddressInstruction = processor.Create(OpCodes.Ldsflda, fieldDefinition);
                                var writeAccessLibraryCall = processor.Create(OpCodes.Call, referencedWriteAccessMethod);
                                processor.InsertBefore(ins, writeAccessLibraryCall);
                                processor.InsertBefore(writeAccessLibraryCall, loadAddressInstruction);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Ldfld))
                            {
                                FieldDefinition fieldDefinition = (FieldDefinition) ins.Operand;
                                var processor = method.Body.GetILProcessor();
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var loadAddressInstruction = processor.Create(OpCodes.Ldflda, fieldDefinition);
                                var readAccessLibraryCall = processor.Create(OpCodes.Call, referencedReadAccessMethod);
                                processor.InsertBefore(ins,readAccessLibraryCall);
                                processor.InsertBefore(readAccessLibraryCall,loadAddressInstruction);
                                processor.InsertBefore(loadAddressInstruction, dupInstruction);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Stfld))
                            {
                                FieldDefinition fieldDefinition = (FieldDefinition)ins.Operand;
                                VariableDefinition varDefinition = new VariableDefinition(fieldDefinition.FieldType);
                                method.Body.Variables.Add(varDefinition);
                                var processor = method.Body.GetILProcessor();
                                var storeLocalInstrution = processor.Create(OpCodes.Stloc, varDefinition);
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var loadAddressInstruction = processor.Create(OpCodes.Ldflda, fieldDefinition);
                                var writeAccessLibraryCall = processor.Create(OpCodes.Call, referencedWriteAccessMethod);
                                var loadLocaleInstrucion = processor.Create(OpCodes.Ldloc, varDefinition);


                                processor.InsertBefore(ins, loadLocaleInstrucion);
                                processor.InsertBefore(loadLocaleInstrucion, writeAccessLibraryCall);
                                processor.InsertBefore(writeAccessLibraryCall,loadAddressInstruction);
                                processor.InsertBefore(loadAddressInstruction,dupInstruction);
                                processor.InsertBefore(dupInstruction,storeLocalInstrution);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Ldelem_Any))
                            {
                                TypeReference typeReference = (TypeReference) ins.Operand;
                                InjectArrayLdElement(module, typeReference, method, referencedReadAccessMethod, ins);
                                //TODO:Dominik:Testfall nicht vorhanden
                            }
                            else if(ins.OpCode.Equals(OpCodes.Ldelem_I)|| ins.OpCode.Equals(OpCodes.Ldelem_I1) 
                                || ins.OpCode.Equals(OpCodes.Ldelem_I2) || ins.OpCode.Equals(OpCodes.Ldelem_I4) || ins.OpCode.Equals(OpCodes.Ldelem_I8)
                                || ins.OpCode.Equals(OpCodes.Ldelem_R4) || ins.OpCode.Equals(OpCodes.Ldelem_R8) || ins.OpCode.Equals(OpCodes.Ldelem_Ref)
                                || ins.OpCode.Equals(OpCodes.Ldelem_U1) || ins.OpCode.Equals(OpCodes.Ldelem_U2) || ins.OpCode.Equals(OpCodes.Ldelem_U4))
                            {
                                TypeReference arrayTypeReference = module.Import(typeof(int));
                                InjectArrayLdElement(module, arrayTypeReference, method, referencedReadAccessMethod, ins);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Stelem_Any))
                            {
                                throw new NotImplementedException();
                            }
                            else if (ins.OpCode.Equals(OpCodes.Stelem_I) || ins.OpCode.Equals(OpCodes.Stelem_I1)
                                || ins.OpCode.Equals(OpCodes.Stelem_I2) || ins.OpCode.Equals(OpCodes.Stelem_I4) || ins.OpCode.Equals(OpCodes.Stelem_I8)
                                || ins.OpCode.Equals(OpCodes.Stelem_R4) || ins.OpCode.Equals(OpCodes.Stelem_R8) || ins.OpCode.Equals(OpCodes.Stelem_Ref))
                            {
                                TypeReference typeReference = module.Import(typeof(int));
                                VariableDefinition variableValueDefinition = new VariableDefinition(typeReference);
                                VariableDefinition variableIndexDefinition = new VariableDefinition(typeReference);
                                VariableDefinition variableArrayDefinition = new VariableDefinition(typeReference);

                                method.Body.Variables.Add(variableValueDefinition);
                                method.Body.Variables.Add(variableIndexDefinition);
                                method.Body.Variables.Add(variableArrayDefinition);

                                var processor = method.Body.GetILProcessor();
                                var storeValueInstruction = processor.Create(OpCodes.Stloc, variableValueDefinition);
                                var storeIndexInstrution = processor.Create(OpCodes.Stloc, variableIndexDefinition);
                                var storeArrayInstrution = processor.Create(OpCodes.Stloc, variableArrayDefinition);
                                var loadValueInstrucion = processor.Create(OpCodes.Ldloc, variableValueDefinition);
                                var loadIndexInstrucion = processor.Create(OpCodes.Ldloc, variableIndexDefinition);
                                var loadArrayInstrucion = processor.Create(OpCodes.Ldloc, variableArrayDefinition);
                                var loadIndexInstrucion2 = processor.Create(OpCodes.Ldloc, variableIndexDefinition);
                                var loadArrayInstrucion2 = processor.Create(OpCodes.Ldloc, variableArrayDefinition);
                                var loadAddressInstruction = processor.Create(OpCodes.Ldelema, typeReference);
                                var writeAccessLibraryCall = processor.Create(OpCodes.Call, referencedWriteAccessMethod);

                                processor.InsertBefore(ins,writeAccessLibraryCall);
                                processor.InsertBefore(writeAccessLibraryCall, loadAddressInstruction);
                                processor.InsertBefore(loadAddressInstruction, loadIndexInstrucion2);
                                processor.InsertBefore(loadIndexInstrucion2, loadArrayInstrucion2);
                                processor.InsertBefore(loadArrayInstrucion2, loadValueInstrucion);
                                processor.InsertBefore(loadValueInstrucion, loadIndexInstrucion);
                                processor.InsertBefore(loadIndexInstrucion, loadArrayInstrucion);
                                processor.InsertBefore(loadArrayInstrucion, storeArrayInstrution);
                                processor.InsertBefore(storeArrayInstrution, storeIndexInstrution);
                                processor.InsertBefore(storeIndexInstrution, storeValueInstruction);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Call))
                            {
                                MethodReference methodReference = (MethodReference)ins.Operand;
                                TypeReference typeReference = module.Import(typeof (int));
                                VariableDefinition variableLockDefinition = new VariableDefinition(typeReference);
                                VariableDefinition variableTempDefinition = new VariableDefinition(typeReference);

                                method.Body.Variables.Add(variableLockDefinition);
                                method.Body.Variables.Add(variableTempDefinition);

                                if ("".Equals(methodReference.FullName))
                                {
                                    
                                }
                                var processor = method.Body.GetILProcessor();
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var storeTempInstruction = processor.Create(OpCodes.Stloc, variableTempDefinition);
                                var loadLockInstruction = processor.Create(OpCodes.Ldloc, variableLockDefinition);
                                var loadTempInstruction = processor.Create(OpCodes.Ldloc, variableTempDefinition);
                                var loadLockInstruction2 = processor.Create(OpCodes.Ldloc, variableLockDefinition);
                                var lockObjectLibraryCall = processor.Create(OpCodes.Call, referencedLockObjectMethod);

                                processor.InsertBefore(ins, loadTempInstruction);
                                processor.InsertBefore(loadTempInstruction, lockObjectLibraryCall);
                                processor.InsertBefore(lockObjectLibraryCall, loadTempInstruction);
                                processor.InsertBefore(loadTempInstruction, dupInstruction);
                                processor.InsertBefore(dupInstruction, storeTempInstruction);
                            }
                        }
                    }
                }
                
            }

            module.Write(fileName);
        }

        private static void InjectArrayLdElement(ModuleDefinition module, TypeReference arrayTypeReference, MethodDefinition method,
            MethodReference referencedReadAccessMethod, Instruction ins)
        {
            TypeReference typeReference = module.Import(typeof (int));
            
            VariableDefinition variableIndexDefinition = new VariableDefinition(typeReference);
            VariableDefinition variableArrayDefinition = new VariableDefinition(typeReference);
            method.Body.Variables.Add(variableIndexDefinition);
            method.Body.Variables.Add(variableArrayDefinition);
            var processor = method.Body.GetILProcessor();
            var storeIndexInstrution = processor.Create(OpCodes.Stloc, variableIndexDefinition);
            var storeArrayInstrution = processor.Create(OpCodes.Stloc, variableArrayDefinition);
            var loadIndexInstrucion = processor.Create(OpCodes.Ldloc, variableIndexDefinition);
            var loadArrayInstrucion = processor.Create(OpCodes.Ldloc, variableArrayDefinition);
            var loadIndexInstrucion2 = processor.Create(OpCodes.Ldloc, variableIndexDefinition);
            var loadArrayInstrucion2 = processor.Create(OpCodes.Ldloc, variableArrayDefinition);
            var loadAddressInstruction = processor.Create(OpCodes.Ldelema, arrayTypeReference);
            var readAccessLibraryCall = processor.Create(OpCodes.Call, referencedReadAccessMethod);

            processor.InsertBefore(ins, readAccessLibraryCall);
            processor.InsertBefore(readAccessLibraryCall, loadAddressInstruction);
            processor.InsertBefore(loadAddressInstruction, loadIndexInstrucion2);
            processor.InsertBefore(loadIndexInstrucion2, loadArrayInstrucion2);
            processor.InsertBefore(loadArrayInstrucion2, loadIndexInstrucion);
            processor.InsertBefore(loadIndexInstrucion, loadArrayInstrucion);
            processor.InsertBefore(loadArrayInstrucion, storeArrayInstrution);
            processor.InsertBefore(storeArrayInstrution, storeIndexInstrution);
        }
    }
}
