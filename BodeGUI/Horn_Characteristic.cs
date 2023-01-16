﻿using System;
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
        public string Name = "name";
        public double resfreq;
        public double antifreq;
        public double res_impedance;
        public double anti_impedance;
        public double capacitance;
    }
    public class Horn_Characteristic
    {
        OnePortMeasurement measurement;
        BodeDevice bode;
        ExecutionState state;
        BodeAutomationInterface auto = new BodeAutomation();
        public Data horn_data = new();

        public Horn_Characteristic()
        {
            bode = auto.Connect();
            measurement = bode.Impedance.CreateOnePortMeasurement();
        }

/*        public void Connect()
        {
            bode = auto.Connect();
            measurement = bode.Impedance.CreateOnePortMeasurement();
        }*/
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
            horn_data.res_impedance = impedance[0];
            horn_data.anti_impedance = impedance[0];
            for(int i = 0; i < impedance.Length;i++)
            {
                if (impedance[i] < horn_data.res_impedance)
                {
                    horn_data.res_impedance = impedance[i];
                    horn_data.resfreq = freq[i];
                }
                if (impedance[i] > horn_data.anti_impedance)
                {
                    horn_data.anti_impedance = impedance[i];
                    horn_data.antifreq = freq[i];
                }
            }
            measurement.ConfigureSinglePoint(1000);
            if (state != ExecutionState.Ok)
            {
                bode.ShutDown();
                return;
            }
            double[] cap = measurement.Results.Cs();
            horn_data.capacitance = cap[0];
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
