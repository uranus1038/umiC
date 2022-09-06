using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace UmiServer
{
    class UmiServer
    {
        public static int maxPlayer { get; private set; }
        public static int port { get; private set; }
        public static Dictionary<int, UmiClientServer> clients = new Dictionary<int, UmiClientServer>();
        public delegate void PacketHandler(int _fClient, UmiPacket _Packet);
        public static Dictionary<int, PacketHandler> packetHandle;


        private static UdpClient udpListenner;
        private static TcpListener tcpListener;
       
        public static void Start(int _maxPlayer, int _port)
        {
            maxPlayer = _maxPlayer;
            port = _port;
            Console.WriteLine("Start Server");
            initializeServerData();
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(tcp_Connect_Callback), null);


            udpListenner = new UdpClient(port);
            udpListenner.BeginReceive(umiUdpReceiveCallback, null);

            Console.WriteLine($"Server Start :) on {port}.");
        }

        private static void tcp_Connect_Callback(IAsyncResult _Result)
        {
            TcpClient _Client = tcpListener.EndAcceptTcpClient(_Result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(tcp_Connect_Callback), null);
            Console.WriteLine($"Incoming connection from {_Client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= maxPlayer; i++)
            {

                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_Client);
                    return;
                }
            }
            Console.WriteLine($"{_Client.Client.RemoteEndPoint}... Server is Full");
        }

        private static void umiUdpReceiveCallback(IAsyncResult _Result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListenner.EndReceive(_Result, ref _clientEndPoint);
                udpListenner.BeginReceive(umiUdpReceiveCallback, null);
                if (_data.Length < 4)
                {
                    Console.WriteLine("Error Connect");
                    return;
                }
                using (UmiPacket _packet = new UmiPacket(_data))
                {
                    int _cId = _packet.ReadInt();
                    if (_cId == 0)
                    {
                        Console.WriteLine("Error Connect 0");
                        return;
                    }
                    if (clients[_cId].udp.endPoint == null)
                    {
                        clients[_cId].udp.Connect(_clientEndPoint);
                        return;
                    }
                    if (clients[_cId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        clients[_cId].udp.umiHandleData(_packet);
                    }
                }

            }
            catch (Exception _Ex)
            {
                Console.WriteLine($"Error Udp sending : {_Ex}");


            }


        }
        public static void umiSendUdpData(IPEndPoint _cEndPoint, UmiPacket _packet)
        {
            try
            {
                if (_cEndPoint != null)
                {
                    udpListenner.BeginSend(_packet.ToArray(), _packet.Length(), _cEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending {_cEndPoint} Udp : {_ex}");
            }
        }
        private static void initializeServerData()

        {
            for (int i = 1; i <= maxPlayer; i++)
            {
                clients.Add(i, new UmiClientServer(i));
            }
            packetHandle = new Dictionary<int, PacketHandler>()
            {
                {   (int)ClientPackets.welcomeReceived , UmiServerHandle.welcomReceived },
                {   (int)ClientPackets.playerMovement , UmiServerHandle.playerMovement},
                {   (int)ClientPackets.disConnectClient , UmiServerHandle.disconnectReceive}

            };
        }


    }
}
