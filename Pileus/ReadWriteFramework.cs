using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Pileus.Configuration;
using Microsoft.WindowsAzure.Storage.Pileus.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Storage.Pileus
{

    public delegate void ReadOp(ICloudBlob blob);
    public delegate void WriteOp(ICloudBlob blob);
        
    /// <summary>
    /// Procedures for correctly performing read and write operations in various conditions and configurations.
    /// </summary>
    public class ReadWriteFramework
    {
        // The name of the bob that is being read and written
        private string blobName;

        // The name of the replicated container in which this blob is stored
        private string containerName;

        // The Consistency-based SLA engine that makes decisions on which blob to use
        private ConsistencySLAEngine slaEngine;

        // Stopwatch for timing roundtrip latencies
        private Stopwatch watch;

        // The configuration that specifies the primary and secondary replicas.
        private ReplicaConfiguration configuration;

        public ReadWriteFramework(string blobName, ReplicaConfiguration configuration, ConsistencySLAEngine engine)
        {
            this.blobName = blobName;
            this.containerName = configuration.Name;
            this.configuration = configuration;
            this.slaEngine = engine;
            this.watch = new Stopwatch();
        }

        #region Read operations

        public void Read(ReadOp op)
        {
            if (configuration.IsInFastMode())
            {
                FastRead(op);
            }
            else
            {
                SlowRead(op);
            }
        }

        /// <summary>
        /// Perform standard read and update server and session state.
        /// </summary>
        /// <param name="op">The read operation</param>
        /// <param name="blob">The blob being read</param>
        /// <param name="ss">The chosen server</param>
        private void FastRead(ReadOp op)
        {
            ServerState ss = slaEngine.FindServerToRead(blobName);
            ICloudBlob blob = ClientRegistry.GetCloudBlob(ss.Name, containerName, blobName);
            
            watch.Start();
            op(blob);
            watch.Stop();
            
            ss.AddRtt(watch.ElapsedMilliseconds);
            slaEngine.Session.RecordObjectRead(blob.Name, Timestamp(blob), ss, slaEngine.Sla.Id);
            // TODO: compute delivered utility
        }

        /// <summary>
        /// Perform read optimistically and then verify configuration.
        /// </summary>
        /// <param name="op">The read operation</param>
        /// <param name="blob">The blob being read</param>
        /// <param name="ss">The chosen server</param>
        /// <returns>whether the read succeeded; if not, then it should be tried again.</returns>
        private void SlowRead(ReadOp op)
        {
            // TODO: Deal with the case that we try to read from a replica that is no longer a replica and the read fails
            // In this case, the read should be retried after refreshing the configuration.
            ServerState ss = null;
            try
            {
                bool isDone = false;
                do  // until we enter fast mode or succeed at reading in slow mode with the correct configuration
                {
                    ss = slaEngine.FindServerToRead(blobName);
                    ICloudBlob blob = ClientRegistry.GetCloudBlob(ss.Name, containerName, blobName);

                    // TODO: this should really check if the reader wants strong consistency
                    // It could be that eventual consistency is fine but the SLA Engine just happened to choose a primary server
                    // In this case, we can do a fast mode read since we don't care if the chosen server is no longer a primary
                    if (configuration.IsInFastMode() || !configuration.PrimaryServers.Contains(ss.Name))
                    {
                        // it is fine to read from the selected secondary replica 
                        // or from a primary replica if we have now entered fast mode
                        FastRead(op);
                        isDone = true;
                    }
                    else
                    {
                        // read from the selected replica that we believe to be a primary replica 
                        watch.Start();
                        op(blob);
                        watch.Stop();

                        // then see if the configuration has changed and the selected replica is no longer primary
                        configuration.SyncWithCloud(ClientRegistry.GetConfigurationAccount());
                        
                        // TODO: check the epoch number on the configuration to make sure that we have not missed a configuration.
                        // i.e. we need to make sure that the server from which we read did not become a secondary during our read,
                        // and then back to primary when we checked.
                        // TODO: maybe we should check that the read delivered a non-zero utility.
                        // It is possible that the configuration was so stale that a much better server could have been chosen.
                        if (configuration.PrimaryServers.Contains(ss.Name))
                        {
                            //We have contacted the primary replica, hence we are good to go.
                            ss.AddRtt(watch.ElapsedMilliseconds);
                            slaEngine.Session.RecordObjectRead(blob.Name, Timestamp(blob), ss, slaEngine.Sla.Id);
                            isDone = true;
                        }
                        isDone = false;  // not done
                    }
                } while (!isDone);
            }
            // TODO: decide if we need to catch any exceptions here or just let then propagate through
            catch (StorageException se)
            {
                if (StorageExceptionCode.NotFound(se))
                {
                    //blob is not found.
                    //this is fine because the replica might have removed.
                    //it simply returns, so client need to re-execute if this happens. 
                    //We can also re-execute the read here, but for debugging purposes, it's simpler for the client to re-execute. 
                    //Of course in real system, we need to re-execute at this level.
                    return;
                }
                // storage exceptions are passed through to the caller
                throw;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Object reference not set to an instance of an object"))
                    return;
                throw ex;
            }
        }

        #endregion

        #region Write operations

        public void Write(WriteOp op)
        {
            // TODO: ensure that there is enough time left on the lease to complete the write
            if (configuration.IsInFastMode())
            {
                if (configuration.PrimaryServers.Count == 1)
                {
                    FastWrite(op);
                }
                else
                {
                    MultiWrite(op);
                }
            }
            else
            {
                SlowWrite(op);
            }
        }

        private void FastWrite(WriteOp op)
        {
            // there should only be one primary server since we are in fast mode
            string server = configuration.PrimaryServers.First();
            ICloudBlob blob = ClientRegistry.GetCloudBlob(server, configuration.Name, blobName, false);
            
            watch.Start();
            op(blob);
            watch.Stop();

            if (slaEngine.Monitor.replicas.ContainsKey(server))
            {
                ServerState ss = slaEngine.Monitor.GetServerState(server);
                ss.AddRtt(watch.ElapsedMilliseconds);
                slaEngine.Session.RecordObjectWritten(blobName, Timestamp(blob), ss);
            }
        }

        private void SlowWrite(WriteOp op)
        {
            // A reconfiguration is in progress, but may not have happened yet.
            // We could try to complete this write before the reconfiguration, but that would require locking
            // to prevent the configuration service from changing the configuration during this write.
            // Instead, we take the simple approach of waiting for the reconfiguration to complete.
            
            bool isDone = false;
            do  // while unable to lease the configuration
            {
                if (configuration.IsInFastMode(renew: true))
                    {
                        if (configuration.PrimaryServers.Count == 1)
                        {
                            FastWrite(op);
                        }
                        else
                        {
                            MultiWrite(op);
                        }
                        isDone = true;
                    }
                    else
                    {
                        Thread.Sleep(ConstPool.CONFIGURATION_ACTION_DURATION);
                    }
            }
            while (!isDone);
        }

        private void MultiWrite(WriteOp op)
        {
            // TODO: recover from failed clients that may leave a write partially completed
            // TODO: remove use of leases and replace with ETags
            // throw new Exception("Write to multiple primaries not yet implemented.");
            try
            {
                bool done = false;
                while (!done)
                {
                    // grab lease on blob to guard against concurrent writers
                    using (PrimaryCloudBlobLease lease = new PrimaryCloudBlobLease(blobName, configuration, true))
                    {
                        if (lease.HasLease)
                        {
                            // TODO: fix code for writing to multiple primaries (and remove it from here)
                            foreach (string server in configuration.PrimaryServers)
                            {
                                watch.Start();
                                ICloudBlob blob = ClientRegistry.GetCloudBlob(server, configuration.Name, blobName, false);
                                op(blob);
                                watch.Stop();

                                if (slaEngine.Monitor.replicas.ContainsKey(server))
                                {
                                    ServerState ss = slaEngine.Monitor.GetServerState(server);
                                    ss.AddRtt(watch.ElapsedMilliseconds);
                                    slaEngine.Session.RecordObjectWritten(blobName, Timestamp(blob), ss);
                                }
                            }
                            done = true;
                        }
                    }
                }
            }
            catch (StorageException se)
            {
                throw se;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Miscellaneous

        private DateTimeOffset Timestamp(ICloudBlob blob)
        {
            // TODO: ensure that this works in the case where the primary server changes
            // get timestamp from metadata if present since primary and secondary will have different LastModified times for same version of blob
            DateTimeOffset result;
            if (blob.Metadata.ContainsKey("LastModifiedOnPrimary"))
            {
                result = DateTimeOffset.Parse(blob.Metadata["LastModifiedOnPrimary"]);
            }
            else
            {
                result = blob.Properties.LastModified ?? DateTimeOffset.MinValue;
            }
            return result;
        }

        // TODO: Virtualize ETags similar to timestamps.
        // Problem: Clients may read from one server and receive an ETag, and then perform a conditional read/write using that ETag.
        // This certainly will not work if the ETag was received from a different server than where it is used.
        // Solution: Either use the main primary's ETag for all replicas (ala timestamps) or just use timestamps as ETags.
        // This means that we need to return the main ETag when a client checks the blob's properties.
        // And when a read/write is performed using an ETag, we need a two step protocol:
        // 1. Read from the selected server and see if it's virtual ETag matches the desired ETag.
        // 2. If match, then perform read/write using the read ETag returned in step 1.
        // Note that there may be a way to optimize this when the client only uses ETags that are received from the main primary.

        #endregion
    }
}
