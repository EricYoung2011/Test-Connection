using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MonopolyDealServer
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
        public List<Card> _tablePropertiesSelectedItems;
        [DataMember(Name = "Table Money Selected Items", EmitDefaultValue = false)]
        public List<Card> _tableMoneySelectedItems;
        [DataMember(Name = "Player Clicked", EmitDefaultValue = false)]
        public int _playerClicked;
        //[DataMember(Name = "PlayerHand", EmitDefaultValue = false)]
        //public System.Windows.Controls.ListBox _playerHand;

        public clientEvent()
        {

        }

    }
}
