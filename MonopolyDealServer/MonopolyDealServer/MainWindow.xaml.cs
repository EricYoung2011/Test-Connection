//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

using System;
using SocketDLL;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Timers;

namespace MonopolyDealServer
{
    public partial class MainWindow : Window
    {
        //Why can I only see these if they are static?
        //private static ServerSocket server;
        //private static ServerSocket server2;
        private static ServerSocket server;
        private static int numOfPlayers = 1;
        private static int serverPort = 50501;
        //private static int serverPort2 = 20001;
        //private static string serverIP = "fe80::19b2:4398:1cda:e641%4";
        private static string serverIP = "68.53.59.162";
        //private static string serverIP = "";
        //private static string serverIP = "10.0.0.13";
        private static state serverState = state.initialize;
        private static string dataToSend = "";
        private static string myName = "";
        private static string userName = "";
        private static string toSend = "";
        System.Timers.Timer aTimer = new System.Timers.Timer(10);
        private enum state
        {
            initialize,
            startServer,
            transmit,
            connecting
        }

        /// <summary>
        /// Main Loop
        /// </summary>
        public MainWindow()
        {
           
            InitializeComponent();
            //display.Visibility = System.Windows.Visibility.Hidden;
            // Create a timer with a two second interval.
            
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;

        }

        public void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            aTimer.Enabled = false;
            if (serverState == state.initialize)
            {
                initializeServers();
                serverState = state.startServer;
            }
            if (serverState == state.startServer)
            {
                //Server setup
                serverState = state.connecting;
                server.start();
                serverState = state.transmit;

            }
            if (serverState == state.transmit)
            {
                storeAndReply();
            }
            aTimer.Enabled = true;
        }

        //public static bool receivedOnce = false;
        public void storeAndReply()
        {
            StreamData sd = new StreamData("");
            //byte[] storage = server.pollAndReceiveData(server.Client, sd, 10);
            byte[] storage = null;
            //if (!receivedOnce)
            //{
                storage = server.pollAndReceiveData(server.Client, sd, 2);
               // receivedOnce = true;
            //}
            if (storage.Count() > 2)
            {
                userName = GetString(storage);
                Dispatcher.BeginInvoke(new ThreadStart(() => this.display.Text += Environment.NewLine + userName)); 
                //display.Content = userName;
            }
            //if(
            //{
            //    //Store data 
            //    string toDisplay = sd.ToString();
            //    display.Text += Environment.NewLine + toDisplay;
            //    display.ScrollToEnd();
            //    //Send eye data
            //    dataToSend = "Hello Buddy";
            //    sd = new StreamData(myName);
            //    server.sendData(server.Client, sd);
            //}
        }

        private void initializeServers()
        {
            server = new ServerSocket(serverPort, 2, serverIP);
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[(bytes.Length / sizeof(char))];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            StreamData sd = new StreamData("");
            //byte[] storage = server.pollAndReceiveData(server.Client, sd, 10);
            display.Text += Environment.NewLine + textBlock.Text;
            byte[] storage = GetBytes(textBlock.Text);
            textBlock.Clear();
            server.sendData(server.Client,storage) ;
        }

    }
}