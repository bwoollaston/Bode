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
        bool IsProgLoading=false;
        public Horn_Characteristic horn_Characteristic = new();
        public ObservableCollection<TaskLog> taskLogs = new();
        public ObservableCollection<Data> horn_list = new();
        public MainWindow()
        {
            InitializeComponent();

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
                Name = horn_Characteristic.horn_data.Name,
                Resfreq = horn_Characteristic.horn_data.Resfreq,
                Antifreq = horn_Characteristic.horn_data.Antifreq,
                Res_impedance = horn_Characteristic.horn_data.Res_impedance,
                Anti_impedance = horn_Characteristic.horn_data.Anti_impedance,
                Capacitance = horn_Characteristic.horn_data.Capacitance
            });
            HornData.ItemsSource = horn_list;

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

        private void Button_Click_Open(object sender, RoutedEventArgs e)
        {
            openBox.Background = new SolidColorBrush(Colors.Red);
            connectProgress.Visibility = Visibility.Visible;
            try
            {
                horn_Characteristic.OpenCal();
                openBox.Background = new SolidColorBrush(Colors.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to perform open test", "Exception Sample", MessageBoxButton.OK);
            }
            connectProgress.Visibility = Visibility.Collapsed;
        }

        private void Button_Click_Short(object sender, RoutedEventArgs e)
        {
            shortBox.Background = new SolidColorBrush(Colors.Red);
            connectProgress.Visibility = Visibility.Visible;
            try
            {
                horn_Characteristic.ShortCal();
                shortBox.Background = new SolidColorBrush(Colors.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to perform short test", "Exception Sample", MessageBoxButton.OK);
            }
            connectProgress.Visibility = Visibility.Collapsed;
        }

        private void Button_Click_Load(object sender, RoutedEventArgs e)
        {
            shortBox.Background = new SolidColorBrush(Colors.Red);
            connectProgress.Visibility = Visibility.Visible;
            try
            {
                horn_Characteristic.ShortCal();
                shortBox.Background = new SolidColorBrush(Colors.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to perform short test", "Exception Sample", MessageBoxButton.OK);
            }
            connectProgress.Visibility = Visibility.Collapsed;
        }
    }
}
