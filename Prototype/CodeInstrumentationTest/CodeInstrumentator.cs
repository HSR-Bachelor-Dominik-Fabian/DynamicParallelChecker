using System;
using System.Linq;
using System.Diagnostics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using Mono.Cecil.Rocks;

namespace CodeInstrumentationTest
{
    internal class CodeInstrumentator
    {
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            string fileName = @"TestProgram.exe";
            InjectCodeInstrumentation(fileName);
            Process process = new Process {StartInfo = {FileName = fileName}};
            process.Start();
            Console.WriteLine("Enter to stop");
            Console.ReadLine();
        }

        private static void InjectCodeInstrumentation(string fileName)
        {
            ModuleDefinition refModul = ModuleDefinition.ReadModule("DPCLibrary.dll");
            TypeDefinition typeDefinition = refModul.Types.First(x => x.Name == "DpcLibrary");
            MethodDefinition readAccessDef = typeDefinition.Methods.Single(x => x.Name == "ReadAccess");
            MethodDefinition writeAccessDef = typeDefinition.Methods.Single(x => x.Name == "WriteAccess");
            MethodDefinition lockObjectDef = typeDefinition.Methods.Single(x => x.Name == "LockObject");
            MethodDefinition unlockObjectDef = typeDefinition.Methods.Single(x => x.Name == "UnLockObject");
            MethodDefinition genericListDef = typeDefinition.Methods.Single(x => x.Name == "GenericList");

            ModuleDefinition module = ModuleDefinition.ReadModule(fileName);
            MethodReference referencedReadAccessMethod = module.Import(readAccessDef);
            MethodReference referencedWriteAccessMethod = module.Import(writeAccessDef);
            MethodReference referencedLockObjectMethod = module.Import(lockObjectDef);
            MethodReference referencedUnlockObjectMethod = module.Import(unlockObjectDef);
            MethodReference referencedGenericListMethod = module.Import(genericListDef);

            TypeReference int8TypeReference = module.Import(typeof (byte));
            TypeReference int16TypeReference = module.Import(typeof (short));
            TypeReference int32TypeReference = module.Import(typeof (int));
            TypeReference int64TypeReference = module.Import(typeof (long));
            TypeReference float32TypeReference = module.Import(typeof (float));
            TypeReference float64TypeReference = module.Import(typeof (double));
            foreach (TypeDefinition type in module.Types)
            {
                if (type.HasMethods)
                {
                    foreach (MethodDefinition method in type.Methods)
                    {
                        method.Body.SimplifyMacros(); // convert every br.s (short branch) to a normal branch
                        VariableDefinition firstInt32VariableDefinition = new VariableDefinition(int32TypeReference);
                        VariableDefinition secondInt32VariableDefinition = new VariableDefinition(int32TypeReference);
                        VariableDefinition int8VariableDefinition = new VariableDefinition(int8TypeReference);
                        VariableDefinition int16VariableDefinition = new VariableDefinition(int16TypeReference);
                        VariableDefinition int64VariableDefinition = new VariableDefinition(int64TypeReference);
                        VariableDefinition float32VariableDefinition = new VariableDefinition(float32TypeReference);
                        VariableDefinition float64VariableDefinition = new VariableDefinition(float64TypeReference);

                        method.Body.Variables.Add(firstInt32VariableDefinition);
                        method.Body.Variables.Add(secondInt32VariableDefinition);
                        method.Body.Variables.Add(int8VariableDefinition);
                        method.Body.Variables.Add(int16VariableDefinition);
                        method.Body.Variables.Add(int64VariableDefinition);
                        method.Body.Variables.Add(float32VariableDefinition);
                        method.Body.Variables.Add(float64VariableDefinition);

                        ArrayList tempList = new ArrayList(method.Body.Instructions.ToList());
                        foreach (Instruction ins in tempList)
                        {
                            if (ins.OpCode.Equals(OpCodes.Ldsfld))
                            {
                                FieldDefinition fieldDefinition = (FieldDefinition) ins.Operand;
                                TypeReference fieldType = (TypeReference) fieldDefinition.FieldType;
                                var processor = method.Body.GetILProcessor();
                                if (fieldType.IsPrimitive || (fieldType.IsDefinition && ((TypeDefinition)fieldType).IsValueType))
                                {
                                    var loadAddressInstruction = processor.Create(OpCodes.Ldsflda, fieldDefinition);
                                    var readAccessLibraryCall = processor.Create(OpCodes.Call, referencedReadAccessMethod);
                                    processor.InsertAfter(ins, loadAddressInstruction);
                                    processor.InsertAfter(loadAddressInstruction, readAccessLibraryCall);
                                }
                                else if (!fieldType.IsPrimitive && !fieldType.IsValueType)
                                {
                                    var dupInstruction = processor.Create(OpCodes.Dup);
                                    var readAccessLibraryCall = processor.Create(OpCodes.Call, referencedReadAccessMethod);
                                    var readAccessLibraryCall2 = processor.Create(OpCodes.Call, referencedReadAccessMethod);
                                    var loadAddressInstruction = processor.Create(OpCodes.Ldsflda, fieldDefinition);
                                    processor.InsertAfter(ins, dupInstruction);
                                    processor.InsertAfter(dupInstruction, readAccessLibraryCall);
                                    processor.InsertAfter(readAccessLibraryCall, loadAddressInstruction);
                                    processor.InsertAfter(loadAddressInstruction, readAccessLibraryCall2);
                                }
                            }
                            else if (ins.OpCode.Equals(OpCodes.Initobj))
                            {
                                TypeReference fieldType = (TypeReference) ins.Operand;
                                if (fieldType.IsDefinition && ((TypeDefinition) fieldType).IsValueType)
                                {
                                    // TODO: Mehr Fälle bei denen initobj beachtet werden muss?
                                    var processor = method.Body.GetILProcessor();
                                    var dupInstruction = processor.Create(OpCodes.Dup);
                                    var writeAccessLibraryCall = processor.Create(OpCodes.Call, referencedWriteAccessMethod);
                                    processor.InsertBefore(ins, writeAccessLibraryCall);
                                    processor.InsertBefore(writeAccessLibraryCall, dupInstruction);
                                }
                            }
                            else if (ins.OpCode.Equals(OpCodes.Stsfld))
                            {
                                FieldDefinition fieldDefinition = (FieldDefinition) ins.Operand;
                                var processor = method.Body.GetILProcessor();
                                var loadAddressInstruction = processor.Create(OpCodes.Ldsflda, fieldDefinition);
                                var writeAccessLibraryCall = processor.Create(OpCodes.Call,
                                    referencedWriteAccessMethod);
                                processor.InsertBefore(ins, writeAccessLibraryCall);
                                processor.InsertBefore(writeAccessLibraryCall, loadAddressInstruction);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Ldfld))
                            {
                                FieldDefinition fieldDefinition = (FieldDefinition) ins.Operand;
                                TypeReference fieldType = (TypeReference)fieldDefinition.FieldType;
                                var processor = method.Body.GetILProcessor();
                                if (fieldType.IsPrimitive || (fieldType.IsDefinition && ((TypeDefinition)fieldType).IsValueType))
                                {
                                    var dupInstruction = processor.Create(OpCodes.Dup);
                                    var loadAddressInstruction = processor.Create(OpCodes.Ldflda, fieldDefinition);
                                    var readAccessLibraryCall = processor.Create(OpCodes.Call,
                                        referencedReadAccessMethod);
                                    processor.InsertBefore(ins, readAccessLibraryCall);
                                    processor.InsertBefore(readAccessLibraryCall, loadAddressInstruction);
                                    processor.InsertBefore(loadAddressInstruction, dupInstruction);
                                }
                                else if (!fieldType.IsPrimitive && !fieldType.IsValueType)
                                {
                                    var dupInstruction = processor.Create(OpCodes.Dup);
                                    var readAccessLibraryCall = processor.Create(OpCodes.Call,
                                        referencedReadAccessMethod);
                                    processor.InsertBefore(ins, readAccessLibraryCall);
                                    processor.InsertBefore(readAccessLibraryCall, dupInstruction);
                                }
                            }
                            else if (ins.OpCode.Equals(OpCodes.Stfld))
                            {
                                FieldDefinition fieldDefinition = (FieldDefinition) ins.Operand;
                                VariableDefinition varDefinition;

                                if (fieldDefinition.FieldType.Equals(int8TypeReference))
                                {
                                    varDefinition = int8VariableDefinition;
                                }
                                else if (fieldDefinition.FieldType.Equals(int16TypeReference))
                                {
                                    varDefinition = int16VariableDefinition;
                                }
                                else if (fieldDefinition.FieldType.Equals(int32TypeReference))
                                {
                                    varDefinition = firstInt32VariableDefinition;
                                }
                                else if (fieldDefinition.FieldType.Equals(int64TypeReference))
                                {
                                    varDefinition = int64VariableDefinition;
                                }
                                else if (fieldDefinition.FieldType.Equals(float32TypeReference))
                                {
                                    varDefinition = float32VariableDefinition;
                                }
                                else if (fieldDefinition.FieldType.Equals(float64TypeReference))
                                {
                                    varDefinition = float64VariableDefinition;
                                }
                                else
                                {
                                    varDefinition = new VariableDefinition(fieldDefinition.FieldType);
                                    method.Body.Variables.Add(varDefinition);
                                }
                                var processor = method.Body.GetILProcessor();
                                var storeLocalInstrution = processor.Create(OpCodes.Stloc, varDefinition);
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var loadAddressInstruction = processor.Create(OpCodes.Ldflda, fieldDefinition);
                                var writeAccessLibraryCall = processor.Create(OpCodes.Call,
                                    referencedWriteAccessMethod);
                                var loadLocaleInstrucion = processor.Create(OpCodes.Ldloc, varDefinition);


                                processor.InsertBefore(ins, loadLocaleInstrucion);
                                processor.InsertBefore(loadLocaleInstrucion, writeAccessLibraryCall);
                                processor.InsertBefore(writeAccessLibraryCall, loadAddressInstruction);
                                processor.InsertBefore(loadAddressInstruction, dupInstruction);
                                processor.InsertBefore(dupInstruction, storeLocalInstrution);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Ldelem_Any))
                            {
                                TypeReference arrayTypeReference = (TypeReference) ins.Operand;
                                InjectArrayLdElement(firstInt32VariableDefinition, arrayTypeReference, method,
                                    referencedReadAccessMethod, ins);
                                //TODO:Dominik:Testfall nicht vorhanden
                            }
                            else if (ins.OpCode.Equals(OpCodes.Ldelem_I) || ins.OpCode.Equals(OpCodes.Ldelem_I1)
                                     || ins.OpCode.Equals(OpCodes.Ldelem_I2) || ins.OpCode.Equals(OpCodes.Ldelem_I4) ||
                                     ins.OpCode.Equals(OpCodes.Ldelem_Ref) || ins.OpCode.Equals(OpCodes.Ldelem_U1) || ins.OpCode.Equals(OpCodes.Ldelem_U2) ||
                                     ins.OpCode.Equals(OpCodes.Ldelem_U4))
                            {
                                InjectArrayLdElement(firstInt32VariableDefinition, int32TypeReference, method,
                                    referencedReadAccessMethod, ins);
                            }     
                            else if (ins.OpCode.Equals(OpCodes.Ldelem_I8))
                            {
                                InjectArrayLdElement(firstInt32VariableDefinition, int64TypeReference, method,
                                    referencedReadAccessMethod, ins);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Ldelem_R4))
                            {
                                InjectArrayLdElement(firstInt32VariableDefinition, float32TypeReference, method,
                                    referencedReadAccessMethod, ins);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Ldelem_R8))
                            {
                                InjectArrayLdElement(firstInt32VariableDefinition, float64TypeReference, method,
                                    referencedReadAccessMethod, ins);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Stelem_Any))
                            {
                                TypeReference valueTypeReference = (TypeReference) ins.Operand;
                                VariableDefinition varDefinition;

                                if (valueTypeReference.Equals(int8TypeReference))
                                {
                                    varDefinition = int8VariableDefinition;
                                }
                                else if (valueTypeReference.Equals(int16TypeReference))
                                {
                                    varDefinition = int16VariableDefinition;
                                }
                                else if (valueTypeReference.Equals(int32TypeReference))
                                {
                                    varDefinition = firstInt32VariableDefinition;
                                }
                                else if (valueTypeReference.Equals(int64TypeReference))
                                {
                                    varDefinition = int64VariableDefinition;
                                }
                                else if (valueTypeReference.Equals(float32TypeReference))
                                {
                                    varDefinition = float32VariableDefinition;
                                }
                                else if (valueTypeReference.Equals(float64TypeReference))
                                {
                                    varDefinition = float64VariableDefinition;
                                }
                                else
                                {
                                    varDefinition = new VariableDefinition(valueTypeReference);
                                    method.Body.Variables.Add(varDefinition);
                                }
                                InjectStrElement(varDefinition, secondInt32VariableDefinition, valueTypeReference,
                                    method, referencedWriteAccessMethod, ins);
                                //TODO:Dominik:Testfall nicht vorhanden
                            }
                            else if (ins.OpCode.Equals(OpCodes.Stelem_I) || ins.OpCode.Equals(OpCodes.Stelem_I1)
                                     || ins.OpCode.Equals(OpCodes.Stelem_I2) || ins.OpCode.Equals(OpCodes.Stelem_I4)
                                     || ins.OpCode.Equals(OpCodes.Stelem_Ref))
                            {
                                InjectStrElement(firstInt32VariableDefinition, secondInt32VariableDefinition,
                                    int32TypeReference, method, referencedWriteAccessMethod, ins);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Stelem_I8))
                            {
                                InjectStrElement(int64VariableDefinition, firstInt32VariableDefinition,
                                    int64TypeReference, method, referencedWriteAccessMethod, ins);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Stelem_R4))
                            {
                                InjectStrElement(float32VariableDefinition, firstInt32VariableDefinition,
                                    float32TypeReference, method, referencedWriteAccessMethod, ins);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Stelem_R8))
                            {
                                InjectStrElement(float64VariableDefinition, firstInt32VariableDefinition,
                                    float64TypeReference, method, referencedWriteAccessMethod, ins);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Call))
                            {
                                MethodReference methodReference = (MethodReference) ins.Operand;

                                string monitorEnterFullName =
                                    "System.Void System.Threading.Monitor::Enter(System.Object,System.Boolean&)";
                                string monitorExitFullName =
                                    "System.Void System.Threading.Monitor::Exit(System.Object)";

                                if (monitorEnterFullName.Equals(methodReference.FullName))
                                {
                                    // TODO:Fabian Bessere Lösung für Vergleich finden.
                                    var processor = method.Body.GetILProcessor();
                                    var dupInstruction = processor.Create(OpCodes.Dup);
                                    var storeTempInstruction = processor.Create(OpCodes.Stloc,
                                        firstInt32VariableDefinition);
                                    var loadTempInstruction = processor.Create(OpCodes.Ldloc,
                                        firstInt32VariableDefinition);
                                    var lockObjectLibraryCall = processor.Create(OpCodes.Call,
                                        referencedLockObjectMethod);

                                    processor.InsertBefore(ins, loadTempInstruction);
                                    processor.InsertBefore(loadTempInstruction, lockObjectLibraryCall);
                                    processor.InsertBefore(lockObjectLibraryCall, dupInstruction);
                                    processor.InsertBefore(dupInstruction, storeTempInstruction);
                                }
                                else if (monitorExitFullName.Equals(methodReference.FullName))
                                {
                                    var processor = method.Body.GetILProcessor();
                                    var dupInstruction = processor.Create(OpCodes.Dup);
                                    var unlockObjectLibraryCall = processor.Create(OpCodes.Call,
                                        referencedUnlockObjectMethod);

                                    processor.InsertBefore(ins, unlockObjectLibraryCall);
                                    processor.InsertBefore(unlockObjectLibraryCall, dupInstruction);
                                }
                            }
                            else if (ins.OpCode.Equals(OpCodes.Callvirt))
                            {
                                MethodReference methodReference = (MethodReference) ins.Operand;
                                TypeReference typeReference = methodReference.DeclaringType;
                                TypeReference listTypeReference = module.Import(typeof(System.Collections.Generic.List<>));
                                if (typeReference.GetElementType().FullName.Equals(listTypeReference.FullName) && methodReference.Name.Equals("get_Item"))
                                {
                                    GenericInstanceType instanceType = (GenericInstanceType) methodReference.DeclaringType;
                                    TypeReference listtTypeReference = instanceType.GenericArguments[0];
                                    
                                    var genericCall = new GenericInstanceMethod(referencedGenericListMethod);
                                    genericCall.GenericArguments.Add(listtTypeReference);

                                    var processor = method.Body.GetILProcessor();
                                    var dupInstruction = processor.Create(OpCodes.Dup);
                                    var storeTempInstruction = processor.Create(OpCodes.Stloc,
                                        firstInt32VariableDefinition);
                                    var loadTempInstruction = processor.Create(OpCodes.Ldloc,
                                        firstInt32VariableDefinition);
                                    var loadTempInstruction2 = processor.Create(OpCodes.Ldloc,
                                        firstInt32VariableDefinition);
                                    var loadelema = processor.Create(OpCodes.Ldelema, listtTypeReference);
                                    var readAccessLibraryCall = processor.Create(OpCodes.Call,
                                        referencedReadAccessMethod);
                                    var genericListLibraryCall = processor.Create(OpCodes.Call,
                                        genericCall);

                                    processor.InsertBefore(ins, loadTempInstruction2);
                                    processor.InsertBefore(loadTempInstruction2, readAccessLibraryCall);
                                    processor.InsertBefore(readAccessLibraryCall, loadelema);
                                    processor.InsertBefore(loadelema, loadTempInstruction);
                                    processor.InsertBefore(loadTempInstruction, genericListLibraryCall);
                                    processor.InsertBefore(genericListLibraryCall, dupInstruction);
                                    processor.InsertBefore(dupInstruction, storeTempInstruction);
                                }
                                else if (typeReference.GetElementType().FullName.Equals(listTypeReference.FullName) && methodReference.Name.Equals("set_Item"))
                                {
                                    GenericInstanceType instanceType = (GenericInstanceType)methodReference.DeclaringType;
                                    TypeReference listtTypeReference = instanceType.GenericArguments[0];
                                    VariableDefinition varDefinition;

                                    if (listtTypeReference.Equals(int8TypeReference))
                                    {
                                        varDefinition = int8VariableDefinition;
                                    }
                                    else if (listtTypeReference.Equals(int16TypeReference))
                                    {
                                        varDefinition = int16VariableDefinition;
                                    }
                                    else if (listtTypeReference.Equals(int32TypeReference))
                                    {
                                        varDefinition = firstInt32VariableDefinition;
                                    }
                                    else if (listtTypeReference.Equals(int64TypeReference))
                                    {
                                        varDefinition = int64VariableDefinition;
                                    }
                                    else if (listtTypeReference.Equals(float32TypeReference))
                                    {
                                        varDefinition = float32VariableDefinition;
                                    }
                                    else if (listtTypeReference.Equals(float64TypeReference))
                                    {
                                        varDefinition = float64VariableDefinition;
                                    }
                                    else
                                    {
                                        varDefinition = new VariableDefinition(listtTypeReference);
                                        method.Body.Variables.Add(varDefinition);
                                    }

                                    //TODO:Dominik: Equals funktioniert nicht

                                    var genericCall = new GenericInstanceMethod(referencedGenericListMethod);
                                    genericCall.GenericArguments.Add(listtTypeReference);

                                    var processor = method.Body.GetILProcessor();
                                    var dupInstruction = processor.Create(OpCodes.Dup);
                                    var storeTempInstruction = processor.Create(OpCodes.Stloc,
                                        firstInt32VariableDefinition);
                                    var storeValueInstruction = processor.Create(OpCodes.Stloc, 
                                        varDefinition);
                                    var loadValueInstruction = processor.Create(OpCodes.Ldloc,
                                        varDefinition);
                                    var loadTempInstruction = processor.Create(OpCodes.Ldloc,
                                        firstInt32VariableDefinition);
                                    var loadTempInstruction2 = processor.Create(OpCodes.Ldloc,
                                        firstInt32VariableDefinition);
                                    var loadelema = processor.Create(OpCodes.Ldelema, listtTypeReference);
                                    var writeAccessLibraryCall = processor.Create(OpCodes.Call,
                                        referencedWriteAccessMethod);
                                    var genericListLibraryCall = processor.Create(OpCodes.Call,
                                        genericCall);

                                    processor.InsertBefore(ins, loadValueInstruction);
                                    processor.InsertBefore(loadValueInstruction, loadTempInstruction2);
                                    processor.InsertBefore(loadTempInstruction2, writeAccessLibraryCall);
                                    processor.InsertBefore(writeAccessLibraryCall, loadelema);
                                    processor.InsertBefore(loadelema, loadTempInstruction);
                                    processor.InsertBefore(loadTempInstruction, genericListLibraryCall);
                                    processor.InsertBefore(genericListLibraryCall, dupInstruction);
                                    processor.InsertBefore(dupInstruction, storeTempInstruction);
                                    processor.InsertBefore(storeTempInstruction, storeValueInstruction);
                                }
                            }
                        }
                        method.Body.OptimizeMacros(); // Convert the normal branches back to short branches if possible
                    }
                }
            }

            module.Write(fileName);
        }

        private static void InjectStrElement(VariableDefinition variableValueDefinition,
            VariableDefinition variableIndexDefinition, TypeReference valueTypeReference, MethodDefinition method,
            MethodReference referencedWriteAccessMethod, Instruction ins)
        {
            var processor = method.Body.GetILProcessor();
            var dupInstruction = processor.Create(OpCodes.Dup);
            var storeValueInstruction = processor.Create(OpCodes.Stloc, variableValueDefinition);
            var storeIndexInstruction = processor.Create(OpCodes.Stloc, variableIndexDefinition);
            var loadIndexInstruction = processor.Create(OpCodes.Ldloc, variableIndexDefinition);
            var loadIndexInstruction2 = processor.Create(OpCodes.Ldloc, variableIndexDefinition);
            var loadValueInstruction = processor.Create(OpCodes.Ldloc, variableValueDefinition);
            var loadAddressInstruction = processor.Create(OpCodes.Ldelema, valueTypeReference);
            var writeAccessLibraryCall = processor.Create(OpCodes.Call, referencedWriteAccessMethod);

            processor.InsertBefore(ins, loadValueInstruction);
            processor.InsertBefore(loadValueInstruction, loadIndexInstruction);
            processor.InsertBefore(loadIndexInstruction, writeAccessLibraryCall);
            processor.InsertBefore(writeAccessLibraryCall, loadAddressInstruction);
            processor.InsertBefore(loadAddressInstruction, loadIndexInstruction2);
            processor.InsertBefore(loadIndexInstruction2, dupInstruction);
            processor.InsertBefore(dupInstruction, storeIndexInstruction);
            processor.InsertBefore(storeIndexInstruction, storeValueInstruction);
        }

        private static void InjectArrayLdElement(VariableDefinition int32Variable, TypeReference arrayTypeReference,
            MethodDefinition method,
            MethodReference referencedReadAccessMethod, Instruction ins)
        {
            var processor = method.Body.GetILProcessor();
            var storeIndexInstrution = processor.Create(OpCodes.Stloc, int32Variable);
            var dupInstruction = processor.Create(OpCodes.Dup);
            var loadIndexInstrucion = processor.Create(OpCodes.Ldloc, int32Variable);
            var loadIndexInstrucion2 = processor.Create(OpCodes.Ldloc, int32Variable);
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