using System;
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
using MaterialDesignColors;

namespace BodeGUI
{
    public partial class MainWindow : Window
    {
        string txt = "string";
        int clk = 0;
        int index = 1;
        public bool IsProgLoading                                           //Flag indicating whether a task is in progress
        {
            get { return _isProgLoading; }
            set
            {
                _isProgLoading = value;
                programLoadingUpdate();
            }
        }
        private bool _isProgLoading;
        public Horn_Characteristic horn_Characteristic = new();             //Instance of class used to interact with bode automation interface
        //public ObservableCollection<TaskLog> taskLogs = new();              //Log of successful and unsuccessful tasks
        public ObservableCollection<Data> horn_list = new();                //Data list to be written to window and exported to csv
        public string ComText
        {
            get { return _comText; }
            set
            {
                _comText = value;
                UpdateComText();
            }
        }
        private string _comText;
        public MainWindow()
        {
            ComText = "Text";
            InitializeComponent();
            this.DataContext = this;
        }

        /* Runs frequency sweep and presents data in form of a table */
        private void Button_Click_Run(object sender, RoutedEventArgs e)
        {
            txt = HornNameBox.Text;
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
                //Index = horn_Characteristic.horn_data.Index,
                Name = horn_Characteristic.horn_data.Name,
                Resfreq = horn_Characteristic.horn_data.Resfreq,
                Antifreq = horn_Characteristic.horn_data.Antifreq,
                Res_impedance = horn_Characteristic.horn_data.Res_impedance,
                Anti_impedance = horn_Characteristic.horn_data.Anti_impedance,
                Capacitance = horn_Characteristic.horn_data.Capacitance
            });
            HornData.ItemsSource = horn_list;
            index += 1;

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
            try
            {
                IsProgLoading = true;
                horn_Characteristic.Connect(); 
                connectBox.Background = new SolidColorBrush(Colors.Green);
            }
            catch(Exception ex)
            {
                connectBox.Background = new SolidColorBrush(Colors.Red);
                MessageBox.Show("Bode not Connected", "Exception Sample", MessageBoxButton.OK);
            }
            IsProgLoading = false;
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

        private void Button_Click_Open(object sender, RoutedEventArgs e)
        {
            openBox.Background = new SolidColorBrush(Colors.Red);
            try
            {
                horn_Characteristic.OpenCal();
                openBox.Background = new SolidColorBrush(Colors.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to perform open test", "Exception Sample", MessageBoxButton.OK);
            }
        }

        private void Button_Click_Short(object sender, RoutedEventArgs e)
        {
            shortBox.Background = new SolidColorBrush(Colors.Red);
            try
            {
                horn_Characteristic.ShortCal();
                shortBox.Background = new SolidColorBrush(Colors.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to perform short test", "Exception Sample", MessageBoxButton.OK);
            }
        }

        private void Button_Click_Load(object sender, RoutedEventArgs e)
        {
            shortBox.Background = new SolidColorBrush(Colors.Red);
            try
            {
                horn_Characteristic.ShortCal();
                shortBox.Background = new SolidColorBrush(Colors.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to perform short test", "Exception Sample", MessageBoxButton.OK);
            }
        }

        private void Task_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EventInProgress()
        {

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            while(HornData.SelectedItems.Count > 0)
            {
                horn_list.Remove((Data)HornData.SelectedItem);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to clear data?", "Application Shutdown Sample", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)                     //check to make sure the user really wants to clear data
                {
                    horn_list.Clear();
                    index = 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Clear Failed", "Exception Sample", MessageBoxButton.OK);
            }
        }
        private void programLoadingUpdate()
        {
            if (IsProgLoading == true)
            {
                runButton.IsEnabled     = false;
                ClearButton.IsEnabled   = false;
                connectButton.IsEnabled = false;
                openButton.IsEnabled    = false;
                shortButton.IsEnabled   = false;
                loadButton.IsEnabled    = false;
                testButton.IsEnabled    = false;
            }
            if (IsProgLoading == false)
            {
                runButton.IsEnabled     = true;
                ClearButton.IsEnabled   = true;
                connectButton.IsEnabled = true;
                openButton.IsEnabled    = true;
                shortButton.IsEnabled   = true;
                loadButton.IsEnabled    = true;
                testButton.IsEnabled    = true;
            }

        }
        private void UpdateComText()
        {
            //TaskBlock.Text = "Enter Text Here";
        }

    }
}
