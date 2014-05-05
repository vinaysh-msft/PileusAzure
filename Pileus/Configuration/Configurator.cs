using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Pileus.Utils;
using Microsoft.WindowsAzure.Storage.Pileus.Configuration.Actions;
using Microsoft.WindowsAzure.Storage.Pileus.Configuration.Constraint;

namespace Microsoft.WindowsAzure.Storage.Pileus.Configuration
{
    public class Configurator
    {

        public static string Logs = "";

        private string containerName { get; set; }

        public Configurator(string containerName)
        {
            this.containerName=containerName;
        }

        public void Configure(CloudStorageAccount account, int currentConfigurationEpoch, ReplicaConfiguration currentConfig, List<ConfigurationConstraint> constraints)
        {
            List<ConfigurationAction> pickedAction = PickNewConfiguration(account, currentConfigurationEpoch, currentConfig, constraints);

            InstallNewConfiguration(pickedAction);
        }

        /// <summary>
        /// Picks a new configuration given a single SLA and single client's session state
        /// </summary>
        /// <param name="sla">The SLA for which the new configuration should be tailored</param>
        /// <param name="ss">The session state</param>
        /// <param name="constraints">The constraints</param>
        /// <returns>a set of reconfiguration actions</returns>
        public List<ConfigurationAction> PickNewConfiguration(string containerName, ServiceLevelAgreement sla, SessionState ss, ServerMonitor monitor, ReplicaConfiguration config, List<ConfigurationConstraint> constraints)
        {
            Dictionary<string, ClientUsageData> clientData = new Dictionary<string,ClientUsageData>();
            ClientUsageData usage = new ClientUsageData("local");

            // Convert args into client data
            usage.SLAs.Add(sla);
            usage.NumberOfReads = ss.GetNumberOfReadsPerMonth();
            usage.NumberOfWrites = ss.GetNumberOfWritesPerMonth();
            usage.ServerRTTs = new Dictionary<string, LatencyDistribution>();
            foreach (ServerState server in monitor.replicas.Values)
            {
                usage.ServerRTTs.Add(server.Name, server.RTTs);
            }

            // Use client data for a single user
            clientData.Add(usage.ClientName, usage);

            // Choose actions to produce a better configuration
            return ChooseReconfigActions(clientData, constraints, config);
        }

        public List<ConfigurationAction> PickNewConfiguration(CloudStorageAccount account, int currentConfigurationEpoch, ReplicaConfiguration config, List<ConfigurationConstraint> constraints)
        {
            // Read client's data
            Dictionary<string, ClientUsageData> clientData;
            ClientUsageCloudStore store = new ClientUsageCloudStore(account, "configurator");
            clientData = store.ReadClientData(containerName, currentConfigurationEpoch);

            // Choose actions to produce a better configuration
            return ChooseReconfigActions(clientData, constraints, config);
        }

        public List<ConfigurationAction> SetConfigurationPrimary(string containerName, string primary)
        {
            List<ConfigurationAction> result = new List<ConfigurationAction>();
            result.Add(ActionSelector.GetMakeSoloPrimaryAction(containerName, primary));
            return result;
        }

        private List<ConfigurationAction> ChooseReconfigActions(Dictionary<string, ClientUsageData> clientData, List<ConfigurationConstraint> constraints, ReplicaConfiguration config)
        {
             Dictionary<string, ConfigurationAction> actions = new Dictionary<string, ConfigurationAction>();
            
            // build sorted list of all SLAs
            SortedSet<ServiceLevelAgreement> SLAs = new SortedSet<ServiceLevelAgreement>(new ServiceLevelAgreementComparer());
            foreach (ClientUsageData client in clientData.Values)
            {
                SLAs.UnionWith(client.SLAs);
            }

            AppendToLogger("Start Computing Actions");
            foreach (string clientName in clientData.Keys)
            {
                foreach (ServiceLevelAgreement sla in clientData[clientName].SLAs)
                {

                    Dictionary<string, ConfigurationAction> tmp = ActionSelector.ComputeActions(this.containerName, config, sla, clientData[clientName]);
                    foreach (string id in tmp.Keys)
                    {
                        if (!actions.ContainsKey(id))
                        {
                            actions[id] = tmp[id];
                        }
                        else
                        {
                            actions[id].Merge(tmp[id]);
                        }

                        //We keep track of clients. We will use this list in the checking phase (below)
                        actions[id].Clients.Add(clientName);

                        AppendToLogger(tmp[id].ToString());
                    }
                }
            }
            
            AppendToLogger("Start Checking Actions");
            foreach (string clientName in clientData.Keys)
            {
                foreach (ServiceLevelAgreement sla in clientData[clientName].SLAs)
                {
                    foreach (ConfigurationAction action in actions.Values)
                    {
                        if (!action.OriginatingSLAs.Contains(sla.Id) || !action.Clients.Contains(clientName))
                        {
                            ActionSelector.CheckAction(action, sla, clientData[clientName], config);
                        }

                    }
                }
            }

            foreach (ConfigurationAction action in actions.Values)
            {
                AppendToLogger(action.ToString());
            }

            List<ConfigurationAction> pickedAction = new List<ConfigurationAction>();
            List<ConfigurationAction> sortedActions = actions.Values.ToList();

            //we do not even consider actions with negative gained utility. 
            sortedActions.RemoveAll(e => e.GainedUtility < 0);
            sortedActions.Sort(new ConfigurationActionComparer());

            // we sort constraints based on their priority.
            constraints.Sort(new ConfigurationConstraintComparer());

            bool foundAction = false;
            for (int i = sortedActions.Count - 1; i >= 0; i--)
            {
                pickedAction.Clear();
                pickedAction.Add(sortedActions[i]);

                if (constraints.Count() == 0)
                {
                    foundAction = true;
                    break;
                }

                //Remove actions that are not satisfying constraints. 
                foreach (ConfigurationConstraint c in constraints)
                {
                    c.Apply(pickedAction, constraints, SLAs, clientData);
                    if (pickedAction.All(e => e.Source == ConfigurationActionSource.Constraint))
                    {
                        //the action is removed because of conflict with other constraints. 
                        foundAction = false;
                        break;
                    }
                    else
                    {
                        foundAction = true;
                    }
                }

                if (foundAction)
                {
                    break;
                }
                
            }

            if (!foundAction)
            {
                //no action is applicable with all constraints. so, we just try to apply constraints. 
                //hence, no non-constraint action will be executed to improve utility. 
                pickedAction.Clear();
                foreach (ConfigurationConstraint c in constraints)
                {
                    c.Apply(pickedAction, constraints, SLAs, clientData);
                }
            }

            return pickedAction;
        }

        public void InstallNewConfiguration(List<ConfigurationAction> pickedAction)
        {
            // execute all actions in pickedAction
            foreach (ConfigurationAction action in pickedAction)
            {
                AppendToLogger("Chosen Action : " + action.ToString());
                action.Execute();
                Console.WriteLine("Action "  + action.Id + " finished.");
                Logs += action.Logs;
            }
        }

        protected void AppendToLogger(string log)
        {
            long curSecond = DateTime.Now.ToUniversalTime().Ticks / 10000000;
            Logs += curSecond + "," + log + "\n";
        }
    }
}
