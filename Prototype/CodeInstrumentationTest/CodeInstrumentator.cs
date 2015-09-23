using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections;

namespace CodeInstrumentationTest
{
    class CodeInstrumentator
    {
        static void Main(string[] args)
        {
            string fileName = @"TestProgram.exe";
            PrintTypes(fileName);
            Process process = new Process();
            process.StartInfo.FileName = fileName;
            process.Start();
            Console.WriteLine("Enter to stop");
            Console.ReadLine();
        }

        public static void PrintTypes(string fileName)
        {
            ModuleDefinition refModul = ModuleDefinition.ReadModule("DPCLibrary.dll");
            TypeDefinition pipecommdef =refModul.Types.First(x => x.Name == "DPCLibrary");
            MethodDefinition readAccessDef = pipecommdef.Methods.Single(x => x.Name == "readAccess");

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
                            if (OpCodes.Ldsfld.Equals(ins.OpCode))
                            {
                                Type insType = ins.GetType();
                                var processor = method.Body.GetILProcessor();
                                var dupInstruction = processor.Create(OpCodes.Dup);
                                var readAccessLibraryCall = processor.Create(OpCodes.Call, referencedReadAccessMethod);
                                processor.InsertAfter(ins, dupInstruction);
                                processor.InsertAfter(dupInstruction, readAccessLibraryCall);
                            }
                        }
                    };
                }
                
            }

            module.Write(fileName);
        }
    }
}
