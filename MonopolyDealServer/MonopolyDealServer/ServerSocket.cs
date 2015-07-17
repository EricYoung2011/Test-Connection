using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SocketDLL
{
    public class ServerSocket
    {
        // The following is an example base data type to hold the 
        // actual data for other data or control or syncronization signals
        // you can create a class that implements the StreamData interface
        private Socket server = null;
        private Socket client = null;
        private StreamData data = null;
        private AddressFamily addressFamily;
        private SocketType socketType;
        private ProtocolType protocolType;
        private IPHostEntry ipEntry = null;
        private IPAddress address = null;
        private IPEndPoint endpoint = null;
        private int port;
        


        // TO DO: add attributes that hold address information and so on see an example at
        // http://msdn.microsoft.com/en-us/library/system.net.sockets.socket.aspx

        /// <summary>
        /// Calls all the initial methods
        /// like create the sockets, init 
        /// the sockets and bind the server socket
        /// </summary>

        public ServerSocket(int _port, int _ipAddressOffset, string _ipAddress = "")
        {            
            port = _port;
            // DEBUG: might need to do something here with client and StreamData
            init(_ipAddressOffset, _ipAddress);
            bindServerSocket();
        }
        

        /// <summary>
        /// Initializes the socket
        /// </summary>`
        public void init(int offset, string ipAddress)
        {
            socketType = SocketType.Stream;
            protocolType = ProtocolType.Tcp;
            //addressFamily = AddressFamily.InterNetwork;
            addressFamily = AddressFamily.InterNetwork;
            try
            {
                ipEntry = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress[] addr = ipEntry.AddressList;
                //endpoint = new IPEndPoint(addr[0], port);
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
                Console.WriteLine("Using the Address {0}: {1}", address.ToString(), port);
                endpoint = new IPEndPoint(address, port);
            }
            catch (SocketException ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            createSocket();

        }


        /// <summary>
        /// creates and initializes the server socket
        /// and initializes the client socket 
        /// </summary>
        public void createSocket()
        {
            try
            {
                server = new Socket(addressFamily, socketType, protocolType);
            }
            catch (SocketException ex)
            {
                System.Console.WriteLine(ex.Message);
                server.Close();
            }
        }

        /// <summary>
        /// binds the server socket after creation
        /// check to see if the server socket is 
        /// created successfully or you will get error
        /// </summary>
        public void bindServerSocket()
        {
            try
            {
                server.Bind(endpoint);
            }
            catch (SocketException ex)
            {
                System.Console.WriteLine(ex.Message);
                server.Close();
            }
        }

        /// <summary>
        /// starts the server
        /// </summary>
        public void start()
        {
            try
            {
                server.Listen(1); //DEBUG: not sure what this "backlog" number should be
                acceptConnection(client);
            }
            catch (SocketException ex)
            {
                System.Console.WriteLine(ex.Message);
                server.Close();
            }

        }

        /// <summary>
        /// stops the server and cleans up
        /// if anything needs to be cleaned up
        /// </summary>
        // DEBUG: Will need a numClients variable if using multiple clients
        public void stop()
        {
            System.Console.WriteLine("Shutting down client");
            try
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                server.Close();
            }
            catch (SocketException ex)
            {
                System.Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// creates the client socket by accepting a connection
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <returns>returns error code if any</returns>
        public int acceptConnection(Socket clientSocket)
        {
            System.Console.WriteLine("Tring to accept a connection...");
            try
            {
                clientSocket = server.Accept();
                client = clientSocket;
                if (client.Connected)
                {
                    System.Console.WriteLine("Successfully Connected");
                }
                return 1;
            }
            catch (SocketException ex)
            {
                System.Console.WriteLine(ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// receives data from the socket stream
        /// might use decode method of the decoder
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="receivedData"></param>
        /// <returns>returns error code, if any</returns>
        public byte[] receiveData(Socket clientSocket, StreamData receivedData)
        {
            byte[] buffer = new byte[2000];
            byte[] example = new byte[1];
            int iResult = 0;
            try
            {
                iResult = clientSocket.Receive(buffer);
                int stillSorting = 0;
                while (stillSorting>=0)
                {
                    if ((buffer[stillSorting] == 255)&&(buffer[stillSorting+1] == 255))
                    {
                        byte[] newbuffer = new byte[stillSorting];
                        for (int i = 0; i < stillSorting; i++)
                        {
                            newbuffer[i] = buffer[i];
                        }
                        buffer = newbuffer;
                        stillSorting = -1;
                    }
                    else //found end of valid characters
                    {
                        stillSorting++;
                    }
                }
                data = receivedData.decode(buffer);
                System.Console.WriteLine("Received the following data:\n");
                receivedData.printData();
                return buffer; // data received and is ready
            }
            catch (SocketException ex)
            {
                // DEBUG: not sure if error handling is absolutely correct, check here later
                if (iResult == 0 && ex.SocketErrorCode != SocketError.WouldBlock)
                {
                    System.Console.WriteLine("Client closes connection.\n");
                    System.Console.WriteLine("Please, start again to reconnect!");
                    clientSocket.Close();
                    return new byte[1]{0}; // client closed connection
                }
                else if (ex.SocketErrorCode == SocketError.WouldBlock ||
                    ex.SocketErrorCode == SocketError.IOPending ||
                    ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                {
                    return new byte[1] { 1 }; //data not ready yet   
                }
                else
                {
                    return new byte[1] { 2 };  // any serious error occurr
                }
            }

        }

        /// <summary>
        /// polls the socket, if there is data, it receives
        /// if not it will continue without blocking and returns
        /// a number that indicates whether it receives 
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="receivedData"></param>
        /// <param name="pollingFrequency"></param>
        /// <returns>returns code to indicate if it receives and/or error code</returns>
        public byte[] pollAndReceiveData(Socket clientSocket, StreamData receivedData, int pollingFrequency)
        {
            bool available = clientSocket.Poll(pollingFrequency, SelectMode.SelectRead);
            if (available)
            {
                try
                {
                    return receiveData(clientSocket, receivedData);
                }
                catch (SocketException ex)
                {
                    System.Console.WriteLine(ex.Message);
                    return new byte[1]{0};
                }
            }
            else
            {
                System.Console.WriteLine("No readable data yet!\n");
                return new byte[1] { 1 };
            }
        }

        /// <summary>
        /// sends the data to the socket stream
        /// might use the encode method of the encoder
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="dataToBeSent"></param>
        /// <returns>returns error code, if any</returns>
   //     
        public int sendData(Socket clientSocket, byte[] dataToBeSent)
        {
            int iResult;
            try
            {
                byte[] encodedData = new byte[dataToBeSent.Length + 2];
                for (int i = 0; i < dataToBeSent.Length; i++)
                {
                    encodedData[i] = dataToBeSent[i];
                }
                encodedData[dataToBeSent.Length] = 255;
                encodedData[dataToBeSent.Length+1] = 255;
                iResult = clientSocket.Send(encodedData);
            }
            catch (SocketException ex)
            {
                System.Console.WriteLine("Client is not running. Please, make sure it is running.");
                System.Console.WriteLine(ex.Message);
                clientSocket.Close();
                return 0;
            }
            return 1;
        }

        public Socket Server
        {
            get
            {
                return server;
            }
        }

        public bool Connected
        {
            get
            {
                return client.Connected;
            }
        }

        public StreamData DataStream
        {
            get
            {
                return data;
            }
        }

        public Socket Client
        {
            get
            {
                return client;
            }
        }
    }
}
