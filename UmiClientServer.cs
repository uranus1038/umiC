using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
namespace UmiServer
{
    class UmiClientServer
    {
        public int id;
        public UmiTcp tcp;
        public UmiUdp udp;
        public static int dataBufferSize = 4096;

        public UmiPlayer player;
        public UmiClientServer(int _clientId)
        {
            id = _clientId;
            tcp = new UmiTcp(id);
            udp = new UmiUdp(id);
        }
        public class UmiTcp
        {
            public TcpClient socket;
            public UmiPacket receiveData;
            private readonly int id;
            private byte[] receiveBuffer;
            private NetworkStream stream;


            public UmiTcp(int _id)
            {
                id = _id;
            }
            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();
                receiveBuffer = new byte[dataBufferSize];
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, umiReceiveCallback, null);
                receiveData = new UmiPacket();
                UmiServerSend.Welcome(id, "Hi you");
            }
            public void umiSendData(UmiPacket _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception _Ex)
                {
                    Console.WriteLine($"Error sendig {id} :{_Ex}");
                }
            }

            private void umiReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        //Console.WriteLine(_byte_Length);
                        UmiServer.clients[id].Disconnect();
                        return;
                    }
                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receiveData.Reset(umiHandleData(_data));
                    stream.BeginRead(receiveBuffer,
                        0, dataBufferSize, umiReceiveCallback, null);
                }
                catch (Exception _EX)
                {
                    Console.WriteLine("Error Receiv TCP Data " + _EX);
                    UmiServer.clients[id].Disconnect();
                }
            }
            private bool umiHandleData(byte[] _data)
            {
                int _packetLenght = 0;

                receiveData.SetBytes(_data);
                if (receiveData.UnreadLength() >= 4)
                {
                    _packetLenght = receiveData.ReadInt();
                    if (_packetLenght <= 0)
                    {
                        return true;
                    }
                }
                while (_packetLenght > 0 && _packetLenght <= receiveData.UnreadLength())
                {
                    byte[] _packetByte = receiveData.ReadBytes(_packetLenght);
                    UmiThreadManager.umiExecuteOnMainThread(() =>
                    {
                        using (UmiPacket _packet = new UmiPacket(_packetByte))
                        {
                            int _packetId = _packet.ReadInt();
                            UmiServer.packetHandle[_packetId](id, _packet);
                        }

                    });
                    _packetLenght = 0;
                    if (receiveData.UnreadLength() >= 4)
                    {
                        _packetLenght = receiveData.ReadInt();
                        if (_packetLenght <= 0)
                        {
                            return true;
                        }
                    }
                }
                if (_packetLenght <= 1)
                {
                    return true;
                }
                return false;
            }

            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receiveBuffer = null;
                receiveData = null;
                socket = null;
            }

        }

        public class UmiUdp
        {
            public IPEndPoint endPoint;

            private int id;
            public UmiUdp(int _Id)
            {
                id = _Id;
            }
            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;
            }
            public void umiSendData(UmiPacket _packet)
            {
                UmiServer.umiSendUdpData(endPoint, _packet);
            }
            public void umiHandleData(UmiPacket _packetData)
            {
                int _packetLength = _packetData.ReadInt();
                byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

                UmiThreadManager.umiExecuteOnMainThread(() =>
                {
                    using (UmiPacket _Packet = new UmiPacket(_packetBytes))
                    {
                        int _packet_Id = _Packet.ReadInt();
                        UmiServer.packetHandle[_packet_Id](id, _Packet);
                    }
                });

            }
            public void Disconnect()
            {
                endPoint = null;
            }

        }
        public void Disconnect()
        {
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has Disconnect");
            player = null;
            tcp.Disconnect();
            udp.Disconnect();
        }

        public void sendIntoGame(string _playerName)
        {
            player = new UmiPlayer(id, _playerName, new Vector3(0, 0, 0));
            foreach (UmiClientServer _client in UmiServer.clients.Values)
            {
                if (_client.player != null)
                {
                    if (_client.id != id)
                    {
                        UmiServerSend.spawnPlayer(id, _client.player);
                    }

                }
            }
            foreach (UmiClientServer _client in UmiServer.clients.Values)
            {
                if (_client.player != null)
                {
                    UmiServerSend.spawnPlayer(_client.id, player);

                }
            }
        }
    }
}