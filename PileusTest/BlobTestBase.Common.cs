// -----------------------------------------------------------------------------------------
// <copyright file="BlobTestBase.Common.cs" company="Microsoft">
//    Copyright 2013 Microsoft Corporation
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>
// -----------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
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
using PileusApp;
using PileusApp.Utils;
using System.Diagnostics;
using PileusApp.YCSB;
using Microsoft.WindowsAzure.Storage.Pileus.Configuration.Actions;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.Pileus
{
    public class DemoLib
    {

        #region Local variables

        static int numReadWrites = 10;
        static int opsBetweenSyncs = 6;
        // static int numBlobs = 1000;
        static int numBlobsToUse = 10;

        static string containerName = "democontainer";

        static string configStorageSite = "dbtsouthstorage";

        static List<string> PrimaryServers = new List<string>() { "dbteuropestorage" };
        static List<string> SecondaryServers = new List<string>() { "dbtwestusstorage", "dbteuropestorage-secondary", "dbtwestusstorage-secondary" };
        static List<string> NonReplicaServers = new List<string>() { "dbtsouthstorage" };
        static List<string> ReadOnlySecondaryServers = new List<string>() { "dbteuropestorage-secondary", "dbtwestusstorage-secondary" };

        static CapCloudBlobClient blobClient;
        static CapCloudBlobContainer container;
        static Dictionary<string, CapCloudBlobContainer> containers;

        static Dictionary<string, CloudStorageAccount> accounts;
        static ReplicaConfiguration config;

        static Configurator configurator;
        static List<ConfigurationAction> proposedActions;

        static Dictionary<string, ConsistencySLAEngine> slaEngines;
        static ServiceLevelAgreement currentSLA;

        static Sampler sampler;

        static int opsSinceSync = 0;
        static bool startExperiment = false;

        static bool clearConfiguration = false;  // should be false unless the first time
        static bool cloudBackedConfiguration = true;  // could be either true or false for demo
        static bool stableConfiguration = true;  // should be set to true when running demo

        #endregion


        #region Configurations and stuff

        public static CapCloudBlobContainer Initialize(string Name)
        {
            containerName = Name;
            Initialize();
            return container;
        }

        public static void Initialize()
        {
            // get list of Azure storage accounts
            accounts = PileusApp.Account.GetStorageAccounts(false);
            ClientRegistry.Init(accounts, accounts[configStorageSite]);

            // delete cloud configuration if desired
            if (clearConfiguration)
            {
                ReplicaConfiguration configToDelete = new ReplicaConfiguration(containerName);
                ConfigurationCloudStore backingStore = new ConfigurationCloudStore(accounts[configStorageSite], configToDelete);
                Log("Deleting configuration in Azure...");
                backingStore.DeleteConfiguration();
                Log("Done.  Now waiting for it to really take effect...");
                Thread.Sleep(40000);  // give system a chance to complete delete
            }

            // read/create configuration 
            Log("Creating/reading configuration...");
            config = new ReplicaConfiguration(containerName, PrimaryServers, SecondaryServers, NonReplicaServers, ReadOnlySecondaryServers, cloudBackedConfiguration, stableConfiguration);
            ClientRegistry.AddConfiguration(config);

            // create server monitor that is shared by all SLA engines
            ServerMonitor monitor = new ServerMonitor(config, true);

            // create SLA engines
            slaEngines = new Dictionary<string, ConsistencySLAEngine>();
            slaEngines["strong"] = new ConsistencySLAEngine(CreateConsistencySla(Consistency.Strong), config, null, monitor);
            slaEngines["causal"] = new ConsistencySLAEngine(CreateConsistencySla(Consistency.Causal), config, null, monitor);
            slaEngines["bounded"] = new ConsistencySLAEngine(CreateBoundedConsistencySla(120), config, null, monitor);
            slaEngines["readmywrites"] = new ConsistencySLAEngine(CreateConsistencySla(Consistency.ReadMyWrites), config, null, monitor);
            slaEngines["monotonic"] = new ConsistencySLAEngine(CreateConsistencySla(Consistency.MonotonicReads), config, null, monitor);
            slaEngines["eventual"] = new ConsistencySLAEngine(CreateConsistencySla(Consistency.Eventual), config, null, monitor);
            currentSLA = CreateFastOrStrongSla();
            slaEngines["sla"] = new ConsistencySLAEngine(currentSLA, config, null, monitor);

            // get/create replicated container
            CapCloudStorageAccount storageAccount = new CapCloudStorageAccount();
            blobClient = storageAccount.CreateCloudBlobClient(ActualNumberOfClients);
            container = blobClient.GetContainerReference(containerName, slaEngines["sla"]);
            //container.CreateIfNotExists(primarySite, secondarySite);
            containers = new Dictionary<string, CapCloudBlobContainer>();
            foreach (string cons in slaEngines.Keys)
            {
                containers[cons] = blobClient.GetContainerReference(containerName, slaEngines[cons]);
            }

            configurator = new Configurator(containerName);
        }

        
        public static void SetInitialConfiguration()
        {
            // Note: This is not the correct way to change the configuration.  We should actually use the Configurator.

            // end current epoch
            config.EndCurrentEpoch();

            // restore to original primary and secondary
            config.PrimaryServers = PrimaryServers.ToList();
            config.SecondaryServers = SecondaryServers.ToList();
            config.ReadOnlySecondaryServers = ReadOnlySecondaryServers.ToList();
            config.NonReplicaServers = NonReplicaServers.ToList();

            // start new epoch and upload to cloud
            config.StartNewEpoch();

            slaEngines["sla"].Sla.ResetHitsAndMisses();
        }

        public static void ProposeNewConfiguration()
        {
            List<ConfigurationConstraint> constraints = new List<ConfigurationConstraint>();
            // for now, we have no constraints...
            // constraints.Add(new LocationConstraint(containerName, "dbtwestusstorage", LocationConstraintType.Replicate));
            // constraints.Add(new ReplicationFactorConstraint(containerName, 1, 2));

            proposedActions = configurator.PickNewConfiguration(containerName, slaEngines["sla"].Sla, slaEngines["sla"].Session, slaEngines["sla"].Monitor, config, constraints);
        }

        public static void InstallNewConfiguration()
        {
            if (proposedActions == null || proposedActions.Count == 0)
            {
                return;  // nothing to do
            }
            configurator.InstallNewConfiguration(proposedActions);
            slaEngines["sla"].Sla.ResetHitsAndMisses();
        }

        public static void CreateDatabase(int numBlobs)
        {
            foreach (string secondarySite in SecondaryServers)
            {
                if (!ReadOnlySecondaryServers.Contains(secondarySite))
                {
                    YCSBClientLoader.UploadData(numBlobs, 1024, false, PrimaryServers.First(), secondarySite, configStorageSite, false);
                }
            }
        }

        #endregion


        #region Reads, Writes, Pings, and Syncs

        public static Sampler PerformReadsWritesSyncs(Sampler reuseSampler = null)
        {
            YCSBWorkload workload = new YCSBWorkload(YCSBWorkloadType.Workload_a, numBlobsToUse);

            sampler = reuseSampler;
            if (sampler == null)
            {
                sampler = YCSBClientExecutor.NewSampler();
                foreach (string cons in containers.Keys)
                {
                    sampler.AddSampleName(cons + "Latency", Sampler.OutputType.Average);
                    sampler.AddSampleName(cons + "PrimaryAccesses", Sampler.OutputType.Total);
                    sampler.AddSampleName(cons + "TotalAccesses", Sampler.OutputType.Total);
                }
            }

            byte[] BlobDataBuffer = new byte[1024];
            Random random = new Random();
            random.NextBytes(BlobDataBuffer);

            // Synchronize before we start the experiment
            if (startExperiment == true)
            {
                Log("Syncing data to secondary replicas...");
                SyncSecondaryServers();
                startExperiment = false;
            }

            for (int ops = 0; ops < numReadWrites; ops++)
            {
                YCSBOperation op = workload.GetNextOperation();
                if (op == null)
                {
                    // exhasted trace so get new one
                    workload = new YCSBWorkload(YCSBWorkloadType.Workload_a, numBlobsToUse);
                    op = workload.GetNextOperation();
                }

                long duration = 0;
                if (op.Type == YCSBOperationType.READ)
                {
                    // HACK: warm up the primary server cache the first time we access it
                    // Reason: Strong pays higher latency compared to later consistencies in "containers" because of cold misses.
                    //duration = YCSBClientExecutor.GetBlob(op.KeyName, containers["eventual"]);
                    //duration = YCSBClientExecutor.GetBlob(op.KeyName, containers["strong"]);

                    foreach (string cons in containers.Keys)
                    {
                        ServerState ss = slaEngines[cons].FindServerToRead(op.KeyName);
                        // executing GetBlob twice substantially reduces the latency variance
                        duration = YCSBClientExecutor.GetBlob(op.KeyName, containers[cons]);
                        duration = YCSBClientExecutor.GetBlob(op.KeyName, containers[cons]);
                        sampler.AddSample(cons + "Latency", duration);
                        sampler.AddSample(cons + "TotalAccesses", 1);
                        if (ss.IsPrimary == true)
                        {
                            sampler.AddSample(cons + "PrimaryAccesses", 1);
                        }

                        Log("Performed " + cons + " read for " + op.KeyName + " in " + duration + " from " + SiteName(ss.Name));
                    }
                    sampler.AddSample("ReadCount", 1);
                }
                else if (op.Type == YCSBOperationType.UPDATE)
                {
                    random.NextBytes(BlobDataBuffer);
                    foreach (string cons in containers.Keys)
                    {
                        duration = YCSBClientExecutor.PutBlob(op.KeyName, BlobDataBuffer, containers[cons]);
                        Log("Performed " + cons + "Write for " + op.KeyName + " in " + duration);
                    }
                    sampler.AddSample("WriteCount", 1);
                    sampler.AddSample("WriteLatency", duration);
                }

                if (++opsSinceSync >= opsBetweenSyncs)
                {
                    Log("Syncing data to secondary replicas...");
                    SyncSecondaryServers();
                    opsSinceSync = 0;
                }
            }
            return sampler;
        }

        public static void PingAllServers()
        {
            ServerMonitor ss = slaEngines.First().Value.Monitor;
            Stopwatch watch = new Stopwatch();
            for (int pingCount = 0; pingCount < 5; pingCount++)
            {
                // foreach (string server in ss.replicas.Keys)
                foreach (string server in ss.replicas.Keys)
                {
                    // ServerState serverState = ss.replicas[server];
                    ServerState serverState = ss.replicas[server];
                    CloudBlobContainer tmpContainer = ClientRegistry.GetCloudBlobContainer(server, containerName);
                    watch.Restart();
                    //we perform a dummy operation to get the rtt latency!
                    bool ok = tmpContainer.Exists();
                    long el = watch.ElapsedMilliseconds;
                    serverState.AddRtt(el);
                    Log("Pinged " + SiteName(server) + " in " + el + " milliseconds");
                }
            }
        }

        private static void SyncSecondaryServers()
        {
            string primarySite = config.PrimaryServers.First();
            foreach (string secondarySite in config.SecondaryServers)
            {
                if (!config.ReadOnlySecondaryServers.Contains(secondarySite))
                {
                    SynchronizeContainer synchronizer = new SynchronizeContainer(ClientRegistry.GetCloudBlobContainer(primarySite, containerName),
                        ClientRegistry.GetCloudBlobContainer(secondarySite, containerName));
                    synchronizer.SyncContainers();
                    Log("Synchronization completed to " + SiteName(secondarySite) + ".");
                }
                else
                {
                    Log("Could not sync to " + SiteName(secondarySite) + " since secondary is read-only.");
                }
            }
        }

        #endregion


        #region SLAs

        public static ServiceLevelAgreement CreateShoppingCartSla()
        {
            ServiceLevelAgreement sla = new ServiceLevelAgreement("Shopping Cart");
            SubSLA subSla1 = new SubSLA(300, Consistency.Strong, 0, 1);
            SubSLA subSla2 = new SubSLA(300, Consistency.ReadMyWrites, 0, 0.75f);
            SubSLA subSla3 = new SubSLA(300, Consistency.Eventual, 0, 0.5f);
            SubSLA subSla4 = new SubSLA(800, Consistency.Eventual, 0, 0.25f);
            sla.Add(subSla1);
            sla.Add(subSla2);
            sla.Add(subSla3);
            sla.Add(subSla4);
            return sla;
        }

        public static ServiceLevelAgreement CreateFastOrStrongSla()
        {
            ServiceLevelAgreement sla = new ServiceLevelAgreement("Fast or Strong");
            SubSLA subSla1 = new SubSLA(150, Consistency.Strong, 0, 1);
            SubSLA subSla2 = new SubSLA(150, Consistency.Bounded, 1, 0.8f);
            SubSLA subSla3 = new SubSLA(150, Consistency.Eventual, 0, 0.5f);
            SubSLA subSla4 = new SubSLA(2000, Consistency.Strong, 0, 0.25f);
            sla.Add(subSla1);
            sla.Add(subSla2);
            sla.Add(subSla3);
            sla.Add(subSla4);
            return sla;
        }

        /// <summary>
        /// Creates a simple SLA with a single desired consistency and a large latency.
        /// This forces reads to be performed at the closest replica with that consistency.
        /// </summary>
        /// <param name="cons"></param>
        /// <returns></returns>
        public static ServiceLevelAgreement CreateConsistencySla(Consistency cons)
        {
            ServiceLevelAgreement sla = new ServiceLevelAgreement(cons.ToString("g"));
            SubSLA subSla1 = new SubSLA(2000, cons, 0, 1);
            sla.Add(subSla1);
            return sla;
        }

        public static ServiceLevelAgreement CreateConsistencySla(Consistency cons, int latency, string Name)
        {
            ServiceLevelAgreement sla = new ServiceLevelAgreement(Name);
            SubSLA subSla1 = new SubSLA(latency, cons, 0, 1);
            sla.Add(subSla1);
            return sla;
        }

        public static ServiceLevelAgreement CreateBoundedConsistencySla(int bound)
        {
            ServiceLevelAgreement sla = new ServiceLevelAgreement("Bounded");
            SubSLA subSla1 = new SubSLA(2000, Consistency.Bounded, bound, 1);
            sla.Add(subSla1);
            return sla;
        }

        public static ServiceLevelAgreement GetCurrentSLA()
        {
            return currentSLA;
        }

        public static void SetCurrentSLA(string name)
        {
            switch (name)
            {
                case "Fast or Strong":
                    currentSLA = CreateFastOrStrongSla();
                    slaEngines["sla"].Sla = currentSLA;
                    break;
                case "Shopping Cart":
                    currentSLA = CreateShoppingCartSla();
                    slaEngines["sla"].Sla = currentSLA;
                    break;
                default:
                    // do nothing
                    break;
            }
        }

        public static float GetCurrentSLAUtility()
        {
            return currentSLA.GetAverageDeliveredUtility();
        }

        #endregion


        #region Loging routines

        public delegate void Logger(string s);

        // private static Logger logger = null;
        private static Logger logger = null;

        // private static void Log(string s)
        public static void Log(string s)
        {
            if (logger != null)
            {
                logger(s);
            }
        }

        public static void RegisterLogger(Logger _logger)
        {
            logger = _logger;
        }

        public static string PrintCurrentConfiguration()
        {
            string result = null;
            ReplicaConfiguration config = ClientRegistry.GetConfiguration(containerName, false);
            result += "Current configuration for " + config.Name + ":" + "\r\n";
            result += "Primary: ";
            bool first = true;
            foreach (string name in config.PrimaryServers)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    result += ", ";
                }
                result += SiteName(name);
            };
            result += "\r\n";
            result += "Secondaries: ";
            first = true;
            foreach (string name in config.SecondaryServers)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    result += ", ";
                }
                result += SiteName(name);
            };
            result += "\r\n";
            return result;
        }

        public static string PrintReadWriteTimes(Sampler sampler)
        {
            string result = null;
            foreach (string cons in containers.Keys)
            {
                result += cons + " read: " + sampler.GetSampleValue(cons + "Latency") + "\r\n";
            }
            result += "write: " + sampler.GetSampleValue("WriteLatency");
            return result;
        }

        public static string PrintReconfigurationActions()
        {
            string result = null;
            if (proposedActions == null || proposedActions.Count == 0)
            {
                result = "No new configuration is suggested at this time.";
            }
            else
            {
                foreach (ConfigurationAction act in proposedActions)
                {
                    result += "Chosen action for " + act.ModifyingContainer.Name.ToString() + ": ";
                    result += act.GetType().Name.ToString() + " " + SiteName(act.ServerName) + "\r\n";
                    result += "expected utility gain is " + act.GainedUtility + "\r\n";
                    result += "expected cost is " + act.Cost + "\r\n";
                }
            }
            return result;
        }

        private static string SiteName(string server)
        {
            string site;
            switch (server)
            {
                case "dbteuropestorage": site = "West Europe"; break;
                case "dbtwestusstorage": site = "West US"; break;
                case "dbtsouthstorage": site = "South Central US"; break;
                case "dbteuropestorage-secondary": site = "North Europe"; break;
                case "dbtwestusstorage-secondary": site = "East US"; break;
                case "dbtsouthstorage-secondary": site = "North Central US"; break;
                default: site = server; break;
            }
            return site;
        }

        #endregion


        #region Miscellaneous

        public static int ActualNumberOfClients()
        {
            return 1;
        }

        #endregion
    }

    public partial class BlobTestBase : TestBase
    {
        public static string GetRandomContainerName()
        {
            return string.Concat("testc", Guid.NewGuid().ToString("N"));
        }

        public static CapCloudBlobContainer GetRandomContainerReference()
        {
            CapCloudBlobClient blobClient = GenerateCloudBlobClient();

            string name = GetRandomContainerName();
            return DemoLib.Initialize(name);
        }

        public static List<string> GetBlockIdList(int count)
        {
            List<string> blocks = new List<string>();
            for (int i = 0; i < count; i++)
            {
                blocks.Add(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
            }
            return blocks;
        }

        public static void AssertAreEqual(ICloudBlob expected, ICloudBlob actual)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
            }
            else
            {
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.BlobType, actual.BlobType);
                Assert.AreEqual(expected.Uri, actual.Uri);
                Assert.AreEqual(expected.StorageUri, actual.StorageUri);
                Assert.AreEqual(expected.SnapshotTime, actual.SnapshotTime);
                Assert.AreEqual(expected.IsSnapshot, actual.IsSnapshot);
                Assert.AreEqual(expected.SnapshotQualifiedUri, actual.SnapshotQualifiedUri);
                AssertAreEqual(expected.Properties, actual.Properties);
                AssertAreEqual(expected.CopyState, actual.CopyState);
            }
        }

        public static void AssertAreEqual(BlobProperties expected, BlobProperties actual)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
            }
            else
            {
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.CacheControl, actual.CacheControl);
                Assert.AreEqual(expected.ContentDisposition, actual.ContentDisposition);
                Assert.AreEqual(expected.ContentEncoding, actual.ContentEncoding);
                Assert.AreEqual(expected.ContentLanguage, actual.ContentLanguage);
                Assert.AreEqual(expected.ContentMD5, actual.ContentMD5);
                Assert.AreEqual(expected.ContentType, actual.ContentType);
                Assert.AreEqual(expected.ETag, actual.ETag);
                Assert.AreEqual(expected.LastModified, actual.LastModified);
                Assert.AreEqual(expected.Length, actual.Length);
            }
        }

        public static void AssertAreEqual(CopyState expected, CopyState actual)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
            }
            else
            {
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.BytesCopied, actual.BytesCopied);
                Assert.AreEqual(expected.CompletionTime, actual.CompletionTime);
                Assert.AreEqual(expected.CopyId, actual.CopyId);
                Assert.AreEqual(expected.Source, actual.Source);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.TotalBytes, actual.TotalBytes);
            }
        }
    }
}
