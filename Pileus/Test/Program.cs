namespace PileusTest
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Pileus;
  

    public class Program
    {
        #region Locals
        private static string containerName = null;
        private static CloudBlobClient blobClient;
        private static CloudBlobContainer container;

        private static double blobs = 1;
        private static int iterations = 1;
        private static int concurrency = 1;
        public static int parallelRequestsPerBlob = 1;
        public static bool useHttps = false;
        public static string resultFileFolderName;
        public static long blobSizeInKB = 1;

        private static string resultFile;
        private static List<long> insertTimes = new List<long>();
        private static List<long> getTimes = new List<long>();
        private static List<long> deleteTimes = new List<long>();
        private static int concurrentWorkers = 0;
        public static byte[] BlobDataBuffer;
        #endregion

        public static void Main(string[] args)
        {
            Process currProc = Process.GetCurrentProcess();
            string accountName = ConfigurationManager.AppSettings["AccountName"];
            string accountKey = ConfigurationManager.AppSettings["AccountKey"];
            containerName = Guid.NewGuid().ToString("N").ToLower();

            if (args.Length == 0)
            {
                args = new string[7];

                // the number of test iterations
                args[0] = "1";

                // the number of blobs
                args[1] = "100";

                // the number of concurrent test workers.
                args[2] = "2"; 

                // the blob size in KB
                args[3] = "1024";

                // the number of parallel requests per blob
                args[4] = "1";

                // use https or not
                args[5] = "false";

                // the result folder name
                args[6] = "folder1";
            }

            iterations = Int32.Parse(args[0]);
            blobs = Int32.Parse(args[1]);
            concurrency = Int32.Parse(args[2]);
            blobSizeInKB = Int32.Parse(args[3]);
            parallelRequestsPerBlob = Int32.Parse(args[4]);
            useHttps = bool.Parse(args[5]);
            resultFileFolderName = args[6];

            if (!Directory.Exists(resultFileFolderName))
            {
                Directory.CreateDirectory(resultFileFolderName);
            }

            resultFile = string.Format(@"{6}\{0}_{1}_{2}_{3}_{4}_{5}.csv", blobSizeInKB, parallelRequestsPerBlob, concurrency, useHttps, iterations, blobs, resultFileFolderName);

            ServicePointManager.DefaultConnectionLimit = 2 * concurrency * parallelRequestsPerBlob;
            ThreadPool.SetMinThreads(concurrency * parallelRequestsPerBlob, concurrency * parallelRequestsPerBlob);
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.Expect100Continue = false;


            StorageCredentials creds = new StorageCredentials(accountName, accountKey);
            CloudStorageAccount httpAcc = new CloudStorageAccount(creds, useHttps);
            blobClient = httpAcc.CreateCloudBlobClient();

            if (parallelRequestsPerBlob != 0)
            {
                blobClient.ParallelOperationThreadCount = parallelRequestsPerBlob;
            }

            container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();
            blobClient.RetryPolicy = new NoRetry();
            

            // Generate random data
            BlobDataBuffer = new byte[1024 * blobSizeInKB];
            Random random = new Random();
            random.NextBytes(BlobDataBuffer);

            Stopwatch totalWatch = new Stopwatch();
            totalWatch.Start();
            for (int m = 0; m < concurrency; m++)
            {
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    Interlocked.Increment(ref concurrentWorkers);
                    for (int i = 0; i < iterations; i++)
                    {
                        try
                        {
                            // do upload blob test.
                            var blobsList = UploadBlob();

                            // GET and DELETE.
                            DoGetAndDelete(blobsList);   
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }

                    Interlocked.Decrement(ref concurrentWorkers);
                });
            }

            Thread.Sleep(5000);
            while (Interlocked.CompareExchange(ref concurrentWorkers, -1, 0) != -1)
            {
                Thread.Sleep(5000);
            }

            container.DeleteIfExists();

            totalWatch.Stop();
            long totalTimeTaken = totalWatch.ElapsedMilliseconds;
            using (StreamWriter sw = new StreamWriter(resultFile))
            {
                sw.Write(String.Format("Total time taken to run the test in ms : {0}\n\n", totalTimeTaken));
                sw.Write(String.Format("Args:Concurrency: {0} BlobSizeInKB:{1} ParallelRequestsPerBlob:{2} UsingHttps:{3} \n", concurrency, blobSizeInKB, parallelRequestsPerBlob, useHttps));

                // display result
                DisplayResults(sw, "Insert", insertTimes);
                DisplayResults(sw, "Get", getTimes);
                DisplayResults(sw, "Delete", deleteTimes);

                // Display perf results
                PerfMetrics metrics = new PerfMetrics();
                metrics.Update(currProc);
                metrics.Print(sw);
            }
        }

        #region Test methods

        public static List<string> UploadBlob()
        {
            var result = new List<string>();
            for (int i = 0; i < blobs; i++)
            {
                string blobName = Guid.NewGuid().ToString();
                var watch = StartTest("Insert");
                try
                {
                    CapCloudBlob b = (CapCloudBlob) container.GetBlockBlobReference(blobName);
                    using (var ms = new MemoryStream(BlobDataBuffer))
                    {
                        b.UploadFromStream(ms);
                    }

                    insertTimes.Add(StopTest(watch));
                    result.Add(blobName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            return result;
        }

        public static void DoGetAndDelete(List<string> blobList)
        {
            for (int i = 0; i < blobList.Count; i++)
            {
                CapCloudBlob blob = (CapCloudBlob) container.GetBlockBlobReference(blobList[i]);
                var watch = StartTest("GetAndDelete");
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        blob.DownloadToStream(ms);
                    }

                    watch.Stop();
                    getTimes.Add(watch.ElapsedTicks);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                watch.Restart();
                try
                {
                    blob.Delete();
                    deleteTimes.Add(StopTest(watch));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        #endregion

        #region Utils

        public static Stopwatch StartTest(string testType)
        {
            // Start the test.
            Stopwatch watch = new Stopwatch();
            Console.WriteLine(testType + " starts... " + DateTime.Now.ToString());
            watch.Start();
            return watch;
        }

        public static long StopTest(Stopwatch watch)
        {
            // The test completes.
            Console.WriteLine("Test ends...   " + DateTime.Now.ToString());
            watch.Stop();
            return watch.ElapsedTicks;
        }

        public static void DisplayResults(StreamWriter sw, string name, List<long> data)
        {
            // Is there a better way to get the times in microseconds ?
            double microsecondspertick = 1000000 / (double)Stopwatch.Frequency;
            if (data.Count > 0)
            {
                long tVal = 0;
                long minVal = Int64.MaxValue;
                long maxVal = 0;
                
                foreach (long val in data)
                {
                    tVal += val;
                    if (val > maxVal)
                    {
                        maxVal = val;
                    }

                    if (val < minVal)
                    {
                        minVal = val;
                    }
                }

                tVal /= data.Count;

                sw.Write(String.Format("{0},,{1},{2},{3},\n", name, minVal * microsecondspertick, tVal * microsecondspertick, maxVal * microsecondspertick));

                Console.WriteLine(String.Format(
                        "Results for {0}: (MIN/AVG/MAX) in Microseconds: ({1}/{2}/{3})", name, minVal * microsecondspertick, tVal * microsecondspertick, maxVal * microsecondspertick));

            }
        }
        #endregion
    }

    public class PerfMetrics
    {
        public TimeSpan TotalProcessorTime { get; set; }
        public TimeSpan UserProcessorTime { get; set; }
        public TimeSpan PrivilegedProcessorTime { get; set; }

        public long PeakVirtualMemorySize64 { get; set; }
        public long VirtualMemorySize64 { get; set; }

        public long PeakPagedMemorySize64 { get; set; }
        public long PagedMemorySize64 { get; set; }

        public long PeakWorkingSet64 { get; set; }
        public long WorkingSet64 { get; set; }

        public long PrivateMemorySize64 { get; set; }

        public long NonpagedSystemMemorySize64 { get; set; }

        public void Update(Process proc)
        {
            this.TotalProcessorTime = proc.TotalProcessorTime;
            this.UserProcessorTime = proc.UserProcessorTime;
            this.PrivilegedProcessorTime = proc.PrivilegedProcessorTime;

            this.PeakVirtualMemorySize64 = proc.PeakVirtualMemorySize64;
            this.VirtualMemorySize64 = proc.VirtualMemorySize64;

            this.PeakPagedMemorySize64 = proc.PeakPagedMemorySize64;
            this.PagedMemorySize64 = proc.PagedMemorySize64;

            this.PeakWorkingSet64 = proc.PeakWorkingSet64;
            this.WorkingSet64 = proc.WorkingSet64;

            this.PrivateMemorySize64 = proc.PrivateMemorySize64;
            this.NonpagedSystemMemorySize64 = proc.NonpagedSystemMemorySize64;
        }

        public void Print(StreamWriter sw)
        {
            Console.WriteLine("Processor Time (ms) (User/ Priv / Total) = {0} / {1} /{2}", this.UserProcessorTime.TotalMilliseconds, this.PrivilegedProcessorTime.TotalMilliseconds, this.TotalProcessorTime.TotalMilliseconds);
            sw.Write(String.Format("UserProcessorTime,, PrivilegedProcessorTime, TotalProcessorTime\n"));
            sw.Write(String.Format("{0},,{1},{2},\n", this.UserProcessorTime.TotalMilliseconds, this.PrivilegedProcessorTime.TotalMilliseconds, this.TotalProcessorTime.TotalMilliseconds));

            Console.WriteLine("Working Set (Current / Peak KB) = {0} / {1}", this.WorkingSet64 / (1024), this.PeakWorkingSet64 / (1024));
            sw.Write(String.Format("Working Set - Current,, Working Set - Peak in KB\n"));
            sw.Write(String.Format("{0},,{1},\n", this.WorkingSet64 / (1024), this.PeakWorkingSet64 / (1024)));

            Console.WriteLine("VirtualMemory (Current / Peak KB) = {0} / {1}", this.VirtualMemorySize64 / (1024), this.PeakVirtualMemorySize64 / (1024));
            sw.Write(String.Format("Virtual Memory - Current,, Virtual Memory - Peak in KB\n"));
            sw.Write(String.Format("{0},,{1},\n", this.VirtualMemorySize64 / (1024), this.PeakVirtualMemorySize64 / (1024)));

            Console.WriteLine("PagedMemory (Current / Peak KB) = {0} / {1}", this.PagedMemorySize64 / (1024), this.PeakPagedMemorySize64 / (1024));
            sw.Write(String.Format("PagedMemory - Current,, PagedMemory - Peak in KB\n"));
            sw.Write(String.Format("{0},,{1},\n", this.PagedMemorySize64 / (1024), this.PeakPagedMemorySize64 / (1024)));

            Console.WriteLine("NonpagedSystemMemorySize = {0}", this.NonpagedSystemMemorySize64 / 1024);
            sw.Write(String.Format("NonPagedMemory in KB\n"));
            sw.Write(String.Format("{0}\n", this.NonpagedSystemMemorySize64 / 1024));

            Console.WriteLine("PrivateMemory = {0}", this.PrivateMemorySize64 / 1024);
            sw.Write(String.Format("PagedMemory in KB\n"));
            sw.Write(String.Format("{0}\n", this.PrivateMemorySize64 / 1024));
        }
    }
}