using System;
using System.Linq;
using System.Diagnostics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections;

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

            ModuleDefinition module = ModuleDefinition.ReadModule(fileName);
            MethodReference referencedReadAccessMethod = module.Import(readAccessDef);
            MethodReference referencedWriteAccessMethod = module.Import(writeAccessDef);
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
                            else if (ins.OpCode.Equals(OpCodes.Ldsflda))
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
                        }
                    }
                }
                
            }

            module.Write(fileName);
        }
    }
}
