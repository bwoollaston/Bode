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
            //horn_Characteristic.Sweep();
            horn_list.Add(new Data()
            {
                Name = horn_Characteristic.horn_data.Name,
                resfreq = horn_Characteristic.horn_data.resfreq,
                antifreq = horn_Characteristic.horn_data.antifreq,
                res_impedance = horn_Characteristic.horn_data.res_impedance,
                anti_impedance = horn_Characteristic.horn_data.anti_impedance,
                capacitance = horn_Characteristic.horn_data.capacitance
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
            horn_Characteristic.Connect();
            connectProgress.Visibility = Visibility.Hidden;
            connectBox.Background = new SolidColorBrush(Colors.Green);
        }
    }
}
