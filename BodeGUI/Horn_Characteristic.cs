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
        public Data()
        {
            Name = "Name";
            Capacitance = 100;
            Resfreq = 180000;
            Antifreq = 190000;
            Res_impedance = 50;
            Anti_impedance = 10000;
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
 /*           bode = auto.Connect();
            measurement = bode.Impedance.CreateOnePortMeasurement();
 */       }

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

            //measurement.Results.CalculateFResQValues(false, true,FResQFormats.Magnitude);
            //measurement.Results.CalculateFResQValues(true, true, FResQFormats.Magnitude);
            //horn_data.Resfreq = measurement.Resul

            double[] freq = measurement.Results.MeasurementFrequencies;
            double[] impedance = measurement.Results.Magnitude(MagnitudeUnit.Lin);
            horn_data.Res_impedance = impedance[0];
            horn_data.Anti_impedance = impedance[0];
            for(int i = 0; i < impedance.Length;i++)
            {
                if (impedance[i] < horn_data.Res_impedance)
                {
                    horn_data.Res_impedance = impedance[i];
                    horn_data.Resfreq = freq[i];
                }
                if (impedance[i] > horn_data.Anti_impedance)
                {
                    horn_data.Anti_impedance = impedance[i];
                    horn_data.Antifreq = freq[i];
                }
            }
            measurement.ConfigureSweep(1000, 1001, 2, SweepMode.Linear);
            if (state != ExecutionState.Ok)
            {
                bode.ShutDown();
                return;
            }

            double[] cap = measurement.Results.Cs();
            horn_data.Capacitance = cap[0];
        }

        public void Calibrate()
        {
            void Open()
            {
                ExecutionState state = measurement.Calibration.FullRange.ExecuteOpen();
            }
            void Short()
            {
                ExecutionState state = measurement.Calibration.FullRange.ExecuteShort();
            }
            void Load()
            {
                ExecutionState state = measurement.Calibration.FullRange.ExecuteLoad();
            }
        }
        public void Disconnect()
        {
            bode.ShutDown();
        }
    }
}
