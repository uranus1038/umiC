using System;
using System.Numerics;
namespace UmiServer
{
    class UmiServerHandle
    {
        public static void welcomReceived(int _fClient, UmiPacket _Packet)
        {
            int _id_User = _Packet.ReadInt();
            string txt = _Packet.ReadString();

            Console.WriteLine($"{UmiServer.clients[_fClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_id_User}");
            if (_fClient != _id_User)
            {
                Console.WriteLine($"Player {_id_User} id : {_fClient}");
            }
           // UmiServer.clients[_id_User].sendIntoGame(txt);
        }
        public static void playerMovement(int _fClient, UmiPacket packet)
        {
            Vector3 _position = packet.ReadVector3();


            UmiServer.clients[_fClient].player.setInput(_position);

        }
        public static void disconnectReceive(int _fClient, UmiPacket packet)
        {

            int _id = packet.ReadInt();
            Console.WriteLine(_id);
            UmiServerSend.disconnectSend(_id);


        }



    }
}