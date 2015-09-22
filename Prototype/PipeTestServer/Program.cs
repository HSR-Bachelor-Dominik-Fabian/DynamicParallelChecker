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

namespace PipeTestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            PrintTypes("PipeTestClient.exe");
            CancellationTokenSource source = new CancellationTokenSource();
            Console.WriteLine("Server stared... Starting Pipe");
            StartServer(source);
            Console.WriteLine("Starting Tool to be analysed");
            Process process = new Process();
            process.StartInfo.FileName = "PipeTestClient.exe";
            process.Start();
            Console.WriteLine("Enter to stop");
            Console.ReadLine();
            source.Cancel();
            process.CloseMainWindow();

            Console.ReadLine();
        }

        public static void PrintTypes(string fileName)
        {
            ModuleDefinition refModul = ModuleDefinition.ReadModule("PipeCommLibrary.dll");
            TypeDefinition pipecommdef =refModul.Types.First(x => x.Name == "PipeComm");
            MethodDefinition writePipeDef = pipecommdef.Methods.First(x => x.Name == "writePipe");

            ModuleDefinition module = ModuleDefinition.ReadModule(fileName);
            MethodReference referenceMethod = module.Import(writePipeDef);
            foreach (TypeDefinition type in module.Types)
            {
                if (type.HasMethods)
                {
                    Parallel.ForEach(type.Methods, (method) =>
                    {
                        Instruction inst = method.Body.Instructions[0];
                        if (type.Name == "Program")
                        {
                            var processor = method.Body.GetILProcessor();
                            var newInstruction = processor.Create(OpCodes.Call, referenceMethod);
                            var parameterInstruction = processor.Create(OpCodes.Ldstr, "Testfrom" + method.Name);
                            processor.InsertBefore(inst, newInstruction);
                            processor.InsertBefore(newInstruction, parameterInstruction);

                        }
                    });
                }
                
            }

            module.Write(fileName);
        }


        private static void StartServer(CancellationTokenSource source)
        {
            Task.Factory.StartNew((object obj) =>
            {
                CancellationToken token = (CancellationToken)obj;
                readPipe(token);


            },source.Token);
        }

        private static void readPipe(CancellationToken token)
        {
            
            NamedPipeServerStream server = new NamedPipeServerStream("pipeDPCNet", PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances,PipeTransmissionMode.Byte,PipeOptions.Asynchronous);
            if(token.IsCancellationRequested)
            {
                Console.WriteLine("Stop");
                return;
            }
            server.BeginWaitForConnection((IAsyncResult asyncResult) =>
            {
                string output = "";
                using (NamedPipeServerStream srv = (NamedPipeServerStream)asyncResult.AsyncState)
                {
                    srv.EndWaitForConnection(asyncResult);

                    readPipe(token);
                    
                    srv.WaitForPipeDrain();
                    StreamReader reader = new StreamReader(srv);
                    output = reader.ReadLine();
                    if (output != "")
                    {
                        Console.WriteLine(output);
                    }
                }
                     
            }, server);
           
        }
    }
}
