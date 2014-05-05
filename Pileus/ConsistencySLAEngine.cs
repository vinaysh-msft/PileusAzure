using Microsoft.WindowsAzure.Storage.Pileus.Configuration;
using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Storage.Pileus
{
    public delegate void ChosenUtility(float utility);

    public class ConsistencySLAEngine
    {
        /// <summary>
        /// The service level agreement used to select replicas from which to read
        /// </summary>
        public ServiceLevelAgreement Sla { get; set; }

        // The last subSLA that was chosen
        public SubSLA chosenSubSLA { get; private set; }
        
        // A callback method for determining the utility of the chosen subSLA
        // It is only used for registering the chosen utility during the evaluation.
        private ChosenUtility chosenUtility;

        public ReplicaConfiguration Config { get; set; }

        public SessionState Session { get; set; }

        public ServerMonitor Monitor { get; set; }

        private ServerSelector selector;

        // Last known epoch number for the replica configuration
        private int lastEpoch = 0;

        public ConsistencySLAEngine(ServiceLevelAgreement sla, ReplicaConfiguration config, SessionState sessionState = null, ServerMonitor monitor = null, ChosenUtility chosenUtility = null)
        {
            this.Sla = sla;
            this.Config = config;

            if (sessionState != null)
            {
                this.Session = sessionState;
            }
            else
            {
                this.Session = new SessionState();
            }

            if (monitor != null)
            {
                this.Monitor = monitor;
            }
            else
            {
                this.Monitor = new ServerMonitor(config);
            }
            
            this.chosenUtility = chosenUtility;
            this.selector = new ServerSelector(Session, Config, Monitor);
            
        }

        /// <summary>
        /// Selects the best server for reading the given blob with the current SLA, that is, 
        /// the server that maximizes the expected utility.
        /// </summary>
        /// <param name="blobName">Name of blob being read</param>
        /// <returns>A state object for the selected server</returns>
        public ServerState FindServerToRead(string blobName)
        {
            ServerState ss = null;
            float maxU = -1;
            SubSLA chosenSLA = null;
            
            // Select server that maximizes the expected utility
            foreach (SubSLA s in Sla)
            {
                ServerUtility su = ComputeUtilityForSubSla(blobName, s);
                if (su.Utility > maxU)
                {
                    chosenSLA = s;
                    maxU = su.Utility;
                    ss = su.Server;
                }
            }

            // Record chosen subSLA so the caller can get it if needed
            this.chosenSubSLA = chosenSLA;

            // Reset hits and misses if the configuration has changed
            if (lastEpoch != Config.Epoch)
            {
                Sla.ResetHitsAndMisses();
                lastEpoch = Config.Epoch;
            }

            // Compute number of hits and misses
            // Misses are only computed for subSLA entries before the chosenSLA (i.e., those that have more preference by the client).
            foreach (SubSLA s in Sla)
                if (s == chosenSLA)
                {
                    s.Hit();
                    break;
                }
                else
                {
                    s.Miss();
                }

            // Report chosen utility
            if (chosenUtility != null)
            {
                chosenUtility.Invoke(chosenSLA.Utility);
            }
            
            // Console.WriteLine("selected server " + ss.Name);
            return ss;
        }
        
        private ServerUtility ComputeUtilityForSubSla(string blobName, SubSLA subSla)
        {
            float maxProb = -1;
            ServerState ret = null;
            HashSet<ServerState> servers = selector.SelectServersForConsistency(blobName, subSla.Consistency, subSla.Bound);
            foreach (ServerState ss in servers)
            {
                float prob = ss.FindProbabilityOfRttLessThan((long)subSla.Latency);
                if (prob > maxProb)
                {
                    ret = ss;
                    maxProb = prob;
                }
                else if (prob == maxProb)
                {
                    // we have a tie, so pick the server with the lowest average latency
                    if (ss.AverageRTT < ret.AverageRTT)
                    {
                        ret = ss;
                    }
                }
            }

            //Debug.Assert(ret != null);
            return new ServerUtility(ret, subSla.Utility * maxProb);
        }

    }
}
