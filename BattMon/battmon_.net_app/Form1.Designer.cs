using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace batt_mon_app
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

// Required method for Designer support - do not modify
// the contents of this method with the code editor, or resource editor will not work

        private void InitializeComponent()
        {
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
			System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
			this.DigitalVoltmBaseUI = new NextUI.BaseUI.BaseUI();
			this.AnalogVoltmBaseUI = new NextUI.BaseUI.BaseUI();
			this.DigitalCurrentBaseUI = new NextUI.BaseUI.BaseUI();
			this.AnalogCurrentBaseUI = new NextUI.BaseUI.BaseUI();
			this.DigitalTempBaseUI = new NextUI.BaseUI.BaseUI();
			this.AnalogTempBaseUI = new NextUI.BaseUI.BaseUI();
			this.StartStopButton = new System.Windows.Forms.Button();
			this.SerPortNameTextBox = new System.Windows.Forms.TextBox();
			this.IterCntTextBox = new System.Windows.Forms.TextBox();
			this.ErrCnt = new System.Windows.Forms.Label();
			this.ErrorCountTextBox = new System.Windows.Forms.TextBox();
			this.CurrentVoltageChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.DataQueueProgressBar1 = new System.Windows.Forms.ProgressBar();
			this.label1 = new System.Windows.Forms.Label();
			this.ChargeProgressBarC = new System.Windows.Forms.ProgressBar();
			this.ChargeProgressBarNL = new System.Windows.Forms.ProgressBar();
			this.ChargeProgressBarD = new System.Windows.Forms.ProgressBar();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.R_Ohm_TextBox1 = new System.Windows.Forms.TextBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.CA_textBox2 = new System.Windows.Forms.TextBox();
			this.LiveDataRadioButton1 = new System.Windows.Forms.RadioButton();
			this.FromFileRadioButton1 = new System.Windows.Forms.RadioButton();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.DischargeChargeCyclesTextBox16 = new System.Windows.Forms.TextBox();
			this.label30 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.label29 = new System.Windows.Forms.Label();
			this.RechrgDuraTextBox15 = new System.Windows.Forms.TextBox();
			this.label28 = new System.Windows.Forms.Label();
			this.label27 = new System.Windows.Forms.Label();
			this.FullyRechargdCheckBox1 = new System.Windows.Forms.CheckBox();
			this.label26 = new System.Windows.Forms.Label();
			this.ChrgInTextBox14 = new System.Windows.Forms.TextBox();
			this.label25 = new System.Windows.Forms.Label();
			this.label24 = new System.Windows.Forms.Label();
			this.ChrgOutTextBox13 = new System.Windows.Forms.TextBox();
			this.label23 = new System.Windows.Forms.Label();
			this.DischrgChrgClcListBox1 = new System.Windows.Forms.ListBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.label22 = new System.Windows.Forms.Label();
			this.BattCoulmbsRelzdTextBox12 = new System.Windows.Forms.TextBox();
			this.label21 = new System.Windows.Forms.Label();
			this.label20 = new System.Windows.Forms.Label();
			this.ComputedCATextBox11 = new System.Windows.Forms.TextBox();
			this.label19 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.BattResOhmsTextBox10 = new System.Windows.Forms.TextBox();
			this.label17 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.TemperTextBox9 = new System.Windows.Forms.TextBox();
			this.label15 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.MaxDchrgCurrTextBox8 = new System.Windows.Forms.TextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.StrtMtrRanDurTextBox7 = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.ripple_filterCheckBox1 = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.CurrentVoltageChart)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// DigitalVoltmBaseUI
			// 
			this.DigitalVoltmBaseUI.Interact = false;
			this.DigitalVoltmBaseUI.Location = new System.Drawing.Point(49, 275);
			this.DigitalVoltmBaseUI.Name = "DigitalVoltmBaseUI";
			this.DigitalVoltmBaseUI.Size = new System.Drawing.Size(241, 113);
			this.DigitalVoltmBaseUI.TabIndex = 0;
			// 
			// AnalogVoltmBaseUI
			// 
			this.AnalogVoltmBaseUI.Interact = false;
			this.AnalogVoltmBaseUI.Location = new System.Drawing.Point(1, 1);
			this.AnalogVoltmBaseUI.Name = "AnalogVoltmBaseUI";
			this.AnalogVoltmBaseUI.Size = new System.Drawing.Size(260, 260);
			this.AnalogVoltmBaseUI.TabIndex = 1;
			// 
			// DigitalCurrentBaseUI
			// 
			this.DigitalCurrentBaseUI.Interact = false;
			this.DigitalCurrentBaseUI.Location = new System.Drawing.Point(314, 269);
			this.DigitalCurrentBaseUI.Name = "DigitalCurrentBaseUI";
			this.DigitalCurrentBaseUI.Size = new System.Drawing.Size(241, 113);
			this.DigitalCurrentBaseUI.TabIndex = 0;
			// 
			// AnalogCurrentBaseUI
			// 
			this.AnalogCurrentBaseUI.Interact = false;
			this.AnalogCurrentBaseUI.Location = new System.Drawing.Point(280, 1);
			this.AnalogCurrentBaseUI.Name = "AnalogCurrentBaseUI";
			this.AnalogCurrentBaseUI.Size = new System.Drawing.Size(260, 260);
			this.AnalogCurrentBaseUI.TabIndex = 1;
			// 
			// DigitalTempBaseUI
			// 
			this.DigitalTempBaseUI.Interact = false;
			this.DigitalTempBaseUI.Location = new System.Drawing.Point(590, 154);
			this.DigitalTempBaseUI.Name = "DigitalTempBaseUI";
			this.DigitalTempBaseUI.Size = new System.Drawing.Size(165, 76);
			this.DigitalTempBaseUI.TabIndex = 0;
			// 
			// AnalogTempBaseUI
			// 
			this.AnalogTempBaseUI.Interact = false;
			this.AnalogTempBaseUI.Location = new System.Drawing.Point(547, -16);
			this.AnalogTempBaseUI.Name = "AnalogTempBaseUI";
			this.AnalogTempBaseUI.Size = new System.Drawing.Size(245, 175);
			this.AnalogTempBaseUI.TabIndex = 1;
			// 
			// StartStopButton
			// 
			this.StartStopButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
			this.StartStopButton.Location = new System.Drawing.Point(103, 75);
			this.StartStopButton.Name = "StartStopButton";
			this.StartStopButton.Size = new System.Drawing.Size(54, 26);
			this.StartStopButton.TabIndex = 2;
			this.StartStopButton.Text = "Start";
			this.StartStopButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.StartStopButton.UseVisualStyleBackColor = true;
			this.StartStopButton.Click += new System.EventHandler(this.StartStopButton_Click);
			// 
			// SerPortNameTextBox
			// 
			this.SerPortNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.SerPortNameTextBox.Location = new System.Drawing.Point(25, 76);
			this.SerPortNameTextBox.Name = "SerPortNameTextBox";
			this.SerPortNameTextBox.Size = new System.Drawing.Size(59, 23);
			this.SerPortNameTextBox.TabIndex = 3;
			this.SerPortNameTextBox.Text = "COM14";
			this.SerPortNameTextBox.TextChanged += new System.EventHandler(this.SerPortNameTextBox_TextChanged);
			// 
			// IterCntTextBox
			// 
			this.IterCntTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.IterCntTextBox.Location = new System.Drawing.Point(60, 108);
			this.IterCntTextBox.Name = "IterCntTextBox";
			this.IterCntTextBox.ReadOnly = true;
			this.IterCntTextBox.Size = new System.Drawing.Size(70, 23);
			this.IterCntTextBox.TabIndex = 4;
			// 
			// ErrCnt
			// 
			this.ErrCnt.AutoSize = true;
			this.ErrCnt.Location = new System.Drawing.Point(148, 110);
			this.ErrCnt.Name = "ErrCnt";
			this.ErrCnt.Size = new System.Drawing.Size(47, 17);
			this.ErrCnt.TabIndex = 5;
			this.ErrCnt.Text = "Errors";
			// 
			// ErrorCountTextBox
			// 
			this.ErrorCountTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.ErrorCountTextBox.ForeColor = System.Drawing.Color.Maroon;
			this.ErrorCountTextBox.ImeMode = System.Windows.Forms.ImeMode.Off;
			this.ErrorCountTextBox.Location = new System.Drawing.Point(204, 106);
			this.ErrorCountTextBox.Name = "ErrorCountTextBox";
			this.ErrorCountTextBox.ReadOnly = true;
			this.ErrorCountTextBox.Size = new System.Drawing.Size(45, 23);
			this.ErrorCountTextBox.TabIndex = 6;
			// 
			// CurrentVoltageChart
			// 
			chartArea1.AxisX.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Seconds;
			chartArea1.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Seconds;
			chartArea1.AxisX.LabelStyle.Format = "mm:ss.f";
			chartArea1.AxisX.LabelStyle.Interval = 0.5D;
			chartArea1.AxisX.MajorGrid.Interval = 0.5D;
			chartArea1.AxisX.MajorGrid.IntervalOffset = 0D;
			chartArea1.AxisX.MajorGrid.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Seconds;
			chartArea1.AxisX.MajorGrid.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Seconds;
			chartArea1.AxisX.MajorTickMark.Interval = 0.5D;
			chartArea1.AxisX.MajorTickMark.IntervalOffset = 0D;
			chartArea1.AxisX.MajorTickMark.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Seconds;
			chartArea1.AxisX.MajorTickMark.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Seconds;
			chartArea1.AxisX.MaximumAutoSize = 80F;
			chartArea1.AxisX.ScaleView.MinSize = 10D;
			chartArea1.AxisX.ScaleView.MinSizeType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Seconds;
			chartArea1.AxisX.ScaleView.SizeType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Seconds;
			chartArea1.AxisX.ScaleView.SmallScrollMinSize = 0.5D;
			chartArea1.AxisX.ScaleView.SmallScrollMinSizeType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Seconds;
			chartArea1.AxisX.ScaleView.SmallScrollSize = 1D;
			chartArea1.AxisX.ScaleView.SmallScrollSizeType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Seconds;
			chartArea1.AxisX.ScrollBar.ButtonStyle = System.Windows.Forms.DataVisualization.Charting.ScrollBarButtonStyles.SmallScroll;
			chartArea1.AxisY.Interval = 25D;
			chartArea1.AxisY.LabelStyle.ForeColor = System.Drawing.Color.Brown;
			chartArea1.AxisY.LineColor = System.Drawing.Color.Brown;
			chartArea1.AxisY.MajorGrid.Interval = 25D;
			chartArea1.AxisY.MajorTickMark.Interval = 25D;
			chartArea1.AxisY.MajorTickMark.LineColor = System.Drawing.Color.Brown;
			chartArea1.AxisY.Maximum = 75D;
			chartArea1.AxisY.MaximumAutoSize = 80F;
			chartArea1.AxisY.Minimum = -125D;
			chartArea1.AxisY2.Interval = 1D;
			chartArea1.AxisY2.LabelStyle.ForeColor = System.Drawing.Color.Blue;
			chartArea1.AxisY2.LineColor = System.Drawing.Color.Blue;
			chartArea1.AxisY2.MajorGrid.Interval = 1D;
			chartArea1.AxisY2.MajorTickMark.Interval = 1D;
			chartArea1.AxisY2.MajorTickMark.LineColor = System.Drawing.Color.Blue;
			chartArea1.AxisY2.Maximum = 16D;
			chartArea1.AxisY2.MaximumAutoSize = 80F;
			chartArea1.AxisY2.Minimum = 8D;
			chartArea1.Name = "BattCurrChartArea2";
			this.CurrentVoltageChart.ChartAreas.Add(chartArea1);
			legend1.Name = "Legend2";
			this.CurrentVoltageChart.Legends.Add(legend1);
			this.CurrentVoltageChart.Location = new System.Drawing.Point(9, 367);
			this.CurrentVoltageChart.Name = "CurrentVoltageChart";
			series1.BorderWidth = 3;
			series1.ChartArea = "BattCurrChartArea2";
			series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
			series1.Color = System.Drawing.Color.Brown;
			series1.Legend = "Legend2";
			series1.Name = "Amps";
			series2.BorderWidth = 2;
			series2.ChartArea = "BattCurrChartArea2";
			series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
			series2.Color = System.Drawing.Color.Blue;
			series2.Legend = "Legend2";
			series2.Name = "Volts";
			series2.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
			this.CurrentVoltageChart.Series.Add(series1);
			this.CurrentVoltageChart.Series.Add(series2);
			this.CurrentVoltageChart.Size = new System.Drawing.Size(583, 325);
			this.CurrentVoltageChart.TabIndex = 8;
			this.CurrentVoltageChart.Text = "Current, A";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(4, 110);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(53, 17);
			this.label2.TabIndex = 10;
			this.label2.Text = "Data in";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.DataQueueProgressBar1);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.ErrorCountTextBox);
			this.groupBox1.Controls.Add(this.ErrCnt);
			this.groupBox1.Controls.Add(this.IterCntTextBox);
			this.groupBox1.Controls.Add(this.SerPortNameTextBox);
			this.groupBox1.Controls.Add(this.StartStopButton);
			this.groupBox1.Location = new System.Drawing.Point(816, 170);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(254, 146);
			this.groupBox1.TabIndex = 11;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "USB Serial port";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(19, 28);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(133, 17);
			this.label3.TabIndex = 12;
			this.label3.Text = "Data Queue Length";
			// 
			// DataQueueProgressBar1
			// 
			this.DataQueueProgressBar1.Location = new System.Drawing.Point(20, 49);
			this.DataQueueProgressBar1.Name = "DataQueueProgressBar1";
			this.DataQueueProgressBar1.Size = new System.Drawing.Size(220, 12);
			this.DataQueueProgressBar1.TabIndex = 11;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(659, 248);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(123, 17);
			this.label1.TabIndex = 12;
			this.label1.Text = "State of Charge at";
			// 
			// ChargeProgressBarC
			// 
			this.ChargeProgressBarC.BackColor = System.Drawing.Color.DimGray;
			this.ChargeProgressBarC.Location = new System.Drawing.Point(644, 274);
			this.ChargeProgressBarC.Name = "ChargeProgressBarC";
			this.ChargeProgressBarC.Size = new System.Drawing.Size(155, 16);
			this.ChargeProgressBarC.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.ChargeProgressBarC.TabIndex = 15;
			// 
			// ChargeProgressBarNL
			// 
			this.ChargeProgressBarNL.BackColor = System.Drawing.Color.DimGray;
			this.ChargeProgressBarNL.ForeColor = System.Drawing.Color.Black;
			this.ChargeProgressBarNL.Location = new System.Drawing.Point(644, 299);
			this.ChargeProgressBarNL.Name = "ChargeProgressBarNL";
			this.ChargeProgressBarNL.Size = new System.Drawing.Size(155, 16);
			this.ChargeProgressBarNL.Step = 2;
			this.ChargeProgressBarNL.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.ChargeProgressBarNL.TabIndex = 14;
			// 
			// ChargeProgressBarD
			// 
			this.ChargeProgressBarD.BackColor = System.Drawing.Color.DimGray;
			this.ChargeProgressBarD.Location = new System.Drawing.Point(644, 324);
			this.ChargeProgressBarD.Name = "ChargeProgressBarD";
			this.ChargeProgressBarD.Size = new System.Drawing.Size(155, 16);
			this.ChargeProgressBarD.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.ChargeProgressBarD.TabIndex = 13;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(576, 273);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(65, 17);
			this.label4.TabIndex = 16;
			this.label4.Text = "Charging";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(576, 299);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(62, 17);
			this.label5.TabIndex = 17;
			this.label5.Text = "No Load";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(555, 323);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(83, 17);
			this.label6.TabIndex = 18;
			this.label6.Text = "Discharging";
			// 
			// R_Ohm_TextBox1
			// 
			this.R_Ohm_TextBox1.Location = new System.Drawing.Point(108, 109);
			this.R_Ohm_TextBox1.Name = "R_Ohm_TextBox1";
			this.R_Ohm_TextBox1.ReadOnly = true;
			this.R_Ohm_TextBox1.Size = new System.Drawing.Size(65, 22);
			this.R_Ohm_TextBox1.TabIndex = 19;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(51, 48);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(74, 37);
			this.pictureBox1.TabIndex = 20;
			this.pictureBox1.TabStop = false;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(113, 88);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(56, 17);
			this.label7.TabIndex = 21;
			this.label7.Text = "R, Ohm";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(35, 88);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(26, 17);
			this.label8.TabIndex = 22;
			this.label8.Text = "CA";
			// 
			// CA_textBox2
			// 
			this.CA_textBox2.Location = new System.Drawing.Point(19, 108);
			this.CA_textBox2.Name = "CA_textBox2";
			this.CA_textBox2.ReadOnly = true;
			this.CA_textBox2.Size = new System.Drawing.Size(59, 22);
			this.CA_textBox2.TabIndex = 23;
			// 
			// LiveDataRadioButton1
			// 
			this.LiveDataRadioButton1.AutoSize = true;
			this.LiveDataRadioButton1.Location = new System.Drawing.Point(12, 21);
			this.LiveDataRadioButton1.Name = "LiveDataRadioButton1";
			this.LiveDataRadioButton1.Size = new System.Drawing.Size(89, 21);
			this.LiveDataRadioButton1.TabIndex = 24;
			this.LiveDataRadioButton1.TabStop = true;
			this.LiveDataRadioButton1.Text = "Live Data";
			this.LiveDataRadioButton1.UseVisualStyleBackColor = true;
			this.LiveDataRadioButton1.CheckedChanged += new System.EventHandler(this.LiveDataRadioButton1_CheckedChanged);
			// 
			// FromFileRadioButton1
			// 
			this.FromFileRadioButton1.AutoSize = true;
			this.FromFileRadioButton1.Location = new System.Drawing.Point(164, 21);
			this.FromFileRadioButton1.Name = "FromFileRadioButton1";
			this.FromFileRadioButton1.Size = new System.Drawing.Size(87, 21);
			this.FromFileRadioButton1.TabIndex = 25;
			this.FromFileRadioButton1.TabStop = true;
			this.FromFileRadioButton1.Text = "From File";
			this.FromFileRadioButton1.UseVisualStyleBackColor = true;
			this.FromFileRadioButton1.CheckedChanged += new System.EventHandler(this.FromFileRadioButton1_CheckedChanged);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.LiveDataRadioButton1);
			this.groupBox2.Controls.Add(this.FromFileRadioButton1);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.CA_textBox2);
			this.groupBox2.Controls.Add(this.pictureBox1);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.R_Ohm_TextBox1);
			this.groupBox2.Location = new System.Drawing.Point(804, 10);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(266, 149);
			this.groupBox2.TabIndex = 26;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Data Source";
			// 
			// DischargeChargeCyclesTextBox16
			// 
			this.DischargeChargeCyclesTextBox16.BackColor = System.Drawing.SystemColors.Window;
			this.DischargeChargeCyclesTextBox16.Location = new System.Drawing.Point(816, 322);
			this.DischargeChargeCyclesTextBox16.Name = "DischargeChargeCyclesTextBox16";
			this.DischargeChargeCyclesTextBox16.ReadOnly = true;
			this.DischargeChargeCyclesTextBox16.Size = new System.Drawing.Size(30, 22);
			this.DischargeChargeCyclesTextBox16.TabIndex = 28;
			// 
			// label30
			// 
			this.label30.AutoSize = true;
			this.label30.Location = new System.Drawing.Point(812, 346);
			this.label30.Name = "label30";
			this.label30.Size = new System.Drawing.Size(176, 17);
			this.label30.TabIndex = 27;
			this.label30.Text = "Discharge - Charge Cycles";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.label29);
			this.groupBox3.Controls.Add(this.RechrgDuraTextBox15);
			this.groupBox3.Controls.Add(this.label28);
			this.groupBox3.Controls.Add(this.label27);
			this.groupBox3.Controls.Add(this.FullyRechargdCheckBox1);
			this.groupBox3.Controls.Add(this.label26);
			this.groupBox3.Controls.Add(this.ChrgInTextBox14);
			this.groupBox3.Controls.Add(this.label25);
			this.groupBox3.Controls.Add(this.label24);
			this.groupBox3.Controls.Add(this.ChrgOutTextBox13);
			this.groupBox3.Controls.Add(this.label23);
			this.groupBox3.Controls.Add(this.DischrgChrgClcListBox1);
			this.groupBox3.Controls.Add(this.groupBox4);
			this.groupBox3.Location = new System.Drawing.Point(632, 367);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(390, 319);
			this.groupBox3.TabIndex = 29;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Selected Cycle";
			// 
			// label29
			// 
			this.label29.AutoSize = true;
			this.label29.Location = new System.Drawing.Point(80, 282);
			this.label29.Name = "label29";
			this.label29.Size = new System.Drawing.Size(30, 17);
			this.label29.TabIndex = 13;
			this.label29.Text = "min";
			// 
			// RechrgDuraTextBox15
			// 
			this.RechrgDuraTextBox15.BackColor = System.Drawing.SystemColors.Window;
			this.RechrgDuraTextBox15.Location = new System.Drawing.Point(15, 281);
			this.RechrgDuraTextBox15.Name = "RechrgDuraTextBox15";
			this.RechrgDuraTextBox15.ReadOnly = true;
			this.RechrgDuraTextBox15.Size = new System.Drawing.Size(49, 22);
			this.RechrgDuraTextBox15.TabIndex = 12;
			// 
			// label28
			// 
			this.label28.AutoSize = true;
			this.label28.Location = new System.Drawing.Point(14, 255);
			this.label28.Name = "label28";
			this.label28.Size = new System.Drawing.Size(139, 17);
			this.label28.TabIndex = 11;
			this.label28.Text = "Recharging Duration";
			// 
			// label27
			// 
			this.label27.AutoSize = true;
			this.label27.Location = new System.Drawing.Point(18, 196);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(37, 17);
			this.label27.TabIndex = 10;
			this.label27.Text = "Fully";
			// 
			// FullyRechargdCheckBox1
			// 
			this.FullyRechargdCheckBox1.AutoSize = true;
			this.FullyRechargdCheckBox1.Location = new System.Drawing.Point(10, 217);
			this.FullyRechargdCheckBox1.Name = "FullyRechargdCheckBox1";
			this.FullyRechargdCheckBox1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.FullyRechargdCheckBox1.Size = new System.Drawing.Size(100, 21);
			this.FullyRechargdCheckBox1.TabIndex = 9;
			this.FullyRechargdCheckBox1.Text = "Recharged";
			this.FullyRechargdCheckBox1.UseVisualStyleBackColor = true;
			this.FullyRechargdCheckBox1.CheckedChanged += new System.EventHandler(this.FullyRechargdCheckBox1_CheckedChanged);
			// 
			// label26
			// 
			this.label26.AutoSize = true;
			this.label26.Location = new System.Drawing.Point(82, 160);
			this.label26.Name = "label26";
			this.label26.Size = new System.Drawing.Size(19, 17);
			this.label26.TabIndex = 7;
			this.label26.Text = "Q";
			// 
			// ChrgInTextBox14
			// 
			this.ChrgInTextBox14.BackColor = System.Drawing.SystemColors.Window;
			this.ChrgInTextBox14.Location = new System.Drawing.Point(22, 158);
			this.ChrgInTextBox14.Name = "ChrgInTextBox14";
			this.ChrgInTextBox14.ReadOnly = true;
			this.ChrgInTextBox14.Size = new System.Drawing.Size(53, 22);
			this.ChrgInTextBox14.TabIndex = 6;
			// 
			// label25
			// 
			this.label25.AutoSize = true;
			this.label25.Location = new System.Drawing.Point(23, 136);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(69, 17);
			this.label25.TabIndex = 5;
			this.label25.Text = "Charge In";
			// 
			// label24
			// 
			this.label24.AutoSize = true;
			this.label24.Location = new System.Drawing.Point(84, 93);
			this.label24.Name = "label24";
			this.label24.Size = new System.Drawing.Size(19, 17);
			this.label24.TabIndex = 4;
			this.label24.Text = "Q";
			// 
			// ChrgOutTextBox13
			// 
			this.ChrgOutTextBox13.BackColor = System.Drawing.SystemColors.Window;
			this.ChrgOutTextBox13.Location = new System.Drawing.Point(21, 91);
			this.ChrgOutTextBox13.Name = "ChrgOutTextBox13";
			this.ChrgOutTextBox13.ReadOnly = true;
			this.ChrgOutTextBox13.Size = new System.Drawing.Size(54, 22);
			this.ChrgOutTextBox13.TabIndex = 3;
			// 
			// label23
			// 
			this.label23.AutoSize = true;
			this.label23.Location = new System.Drawing.Point(17, 68);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(81, 17);
			this.label23.TabIndex = 2;
			this.label23.Text = "Charge Out";
			// 
			// DischrgChrgClcListBox1
			// 
			this.DischrgChrgClcListBox1.FormattingEnabled = true;
			this.DischrgChrgClcListBox1.ItemHeight = 16;
			this.DischrgChrgClcListBox1.Location = new System.Drawing.Point(15, 32);
			this.DischrgChrgClcListBox1.Name = "DischrgChrgClcListBox1";
			this.DischrgChrgClcListBox1.Size = new System.Drawing.Size(328, 36);
			this.DischrgChrgClcListBox1.TabIndex = 1;
			this.DischrgChrgClcListBox1.SelectedIndexChanged += new System.EventHandler(this.DischrgChrgClcListBox1_SelectedIndexChanged);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.label22);
			this.groupBox4.Controls.Add(this.BattCoulmbsRelzdTextBox12);
			this.groupBox4.Controls.Add(this.label21);
			this.groupBox4.Controls.Add(this.label20);
			this.groupBox4.Controls.Add(this.ComputedCATextBox11);
			this.groupBox4.Controls.Add(this.label19);
			this.groupBox4.Controls.Add(this.label18);
			this.groupBox4.Controls.Add(this.BattResOhmsTextBox10);
			this.groupBox4.Controls.Add(this.label17);
			this.groupBox4.Controls.Add(this.label16);
			this.groupBox4.Controls.Add(this.TemperTextBox9);
			this.groupBox4.Controls.Add(this.label15);
			this.groupBox4.Controls.Add(this.label14);
			this.groupBox4.Controls.Add(this.MaxDchrgCurrTextBox8);
			this.groupBox4.Controls.Add(this.label13);
			this.groupBox4.Controls.Add(this.label12);
			this.groupBox4.Controls.Add(this.StrtMtrRanDurTextBox7);
			this.groupBox4.Controls.Add(this.label11);
			this.groupBox4.Location = new System.Drawing.Point(116, 73);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(258, 239);
			this.groupBox4.TabIndex = 0;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Engine Starting Cycle";
			// 
			// label22
			// 
			this.label22.AutoSize = true;
			this.label22.Location = new System.Drawing.Point(188, 197);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(19, 17);
			this.label22.TabIndex = 17;
			this.label22.Text = "Q";
			// 
			// BattCoulmbsRelzdTextBox12
			// 
			this.BattCoulmbsRelzdTextBox12.BackColor = System.Drawing.SystemColors.Window;
			this.BattCoulmbsRelzdTextBox12.Location = new System.Drawing.Point(154, 194);
			this.BattCoulmbsRelzdTextBox12.Name = "BattCoulmbsRelzdTextBox12";
			this.BattCoulmbsRelzdTextBox12.ReadOnly = true;
			this.BattCoulmbsRelzdTextBox12.Size = new System.Drawing.Size(32, 22);
			this.BattCoulmbsRelzdTextBox12.TabIndex = 16;
			// 
			// label21
			// 
			this.label21.AutoSize = true;
			this.label21.Location = new System.Drawing.Point(14, 195);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(134, 17);
			this.label21.TabIndex = 15;
			this.label21.Text = "Coulombs Released";
			// 
			// label20
			// 
			this.label20.AutoSize = true;
			this.label20.Location = new System.Drawing.Point(158, 162);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(64, 17);
			this.label20.TabIndex = 14;
			this.label20.Text = "Amperes";
			// 
			// ComputedCATextBox11
			// 
			this.ComputedCATextBox11.BackColor = System.Drawing.SystemColors.Info;
			this.ComputedCATextBox11.Location = new System.Drawing.Point(114, 160);
			this.ComputedCATextBox11.Name = "ComputedCATextBox11";
			this.ComputedCATextBox11.ReadOnly = true;
			this.ComputedCATextBox11.Size = new System.Drawing.Size(41, 22);
			this.ComputedCATextBox11.TabIndex = 13;
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(10, 162);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(94, 17);
			this.label19.TabIndex = 12;
			this.label19.Text = "Computed CA";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(199, 126);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(38, 17);
			this.label18.TabIndex = 11;
			this.label18.Text = "Ohm";
			// 
			// BattResOhmsTextBox10
			// 
			this.BattResOhmsTextBox10.BackColor = System.Drawing.SystemColors.Info;
			this.BattResOhmsTextBox10.Location = new System.Drawing.Point(130, 127);
			this.BattResOhmsTextBox10.Name = "BattResOhmsTextBox10";
			this.BattResOhmsTextBox10.ReadOnly = true;
			this.BattResOhmsTextBox10.Size = new System.Drawing.Size(56, 22);
			this.BattResOhmsTextBox10.TabIndex = 10;
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(18, 128);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(107, 17);
			this.label17.TabIndex = 9;
			this.label17.Text = "Batt Resistance";
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(163, 96);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(45, 17);
			this.label16.TabIndex = 8;
			this.label16.Text = "deg C";
			// 
			// TemperTextBox9
			// 
			this.TemperTextBox9.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.TemperTextBox9.Location = new System.Drawing.Point(117, 94);
			this.TemperTextBox9.Name = "TemperTextBox9";
			this.TemperTextBox9.ReadOnly = true;
			this.TemperTextBox9.Size = new System.Drawing.Size(42, 22);
			this.TemperTextBox9.TabIndex = 7;
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(13, 93);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(90, 17);
			this.label15.TabIndex = 6;
			this.label15.Text = "Temperature";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(229, 61);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(17, 17);
			this.label14.TabIndex = 5;
			this.label14.Text = "A";
			// 
			// MaxDchrgCurrTextBox8
			// 
			this.MaxDchrgCurrTextBox8.BackColor = System.Drawing.SystemColors.Window;
			this.MaxDchrgCurrTextBox8.Location = new System.Drawing.Point(174, 63);
			this.MaxDchrgCurrTextBox8.Name = "MaxDchrgCurrTextBox8";
			this.MaxDchrgCurrTextBox8.ReadOnly = true;
			this.MaxDchrgCurrTextBox8.Size = new System.Drawing.Size(48, 22);
			this.MaxDchrgCurrTextBox8.TabIndex = 4;
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(17, 62);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(152, 17);
			this.label13.TabIndex = 3;
			this.label13.Text = "Max Discharge Current";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(220, 30);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(30, 17);
			this.label12.TabIndex = 2;
			this.label12.Text = "sec";
			// 
			// StrtMtrRanDurTextBox7
			// 
			this.StrtMtrRanDurTextBox7.BackColor = System.Drawing.SystemColors.Window;
			this.StrtMtrRanDurTextBox7.Location = new System.Drawing.Point(172, 30);
			this.StrtMtrRanDurTextBox7.Name = "StrtMtrRanDurTextBox7";
			this.StrtMtrRanDurTextBox7.ReadOnly = true;
			this.StrtMtrRanDurTextBox7.Size = new System.Drawing.Size(39, 22);
			this.StrtMtrRanDurTextBox7.TabIndex = 1;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(13, 33);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(146, 17);
			this.label11.TabIndex = 0;
			this.label11.Text = "Starter Motor Ran For";
			// 
			// ripple_filterCheckBox1
			// 
			this.ripple_filterCheckBox1.AutoSize = true;
			this.ripple_filterCheckBox1.Location = new System.Drawing.Point(965, 324);
			this.ripple_filterCheckBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.ripple_filterCheckBox1.Name = "ripple_filterCheckBox1";
			this.ripple_filterCheckBox1.Size = new System.Drawing.Size(105, 21);
			this.ripple_filterCheckBox1.TabIndex = 30;
			this.ripple_filterCheckBox1.Text = "Ripple Filter";
			this.ripple_filterCheckBox1.UseVisualStyleBackColor = true;
			this.ripple_filterCheckBox1.CheckedChanged += new System.EventHandler(this.RippleFilterOn_toggled);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1081, 698);
			this.Controls.Add(this.ripple_filterCheckBox1);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.DischargeChargeCyclesTextBox16);
			this.Controls.Add(this.label30);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.ChargeProgressBarC);
			this.Controls.Add(this.ChargeProgressBarD);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.CurrentVoltageChart);
			this.Controls.Add(this.ChargeProgressBarNL);
			this.Controls.Add(this.DigitalVoltmBaseUI);
			this.Controls.Add(this.AnalogVoltmBaseUI);
			this.Controls.Add(this.DigitalCurrentBaseUI);
			this.Controls.Add(this.AnalogCurrentBaseUI);
			this.Controls.Add(this.DigitalTempBaseUI);
			this.Controls.Add(this.AnalogTempBaseUI);
			this.Name = "Form1";
			((System.ComponentModel.ISupportInitialize)(this.CurrentVoltageChart)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion
// our class members here
// and 3rd party controls
        private NextUI.BaseUI.BaseUI DigitalVoltmBaseUI; // digital battery voltage display
		private NextUI.BaseUI.BaseUI AnalogVoltmBaseUI;  // analog battery voltmeter  
		private NextUI.BaseUI.BaseUI DigitalCurrentBaseUI; // digital battery current display
		private NextUI.BaseUI.BaseUI AnalogCurrentBaseUI; // analog battery current display
		private NextUI.BaseUI.BaseUI DigitalTempBaseUI; // digital battery trmp display
		private NextUI.BaseUI.BaseUI AnalogTempBaseUI;
		private System.Windows.Forms.Button StartStopButton;
		public System.Windows.Forms.TextBox SerPortNameTextBox;
		private System.Windows.Forms.TextBox IterCntTextBox;
		private System.Windows.Forms.Label ErrCnt;
		private System.Windows.Forms.TextBox ErrorCountTextBox; // diagram battery temperature
		private System.Windows.Forms.DataVisualization.Charting.Chart CurrentVoltageChart; // battery voltage-current chart
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ProgressBar ChargeProgressBarC; // battery charge state - based on charging curve
		private System.Windows.Forms.ProgressBar ChargeProgressBarNL; // battery charge state - based on at rest
		private System.Windows.Forms.ProgressBar ChargeProgressBarD; // battery charge state - based on discharging curve
		private System.Windows.Forms.ProgressBar DataQueueProgressBar1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox R_Ohm_TextBox1;  // run time size of batt_mon data queue
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox CA_textBox2;
		private System.Windows.Forms.RadioButton LiveDataRadioButton1;
		private System.Windows.Forms.RadioButton FromFileRadioButton1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox DischargeChargeCyclesTextBox16;
		private System.Windows.Forms.Label label30;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label29;
		private System.Windows.Forms.TextBox RechrgDuraTextBox15;
		private System.Windows.Forms.Label label28;
		private System.Windows.Forms.Label label27;
		private System.Windows.Forms.CheckBox FullyRechargdCheckBox1;
		private System.Windows.Forms.Label label26;
		private System.Windows.Forms.TextBox ChrgInTextBox14;
		private System.Windows.Forms.Label label25;
		private System.Windows.Forms.Label label24;
		private System.Windows.Forms.TextBox ChrgOutTextBox13;
		private System.Windows.Forms.Label label23;
		private System.Windows.Forms.ListBox DischrgChrgClcListBox1;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.TextBox BattCoulmbsRelzdTextBox12;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.TextBox ComputedCATextBox11;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.TextBox BattResOhmsTextBox10;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.TextBox TemperTextBox9;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.TextBox MaxDchrgCurrTextBox8;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.TextBox StrtMtrRanDurTextBox7;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.CheckBox ripple_filterCheckBox1; // starter motor icon
    }
}

