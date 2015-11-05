using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections;
using System.IO;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using Mono.Cecil.Rocks;

namespace CodeInstrumentation
{
    public class CodeInstrumentator
    {
        /*
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            string fileName = @"TestProgram.exe";
            InjectCodeInstrumentation(fileName);
            Process process = new Process {StartInfo = {FileName = fileName}};
            var readLine = Console.ReadLine();
            while (readLine != null && !readLine.Equals("x"))
            {
                process.Start();
                readLine = Console.ReadLine();
            }
        }
        */

        public static void InjectCodeInstrumentation(string fileName)
        {
            ModuleDefinition refModul = ModuleDefinition.ReadModule("DPCLibrary.dll");
            TypeDefinition typeDefinition = refModul.Types.First(x => x.Name == "DpcLibrary");
            MethodDefinition readAccessDef = typeDefinition.Methods.Single(x => x.Name == "ReadAccess");
            MethodDefinition writeAccessDef = typeDefinition.Methods.Single(x => x.Name == "WriteAccess");
            MethodDefinition lockObjectDef = typeDefinition.Methods.Single(x => x.Name == "LockObject");
            MethodDefinition unlockObjectDef = typeDefinition.Methods.Single(x => x.Name == "UnLockObject");


            ModuleDefinition module = ModuleDefinition.ReadModule(fileName);
            MethodReference referencedReadAccessMethod = module.Import(readAccessDef);
            MethodReference referencedWriteAccessMethod = module.Import(writeAccessDef);
            MethodReference referencedLockObjectMethod = module.Import(lockObjectDef);
            MethodReference referencedUnlockObjectMethod = module.Import(unlockObjectDef);

            TypeReference int8TypeReference = module.Import(typeof (byte));
            TypeReference int16TypeReference = module.Import(typeof (short));
            TypeReference int32TypeReference = module.Import(typeof (int));
            TypeReference int64TypeReference = module.Import(typeof (long));
            TypeReference float32TypeReference = module.Import(typeof (float));
            TypeReference float64TypeReference = module.Import(typeof (double));
            foreach (TypeDefinition type in module.Types)
            {
                InstrumentateType(type, int32TypeReference, int8TypeReference, int16TypeReference, int64TypeReference,
                    float32TypeReference, float64TypeReference, referencedReadAccessMethod, referencedWriteAccessMethod,
                    referencedLockObjectMethod, referencedUnlockObjectMethod);
            }

            module.Write(fileName);
        }

