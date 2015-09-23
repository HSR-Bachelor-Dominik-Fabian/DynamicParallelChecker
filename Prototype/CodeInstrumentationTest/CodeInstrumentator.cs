using System;
using System.CodeDom;
using System.Linq;
using System.Diagnostics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections;
using Mono.CompilerServices.SymbolWriter;

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
            TypeDefinition pipecommdef =refModul.Types.First(x => x.Name == "DpcLibrary");
            MethodDefinition readAccessDef = pipecommdef.Methods.Single(x => x.Name == "ReadAccess");

            ModuleDefinition module = ModuleDefinition.ReadModule(fileName);
            MethodReference referencedReadAccessMethod = module.Import(readAccessDef);
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
                                var processor = method.Body.GetILProcessor();
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var readAccessLibraryCall = processor.Create(OpCodes.Call, referencedReadAccessMethod);
                                processor.InsertAfter(ins, dupInstruction);
                                processor.InsertAfter(dupInstruction, readAccessLibraryCall);
                            }
                            else if (ins.OpCode.Equals(OpCodes.Ldsflda))
                            {
                                var processor = method.Body.GetILProcessor();
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var dupInstruction2 = processor.Create(OpCodes.Dup);
                                var makeRefAnyTypeInstuction = processor.Create(OpCodes.Mkrefany);
                                var refAnyTypeInstruction = processor.Create(OpCodes.Refanytype);
                                var popInstruciton = processor.Create(OpCodes.Pop);
                                var boxInstruction = processor.Create(OpCodes.Box, popInstruciton);
                                var readAccessLibraryCall = processor.Create(OpCodes.Call, referencedReadAccessMethod);
                                processor.InsertAfter(ins, dupInstruction);
                                processor.InsertAfter(dupInstruction, dupInstruction2);
                                processor.InsertAfter(dupInstruction2, makeRefAnyTypeInstuction);
                                processor.InsertAfter(makeRefAnyTypeInstuction, refAnyTypeInstruction);
                                processor.InsertAfter(refAnyTypeInstruction, boxInstruction);
                                processor.InsertAfter(boxInstruction, readAccessLibraryCall);
                            }
                        }
                    }
                }
                
            }

            module.Write(fileName);
        }
    }
}
