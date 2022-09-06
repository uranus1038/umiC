public enum ServerPackets
{
    welcome = 1,
    spawnPlayer,
    playerPosition
    , playerRotation,
    disConnectSv
}

/// <summary>Sent from client to server.</summary>
public enum ClientPackets
{
    welcomeReceived = 1,
    playerMovement,
    disConnectClient

}