        private static void InstrumentateType(TypeDefinition type, TypeReference int32TypeReference,
            TypeReference int8TypeReference, TypeReference int16TypeReference, TypeReference int64TypeReference,
            TypeReference float32TypeReference, TypeReference float64TypeReference,
            MethodReference referencedReadAccessMethod,
            MethodReference referencedWriteAccessMethod, MethodReference referencedLockObjectMethod,
            MethodReference referencedUnlockObjectMethod)
        {
            if (type.HasNestedTypes)
            {
                foreach (TypeDefinition nestedType in type.NestedTypes)
                {
                    InstrumentateType(nestedType, int32TypeReference, int8TypeReference, int16TypeReference,
                        int64TypeReference, float32TypeReference, float64TypeReference, referencedReadAccessMethod,
                        referencedWriteAccessMethod, referencedLockObjectMethod, referencedUnlockObjectMethod);
                }
            }
            if (type.HasMethods)
            {
                foreach (MethodDefinition method in type.Methods)
                {
                    if (method.Body == null)
                    {
                        continue;
                    }
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
                            FieldReference fieldDefinition = (FieldReference) ins.Operand;

                            TypeReference fieldType = fieldDefinition.FieldType;
                            var processor = method.Body.GetILProcessor();
                            if (fieldType.IsPrimitive ||
                                (fieldType.IsDefinition && ((TypeDefinition) fieldType).IsValueType))
                            {
                                var methodLoad = processor.Create(OpCodes.Ldstr, method.FullName);
                                var constLoad = processor.Create(OpCodes.Ldc_I4, ins.Offset);
                                var loadAddressInstruction = processor.Create(OpCodes.Ldsflda, fieldDefinition);
                                var readAccessLibraryCall = processor.Create(OpCodes.Call, referencedReadAccessMethod);
                                processor.InsertAfter(ins, loadAddressInstruction);
                                processor.InsertAfter(loadAddressInstruction, constLoad);
                                processor.InsertAfter(constLoad, methodLoad);
                                processor.InsertAfter(methodLoad, readAccessLibraryCall);
                            }
                            else if (!fieldType.IsPrimitive && !fieldType.IsValueType)
                            {
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var readAccessLibraryCall = processor.Create(OpCodes.Call, referencedReadAccessMethod);
                                var readAccessLibraryCall2 = processor.Create(OpCodes.Call, referencedReadAccessMethod);
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
                            TypeReference fieldType = (TypeReference) ins.Operand;
                            if (fieldType.IsDefinition && ((TypeDefinition) fieldType).IsValueType)
                            {
                                // TODO: Mehr Fälle bei denen initobj beachtet werden muss?
                                var processor = method.Body.GetILProcessor();
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var constLoad = processor.Create(OpCodes.Ldc_I4, ins.Offset);
                                var methodLoad = processor.Create(OpCodes.Ldstr, method.FullName);
                                var writeAccessLibraryCall = processor.Create(OpCodes.Call, referencedWriteAccessMethod);
                                processor.InsertBefore(ins, writeAccessLibraryCall);
                                processor.InsertBefore(writeAccessLibraryCall, methodLoad);
                                processor.InsertBefore(methodLoad, constLoad);
                                processor.InsertBefore(constLoad, dupInstruction);
                            }
                        }
                        else if (ins.OpCode.Equals(OpCodes.Stsfld))
                        {
                            FieldDefinition fieldDefinition = (FieldDefinition) ins.Operand;
                            var processor = method.Body.GetILProcessor();
                            var loadAddressInstruction = processor.Create(OpCodes.Ldsflda, fieldDefinition);
                            var constLoad = processor.Create(OpCodes.Ldc_I4, ins.Offset);
                            var methodLoad = processor.Create(OpCodes.Ldstr, method.FullName);
                            var writeAccessLibraryCall = processor.Create(OpCodes.Call,
                                referencedWriteAccessMethod);
                            processor.InsertBefore(ins, writeAccessLibraryCall);
                            processor.InsertBefore(writeAccessLibraryCall, methodLoad);
                            processor.InsertBefore(methodLoad, constLoad);
                            processor.InsertBefore(constLoad, loadAddressInstruction);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Ldfld))
                        {
                            FieldDefinition fieldDefinition = (FieldDefinition) ins.Operand;
                            TypeReference fieldType = fieldDefinition.FieldType;
                            var processor = method.Body.GetILProcessor();
                            if (fieldType.IsPrimitive ||
                                (fieldType.IsDefinition && ((TypeDefinition) fieldType).IsValueType))
                            {
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var loadAddressInstruction = processor.Create(OpCodes.Ldflda, fieldDefinition);
                                var constLoad = processor.Create(OpCodes.Ldc_I4, ins.Offset);
                                var methodLoad = processor.Create(OpCodes.Ldstr, method.FullName);
                                var readAccessLibraryCall = processor.Create(OpCodes.Call,
                                    referencedReadAccessMethod);
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
                                    referencedReadAccessMethod);
                                processor.InsertBefore(ins, readAccessLibraryCall);
                                processor.InsertBefore(readAccessLibraryCall, methodLoad);
                                processor.InsertBefore(methodLoad, constLoad);
                                processor.InsertBefore(constLoad, dupInstruction);
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
                            TypeReference arrayTypeReference = (TypeReference) ins.Operand;
                            InjectArrayLdElement(firstInt32VariableDefinition, arrayTypeReference, method,
                                referencedReadAccessMethod, ins);
                        }
                        else if (ins.OpCode.Equals(OpCodes.Ldelem_I) || ins.OpCode.Equals(OpCodes.Ldelem_I1)
                                 || ins.OpCode.Equals(OpCodes.Ldelem_I2) || ins.OpCode.Equals(OpCodes.Ldelem_I4) ||
                                 ins.OpCode.Equals(OpCodes.Ldelem_Ref) || ins.OpCode.Equals(OpCodes.Ldelem_U1) ||
                                 ins.OpCode.Equals(OpCodes.Ldelem_U2) ||
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
                    }
                    method.Body.OptimizeMacros(); // Convert the normal branches back to short branches if possible
                }
            }
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
            processor.InsertBefore(constLoad, loadAddressInstruction);
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
            AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(pathToAssembly);
            string result = "";

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