﻿using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using Mono.Cecil.Rocks;

namespace CodeInstrumentation
{
    public class CodeInstrumentator
    {
        private static Dictionary<string, MethodReference> _methodReferences; 
        private static Dictionary<string, TypeReference> _typeReferences;

        public static void InjectCodeInstrumentation(string fileName)
        {
            _methodReferences = new Dictionary<string, MethodReference>();
            _typeReferences = new Dictionary<string, TypeReference>();
            ModuleDefinition refModul = ModuleDefinition.ReadModule(@"work\DPCLibrary.dll");
            TypeDefinition typeDefinition = refModul.Types.First(x => x.Name == "DpcLibrary");
            ModuleDefinition module = ModuleDefinition.ReadModule(fileName);
            ImportAllPublicMethods(typeDefinition, module);

            ImportAllTypes(module);
            
            foreach (TypeDefinition type in module.Types)
            {
                InstrumentateType(type);
            }

            module.Write(fileName);
        }

        private static void InstrumentateType(TypeDefinition type)
        {
            if (type.HasNestedTypes)
            {
                foreach (TypeDefinition nestedType in type.NestedTypes)
                {
                    InstrumentateType(nestedType);
                }
            }
            if (type.HasMethods)
            {
                foreach (MethodDefinition method in type.Methods.Where(method => method.Body != null))
                {
                    method.Body.SimplifyMacros(); // convert every br.s (short branch) to a normal branch
                    Dictionary<string, VariableDefinition> variableDefinitions = AddAllVariablesToMethod(method);

                    ArrayList tempList = new ArrayList(method.Body.Instructions.ToList());
                    foreach (Instruction ins in tempList)
                    {
                        var processor = method.Body.GetILProcessor();
                        if (ins.OpCode.Equals(OpCodes.Ldsfld))
                        {
                            FieldReference fieldDefinition = (FieldReference)ins.Operand;

                            TypeReference fieldType = fieldDefinition.FieldType;
                            if (fieldType.IsPrimitive ||
                                (fieldType.IsDefinition && ((TypeDefinition)fieldType).IsValueType))
                            {
                                var methodLoad = processor.Create(OpCodes.Ldstr, method.FullName);
                                var constLoad = processor.Create(OpCodes.Ldc_I4, ins.Offset);
                                var loadAddressInstruction = processor.Create(OpCodes.Ldsflda, fieldDefinition);
                                var readAccessLibraryCall = processor.Create(OpCodes.Call, _methodReferences["ReadAccess"]);
                                processor.InsertAfter(ins, loadAddressInstruction);
                                processor.InsertAfter(loadAddressInstruction, constLoad);
                                processor.InsertAfter(constLoad, methodLoad);
                                processor.InsertAfter(methodLoad, readAccessLibraryCall);
                            }
                            else if (!fieldType.IsPrimitive && !fieldType.IsValueType)
                            {
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var readAccessLibraryCall = processor.Create(OpCodes.Call, _methodReferences["ReadAccess"]);
                                var readAccessLibraryCall2 = processor.Create(OpCodes.Call, _methodReferences["ReadAccess"]);
                                var loadAddressInstruction = processor.Create(OpCodes.Ldsflda, fieldDefinition);
                                var constLoad = processor.Create(OpCodes.Ldc_I4, ins.Offset);
                                var constLoad2 = processor.Create(OpCodes.Ldc_I4, ins.Offset);
                                var methodLoad = processor.Create(OpCodes.Ldstr, method.FullName);
                                var methodLoad2 = processor.Create(OpCodes.Ldstr, method.FullName);
                                processor.InsertAfter(ins, dupInstruction);
                                processor.InsertAfter(dupInstruction, constLoad);
                                processor.InsertAfter(constLoad, methodLoad);
                                processor.InsertAfter(methodLoad, readAccessLibraryCall);
                                processor.InsertAfter(readAccessLibraryCall, loadAddressInstruction);
                                processor.InsertAfter(loadAddressInstruction, constLoad2);
                                processor.InsertAfter(constLoad2, methodLoad2);
                                processor.InsertAfter(methodLoad2, readAccessLibraryCall2);
                            }
                        }
                        else if (ins.OpCode.Equals(OpCodes.Initobj))
                        {
                            TypeReference fieldType = (TypeReference)ins.Operand;
                            if (fieldType.IsDefinition && ((TypeDefinition)fieldType).IsValueType)
                            {
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var constLoad = processor.Create(OpCodes.Ldc_I4, ins.Offset);
                                var methodLoad = processor.Create(OpCodes.Ldstr, method.FullName);
                                var writeAccessLibraryCall = processor.Create(OpCodes.Call, _methodReferences["WriteAccess"]);
                                processor.InsertBefore(ins, writeAccessLibraryCall);
                                processor.InsertBefore(writeAccessLibraryCall, methodLoad);
                                processor.InsertBefore(methodLoad, constLoad);
                                processor.InsertBefore(constLoad, dupInstruction);
                            }
                        }
                        else if (ins.OpCode.Equals(OpCodes.Stsfld))
                        {
                            FieldReference fieldDefinition = (FieldReference)ins.Operand;
                            var loadAddressInstruction = processor.Create(OpCodes.Ldsflda, fieldDefinition);
                            var constLoad = processor.Create(OpCodes.Ldc_I4, ins.Offset);
                            var methodLoad = processor.Create(OpCodes.Ldstr, method.FullName);
                            var writeAccessLibraryCall = processor.Create(OpCodes.Call,
                                _methodReferences["WriteAccess"]);
                            processor.InsertBefore(ins, writeAccessLibraryCall);
                            processor.InsertBefore(writeAccessLibraryCall, methodLoad);
                            processor.InsertBefore(methodLoad, constLoad);
                            processor.InsertBefore(constLoad, loadAddressInstruction);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Ldfld))
                        {
                            FieldDefinition fieldDefinition = (FieldDefinition)ins.Operand;
                            TypeReference fieldType = fieldDefinition.FieldType;
                            if (fieldType.IsPrimitive ||
                                (fieldType.IsDefinition && ((TypeDefinition)fieldType).IsValueType))
                            {
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var loadAddressInstruction = processor.Create(OpCodes.Ldflda, fieldDefinition);
                                var constLoad = processor.Create(OpCodes.Ldc_I4, ins.Offset);
                                var methodLoad = processor.Create(OpCodes.Ldstr, method.FullName);
                                var readAccessLibraryCall = processor.Create(OpCodes.Call,
                                    _methodReferences["ReadAccess"]);
                                processor.InsertBefore(ins, readAccessLibraryCall);
                                processor.InsertBefore(readAccessLibraryCall, methodLoad);
                                processor.InsertBefore(methodLoad, constLoad);
                                processor.InsertBefore(constLoad, loadAddressInstruction);
                                processor.InsertBefore(loadAddressInstruction, dupInstruction);
                            }
                            else if (!fieldType.IsPrimitive && !fieldType.IsValueType)
                            {
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var constLoad = processor.Create(OpCodes.Ldc_I4, ins.Offset);
                                var methodLoad = processor.Create(OpCodes.Ldstr, method.FullName);
                                var readAccessLibraryCall = processor.Create(OpCodes.Call,
                                    _methodReferences["ReadAccess"]);
                                processor.InsertBefore(ins, readAccessLibraryCall);
                                processor.InsertBefore(readAccessLibraryCall, methodLoad);
                                processor.InsertBefore(methodLoad, constLoad);
                                processor.InsertBefore(constLoad, dupInstruction);
                            }
                        }
                        else if (ins.OpCode.Equals(OpCodes.Stfld))
                        {
                            FieldDefinition fieldDefinition = (FieldDefinition)ins.Operand;
                            VariableDefinition varDefinition;

                            if (fieldDefinition.FieldType.Equals(_typeReferences["int8"]))
                            {
                                varDefinition = variableDefinitions["int8"];
                            }
                            else if (fieldDefinition.FieldType.Equals(_typeReferences["int16"]))
                            {
                                varDefinition = variableDefinitions["int16"];
                            }
                            else if (fieldDefinition.FieldType.Equals(_typeReferences["int32"]))
                            {
                                varDefinition = variableDefinitions["firstint32"];
                            }
                            else if (fieldDefinition.FieldType.Equals(_typeReferences["int64"]))
                            {
                                varDefinition = variableDefinitions["int64"];
                            }
                            else if (fieldDefinition.FieldType.Equals(_typeReferences["float32"]))
                            {
                                varDefinition = variableDefinitions["float32"];
                            }
                            else if (fieldDefinition.FieldType.Equals(_typeReferences["float64"]))
                            {
                                varDefinition = variableDefinitions["float64"];
                            }
                            else
                            {
                                varDefinition = new VariableDefinition(fieldDefinition.FieldType);
                                method.Body.Variables.Add(varDefinition);
                            }
                            var storeLocalInstrution = processor.Create(OpCodes.Stloc, varDefinition);
                            var dupInstruction = processor.Create(OpCodes.Dup);
                            var loadAddressInstruction = processor.Create(OpCodes.Ldflda, fieldDefinition);
                            var writeAccessLibraryCall = processor.Create(OpCodes.Call,
                                _methodReferences["WriteAccess"]);
                            var constLoad = processor.Create(OpCodes.Ldc_I4, ins.Offset);
                            var methodLoad = processor.Create(OpCodes.Ldstr, method.FullName);
                            var loadLocaleInstrucion = processor.Create(OpCodes.Ldloc, varDefinition);


                            processor.InsertBefore(ins, loadLocaleInstrucion);
                            processor.InsertBefore(loadLocaleInstrucion, writeAccessLibraryCall);
                            processor.InsertBefore(writeAccessLibraryCall, methodLoad);
                            processor.InsertBefore(methodLoad, constLoad);
                            processor.InsertBefore(constLoad, loadAddressInstruction);
                            processor.InsertBefore(loadAddressInstruction, dupInstruction);
                            processor.InsertBefore(dupInstruction, storeLocalInstrution);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Ldelem_Any))
                        {
                            TypeReference arrayTypeReference = (TypeReference)ins.Operand;
                            InjectArrayLdElement(variableDefinitions["firstint32"], arrayTypeReference, method,
                                _methodReferences["ReadAccess"], ins);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Ldelem_I) || ins.OpCode.Equals(OpCodes.Ldelem_I1)
                                 || ins.OpCode.Equals(OpCodes.Ldelem_I2) || ins.OpCode.Equals(OpCodes.Ldelem_I4) ||
                                 ins.OpCode.Equals(OpCodes.Ldelem_Ref) || ins.OpCode.Equals(OpCodes.Ldelem_U1) ||
                                 ins.OpCode.Equals(OpCodes.Ldelem_U2) ||
                                 ins.OpCode.Equals(OpCodes.Ldelem_U4))
                        {
                            InjectArrayLdElement(variableDefinitions["firstint32"], _typeReferences["int32"], method,
                                _methodReferences["ReadAccess"], ins);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Ldelem_I8))
                        {
                            InjectArrayLdElement(variableDefinitions["firstint32"], _typeReferences["int64"], method,
                                _methodReferences["ReadAccess"], ins);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Ldelem_R4))
                        {
                            InjectArrayLdElement(variableDefinitions["firstint32"], _typeReferences["float32"], method,
                                _methodReferences["ReadAccess"], ins);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Ldelem_R8))
                        {
                            InjectArrayLdElement(variableDefinitions["firstint32"], _typeReferences["float64"], method,
                                _methodReferences["ReadAccess"], ins);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Stelem_Any))
                        {
                            TypeReference valueTypeReference = (TypeReference)ins.Operand;
                            VariableDefinition varDefinition;

                            if (valueTypeReference.Equals(_typeReferences["int8"]))
                            {
                                varDefinition = variableDefinitions["int8"];
                            }
                            else if (valueTypeReference.Equals(_typeReferences["int16"]))
                            {
                                varDefinition = variableDefinitions["int16"];
                            }
                            else if (valueTypeReference.Equals(_typeReferences["int32"]))
                            {
                                varDefinition = variableDefinitions["firstint32"];
                            }
                            else if (valueTypeReference.Equals(_typeReferences["int64"]))
                            {
                                varDefinition = variableDefinitions["int64"];
                            }
                            else if (valueTypeReference.Equals(_typeReferences["float32"]))
                            {
                                varDefinition = variableDefinitions["float32"];
                            }
                            else if (valueTypeReference.Equals(_typeReferences["float64"]))
                            {
                                varDefinition = variableDefinitions["float64"];
                            }
                            else
                            {
                                varDefinition = new VariableDefinition(valueTypeReference);
                                method.Body.Variables.Add(varDefinition);
                            }
                            InjectStrElement(varDefinition, variableDefinitions["secondint32"], valueTypeReference,
                                method, _methodReferences["WriteAccess"], ins);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Stelem_I) || ins.OpCode.Equals(OpCodes.Stelem_I1)
                                 || ins.OpCode.Equals(OpCodes.Stelem_I2) || ins.OpCode.Equals(OpCodes.Stelem_I4)
                            )
                        {
                            InjectStrElement(variableDefinitions["firstint32"], variableDefinitions["secondint32"],
                                _typeReferences["int32"], method, _methodReferences["WriteAccess"], ins);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Stelem_Ref))
                        {
                            InjectStrElement(variableDefinitions["object"], variableDefinitions["secondint32"],
                                _typeReferences["int32"], method, _methodReferences["WriteAccess"], ins);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Stelem_I8))
                        {
                            InjectStrElement(variableDefinitions["int64"], variableDefinitions["firstint32"],
                                _typeReferences["int64"], method, _methodReferences["WriteAccess"], ins);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Stelem_R4))
                        {
                            InjectStrElement(variableDefinitions["float32"], variableDefinitions["firstint32"],
                                _typeReferences["float32"], method, _methodReferences["WriteAccess"], ins);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Stelem_R8))
                        {
                            InjectStrElement(variableDefinitions["float64"], variableDefinitions["firstint32"],
                                _typeReferences["float64"], method, _methodReferences["WriteAccess"], ins);
                        }
                        if (ins.OpCode.Equals(OpCodes.Call))
                        {
                            MethodReference reference = (MethodReference)ins.Operand;
                            
                            string monitorEnterFullName =
                                "System.Void System.Threading.Monitor::Enter(System.Object,System.Boolean&)";
                            string monitorExitFullName =
                                "System.Void System.Threading.Monitor::Exit(System.Object)";

                            if (monitorEnterFullName.Equals(reference.FullName))
                            {
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var storeTempInstruction = processor.Create(OpCodes.Stloc,
                                    variableDefinitions["firstint32"]);
                                var loadTempInstruction = processor.Create(OpCodes.Ldloc,
                                    variableDefinitions["firstint32"]);
                                var lockObjectLibraryCall = processor.Create(OpCodes.Call,
                                    _methodReferences["LockObject"]);

                                processor.InsertBefore(ins, loadTempInstruction);
                                processor.InsertBefore(loadTempInstruction, lockObjectLibraryCall);
                                processor.InsertBefore(lockObjectLibraryCall, dupInstruction);
                                processor.InsertBefore(dupInstruction, storeTempInstruction);
                            }
                            else if (monitorExitFullName.Equals(reference.FullName))
                            {
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var unlockObjectLibraryCall = processor.Create(OpCodes.Call,
                                    _methodReferences["UnLockObject"]);

                                processor.InsertBefore(ins, unlockObjectLibraryCall);
                                processor.InsertBefore(unlockObjectLibraryCall, dupInstruction);
                            }
                            else if (reference.FullName.Contains("System.Threading.Tasks.Task::Run"))
                            {
                                if (reference.Parameters[0].ParameterType.FullName.Equals("System.Action"))
                                {
                                    if (reference.Parameters.Count == 1)
                                    {
                                        ins.Operand = _methodReferences["RunTask"];
                                    }
                                    else if (reference.Parameters.Count == 2)
                                    {
                                        ins.Operand = _methodReferences["RunTaskCancel"];
                                    }
                                }
                                else if (reference.Parameters[0].ParameterType.FullName.Equals("System.Func`1<!!0>"))
                                {
                                    MethodReference methodReference = (MethodReference) ins.Operand;
                                    GenericInstanceMethod genericMethod = (GenericInstanceMethod) methodReference;
                                    TypeReference genericArgument = genericMethod.GenericArguments[0];
                                    if (reference.Parameters.Count == 1)
                                    {
                                        var newGenericMethod = new GenericInstanceMethod(_methodReferences["RunTaskTResult"]);
                                        newGenericMethod.GenericArguments.Add(genericArgument);
                                        ins.Operand = newGenericMethod;
                                    }
                                    else if (reference.Parameters.Count == 2)
                                    {
                                        var newGenericMethod = new GenericInstanceMethod(_methodReferences["RunTaskTResultCancel"]);
                                        newGenericMethod.GenericArguments.Add(genericArgument);
                                        ins.Operand = newGenericMethod;
                                    }
                                }
                                else if (reference.Parameters[0].ParameterType.FullName.Equals("System.Func`1<System.Threading.Tasks.Task>"))
                                {
                                    if (reference.Parameters.Count == 1)
                                    {
                                        ins.Operand = _methodReferences["RunTaskFunc"];
                                    }
                                    else if (reference.Parameters.Count == 2)
                                    {
                                        ins.Operand = _methodReferences["RunTaskFuncCancel"];
                                    }
                                }
                                else if (reference.Parameters[0].ParameterType.FullName.Equals("System.Func`1<System.Threading.Tasks.Task`1<!!0>>"))
                                {
                                    MethodReference methodReference = (MethodReference)ins.Operand;
                                    GenericInstanceMethod genericMethod = (GenericInstanceMethod)methodReference;
                                    TypeReference genericArgument = genericMethod.GenericArguments[0];
                                    if (reference.Parameters.Count == 1)
                                    {
                                        var newGenericMethod = new GenericInstanceMethod(_methodReferences["RunTaskTaskTResult"]);
                                        newGenericMethod.GenericArguments.Add(genericArgument);
                                        ins.Operand = newGenericMethod;
                                    }
                                    else if (reference.Parameters.Count == 2)
                                    {
                                        var newGenericMethod = new GenericInstanceMethod(_methodReferences["RunTaskTaskTResultCancel"]);
                                        newGenericMethod.GenericArguments.Add(genericArgument);
                                        ins.Operand = newGenericMethod;
                                    }
                                }
                            }
                        }
                        else if (ins.OpCode.Equals(OpCodes.Callvirt))
                        {
                            MethodReference reference = (MethodReference)ins.Operand;
                            if (reference.FullName.Contains("System.Void System.Threading.Thread::Start"))
                            {
                                if (!reference.HasParameters)
                                {
                                    var ldNull = processor.Create(OpCodes.Ldnull);
                                    processor.InsertBefore(ins, ldNull);
                                }
                                ins.OpCode = OpCodes.Call;
                                ins.Operand = _methodReferences["StartThread"];
                            }
                            else if (reference.FullName.Contains("System.Void System.Threading.Thread::Join"))
                            {
                                ins.OpCode = OpCodes.Call;
                                ins.Operand = _methodReferences["JoinThread"];
                            }
                            else if (reference.FullName.Contains("System.Boolean System.Threading.Thread::Join"))
                            {
                                ins.OpCode = OpCodes.Call;
                                ins.Operand = reference.Parameters[0].ParameterType.IsPrimitive 
                                    ? _methodReferences["JoinThreadMilliseconds"] 
                                    : _methodReferences["JoinThreadTimeout"];
                            }
                            else if (reference.FullName.Contains("System.Void System.Threading.Tasks.Task::Start"))
                            {
                                if (!reference.HasParameters)
                                {
                                    var ldNull = processor.Create(OpCodes.Ldnull);
                                    processor.InsertBefore(ins, ldNull);
                                }
                                ins.OpCode = OpCodes.Call;
                                ins.Operand = _methodReferences["StartTask"];
                            }
                            else if (reference.FullName.Contains("System.Void System.Threading.Tasks.Task::Wait"))
                            {
                                ins.OpCode = OpCodes.Call;
                                ins.Operand = !reference.HasParameters
                                    ? _methodReferences["TaskWait"]
                                    : _methodReferences["TaskWaitCancelToken"];
                            }
                            else if (reference.FullName.Contains("System.Boolean System.Threading.Tasks.Task::Wait"))
                            {
                                if (reference.Parameters.Count == 1 && reference.Parameters[0].ParameterType.IsPrimitive)
                                {
                                    ins.OpCode = OpCodes.Call;
                                    ins.Operand = _methodReferences["TaskWaitTimeout"];
                                }
                                else if (reference.Parameters.Count == 2)
                                {
                                    ins.OpCode = OpCodes.Call;
                                    ins.Operand = _methodReferences["TaskWaitTimeOutCancelToken"];
                                }
                                else
                                {
                                    ins.OpCode = OpCodes.Call;
                                    ins.Operand = _methodReferences["TaskWaitTimespan"];
                                }
                            }
                        }
                    }
                    method.Body.OptimizeMacros(); // Convert the normal branches back to short branches if possible
                }
            }
        }

        private static void ImportAllPublicMethods(TypeDefinition typeDefinition, ModuleDefinition module)
        {
            typeDefinition.Methods.ToList().ForEach(x => { if (x.IsPublic) { _methodReferences.Add(x.Name, module.Import(x));} });
        }

        private static void ImportAllTypes(ModuleDefinition module)
        {
            _typeReferences.Add("int8", module.Import(typeof(byte)));
            _typeReferences.Add("int16", module.Import(typeof(short)));
            _typeReferences.Add("int32", module.Import(typeof(int)));
            _typeReferences.Add("int64", module.Import(typeof(long)));
            _typeReferences.Add("float32", module.Import(typeof(float)));
            _typeReferences.Add("float64", module.Import(typeof(double)));
            _typeReferences.Add("object", new ByReferenceType(module.Import(typeof(object))));
        }

        private static Dictionary<string, VariableDefinition> AddAllVariablesToMethod(MethodDefinition method)
        {
            Dictionary<string, VariableDefinition> result = new Dictionary<string, VariableDefinition>();
            // ReSharper disable once JoinDeclarationAndInitializer
            VariableDefinition tempVariableDefinition;
            _typeReferences.ToList().ForEach(x => { if (!x.Key.Equals("int32"))
                {
                    tempVariableDefinition = new VariableDefinition(x.Value);
                    method.Body.Variables.Add(tempVariableDefinition);
                    result.Add(x.Key, tempVariableDefinition);
                }
                else
                {
                    tempVariableDefinition = new VariableDefinition(x.Value);
                    method.Body.Variables.Add(tempVariableDefinition);
                    result.Add("firstint32", tempVariableDefinition);
                }
            });

            tempVariableDefinition = new VariableDefinition(_typeReferences["int32"]);
            method.Body.Variables.Add(tempVariableDefinition);
            result.Add("secondint32", tempVariableDefinition);
            method.Body.InitLocals = true;
            return result;
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
            var constLoad = processor.Create(OpCodes.Ldc_I4, ins.Offset);
            var methodLoad = processor.Create(OpCodes.Ldstr, method.FullName);
            var writeAccessLibraryCall = processor.Create(OpCodes.Call, referencedWriteAccessMethod);

            processor.InsertBefore(ins, loadValueInstruction);
            processor.InsertBefore(loadValueInstruction, loadIndexInstruction);
            processor.InsertBefore(loadIndexInstruction, writeAccessLibraryCall);
            processor.InsertBefore(writeAccessLibraryCall, methodLoad);
            processor.InsertBefore(methodLoad, constLoad);
            processor.InsertBefore(constLoad,loadAddressInstruction);
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
            var constLoad = processor.Create(OpCodes.Ldc_I4, ins.Offset);
            var methodLoad = processor.Create(OpCodes.Ldstr, method.FullName);
            var readAccessLibraryCall = processor.Create(OpCodes.Call, referencedReadAccessMethod);

            processor.InsertBefore(ins, loadIndexInstrucion2);
            processor.InsertBefore(loadIndexInstrucion2, readAccessLibraryCall);
            processor.InsertBefore(readAccessLibraryCall, methodLoad);
            processor.InsertBefore(methodLoad, constLoad);
            processor.InsertBefore(constLoad, loadAddressInstruction);
            processor.InsertBefore(loadAddressInstruction, loadIndexInstrucion);
            processor.InsertBefore(loadIndexInstrucion, dupInstruction);
            processor.InsertBefore(dupInstruction, storeIndexInstrution);
        }

        public static string DecompileCode(string fileName, string typeName, string methodName, int offset)
        {
            FileInfo assemblyFile = new FileInfo(fileName);
            string pathToAssembly = assemblyFile.FullName;
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(assemblyFile.DirectoryName);

            var parameters = new ReaderParameters
            {
                AssemblyResolver =  resolver
            };

            AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(pathToAssembly, parameters);
            string result = string.Empty;

            MethodDefinition method = null;
            TypeDefinition typeDefinition = null;
            foreach (TypeDefinition type in assemblyDefinition.MainModule.Types)
            {
                method = GetMethodeDefinition(typeName, methodName, type, out typeDefinition);
                if (method != null)
                {
                    break;
                }
            }

            if (method != null)
            {
                Instruction ins = method.Body.Instructions.SingleOrDefault(x => x.Offset == offset);

                AddDetectedInstructionBefore(assemblyDefinition.MainModule, method, ins);
                AstBuilder astBuilder = new AstBuilder(new DecompilerContext(assemblyDefinition.MainModule)
                {
                    CurrentType = typeDefinition,
                    CurrentMethod = method
                });
                
                astBuilder.AddMethod(method);
                
                StringWriter output = new StringWriter();
                astBuilder.GenerateCode(new PlainTextOutput(output));
                result = output.ToString();
                output.Dispose();
            }
            return result;
        }

        private static void AddDetectedInstructionBefore(ModuleDefinition module, MethodDefinition method, Instruction ins)
        {
            ModuleDefinition refModul = ModuleDefinition.ReadModule("DPCLibrary.dll");
            TypeDefinition typeDefinition = refModul.Types.First(x => x.Name == "DpcLibrary");
            MethodDefinition raceConditionDetectedDef =
                typeDefinition.Methods.Single(x => x.Name == "RaceConditionDetectedIdentifier");
            MethodReference raceConditionDetectedRef = module.Import(raceConditionDetectedDef);

            var processor = method.Body.GetILProcessor();
            var raceDetectedLibraryCall = processor.Create(OpCodes.Call,
                                    raceConditionDetectedRef);
            processor.InsertBefore(ins, raceDetectedLibraryCall);
        }

        private static MethodDefinition GetMethodeDefinition(string typeName, string methodName, TypeDefinition type, out TypeDefinition typeDefinition)
        {
            if (type.FullName.Equals(typeName))
            {
                typeDefinition = type;
                MethodDefinition method = type.Methods.SingleOrDefault(x => x.FullName.Equals(methodName));
                if (method != null)
                {
                    return method;
                }
            }

            if (type.HasNestedTypes)
            {
                foreach (TypeDefinition nestedType in type.NestedTypes)
                {
                    MethodDefinition method = GetMethodeDefinition(typeName, methodName, nestedType, out typeDefinition);
                    if (method != null)
                    {
                        return method;
                    }
                }
            }
            typeDefinition = null;
            return null;
        }
    }
}