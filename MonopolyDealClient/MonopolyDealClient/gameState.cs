using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MonopolyDealClient
{
    [DataContract]
    public class gameState
    {
        [DataMember(Name = "ServerPort", EmitDefaultValue = false)]
        public int _serverPort;
        [DataMember(Name = "MyPlayerNum", EmitDefaultValue = false)]
        public int _myPlayerNum;
        [DataMember(Name = "AllHands", EmitDefaultValue = false)]
        public List<List<Card>> _AllHands;
        [DataMember(Name = "AllTableProperties", EmitDefaultValue = false)]
        public List<List<List<Card>>> _AllTableProperties;
        [DataMember(Name = "AllTableMoney", EmitDefaultValue = false)]
        public List<List<Card>> _AllTableMoney;
        [DataMember(Name = "PlayerNames", EmitDefaultValue = false)]
        public List<string> _playerNames;
        [DataMember(Name = "NumOfPlayers", EmitDefaultValue = false)]
        public int _numOfPlayers;
        [DataMember(Name = "NumOfPlayersConnected", EmitDefaultValue = false)]
        public int _numOfPlayersConnected;
        [DataMember(Name = "PlayerNum", EmitDefaultValue = false)]
        public int _playerNum;
        [DataMember(Name = "PlayNum", EmitDefaultValue = false)]
        public int _playNum;
        [DataMember(Name = "NumCardsInDeck", EmitDefaultValue = false)]
        public int _numCardsInDeck;
        [DataMember(Name = "NewUniversalPrompt", EmitDefaultValue = false)]
        public string _newUniversalPrompt;
        [DataMember(Name = "IndividualPrompt", EmitDefaultValue = false)]
        public string _individualPrompt;
        [DataMember(Name = "Button1Text", EmitDefaultValue = false)]
        public string _button1Text;
        [DataMember(Name = "Button1Visibility", EmitDefaultValue = false)]
        public System.Windows.Visibility _button1Visibility;
        [DataMember(Name = "Button2Text", EmitDefaultValue = false)]
        public string _button2Text;
        [DataMember(Name = "Button2Visibility", EmitDefaultValue = false)]
        public System.Windows.Visibility _button2Visibility;
        [DataMember(Name = "Button3Text", EmitDefaultValue = false)]
        public string _button3Text;
        [DataMember(Name = "Button3Visibility", EmitDefaultValue = false)]
        public System.Windows.Visibility _button3Visibility;
        [DataMember(Name = "ButtonBackText", EmitDefaultValue = false)]
        public string _buttonBackText;
        [DataMember(Name = "ButtonBackVisibility", EmitDefaultValue = false)]
        public System.Windows.Visibility _buttonBackVisibility;
        [DataMember(Name = "BeginGame", EmitDefaultValue = false)]
        public bool _beginGame;
        [DataMember(Name = "UpdateCards", EmitDefaultValue = false)]
        public bool _updateCards;

        public gameState(int _recipient, int stage = 1)
        {

        }
    }
}


