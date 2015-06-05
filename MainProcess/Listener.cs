using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Util;

namespace MainProcess
{
    static class Listener
    {

        private static int bufferCapacity = int.Parse(ConfigurationManager.AppSettings["BufferCapacity"]);
        private static readonly CircularBuffer BUFFER = new CircularBuffer(bufferCapacity * 100);
        private static int consumerTransactionCount = 0;
        private static int producerTransactionCount = 0;

        public static void StartServer(CancellationToken token)
        {
            var server = new NamedPipeServerStream("DataServer");

            //Listens PipeServerStream for GET/PUT calls from processes
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("Canceled");
                    break;
                }

                server.WaitForConnection();
                StreamReader reader = new StreamReader(server);
                StreamWriter writer = new StreamWriter(server);

                var commandLine = reader.ReadLine();

                //If client wants to PUT transaction
                if (commandLine.StartsWith("PUT"))
                {
                    byte[] data = Convert.FromBase64String(reader.ReadLine());
                    BUFFER.Enqueue(data);
                    Console.WriteLine("-----------------------------------------------------------------------------");
                    Console.WriteLine("Producing transaction. Input:" + Encoding.UTF8.GetString(data).ToString().Substring(20));
                    writer.Write("OK");
                    writer.Flush();
                    producerTransactionCount++;
                }
                    //If client wants to GET existing transaction
                else if (commandLine.StartsWith("GET"))
                {
                    byte[] data = BUFFER.Dequeue();

                    Console.WriteLine("-----------------------------------------------------------------------------");
                    Console.WriteLine("Consuming transaction. Output:" + Crypto.Decrypt(Encoding.UTF8.GetString(data).ToString().Substring(20),Encoding.UTF8.GetString(data).ToString().Substring(4,16), true));
                    writer.WriteLine(Convert.ToBase64String(data));
                    writer.Flush();
                    consumerTransactionCount++;
                }

                server.Disconnect();
            }

            server.Close();
            server.Dispose();
        }
        //To get producer transaction count
        public static int getProducerTransactionsCount()
        {
            return producerTransactionCount;

        }
        //To get consumer transaction count
        public static int getConsumerTransactionsCount()
        {
            return consumerTransactionCount;

        }
    }
}