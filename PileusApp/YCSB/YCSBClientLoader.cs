using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Core;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Pileus;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Pileus.Configuration;
using Microsoft.WindowsAzure.Storage.Pileus.Utils;
using Microsoft.WindowsAzure.Storage.Pileus.Configuration.Constraint;
using PileusApp.Utils;
using PileusApp.YCSB;

namespace PileusApp.YCSB
{
    /// <summary>
    /// Load the azure account with YCSB objects
    /// </summary>
    public class YCSBClientLoader
    {
        private static  int concurrentWorkers = 0;

        #region Locals
        private static string containerName = "democontainer";
        private static CapCloudBlobClient blobClient;
        private static CapCloudBlobContainer container;

        public static int YCSBNumberofObjects = 1000;

        public static bool useHttps = false;
        public static long blobSizeInB = 1024;

        public static string primarySite = "dbteuropestorage";
        public static string secondarySite = "dbtwestusstorage";
        public static string configurationSite = "dbtsouthstorage";

        public static bool wipeEverythingBeforeLoading = false;

        #endregion

        public static void Main(string[] args)
        {
            Process currProc = Process.GetCurrentProcess();

            containerName = "testcontainer";

            if (args.Length == 0)
            {
                args = new string[7];

                //number of objects per container
                args[0] = "1000";

                // the blob size in Byte
                args[1] = "1024";

                // use https or not
                args[2] = "false";

                // primary site account
                args[3] = "dbteastasiastorage";

                // secondary site account, or empty string if no secondary is desired.
                args[4] = "dbteuropestorage"; //dbteuropestorage, dbtsouthstorage, dbteastasiastorage

                // configuration site account. I.e., the site holds configuration of a CapCloudBlobAccount. 
                args[5] = "dbtsouthstorage";

                //true if we want to remove all blobs and containers in our storage account.
                //***CAUTION, this will literally remove everything
                args[6] = "false";

            }

            YCSBNumberofObjects = Int32.Parse(args[0]);
            blobSizeInB = Int32.Parse(args[1]);
            useHttps = bool.Parse(args[2]);

            primarySite= args[3];
            secondarySite = args[4].Equals("") ? null : args[4];
            configurationSite= args[5];

            wipeEverythingBeforeLoading = bool.Parse(args[6]);

            UploadData(YCSBNumberofObjects, blobSizeInB, useHttps, primarySite, secondarySite, configurationSite, wipeEverythingBeforeLoading);
 
            Console.WriteLine("Finihsed populating blobs");
            Console.Read();
        }


        public static void UploadData(int numBlobs, long sizeBlobs, bool secure, string primary, string secondary, string configSite, bool wipe)
        {
            YCSBNumberofObjects = numBlobs;
            blobSizeInB = sizeBlobs;
            useHttps = secure;
            primarySite = primary;
            secondarySite = secondary;
            configurationSite = configSite;
            wipeEverythingBeforeLoading = wipe;

            if (wipeEverythingBeforeLoading)
            {
                foreach (CloudStorageAccount account in Account.GetStorageAccounts(false).Values)
                {
                    foreach (CloudBlobContainer cont in account.CreateCloudBlobClient().ListContainers())
                    {
                        try
                        {

                            foreach (ICloudBlob blob in cont.ListBlobs())
                            {
                                Console.WriteLine("removing " + blob.Name + " ...");
                                try
                                {
                                    blob.BreakLease(new TimeSpan(0));
                                }
                                catch
                                {
                                }
                                blob.DeleteIfExists();
                            }
                        }
                        catch
                        {

                        }
                        try
                        {
                            cont.BreakLease(new TimeSpan(0));
                        }
                        catch
                        {
                        }
                        cont.DeleteIfExists();
                    }
                    //Delete configuration blob for this name
                    account.CreateCloudBlobClient().GetContainerReference(ConstPool.CONFIGURATION_CONTAINER_PREFIX + containerName).DeleteIfExists();
                    //account.CreateCloudBlobClient().GetContainerReference(containerName).DeleteIfExists();

                    CloudTable table = account.CreateCloudTableClient().GetTableReference(ConstPool.SLA_CONFIGURATION_TABLE_NAME);
                    table.DeleteIfExists();
                    table = account.CreateCloudTableClient().GetTableReference(ConstPool.SESSION_STATE_CONFIGURATION_TABLE_NAME);
                    table.DeleteIfExists();
                }

                //We need to wait at least 30 seconds after removing a container. 
                //In other words, it can take up to 30 seconds to remove a container in Azure!
                Console.WriteLine("removed everything, wait 40 seconds ...");
                Thread.Sleep(40000);
            }

            CapCloudStorageAccount storageAccount = new CapCloudStorageAccount();
            blobClient = storageAccount.CreateCloudBlobClient(null);

            ReplicaConfiguration configuration = new ReplicaConfiguration(containerName);
            configuration.PrimaryServers.Add(primarySite);
            configuration.SecondaryServers.Add(secondarySite);

            container = blobClient.GetContainerReference(containerName, new ConsistencySLAEngine(DummySLA(), configuration));

            container.CreateIfNotExists(primarySite, secondarySite);


            ThreadPool.SetMaxThreads(50, 50);

            byte[] BlobDataBuffer = new byte[blobSizeInB];
            Random random = new Random();
            random.NextBytes(BlobDataBuffer);
            List<string> keys = YCSBWorkload.GetAllKeys(YCSBNumberofObjects);

            for (int i = 0; i < keys.Count; i++)
            {
                Put(keys[i], BlobDataBuffer);
            }


            while (Interlocked.CompareExchange(ref concurrentWorkers, -1, 0) != -1)
            {
                Console.WriteLine("Waiting for a thread because thread are " + concurrentWorkers + " threads.");
                Thread.Sleep(1000);
            }

        }

        public static void Put(string blobName, byte[] blobDataBuffer)
        {
            Interlocked.Increment(ref concurrentWorkers);
            ICloudBlob blob = container.GetBlobReferenceFromServer(blobName);

            using (var ms = new MemoryStream(blobDataBuffer))
            {
                blob.UploadFromStream(ms);
            }
            Console.WriteLine("Populated " + blobName);
            Interlocked.Decrement(ref concurrentWorkers);
        }

        public static ServiceLevelAgreement DummySLA()
        {
            ServiceLevelAgreement sla = new ServiceLevelAgreement("" + 1);
            SubSLA subSla1 = new SubSLA(1, Consistency.Strong, 0, 1);
            sla.Add(subSla1);
            return sla;
        }
    }

}