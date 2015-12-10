using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        private static readonly string dpcLibraryPath = @"work\DPCLibrary.dll";
        private static readonly string dpcLibraryName = "DpcLibrary";

        public static void InjectCodeInstrumentation(string fileName)
        {
            _methodReferences = new Dictionary<string, MethodReference>();
            _typeReferences = new Dictionary<string, TypeReference>();
            var refModul = ModuleDefinition.ReadModule(dpcLibraryPath);
            var typeDefinition = refModul.Types.First(x => x.Name == dpcLibraryName);
            var module = ModuleDefinition.ReadModule(fileName);
            ImportAllPublicMethods(typeDefinition, module);

            ImportAllTypes(module);
            
            foreach (var type in module.Types)
            {
                InstrumentateType(type, module);
            }

            module.Write(fileName);
        }

        private static void InstrumentateType(TypeDefinition type, ModuleDefinition module)
        {
            if (type.HasNestedTypes)
            {
                foreach (var nestedType in type.NestedTypes)
                {
                    InstrumentateType(nestedType, module);
                }
            }
            if (type.HasMethods)
            {
                foreach (var method in type.Methods.Where(method => method.Body != null))
                {
                    method.Body.SimplifyMacros(); // convert every br.s (short branch) to a normal branch
                    var variableDefinitions = AddAllVariablesToMethod(method);

                    var tempList = new ArrayList(method.Body.Instructions.ToList());
                    foreach (Instruction ins in tempList)
                    {
                        var processor = method.Body.GetILProcessor();
                        if (ins.OpCode.Equals(OpCodes.Ldsfld))
                        {
                            InstrumentateLdsfld(ins, processor, method);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Initobj))
                        {
                            InstrumentateInitobj(ins, processor, method);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Stsfld))
                        {
                            InstrumentateStsfld(ins, processor, method);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Ldfld))
                        {
                            InstrumentateLdfld(ins, processor, method);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Stfld))
                        {
                            InstrumentateStfld(ins, variableDefinitions, method, processor);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Ldelem_Any))
                        {
                            var arrayTypeReference = (TypeReference)ins.Operand;
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
                            VariableDefinition varDefinition;
                            var valueTypeReference = GetValueTypeReference(ins, variableDefinitions, method, out varDefinition);
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
                            InstrumentateCall(ins, processor, variableDefinitions);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Callvirt))
                        {
                            InstrumentateCallvirt(module, ins, processor, method, variableDefinitions);
                        }
                    }
                    method.Body.OptimizeMacros(); // Convert the normal branches back to short branches if possible
                }
            }
        }

        private static void InstrumentateCallvirt(ModuleDefinition module, Instruction ins, ILProcessor processor,
            MethodDefinition method, Dictionary<string, VariableDefinition> variableDefinitions)
        {
            var reference = (MethodReference) ins.Operand;
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
            else if (reference.FullName.Contains("System.Threading.Tasks.Task System.Threading.Tasks.TaskFactory::StartNew"))
            {
                if (reference.Parameters.Count > 0 && reference.Parameters[0].ParameterType.FullName.Equals("System.Action"))
                {
                    if (reference.Parameters.Count >= 2 &&
                        reference.Parameters[1].ParameterType.FullName.Equals(
                            "System.Threading.CancellationToken"))
                    {
                        ins.OpCode = OpCodes.Call;
                        ins.Operand = _methodReferences["StartNewCancel"];
                        if (reference.Parameters.Count == 2)
                        {
                            var ldZero = processor.Create(OpCodes.Ldc_I4_0);
                            var ldNull = processor.Create(OpCodes.Ldnull);
                            processor.InsertBefore(ins, ldNull);
                            processor.InsertBefore(ldNull, ldZero);
                        }
                    }
                    else
                    {
                        ins.OpCode = OpCodes.Call;
                        ins.Operand = _methodReferences["StartNew"];
                        if (reference.Parameters.Count == 1)
                        {
                            var ldZero = processor.Create(OpCodes.Ldc_I4_0);
                            processor.InsertBefore(ins, ldZero);
                        }
                    }
                }
                else if (reference.Parameters.Count > 0 &&
                         reference.Parameters[0].ParameterType.FullName.Equals("System.Action`1<System.Object>"))
                {
                    if (reference.Parameters.Count > 2 &&
                        reference.Parameters[2].ParameterType.FullName.Equals(
                            "System.Threading.CancellationToken"))
                    {
                        ins.OpCode = OpCodes.Call;
                        ins.Operand = _methodReferences["StartNewObjectCancel"];
                        if (reference.Parameters.Count == 3)
                        {
                            var ldZero = processor.Create(OpCodes.Ldc_I4_0);
                            var ldNull = processor.Create(OpCodes.Ldnull);
                            processor.InsertBefore(ins, ldNull);
                            processor.InsertBefore(ldNull, ldZero);
                        }
                    }
                    else
                    {
                        ins.OpCode = OpCodes.Call;
                        ins.Operand = _methodReferences["StartNewObject"];
                        if (reference.Parameters.Count == 2)
                        {
                            var ldZero = processor.Create(OpCodes.Ldc_I4_0);
                            processor.InsertBefore(ins, ldZero);
                        }
                    }
                }
                PopTaskFactory(module, method, ins, processor, variableDefinitions, false);
            }
            else if (
                reference.FullName.Contains(
                    "System.Threading.Tasks.Task`1<!!0> System.Threading.Tasks.TaskFactory::StartNew"))
            {
                var methodReference = (MethodReference) ins.Operand;
                var genericMethod = (GenericInstanceMethod) methodReference;
                var genericArgument = genericMethod.GenericArguments[0];
                if (reference.Parameters.Count > 0 &&
                    reference.Parameters[0].ParameterType.FullName.Equals("System.Func`1<!!0>"))
                {
                    if (reference.Parameters.Count >= 2 &&
                        reference.Parameters[1].ParameterType.FullName.Equals(
                            "System.Threading.CancellationToken"))
                    {
                        var newGenericMethod = new GenericInstanceMethod(_methodReferences["StartNewTResultCancel"]);
                        newGenericMethod.GenericArguments.Add(genericArgument);
                        ins.OpCode = OpCodes.Call;
                        ins.Operand = newGenericMethod;
                        if (reference.Parameters.Count == 2)
                        {
                            var ldZero = processor.Create(OpCodes.Ldc_I4_0);
                            var ldNull = processor.Create(OpCodes.Ldnull);
                            processor.InsertBefore(ins, ldNull);
                            processor.InsertBefore(ldNull, ldZero);
                        }
                    }
                    else
                    {
                        var newGenericMethod = new GenericInstanceMethod(_methodReferences["StartNewTResult"]);
                        newGenericMethod.GenericArguments.Add(genericArgument);
                        ins.OpCode = OpCodes.Call;
                        ins.Operand = newGenericMethod;
                        if (reference.Parameters.Count == 1)
                        {
                            var ldZero = processor.Create(OpCodes.Ldc_I4_0);
                            processor.InsertBefore(ins, ldZero);
                        }
                    }
                }
                else if (reference.Parameters.Count > 0 &&
                         reference.Parameters[0].ParameterType.FullName.Equals("System.Func`2<System.Object,!!0>"))
                {
                    if (reference.Parameters.Count > 2 &&
                        reference.Parameters[2].ParameterType.FullName.Equals(
                            "System.Threading.CancellationToken"))
                    {
                        var newGenericMethod = new GenericInstanceMethod(_methodReferences["StartNewObjectTResultCancel"]);
                        newGenericMethod.GenericArguments.Add(genericArgument);
                        ins.OpCode = OpCodes.Call;
                        ins.Operand = newGenericMethod;
                        if (reference.Parameters.Count == 3)
                        {
                            var ldZero = processor.Create(OpCodes.Ldc_I4_0);
                            var ldNull = processor.Create(OpCodes.Ldnull);
                            processor.InsertBefore(ins, ldNull);
                            processor.InsertBefore(ldNull, ldZero);
                        }
                    }
                    else
                    {
                        var newGenericMethod = new GenericInstanceMethod(_methodReferences["StartNewObjectTResult"]);
                        newGenericMethod.GenericArguments.Add(genericArgument);
                        ins.OpCode = OpCodes.Call;
                        ins.Operand = newGenericMethod;
                        if (reference.Parameters.Count == 2)
                        {
                            var ldZero = processor.Create(OpCodes.Ldc_I4_0);
                            processor.InsertBefore(ins, ldZero);
                        }
                    }
                }
                PopTaskFactory(module, method, ins, processor, variableDefinitions, true);
            }
        }

        private static void InstrumentateCall(Instruction ins, ILProcessor processor, Dictionary<string, VariableDefinition> variableDefinitions)
        {
            var reference = (MethodReference) ins.Operand;

            var monitorEnterFullName =
                "System.Void System.Threading.Monitor::Enter(System.Object,System.Boolean&)";
            var monitorExitFullName =
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
                    var methodReference = (MethodReference) ins.Operand;
                    var genericMethod = (GenericInstanceMethod) methodReference;
                    var genericArgument = genericMethod.GenericArguments[0];
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
                else if (
                    reference.Parameters[0].ParameterType.FullName.Equals(
                        "System.Func`1<System.Threading.Tasks.Task`1<!!0>>"))
                {
                    var methodReference = (MethodReference) ins.Operand;
                    var genericMethod = (GenericInstanceMethod) methodReference;
                    var genericArgument = genericMethod.GenericArguments[0];
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

        private static TypeReference GetValueTypeReference(Instruction ins, Dictionary<string, VariableDefinition> variableDefinitions,
            MethodDefinition method, out VariableDefinition varDefinition)
        {
            var valueTypeReference = (TypeReference) ins.Operand;

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
            return valueTypeReference;
        }

        private static void InstrumentateStfld(Instruction ins, Dictionary<string, VariableDefinition> variableDefinitions, MethodDefinition method,
            ILProcessor processor)
        {
            var fieldDefinition = (FieldDefinition) ins.Operand;
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

        private static void InstrumentateLdfld(Instruction ins, ILProcessor processor, MethodDefinition method)
        {
            var fieldDefinition = (FieldDefinition) ins.Operand;
            var fieldType = fieldDefinition.FieldType;
            if (fieldType.IsPrimitive ||
                (fieldType.IsDefinition && ((TypeDefinition) fieldType).IsValueType))
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

        private static void InstrumentateStsfld(Instruction ins, ILProcessor processor, MethodDefinition method)
        {
            var fieldDefinition = (FieldReference) ins.Operand;
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

        private static void InstrumentateInitobj(Instruction ins, ILProcessor processor, MethodDefinition method)
        {
            var fieldType = (TypeReference) ins.Operand;
            if (fieldType.IsDefinition && ((TypeDefinition) fieldType).IsValueType)
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

        private static void InstrumentateLdsfld(Instruction ins, ILProcessor processor, MethodDefinition method)
        {
            var fieldDefinition = (FieldReference) ins.Operand;

            var fieldType = fieldDefinition.FieldType;
            if (fieldType.IsPrimitive ||
                (fieldType.IsDefinition && ((TypeDefinition) fieldType).IsValueType))
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

        private static void PopTaskFactory(ModuleDefinition module, MethodDefinition method, Instruction ins, ILProcessor processor, 
            Dictionary<string, VariableDefinition> variableDefinitions, bool generics)
        {
            VariableDefinition varDefinition;
            if (!generics)
            {
                varDefinition = variableDefinitions["task"];
            }
            else
            {
                var methodReference = (MethodReference)ins.Operand;
                var genericMethod = (GenericInstanceMethod) methodReference;
                var genericType = new GenericInstanceType(genericMethod.ReturnType.GetElementType());
                genericType.GenericArguments.Add(genericMethod.GenericArguments[0]);
                var typeReference = module.Import(genericType);
                varDefinition = new VariableDefinition(typeReference);
                method.Body.Variables.Add(varDefinition);
            }

            var storeTask = processor.Create(OpCodes.Stloc_S, varDefinition);
            var loadTask = processor.Create(OpCodes.Ldloc_S, varDefinition);
            var pop = processor.Create(OpCodes.Pop);
            processor.InsertAfter(ins, storeTask);
            processor.InsertAfter(storeTask, pop);
            processor.InsertAfter(pop, loadTask);
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
            _typeReferences.Add("task", new ByReferenceType(module.Import(typeof(Task))));
        }

        private static Dictionary<string, VariableDefinition> AddAllVariablesToMethod(MethodDefinition method)
        {
            var result = new Dictionary<string, VariableDefinition>();
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
            var assemblyFile = new FileInfo(fileName);
            var pathToAssembly = assemblyFile.FullName;
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(assemblyFile.DirectoryName);

            var parameters = new ReaderParameters
            {
                AssemblyResolver =  resolver
            };

            var assemblyDefinition = AssemblyDefinition.ReadAssembly(pathToAssembly, parameters);
            var result = string.Empty;

            MethodDefinition method = null;
            TypeDefinition typeDefinition = null;
            foreach (var type in assemblyDefinition.MainModule.Types)
            {
                method = GetMethodeDefinition(typeName, methodName, type, out typeDefinition);
                if (method != null)
                {
                    break;
                }
            }

            if (method != null)
            {
                var ins = method.Body.Instructions.SingleOrDefault(x => x.Offset == offset);

                AddDetectedInstructionBefore(assemblyDefinition.MainModule, method, ins);
                var astBuilder = new AstBuilder(new DecompilerContext(assemblyDefinition.MainModule)
                {
                    CurrentType = typeDefinition,
                    CurrentMethod = method
                });
                
                astBuilder.AddMethod(method);
                
                var output = new StringWriter();
                astBuilder.GenerateCode(new PlainTextOutput(output));
                result = output.ToString();
                output.Dispose();
            }
            return result;
        }

        private static void AddDetectedInstructionBefore(ModuleDefinition module, MethodDefinition method, Instruction ins)
        {
            var refModul = ModuleDefinition.ReadModule("DPCLibrary.dll");
            var typeDefinition = refModul.Types.First(x => x.Name == "DpcLibrary");
            var raceConditionDetectedDef =
                typeDefinition.Methods.Single(x => x.Name == "RaceConditionDetectedIdentifier");
            var raceConditionDetectedRef = module.Import(raceConditionDetectedDef);

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
                var method = type.Methods.SingleOrDefault(x => x.FullName.Equals(methodName));
                if (method != null)
                {
                    return method;
                }
            }

            if (type.HasNestedTypes)
            {
                foreach (var nestedType in type.NestedTypes)
                {
                    var method = GetMethodeDefinition(typeName, methodName, nestedType, out typeDefinition);
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