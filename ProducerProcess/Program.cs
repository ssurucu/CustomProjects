using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Util;

namespace ProducerProcess
{
    class Program
    {
        
        static void Main(string[] args)
        {
            int DemoTime = int.Parse(ConfigurationManager.AppSettings["DemoTime"]);
            int DelayMin = int.Parse(ConfigurationManager.AppSettings["DelayMin"]);
            int DelayMax = int.Parse(ConfigurationManager.AppSettings["DelayMax"]);
            string timestamp;
            string[] alphabet = new string[] { "ABCDEFGHIJKLMNOPRSTUVYWXZ" };
            Random rnd = new Random();
            Stopwatch sw = new Stopwatch();

            //Semaphores are opened for using
            Semaphore b = Semaphore.OpenExisting("Busy");
            Semaphore e = Semaphore.OpenExisting("Empty");
            Semaphore f = Semaphore.OpenExisting("Full");
            Semaphore l = Semaphore.OpenExisting("LogAccess");

            //Process will be killed when it is reached DemoTime
            sw.Start();
            while (sw.ElapsedMilliseconds < DemoTime)
            {
                bool exit = false;

                int nProcessID = Process.GetCurrentProcess().Id;

                l.WaitOne();
                timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss:fff", CultureInfo.InvariantCulture);
                Log.SaveLog(timestamp + " " + nProcessID + " ProducerProcess is producing a transaction.");
                l.Release();

                Console.WriteLine("Producing a transaction. It's a long operation. Please wait...");

                // produce item here with random delay
                Thread.Sleep(rnd.Next(DelayMin, DelayMax));
                string randomText = "";
                string randomLetter = "";

                //Randomizing the data
                int loopNum = rnd.Next(10)+1;
                for (int i = 0; i <= loopNum; i++)
                {
                    for (int j = 0; j <= 7; j++)
                    {
                        randomLetter = alphabet[0].ToString().ElementAt(rnd.Next(25)).ToString();
                        randomText = randomText + randomLetter;
                    }
                
                }

                string randomKey = "";
                string randomKeyLetter = "";

                //Randomizing the encryption key
                for (int i = 0; i <= 15; i++)
                {
                    randomKeyLetter = alphabet[0].ToString().ElementAt(rnd.Next(25)).ToString();
                    randomKey = randomKey + randomKeyLetter;
                }


                //Encrypt the data
                randomText = Crypto.Encrypt(randomText,randomKey, true);

                l.WaitOne();
                timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss:fff", CultureInfo.InvariantCulture);
                Log.SaveLog(timestamp + " " + nProcessID + " ProducerProcess is encrypting the data.");
                l.Release();

                short length = (short)randomText.Length;
                short alg = 1;

                byte[] dataLength = BitConverter.GetBytes(length);
                byte[] encAlg = BitConverter.GetBytes(alg);
                byte[] encKey = Encoding.UTF8.GetBytes(randomKey);
                byte[] data = Encoding.UTF8.GetBytes(randomText);

                byte[] body = dataLength.Concat(encAlg).Concat(encKey).Concat(data).ToArray();
                Console.WriteLine("Transaction produced. Good!!!");

                l.WaitOne();
                timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss:fff", CultureInfo.InvariantCulture);
                Log.SaveLog(timestamp + " " + nProcessID + " ProducerProcess has produced a transaction.");
                l.Release();

                Console.WriteLine("Waiting for empty slots.");

                l.WaitOne();
                timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss:fff", CultureInfo.InvariantCulture);
                Log.SaveLog(timestamp + " " + nProcessID + " ProducerProcess is waiting for empty slots.");
                l.Release();

                while (!e.WaitOne(5))
                {
                    exit = sw.ElapsedMilliseconds > DemoTime;
                    if (exit)
                        break;
                }

                if (exit)
                {
                    break;
                }
                Console.WriteLine("Empty slots available. Good!!!");


                Console.WriteLine("Waiting for storage access.");

                l.WaitOne();
                timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss:fff", CultureInfo.InvariantCulture);
                Log.SaveLog(timestamp + " " + nProcessID + " ProducerProcess is waiting for storage access");
                l.Release();

                b.WaitOne();
                Console.WriteLine("Storage access granted.");

                l.WaitOne();
                timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss:fff", CultureInfo.InvariantCulture);
                Log.SaveLog(timestamp + " " + nProcessID + " ProducerProcess has granted storage access.");
                l.Release();

                // Put call to PipeClientServer
                var client = new NamedPipeClientStream("DataServer");
                client.Connect();

                StreamReader reader = new StreamReader(client);
                StreamWriter writer = new StreamWriter(client);

                // Send data to server
                writer.WriteLine("PUT");
                writer.WriteLine(Convert.ToBase64String(body));

                writer.Flush();

                if (reader.ReadLine() == "OK")
                {
                    Console.WriteLine("Transaction Produced");
                }

                l.WaitOne();
                timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss:fff", CultureInfo.InvariantCulture);
                Log.SaveLog(timestamp + " " + nProcessID + " ProducerProcess has produced transaction.");
                l.Release();

                client.Close();
                client.Dispose();

                Console.WriteLine("---------------------------------------");
                Console.WriteLine("Releasing storage access.");
                b.Release();
                Console.WriteLine("Storage access released.");

                l.WaitOne();
                timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss:fff", CultureInfo.InvariantCulture);
                Log.SaveLog(timestamp + " " + nProcessID + " ProducerProcess has released storage access.");
                l.Release();

                Console.WriteLine("Adding full slot.");
                int full = f.Release();
                Console.WriteLine("Now there are {0} full slots.", full);

                l.WaitOne();
                timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss:fff", CultureInfo.InvariantCulture);
                Log.SaveLog(timestamp + " " + nProcessID + " ProducerProcess added full slot to buffer.");
                l.Release();

            }
        }
    }
}
