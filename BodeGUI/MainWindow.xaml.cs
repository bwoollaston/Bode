﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using CsvHelper;
using System.Globalization;
using System.Diagnostics;
using MaterialDesignThemes;

namespace BodeGUI
{
    public partial class MainWindow : Window
    {
        string txt = "string";
        int clk = 0;
        bool IsProgLoading=false;
        public Horn_Characteristic horn_Characteristic = new();
        public ObservableCollection<Data> horn_list = new();
        public MainWindow()
        {
            InitializeComponent();

        }

        /* Runs frequency sweep and presents data in form of a table */
        private void Button_Click_Run(object sender, RoutedEventArgs e)
        {
            txt = Text1.Text;
            horn_Characteristic.horn_data.Name = txt;

            try
            {
                horn_Characteristic.Sweep();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bode not connected", "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            horn_list.Add(new Data()
            {
                Name = horn_Characteristic.horn_data.Name,
                Resfreq = horn_Characteristic.horn_data.Resfreq,
                Antifreq = horn_Characteristic.horn_data.Antifreq,
                Res_impedance = horn_Characteristic.horn_data.Res_impedance,
                Anti_impedance = horn_Characteristic.horn_data.Anti_impedance,
                Capacitance = horn_Characteristic.horn_data.Capacitance
            });
            HornData.ItemsSource = horn_list;

        }

        /* Button click initializes the calibration process, iterates through the 3 cal steps and returns to original state */
        private void Button_Click_Cal(object sender, RoutedEventArgs e)
        {
            if(clk == 0)
            {
                openBox.Background = new SolidColorBrush(Colors.Red);
                shortBox.Background = new SolidColorBrush(Colors.Red);
                loadBox.Background = new SolidColorBrush(Colors.Red);
                calibrateBox.Background = new SolidColorBrush(Colors.Red);
                calInstruct.Visibility = Visibility.Visible;
                clk += 1;
            }
            else if(clk == 1)
            {
                try
                {
                    horn_Characteristic.Open();
                    openBox.Background = new SolidColorBrush(Colors.Green);
                    calInstruct.Text = "Click Calibrate when ready to perform the short test";
                    clk += 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Open calibration failed", "Exception Sample", MessageBoxButton.OK);
                }
            }
            else if (clk == 2)
            {
                try
                {
                    horn_Characteristic.Short();
                    shortBox.Background = new SolidColorBrush(Colors.Green);
                    calInstruct.Text = "Click Calibrate when ready to perform the load test with 50Ω test load";
                    clk += 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Short calibration failed", "Exception Sample", MessageBoxButton.OK);
                }
            }
            else if (clk == 3)
            {
                try
                {
                    horn_Characteristic.Load();
                    loadBox.Background = new SolidColorBrush(Colors.Green);
                    calInstruct.Text = "Click Calibrate when ready to perform the open test";
                    calInstruct.Visibility = Visibility.Collapsed;
                    calibrateBox.Background = new SolidColorBrush(Colors.Green);
                    clk = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Load calibration failed", "Exception Sample", MessageBoxButton.OK);
                }
            }
        }

        /* When exiting window promts user before disconnecting the bode device */
        void ChildWindow_Closing(object sender, CancelEventArgs e)
        {
            Console.WriteLine("Closing");
            MessageBoxResult result = MessageBox.Show("Allow Shutdown?", "Application Shutdown Sample", MessageBoxButton.YesNo, MessageBoxImage.Question);
            e.Cancel = (result == MessageBoxResult.No);
            horn_Characteristic.Disconnect();
        }

        /* Searches for and connects to first availible bode100 else presents error */
        private void Button_Click_Connect(object sender, RoutedEventArgs e)
        {
            connectProgress.Visibility = Visibility.Visible;
            try
            {
                IsProgLoading = true;
                horn_Characteristic.Connect(); 
                connectProgress.Visibility = Visibility.Hidden;
                connectBox.Background = new SolidColorBrush(Colors.Green);
                IsProgLoading = false;
            }
            catch(Exception ex)
            {
                connectProgress.Visibility = Visibility.Hidden;
                MessageBox.Show("Bode not Connected", "Exception Sample", MessageBoxButton.OK);
            }

        }

        /* When checking caliration selecting test button returns the magnitude of impeadance and presents on screen */
        private void click_testButton(object sender, RoutedEventArgs e)
        {
            try
            {
                horn_Characteristic.TestCal();
                double resistance = horn_Characteristic.horn_data.Resistance;
                testBox.Text = resistance.ToString("000.0") + " Ω";
            }
            catch(Exception ex)
            {
                MessageBox.Show("No Bode connected", "Exception Sample", MessageBoxButton.OK);
            }

        }

        /* Exports data form horn_list to desktop directory named "bodeData" */
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fileName = horn_Characteristic.ExportPath();
                using (var writer = new StreamWriter(fileName))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(horn_list);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to write csv", "Exception Sample", MessageBoxButton.OK);
            }

        }

    }
}
