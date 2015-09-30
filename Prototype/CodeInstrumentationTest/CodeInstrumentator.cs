﻿using System;
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
            MethodDefinition unlockObjectDef = typeDefinition.Methods.Single(x => x.Name == "UnLockObject");

            ModuleDefinition module = ModuleDefinition.ReadModule(fileName);
            MethodReference referencedReadAccessMethod = module.Import(readAccessDef);
            MethodReference referencedWriteAccessMethod = module.Import(writeAccessDef);
            MethodReference referencedLockObjectMethod = module.Import(lockObjectDef);
            MethodReference referencedUnlockObjectMethod = module.Import(unlockObjectDef);
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
                                TypeReference indexTypeReference = module.Import(typeof(int));
                                TypeReference valueTypeReference = (TypeReference)ins.Operand;
                                InjectStrElement(valueTypeReference, indexTypeReference, method, referencedWriteAccessMethod, ins);
                                //TODO:Dominik:Testfall nicht vorhanden
                            }
                            else if (ins.OpCode.Equals(OpCodes.Stelem_I) || ins.OpCode.Equals(OpCodes.Stelem_I1)
                                     || ins.OpCode.Equals(OpCodes.Stelem_I2) || ins.OpCode.Equals(OpCodes.Stelem_I4))
                            {
                                
                            }
                            else if (ins.OpCode.Equals(OpCodes.Stelem_I8))
                            {
                                
                            }
                            else if( ins.OpCode.Equals(OpCodes.Stelem_R4) || ins.OpCode.Equals(OpCodes.Stelem_R8) || ins.OpCode.Equals(OpCodes.Stelem_Ref))
                            {
                                TypeReference indexTypeReference = module.Import(typeof(int));
                                TypeReference valueTypeReference = module.Import(typeof(long));
                                InjectStrElement(valueTypeReference, indexTypeReference, method, referencedWriteAccessMethod, ins);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Call))
                            {
                                MethodReference methodReference = (MethodReference)ins.Operand;
                                TypeReference typeReference = module.Import(typeof (int));
                                VariableDefinition variableTempDefinition = new VariableDefinition(typeReference);

                                method.Body.Variables.Add(variableTempDefinition);
                                string monitorEnterFullName =
                                    "System.Void System.Threading.Monitor::Enter(System.Object,System.Boolean&)";
                                string monitorExitFullName = "System.Void System.Threading.Monitor::Exit(System.Object)";

                                if (monitorEnterFullName.Equals(methodReference.FullName))
                                {
                                    // TODO:Fabian Bessere Lösung für Vergleich finden.
                                    var processor = method.Body.GetILProcessor();
                                    var dupInstruction = processor.Create(OpCodes.Dup);
                                    var storeTempInstruction = processor.Create(OpCodes.Stloc, variableTempDefinition);
                                    var loadTempInstruction = processor.Create(OpCodes.Ldloc, variableTempDefinition);
                                    var lockObjectLibraryCall = processor.Create(OpCodes.Call, referencedLockObjectMethod);

                                    processor.InsertBefore(ins, loadTempInstruction);
                                    processor.InsertBefore(loadTempInstruction, lockObjectLibraryCall);
                                    processor.InsertBefore(lockObjectLibraryCall, dupInstruction);
                                    processor.InsertBefore(dupInstruction, storeTempInstruction);
                                }
                                else if (monitorExitFullName.Equals(methodReference.FullName))
                                {
                                    var processor = method.Body.GetILProcessor();
                                    var dupInstruction = processor.Create(OpCodes.Dup);
                                    var unlockObjectLibraryCall = processor.Create(OpCodes.Call, referencedUnlockObjectMethod);

                                    processor.InsertBefore(ins, unlockObjectLibraryCall);
                                    processor.InsertBefore(unlockObjectLibraryCall, dupInstruction);
                                }
                            }

                        }
                    }
                }
                
            }

            module.Write(fileName);
        }

        private static void InjectStrElement(TypeReference valueTypeReference, TypeReference indexTypeReference, MethodDefinition method,
            MethodReference referencedWriteAccessMethod, Instruction ins)
        {
            //TypeReference valueTypeReference = getValueTypeReferenceStelem(ins);
            VariableDefinition variableValueDefinition = new VariableDefinition(valueTypeReference);
            VariableDefinition variableIndexDefinition = new VariableDefinition(indexTypeReference);

            method.Body.Variables.Add(variableValueDefinition);
            method.Body.Variables.Add(variableIndexDefinition);

            var processor = method.Body.GetILProcessor();
            var dupInstruction = processor.Create(OpCodes.Dup);
            var storeValueInstruction = processor.Create(OpCodes.Stloc, variableValueDefinition);
            var storeIndexInstruction = processor.Create(OpCodes.Stloc, variableIndexDefinition);
            var loadIndexInstruction = processor.Create(OpCodes.Ldloc, variableIndexDefinition);
            var loadIndexInstruction2 = processor.Create(OpCodes.Ldloc, variableIndexDefinition);
            var loadValueInstruction = processor.Create(OpCodes.Ldloc, variableValueDefinition);
            var loadAddressInstruction = processor.Create(OpCodes.Ldelema, indexTypeReference);
            var writeAccessLibraryCall = processor.Create(OpCodes.Call, referencedWriteAccessMethod);

            //processor.InsertBefore(ins, loadValueInstruction);
            //processor.InsertBefore(loadValueInstruction, loadIndexInstruction);
            //processor.InsertBefore(loadIndexInstruction, storeIndexInstruction);
            //processor.InsertBefore(storeIndexInstruction, storeValueInstruction);
            
            processor.InsertBefore(ins, loadValueInstruction);
            processor.InsertBefore(loadValueInstruction, loadIndexInstruction);
            processor.InsertBefore(loadIndexInstruction, writeAccessLibraryCall);
            processor.InsertBefore(writeAccessLibraryCall, loadAddressInstruction);
            processor.InsertBefore(loadAddressInstruction, loadIndexInstruction2);
            processor.InsertBefore(loadIndexInstruction2, dupInstruction);
            processor.InsertBefore(dupInstruction, storeIndexInstruction);
            processor.InsertBefore(storeIndexInstruction, storeValueInstruction);

            /*processor.InsertBefore(ins, loadValueInstrucion);
            processor.InsertBefore(loadValueInstruction, loadIndexInstruction);
            processor.InsertBefore(loadIndexInstruction, loadArrayInstruction);
            processor.InsertBefore(loadArrayInstruction, storeArrayInstruction);
            processor.InsertBefore(storeArrayInstruction, storeIndexInstruction);
            processor.InsertBefore(storeIndexInstruction, storeValueInstruction);*/
        }

        private static TypeReference getValueTypeReferenceStelem(Instruction ins)
        {
            /*TypeReference valueTypeReference;
            if (ins.OpCode.Equals(OpCodes.Stelem_))
            {
                
            }*/
            return null;
        }

        private static void InjectArrayLdElement(ModuleDefinition module, TypeReference arrayTypeReference, MethodDefinition method,
            MethodReference referencedReadAccessMethod, Instruction ins)
        {
            TypeReference typeReference = module.Import(typeof (int));
            
            VariableDefinition variableIndexDefinition = new VariableDefinition(typeReference);
            method.Body.Variables.Add(variableIndexDefinition);
            var processor = method.Body.GetILProcessor();
            var storeIndexInstrution = processor.Create(OpCodes.Stloc, variableIndexDefinition);
            var dupInstruction = processor.Create(OpCodes.Dup);
            var loadIndexInstrucion = processor.Create(OpCodes.Ldloc, variableIndexDefinition);
            var loadIndexInstrucion2 = processor.Create(OpCodes.Ldloc, variableIndexDefinition);
            var loadAddressInstruction = processor.Create(OpCodes.Ldelema, arrayTypeReference);
            var readAccessLibraryCall = processor.Create(OpCodes.Call, referencedReadAccessMethod);

            processor.InsertBefore(ins, loadIndexInstrucion2);
            processor.InsertBefore(loadIndexInstrucion2, readAccessLibraryCall);
            processor.InsertBefore(readAccessLibraryCall, loadAddressInstruction);
            processor.InsertBefore(loadAddressInstruction, loadIndexInstrucion);
            processor.InsertBefore(loadIndexInstrucion, dupInstruction);
            processor.InsertBefore(dupInstruction, storeIndexInstrution);
        }
    }
}
