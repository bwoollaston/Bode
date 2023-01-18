using System;
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

namespace BodeGUI
{
    public partial class MainWindow : Window
    {
        string txt = "string";
        int clk = 0;
        public Horn_Characteristic horn_Characteristic = new();
        public ObservableCollection<Data> horn_list = new();
        public MainWindow()
        {
            InitializeComponent();

        }
        private void Button_Click_Run(object sender, RoutedEventArgs e)
        {
            txt = Text1.Text;
            horn_Characteristic.horn_data.Name = txt;
            //horn_Characteristic.Sweep();
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
        private void Button_Click_Cal(object sender, RoutedEventArgs e)
        {
            if(clk == 0)
            {
                openBox.Background = new SolidColorBrush(Colors.Red);
                shortBox.Background = new SolidColorBrush(Colors.Red);
                loadBox.Background = new SolidColorBrush(Colors.Red);
                calibrateBox.Foreground = new SolidColorBrush(Colors.Red);
                calInstruct.Visibility = Visibility.Visible;
                clk += 1;
            }
            else if(clk == 1)
            {
                try
                {
                    //horn_Characteristic.Open();
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
                    //horn_Characteristic.Short();
                    shortBox.Background = new SolidColorBrush(Colors.Green);
                    calInstruct.Text = "Click Calibrate when ready to perform the load test";
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
                    //horn_Characteristic.Load();
                    loadBox.Background = new SolidColorBrush(Colors.Green);
                    calInstruct.Text = "Click Calibrate when ready to perform the open test";
                    calInstruct.Visibility = Visibility.Collapsed;
                    calibrateBox.Foreground = new SolidColorBrush(Colors.Green);
                    clk = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Load calibration failed", "Exception Sample", MessageBoxButton.OK);
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        void ChildWindow_Closing(object sender, CancelEventArgs e)
        {
            Console.WriteLine("Closing");
            MessageBoxResult result = MessageBox.Show("Allow Shutdown?", "Application Shutdown Sample", MessageBoxButton.YesNo, MessageBoxImage.Question);
            e.Cancel = (result == MessageBoxResult.No);
            try
            {
                horn_Characteristic.Disconnect();
            }
            catch(Exception ex)
            {

            }
        }

        private void Button_Click_Connect(object sender, RoutedEventArgs e)
        {
            connectProgress.Visibility = Visibility.Visible;
            try
            {
                horn_Characteristic.Connect(); 
                connectProgress.Visibility = Visibility.Hidden;
                connectBox.Background = new SolidColorBrush(Colors.Green);
            }
            catch(Exception ex)
            {
                connectProgress.Visibility = Visibility.Hidden;
                MessageBox.Show("Bode not Connected", "Exception Sample", MessageBoxButton.OK);
            }

        }
    }
}
