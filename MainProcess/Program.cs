using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace MainProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            Task.Factory.StartNew(() =>
                            {
                                Listener.StartServer(token);
                            }, token);

            int bufferCapacity = int.Parse(ConfigurationManager.AppSettings["BufferCapacity"]);

            //Defining the semaphores to be used
            Semaphore b = new Semaphore(1, 1, "Busy"); //The semaphore will be used for Busy condition of the buffer
            Semaphore e = new Semaphore(bufferCapacity, bufferCapacity, "Empty"); //Empty semaphore will represent empty buffer
            Semaphore f = new Semaphore(0, bufferCapacity, "Full"); //Empty semaphore will represent full buffer
            Semaphore l = new Semaphore(1, 1, "LogAccess"); //The semaphore will be used for Busy condition of the logfile
             
            //Read from configuration file
            int producerCount = int.Parse(ConfigurationManager.AppSettings["ProducerNum"]);
            int consumerCount = int.Parse(ConfigurationManager.AppSettings["ConsumerNum"]);

            Task[] producers = new Task[producerCount];
            Task[] consumers = new Task[consumerCount];

            //To start procuder processes
            for (int i = 0; i < producerCount; i++)
            {
                producers[i] = Task.Factory.StartNew(() =>
                {
                    ProcessStartInfo psi = new ProcessStartInfo(@"..\..\..\..\P2-BPC\ProducerProcess\bin\debug\ProducerProcess.exe");
                    Process p = new Process();
                    p.StartInfo = psi;
                    p.Start();
                    p.WaitForExit();
                }, TaskCreationOptions.LongRunning);
            }

            //To start consumer processes
            for (int i = 0; i < consumerCount; i++)
            {
                consumers[i] = Task.Factory.StartNew(() =>
                {
                    ProcessStartInfo psi = new ProcessStartInfo(@"..\..\..\..\P2-BPC\ConsumerProcess\bin\debug\ConsumerProcess.exe");
                    Process p = new Process();
                    p.StartInfo = psi;
                    p.Start();
                    p.WaitForExit();
                }, TaskCreationOptions.LongRunning);
            }

            Console.WriteLine("///////////////////////////////////////////////////////");
            Console.WriteLine("Processes are created");
            Console.WriteLine("///////////////////////////////////////////////////////");
            Task.WaitAll(producers.Concat(consumers).ToArray());

            cts.Cancel();

            int full = f.Release(); 
            int empty = e.Release(); 


            b.Dispose();
            f.Dispose();
            e.Dispose();

            Console.WriteLine("\n");
            Console.WriteLine("///////////////////////////////////////////////////////");
            Console.WriteLine("Time is Up");
            Console.WriteLine("///////////////////////////////////////////////////////"); 
            Console.WriteLine("Processes are killed");
            Console.WriteLine("///////////////////////////////////////////////////////");
            Console.WriteLine("Result:\n");
            Console.WriteLine("Empty Slot:" + empty + " Full Slot:" + full);
            Console.WriteLine("Consumed Transactions Done: " + Listener.getConsumerTransactionsCount() +"\n");
            Console.WriteLine("Produced Transactions Done: " + Listener.getProducerTransactionsCount() + "\n");
            Console.WriteLine("Total Transactions Done: " + full + "\n");
            Console.WriteLine("///////////////////////////////////////////////////////");
            Console.ReadLine();
        }
    }
}
