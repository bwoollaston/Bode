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

namespace BodeGUI
{
    public partial class MainWindow : Window
    {
        string txt = "string";
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
            horn_Characteristic.Sweep();
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

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        void ChildWindow_Closing(object sender, CancelEventArgs e)
        {
            Console.WriteLine("Closing");
            MessageBoxResult result = MessageBox.Show("Allow Shutdown?", "Application Shutdown Sample", MessageBoxButton.YesNo, MessageBoxImage.Question);
            e.Cancel = (result == MessageBoxResult.No);
            horn_Characteristic.Disconnect();
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
