using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;


namespace MonopolyDealClient
{
    [DataContract]
    public class clientEvent
    {
        [DataMember(Name = "Button1Clicked", EmitDefaultValue = false)]
        public int _button1Clicked;
        [DataMember(Name = "Button2Clicked", EmitDefaultValue = false)]
        public int _button2Clicked;
        [DataMember(Name = "Button3Clicked", EmitDefaultValue = false)]
        public int _button3Clicked;
        [DataMember(Name = "ButtonBackClicked", EmitDefaultValue = false)]
        public int _buttonBackClicked;
        [DataMember(Name = "HandDoubleClicked", EmitDefaultValue = false)]
        public int _handDoubleClicked;
        [DataMember(Name = "PropertiesDoubleClicked", EmitDefaultValue = false)]
        public int _propertiesDoubleClicked;
        [DataMember(Name = "PropertiesSelectionChanged", EmitDefaultValue = false)]
        public int _propertiesSelectionChanged;
        [DataMember(Name = "MoneySelectionChanged", EmitDefaultValue = false)]
        public int _moneySelectionChanged;
        [DataMember(Name = "OtherPlayerClicked", EmitDefaultValue = false)]
        public int _otherPlayerClicked;
        [DataMember(Name = "OtherPropertiesDoubleClicked", EmitDefaultValue = false)]
        public int _otherPropertiesDoubleClicked;
        [DataMember(Name = "Hand Selected Index", EmitDefaultValue = false)]
        public int _handSelectedIndex;
        [DataMember(Name = "Table Properties Selected Index", EmitDefaultValue = false)]
        public int _tablePropertiesSelectedIndex;
        [DataMember(Name = "Other Properties Selected Index", EmitDefaultValue = false)]
        public int _otherPropertiesSelectedIndex;
        [DataMember(Name = "Table Properties Selected Items", EmitDefaultValue = false)]
        public List<Card> _tablePropertiesSelectedItems= new List<Card>();
        [DataMember(Name = "Table Money Selected Items", EmitDefaultValue = false)]
        public List<Card> _tableMoneySelectedItems = new List<Card>();
        [DataMember(Name = "Player Clicked", EmitDefaultValue = false)]
        public int _playerClicked;
        //[DataMember(Name = "PlayerHand", EmitDefaultValue = false)]
        //public System.Windows.Controls.ListBox _playerHand;

        public clientEvent()
        {
            _button1Clicked = MainWindow.button1Clicked;
            _button2Clicked = MainWindow.button2Clicked;
            _button3Clicked = MainWindow.button3Clicked;
            _buttonBackClicked = MainWindow.buttonBackClicked;
            _handDoubleClicked = MainWindow.handDoubleClicked;
            _moneySelectionChanged = MainWindow.moneySelectionChanged;
            _otherPlayerClicked = MainWindow.otherPlayerClicked;
            _otherPropertiesDoubleClicked = MainWindow.otherPropertiesDoubleClicked;
            _propertiesDoubleClicked = MainWindow.propertiesDoubleClicked;
            _propertiesSelectionChanged = MainWindow.propertiesSelectionChanged;
            //_playerHand = MainWindow.myDisplay.Hand;
            _handSelectedIndex = MainWindow.myDisplay.Hand.SelectedIndex;
            _tablePropertiesSelectedIndex = MainWindow.myDisplay.Table_Properties.SelectedIndex;
            foreach (Card card in MainWindow.myDisplay.Table_Properties.SelectedItems)
            {
                _tablePropertiesSelectedItems.Add(card);
            } 
            foreach (Card card in MainWindow.myDisplay.Table_Money.SelectedItems)
            {
                _tableMoneySelectedItems.Add(card);
            }
            _playerClicked = MainWindow.playerClicked;
            _otherPropertiesSelectedIndex = MainWindow.otherPropertiesSelectedIndex;
            




        }

    }
}
