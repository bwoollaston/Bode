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



namespace BodeGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Horn_Characteristic horn_Characteristic = new Horn_Characteristic();
        }

        private void Button_Click_Run(object sender, RoutedEventArgs e)
        {
            RunWindow runWindow = new RunWindow();
            runWindow.Show();
        }

        private void Button_Click_Cal(object sender, RoutedEventArgs e)
        {
            CalWindow calWindow = new CalWindow();
            calWindow.Show();
        }
    }
}
