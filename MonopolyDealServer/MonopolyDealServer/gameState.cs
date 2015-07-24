using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MonopolyDealServer
{
    [DataContract]
    public class gameState
    {
        [DataMember(Name = "ServerPort",EmitDefaultValue = false)]
        public int serverPort;
        [DataMember(Name = "Hand", EmitDefaultValue = false)]
        public List<Card> hand;
        public gameState()
        {
            serverPort = MainWindow.serverPort;
            hand = MainWindow.AllHands[0];
        }
    }
}
