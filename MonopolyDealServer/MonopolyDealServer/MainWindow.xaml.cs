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
using System.Timers;

namespace MonopolyDealServer
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables
        private static List<ServerSocket> servers = new List<ServerSocket>();
        public static int numOfPlayers;
        private static int serverPort = 50502;
        private static string serverIP;
        public static byte[] storage;
        public static List<string> playerNames = new List<string>();
        public static List<Card> deckShuffled = new List<Card>();
        public static List<Card> deckDiscard = new List<Card>();
        //public static int numOfPlayers = 2;
        //public static List<string> playerNames = new List<string>(){"Eric","Loser2","Loser3","Loser4","Loser5","Loser6"};
        public static List<List<ListBox>> otherTable_Money = new List<List<ListBox>>();
        public static List<List<ListBox>> otherTable_Properties = new List<List<ListBox>>();
        public static List<List<Button>> otherNames = new List<List<Button>>();
        public static List<List<TextBox>> otherMoneyLabels = new List<List<TextBox>>();
        public static List<List<TextBox>> otherPropertyLabels = new List<List<TextBox>>();
        public static List<List<TextBox>> otherCardsLeftText = new List<List<TextBox>>();
        public static List<List<TextBox>> otherCardsLeft = new List<List<TextBox>>();
        public static List<List<TextBox>> otherTurnsLeftText = new List<List<TextBox>>();
        public static List<List<TextBox>> otherTurnsLeft = new List<List<TextBox>>();
        public static int cardsToDeal = 5;
        public static bool gameContinues = true;
        public static int playerNum = 0;
        public static int chosenPlayer = 0;
        public static int cardNum;
        public static int cardNum2;
        public static int propIndex;
        public static int propIndex2;
        public static string propColor;
        public static CardType cardType;
        public static int playNum;
        public static int payment;
        public static Card rentCard;
        public static Card propertyCard;
        public static Card playedCard;
        public static PropertyType propertyType;
        public static List<int> payersLeft = new List<int>();
        public static int justSayNos = 0;
        public static int monopolyIndex1;
        public static int monopolyIndex2;
        public static List<List<bool>> hasBeenRented = new List<List<bool>>();
        public static bool doublePlayed = false;
        public static bool doublePlayed2 = false;
        public static List<int> monopolyPropsNeeded = new List<int>() { 2, 2, 2, 3, 3, 3, 3, 3, 3, 4, 20 };
        public static List<List<Card>> AllHands = new List<List<Card>>();
        public static List<List<Card>> AllTableMoney = new List<List<Card>>();
        public static string newUniversalPrompt;
        public static List<string> individualPrompts;
        //Heirarchy... 
        //1: All properties on table
        //2: Individual's properties
        //3: Individual's properties of certain color
        //[brown,utility,blue,lightblue,pink,orange,red,yellow,green,black]
        public static List<List<List<Card>>> AllTableProperties = new List<List<List<Card>>>();
        public static turnStage stage = new turnStage();
        public enum turnStage
        {
            begin,
            ready,
            decidePlayType,
            moveCards,
            moveCardsDecideType,
            moveCardsDecideTypeWild,
            checkMoveCard,
            playCardFromeHand,
            decideCardType,
            decidePropertyType,
            decidePropertyTypeWild,
            checkPlayCard,
            discard,
            checkDiscard,
            house,
            checkHouse,
            hotel,
            checkHotel,
            debtCollect,
            birthday,
            forcedDeal1,
            forcedDeal2,
            receiveForcedDeal,
            slyDeal,
            dealBreaker,
            rentWild,
            rentWild2,
            rent,
            doubleRent,
            doubleRentWild,
            checkRent,
            acknowledgeAttack,
        }
        public static List<playerTurn> playerTurns = new List<playerTurn>();
        public static Type type;
#endregion

        #region Deck Definitions
        /// <summary>
        /// Unshuffled Deck
        /// </summary>
        public static List<Card> deckBeginning = new List<Card>()
        {
            Card.One,
            Card.One,
            Card.One,
            Card.One,
            Card.One,
            Card.One,
            Card.Two,
            Card.Two,
            Card.Two,
            Card.Two,
            Card.Two,
            Card.Three,
            Card.Three,
            Card.Three,
            Card.Four,
            Card.Four,
            Card.Four,
            Card.Five,
            Card.Five,
            Card.Ten,
            Card.PassGo__1,
            Card.PassGo__1,
            Card.PassGo__1,
            Card.PassGo__1,
            Card.PassGo__1,
            Card.PassGo__1,
            Card.PassGo__1,
            Card.PassGo__1,
            Card.PassGo__1,
            Card.PassGo__1,
            Card.DoubleTheRent__1,
            Card.DoubleTheRent__1,
            Card.Birthday__2,
            Card.Birthday__2,
            Card.Birthday__2,
            Card.House__3,
            Card.House__3,
            Card.House__3,
            Card.Hotel__4,
            Card.Hotel__4,
            Card.SlyDeal__3,
            Card.SlyDeal__3,
            Card.SlyDeal__3,
            Card.ForcedDeal__3,
            Card.ForcedDeal__3,
            Card.ForcedDeal__3,
            Card.DebtCollector__3,
            Card.DebtCollector__3,
            Card.DebtCollector__3,
            Card.JustSayNo__4,
            Card.JustSayNo__4,
            Card.JustSayNo__4,
            Card.DealBreaker__5,
            Card.DealBreaker__5,
            Card.RentWild__3,
            Card.RentWild__3,
            Card.RentWild__3,
            Card.RentRedYellow__1,
            Card.RentRedYellow__1,
            Card.RentPinkOrange__1,
            Card.RentPinkOrange__1,
            Card.RentGreenBlue__1,
            Card.RentGreenBlue__1,
            Card.RentBrownLightBlue__1,
            Card.RentBrownLightBlue__1,
            Card.RentBlackUtility__1,
            Card.RentBlackUtility__1,
            Card.PropertyBlack__2,
            Card.PropertyBlack__2,
            Card.PropertyBlack__2,
            Card.PropertyBlack__2,
            Card.PropertyBlue__4,
            Card.PropertyBlue__4,
            Card.PropertyBrown__1,
            Card.PropertyBrown__1,
            Card.PropertyGreen__4,
            Card.PropertyGreen__4,
            Card.PropertyGreen__4,
            Card.PropertyLightBlue__1,
            Card.PropertyLightBlue__1,
            Card.PropertyLightBlue__1,
            Card.PropertyUtility__1,
            Card.PropertyUtility__1,
            Card.PropertyOrange__2,
            Card.PropertyOrange__2,
            Card.PropertyOrange__2,
            Card.PropertyPink__2,
            Card.PropertyPink__2,
            Card.PropertyPink__2,
            Card.PropertyRed__3,
            Card.PropertyRed__3,
            Card.PropertyRed__3,
            Card.PropertyWild,
            Card.PropertyWild,
            Card.PropertyYellow__3,
            Card.PropertyYellow__3,
            Card.PropertyYellow__3,
            Card.PropertyRedYellow__3,
            Card.PropertyRedYellow__3,
            Card.PropertyPinkOrange__2,
            Card.PropertyPinkOrange__2,
            Card.PropertyLightBlueBlack__4,
            Card.PropertyUtilityBlack__2,
            Card.PropertyBrownLightBlue__1,
            Card.PropertyBlueGreen__4,
            Card.PropertyGreenBlack__4
        };
        #endregion

        #region Server Stuff
        private static state serverState = state.initialize;
        System.Timers.Timer aTimer = new System.Timers.Timer(100);
        private enum state
        {
            initialize,
            startServers,
            getName,
            transmit,
            connecting
        }

        /// <summary>
        /// Main Loop
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;
            //StreamData sd = new StreamData("");
            //byte[] storage = GetBytes(textBlock.Text);
            //server.sendData(server.Client,storage) ;
        }

        public void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            aTimer.Enabled = false;
            Application.Current.Dispatcher.BeginInvoke(new ThreadStart(() => this.timerDing()));
            Thread.CurrentThread.Abort();
        }

        public void timerDing()
        {
            if (serverState == state.startServers)
            {
                servers.Add(new ServerSocket(50501, 2, serverIP));
                servers[servers.Count - 1].start();
                byte[] newPort = GetBytes(serverPort.ToString());
                servers[servers.Count - 1].sendData(servers[servers.Count - 1].Client, newPort);
                servers[servers.Count - 1].stop();
                servers[servers.Count - 1] = new ServerSocket(serverPort, 2, serverIP);
                servers[servers.Count - 1].start();
                serverPort++;
                serverState = state.getName;
            }
            if (serverState == state.getName)
            {
                StreamData sd = new StreamData("");
                byte[] storage = null;
                storage = servers[servers.Count - 1].pollAndReceiveData(servers[servers.Count - 1].Client, 2);
                if (storage.Count() > 2)
                {
                    playerNames.Add(GetString(storage));
                    if (servers.Count < numOfPlayers)
                    {
                        serverState = state.startServers;
                    }
                    else
                    {
                        serverState = state.transmit;
                    }
                }
            }

            if (serverState == state.transmit)
            {
                checkForMessages();
            }
            aTimer.Enabled = true;
        }

        public void checkForMessages()
        {
            for (int player = 0;player<numOfPlayers;player++)
            {
            storage = null;
            try
            {
                storage = servers[player].pollAndReceiveData(servers[player].Client, 2);
            }
            catch
            {

            }
            if (storage.Count() > 2)
            {

            }
            }
            //if(
            //{
            //    //Store data 
            //    dataToSend = "Hello Buddy";
            //    sd = new StreamData(myName);
            //    server.sendData(server.Client, sd);
            //}
        }

        public void sendMessages()
        {
            for (int player = 0; player < numOfPlayers; player++)
            {
                Hashtable message = new Hashtable();
                message.Add("NumOfPlayers", numOfPlayers);
                message.Add("PlayerNum", playerNum);
                message.Add("AllTableProperties", AllTableProperties);
                message.Add("AllTableMoney", AllTableMoney);
                message.Add("AllHands", AllHands);
                message.Add("PlayerNames", playerNames);
                message.Add("PlayNum", playNum);
                message.Add("Button1Text", playerTurns[player].button1.Content.ToString());
                message.Add("Button1Visibility", playerTurns[player].button1.Visibility);
                message.Add("Button2Text", playerTurns[player].button2.Content.ToString());
                message.Add("Button2Visibility", playerTurns[player].button2.Visibility);
                message.Add("Button3Text", playerTurns[player].button3.Content.ToString());
                message.Add("Button3Visibility", playerTurns[player].button3.Visibility);
                message.Add("ButtonBackText", playerTurns[player].buttonBack.Content.ToString());
                message.Add("ButtonBackVisibility", playerTurns[player].buttonBack.Visibility);
                message.Add("NumCardsInDeck", deckShuffled.Count());
                message.Add("NewUniversalPrompt", newUniversalPrompt);
                message.Add("IndividualPrompt", individualPrompts);
                string messageString = JsonUtilities.SerializeObjectToJSON(message);
                byte[] messageByte = GetBytes(messageString);
                servers[player].sendData(servers[player].Client, messageByte);
            }
            newUniversalPrompt = "";
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
            serverIP = textBlock.Text;
            numOfPlayers = Int32.Parse(textBlock_players.Text);
            serverState = state.startServers;
            timerDing();
        }
