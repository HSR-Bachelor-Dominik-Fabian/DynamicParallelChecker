# PrototypeCecilPipeCommunication
Small Prototype where the main tool adds PipeCommLib Library to other tool via Cecil, enabling Interprocess Communication over a Pipe.

V1:
To Start Prototype, Build the Project and Copy PipeCommLibrary.dll to bin Directory of PipeTestServer
And Copy PipeTestClient.exe to the same directory

Now the PipeTestServer will add the PipeCommLibrary via Cecil to the PipeTestClient and also will add some code to every Method in the Client.
The Client will then start to send messages over the NamedPipe to the Server
