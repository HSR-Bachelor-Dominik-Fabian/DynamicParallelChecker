using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using DPCClient.Model;
using DPCClient.ViewModel;
using Newtonsoft.Json;

namespace DPCClient.Process
{
    class NLogSocketProcessor
    {
        private readonly HttpListener _listener = new HttpListener();

        public NLogSocketProcessor()
        {
            _listener.Prefixes.Add("http://localhost:9999/NLog/");
            _listener.Start();
        }

        public void Run(DPCViewModel viewModel)
        {
            Dispatcher dispacherObject = Dispatcher.CurrentDispatcher;
            ThreadPool.QueueUserWorkItem(o =>
            {
                Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                StreamReader stream = new StreamReader(ctx.Request.InputStream);
                                string x = stream.ReadToEnd();
                                NLogMessage message = JsonConvert.DeserializeObject<NLogMessage>(x);
                                dispacherObject.Invoke(() =>
                                {
                                    viewModel.AddLogEntry(new LogEntryModel(message.Message));
                                });
                                
                            }
                            catch
                            {
                                // ignored
                            } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch
                {
                    // ignored
                } // suppress any exceptions
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }

    }
}
