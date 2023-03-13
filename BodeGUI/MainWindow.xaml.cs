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
//using MaterialDesignThemes;
//using MaterialDesignColors;

namespace BodeGUI
{
    public partial class MainWindow : Window
    {
        string txt = "string";
        int index = 1;
        public double DataWidth
        {
            get { return ListViewDataColumn.ActualWidth; }
            set { data2width = value - 70; }
        }
        double data2width
        {
            get {return DataWidth-70; }
            set
            {
                ColumnResize(value);
            }
        }
        public bool IsProgLoading                                           //Flag indicating whether a task is in progress
        {
            get { return _isProgLoading; }
            set
            {
                _isProgLoading = value;
                ProgramLoadingUpdate();                                     //Flag triggers program loading event
            }
        }
        private bool _isProgLoading;

        /*  Variable that sets high sweep freq and gets default to start program */
        public double lowSweepFreq
        {
            get { return horn_Characteristic.sweep_LOW; }
            set
            {
                horn_Characteristic.sweep_LOW = value;
                LowFreqTextBox.Text = Convert.ToString(value);
            }
        }

        /* Variable that sets high sweep freq and gets default to start program */
        public double highSweepFreq
        {
            get { return horn_Characteristic.sweep_HIGH; }
            set
            { 
                horn_Characteristic.sweep_HIGH = value;
                HighFreqTextBox.Text = Convert.ToString(value);
            }
        }
        public double RecieverBW
        {
            get { return horn_Characteristic.Bandwidth/1000; }
            set
            {
                horn_Characteristic.Bandwidth = value * 1000;
                BandwidthTextBox.Text = Convert.ToString(value);
            }
        }
        public bool QF_Checked
        {
            get { return horn_Characteristic.IsQF_Checked; }
            set
            {
                horn_Characteristic.IsQF_Checked = value;
            }
        }
        public Dictionary<string, bool> CalStatus { get; set; }
        public Horn_Characteristic horn_Characteristic = new();             //Instance of class used to interact with bode automation interface
        public ObservableCollection<Data> horn_list = new();                //Data list to be written to window and exported to csv
        public ObservableCollection<TaskLog> taskLogs = new();
        private TaskLog reader = new();
        public bool ConnectedStatus = false;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                LowFreqTextBox.Text = lowSweepFreq.ToString();
                HighFreqTextBox.Text = highSweepFreq.ToString();
                BandwidthTextBox.Text = RecieverBW.ToString();
                CalStatus = new Dictionary<string, bool> { { "open",false},{ "short",false},{ "load",false} };
                taskLogs.Add(new TaskLog() { Name = "Initialize automation GUI", IsTaskSuccessful = true });
                TaskBlock.Text = reader.ReaderOut["Initialize"];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Initialization failed", "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Error);
                taskLogs.Add(new TaskLog() { Name = "Initialize automation GUI", IsTaskSuccessful = false });
            }

        }

        /* Runs frequency sweep and presents data in form of a table */
        private async void Button_Click_Run(object sender, RoutedEventArgs e)
        {
            txt = HornNameBox.Text;
            horn_Characteristic.horn_data.Name = txt;
            try
            {
                IsProgLoading = true;
                TaskBlock.Text = reader.ReaderOut["Run"];
                await Task.Run(() => horn_Characteristic.Sweep());
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
                Capacitance = horn_Characteristic.horn_data.Capacitance,
                QualityFactor = horn_Characteristic.horn_data.QualityFactor
            });
            HornData.ItemsSource = horn_list;
            index += 1;
            IsProgLoading=false;
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
        private async void Button_Click_Connect(object sender, RoutedEventArgs e)
        {
            try
            {
                TaskBlock.Text = reader.ReaderOut["Connecting"];
                IsProgLoading = true;
                await Task.Run(() => horn_Characteristic.Connect());
                connectBox.Background = new SolidColorBrush(Colors.Green);
                ConnectedStatus = true;
            }
            catch (Exception ex)
            {
                TaskBlock.Text = reader.ReaderOut["ConnectFailed"];
                MessageBox.Show("Bode not Connected", "Exception Sample", MessageBoxButton.OK);
                ConnectedStatus = false;
            }
            IsProgLoading = false;
        }

        /* When checking caliration selecting test button returns the magnitude of impeadance and presents on screen */
        private void click_testButton(object sender, RoutedEventArgs e)
        {
            try
            {
                IsProgLoading = true;
                double resistance = horn_Characteristic.TestCal();
                testBox.Text = resistance.ToString("000.000") + " Ω";
            }
            catch(Exception ex)
            {
                MessageBox.Show("No Bode connected", "Exception Sample", MessageBoxButton.OK);
            }
            IsProgLoading = false;
        }

        /* Exports data form horn_list to desktop directory named "bodeData" */
        private async void Export_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsProgLoading = true;
                TaskBlock.Text = reader.ReaderOut["Export"];
                string fileName = await Task.Run(() => horn_Characteristic.ExportPath());
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
            IsProgLoading = false;
        }

        private async void Button_Click_Open(object sender, RoutedEventArgs e)
        {
            openBox.Background = new SolidColorBrush(Colors.Red);
            try
            {
                IsProgLoading = true;
                TaskBlock.Text = reader.ReaderOut["Open"];
                await Task.Run(() => horn_Characteristic.OpenCal());
                openBox.Background = new SolidColorBrush(Colors.Green);
                CalStatus["open"] = true;
            }
            catch (Exception ex)
            {
                CalStatus["open"] = false;
                MessageBox.Show("Unable to perform open test", "Exception Sample", MessageBoxButton.OK);
            }
            IsProgLoading = false;
        }

        private async void Button_Click_Short(object sender, RoutedEventArgs e)
        {
            shortBox.Background = new SolidColorBrush(Colors.Red);
            try
            {
                IsProgLoading = true;
                TaskBlock.Text = reader.ReaderOut["Short"];
                await Task.Run(() => horn_Characteristic.ShortCal());
                shortBox.Background = new SolidColorBrush(Colors.Green);
                CalStatus["short"] = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to perform short test", "Exception Sample", MessageBoxButton.OK);
                CalStatus["short"] = false;
            }
            IsProgLoading = false;
        }

        private async void Button_Click_Load(object sender, RoutedEventArgs e)
        {
            loadBox.Background = new SolidColorBrush(Colors.Red);
            try
            {
                IsProgLoading = true;
                TaskBlock.Text = reader.ReaderOut["Load"];
                await Task.Run(() => horn_Characteristic.ShortCal());
                loadBox.Background = new SolidColorBrush(Colors.Green);
                CalStatus["load"] = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to perform short test", "Exception Sample", MessageBoxButton.OK);
                CalStatus["load"] = false;
            }
            IsProgLoading = false;
        }

        private void Task_Click(object sender, RoutedEventArgs e)
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
        private void ProgramLoadingUpdate()
        {
            if (IsProgLoading == true)
            {
                connectProgress.Visibility = Visibility.Visible;
                runButton.IsEnabled     = false;
                ClearButton.IsEnabled   = false;
                connectButton.IsEnabled = false;
                openButton.IsEnabled    = false;
                shortButton.IsEnabled   = false;
                loadButton.IsEnabled    = false;
                testButton.IsEnabled    = false;
                DeleteButton.IsEnabled  = false;
                TaskButton.IsEnabled    = false;
                ExportButton.IsEnabled  = false;
            }
            if (IsProgLoading == false)
            {
                UpdateComText();
                connectProgress.Visibility = Visibility.Collapsed;
                runButton.IsEnabled     = true;
                ClearButton.IsEnabled   = true;
                connectButton.IsEnabled = true;
                openButton.IsEnabled    = true;
                shortButton.IsEnabled   = true;
                loadButton.IsEnabled    = true;
                testButton.IsEnabled    = true;
                DeleteButton.IsEnabled  = true;
                TaskButton.IsEnabled    = true;
                ExportButton.IsEnabled  = true;

            }
        }

        private void UpdateComText()
        {
            if (CalStatus["open"] && CalStatus["short"] && CalStatus["load"] && ConnectedStatus == true)
            {
                TaskBlock.Text = reader.ReaderOut["Ready"];
            }
            else if (CalStatus["open"] || CalStatus["short"] || CalStatus["load"] == false && ConnectedStatus)
            {
                TaskBlock.Text = reader.ReaderOut["Calibrate"];
            }
            else if ((CalStatus["open"] && CalStatus["short"] && ConnectedStatus) == true && CalStatus["load"] == false)
            {
                TaskBlock.Text = reader.ReaderOut["CalLoad"];
            }

        }
        private void LowFreqTextBox_LostFocus_1(object sender, RoutedEventArgs e)
        {
            lowSweepFreq = Convert.ToDouble(LowFreqTextBox.Text);
            if (lowSweepFreq >= highSweepFreq)
            {
                MessageBox.Show("High sweep frequency must be greater than low sweep frequency", "Exception Sample", MessageBoxButton.OK);
                lowSweepFreq = highSweepFreq - 100;
            }

        }

        private void HighFreqTextBox_LostFocus_1(object sender, RoutedEventArgs e)
        {
            highSweepFreq = Convert.ToDouble(HighFreqTextBox.Text);
            if (highSweepFreq <= lowSweepFreq)
            {
                MessageBox.Show("High sweep frequency must be greater than low sweep frequency", "Exception Sample", MessageBoxButton.OK);
                highSweepFreq = lowSweepFreq + 100;
            }
        }

        private void LowFreqTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) LowFreqTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void HighFreqTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) HighFreqTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void QFCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ColumnResize(data2width);
        }
        private void ColumnResize(double value)
        { 
                if (QFCheckBox.IsChecked == true)
                {
                    CapColumn.Width = value / 6;
                    ResColumn.Width = value / 6;
                    AntiColumn.Width = value / 6;
                    ImpColumn.Width = value / 6;
                    AntiImpColumn.Width = value / 6;
                    QFColumn.Width = value / 6;
                }
                else
                {
                    CapColumn.Width = value / 5;
                    ResColumn.Width = value / 5;
                    AntiColumn.Width = value / 5;
                    ImpColumn.Width = value / 5;
                    AntiImpColumn.Width = value / 5;
                    QFColumn.Width = 0;
                }
        
        }

        private void HornData_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DataWidth = ListViewDataColumn.ActualWidth;
        }

        private void BandwidthTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            RecieverBW = Convert.ToDouble(BandwidthTextBox.Text);
        }
    }
}
