using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmicronLab.VectorNetworkAnalysis.AutomationInterface;
using OmicronLab.VectorNetworkAnalysis.AutomationInterface.Interfaces;
using OmicronLab.VectorNetworkAnalysis.AutomationInterface.Interfaces.Measurements;
using OmicronLab.VectorNetworkAnalysis.AutomationInterface.Enumerations;
using OmicronLab.VectorNetworkAnalysis.AutomationInterface.DataTypes;

namespace BodeGUI
{

    public class Data
    {
        public string Name { get; set; }
        public double Resfreq { get; set; }
        public double Antifreq { get; set; }
        public double Res_impedance  { get; set; }
        public double Anti_impedance { get; set; }
        public double Capacitance { get; set; }
        public double Resistance { get; set; }

        public Data()
        {
            Name = "Name";
            Capacitance = 0;
            Resfreq = 0;
            Antifreq = 0;
            Res_impedance = 0;
            Anti_impedance = 0;
        }
    }
    public class Horn_Characteristic
    {
        public OnePortMeasurement measurement;
        public BodeDevice bode;
        public ExecutionState state;
        public BodeAutomationInterface auto = new BodeAutomation();
        public Data horn_data = new();

        public Horn_Characteristic()
        {

        }

        public void Connect()
        {
            bode = auto.Connect();
            measurement = bode.Impedance.CreateOnePortMeasurement();
        }
        public void Sweep()
        {
            int sweep_PTS = 201;
            double sweep_LOW = 180000;
            double sweep_HIGH = 190000;
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
            
            for(int i = 0; i < freq.Length; i++)
            {
                if(freq[i] < horn_data.Resfreq && freq[i+1] >= horn_data.Resfreq)
                {
                    horn_data.Res_impedance = impedance[i];
                }
                if (freq[i] < horn_data.Antifreq && freq[i + 1] >= horn_data.Antifreq)
                {
                    horn_data.Anti_impedance = impedance[i];
                }
            }
            horn_data.Resfreq = Math.Round(horn_data.Resfreq / 1000.0 , 3 ,MidpointRounding.AwayFromZero);
            horn_data.Antifreq = Math.Round(horn_data.Antifreq / 1000.0, 3 , MidpointRounding.AwayFromZero);
            horn_data.Res_impedance = Math.Round(horn_data.Res_impedance, 3, MidpointRounding.AwayFromZero);
            horn_data.Anti_impedance = Math.Round(horn_data.Anti_impedance / 1000.0, 3, MidpointRounding.AwayFromZero);
            measurement.ConfigureSweep(1000, 1201, 51, SweepMode.Logarithmic);
            if (state != ExecutionState.Ok)
            {
                bode.ShutDown();
                return;
            }
            freq = measurement.Results.MeasurementFrequencies;
            //double[] cap = measurement.Results.Cs();
            //horn_data.Capacitance = cap[0];

        }

        public void Open()
        {
            ExecutionState state = measurement.Calibration.FullRange.ExecuteOpen();
        }

        public void Short()
        {
            ExecutionState state = measurement.Calibration.FullRange.ExecuteShort();
        }

        public void Load()
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
            horn_data.Resistance = measurement.Results.MagnitudeAt(0,MagnitudeUnit.Lin);
        }

        public void Disconnect()
        {
            bode.ShutDown();
        }
    }
}
