using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;


namespace SocketDLL
{
    public class ClientSocket
    {
        private Socket server = null;
        private Socket client = null;
        private StreamData data = null;
        private AddressFamily addressFamily;
        private SocketType socketType;
        private ProtocolType protocolType;
        private IPHostEntry ipEntry = null;
        private static string address_string = "";
        private IPAddress address = null;
        private IPEndPoint endpoint = null;
        //private IPAddress ipAddress = null;
        private int port;

        /// <summary>
        /// Calls all the initial methods
        /// like create the sockets and init the 
        /// sockets        
        public ClientSocket(int _port, int _ipAddressOffset, string _ipAddress="")
        {
            port = _port;
            init(_ipAddressOffset, _ipAddress);
        }

        /// <summary>
        /// Initializes the socket
        /// </summary>
        public void init(int offset, string ipAddress)
        {
            addressFamily = AddressFamily.InterNetwork;
            protocolType = ProtocolType.Tcp;
            addressFamily = AddressFamily.InterNetwork;
            socketType = SocketType.Stream;
            try
            {
                // TODO: This is so it will work for now in Unity. Go back and use
                // getAddress function to have the user input the IP address of the 
                // machine
                //address_string = "192.168.1.101";
                //ipAddress = IPAddress.Parse(address_string);
                ipEntry = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress[] addr = ipEntry.AddressList;
                for (int i = 0; i < addr.Length; i++)
                {
                    Console.WriteLine("IP Address {0}: {1} ", i, addr[i].ToString());
                    
                }
                if (ipAddress == "")
                {
                    address = addr[addr.Length - offset];
                }
                else
                {
                    address = IPAddress.Parse(ipAddress);
                }
                Console.WriteLine("Using the Address {0}: {1}", address.ToString(),port);                
                endpoint = new IPEndPoint(address, port);
            }
            catch (SocketException ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            createSocket();
            connectToServer();

            string sentData = "Testing connection to server: " + port;
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            try
            {
                Byte[] encodedString = encoding.GetBytes(sentData);
            }
            catch (SocketException ex)
            {
                System.Console.WriteLine(ex.Message);
                client.Close();
            }
        }

        /// <summary>
        /// creates and initializes the client socket
        /// and initializes the server socket 
        /// </summary>
        public void createSocket()
        {
            try
            {
                client = new Socket(addressFamily, socketType, protocolType);
            }
            catch (SocketException ex)
            {
                System.Console.WriteLine(ex.Message);
                client.Close();
            }
        }

        /// <summary>
        /// creates the server socket by reqeusting a connection
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <returns>returns error code if any</returns>
        public int connectToServer()
        {
            try
            {   
                client.Connect(endpoint);
                if (client.Connected)
                {
                    System.Console.WriteLine("Successfully connected");
                }
            }
            catch (SocketException ex)
            {
                System.Console.WriteLine(ex.Message);
                client.Close();
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// receives data from the socket stream
        /// </summary>
        /// <param name="serverSocket"></param>
        /// <param name="receivedData"></param>
        public int receiveData(Socket serverSocket, StreamData receivedData)
        {
            byte[] buffer = new byte[2000];
            int iResult = 0;
            try
            {
                iResult = serverSocket.Receive(buffer);
                data = receivedData.decode(buffer);
                System.Console.WriteLine("Received the following data:\n");
                receivedData.printData();
                return 1; // data received and is ready
            }
            catch (SocketException ex)
            {
                if (iResult == 0 && ex.SocketErrorCode != SocketError.WouldBlock)
                {
                    System.Console.WriteLine("Client closes connection.\n");
                    System.Console.WriteLine("Please, start again to reconnect!");
                    serverSocket.Close();
                    return -1; // client closed connection
                }
                else if (ex.SocketErrorCode == SocketError.WouldBlock ||
                    ex.SocketErrorCode == SocketError.IOPending ||
                    ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                {
                    return 0; //data not ready yet   
                }
                else
                {
                    return -1;  // any serious error occurr
                }
            }
        }

        /// <summary>
        /// polls socket to see if data is ready to be received
        /// if so, receives data from socket stream
        /// </summary>
        /// <param name="serverSocket"></param>
        /// <param name="receivedData"></param>
        /// <param name="pollingFrequency"></param>
        /// <returns>returns code to indicate if it receives and/or error code</returns>
        public int pollAndReceiveData(Socket serverSocket, StreamData receivedData, int pollingFrequency)
        {
            bool available=false; int iResult = 0;
            try
            {
                available = serverSocket.Poll(pollingFrequency, SelectMode.SelectRead);
            }
            catch (SocketException ex)
            {
                if (iResult == 0 && ex.SocketErrorCode != SocketError.WouldBlock)
                {
                    System.Console.WriteLine("Client closes connection.\n");
                    System.Console.WriteLine("Please, start again to reconnect!");
                    serverSocket.Close();
                    return -1; // client closed connection
                }
                else if (ex.SocketErrorCode == SocketError.WouldBlock ||
                    ex.SocketErrorCode == SocketError.IOPending ||
                    ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                {
                    return 0; //data not ready yet   
                }
                else
                {
                    return -1;  // any serious error occurr
                }
            }
            catch (ObjectDisposedException ex)
            {
                System.Console.WriteLine("Server closes connection.\n");
            }
            if (available)
            {
                try
                {
                    return receiveData(serverSocket, receivedData);
                }
                catch (SocketException ex)
                {
                    System.Console.WriteLine(ex.Message);
                    return -1;
                }
            }
            else
            {
                //System.Console.WriteLine("No readable data yet!\n");
                return 0;
            }
        }

        /// <summary>
        /// sends the data to the socket stream
        /// 
        /// </summary>
        /// <param name="dataToBeSent"></param>
        /// <returns>returns error code, if any</returns>
        public int sendData(StreamData dataToBeSent)
        {
            int iResult;
            try
            {
                data = dataToBeSent;
                byte[] encodedData = data.encode();
                iResult = client.Send(encodedData);
            }
            catch (SocketException ex)
            {
                System.Console.WriteLine("Client is not running. Please, make sure it is running.");
                System.Console.WriteLine(ex.Message);
                server.Close();
                return 0;
            }
            System.Console.WriteLine("Sent message: ");
            data.printData();
            System.Console.WriteLine(iResult + " bytes long.\n");
            return 1;
        }

        public static int getAddress()
        {
            try
            {
                System.Console.WriteLine("Please enter the server's IP address: ");
                address_string = Console.ReadLine();
                return 0;
            }
            catch (System.FormatException)
            {
                System.Console.WriteLine("Error: Invalid formatting");
                return 1;
            }
        }

        public bool Connected
        {
            get
            {
                return client.Connected;
            }
        }

        public Socket Client
        {
            get
            {
                return client;
            }
        }

        public Socket Server
        {
            get
            {
                return server;
            }
        }

        public StreamData DataStream
        {
            get
            {
                return data;
            }
        }

    }
}
