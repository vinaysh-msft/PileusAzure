namespace TechFestDemoApplication
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.logTabPage = new System.Windows.Forms.TabPage();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.configTabPage = new System.Windows.Forms.TabPage();
            this.configTextBox = new System.Windows.Forms.TextBox();
            this.restroreConfigButton = new System.Windows.Forms.Button();
            this.installNewConfigButton = new System.Windows.Forms.Button();
            this.proposeNewConfigButton = new System.Windows.Forms.Button();
            this.getConfigButton = new System.Windows.Forms.Button();
            this.slaTabPage = new System.Windows.Forms.TabPage();
            this.slaUtilityListBox = new System.Windows.Forms.ListBox();
            this.slaLatencyListBox = new System.Windows.Forms.ListBox();
            this.slaConsistencyListBox = new System.Windows.Forms.ListBox();
            this.slaUtilityLabel = new System.Windows.Forms.Label();
            this.slaLatencyLabel = new System.Windows.Forms.Label();
            this.slaConsistencylabel = new System.Windows.Forms.Label();
            this.installSLA2Button = new System.Windows.Forms.Button();
            this.installSLA1Button = new System.Windows.Forms.Button();
            this.getSLAButton = new System.Windows.Forms.Button();
            this.consistencyTabPage = new System.Windows.Forms.TabPage();
            this.readtimelabel = new System.Windows.Forms.Label();
            this.readconsistencylabel = new System.Windows.Forms.Label();
            this.readAgainLatencyListBox = new System.Windows.Forms.ListBox();
            this.readLatencyListBox = new System.Windows.Forms.ListBox();
            this.consistencyListBox = new System.Windows.Forms.ListBox();
            this.clearButton = new System.Windows.Forms.Button();
            this.readAgainButton = new System.Windows.Forms.Button();
            this.readButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.label1 = new System.Windows.Forms.Label();
            this.slaDeliveredUtility = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.slaReadTime = new System.Windows.Forms.Label();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.logTabPage.SuspendLayout();
            this.configTabPage.SuspendLayout();
            this.slaTabPage.SuspendLayout();
            this.consistencyTabPage.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button2);
            this.tabPage1.Controls.Add(this.button1);
            this.tabPage1.Controls.Add(this.chart1);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1004, 574);
            this.tabPage1.TabIndex = 4;
            this.tabPage1.Text = "Chart";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(840, 116);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(135, 31);
            this.button2.TabIndex = 2;
            this.button2.Text = "Refresh";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(840, 65);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(135, 32);
            this.button1.TabIndex = 1;
            this.button1.Text = "Load chart";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // chart1
            // 
            chartArea1.AxisX.IsStartedFromZero = false;
            chartArea1.AxisX.Title = "Consistency";
            chartArea1.AxisX.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            chartArea1.AxisY.Title = "Latency";
            chartArea1.AxisY.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            chartArea1.AxisY2.MajorGrid.Enabled = false;
            chartArea1.AxisY2.TextOrientation = System.Windows.Forms.DataVisualization.Charting.TextOrientation.Rotated270;
            chartArea1.AxisY2.Title = "Primary hit rate (%)";
            chartArea1.AxisY2.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Enabled = false;
            legend1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            legend1.IsDockedInsideChartArea = false;
            legend1.IsTextAutoFit = false;
            legend1.Name = "Latency";
            legend1.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(38, 6);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Latency";
            series1.Name = "Latency";
            series1.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.String;
            series2.ChartArea = "ChartArea1";
            series2.IsVisibleInLegend = false;
            series2.Legend = "Latency";
            series2.Name = "Primary hit rate";
            series2.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.String;
            series2.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Size = new System.Drawing.Size(786, 568);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            title1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            title1.Name = "Title1";
            title1.Text = "Consistency - Latency Tradeoff";
            this.chart1.Titles.Add(title1);
            this.chart1.Click += new System.EventHandler(this.chart1_Click);
            // 
            // logTabPage
            // 
            this.logTabPage.Controls.Add(this.logTextBox);
            this.logTabPage.Location = new System.Drawing.Point(4, 29);
            this.logTabPage.Name = "logTabPage";
            this.logTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.logTabPage.Size = new System.Drawing.Size(1004, 574);
            this.logTabPage.TabIndex = 3;
            this.logTabPage.Text = "Log";
            this.logTabPage.UseVisualStyleBackColor = true;
            // 
            // logTextBox
            // 
            this.logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logTextBox.Location = new System.Drawing.Point(3, 3);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextBox.Size = new System.Drawing.Size(998, 568);
            this.logTextBox.TabIndex = 0;
            // 
            // configTabPage
            // 
            this.configTabPage.Controls.Add(this.configTextBox);
            this.configTabPage.Controls.Add(this.restroreConfigButton);
            this.configTabPage.Controls.Add(this.installNewConfigButton);
            this.configTabPage.Controls.Add(this.proposeNewConfigButton);
            this.configTabPage.Controls.Add(this.getConfigButton);
            this.configTabPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.configTabPage.Location = new System.Drawing.Point(4, 29);
            this.configTabPage.Name = "configTabPage";
            this.configTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.configTabPage.Size = new System.Drawing.Size(1004, 574);
            this.configTabPage.TabIndex = 2;
            this.configTabPage.Text = "Configuration";
            this.configTabPage.UseVisualStyleBackColor = true;
            // 
            // configTextBox
            // 
            this.configTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.configTextBox.Location = new System.Drawing.Point(55, 96);
            this.configTextBox.Multiline = true;
            this.configTextBox.Name = "configTextBox";
            this.configTextBox.Size = new System.Drawing.Size(526, 284);
            this.configTextBox.TabIndex = 4;
            // 
            // restroreConfigButton
            // 
            this.restroreConfigButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.restroreConfigButton.Location = new System.Drawing.Point(541, 25);
            this.restroreConfigButton.Name = "restroreConfigButton";
            this.restroreConfigButton.Size = new System.Drawing.Size(124, 42);
            this.restroreConfigButton.TabIndex = 3;
            this.restroreConfigButton.Text = "Restore";
            this.restroreConfigButton.UseVisualStyleBackColor = true;
            this.restroreConfigButton.Click += new System.EventHandler(this.restroreConfigButton_Click);
            // 
            // installNewConfigButton
            // 
            this.installNewConfigButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.installNewConfigButton.Location = new System.Drawing.Point(395, 25);
            this.installNewConfigButton.Name = "installNewConfigButton";
            this.installNewConfigButton.Size = new System.Drawing.Size(128, 42);
            this.installNewConfigButton.TabIndex = 2;
            this.installNewConfigButton.Text = "Install New";
            this.installNewConfigButton.UseVisualStyleBackColor = true;
            this.installNewConfigButton.Click += new System.EventHandler(this.installNewConfigButton_Click);
            // 
            // proposeNewConfigButton
            // 
            this.proposeNewConfigButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.proposeNewConfigButton.Location = new System.Drawing.Point(212, 25);
            this.proposeNewConfigButton.Name = "proposeNewConfigButton";
            this.proposeNewConfigButton.Size = new System.Drawing.Size(168, 43);
            this.proposeNewConfigButton.TabIndex = 1;
            this.proposeNewConfigButton.Text = "Propose New";
            this.proposeNewConfigButton.UseVisualStyleBackColor = true;
            this.proposeNewConfigButton.Click += new System.EventHandler(this.proposeNewConfigButton_Click);
            // 
            // getConfigButton
            // 
            this.getConfigButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.getConfigButton.Location = new System.Drawing.Point(53, 25);
            this.getConfigButton.Name = "getConfigButton";
            this.getConfigButton.Size = new System.Drawing.Size(143, 43);
            this.getConfigButton.TabIndex = 0;
            this.getConfigButton.Text = "Get Current";
            this.getConfigButton.UseVisualStyleBackColor = true;
            this.getConfigButton.Click += new System.EventHandler(this.getConfigButton_Click);
            // 
            // slaTabPage
            // 
            this.slaTabPage.Controls.Add(this.slaReadTime);
            this.slaTabPage.Controls.Add(this.label3);
            this.slaTabPage.Controls.Add(this.slaDeliveredUtility);
            this.slaTabPage.Controls.Add(this.label1);
            this.slaTabPage.Controls.Add(this.slaUtilityListBox);
            this.slaTabPage.Controls.Add(this.slaLatencyListBox);
            this.slaTabPage.Controls.Add(this.slaConsistencyListBox);
            this.slaTabPage.Controls.Add(this.slaUtilityLabel);
            this.slaTabPage.Controls.Add(this.slaLatencyLabel);
            this.slaTabPage.Controls.Add(this.slaConsistencylabel);
            this.slaTabPage.Controls.Add(this.installSLA2Button);
            this.slaTabPage.Controls.Add(this.installSLA1Button);
            this.slaTabPage.Controls.Add(this.getSLAButton);
            this.slaTabPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slaTabPage.Location = new System.Drawing.Point(4, 29);
            this.slaTabPage.Name = "slaTabPage";
            this.slaTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.slaTabPage.Size = new System.Drawing.Size(1004, 574);
            this.slaTabPage.TabIndex = 1;
            this.slaTabPage.Text = "Service Level Agreement";
            this.slaTabPage.UseVisualStyleBackColor = true;
            // 
            // slaUtilityListBox
            // 
            this.slaUtilityListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.slaUtilityListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slaUtilityListBox.FormattingEnabled = true;
            this.slaUtilityListBox.ItemHeight = 31;
            this.slaUtilityListBox.Location = new System.Drawing.Point(508, 146);
            this.slaUtilityListBox.Name = "slaUtilityListBox";
            this.slaUtilityListBox.Size = new System.Drawing.Size(167, 217);
            this.slaUtilityListBox.TabIndex = 8;
            // 
            // slaLatencyListBox
            // 
            this.slaLatencyListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.slaLatencyListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slaLatencyListBox.FormattingEnabled = true;
            this.slaLatencyListBox.ItemHeight = 31;
            this.slaLatencyListBox.Location = new System.Drawing.Point(351, 146);
            this.slaLatencyListBox.Name = "slaLatencyListBox";
            this.slaLatencyListBox.Size = new System.Drawing.Size(143, 217);
            this.slaLatencyListBox.TabIndex = 7;
            // 
            // slaConsistencyListBox
            // 
            this.slaConsistencyListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.slaConsistencyListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slaConsistencyListBox.FormattingEnabled = true;
            this.slaConsistencyListBox.ItemHeight = 31;
            this.slaConsistencyListBox.Location = new System.Drawing.Point(134, 146);
            this.slaConsistencyListBox.Name = "slaConsistencyListBox";
            this.slaConsistencyListBox.Size = new System.Drawing.Size(194, 217);
            this.slaConsistencyListBox.TabIndex = 6;
            // 
            // slaUtilityLabel
            // 
            this.slaUtilityLabel.AutoSize = true;
            this.slaUtilityLabel.Location = new System.Drawing.Point(502, 104);
            this.slaUtilityLabel.Name = "slaUtilityLabel";
            this.slaUtilityLabel.Size = new System.Drawing.Size(77, 31);
            this.slaUtilityLabel.TabIndex = 5;
            this.slaUtilityLabel.Text = "utility";
            // 
            // slaLatencyLabel
            // 
            this.slaLatencyLabel.AutoSize = true;
            this.slaLatencyLabel.Location = new System.Drawing.Point(345, 104);
            this.slaLatencyLabel.Name = "slaLatencyLabel";
            this.slaLatencyLabel.Size = new System.Drawing.Size(101, 31);
            this.slaLatencyLabel.TabIndex = 4;
            this.slaLatencyLabel.Text = "latency";
            // 
            // slaConsistencylabel
            // 
            this.slaConsistencylabel.AutoSize = true;
            this.slaConsistencylabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slaConsistencylabel.Location = new System.Drawing.Point(128, 104);
            this.slaConsistencylabel.Name = "slaConsistencylabel";
            this.slaConsistencylabel.Size = new System.Drawing.Size(158, 31);
            this.slaConsistencylabel.TabIndex = 3;
            this.slaConsistencylabel.Text = "consistency";
            // 
            // installSLA2Button
            // 
            this.installSLA2Button.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.installSLA2Button.Location = new System.Drawing.Point(471, 32);
            this.installSLA2Button.Name = "installSLA2Button";
            this.installSLA2Button.Size = new System.Drawing.Size(166, 44);
            this.installSLA2Button.TabIndex = 2;
            this.installSLA2Button.Text = "Install SLA2";
            this.installSLA2Button.UseVisualStyleBackColor = true;
            this.installSLA2Button.Click += new System.EventHandler(this.installSLA2Button_Click);
            // 
            // installSLA1Button
            // 
            this.installSLA1Button.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.installSLA1Button.Location = new System.Drawing.Point(268, 32);
            this.installSLA1Button.Name = "installSLA1Button";
            this.installSLA1Button.Size = new System.Drawing.Size(178, 44);
            this.installSLA1Button.TabIndex = 1;
            this.installSLA1Button.Text = "Install SLA1";
            this.installSLA1Button.UseVisualStyleBackColor = true;
            this.installSLA1Button.Click += new System.EventHandler(this.installSLA1Button_Click);
            // 
            // getSLAButton
            // 
            this.getSLAButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.getSLAButton.Location = new System.Drawing.Point(54, 32);
            this.getSLAButton.Name = "getSLAButton";
            this.getSLAButton.Size = new System.Drawing.Size(190, 45);
            this.getSLAButton.TabIndex = 0;
            this.getSLAButton.Text = "Get Current SLA";
            this.getSLAButton.UseVisualStyleBackColor = true;
            this.getSLAButton.Click += new System.EventHandler(this.getCurrentSLAButton_Click);
            // 
            // consistencyTabPage
            // 
            this.consistencyTabPage.Controls.Add(this.readtimelabel);
            this.consistencyTabPage.Controls.Add(this.readconsistencylabel);
            this.consistencyTabPage.Controls.Add(this.readAgainLatencyListBox);
            this.consistencyTabPage.Controls.Add(this.readLatencyListBox);
            this.consistencyTabPage.Controls.Add(this.consistencyListBox);
            this.consistencyTabPage.Controls.Add(this.clearButton);
            this.consistencyTabPage.Controls.Add(this.readAgainButton);
            this.consistencyTabPage.Controls.Add(this.readButton);
            this.consistencyTabPage.Location = new System.Drawing.Point(4, 29);
            this.consistencyTabPage.Name = "consistencyTabPage";
            this.consistencyTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.consistencyTabPage.Size = new System.Drawing.Size(1004, 574);
            this.consistencyTabPage.TabIndex = 0;
            this.consistencyTabPage.Text = "Read Consistency";
            this.consistencyTabPage.UseVisualStyleBackColor = true;
            // 
            // readtimelabel
            // 
            this.readtimelabel.AutoSize = true;
            this.readtimelabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.readtimelabel.Location = new System.Drawing.Point(345, 92);
            this.readtimelabel.Name = "readtimelabel";
            this.readtimelabel.Size = new System.Drawing.Size(153, 26);
            this.readtimelabel.TabIndex = 7;
            this.readtimelabel.Text = "read time (ms)";
            // 
            // readconsistencylabel
            // 
            this.readconsistencylabel.AutoSize = true;
            this.readconsistencylabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.readconsistencylabel.Location = new System.Drawing.Point(46, 92);
            this.readconsistencylabel.Name = "readconsistencylabel";
            this.readconsistencylabel.Size = new System.Drawing.Size(195, 26);
            this.readconsistencylabel.TabIndex = 6;
            this.readconsistencylabel.Text = "consistency choice";
            // 
            // readAgainLatencyListBox
            // 
            this.readAgainLatencyListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.readAgainLatencyListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.readAgainLatencyListBox.FormattingEnabled = true;
            this.readAgainLatencyListBox.ItemHeight = 31;
            this.readAgainLatencyListBox.Location = new System.Drawing.Point(478, 127);
            this.readAgainLatencyListBox.Name = "readAgainLatencyListBox";
            this.readAgainLatencyListBox.Size = new System.Drawing.Size(123, 186);
            this.readAgainLatencyListBox.TabIndex = 5;
            // 
            // readLatencyListBox
            // 
            this.readLatencyListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.readLatencyListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.readLatencyListBox.FormattingEnabled = true;
            this.readLatencyListBox.ItemHeight = 31;
            this.readLatencyListBox.Location = new System.Drawing.Point(341, 127);
            this.readLatencyListBox.Name = "readLatencyListBox";
            this.readLatencyListBox.Size = new System.Drawing.Size(119, 186);
            this.readLatencyListBox.TabIndex = 4;
            // 
            // consistencyListBox
            // 
            this.consistencyListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.consistencyListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.consistencyListBox.FormattingEnabled = true;
            this.consistencyListBox.ItemHeight = 31;
            this.consistencyListBox.Location = new System.Drawing.Point(43, 127);
            this.consistencyListBox.Name = "consistencyListBox";
            this.consistencyListBox.Size = new System.Drawing.Size(280, 186);
            this.consistencyListBox.TabIndex = 3;
            // 
            // clearButton
            // 
            this.clearButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clearButton.Location = new System.Drawing.Point(326, 28);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(107, 35);
            this.clearButton.TabIndex = 2;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // readAgainButton
            // 
            this.readAgainButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.readAgainButton.Location = new System.Drawing.Point(158, 28);
            this.readAgainButton.Name = "readAgainButton";
            this.readAgainButton.Size = new System.Drawing.Size(148, 35);
            this.readAgainButton.TabIndex = 1;
            this.readAgainButton.Text = "Read Again";
            this.readAgainButton.UseVisualStyleBackColor = true;
            this.readAgainButton.Click += new System.EventHandler(this.ReadAgainButton_Click);
            // 
            // readButton
            // 
            this.readButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.readButton.Location = new System.Drawing.Point(42, 28);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(102, 35);
            this.readButton.TabIndex = 0;
            this.readButton.Text = "Read";
            this.readButton.UseVisualStyleBackColor = true;
            this.readButton.Click += new System.EventHandler(this.ReadButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.consistencyTabPage);
            this.tabControl1.Controls.Add(this.slaTabPage);
            this.tabControl1.Controls.Add(this.configTabPage);
            this.tabControl1.Controls.Add(this.logTabPage);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1012, 607);
            this.tabControl1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(141, 439);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(307, 31);
            this.label1.TabIndex = 9;
            this.label1.Text = "average delivered utility:";
            // 
            // slaDeliveredUtility
            // 
            this.slaDeliveredUtility.AutoSize = true;
            this.slaDeliveredUtility.Location = new System.Drawing.Point(493, 439);
            this.slaDeliveredUtility.Name = "slaDeliveredUtility";
            this.slaDeliveredUtility.Size = new System.Drawing.Size(21, 31);
            this.slaDeliveredUtility.TabIndex = 10;
            this.slaDeliveredUtility.Text = " ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(141, 482);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(300, 31);
            this.label3.TabIndex = 11;
            this.label3.Text = "average read time (ms):";
            // 
            // slaReadTime
            // 
            this.slaReadTime.AutoSize = true;
            this.slaReadTime.Location = new System.Drawing.Point(493, 482);
            this.slaReadTime.Name = "slaReadTime";
            this.slaReadTime.Size = new System.Drawing.Size(0, 31);
            this.slaReadTime.TabIndex = 12;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1012, 607);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Pileus Demo";
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.logTabPage.ResumeLayout(false);
            this.logTabPage.PerformLayout();
            this.configTabPage.ResumeLayout(false);
            this.configTabPage.PerformLayout();
            this.slaTabPage.ResumeLayout(false);
            this.slaTabPage.PerformLayout();
            this.consistencyTabPage.ResumeLayout(false);
            this.consistencyTabPage.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.TabPage logTabPage;
        private System.Windows.Forms.TextBox logTextBox;
        private System.Windows.Forms.TabPage configTabPage;
        private System.Windows.Forms.TextBox configTextBox;
        private System.Windows.Forms.Button restroreConfigButton;
        private System.Windows.Forms.Button installNewConfigButton;
        private System.Windows.Forms.Button proposeNewConfigButton;
        private System.Windows.Forms.Button getConfigButton;
        private System.Windows.Forms.TabPage slaTabPage;
        private System.Windows.Forms.ListBox slaUtilityListBox;
        private System.Windows.Forms.ListBox slaLatencyListBox;
        private System.Windows.Forms.ListBox slaConsistencyListBox;
        private System.Windows.Forms.Label slaUtilityLabel;
        private System.Windows.Forms.Label slaLatencyLabel;
        private System.Windows.Forms.Label slaConsistencylabel;
        private System.Windows.Forms.Button installSLA2Button;
        private System.Windows.Forms.Button installSLA1Button;
        private System.Windows.Forms.Button getSLAButton;
        private System.Windows.Forms.TabPage consistencyTabPage;
        private System.Windows.Forms.ListBox readAgainLatencyListBox;
        private System.Windows.Forms.ListBox readLatencyListBox;
        private System.Windows.Forms.ListBox consistencyListBox;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button readAgainButton;
        private System.Windows.Forms.Button readButton;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label readtimelabel;
        private System.Windows.Forms.Label readconsistencylabel;
        private System.Windows.Forms.Label slaReadTime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label slaDeliveredUtility;
        private System.Windows.Forms.Label label1;


    }
}

