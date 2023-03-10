using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmicronLab.VectorNetworkAnalysis.AutomationInterface;
using OmicronLab.VectorNetworkAnalysis.AutomationInterface.Interfaces;
using OmicronLab.VectorNetworkAnalysis.AutomationInterface.Interfaces.Measurements;
using OmicronLab.VectorNetworkAnalysis.AutomationInterface.Enumerations;
using OmicronLab.VectorNetworkAnalysis.AutomationInterface.DataTypes;
using Microsoft.Win32;
using System.Collections.ObjectModel;


namespace BodeGUI
{
    public class TaskLog
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsTaskSuccessful { get; set; }
        public Dictionary<string, string> ReaderOut { get; set; }
        public TaskLog()
        {
            Name = string.Empty;
            Description = string.Empty;
            IsTaskSuccessful = false;
            ReaderOut = new Dictionary<string, string>
            { {"Initialize","Waiting to connect bode, make sure bode100 is connected to computer and Bode Analyzer Suite app is closed before connecting" },
                {"Connecting","Bode Connecting Please wait" },
                {"ConnectFailed","Bode connection failed, make sure bode100 is connected to computer and Bode Analyzer Suite app is closed before connecting" },
                {"Open", "Performing Open Calibration" },
                {"Short", "Perfroming Short Calibration" },
                {"Load", "Performing Load Calibration" },
                {"Run", "Performing Test Please wait" },
                {"Export", "Exporting to CSV"},
                {"Ready", "Ready to collect data" } };
    }
    }

    /* Standard orgization of data for measuring horn charateristics */
    public class Data
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public double Resfreq { get; set; }
        public double Antifreq { get; set; }
        public double Res_impedance { get; set; }
        public double Anti_impedance { get; set; }
        public double Capacitance { get; set; }
        public double Resistance { get; set; }
        public double QualityFactor { get; set; }

        public Data()
        {
            Index = 0;
            Name = "Name";
            Capacitance = 0;
            Resfreq = 0;
            Antifreq = 0;
            Res_impedance = 0;
            Anti_impedance = 0;
            QualityFactor = 0;
        }
    }
    /* Main class with methods for measurements of horn characteristics */
    public class Horn_Characteristic
    {
        public OnePortMeasurement measurement;
        public BodeDevice bode;
        public ExecutionState state;
        public BodeAutomationInterface auto = new BodeAutomation();
        public Data horn_data = new();
        public int sweep_PTS { get; set; }
        public double sweep_LOW { get; set; }
        public double sweep_HIGH { get; set; }
        public double Bandwidth { get; set; }
        public bool IsQF_Checked { get; set; }
        public Horn_Characteristic()
        {
            sweep_PTS = 201;
            sweep_LOW = 180000;
            sweep_HIGH = 190000;
            Bandwidth = 300000;
            IsQF_Checked = false;
        }

        /* Automatically searches for first availible bode100 device */
        public void Connect()
        {
            bode = auto.Connect();
            measurement = bode.Impedance.CreateOnePortMeasurement();
        }

        /* Sweeps relevant frequencies for Bluesky pushmode piezo */
        public void Sweep()
        {

            /* Run sweeps to dtermine res and anti-res frequencies */
            measurement.ReceiverBandwidth = (ReceiverBandwidth)Bandwidth;
            measurement.ConfigureSweep(sweep_LOW, sweep_HIGH, sweep_PTS, SweepMode.Logarithmic);
            state = measurement.ExecuteMeasurement();
            if (state != ExecutionState.Ok)
            {
                bode.ShutDown();
                return;
            }
            double[] freq = measurement.Results.MeasurementFrequencies;
            double[] impedance = measurement.Results.Magnitude(MagnitudeUnit.Lin);
            horn_data.Resfreq = measurement.Results.CalculateFResQValues(false, true, FResQFormats.Magnitude).ResonanceFrequency;
            horn_data.Antifreq = measurement.Results.CalculateFResQValues(true, true, FResQFormats.Magnitude).ResonanceFrequency;

            /* Use data from CalcResFreq to measure impedance at resonance as single measurement */
            measurement.ConfigureSinglePoint(horn_data.Resfreq);
            state = measurement.ExecuteMeasurement();
            if (state != ExecutionState.Ok)
            {
                bode.ShutDown();
                return;
            }
            horn_data.Res_impedance = measurement.Results.MagnitudeAt(0, MagnitudeUnit.Lin);
            horn_data.QualityFactor = measurement.Results.QAt(0);

            /* Use data from CalcResFreq to measure impedance at anti-resonance as single measurement */
            measurement.ConfigureSinglePoint(horn_data.Antifreq);
            state = measurement.ExecuteMeasurement();
            if (state != ExecutionState.Ok)
            {
                bode.ShutDown();
                return;
            }
            horn_data.Anti_impedance = measurement.Results.MagnitudeAt(0, MagnitudeUnit.Lin);

            /* Round data for GUI output */
            horn_data.Resfreq = Math.Round(horn_data.Resfreq / 1000.0, 3, MidpointRounding.AwayFromZero);
            horn_data.Antifreq = Math.Round(horn_data.Antifreq / 1000.0, 3, MidpointRounding.AwayFromZero);
            horn_data.Res_impedance = Math.Round(horn_data.Res_impedance, 3, MidpointRounding.AwayFromZero);
            horn_data.Anti_impedance = Math.Round(horn_data.Anti_impedance / 1000.0, 3, MidpointRounding.AwayFromZero);
            horn_data.QualityFactor = Math.Round(horn_data.QualityFactor,3,MidpointRounding.AwayFromZero);

            /* Measure Capacitance value at 1000 Hz */
            measurement.ConfigureSinglePoint(1000);
            state = measurement.ExecuteMeasurement();
            if (state != ExecutionState.Ok)
            {
                bode.ShutDown();
                return;
            }
            double cap = measurement.Results.CsAt(0);
            horn_data.Capacitance = Math.Round(cap * 1e12, 3, MidpointRounding.AwayFromZero);
        }

        public void OpenCal()
        {
            /* Bode Automation Suite method runs open calibration */
            ExecutionState state = measurement.Calibration.FullRange.ExecuteOpen();
        }

        public void ShortCal()
        {
            ExecutionState state = measurement.Calibration.FullRange.ExecuteShort();
        }

        public void LoadCal()
        {
            ExecutionState state = measurement.Calibration.FullRange.ExecuteLoad();
        }

        public void TestCal()
        {

            measurement.ConfigureSinglePoint(1000);
            state = measurement.ExecuteMeasurement();
            if (state != ExecutionState.Ok)
            {
                bode.ShutDown();
                return;
            }
            horn_data.Resistance = measurement.Results.MagnitudeAt(0, MagnitudeUnit.Lin);
        }

        /* Disconnects Bode100 device from computer */
        public void Disconnect()
        {
            if (bode != null) bode.ShutDown();      //If bode is connectect run bode disconnect method
        }

        /* Eport path opens file explorer and returns a path to save data entered by user */
        public string ExportPath()
        {
            string fileSelected = "";
            SaveFileDialog openFileDialog = new SaveFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.Title = "Select file loaction";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                fileSelected = openFileDialog.FileName;
            }
            return fileSelected;
        }
    }
}
