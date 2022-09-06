using System;
using System.Numerics;
namespace UmiServer
{
    public class UmiPlayer
    {
        public UmiPlayer instance;
        public int id;
        public string user_Name;
        public Vector3 position;
        public Quaternion rotation;

        public UmiPlayer(int _id, string _username, Vector3 _spawnPosition)
        {
            id = _id;
            user_Name = _username;
            position = _spawnPosition;
            rotation = Quaternion.Identity;

        }
        public void umiUpdate()
        {
            UmiServerSend.playerPosition(this);
        }


        public void setInput(Vector3 _position)
        {
            position = _position;
        }

    }
}
