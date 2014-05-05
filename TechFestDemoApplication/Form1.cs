using Microsoft.WindowsAzure.Storage.Pileus;
using PileusApp.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TechFestDemo;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;


namespace TechFestDemoApplication
{
    public partial class Form1 : Form
    {
        bool enableHitRate = false;

        Sampler initialSampler = null;
        Sampler reconfigSampler = null;
        bool initialConfig = true;

        List<string> consistencyChoices;
        List<string> readLatency;
        List<string> readAgainLatency;


        public Form1()
        {
            InitializeComponent();

            consistencyChoices = new List<string> { 
                "strong consistency", 
                "causal consistency", 
                "bounded staleness", 
                "read my writes", 
                "monotonic freshness", 
                "eventual consistency" 
            };
            consistencyListBox.DataSource = consistencyChoices;
            consistencyListBox.ClearSelected();

            Print("Running demo program...");
            DemoLib.RegisterLogger(Print);

            //
            // Create initial configuration
            //
            Print("Reading configuration...");
            DemoLib.Initialize();
            Print(DemoLib.PrintCurrentConfiguration());

            // ping servers to measure and record round-trip times in the server state
            Print("Pinging servers...");
            DemoLib.PingAllServers();
        }

        private void Print(string s)
        {
            logTextBox.AppendText(s);
            logTextBox.AppendText("\r\n");
        }


        private void ReadButton_Click(object sender, EventArgs e)
        {
            Print("Reading and writing blobs...");
            Sampler sampler = initialConfig ? initialSampler : reconfigSampler;
            sampler = DemoLib.PerformReadsWritesSyncs(sampler);
            if (initialConfig)
                initialSampler = sampler;
            else
                reconfigSampler = sampler;
            
            Print("Read and write average latencies:");
            Print(DemoLib.PrintReadWriteTimes(sampler));

            readLatency = new List<string>();
            readLatency.Add(sampler.GetSampleValue("strongLatency").ToString());
            readLatency.Add(sampler.GetSampleValue("causalLatency").ToString());
            readLatency.Add(sampler.GetSampleValue("boundedLatency").ToString());
            readLatency.Add(sampler.GetSampleValue("readmywritesLatency").ToString());
            readLatency.Add(sampler.GetSampleValue("monotonicLatency").ToString());
            readLatency.Add(sampler.GetSampleValue("eventualLatency").ToString());
            readLatencyListBox.DataSource = null;
            readLatencyListBox.DataSource = readLatency;
            readLatencyListBox.ClearSelected();
        }

        private void ReadAgainButton_Click(object sender, EventArgs e)
        {
            Print("Reading and writing blobs...");
            Sampler sampler = initialConfig ? initialSampler : reconfigSampler;
            sampler = DemoLib.PerformReadsWritesSyncs(sampler);
            if (initialConfig)
                initialSampler = sampler;
            else
                reconfigSampler = sampler;
            
            Print("Read and write average latencies:");
            Print(DemoLib.PrintReadWriteTimes(sampler));

            readAgainLatency = new List<string>();
            readAgainLatency.Add(sampler.GetSampleValue("strongLatency").ToString());
            readAgainLatency.Add(sampler.GetSampleValue("causalLatency").ToString());
            readAgainLatency.Add(sampler.GetSampleValue("boundedLatency").ToString());
            readAgainLatency.Add(sampler.GetSampleValue("readmywritesLatency").ToString());
            readAgainLatency.Add(sampler.GetSampleValue("monotonicLatency").ToString());
            readAgainLatency.Add(sampler.GetSampleValue("eventualLatency").ToString());
            readAgainLatencyListBox.DataSource = null;
            readAgainLatencyListBox.DataSource = readAgainLatency;
            readAgainLatencyListBox.ClearSelected();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            readLatencyListBox.DataSource = null;
            readAgainLatencyListBox.DataSource = null;
            consistencyListBox.ClearSelected();
        }

        private void getConfigButton_Click(object sender, EventArgs e)
        {
            configTextBox.Clear();
            configTextBox.AppendText(DemoLib.PrintCurrentConfiguration() + "\n");
        }

        private void restroreConfigButton_Click(object sender, EventArgs e)
        {
            configTextBox.Clear();
            configTextBox.AppendText("Restoring initial configuration...");
            DemoLib.SetInitialConfiguration();
            initialConfig = true;
            configTextBox.Clear();
            configTextBox.AppendText(DemoLib.PrintCurrentConfiguration() + "\n");
        }

        private void proposeNewConfigButton_Click(object sender, EventArgs e)
        {
            configTextBox.Clear();
            configTextBox.AppendText("Proposing new configuration...");
            DemoLib.ProposeNewConfiguration();
            configTextBox.Clear();
            configTextBox.AppendText(DemoLib.PrintReconfigurationActions() + "\n");
        }

        private void installNewConfigButton_Click(object sender, EventArgs e)
        {
            configTextBox.Clear();
            configTextBox.AppendText("Installing new configuration...");
            DemoLib.InstallNewConfiguration();
            initialConfig = false;
            configTextBox.Clear();
            configTextBox.AppendText(DemoLib.PrintCurrentConfiguration() + "\n");
        }

        private void installSLA2Button_Click(object sender, EventArgs e)
        {
            DemoLib.SetCurrentSLA("Shopping Cart");
            ServiceLevelAgreement sla = DemoLib.GetCurrentSLA();
            DisplaySLA(sla);
        }

