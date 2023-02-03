using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.IO.Ports;
using System.Threading;

namespace SDmanager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Compilare combobox con "string[] ports = SerialPort.GetPortNames()" per scegliere le porte;

        static SerialPort port = new SerialPort();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!port.IsOpen)
            {
                string portname = COMlist.Text;
                int baudrate = int.Parse(BaudRateList.Text);

                try
                {
                    // open port and wait for Arduino to boot
                    port.PortName = portname;
                    port.BaudRate = baudrate;
                    port.Open();
                    port.NewLine = "\n";
                    port.DtrEnable = true;
                    port.RtsEnable = true;

                    LogBox.Text += "Port Open\n\n";
                }
                catch (Exception ex)
                {
                    LogBox.Text += $"Cannot open serial port.\n (PortName={portname} BaudRate={baudrate})\n\n";
                    LogBox.Text += ex.ToString();
                    return;
                }
            }
            else
            {
                LogBox.Text += $"Already connected to serial port.\n (PortName={port.PortName} BaudRate={port.BaudRate})\n\n";
            }
        }

        private async void SDListButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Compile Test List
                SDList.Items.Clear();
                SDList.Items.Add("Test0");
                SDList.Items.Add("Test1");
                SDList.Items.Add("Test2");
                // End Test List

                string command = "M20";
                port.WriteLine(command);               
                LogBox.Text += $"Sending \"{command}\"\n\n";

                string multilines = PortReader();
                multilines = multilines.Replace("\r\n", "\n");

                string[] lines = multilines.Split("\n");

                SDList.Items.Clear();
                
                bool FileList = false;
                foreach(string line in lines)
                {
                    if (line == "End file list") FileList = false;

                    if (FileList) SDList.Items.Add(line);

                    if (line == "Begin file list") FileList = true;
                }
            }
            catch (Exception ex)
            {
                LogBox.Text += $"{ex.ToString()}\n\n";
            }
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SDList.SelectedValue == null)
            {
                return;
            }
            if (MessageBox.Show($"Do you want to remove {SDList.SelectedValue}?", "Confirm operation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }
            else
            {
                try
                {
                    string command = $"M30 {SDList.SelectedValue.ToString().Split(' ')[0]}";
                    port.WriteLine(command);
                    LogBox.Text += $"Sending \"{command}\"\n\n";

                    string multilines = PortReader();

                    SDListButton_Click(sender,e);
                }
                catch (Exception ex)
                {
                    LogBox.Text += $"{ex.ToString()}\n\n";
                }
            }
        }
        private string PortReader()
        {
            string ReadMessage = "";

            LogBox.Text += "Wait for response\n\n";
            Thread.Sleep(1000);
            LogBox.Text += "Start reading\n\n";

            while (port.BytesToRead > 0)
            {
                ReadMessage += port.ReadExisting();
            }
            
            LogBox.Text += ReadMessage;

            return ReadMessage;
        }

        private void ScrollDown(object sender, TextChangedEventArgs e)
        {
            LogBox.ScrollToEnd();
        }

        private void COMlist_Drop(object sender, EventArgs e)
        {
            COMlist.Items.Clear();

            string[] ports = SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                COMlist.Items.Add(port);
            }

            COMlist.SelectedIndex= 0;
        }
    }
}
