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
        public static bool beginGame = false;
        public static int serverPort = 50501;
        private static int clientPort;
        private static string serverIP;
        private static string clientIP;
        private static int numOfPlayers;
        private static int numOfPlayersConnected;
        private static int playerNum;
        private static int playNum;
        private static int numCardsInDeck;
        public static List<List<Card>> AllHands = new List<List<Card>>();
        private static List<List<Card>> AllTableMoney = new List<List<Card>>();
        private static List<List<List<Card>>> AllTableProperties = new List<List<List<Card>>>();
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
        public static playerDisplay myDisplay;
        private static string myName;
        private static int myPlayerNum;
        private static Hashtable messageToReceive;
        private static Hashtable messageToSend;
        private static byte[] dataToSend;
        private static state clientState = state.initialize;
        private gameState currGameState;
        private clientEvent currClientEvent;
        public static int button1Clicked = 0;
        public static int button2Clicked = 0;
        public static int button3Clicked = 0;
        public static int buttonBackClicked = 0;
        public static int handDoubleClicked = 0;
        public static int propertiesDoubleClicked = 0;
        public static int propertiesSelectionChanged = 0;
        public static int moneySelectionChanged = 0;
        public static int otherPlayerClicked = 0;
        public static int otherPropertiesDoubleClicked = 0;
        public static int playerClicked;
        public static int otherPropertiesSelectedIndex;


        System.Timers.Timer aTimer = new System.Timers.Timer(100);
        private enum state
        {
            initialize,
            connectToServer,
            getNewPort,
            waitForOthers,
            beginGame,
            transmit,
        }

        /// <summary>
        /// Main Loop
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            //myDisplay.window.SizeChanged += window_SizeChanged;
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;

            //StreamData sd = new StreamData("");
            //byte[] storage = GetBytes(textBlock.Text);
            //client.sendData(storage);

        }

        public void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            aTimer.Enabled = false;
            Application.Current.Dispatcher.BeginInvoke(new ThreadStart(() => timerDing()));
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
                    sendAcknowledgement();
                    string tempString = GetString(storage);
                    currGameState = Newtonsoft.Json.JsonConvert.DeserializeObject<gameState>(tempString);
                    serverPort = currGameState._serverPort;
                    myPlayerNum = currGameState._myPlayerNum;
                    numOfPlayers = currGameState._numOfPlayers;
                    numOfPlayersConnected = currGameState._numOfPlayersConnected;
                    playerNames = currGameState._playerNames;
                    beginGame = currGameState._beginGame;
                    initializeDisplay();
                    client.stop();
                    Thread.Sleep(100);
                    client = new ClientSocket(serverPort, 2, clientPort, clientIP, serverIP);
                    storage = GetBytes(myName);
                    client.sendData(storage);
                    if (beginGame)
                    {
                        clientState = state.beginGame;
                    }
                    else
                    {
                        clientState = state.waitForOthers;
                    }
                }
            }
            if (clientState == state.waitForOthers)
            {
                myDisplay.Prompt.Content = "Waiting on " + (numOfPlayers - numOfPlayersConnected) + " more players...";
                byte[] storage = null;
                storage = client.pollAndReceiveData(client.Server, 2);
                if (storage.Count() > 2)
                {
                    sendAcknowledgement();
                    string tempString = GetString(storage);
                    currGameState = Newtonsoft.Json.JsonConvert.DeserializeObject<gameState>(tempString);
                    numOfPlayersConnected = currGameState._numOfPlayersConnected;
                    playerNames = currGameState._playerNames;
                    beginGame = currGameState._beginGame;
                }
                if (beginGame)
                {
                    clientState = state.beginGame;
                }
            }
            if (clientState == state.beginGame)
            {
                byte[] storage = null;
                storage = client.pollAndReceiveData(client.Server, 2);
                if (storage.Count() > 2)
                {
                    sendAcknowledgement();
                    string tempString = GetString(storage);
                    currGameState = Newtonsoft.Json.JsonConvert.DeserializeObject<gameState>(tempString);
                    AllHands = currGameState._AllHands;
                    AllTableMoney = currGameState._AllTableMoney;
                    AllTableProperties = currGameState._AllTableProperties;
                    myDisplay.button1.Content = currGameState._button1Text;
                    myDisplay.button1.Visibility = currGameState._button1Visibility;
                    myDisplay.button2.Content = currGameState._button2Text;
                    myDisplay.button2.Visibility = currGameState._button2Visibility;
                    myDisplay.button3.Content = currGameState._button3Text;
                    myDisplay.button3.Visibility = currGameState._button3Visibility;
                    myDisplay.buttonBack.Content = currGameState._buttonBackText;
                    myDisplay.buttonBack.Visibility = currGameState._buttonBackVisibility;
                    myDisplay.Prompt.Content = currGameState._individualPrompt;
                    myDisplay.universalPrompt.Text += currGameState._newUniversalPrompt;
                    numCardsInDeck = currGameState._numCardsInDeck;
                    playNum = currGameState._playNum;
                    playerNum = currGameState._playerNum;
                    playerNames = currGameState._playerNames;
                    showFullDisplay();
                    clientState = state.transmit;
                }
            }
            if (clientState == state.transmit)
            {
                byte[] storage = null;
                storage = client.pollAndReceiveData(client.Server, 2);
                if (storage.Count() > 2)
                {
                    sendAcknowledgement();
                    string tempString = GetString(storage);
                    currGameState = Newtonsoft.Json.JsonConvert.DeserializeObject<gameState>(tempString);
                    AllHands = currGameState._AllHands;
                    AllTableMoney = currGameState._AllTableMoney;
                    AllTableProperties = currGameState._AllTableProperties;
                    myDisplay.button1.Content = currGameState._button1Text;
                    myDisplay.button1.Visibility = currGameState._button1Visibility;
                    myDisplay.button2.Content = currGameState._button2Text;
                    myDisplay.button2.Visibility = currGameState._button2Visibility;
                    myDisplay.button3.Content = currGameState._button3Text;
                    myDisplay.button3.Visibility = currGameState._button3Visibility;
                    myDisplay.buttonBack.Content = currGameState._buttonBackText;
                    myDisplay.buttonBack.Visibility = currGameState._buttonBackVisibility;
                    myDisplay.Prompt.Content = currGameState._individualPrompt;
                    myDisplay.universalPrompt.Text += currGameState._newUniversalPrompt;
                    numCardsInDeck = currGameState._numCardsInDeck;
                    playNum = currGameState._playNum;
                    playerNum = currGameState._playerNum;
                    playerNames = currGameState._playerNames;
                    showTable(currGameState._updateCards);
                }
            }
            aTimer.Enabled = true;
        }

        public void initializeDisplay()
        {
            myDisplay = new playerDisplay();
            myDisplay.button1.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.button2.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.button3.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.buttonBack.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.buttonPlayer.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.cardsLeftText.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.deckCountDisplay.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.Hand.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.handLabel.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.moneyLabel.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.propertiesLabel.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.Table_Money.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.Table_Properties.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.turnsLeftDisplay.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.turnsLeftText.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.universalPrompt.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.Show();
            this.Hide();
        }

        public void showFullDisplay()
        {
            myDisplay.button1.Click += button1_Click;
            myDisplay.button2.Click += button2_Click;
            myDisplay.button3.Click += button3_Click;
            myDisplay.buttonBack.Click += buttonBack_Click;
            myDisplay.Table_Properties.SelectionChanged += Table_Properties_SelectionChanged;
            myDisplay.Table_Properties.MouseDoubleClick += Table_Properties_MouseDoubleClick;
            myDisplay.Table_Money.SelectionChanged += Table_Money_SelectionChanged;
            myDisplay.Hand.MouseDoubleClick += Hand_MouseDoubleClick;
            //myDisplay.button1.Visibility = System.Windows.Visibility.Visible;
            //myDisplay.button2.Visibility = System.Windows.Visibility.Visible;
            //myDisplay.button3.Visibility = System.Windows.Visibility.Visible;
            //myDisplay.buttonBack.Visibility = System.Windows.Visibility.Visible;
            myDisplay.buttonPlayer.Visibility = System.Windows.Visibility.Visible;
            myDisplay.cardsLeftText.Visibility = System.Windows.Visibility.Visible;
            myDisplay.deckCountDisplay.Visibility = System.Windows.Visibility.Visible;
            myDisplay.Hand.Visibility = System.Windows.Visibility.Visible;
            myDisplay.handLabel.Visibility = System.Windows.Visibility.Visible;
            myDisplay.moneyLabel.Visibility = System.Windows.Visibility.Visible;
            myDisplay.propertiesLabel.Visibility = System.Windows.Visibility.Visible;
            myDisplay.Table_Money.Visibility = System.Windows.Visibility.Visible;
            myDisplay.Table_Properties.Visibility = System.Windows.Visibility.Visible;
            myDisplay.turnsLeftDisplay.Visibility = System.Windows.Visibility.Visible;
            myDisplay.turnsLeftText.Visibility = System.Windows.Visibility.Visible;
            myDisplay.universalPrompt.Visibility = System.Windows.Visibility.Visible;

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
                    otherTable_Properties[player2].MouseDoubleClick += OtherPlayer_Properties_MouseDoubleClick;
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
                    otherNames[player2].Click += OtherPlayer_Click;
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
            myDisplay.window.SizeChanged += window_SizeChanged;
            myDisplay.UpdateLayout();
            myDisplay.Show();
        }

        public void resizeDisplay()
        {
            myDisplay.buttonPlayer.Visibility = System.Windows.Visibility.Visible;
            myDisplay.cardsLeftText.Visibility = System.Windows.Visibility.Visible;
            myDisplay.deckCountDisplay.Visibility = System.Windows.Visibility.Visible;
            myDisplay.Hand.Visibility = System.Windows.Visibility.Visible;
            myDisplay.handLabel.Visibility = System.Windows.Visibility.Visible;
            myDisplay.moneyLabel.Visibility = System.Windows.Visibility.Visible;
            myDisplay.propertiesLabel.Visibility = System.Windows.Visibility.Visible;
            myDisplay.Table_Money.Visibility = System.Windows.Visibility.Visible;
            myDisplay.Table_Properties.Visibility = System.Windows.Visibility.Visible;
            myDisplay.turnsLeftDisplay.Visibility = System.Windows.Visibility.Visible;
            myDisplay.turnsLeftText.Visibility = System.Windows.Visibility.Visible;
            myDisplay.universalPrompt.Visibility = System.Windows.Visibility.Visible;

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
                    otherTable_Properties[player2].Width = otherBoxWidth;
                    otherTable_Properties[player2].Height = otherBoxHeight;
                    otherTable_Properties[player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    otherTable_Properties[player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    otherTable_Properties[player2].Margin = new Thickness(otherMargin * (2 + 3 * otherPlayers) + otherBoxWidth * (2 * otherPlayers + 1), 5 * totalHeight / 8, 0, 0);
                    otherTable_Properties[player2].Name = playerNames[player2];
                    otherTable_Money[player2].Width = otherBoxWidth;
                    otherTable_Money[player2].Height = otherBoxHeight;
                    otherTable_Money[player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    otherTable_Money[player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    otherTable_Money[player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1) + 2 * otherBoxWidth * otherPlayers, 5 * totalHeight / 8, 0, 0);
                    otherNames[player2].Height = 5 * totalHeight / 96;
                    otherNames[player2].Width = otherBoxWidth;//2 * otherBoxWidth + otherMargin;
                    otherNames[player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    otherNames[player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    otherNames[player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1.5) + otherBoxWidth * (2 * otherPlayers + 0.5), 49 * totalHeight / 96, 0, 0);
                    otherNames[player2].Content = playerNames[player2];
                    otherNames[player2].HorizontalContentAlignment = HorizontalAlignment.Center;
                    otherNames[player2].VerticalContentAlignment = VerticalAlignment.Center;
                    otherNames[player2].Background = Brushes.DarkRed;
                    otherMoneyLabels[player2].Height = totalHeight / 24;
                    otherMoneyLabels[player2].Width = otherBoxWidth;
                    otherMoneyLabels[player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    otherMoneyLabels[player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    otherMoneyLabels[player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1) + 2 * otherBoxWidth * otherPlayers, 55 * totalHeight / 96, 0, 0);
                    otherMoneyLabels[player2].Text = "Money:";
                    otherMoneyLabels[player2].TextAlignment = TextAlignment.Center;
                    otherPropertyLabels[player2].Height = totalHeight / 24;
                    otherPropertyLabels[player2].Width = otherBoxWidth;
                    otherPropertyLabels[player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    otherPropertyLabels[player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    otherPropertyLabels[player2].Margin = new Thickness(otherMargin * (2 + 3 * otherPlayers) + otherBoxWidth * (2 * otherPlayers + 1), 55 * totalHeight / 96, 0, 0);
                    otherPropertyLabels[player2].Text = "Properties:";
                    otherPropertyLabels[player2].TextAlignment = TextAlignment.Center;
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
            myDisplay.UpdateLayout();
            myDisplay.Show();
        }

        public void showTable(bool updateCards)
        {
            if (updateCards)
            {
                myDisplay.Hand.Items.Clear();
                foreach (Card card in AllHands[myPlayerNum])
                {
                    myDisplay.Hand.Items.Add(card);
                }
                myDisplay.Table_Money.Items.Clear();
                foreach (Card card in MainWindow.AllTableMoney[myPlayerNum])
                {
                    myDisplay.Table_Money.Items.Add(card);
                }
                myDisplay.Table_Properties.Items.Clear();
                foreach (List<Card> propColor in MainWindow.AllTableProperties[myPlayerNum])
                {
                    foreach (Card card in propColor)
                    {
                        myDisplay.Table_Properties.Items.Add(card);
                    }
                }
                for (int otherPlayer = 0; otherPlayer < MainWindow.numOfPlayers; otherPlayer++)
                {
                    MainWindow.otherTable_Money[otherPlayer].Items.Clear();
                    MainWindow.otherTable_Properties[otherPlayer].Items.Clear();
                    if (otherPlayer != myPlayerNum)
                    {
                        MainWindow.otherCardsLeft[otherPlayer].Text = MainWindow.AllHands[otherPlayer].Count().ToString();
                        if (otherPlayer == MainWindow.playerNum)
                        {
                            MainWindow.otherTurnsLeft[otherPlayer].Text = (3 - playNum).ToString();
                        }
                        else
                        {
                            MainWindow.otherTurnsLeft[otherPlayer].Text = "0";
                        }
                        foreach (Card card in MainWindow.AllTableMoney[otherPlayer])
                        {
                            MainWindow.otherTable_Money[otherPlayer].Items.Add(card);
                        }
                        foreach (List<Card> propColor in MainWindow.AllTableProperties[otherPlayer])
                        {
                            foreach (Card card in propColor)
                            {
                                MainWindow.otherTable_Properties[otherPlayer].Items.Add(card);
                            }
                        }
                    }
                }
            }
            //myDisplay.Hand.Visibility = System.Windows.Visibility.Visible;
            //myDisplay.Table_Properties.Visibility = System.Windows.Visibility.Visible;
            //myDisplay.Table_Money.Visibility = System.Windows.Visibility.Visible;
            myDisplay.Hand.IsEnabled = true;
            myDisplay.Table_Properties.IsEnabled = true;
            myDisplay.Table_Money.IsEnabled = true;
            foreach (Button playerName in otherNames)
            {
                playerName.IsEnabled = true;
            }
            foreach (ListBox otherProps in otherTable_Properties)
            {
                otherProps.IsEnabled = true;
            }
        }

        public void disableTable()
        {
            myDisplay.button1.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.button2.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.button3.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.buttonBack.Visibility = System.Windows.Visibility.Hidden;
            myDisplay.Hand.IsEnabled = false;
            myDisplay.Table_Properties.IsEnabled = false;
            myDisplay.Table_Money.IsEnabled = false;
            foreach (Button playerName in otherNames)
            {
                playerName.IsEnabled = false;
            }
            foreach (ListBox otherProps in otherTable_Properties)
            {
                otherProps.IsEnabled = false;
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

        public void sendClientEvent()
        {
            //Hide all buttons
            disableTable();
            currClientEvent = new clientEvent();
            string tempString = Newtonsoft.Json.JsonConvert.SerializeObject(currClientEvent);
            byte[] toSend = GetBytes(tempString.ToString());
            client.sendData(toSend);
            waitForAcknowledgement();
        }

        public void sendAcknowledgement()
        {
            string tempString = "Ack";
            byte[] toSend = GetBytes(tempString.ToString());
            client.sendData(toSend);
        }

        public void waitForAcknowledgement()
        {
            bool wait = true;
            DateTime start = DateTime.Now;
            while (wait)
            {
                byte[] storage = null;
                storage = client.pollAndReceiveData(client.Server, 2);
                if (storage.Count() > 2)
                {
                    string tempString = GetString(storage);
                    if (tempString == "Ack")
                    {
                        wait = false;
                        clearEvents();
                    }
                }
                else
                {
                    TimeSpan duration = DateTime.Now - start;
                    if (duration.TotalMilliseconds > 1000)
                    {
                        sendClientEvent();
                        break;
                    }
                }
            }
        }

        #region Events
        public void clearEvents()
        {
            button1Clicked = 0;
            button2Clicked = 0;
            button3Clicked = 0;
            buttonBackClicked = 0;
            handDoubleClicked = 0;
            moneySelectionChanged = 0;
            otherPlayerClicked = 0;
            otherPropertiesDoubleClicked = 0;
            propertiesDoubleClicked = 0;
            propertiesSelectionChanged = 0;
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            serverIP = serverIPBlock.Text;
            clientIP = myIPBlock.Text;
            clientPort = Int32.Parse(myPort.Text);
            myName = nameBlock.Text;
            clientState = state.connectToServer;
        }

        public void button1_Click(object sender, RoutedEventArgs e)
        {
            button1Clicked = 1;
            sendClientEvent();
        }

        public void button2_Click(object sender, RoutedEventArgs e)
        {
            button2Clicked = 1;
            sendClientEvent();
        }

        public void button3_Click(object sender, RoutedEventArgs e)
        {
            button3Clicked = 1;
            sendClientEvent();
        }

        public void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            buttonBackClicked = 1;
            sendClientEvent();
        }

        public void Table_Properties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            propertiesSelectionChanged = 1;
            sendClientEvent();
        }

        public void Table_Money_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            moneySelectionChanged = 1;
            sendClientEvent();
        }

        public void Table_Properties_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            propertiesDoubleClicked = 1;
            sendClientEvent();
        }

        public void Hand_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            handDoubleClicked = 1;
            sendClientEvent();
        }

        public void OtherPlayer_Click(object sender, RoutedEventArgs e)
        {
            otherPlayerClicked = 1;
            Button copySender = (Button)sender;
            playerClicked = MainWindow.otherNames.IndexOf(copySender);
            sendClientEvent();
        }

        public void OtherPlayer_Properties_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            otherPropertiesDoubleClicked = 1;
            ListBox copySender = (ListBox)sender;
            playerClicked = MainWindow.otherTable_Properties.IndexOf(copySender);
            otherPropertiesSelectedIndex = MainWindow.otherTable_Properties[playerClicked].SelectedIndex;
            sendClientEvent();
        }

        public void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            resizeDisplay();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            client.stop();
            this.Close();
        }
    }
        #endregion
}
