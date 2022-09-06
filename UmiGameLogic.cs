
namespace UmiServer
{
    class UmiGameLogic
    {
        public static void umiUpdate()
        {
            foreach (UmiClientServer _client in UmiServer.clients.Values)
            {
                if (_client.player != null)
                {
                    _client.player.umiUpdate();

                }

            }
            UmiThreadManager.umiUpdateMain();
        }
    }
}