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

namespace MonopolyDealClient
{
    public partial class MainWindow : Window
    {
        //Why can I only see these if they are static?
        private static ClientSocket client;
        private static int serverPort = 20000;
        private static string serverIP = "127.0.0.1";
        private static state clientState = state.initialize;
        private static byte[] dataToSend;
        private static string myName = "";
        string newText = "";
        private enum state
        {
            initialize,
            waitToBegin,
            startClient,
            transmit,
            connecting
        }

        /// <summary>
        /// Main Loop
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            // Create a timer with a two second interval.
            System.Timers.Timer aTimer = new System.Timers.Timer(10);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;

        }

        public void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (clientState == state.startClient)
            {
                //Server setup
                clientState = state.connecting;
                client = new ClientSocket(serverPort, 2, serverIP);
                dataToSend = GetBytes(myName);
                
                client.sendData(dataToSend);
                clientState = state.transmit;

            }
            else if (clientState == state.transmit)
            {
                storeAndReply();
            }
        }

        public void storeAndReply()
        {
            StreamData sd = new StreamData("");
            //byte[] storage = server.pollAndReceiveData(server.Client, sd, 10);
            byte[] storage = null;
            //if (!receivedOnce)
            //{
            storage = client.pollAndReceiveData(client.Server, sd, 2);
            // receivedOnce = true;
            //}
            if (storage.Count() > 2)
            {
                newText = GetString(storage);
                Dispatcher.BeginInvoke(new ThreadStart(() => this.display.Text += Environment.NewLine + newText));
                //display.Content = userName;
            }
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            clientState = state.startClient;
            myName = nameBlock.Text;
            nameBlock.Visibility = System.Windows.Visibility.Hidden;
            connectButton.Visibility = System.Windows.Visibility.Hidden;
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            StreamData sd = new StreamData("");
            //byte[] storage = server.pollAndReceiveData(server.Client, sd, 10);
            display.Text += Environment.NewLine + textBlock.Text;
            byte[] storage = GetBytes(textBlock.Text);
            textBlock.Clear();
            client.sendData(storage);
        }

    }
}
