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
using Newtonsoft.Json;

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
        public static int serverPort = 50501;
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
        public static List<ListBox> otherTable_Money = new List<ListBox>();
        public static List<ListBox> otherTable_Properties = new List<ListBox>();
        public static List<Button> otherNames = new List<Button>();
        public static List<TextBox> otherMoneyLabels = new List<TextBox>();
        public static List<TextBox> otherPropertyLabels = new List<TextBox>();
        public static List<TextBox> otherCardsLeftText = new List<TextBox>();
        public static List<TextBox> otherCardsLeft = new List<TextBox>();
        public static List<TextBox> otherTurnsLeftText = new List<TextBox>();
        public static List<TextBox> otherTurnsLeft = new List<TextBox>();
        private static List<string> playerNames;
        private static string universalPrompt;
        private static string individualPrompt;
        private static playerDisplay myDisplay;
        private static string myName;
        private static int myPlayerNum;
        private static Hashtable messageToReceive;
        private static Hashtable messageToSend;
        private static byte[] dataToSend;
        private static state clientState = state.initialize;
        private gameState curGameState;
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
            Dispatcher.BeginInvoke(new ThreadStart(() => timerDing()));
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
                //Hashtable tempStorage = new Hashtable();
                //tempStorage.Add("ServerPort", 0);
                //tempStorage.Add("NumOfPlayers", 0);
                //tempStorage.Add("MyPlayerNum", 0);
                storage = client.pollAndReceiveData(client.Server, 2);
                if (storage.Count() > 2)
                {
                    //string tempString = GetString(storage);
                    //string tempString = Encoding.ASCII.GetString(storage);
                    //tempStorage = JsonConvert.DeserializeObject<Hashtable>(tempString);
                    //var helpMe = tempStorage["ServerPort"];
                    //serverPort = Int32.Parse(helpMe.ToString());
                    //Console.Write(helpMe);
                    //numOfPlayers = (int)tempStorage["NumOfPlayers"];
                    //myPlayerNum = (int)tempStorage["MyPlayerNum"];

                    string tempString = GetString(storage);
                    //gameState newGameState = (gameState)JsonUtilities.DeserializeObjectFromJSON(tempString, curGameState.GetType());

                    gameState newGameState = Newtonsoft.Json.JsonConvert.DeserializeObject<gameState>(tempString);
                    serverPort = newGameState.serverPort;
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
            myDisplay = new playerDisplay();
            myDisplay.button1.Click += button1_Click;
            myDisplay.button2.Click += button2_Click;
            myDisplay.button3.Click += button3_Click;
            myDisplay.buttonBack.Click += buttonBack_Click;
            myDisplay.Table_Properties.SelectionChanged +=Table_Properties_SelectionChanged;
            myDisplay.Table_Properties.MouseDoubleClick +=Table_Properties_MouseDoubleClick;
            myDisplay.Table_Money.SelectionChanged +=Table_Money_SelectionChanged;
            myDisplay.Hand.MouseDoubleClick +=Hand_MouseDoubleClick;

            for (int j = 0; j < (numOfPlayers); j++)
            {
                otherMoneyLabels.Add(new TextBox());
                otherNames.Add(new Button());
                otherPropertyLabels.Add(new TextBox());
                otherTable_Money.Add(new ListBox());
                otherTable_Properties.Add(new ListBox());
                otherCardsLeft.Add(new TextBox());
                otherCardsLeftText.Add(new TextBox());
                otherTurnsLeft.Add(new TextBox());
                otherTurnsLeftText.Add(new TextBox());
            }
            
            double totalWidth = myDisplay.window.RenderSize.Width;
            double totalHeight = myDisplay.window.RenderSize.Height;
            double boxWidth = 7 * totalWidth / 48;
            double boxHeight = totalHeight / 4;
            myDisplay.grid.Width = totalWidth;
            myDisplay.grid.Height = totalHeight;
            myDisplay.grid.Margin = new Thickness(0, 0, 0, 0);
            myDisplay.Hand.Width = boxWidth;
            myDisplay.Table_Money.Width = boxWidth;
            myDisplay.Table_Properties.Width = boxWidth;
            myDisplay.Hand.Margin = new Thickness((totalWidth / 2 - 3 * boxWidth) / 6, totalHeight / 8, 0, 0);
            myDisplay.Table_Money.Margin = new Thickness(boxWidth + ((totalWidth / 2 - 3 * boxWidth) / 2), totalHeight / 8, 0, 0);
            myDisplay.Table_Properties.Margin = new Thickness(2 * boxWidth + 5 * (totalWidth / 2 - 3 * boxWidth) / 6, totalHeight / 8, 0, 0);
            myDisplay.handLabel.Width = boxWidth;
            myDisplay.handLabel.Height = totalHeight / 24;
            myDisplay.handLabel.Margin = new Thickness((totalWidth / 2 - 3 * boxWidth) / 6, 7 * totalHeight / 96, 0, 0);
            myDisplay.moneyLabel.Width = boxWidth;
            myDisplay.moneyLabel.Height = totalHeight / 24;
            myDisplay.moneyLabel.Margin = new Thickness(boxWidth + ((totalWidth / 2 - 3 * boxWidth) / 2), 7 * totalHeight / 96, 0, 0);
            myDisplay.propertiesLabel.Width = boxWidth;
            myDisplay.propertiesLabel.Height = totalHeight / 24;
            myDisplay.propertiesLabel.Margin = new Thickness(2 * boxWidth + 5 * (totalWidth / 2 - 3 * boxWidth) / 6, 7 * totalHeight / 96, 0, 0);
            myDisplay.buttonPlayer.Width = totalWidth / 16;
            myDisplay.buttonPlayer.Height = 5 * totalHeight / 96;
            myDisplay.buttonPlayer.Margin = new Thickness((totalWidth / 2 - 3 * boxWidth) / 6, totalHeight / 96, 0, 0);
            myDisplay.Prompt.Width = 10 * totalWidth / 32;
            myDisplay.Prompt.Height = 5 * totalHeight / 96;
            myDisplay.Prompt.Margin = new Thickness(4 * totalWidth / 48, totalHeight / 96, 0, 0);
            myDisplay.cardsLeftText.Width = 4 * totalWidth / 64;
            myDisplay.cardsLeftText.Height = 2 * totalHeight / 96;
            myDisplay.cardsLeftText.Margin = new Thickness(39 * totalWidth / 96, totalHeight / 96, 0, 0);
            myDisplay.deckCountDisplay.Width = 2 * totalWidth / 64;
            myDisplay.deckCountDisplay.Height = 2 * totalHeight / 96;
            myDisplay.deckCountDisplay.Margin = new Thickness(30 * totalWidth / 64, totalHeight / 96, 0, 0);
            myDisplay.turnsLeftText.Width = 4 * totalWidth / 64;
            myDisplay.turnsLeftText.Height = 2 * totalHeight / 96;
            myDisplay.turnsLeftText.Margin = new Thickness(39 * totalWidth / 96, 4 * totalHeight / 96, 0, 0);
            myDisplay.turnsLeftDisplay.Width = 2 * totalWidth / 64;
            myDisplay.turnsLeftDisplay.Height = 2 * totalHeight / 96;
            myDisplay.turnsLeftDisplay.Margin = new Thickness(30 * totalWidth / 64, 4 * totalHeight / 96, 0, 0);
            myDisplay.buttonBack.Width = 4 * totalWidth / 64;
            myDisplay.buttonBack.Height = 6 * totalHeight / 96;
            myDisplay.buttonBack.Margin = new Thickness(totalWidth / 96, 37 * totalHeight / 96, 0, 0);
            myDisplay.button1.Width = 8 * totalWidth / 64;
            myDisplay.button1.Height = 6 * totalHeight / 96;
            myDisplay.button1.Margin = new Thickness(8 * totalWidth / 96, 37 * totalHeight / 96, 0, 0);
            myDisplay.button2.Width = 8 * totalWidth / 64;
            myDisplay.button2.Height = 6 * totalHeight / 96;
            myDisplay.button2.Margin = new Thickness(21 * totalWidth / 96, 37 * totalHeight / 96, 0, 0);
            myDisplay.button3.Width = 8 * totalWidth / 64;
            myDisplay.button3.Height = 6 * totalHeight / 96;
            myDisplay.button3.Margin = new Thickness(34 * totalWidth / 96, 37 * totalHeight / 96, 0, 0);
            myDisplay.universalPrompt.Width = 46 * totalWidth / 96;
            myDisplay.universalPrompt.Height = 46 * totalHeight / 96;
            myDisplay.universalPrompt.Margin = new Thickness(48 * totalWidth / 96, totalHeight / 96, 0, 0);

            int otherPlayers = 0;
            for (int player2 = 0; player2 < numOfPlayers; player2++)
            {
                double otherBoxWidth = totalWidth / ((numOfPlayers - 1) * 2 + 1);
                otherBoxWidth = Math.Min(otherBoxWidth, boxWidth);
                double otherBoxHeight = totalHeight / 4;
                double otherMargin = (totalWidth - otherBoxWidth * 2 * (numOfPlayers - 1)) / (3 * (numOfPlayers - 1));
                if (player2 != myPlayerNum)
                {
                    myDisplay.grid.Children.Add(otherTable_Properties[player2]);
                    otherTable_Properties[player2].Width = otherBoxWidth;
                    otherTable_Properties[player2].Height = otherBoxHeight;
                    otherTable_Properties[player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    otherTable_Properties[player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    otherTable_Properties[player2].Margin = new Thickness(otherMargin * (2 + 3 * otherPlayers) + otherBoxWidth * (2 * otherPlayers + 1), 5 * totalHeight / 8, 0, 0);
                    otherTable_Properties[player2].Name = playerNames[player2];
                    //otherTable_Properties[player2].MouseDoubleClick += myDisplay[player2].OtherPlayer_Properties_MouseDoubleClick;
                    myDisplay.grid.Children.Add(otherTable_Money[player2]);
                    otherTable_Money[player2].Width = otherBoxWidth;
                    otherTable_Money[player2].Height = otherBoxHeight;
                    otherTable_Money[player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    otherTable_Money[player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    otherTable_Money[player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1) + 2 * otherBoxWidth * otherPlayers, 5 * totalHeight / 8, 0, 0);
                    myDisplay.grid.Children.Add(otherNames[player2]);
                    otherNames[player2].Height = 5 * totalHeight / 96;
                    otherNames[player2].Width = otherBoxWidth;//2 * otherBoxWidth + otherMargin;
                    otherNames[player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    otherNames[player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    otherNames[player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1.5) + otherBoxWidth * (2 * otherPlayers + 0.5), 49 * totalHeight / 96, 0, 0);
                    otherNames[player2].Content = playerNames[player2];
                    otherNames[player2].HorizontalContentAlignment = HorizontalAlignment.Center;
                    otherNames[player2].VerticalContentAlignment = VerticalAlignment.Center;
                    otherNames[player2].Background = Brushes.DarkRed;
                    //otherNames[player2].Click += myDisplay[player2].OtherPlayer_Click;
                    myDisplay.grid.Children.Add(otherMoneyLabels[player2]);
                    otherMoneyLabels[player2].Height = totalHeight / 24;
                    otherMoneyLabels[player2].Width = otherBoxWidth;
                    otherMoneyLabels[player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    otherMoneyLabels[player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    otherMoneyLabels[player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1) + 2 * otherBoxWidth * otherPlayers, 55 * totalHeight / 96, 0, 0);
                    otherMoneyLabels[player2].Text = "Money:";
                    otherMoneyLabels[player2].TextAlignment = TextAlignment.Center;
                    myDisplay.grid.Children.Add(otherPropertyLabels[player2]);
                    otherPropertyLabels[player2].Height = totalHeight / 24;
                    otherPropertyLabels[player2].Width = otherBoxWidth;
                    otherPropertyLabels[player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    otherPropertyLabels[player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    otherPropertyLabels[player2].Margin = new Thickness(otherMargin * (2 + 3 * otherPlayers) + otherBoxWidth * (2 * otherPlayers + 1), 55 * totalHeight / 96, 0, 0);
                    otherPropertyLabels[player2].Text = "Properties:";
                    otherPropertyLabels[player2].TextAlignment = TextAlignment.Center;
                    myDisplay.grid.Children.Add(otherCardsLeft[player2]);
                    otherCardsLeft[player2].Height = 5 * totalHeight / 96;
                    otherCardsLeft[player2].Width = (otherBoxWidth + otherMargin) / 6;
                    otherCardsLeft[player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1.333333) + otherBoxWidth * (2 * otherPlayers + 0.33333), 49 * totalHeight / 96, 0, 0);
                    otherCardsLeft[player2].Text = AllHands[player2].Count().ToString();
                    otherCardsLeft[player2].VerticalAlignment = VerticalAlignment.Top;
                    otherCardsLeft[player2].HorizontalAlignment = HorizontalAlignment.Left;
                    otherCardsLeft[player2].VerticalContentAlignment = VerticalAlignment.Center;
                    otherCardsLeft[player2].HorizontalContentAlignment = HorizontalAlignment.Center;
                    otherCardsLeft[player2].BorderThickness = new Thickness(0, 0, 0, 0);
                    otherCardsLeft[player2].Padding = new Thickness(0, 0, 0, 0);
                    myDisplay.grid.Children.Add(otherCardsLeftText[player2]);
                    otherCardsLeftText[player2].Height = 5 * totalHeight / 96;
                    otherCardsLeftText[player2].Width = (otherBoxWidth + otherMargin) / 3;
                    otherCardsLeftText[player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1) + otherBoxWidth * (2 * otherPlayers), 49 * totalHeight / 96, 0, 0);
                    otherCardsLeftText[player2].Text = "Cards in Hand:";
                    otherCardsLeftText[player2].VerticalAlignment = VerticalAlignment.Top;
                    otherCardsLeftText[player2].HorizontalAlignment = HorizontalAlignment.Left;
                    otherCardsLeftText[player2].VerticalContentAlignment = VerticalAlignment.Center;
                    otherCardsLeftText[player2].HorizontalContentAlignment = HorizontalAlignment.Right;
                    otherCardsLeftText[player2].BorderThickness = new Thickness(0, 0, 0, 0);
                    otherCardsLeftText[player2].Padding = new Thickness(0, 0, 0, 0);
                    myDisplay.grid.Children.Add(otherTurnsLeftText[player2]);
                    otherTurnsLeftText[player2].Height = 5 * totalHeight / 96;
                    otherTurnsLeftText[player2].Width = (otherBoxWidth + otherMargin) / 3;
                    otherTurnsLeftText[player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1.5) + otherBoxWidth * (2 * otherPlayers + 1.5), 49 * totalHeight / 96, 0, 0);
                    otherTurnsLeftText[player2].Text = "Turns Left:";
                    otherTurnsLeftText[player2].VerticalAlignment = VerticalAlignment.Top;
                    otherTurnsLeftText[player2].HorizontalAlignment = HorizontalAlignment.Left;
                    otherTurnsLeftText[player2].VerticalContentAlignment = VerticalAlignment.Center;
                    otherTurnsLeftText[player2].HorizontalContentAlignment = HorizontalAlignment.Right;
                    otherTurnsLeftText[player2].BorderThickness = new Thickness(0, 0, 0, 0);
                    otherTurnsLeftText[player2].Padding = new Thickness(0, 0, 0, 0);
                    myDisplay.grid.Children.Add(otherTurnsLeft[player2]);
                    otherTurnsLeft[player2].Height = 5 * totalHeight / 96;
                    otherTurnsLeft[player2].Width = (otherBoxWidth + otherMargin) / 6;
                    otherTurnsLeft[player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1.86667) + otherBoxWidth * (2 * otherPlayers + 1.866667), 49 * totalHeight / 96, 0, 0);
                    if (player2 == playerNum)
                    {
                        otherTurnsLeft[player2].Text = playNum.ToString();
                    }
                    else //Someone else
                    {
                        otherTurnsLeft[player2].Text = "0";
                    }
                    otherTurnsLeft[player2].VerticalAlignment = VerticalAlignment.Top;
                    otherTurnsLeft[player2].HorizontalAlignment = HorizontalAlignment.Left;
                    otherTurnsLeft[player2].VerticalContentAlignment = VerticalAlignment.Center;
                    otherTurnsLeft[player2].HorizontalContentAlignment = HorizontalAlignment.Center;
                    otherTurnsLeft[player2].BorderThickness = new Thickness(0, 0, 0, 0);
                    otherTurnsLeft[player2].Padding = new Thickness(0, 0, 0, 0);
                    otherPlayers++;
                }
            }

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
                messageToReceive = (Hashtable)JsonUtilities.DeserializeObjectFromJSON(receivedMessage, messageToReceive.GetType());
                AllHands = (List<List<Card>>)messageToReceive["AllHands"];
                AllTableMoney = (List<List<Card>>)messageToReceive["AllTableMoney"];
                numCardsInDeck = (int)messageToReceive["NumCardsInDeck"];
                numOfPlayers = (int)messageToReceive["NumOfPlayers"];
                playerNum = (int)messageToReceive["PlayerNum"];
                AllTableProperties = (List<List<List<Card>>>)messageToReceive["AllTableProperties"];
                playerNames = (List<string>)messageToReceive["PlayerNames"];
                playNum = (int)messageToReceive["PlayNum"];
                universalPrompt = (string)messageToReceive["NewUniversalPrompt"];
                individualPrompt = (string)messageToReceive["IndividualPrompt"];
                myDisplay.button1.Content = (string)messageToReceive["Button1Text"];
                myDisplay.button1.Visibility = (System.Windows.Visibility)messageToReceive["Button1Visibility"];
                myDisplay.button2.Content = (string)messageToReceive["Button2Text"];
                myDisplay.button2.Visibility = (System.Windows.Visibility)messageToReceive["Button2Visibility"];
                myDisplay.button3.Content = (string)messageToReceive["Button3Text"];
                myDisplay.button3.Visibility = (System.Windows.Visibility)messageToReceive["Button3Visibility"];
                myDisplay.buttonBack.Content = (string)messageToReceive["ButtonBackText"];
                myDisplay.buttonBack.Visibility = (System.Windows.Visibility)messageToReceive["ButtonBackVisibility"];
                updateDisplay();
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

        public static void updateDisplay()
        {

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
