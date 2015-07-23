//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

using System;
using SocketDLL;
using System.Threading;
using System.Collections;
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
using System.Net;
using System.Timers;

namespace MonopolyDealClient
{
    public enum Card
    {
        One,
        Two,
        Three,
        Four,
        Five,
        Ten,
        PassGo__1,
        DoubleTheRent__1,
        Birthday__2,
        House__3,
        Hotel__4,
        JustSayNo__4,
        DealBreaker__5,
        SlyDeal__3,
        ForcedDeal__3,
        DebtCollector__3,
        Rent,
        RentWild__3,
        RentBrownLightBlue__1,
        RentRedYellow__1,
        RentGreenBlue__1,
        RentPinkOrange__1,
        RentBlackUtility__1,
        PropertyBrown__1,
        PropertyUtility__1,
        PropertyBlue__4,
        PropertyLightBlue__1,
        PropertyPink__2,
        PropertyOrange__2,
        PropertyRed__3,
        PropertyYellow__3,
        PropertyGreen__4,
        PropertyBlack__2,
        PropertyWild,
        PropertyRedYellow__3,
        PropertyYellowRed__3,
        PropertyPinkOrange__2,
        PropertyOrangePink__2,
        PropertyLightBlueBlack__4,
        PropertyBlackLightBlue__4,
        PropertyUtilityBlack__2,
        PropertyBlackUtility__2,
        PropertyBrownLightBlue__1,
        PropertyLightBlueBrown__1,
        PropertyBlueGreen__4,
        PropertyGreenBlue__4,
        PropertyGreenBlack__4,
        PropertyBlackGreen__4,
        PropertyWildBrown,
        PropertyWildUtility,
        PropertyWildBlue,
        PropertyWildLightBlue,
        PropertyWildPink,
        PropertyWildOrange,
        PropertyWildRed,
        PropertyWildYellow,
        PropertyWildGreen,
        PropertyWildBlack
    }

    public enum CardType
    {
        Action,
        Money,
        Property,
        Error
    }

    public enum PropertyType
    {
        Normal,
        Duo,
        Wild,
    }
    public partial class MainWindow : Window
    {
        private static ClientSocket client;
        private static int serverPort = 50501;
        private static int clientPort = 50500;
        private static string serverIP;
        private static string clientIP;
        private static int numOfPlayers;
        private static int playerNum;
        private static int playNum;
        private static int numCardsInDeck;
        private static List<List<Card>> AllHands;
        private static List<List<Card>> AllTableMoney;
        private static List<List<List<Card>>> AllTableProperties;
        private static List<string> playerNames;
        private static string universalPrompt;
        private static string individualPrompt;

        private static string myName;
        private static Hashtable message;
        private static byte[] dataToSend;
        private static state clientState = state.initialize;
        private enum state
        {
            initialize,
            connectToServer,
            getNewPort,
            transmit,
        }

        /// <summary>
        /// Main Loop
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            System.Timers.Timer aTimer = new System.Timers.Timer(100);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;

            //StreamData sd = new StreamData("");
            //byte[] storage = GetBytes(textBlock.Text);
            //client.sendData(storage);

        }

        public void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new ThreadStart(() => this.timerDing()));
            Thread.CurrentThread.Abort();
        }

        public void timerDing()
        {
            if (clientState == state.connectToServer)
            {
                client = new ClientSocket(serverPort, 2, clientPort, clientIP, serverIP);
                clientState = state.getNewPort;
            }

            if (clientState == state.getNewPort)
            {
                byte[] storage = null;
                storage = client.pollAndReceiveData(client.Server, 2);
                if (storage.Count() > 2)
                {
                    serverPort = Int32.Parse(GetString(storage));
                    client.stop();
                    Thread.Sleep(100);
                    client = new ClientSocket(serverPort, 2, clientPort, clientIP, serverIP);
                    storage = GetBytes(myName);
                    client.sendData(storage);
                    initializeDisplay();
                    clientState = state.transmit;
                }
            }
            if (clientState == state.transmit)
            {
                checkForMessages();
            }
        }

        public void initializeDisplay()
        {
            playerDisplay myDisplay = new playerDisplay();
            myDisplay.button1.Click += button1_Click;
            myDisplay.button2.Click += button2_Click;
            myDisplay.button3.Click += button3_Click;
            myDisplay.buttonBack.Click += buttonBack_Click;
            myDisplay.Table_Properties.SelectionChanged +=Table_Properties_SelectionChanged;
            myDisplay.Table_Properties.MouseDoubleClick +=Table_Properties_MouseDoubleClick;
            myDisplay.Table_Money.SelectionChanged +=Table_Money_SelectionChanged;
            myDisplay.Hand.MouseDoubleClick +=Hand_MouseDoubleClick;
            myDisplay.Show();
            this.Hide();
        }

        public static void resizeLayout()
        {

        }

        public void checkForMessages()
        {
            byte[] storage = null;
            storage = client.pollAndReceiveData(client.Server, 2);
            if (storage.Count() > 2)
            {
                string receivedMessage = GetString(storage);
                message = (Hashtable)JsonUtilities.DeserializeObjectFromJSON(receivedMessage, message.GetType());
                AllHands = (List<List<Card>>)message["AllHands"];
                AllTableMoney = (List<List<Card>>)message["AllTableMoney"];
                numCardsInDeck = (int)message["NumCardsInDeck"];
                numOfPlayers = (int)message["NumOfPlayers"];
                playerNum = (int)message["PlayerNum"];
                AllTableProperties = (List<List<List<Card>>>)message["AllTableProperties"];
                playerNames = (List<string>)message["PlayerNames"];
                playNum = (int)message["PlayNum"];
                universalPrompt = (string)message["NewUniversalPrompt"];
                individualPrompt = (string)message["IndividualPrompt"];


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
            serverIP = serverIPBlock.Text;
            clientIP = myIPBlock.Text;
            myName = nameBlock.Text;
            clientState = state.connectToServer;
        }

        public void button1_Click(object sender, RoutedEventArgs e)
        {
            
        }

        public void button2_Click(object sender, RoutedEventArgs e)
        {
            
        }

        public void button3_Click(object sender, RoutedEventArgs e)
        {
            
        }

        public void buttonBack_Click(object sender, RoutedEventArgs e)
        {
        }

        public static void Table_Properties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        public static void Table_Money_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        public static void Table_Properties_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           
        }

        public static void Hand_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           
        }

        public static void OtherPlayer_Click(object sender, RoutedEventArgs e)
        {
            
        }

        public static void OtherPlayer_Properties_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }

        public static void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            resizeLayout();
        }
    }
}