#endregion

        #region Monopoly Deal Stuff
        
        public void beginMonopolyDeal()
        {
            InitializeComponent();
            double left = 0;
            double top = 0;
            for (int i = 0; i < numOfPlayers; i++)
            {
                left = (i % 2) * 680;
                top = (i * 175) - (i % 2) * 175;
                playerTurns.Add(new playerTurn(i));
            }
            for (int i = 0; i < numOfPlayers; i++)
            {
                MainWindow.playerTurns[i].window.SizeChanged += MainWindow.window_SizeChanged;
                AllHands.Add(new List<Card>());
                AllTableMoney.Add(new List<Card>());
                AllTableProperties.Add(new List<List<Card>>());
                otherMoneyLabels.Add(new List<TextBox>());
                otherNames.Add(new List<Button>());
                otherPropertyLabels.Add(new List<TextBox>());
                otherTable_Money.Add(new List<ListBox>());
                otherTable_Properties.Add(new List<ListBox>());
                otherCardsLeft.Add(new List<TextBox>());
                otherCardsLeftText.Add(new List<TextBox>());
                otherTurnsLeft.Add(new List<TextBox>());
                otherTurnsLeftText.Add(new List<TextBox>());

                for (int j = 0; j < (numOfPlayers); j++)
                {
                    otherMoneyLabels[i].Add(new TextBox());
                    otherNames[i].Add(new Button());
                    otherPropertyLabels[i].Add(new TextBox());
                    otherTable_Money[i].Add(new ListBox());
                    otherTable_Properties[i].Add(new ListBox());
                    otherCardsLeft[i].Add(new TextBox());
                    otherCardsLeftText[i].Add(new TextBox());
                    otherTurnsLeft[i].Add(new TextBox());
                    otherTurnsLeftText[i].Add(new TextBox());
                }
                hasBeenRented.Add(new List<bool>());
                for (int j = 0; j < 11; j++)
                {
                    AllTableProperties[i].Add(new List<Card>());
                    hasBeenRented[i].Add(false);
                }
            }
            setupLayout();
            shuffleDeck();
            dealDeck();
            playerTurns[0].showTable();
            playerTurns[0].readyToBegin();
            this.Hide();
        }

        public static void setupLayout()
        {
            for (int player = 0; player < numOfPlayers; player++)
            {
                double totalWidth = MainWindow.playerTurns[player].window.RenderSize.Width;
                double totalHeight = MainWindow.playerTurns[player].window.RenderSize.Height;
                double boxWidth = 7 * totalWidth / 48;
                double boxHeight = totalHeight / 4;
                MainWindow.playerTurns[player].grid.Width = totalWidth;
                MainWindow.playerTurns[player].grid.Height = totalHeight;
                MainWindow.playerTurns[player].grid.Margin = new Thickness(0, 0, 0, 0);
                MainWindow.playerTurns[player].Hand.Width = boxWidth;
                MainWindow.playerTurns[player].Table_Money.Width = boxWidth;
                MainWindow.playerTurns[player].Table_Properties.Width = boxWidth;
                MainWindow.playerTurns[player].Hand.Margin = new Thickness((totalWidth / 2 - 3 * boxWidth) / 6, totalHeight / 8, 0, 0);
                MainWindow.playerTurns[player].Table_Money.Margin = new Thickness(boxWidth + ((totalWidth / 2 - 3 * boxWidth) / 2), totalHeight / 8, 0, 0);
                MainWindow.playerTurns[player].Table_Properties.Margin = new Thickness(2 * boxWidth + 5 * (totalWidth / 2 - 3 * boxWidth) / 6, totalHeight / 8, 0, 0);
                MainWindow.playerTurns[player].handLabel.Width = boxWidth;
                MainWindow.playerTurns[player].handLabel.Height = totalHeight / 24;
                MainWindow.playerTurns[player].handLabel.Margin = new Thickness((totalWidth / 2 - 3 * boxWidth) / 6, 7 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].moneyLabel.Width = boxWidth;
                MainWindow.playerTurns[player].moneyLabel.Height = totalHeight / 24;
                MainWindow.playerTurns[player].moneyLabel.Margin = new Thickness(boxWidth + ((totalWidth / 2 - 3 * boxWidth) / 2), 7 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].propertiesLabel.Width = boxWidth;
                MainWindow.playerTurns[player].propertiesLabel.Height = totalHeight / 24;
                MainWindow.playerTurns[player].propertiesLabel.Margin = new Thickness(2 * boxWidth + 5 * (totalWidth / 2 - 3 * boxWidth) / 6, 7 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].buttonPlayer.Width = totalWidth / 16;
                MainWindow.playerTurns[player].buttonPlayer.Height = 5 * totalHeight / 96;
                MainWindow.playerTurns[player].buttonPlayer.Margin = new Thickness((totalWidth / 2 - 3 * boxWidth) / 6, totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].Prompt.Width = 10 * totalWidth / 32;
                MainWindow.playerTurns[player].Prompt.Height = 5 * totalHeight / 96;
                MainWindow.playerTurns[player].Prompt.Margin = new Thickness(4 * totalWidth / 48, totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].cardsLeftText.Width = 4 * totalWidth / 64;
                MainWindow.playerTurns[player].cardsLeftText.Height = 2 * totalHeight / 96;
                MainWindow.playerTurns[player].cardsLeftText.Margin = new Thickness(39 * totalWidth / 96, totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].deckCountDisplay.Width = 2 * totalWidth / 64;
                MainWindow.playerTurns[player].deckCountDisplay.Height = 2 * totalHeight / 96;
                MainWindow.playerTurns[player].deckCountDisplay.Margin = new Thickness(30 * totalWidth / 64, totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].turnsLeftText.Width = 4 * totalWidth / 64;
                MainWindow.playerTurns[player].turnsLeftText.Height = 2 * totalHeight / 96;
                MainWindow.playerTurns[player].turnsLeftText.Margin = new Thickness(39 * totalWidth / 96, 4 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].turnsLeftDisplay.Width = 2 * totalWidth / 64;
                MainWindow.playerTurns[player].turnsLeftDisplay.Height = 2 * totalHeight / 96;
                MainWindow.playerTurns[player].turnsLeftDisplay.Margin = new Thickness(30 * totalWidth / 64, 4 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].buttonBack.Width = 4 * totalWidth / 64;
                MainWindow.playerTurns[player].buttonBack.Height = 6 * totalHeight / 96;
                MainWindow.playerTurns[player].buttonBack.Margin = new Thickness(totalWidth / 96, 37 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].button1.Width = 8 * totalWidth / 64;
                MainWindow.playerTurns[player].button1.Height = 6 * totalHeight / 96;
                MainWindow.playerTurns[player].button1.Margin = new Thickness(8 * totalWidth / 96, 37 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].button2.Width = 8 * totalWidth / 64;
                MainWindow.playerTurns[player].button2.Height = 6 * totalHeight / 96;
                MainWindow.playerTurns[player].button2.Margin = new Thickness(21 * totalWidth / 96, 37 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].button3.Width = 8 * totalWidth / 64;
                MainWindow.playerTurns[player].button3.Height = 6 * totalHeight / 96;
                MainWindow.playerTurns[player].button3.Margin = new Thickness(34 * totalWidth / 96, 37 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].universalPrompt.Width = 46 * totalWidth / 96;
                MainWindow.playerTurns[player].universalPrompt.Height = 46 * totalHeight / 96;
                MainWindow.playerTurns[player].universalPrompt.Margin = new Thickness(48 * totalWidth / 96, totalHeight / 96, 0, 0);

                int otherPlayers = 0;
                for (int player2 = 0; player2 < MainWindow.numOfPlayers; player2++)
                {
                    double otherBoxWidth = totalWidth / ((MainWindow.numOfPlayers - 1) * 2 + 1);
                    otherBoxWidth = Math.Min(otherBoxWidth, boxWidth);
                    double otherBoxHeight = totalHeight / 4;
                    double otherMargin = (totalWidth - otherBoxWidth * 2 * (MainWindow.numOfPlayers - 1)) / (3 * (MainWindow.numOfPlayers - 1));
                    if (player2 != player)
                    {
                        MainWindow.playerTurns[player].grid.Children.Add(MainWindow.otherTable_Properties[player][player2]);
                        MainWindow.otherTable_Properties[player][player2].Width = otherBoxWidth;
                        MainWindow.otherTable_Properties[player][player2].Height = otherBoxHeight;
                        MainWindow.otherTable_Properties[player][player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        MainWindow.otherTable_Properties[player][player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        MainWindow.otherTable_Properties[player][player2].Margin = new Thickness(otherMargin * (2 + 3 * otherPlayers) + otherBoxWidth * (2 * otherPlayers + 1), 5 * totalHeight / 8, 0, 0);
                        MainWindow.otherTable_Properties[player][player2].Name = MainWindow.playerNames[player2];
                        //MainWindow.otherTable_Properties[player][player2].MouseDoubleClick += MainWindow.playerTurns[MainWindow.player2].OtherPlayer_Properties_MouseDoubleClick;
                        MainWindow.playerTurns[player].grid.Children.Add(MainWindow.otherTable_Money[player][player2]);
                        MainWindow.otherTable_Money[player][player2].Width = otherBoxWidth;
                        MainWindow.otherTable_Money[player][player2].Height = otherBoxHeight;
                        MainWindow.otherTable_Money[player][player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        MainWindow.otherTable_Money[player][player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        MainWindow.otherTable_Money[player][player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1) + 2 * otherBoxWidth * otherPlayers, 5 * totalHeight / 8, 0, 0);
                        MainWindow.playerTurns[player].grid.Children.Add(MainWindow.otherNames[player][player2]);
                        MainWindow.otherNames[player][player2].Height = 5* totalHeight / 96;
                        MainWindow.otherNames[player][player2].Width = otherBoxWidth;//2 * otherBoxWidth + otherMargin;
                        MainWindow.otherNames[player][player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        MainWindow.otherNames[player][player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        MainWindow.otherNames[player][player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1.5) + otherBoxWidth * (2*otherPlayers+0.5), 49 * totalHeight / 96, 0, 0);
                        MainWindow.otherNames[player][player2].Content = MainWindow.playerNames[player2];
                        MainWindow.otherNames[player][player2].HorizontalContentAlignment = HorizontalAlignment.Center;
                        MainWindow.otherNames[player][player2].VerticalContentAlignment = VerticalAlignment.Center;
                        MainWindow.otherNames[player][player2].Background = Brushes.DarkRed;
                        //MainWindow.otherNames[player][player2].Click += MainWindow.playerTurns[MainWindow.player2].OtherPlayer_Click;
                        MainWindow.playerTurns[player].grid.Children.Add(MainWindow.otherMoneyLabels[player][player2]);
                        MainWindow.otherMoneyLabels[player][player2].Height = totalHeight / 24;
                        MainWindow.otherMoneyLabels[player][player2].Width = otherBoxWidth;
                        MainWindow.otherMoneyLabels[player][player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        MainWindow.otherMoneyLabels[player][player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        MainWindow.otherMoneyLabels[player][player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1) + 2 * otherBoxWidth * otherPlayers, 55 * totalHeight / 96, 0, 0);
                        MainWindow.otherMoneyLabels[player][player2].Text = "Money:";
                        MainWindow.otherMoneyLabels[player][player2].TextAlignment = TextAlignment.Center;
                        MainWindow.playerTurns[player].grid.Children.Add(MainWindow.otherPropertyLabels[player][player2]);
                        MainWindow.otherPropertyLabels[player][player2].Height = totalHeight / 24;
                        MainWindow.otherPropertyLabels[player][player2].Width = otherBoxWidth;
                        MainWindow.otherPropertyLabels[player][player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        MainWindow.otherPropertyLabels[player][player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        MainWindow.otherPropertyLabels[player][player2].Margin = new Thickness(otherMargin * (2 + 3 * otherPlayers) + otherBoxWidth * (2 * otherPlayers + 1), 55 * totalHeight / 96, 0, 0);
                        MainWindow.otherPropertyLabels[player][player2].Text = "Properties:";
                        MainWindow.otherPropertyLabels[player][player2].TextAlignment = TextAlignment.Center;
                        MainWindow.playerTurns[player].grid.Children.Add(MainWindow.otherCardsLeft[player][player2]);
                        MainWindow.otherCardsLeft[player][player2].Height = 5*totalHeight/96;
                        MainWindow.otherCardsLeft[player][player2].Width = (otherBoxWidth + otherMargin) / 6;
                        MainWindow.otherCardsLeft[player][player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1.333333) + otherBoxWidth *(2* otherPlayers+0.33333), 49 * totalHeight / 96, 0, 0);
                        MainWindow.otherCardsLeft[player][player2].Text = MainWindow.AllHands[player2].Count().ToString();
                        MainWindow.otherCardsLeft[player][player2].VerticalAlignment = VerticalAlignment.Top;
                        MainWindow.otherCardsLeft[player][player2].HorizontalAlignment = HorizontalAlignment.Left;
                        MainWindow.otherCardsLeft[player][player2].VerticalContentAlignment = VerticalAlignment.Center;
                        MainWindow.otherCardsLeft[player][player2].HorizontalContentAlignment = HorizontalAlignment.Center;
                        MainWindow.otherCardsLeft[player][player2].BorderThickness = new Thickness(0, 0, 0, 0);
                        MainWindow.otherCardsLeft[player][player2].Padding = new Thickness(0, 0, 0, 0);
                        MainWindow.playerTurns[player].grid.Children.Add(MainWindow.otherCardsLeftText[player][player2]);
                        MainWindow.otherCardsLeftText[player][player2].Height = 5 * totalHeight / 96;
                        MainWindow.otherCardsLeftText[player][player2].Width = (otherBoxWidth + otherMargin) / 3;
                        MainWindow.otherCardsLeftText[player][player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1) + otherBoxWidth * (2 * otherPlayers ), 49 * totalHeight / 96, 0, 0);
                        MainWindow.otherCardsLeftText[player][player2].Text = "Cards in Hand:";
                        MainWindow.otherCardsLeftText[player][player2].VerticalAlignment = VerticalAlignment.Top;
                        MainWindow.otherCardsLeftText[player][player2].HorizontalAlignment = HorizontalAlignment.Left;
                        MainWindow.otherCardsLeftText[player][player2].VerticalContentAlignment = VerticalAlignment.Center;
                        MainWindow.otherCardsLeftText[player][player2].HorizontalContentAlignment = HorizontalAlignment.Right;
                        MainWindow.otherCardsLeftText[player][player2].BorderThickness = new Thickness(0, 0, 0, 0);
                        MainWindow.otherCardsLeftText[player][player2].Padding = new Thickness(0, 0, 0, 0);
                        MainWindow.playerTurns[player].grid.Children.Add(MainWindow.otherTurnsLeftText[player][player2]);
                        MainWindow.otherTurnsLeftText[player][player2].Height = 5 * totalHeight / 96;
                        MainWindow.otherTurnsLeftText[player][player2].Width = (otherBoxWidth + otherMargin) / 3;
                        MainWindow.otherTurnsLeftText[player][player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1.5) + otherBoxWidth * (2 * otherPlayers +1.5), 49 * totalHeight / 96, 0, 0);
                        MainWindow.otherTurnsLeftText[player][player2].Text = "Turns Left:";
                        MainWindow.otherTurnsLeftText[player][player2].VerticalAlignment = VerticalAlignment.Top;
                        MainWindow.otherTurnsLeftText[player][player2].HorizontalAlignment = HorizontalAlignment.Left;
                        MainWindow.otherTurnsLeftText[player][player2].VerticalContentAlignment = VerticalAlignment.Center;
                        MainWindow.otherTurnsLeftText[player][player2].HorizontalContentAlignment = HorizontalAlignment.Right;
                        MainWindow.otherTurnsLeftText[player][player2].BorderThickness = new Thickness(0, 0, 0, 0);
                        MainWindow.otherTurnsLeftText[player][player2].Padding = new Thickness(0, 0, 0, 0);
                        MainWindow.playerTurns[player].grid.Children.Add(MainWindow.otherTurnsLeft[player][player2]);
                        MainWindow.otherTurnsLeft[player][player2].Height = 5 * totalHeight / 96;
                        MainWindow.otherTurnsLeft[player][player2].Width = (otherBoxWidth + otherMargin) / 6;
                        MainWindow.otherTurnsLeft[player][player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1.86667) + otherBoxWidth * (2 * otherPlayers + 1.866667), 49 * totalHeight / 96, 0, 0);
                        if (player2 == MainWindow.playerNum)
                        {
                            MainWindow.otherTurnsLeft[player][player2].Text = MainWindow.playNum.ToString();
                        }
                        else //Someone else
                        {
                            MainWindow.otherTurnsLeft[player][player2].Text = "0";
                        }
                        MainWindow.otherTurnsLeft[player][player2].VerticalAlignment = VerticalAlignment.Top;
                        MainWindow.otherTurnsLeft[player][player2].HorizontalAlignment = HorizontalAlignment.Left;
                        MainWindow.otherTurnsLeft[player][player2].VerticalContentAlignment = VerticalAlignment.Center;
                        MainWindow.otherTurnsLeft[player][player2].HorizontalContentAlignment = HorizontalAlignment.Center;
                        MainWindow.otherTurnsLeft[player][player2].BorderThickness = new Thickness(0, 0, 0, 0);
                        MainWindow.otherTurnsLeft[player][player2].Padding = new Thickness(0, 0, 0, 0);
                        otherPlayers++;
                    }
                }
            }
        }

        public static void resizeLayout()
        {
            for (int player = 0; player < numOfPlayers; player++)
            {
                double totalWidth = MainWindow.playerTurns[player].window.RenderSize.Width;
                double totalHeight = MainWindow.playerTurns[player].window.RenderSize.Height;
                double boxWidth = 7 * totalWidth / 48;
                double boxHeight = totalHeight / 4;
                MainWindow.playerTurns[player].grid.Width = totalWidth;
                MainWindow.playerTurns[player].grid.Height = totalHeight;
                MainWindow.playerTurns[player].grid.Margin = new Thickness(0, 0, 0, 0);
                MainWindow.playerTurns[player].Hand.Width = boxWidth;
                MainWindow.playerTurns[player].Table_Money.Width = boxWidth;
                MainWindow.playerTurns[player].Table_Properties.Width = boxWidth;
                MainWindow.playerTurns[player].Hand.Margin = new Thickness((totalWidth / 2 - 3 * boxWidth) / 6, totalHeight / 8, 0, 0);
                MainWindow.playerTurns[player].Table_Money.Margin = new Thickness(boxWidth + ((totalWidth / 2 - 3 * boxWidth) / 2), totalHeight / 8, 0, 0);
                MainWindow.playerTurns[player].Table_Properties.Margin = new Thickness(2 * boxWidth + 5 * (totalWidth / 2 - 3 * boxWidth) / 6, totalHeight / 8, 0, 0);
                MainWindow.playerTurns[player].handLabel.Width = boxWidth;
                MainWindow.playerTurns[player].handLabel.Height = totalHeight / 24;
                MainWindow.playerTurns[player].handLabel.Margin = new Thickness((totalWidth / 2 - 3 * boxWidth) / 6, 7 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].moneyLabel.Width = boxWidth;
                MainWindow.playerTurns[player].moneyLabel.Height = totalHeight / 24;
                MainWindow.playerTurns[player].moneyLabel.Margin = new Thickness(boxWidth + ((totalWidth / 2 - 3 * boxWidth) / 2), 7 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].propertiesLabel.Width = boxWidth;
                MainWindow.playerTurns[player].propertiesLabel.Height = totalHeight / 24;
                MainWindow.playerTurns[player].propertiesLabel.Margin = new Thickness(2 * boxWidth + 5 * (totalWidth / 2 - 3 * boxWidth) / 6, 7 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].buttonPlayer.Width = totalWidth / 16;
                MainWindow.playerTurns[player].buttonPlayer.Height = 5 * totalHeight / 96;
                MainWindow.playerTurns[player].buttonPlayer.Margin = new Thickness((totalWidth / 2 - 3 * boxWidth) / 6, totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].Prompt.Width = 10 * totalWidth / 32;
                MainWindow.playerTurns[player].Prompt.Height = 5 * totalHeight / 96;
                MainWindow.playerTurns[player].Prompt.Margin = new Thickness(4 * totalWidth / 48, totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].cardsLeftText.Width = 4 * totalWidth / 64;
                MainWindow.playerTurns[player].cardsLeftText.Height = 2 * totalHeight / 96;
                MainWindow.playerTurns[player].cardsLeftText.Margin = new Thickness(39 * totalWidth / 96, totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].deckCountDisplay.Width = 2 * totalWidth / 64;
                MainWindow.playerTurns[player].deckCountDisplay.Height = 2 * totalHeight / 96;
                MainWindow.playerTurns[player].deckCountDisplay.Margin = new Thickness(30 * totalWidth / 64, totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].turnsLeftText.Width = 4 * totalWidth / 64;
                MainWindow.playerTurns[player].turnsLeftText.Height = 2 * totalHeight / 96;
                MainWindow.playerTurns[player].turnsLeftText.Margin = new Thickness(39 * totalWidth / 96, 4 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].turnsLeftDisplay.Width = 2 * totalWidth / 64;
                MainWindow.playerTurns[player].turnsLeftDisplay.Height = 2 * totalHeight / 96;
                MainWindow.playerTurns[player].turnsLeftDisplay.Margin = new Thickness(30 * totalWidth / 64, 4 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].buttonBack.Width = 4 * totalWidth / 64;
                MainWindow.playerTurns[player].buttonBack.Height = 6 * totalHeight / 96;
                MainWindow.playerTurns[player].buttonBack.Margin = new Thickness(totalWidth / 96, 37 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].button1.Width = 8 * totalWidth / 64;
                MainWindow.playerTurns[player].button1.Height = 6 * totalHeight / 96;
                MainWindow.playerTurns[player].button1.Margin = new Thickness(8 * totalWidth / 96, 37 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].button2.Width = 8 * totalWidth / 64;
                MainWindow.playerTurns[player].button2.Height = 6 * totalHeight / 96;
                MainWindow.playerTurns[player].button2.Margin = new Thickness(21 * totalWidth / 96, 37 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].button3.Width = 8 * totalWidth / 64;
                MainWindow.playerTurns[player].button3.Height = 6 * totalHeight / 96;
                MainWindow.playerTurns[player].button3.Margin = new Thickness(34 * totalWidth / 96, 37 * totalHeight / 96, 0, 0);
                MainWindow.playerTurns[player].universalPrompt.Width = 46 * totalWidth / 96;
                MainWindow.playerTurns[player].universalPrompt.Height = 46 * totalHeight / 96;
                MainWindow.playerTurns[player].universalPrompt.Margin = new Thickness(48 * totalWidth / 96, totalHeight / 96, 0, 0);

                int otherPlayers = 0;
                for (int player2 = 0; player2 < MainWindow.numOfPlayers; player2++)
                {
                    double otherBoxWidth = totalWidth / ((MainWindow.numOfPlayers - 1) * 2 + 1);
                    otherBoxWidth = Math.Min(otherBoxWidth, boxWidth);
                    double otherBoxHeight = totalHeight / 4;
                    double otherMargin = (totalWidth - otherBoxWidth * 2 * (MainWindow.numOfPlayers - 1)) / (3 * (MainWindow.numOfPlayers - 1));
                    if (player2 != player)
                    {
                        MainWindow.otherTable_Properties[player][player2].Width = otherBoxWidth;
                        MainWindow.otherTable_Properties[player][player2].Height = otherBoxHeight;
                        MainWindow.otherTable_Properties[player][player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        MainWindow.otherTable_Properties[player][player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        MainWindow.otherTable_Properties[player][player2].Margin = new Thickness(otherMargin * (2 + 3 * otherPlayers) + otherBoxWidth * (2 * otherPlayers + 1), 5 * totalHeight / 8, 0, 0);
                        MainWindow.otherTable_Properties[player][player2].Name = MainWindow.playerNames[player2];
                        MainWindow.otherTable_Money[player][player2].Width = otherBoxWidth;
                        MainWindow.otherTable_Money[player][player2].Height = otherBoxHeight;
                        MainWindow.otherTable_Money[player][player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        MainWindow.otherTable_Money[player][player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        MainWindow.otherTable_Money[player][player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1) + 2 * otherBoxWidth * otherPlayers, 5 * totalHeight / 8, 0, 0);
                        MainWindow.otherNames[player][player2].Height = totalHeight / 24;
                        MainWindow.otherNames[player][player2].Width = otherBoxWidth;
                        MainWindow.otherNames[player][player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        MainWindow.otherNames[player][player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        MainWindow.otherNames[player][player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1.5) + otherBoxWidth * (2 * otherPlayers + 0.5), 49 * totalHeight / 96, 0, 0);
                        MainWindow.otherNames[player][player2].Content = MainWindow.playerNames[player2];
                        MainWindow.otherNames[player][player2].HorizontalContentAlignment = HorizontalAlignment.Center;
                        MainWindow.otherNames[player][player2].VerticalContentAlignment = VerticalAlignment.Center;
                        MainWindow.otherNames[player][player2].Background = Brushes.DarkRed;
                        MainWindow.otherMoneyLabels[player][player2].Height = totalHeight / 24;
                        MainWindow.otherMoneyLabels[player][player2].Width = otherBoxWidth;
                        MainWindow.otherMoneyLabels[player][player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        MainWindow.otherMoneyLabels[player][player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        MainWindow.otherMoneyLabels[player][player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1) + 2 * otherBoxWidth * otherPlayers, 55 * totalHeight / 96, 0, 0);
                        MainWindow.otherMoneyLabels[player][player2].Text = "Money:";
                        MainWindow.otherMoneyLabels[player][player2].TextAlignment = TextAlignment.Center;
                        MainWindow.otherPropertyLabels[player][player2].Height = totalHeight / 24;
                        MainWindow.otherPropertyLabels[player][player2].Width = otherBoxWidth;
                        MainWindow.otherPropertyLabels[player][player2].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        MainWindow.otherPropertyLabels[player][player2].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        MainWindow.otherPropertyLabels[player][player2].Margin = new Thickness(otherMargin * (2 + 3 * otherPlayers) + otherBoxWidth * (2 * otherPlayers + 1), 55 * totalHeight / 96, 0, 0);
                        MainWindow.otherPropertyLabels[player][player2].Text = "Properties:";
                        MainWindow.otherPropertyLabels[player][player2].TextAlignment = TextAlignment.Center;
                        MainWindow.otherCardsLeft[player][player2].Height = 5 * totalHeight / 96;
                        MainWindow.otherCardsLeft[player][player2].Width = (otherBoxWidth + otherMargin) / 6;
                        MainWindow.otherCardsLeft[player][player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1.333333) + otherBoxWidth * (2 * otherPlayers + 0.33333), 49 * totalHeight / 96, 0, 0);
                        MainWindow.otherCardsLeft[player][player2].Text = MainWindow.AllHands[player2].Count().ToString();
                        MainWindow.otherCardsLeft[player][player2].VerticalAlignment = VerticalAlignment.Top;
                        MainWindow.otherCardsLeft[player][player2].HorizontalAlignment = HorizontalAlignment.Left;
                        MainWindow.otherCardsLeft[player][player2].VerticalContentAlignment = VerticalAlignment.Center;
                        MainWindow.otherCardsLeft[player][player2].HorizontalContentAlignment = HorizontalAlignment.Center;
                        MainWindow.otherCardsLeft[player][player2].BorderThickness = new Thickness(0, 0, 0, 0);
                        MainWindow.otherCardsLeft[player][player2].Padding = new Thickness(0, 0, 0, 0);
                        MainWindow.otherCardsLeftText[player][player2].Height = 5 * totalHeight / 96;
                        MainWindow.otherCardsLeftText[player][player2].Width = (otherBoxWidth + otherMargin) / 3;
                        MainWindow.otherCardsLeftText[player][player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1) + otherBoxWidth * (2 * otherPlayers), 49 * totalHeight / 96, 0, 0);
                        MainWindow.otherCardsLeftText[player][player2].Text = "Cards in Hand:";
                        MainWindow.otherCardsLeftText[player][player2].VerticalAlignment = VerticalAlignment.Top;
                        MainWindow.otherCardsLeftText[player][player2].HorizontalAlignment = HorizontalAlignment.Left;
                        MainWindow.otherCardsLeftText[player][player2].VerticalContentAlignment = VerticalAlignment.Center;
                        MainWindow.otherCardsLeftText[player][player2].HorizontalContentAlignment = HorizontalAlignment.Right;
                        MainWindow.otherCardsLeftText[player][player2].BorderThickness = new Thickness(0, 0, 0, 0);
                        MainWindow.otherCardsLeftText[player][player2].Padding = new Thickness(0, 0, 0, 0);
                        MainWindow.otherTurnsLeftText[player][player2].Height = 5 * totalHeight / 96;
                        MainWindow.otherTurnsLeftText[player][player2].Width = (otherBoxWidth + otherMargin) / 3;
                        MainWindow.otherTurnsLeftText[player][player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1.5) + otherBoxWidth * (2 * otherPlayers + 1.5), 49 * totalHeight / 96, 0, 0);
                        MainWindow.otherTurnsLeftText[player][player2].Text = "Turns Left:";
                        MainWindow.otherTurnsLeftText[player][player2].VerticalAlignment = VerticalAlignment.Top;
                        MainWindow.otherTurnsLeftText[player][player2].HorizontalAlignment = HorizontalAlignment.Left;
                        MainWindow.otherTurnsLeftText[player][player2].VerticalContentAlignment = VerticalAlignment.Center;
                        MainWindow.otherTurnsLeftText[player][player2].HorizontalContentAlignment = HorizontalAlignment.Right;
                        MainWindow.otherTurnsLeftText[player][player2].BorderThickness = new Thickness(0, 0, 0, 0);
                        MainWindow.otherTurnsLeftText[player][player2].Padding = new Thickness(0, 0, 0, 0);
                        MainWindow.otherTurnsLeft[player][player2].Height = 5 * totalHeight / 96;
                        MainWindow.otherTurnsLeft[player][player2].Width = (otherBoxWidth + otherMargin) / 6;
                        MainWindow.otherTurnsLeft[player][player2].Margin = new Thickness(otherMargin * (3 * otherPlayers + 1.86667) + otherBoxWidth * (2 * otherPlayers + 1.866667), 49 * totalHeight / 96, 0, 0);
                        if (player2 == MainWindow.playerNum)
                        {
                            MainWindow.otherTurnsLeft[player][player2].Text = MainWindow.playNum.ToString();
                        }
                        else //Someone else
                        {
                            MainWindow.otherTurnsLeft[player][player2].Text = "0";
                        }
                        MainWindow.otherTurnsLeft[player][player2].VerticalAlignment = VerticalAlignment.Top;
                        MainWindow.otherTurnsLeft[player][player2].HorizontalAlignment = HorizontalAlignment.Left;
                        MainWindow.otherTurnsLeft[player][player2].VerticalContentAlignment = VerticalAlignment.Center;
                        MainWindow.otherTurnsLeft[player][player2].HorizontalContentAlignment = HorizontalAlignment.Center;
                        MainWindow.otherTurnsLeft[player][player2].BorderThickness = new Thickness(0, 0, 0, 0);
                        MainWindow.otherTurnsLeft[player][player2].Padding = new Thickness(0, 0, 0, 0);
                        otherPlayers++;
                    }
                }
            }
        }

        public static void shuffleDeck()
        {
            List<Card> deckBeginningDuplicate = new List<Card>(deckBeginning);
            int numOfCards = deckBeginning.Count;
            Random rnd = new Random();
            for (int i = 0; i < numOfCards; i++)
            {
                int randomNum = rnd.Next(0, numOfCards - 1 - i);
                deckShuffled.Add(deckBeginningDuplicate[randomNum]);
                deckBeginningDuplicate.RemoveAt(randomNum);
            }
        }

        public static void shuffleDiscards()
        {
             List<Card> deckDuplicate = new List<Card>(deckDiscard);
            int numOfCards = deckDiscard.Count;
            Random rnd = new Random();
            for (int i = 0; i < numOfCards; i++)
            {
                int randomNum = rnd.Next(0, numOfCards - 1 - i);
                deckShuffled.Add(deckDuplicate[randomNum]);
                deckDuplicate.RemoveAt(randomNum);
            }
        }

        public static void dealDeck()
        {
            for (int cardNum = 0; cardNum < cardsToDeal; cardNum++)
            {
                for (int player = 0; player < numOfPlayers; player++)
                {
                    AllHands[player].Add(deckShuffled[0]);
                    deckShuffled.RemoveAt(0);
                }
            }
        }

        public static void sortMoney()
        {
            for (int player = 0; player < MainWindow.numOfPlayers; player++)
            {
                List<Card> newMoney= new List<Card>();
                int count = MainWindow.AllTableMoney[player].Count;
                while (newMoney.Count < count)
                {
                    int minValue = 20;
                    Card minCard = Card.Ten;
                    for (int index = 0; index < MainWindow.AllTableMoney[player].Count; index++)
                    {
                        int currentValue = getValue(MainWindow.AllTableMoney[player][index]);
                        if (currentValue < minValue)
                        {
                            minValue = currentValue;
                            minCard = MainWindow.AllTableMoney[player][index];
                        }
                    }
                    newMoney.Add(minCard);
                    MainWindow.AllTableMoney[player].Remove(minCard);
                }
                MainWindow.AllTableMoney[player] = newMoney;
            }
        }

        public static CardType getCardType(Card card)
        {
            switch (card)
            {
                case Card.One:
                    return CardType.Money;
                case Card.Two:
                    return CardType.Money;
                case Card.Three:
                    return CardType.Money;
                case Card.Four:
                    return CardType.Money;
                case Card.Five:
                    return CardType.Money;
                case Card.Ten:
                    return CardType.Money;
                case Card.PassGo__1:
                    return CardType.Action;
                case Card.DoubleTheRent__1:
                    return CardType.Action;
                case Card.Birthday__2:
                    return CardType.Action;
                case Card.House__3:
                    return CardType.Action;
                case Card.Hotel__4:
                    return CardType.Action;
                case Card.JustSayNo__4:
                    return CardType.Action;
                case Card.DealBreaker__5:
                    return CardType.Action;
                case Card.SlyDeal__3:
                    return CardType.Action;
                case Card.ForcedDeal__3:
                    return CardType.Action;
                case Card.DebtCollector__3:
                    return CardType.Action;
                case Card.RentWild__3:
                    return CardType.Action;
                case Card.RentBrownLightBlue__1:
                    return CardType.Action;
                case Card.RentRedYellow__1:
                    return CardType.Action;
                case Card.RentGreenBlue__1:
                    return CardType.Action;
                case Card.RentPinkOrange__1:
                    return CardType.Action;
                case Card.RentBlackUtility__1:
                    return CardType.Action;
                case Card.PropertyYellow__3:
                    return CardType.Property;
                case Card.PropertyOrange__2:
                    return CardType.Property;
                case Card.PropertyPink__2:
                    return CardType.Property;
                case Card.PropertyRed__3:
                    return CardType.Property;
                case Card.PropertyLightBlue__1:
                    return CardType.Property;
                case Card.PropertyGreen__4:
                    return CardType.Property;
                case Card.PropertyBlack__2:
                    return CardType.Property;
                case Card.PropertyBrown__1:
                    return CardType.Property;
                case Card.PropertyUtility__1:
                    return CardType.Property;
                case Card.PropertyBlue__4:
                    return CardType.Property;
                case Card.PropertyWild:
                    return CardType.Property;
                case Card.PropertyRedYellow__3:
                    return CardType.Property;
                case Card.PropertyPinkOrange__2:
                    return CardType.Property;
                case Card.PropertyLightBlueBlack__4:
                    return CardType.Property;
                case Card.PropertyUtilityBlack__2:
                    return CardType.Property;
                case Card.PropertyBrownLightBlue__1:
                    return CardType.Property;
                case Card.PropertyBlueGreen__4:
                    return CardType.Property;
                case Card.PropertyGreenBlack__4:
                    return CardType.Property;
            }
            return CardType.Error;
        }

        public static PropertyType getPropertyType(Card card)
        {
            switch (card)
            {
                case Card.PropertyYellow__3:
                    return PropertyType.Normal;
                case Card.PropertyOrange__2:
                    return PropertyType.Normal;
                case Card.PropertyPink__2:
                    return PropertyType.Normal;
                case Card.PropertyRed__3:
                    return PropertyType.Normal;
                case Card.PropertyLightBlue__1:
                    return PropertyType.Normal;
                case Card.PropertyGreen__4:
                    return PropertyType.Normal;
                case Card.PropertyBlack__2:
                    return PropertyType.Normal;
                case Card.PropertyBrown__1:
                    return PropertyType.Normal;
                case Card.PropertyUtility__1:
                    return PropertyType.Normal;
                case Card.PropertyBlue__4:
                    return PropertyType.Normal;
                case Card.PropertyRedYellow__3:
                    return PropertyType.Duo;
                case Card.PropertyYellowRed__3:
                    return PropertyType.Duo;
                case Card.PropertyPinkOrange__2:
                    return PropertyType.Duo;
                case Card.PropertyOrangePink__2:
                    return PropertyType.Duo;
                case Card.PropertyLightBlueBlack__4:
                    return PropertyType.Duo;
                case Card.PropertyBlackLightBlue__4:
                    return PropertyType.Duo;
                case Card.PropertyUtilityBlack__2:
                    return PropertyType.Duo;
                case Card.PropertyBlackUtility__2:
                    return PropertyType.Duo;
                case Card.PropertyBrownLightBlue__1:
                    return PropertyType.Duo;
                case Card.PropertyLightBlueBrown__1:
                    return PropertyType.Duo;
                case Card.PropertyBlueGreen__4:
                    return PropertyType.Duo;
                case Card.PropertyGreenBlue__4:
                    return PropertyType.Duo;
                case Card.PropertyGreenBlack__4:
                    return PropertyType.Duo;
                case Card.PropertyBlackGreen__4:
                    return PropertyType.Duo;
                case Card.PropertyWild:
                    return PropertyType.Wild;
                case Card.PropertyWildBrown:
                    return PropertyType.Wild;
                case Card.PropertyWildUtility:
                    return PropertyType.Wild;
                case Card.PropertyWildBlue:
                    return PropertyType.Wild;
                case Card.PropertyWildLightBlue:
                    return PropertyType.Wild;
                case Card.PropertyWildPink:
                    return PropertyType.Wild;
                case Card.PropertyWildOrange:
                    return PropertyType.Wild;
                case Card.PropertyWildRed:
                    return PropertyType.Wild;
                case Card.PropertyWildYellow:
                    return PropertyType.Wild;
                case Card.PropertyWildGreen:
                    return PropertyType.Wild;
                case Card.PropertyWildBlack:
                    return PropertyType.Wild;
            }
            return PropertyType.Normal;
        }

        public static int getValue(Card card)
        {
            switch (card)
            {
                case Card.One:
                    return 1;
                case Card.Two:
                    return 2;
                case Card.Three:
                    return 3;
                case Card.Four:
                    return 4;
                case Card.Five:
                    return 5;
                case Card.Ten:
                    return 10;
                case Card.PassGo__1:
                    return 1;
                case Card.DoubleTheRent__1:
                    return 1;
                case Card.Birthday__2:
                    return 2;
                case Card.House__3:
                    return 3;
                case Card.Hotel__4:
                    return 4;
                case Card.JustSayNo__4:
                    return 4;
                case Card.DealBreaker__5:
                    return 5;
                case Card.SlyDeal__3:
                    return 3;
                case Card.ForcedDeal__3:
                    return 3;
                case Card.DebtCollector__3:
                    return 3;
                case Card.RentWild__3:
                    return 3;
                case Card.RentBrownLightBlue__1:
                    return 1;
                case Card.RentRedYellow__1:
                    return 1;
                case Card.RentGreenBlue__1:
                    return 1;
                case Card.RentPinkOrange__1:
                    return 1;
                case Card.RentBlackUtility__1:
                    return 1;
                case Card.PropertyYellow__3:
                    return 3;
                case Card.PropertyOrange__2:
                    return 2;
                case Card.PropertyPink__2:
                    return 2;
                case Card.PropertyRed__3:
                    return 3;
                case Card.PropertyLightBlue__1:
                    return 1;
                case Card.PropertyGreen__4:
                    return 4;
                case Card.PropertyBlack__2:
                    return 2;
                case Card.PropertyBrown__1:
                    return 1;
                case Card.PropertyUtility__1:
                    return 1;
                case Card.PropertyBlue__4:
                    return 4;
                case Card.PropertyWild:
                    return 0;
                case Card.PropertyRedYellow__3:
                    return 3;
                case Card.PropertyYellowRed__3:
                    return 3;
                case Card.PropertyPinkOrange__2:
                    return 2;
                case Card.PropertyOrangePink__2:
                    return 2;
                case Card.PropertyLightBlueBlack__4:
                    return 4;
                case Card.PropertyBlackLightBlue__4:
                    return 4;
                case Card.PropertyUtilityBlack__2:
                    return 2;
                case Card.PropertyBlackUtility__2:
                    return 2;
                case Card.PropertyBrownLightBlue__1:
                    return 1;
                case Card.PropertyLightBlueBrown__1:
                    return 1;
                case Card.PropertyBlueGreen__4:
                    return 4;
                case Card.PropertyGreenBlue__4:
                    return 4;
                case Card.PropertyGreenBlack__4:
                    return 4;
                case Card.PropertyBlackGreen__4:
                    return 4;
                case Card.PropertyWildBrown:
                    return 0;
                case Card.PropertyWildUtility:
                    return 0;
                case Card.PropertyWildBlue:
                    return 0;
                case Card.PropertyWildLightBlue:
                    return 0;
                case Card.PropertyWildPink:
                    return 0;
                case Card.PropertyWildOrange:
                    return 0;
                case Card.PropertyWildRed:
                    return 0;
                case Card.PropertyWildYellow:
                    return 0;
                case Card.PropertyWildGreen:
                    return 0;
                case Card.PropertyWildBlack:
                    return 0;
            }
            return 0;
        }

        //[brown,utility,blue,lightblue,pink,orange,red,yellow,green,black]
        public static int getPropertyIndex(Card card)
        {
            switch (card)
            {
                case Card.PropertyBrown__1:
                    return 0;
                case Card.PropertyBrownLightBlue__1:
                    return 0;
                case Card.PropertyWildBrown:
                    return 0;
                case Card.PropertyUtility__1:
                    return 1;
                case Card.PropertyUtilityBlack__2:
                    return 1;
                case Card.PropertyWildUtility:
                    return 1;
                case Card.PropertyBlue__4:
                    return 2;
                case Card.PropertyBlueGreen__4:
                    return 2;
                case Card.PropertyWildBlue:
                    return 2;
                case Card.PropertyLightBlue__1:
                    return 3;
                case Card.PropertyLightBlueBlack__4:
                    return 3;
                case Card.PropertyLightBlueBrown__1:
                    return 3;
                case Card.PropertyWildLightBlue:
                    return 3;
                case Card.PropertyPink__2:
                    return 4;
                case Card.PropertyPinkOrange__2:
                    return 4;
                case Card.PropertyWildPink:
                    return 4;
                case Card.PropertyOrange__2:
                    return 5;
                case Card.PropertyOrangePink__2:
                    return 5;
                case Card.PropertyWildOrange:
                    return 5;
                case Card.PropertyRed__3:
                    return 6;
                case Card.PropertyRedYellow__3:
                    return 6;
                case Card.PropertyWildRed:
                    return 6;
                case Card.PropertyYellow__3:
                    return 7;
                case Card.PropertyYellowRed__3:
                    return 7;
                case Card.PropertyWildYellow:
                    return 7;
                case Card.PropertyGreen__4:
                    return 8;
                case Card.PropertyGreenBlack__4:
                    return 8;
                case Card.PropertyGreenBlue__4:
                    return 8;
                case Card.PropertyWildGreen:
                    return 8;
                case Card.PropertyBlack__2:
                    return 9;
                case Card.PropertyBlackGreen__4:
                    return 9;
                case Card.PropertyBlackLightBlue__4:
                    return 9;
                case Card.PropertyBlackUtility__2:
                    return 9;
                case Card.PropertyWildBlack:
                    return 9;
                case Card.PropertyWild:
                    return 10;
            }
            return 11;
        }

        public static int getRentAmount()
        {
            int numOfProps = MainWindow.AllTableProperties[MainWindow.playerNum][MainWindow.propIndex].Count;
            switch (MainWindow.propIndex)
            {
                case 0:
                    switch (numOfProps)
                    {
                        case 0:
                            return 0;
                        case 1:
                            return 1;
                        case 2:
                            return 2;
                        case 3:
                            return 5;
                        case 4:
                            return 9;
                    }
                    return 100;

                case 1:
                    switch (numOfProps)
                    {
                        case 0:
                            return 0;
                        case 1:
                            return 1;
                        case 2:
                            return 2;
                        case 3:
                            return 5;
                        case 4:
                            return 9;
                    }
                    return 100;

                case 2:
                    switch (numOfProps)
                    {
                        case 0:
                            return 0;
                        case 1:
                            return 3;
                        case 2:
                            return 8;
                        case 3:
                            return 11;
                        case 4:
                            return 15;
                    }
                    return 100;

                case 3:
                    switch (numOfProps)
                    {
                        case 0:
                            return 0;
                        case 1:
                            return 1;
                        case 2:
                            return 2;
                        case 3:
                            return 3;
                        case 4:
                            return 6;
                        case 5:
                            return 10;
                    }
                    return 100;

                case 4:
                    switch (numOfProps)
                    {
                        case 0:
                            return 0;
                        case 1:
                            return 1;
                        case 2:
                            return 2;
                        case 3:
                            return 4;
                        case 4:
                            return 7;
                        case 5:
                            return 11;
                    }
                    return 100;

                case 5:
                    switch (numOfProps)
                    {
                        case 0:
                            return 0;
                        case 1:
                            return 1;
                        case 2:
                            return 3;
                        case 3:
                            return 5;
                        case 4:
                            return 8;
                        case 5:
                            return 12;
                    }
                    return 100;

                case 6:
                    switch (numOfProps)
                    {
                        case 0:
                            return 0;
                        case 1:
                            return 2;
                        case 2:
                            return 3;
                        case 3:
                            return 6;
                        case 4:
                            return 9;
                        case 5:
                            return 13;
                    }
                    return 100;

                case 7:
                    switch (numOfProps)
                    {
                        case 0:
                            return 0;
                        case 1:
                            return 2;
                        case 2:
                            return 4;
                        case 3:
                            return 6;
                        case 4:
                            return 9;
                        case 5:
                            return 13;
                    }
                    return 100;

                case 8:
                    switch (numOfProps)
                    {
                        case 0:
                            return 0;
                        case 1:
                            return 2;
                        case 2:
                            return 4;
                        case 3:
                            return 7;
                        case 4:
                            return 10;
                        case 5:
                            return 14;
                    }
                    return 100;

                case 9:
                    switch (numOfProps)
                    {
                        case 0:
                            return 0;
                        case 1:
                            return 1;
                        case 2:
                            return 2;
                        case 3:
                            return 3;
                        case 4:
                            return 4;
                        case 5:
                            return 7;
                        case 6:
                            return 11;
                    }
                    return 100;
            }
            return 0;
        }

        public static void updateMonopolies()
        {
            for (int player = 0; player < numOfPlayers; player++)
            {
                for (int propertyIndex = 0; propertyIndex < 10; propertyIndex++)
                {
                    if (AllTableProperties[player][propertyIndex].Count >= monopolyPropsNeeded[propertyIndex])
                    {
                        //isMonopoly[player][propertyIndex] = true;
                    }
                }
            }
        }

        public static int numOfHouseOptions()
        {
            int options = 0;
            for (int i = 0; i < 10; i++)
            {
                if (AllTableProperties[playerNum][i].Count == monopolyPropsNeeded[i])
                {
                    options++;
                }
            }
            return options;
        }

        public static int numOfHotelOptions()
        {
            int options = 0;
            for (int i = 0; i < 10; i++)
            {
                if (AllTableProperties[playerNum][i].Count == (monopolyPropsNeeded[i] + 1))
                {
                    options++;
                }
            }
            return options;
        }

        public static int numOfMonopolies()
        {
            int options = 0;
            for (int i = 0; i < 10; i++)
            {
                if (AllTableProperties[chosenPlayer][i].Count >= (monopolyPropsNeeded[i]))
                {
                    options++;
                }
            }
            return options;
        }

        //Check if houses and hotels are still valid...
        public static void checkProperties()
        {
            for (int player = 0; player<numOfPlayers;player++)
            {
                for (int index = 0; index < 10; index++)
                {
                    if (MainWindow.AllTableProperties[player][index].Contains(Card.House__3))
                    {
                        if (MainWindow.AllTableProperties[player][index].IndexOf(Card.House__3) < monopolyPropsNeeded[index])
                        {
                            MainWindow.AllTableMoney[player].Add(Card.House__3);
                            MainWindow.AllTableProperties[player][index].Remove(Card.House__3);
                        }
                    }
                    if (MainWindow.AllTableProperties[player][index].Contains(Card.Hotel__4))
                    {
                        if (MainWindow.AllTableProperties[player][index].IndexOf(Card.Hotel__4) <= monopolyPropsNeeded[index])
                        {
                            MainWindow.AllTableMoney[player].Add(Card.Hotel__4);
                            MainWindow.AllTableProperties[player][index].Remove(Card.Hotel__4);
                        }
                    }
                }
            }
        }

        public static bool checkForWinner()
        {
            for (int player = 0; player < MainWindow.numOfPlayers; player++)
            {
                int numOfMonopolies = 0;
                for (int cardIndex = 0; cardIndex < 10; cardIndex++)
                {
                    if (MainWindow.AllTableProperties[player][cardIndex].Count >= MainWindow.monopolyPropsNeeded[cardIndex])
                    {
                        numOfMonopolies++;
                    }
                }
                if (numOfMonopolies >= 3)
                {
                    MainWindow.playerNum = player;
                    MainWindow.endGame();
                    return true;
                }
            }
            return false;
        }

        public static void endGame()
        {
            for (int player = 0; player < MainWindow.numOfPlayers; player++)
            {
                MainWindow.playerTurns[player].button1.Visibility = Visibility.Hidden;
                MainWindow.playerTurns[player].button2.Visibility = Visibility.Hidden;
                MainWindow.playerTurns[player].button3.Visibility = Visibility.Hidden;
                MainWindow.playerTurns[player].buttonBack.Visibility = Visibility.Hidden;
                MainWindow.playerTurns[player].Prompt.Content = MainWindow.playerNames[MainWindow.playerNum] + " won the game!  Get rocked!";
            }
            MainWindow.playerTurns[MainWindow.playerNum].Prompt.Content = "You won!!! Congratulations!";
        }

        public static void Table_Properties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainWindow.stage == MainWindow.turnStage.acknowledgeAttack)
            {
                int totalValue = 0;
                int numOfWilds = 0;
                foreach (Card card in MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Properties.Items)
                {
                    if (MainWindow.getPropertyType(card) == PropertyType.Wild)
                    {
                        numOfWilds++;
                    }
                }
                foreach (Card card in MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Properties.SelectedItems)
                {
                    if (MainWindow.getPropertyType(card) == PropertyType.Wild)
                    {
                        numOfWilds--;
                    }
                }
                foreach (Card card in MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Properties.SelectedItems)
                {
                    totalValue += MainWindow.getValue(card);

                }
                foreach (Card card in MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Money.SelectedItems)
                {
                    totalValue += MainWindow.getValue(card);
                }
                if ((totalValue >= MainWindow.payment) || ((MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Properties.SelectedItems.Count >= (MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Properties.Items.Count - numOfWilds)) && (MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Money.SelectedItems.Count == MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Money.Items.Count)))
                {
                    MainWindow.playerTurns[MainWindow.chosenPlayer].button3.Content = "Make Payment";
                    MainWindow.playerTurns[MainWindow.chosenPlayer].button3.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    MainWindow.playerTurns[MainWindow.chosenPlayer].button3.Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }

        public static void Table_Money_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainWindow.stage == MainWindow.turnStage.acknowledgeAttack)
            {
                int totalValue = 0;
                int numOfWilds = 0;
                foreach (Card card in MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Properties.Items)
                {
                    if (MainWindow.getPropertyType(card) == PropertyType.Wild)
                    {
                        numOfWilds++;
                    }
                }
                foreach (Card card in MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Properties.SelectedItems)
                {
                    if (MainWindow.getPropertyType(card) == PropertyType.Wild)
                    {
                        numOfWilds--;
                    }
                }
                foreach (Card card in MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Properties.SelectedItems)
                {
                    totalValue += MainWindow.getValue(card);
                }
                foreach (Card card in MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Money.SelectedItems)
                {
                    totalValue += MainWindow.getValue(card);
                }
                if ((totalValue >= MainWindow.payment) || (((MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Properties.SelectedItems.Count == MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Properties.Items.Count - numOfWilds)) && (MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Money.SelectedItems.Count == MainWindow.playerTurns[MainWindow.chosenPlayer].Table_Money.Items.Count)))
                {
                    MainWindow.playerTurns[MainWindow.chosenPlayer].button3.Content = "Make Payment";
                    MainWindow.playerTurns[MainWindow.chosenPlayer].button3.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    MainWindow.playerTurns[MainWindow.chosenPlayer].button3.Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }

        public static void Table_Properties_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MainWindow.playerTurns[MainWindow.playerNum].Table_Properties.SelectedIndex < 0)
            {
                return;
            }
            //if (MainWindow.stage == MainWindow.turnStage.slyDeal)
            //{
            //    MainWindow.cardNum = Table_Properties.SelectedIndex;
            //    MainWindow.propertyCard = Table_Properties.Items.Cast<Card>().ElementAt(MainWindow.cardNum);
            //    string chosenPlayer = buttonPlayer.Content.ToString();
            //    MainWindow.chosenPlayer = (chosenPlayer[7] - 49);
            //    MainWindow.propIndex = MainWindow.getPropertyIndex(MainWindow.propertyCard);
            //    MainWindow.cardNum = MainWindow.AllTableProperties[MainWindow.chosenPlayer][MainWindow.propIndex].IndexOf(MainWindow.propertyCard);
            //    if (MainWindow.chosenPlayer != MainWindow.playerNum)
            //    {
            //        MainWindow.playerTurns[MainWindow.playerNum].checkSlyDeal();
            //    }
            //}

            if (MainWindow.stage == MainWindow.turnStage.forcedDeal1)
            {
                MainWindow.cardNum = MainWindow.playerTurns[MainWindow.playerNum].Table_Properties.SelectedIndex;
                Card currentCard = MainWindow.playerTurns[MainWindow.playerNum].Table_Properties.Items.Cast<Card>().ElementAt(MainWindow.cardNum);
                MainWindow.propIndex = MainWindow.getPropertyIndex(currentCard);
                MainWindow.cardNum = MainWindow.AllTableProperties[MainWindow.playerNum][MainWindow.propIndex].IndexOf(currentCard);
                MainWindow.playerTurns[MainWindow.playerNum].playForcedDeal2();
            }
            //else if (MainWindow.stage == MainWindow.turnStage.forcedDeal2)
            //{
            //    MainWindow.cardNum2 = Table_Properties.SelectedIndex;
            //    MainWindow.propertyCard = Table_Properties.Items.Cast<Card>().ElementAt(MainWindow.cardNum2);
            //    MainWindow.propIndex2 = MainWindow.getPropertyIndex(MainWindow.propertyCard);
            //    string chosenPlayer = buttonPlayer.Content.ToString();
            //    MainWindow.chosenPlayer = (chosenPlayer[7] - 49);
            //    MainWindow.cardNum2 = MainWindow.AllTableProperties[MainWindow.chosenPlayer][MainWindow.propIndex2].IndexOf(MainWindow.propertyCard);
            //    MainWindow.playerTurns[MainWindow.playerNum].checkForcedDeal();
            //}

            if (MainWindow.stage == MainWindow.turnStage.rentWild)
            {
                MainWindow.cardNum = MainWindow.playerTurns[MainWindow.playerNum].Table_Properties.SelectedIndex;
                Card currentCard = MainWindow.playerTurns[MainWindow.playerNum].Table_Properties.Items.Cast<Card>().ElementAt(MainWindow.cardNum);
                MainWindow.propIndex = MainWindow.getPropertyIndex(currentCard);
                MainWindow.cardNum = MainWindow.AllTableProperties[MainWindow.playerNum][MainWindow.propIndex].IndexOf(currentCard);
                MainWindow.payment = MainWindow.getRentAmount();
                if ((MainWindow.AllHands[MainWindow.playerNum].Contains(Card.DoubleTheRent__1)) && (MainWindow.playNum < 2))
                {
                    MainWindow.playerTurns[MainWindow.playerNum].askDoubleRentWild();
                }
                else
                {
                    MainWindow.playerTurns[MainWindow.playerNum].playRentWild2();
                }
            }

            if (MainWindow.stage == MainWindow.turnStage.decidePropertyTypeWild)
            {
                MainWindow.cardNum2 = MainWindow.playerTurns[MainWindow.playerNum].Table_Properties.SelectedIndex;
                MainWindow.propertyCard = MainWindow.playerTurns[MainWindow.playerNum].Table_Properties.Items.Cast<Card>().ElementAt(MainWindow.cardNum2);
                MainWindow.propIndex = MainWindow.getPropertyIndex(MainWindow.propertyCard);
                MainWindow.playerTurns[MainWindow.playerNum].checkPlayCard();
            }

            if (MainWindow.stage == MainWindow.turnStage.moveCards)
            {
                MainWindow.cardNum = MainWindow.playerTurns[MainWindow.playerNum].Table_Properties.SelectedIndex;
                MainWindow.propertyCard = MainWindow.playerTurns[MainWindow.playerNum].Table_Properties.Items.Cast<Card>().ElementAt(MainWindow.cardNum);
                //Problem... propIndex only helps IFF the property is in the normal stack.  This won't work for extra [10] stack
                //So... I will first check the extra stack [10] when I remove a card...
                if (MainWindow.AllTableProperties[MainWindow.playerNum][10].Contains(MainWindow.propertyCard))
                {
                    MainWindow.propIndex = 10;
                }
                else
                {
                    MainWindow.propIndex = MainWindow.getPropertyIndex(MainWindow.propertyCard);
                }
                MainWindow.cardNum = MainWindow.AllTableProperties[MainWindow.playerNum][MainWindow.propIndex].IndexOf(MainWindow.propertyCard);
                MainWindow.propertyType = MainWindow.getPropertyType(MainWindow.propertyCard);
                if (MainWindow.propertyType == PropertyType.Duo)
                {
                    MainWindow.playerTurns[MainWindow.playerNum].moveCardsDecideType();
                }
                else if (MainWindow.propertyType == PropertyType.Wild)
                {
                    MainWindow.playerTurns[MainWindow.playerNum].moveCardsDecideTypeWild();
                }
                else
                {
                    MainWindow.playerTurns[MainWindow.playerNum].Prompt.Content = "This card cannot be moved.";
                    MainWindow.playerTurns[MainWindow.playerNum].button1.Visibility = System.Windows.Visibility.Hidden;
                    MainWindow.playerTurns[MainWindow.playerNum].button2.Visibility = System.Windows.Visibility.Hidden;
                    MainWindow.playerTurns[MainWindow.playerNum].button3.Visibility = System.Windows.Visibility.Hidden;
                    MainWindow.playerTurns[MainWindow.playerNum].buttonBack.Visibility = System.Windows.Visibility.Visible;
                }
                return;
            }
            if (MainWindow.stage == MainWindow.turnStage.moveCardsDecideTypeWild)
            {
                MainWindow.cardNum2 = MainWindow.playerTurns[MainWindow.playerNum].Table_Properties.SelectedIndex;
                Card currentCard = MainWindow.playerTurns[MainWindow.playerNum].Table_Properties.Items.Cast<Card>().ElementAt(MainWindow.cardNum2);
                MainWindow.propIndex2 = MainWindow.getPropertyIndex(currentCard);
                MainWindow.playerTurns[MainWindow.playerNum].checkMoveCard();
            }
        }

        public static void Hand_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MainWindow.playerTurns[MainWindow.playerNum].Hand.SelectedIndex < 0)
            {
                return;
            }
            if (MainWindow.stage == MainWindow.turnStage.playCardFromeHand)
            {
                MainWindow.cardNum = MainWindow.playerTurns[MainWindow.playerNum].Hand.SelectedIndex;
                MainWindow.playerTurns[MainWindow.playerNum].decideCardType();
            }

            if (MainWindow.stage == MainWindow.turnStage.discard)
            {
                MainWindow.cardNum = MainWindow.playerTurns[MainWindow.playerNum].Hand.SelectedIndex;
                MainWindow.playerTurns[MainWindow.playerNum].checkDiscard();
            }

            if (MainWindow.stage == MainWindow.turnStage.slyDeal)
            {
                if (MainWindow.chosenPlayer != MainWindow.playerNum)
                {
                    MainWindow.playerTurns[MainWindow.playerNum].checkSlyDeal();
                }
                MainWindow.cardNum = MainWindow.playerTurns[MainWindow.playerNum].Hand.SelectedIndex;
            }
        }

        public static void OtherPlayer_Click(object sender, RoutedEventArgs e)
        {
            Button copySender = (Button)sender;
            int playerClicked = MainWindow.otherNames[MainWindow.playerNum].IndexOf(copySender);

            if (MainWindow.stage == MainWindow.turnStage.debtCollect)
            {
                MainWindow.chosenPlayer = playerClicked;
                if (MainWindow.chosenPlayer != MainWindow.playerNum)
                {
                    MainWindow.playerTurns[MainWindow.playerNum].checkDebtCollector();
                }
            }
            if (MainWindow.stage == MainWindow.turnStage.rentWild2)
            {
                MainWindow.chosenPlayer = playerClicked;
                if (MainWindow.chosenPlayer != MainWindow.playerNum)
                {
                    MainWindow.playerTurns[MainWindow.playerNum].checkRentWild();
                }
            }
            if (MainWindow.stage == MainWindow.turnStage.dealBreaker)
            {
                MainWindow.chosenPlayer = playerClicked;
                if (MainWindow.chosenPlayer != MainWindow.playerNum)
                {
                    MainWindow.playerTurns[MainWindow.playerNum].playDealBreaker2();
                }
            }
        }

        public static void OtherPlayer_Properties_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox copySender = (ListBox)sender;
            int playerClicked = MainWindow.otherTable_Properties[MainWindow.playerNum].IndexOf(copySender);

            if (MainWindow.otherTable_Properties[MainWindow.playerNum][playerClicked].SelectedIndex < 0)
            {
                return;
            }
            if (MainWindow.stage == MainWindow.turnStage.slyDeal)
            {
                MainWindow.cardNum = MainWindow.otherTable_Properties[MainWindow.playerNum][playerClicked].SelectedIndex;
                MainWindow.propertyCard = MainWindow.otherTable_Properties[MainWindow.playerNum][playerClicked].Items.Cast<Card>().ElementAt(MainWindow.cardNum);
                //string chosenPlayer = buttonPlayer.Content.ToString();
                MainWindow.chosenPlayer = playerClicked;
                MainWindow.propIndex = MainWindow.getPropertyIndex(MainWindow.propertyCard);
                MainWindow.cardNum = MainWindow.AllTableProperties[MainWindow.chosenPlayer][MainWindow.propIndex].IndexOf(MainWindow.propertyCard);
                if (MainWindow.chosenPlayer != MainWindow.playerNum)
                {
                    MainWindow.playerTurns[MainWindow.playerNum].checkSlyDeal();
                }
            }

            //if (MainWindow.stage == MainWindow.turnStage.forcedDeal1)
            //{
            //    MainWindow.cardNum = Table_Properties.SelectedIndex;
            //    Card currentCard = Table_Properties.Items.Cast<Card>().ElementAt(MainWindow.cardNum);
            //    MainWindow.propIndex = MainWindow.getPropertyIndex(currentCard);
            //    MainWindow.cardNum = MainWindow.AllTableProperties[MainWindow.playerNum][MainWindow.propIndex].IndexOf(currentCard);
            //    MainWindow.playerTurns[MainWindow.playerNum].playForcedDeal2();
            //}
            else if (MainWindow.stage == MainWindow.turnStage.forcedDeal2)
            {
                MainWindow.cardNum2 = MainWindow.otherTable_Properties[MainWindow.playerNum][playerClicked].SelectedIndex;
                MainWindow.propertyCard = MainWindow.otherTable_Properties[MainWindow.playerNum][playerClicked].Items.Cast<Card>().ElementAt(MainWindow.cardNum2);
                MainWindow.propIndex2 = MainWindow.getPropertyIndex(MainWindow.propertyCard);
                //string chosenPlayer = buttonPlayer.Content.ToString();
                MainWindow.chosenPlayer = playerClicked;
                MainWindow.cardNum2 = MainWindow.AllTableProperties[MainWindow.chosenPlayer][MainWindow.propIndex2].IndexOf(MainWindow.propertyCard);
                MainWindow.playerTurns[MainWindow.playerNum].checkForcedDeal();
            }
        }

        public static void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainWindow.resizeLayout();
        }
#endregion
    }
}