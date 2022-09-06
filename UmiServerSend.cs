using System;
using System.Numerics;
namespace UmiServer
{
    public class UmiServerSend
    {
        private static void umiSendTcpData(int _toClient, UmiPacket _packet)
        {
            _packet.WriteLength();
            UmiServer.clients[_toClient].tcp.umiSendData(_packet);
        }
        private static void umiSendUdpData(int _toClient, UmiPacket _packet)
        {
            _packet.WriteLength();
            UmiServer.clients[_toClient].udp.umiSendData(_packet);
        }
        private static void umiSendTcpDataToAll(UmiPacket _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= UmiServer.maxPlayer; i++)
            {
                UmiServer.clients[i].tcp.umiSendData(_packet);
            }
        }
        private static void umiSendTcpDataToAll(int _exceptClient, UmiPacket _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= UmiServer.maxPlayer; i++)
            {
                if (i != _exceptClient)
                {
                    UmiServer.clients[i].tcp.umiSendData(_packet);
                }
            }
        }

        private static void umiSendUdpDataToAll(UmiPacket _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= UmiServer.maxPlayer; i++)
            {
                UmiServer.clients[i].udp.umiSendData(_packet);
            }
        }
        private static void umiSendUdpDataToAll(int _exceptClient, UmiPacket _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= UmiServer.maxPlayer; i++)
            {
                if (i != _exceptClient)
                {
                    UmiServer.clients[i].udp.umiSendData(_packet);
                }
            }
        }
        public static void Welcome(int _toClient, string _msg)
        {
            using (UmiPacket _packet = new UmiPacket((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                umiSendTcpData(_toClient, _packet);

            }
        }
        public static void spawnPlayer(int _toClient, UmiPlayer _player)
        {
            using (UmiPacket _packet = new UmiPacket((int)ServerPackets.spawnPlayer))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.user_Name);
                _packet.Write(_player.position);
                _packet.Write(_player.rotation);

                umiSendTcpData(_toClient, _packet);

            }
        }
        public static void playerPosition(UmiPlayer _player)
        {
            using (UmiPacket _packet = new UmiPacket((int)ServerPackets.playerPosition))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.position);



                umiSendUdpDataToAll(_player.id, _packet);
            }
        }

        public static void disconnectSend(int _fClient)
        {
            using (UmiPacket _packet = new UmiPacket((int)ServerPackets.disConnectSv))
            {

                _packet.Write(_fClient);

                umiSendUdpDataToAll(_fClient, _packet);
            }
        }





    }
}