        private void installSLA1Button_Click(object sender, EventArgs e)
        {
            DemoLib.SetCurrentSLA("Fast or Strong");
            ServiceLevelAgreement sla = DemoLib.GetCurrentSLA();
            DisplaySLA(sla);
        }

        private void getCurrentSLAButton_Click(object sender, EventArgs e)
        {
            ServiceLevelAgreement sla = DemoLib.GetCurrentSLA();
            DisplaySLA(sla);
        }

        private void DisplaySLA(ServiceLevelAgreement sla) 
        {
            List<string> slaConsistency = new List<string>();
            List<string> slaLatency = new List<string>();
            List<string> slaUtility = new List<string>();
            foreach (SubSLA sub in sla)
            {
                slaConsistency.Add(sub.Consistency.ToString());
                slaLatency.Add(sub.Latency.ToString() + " ms.");
                slaUtility.Add(sub.Utility.ToString());
            }
            slaConsistencyListBox.DataSource = null;
            slaConsistencyListBox.DataSource = slaConsistency;
            slaConsistencyListBox.ClearSelected();
            slaLatencyListBox.DataSource = null;
            slaLatencyListBox.DataSource = slaLatency;
            slaLatencyListBox.ClearSelected();
            slaUtilityListBox.DataSource = null;
            slaUtilityListBox.DataSource = slaUtility;
            slaUtilityListBox.ClearSelected();
            
            Sampler sampler = initialConfig ? initialSampler : reconfigSampler;
            slaDeliveredUtility.Text = DemoLib.GetCurrentSLAUtility().ToString();
            slaReadTime.Text = sampler.GetSampleValue("slaLatency").ToString();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Sampler sampler = initialConfig ? initialSampler : reconfigSampler;
            sampler = DemoLib.PerformReadsWritesSyncs(sampler);
            if (initialConfig)
                initialSampler = sampler;
            else
                reconfigSampler = sampler;

            this.chart1.Series["Latency"].Points.Clear();
            this.chart1.Series["Primary hit rate"].Points.Clear(); 
            this.chart1.Legends["Latency"].Enabled = true;

            if (enableHitRate)
            {
                this.chart1.Series["Primary hit rate"].IsVisibleInLegend = true;
            }
            else
            {
                this.chart1.Series["Primary hit rate"].IsVisibleInLegend = false;
            }

            this.chart1.Series["Latency"].Points.AddXY("Strong", sampler.GetSampleValue("strongLatency"));
            if (enableHitRate)
            {
                float primaryHitRate = (sampler.GetSampleValue("strongPrimaryAccesses") * 100) / sampler.GetSampleValue("strongTotalAccesses");
                this.chart1.Series["Primary hit rate"].Points.AddXY("Strong", primaryHitRate);
            }

            this.chart1.Series["Latency"].Points.AddXY("Causal", sampler.GetSampleValue("causalLatency"));
            if (enableHitRate)
            {
                float primaryHitRate = (sampler.GetSampleValue("causalPrimaryAccesses") * 100) / sampler.GetSampleValue("causalTotalAccesses");
                this.chart1.Series["Primary hit rate"].Points.AddXY("Causal", primaryHitRate);
            }

            this.chart1.Series["Latency"].Points.AddXY("Bounded", sampler.GetSampleValue("boundedLatency"));
            if (enableHitRate)
            {
                float primaryHitRate = (sampler.GetSampleValue("boundedPrimaryAccesses") * 100) / sampler.GetSampleValue("boundedTotalAccesses");
                this.chart1.Series["Primary hit rate"].Points.AddXY("Bounded", primaryHitRate);
            }

            this.chart1.Series["Latency"].Points.AddXY("Read my writes", sampler.GetSampleValue("readmywritesLatency"));
            if (enableHitRate)
            {
                float primaryHitRate = (sampler.GetSampleValue("readmywritesPrimaryAccesses") * 100) / sampler.GetSampleValue("readmywritesTotalAccesses");
                this.chart1.Series["Primary hit rate"].Points.AddXY("Read my writes", primaryHitRate);
            }

            this.chart1.Series["Latency"].Points.AddXY("Monotonic", sampler.GetSampleValue("monotonicLatency"));
            if (enableHitRate)
            {
                float primaryHitRate = (sampler.GetSampleValue("monotonicPrimaryAccesses") * 100) / sampler.GetSampleValue("monotonicTotalAccesses");
                this.chart1.Series["Primary hit rate"].Points.AddXY("Monotonic", primaryHitRate);
            }

            this.chart1.Series["Latency"].Points.AddXY("Eventual", sampler.GetSampleValue("eventualLatency"));
            if (enableHitRate)
            {
                float primaryHitRate = (sampler.GetSampleValue("eventualPrimaryAccesses") * 100) / sampler.GetSampleValue("eventualTotalAccesses");
                this.chart1.Series["Primary hit rate"].Points.AddXY("Eventual", primaryHitRate);
            }
        }

        public void DrawChart()
        {
            while (true)
            {
                this.Invoke(new Action(() => { button1.PerformClick(); }));
                Thread.Sleep(100);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
           Thread workedThread = new Thread(DrawChart);
            workedThread.Start();
            button2.Enabled = false;
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            if (this.enableHitRate)
            {
                enableHitRate = false;
            }
            else
            {
                enableHitRate = true;
            }
        }
        
    }
}
