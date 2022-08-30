public class ServerConnectionData
{
    public string ipv4Address;
    public ushort port;

    public ServerConnectionData(string ipv4Address, ushort port)
    {
        this.ipv4Address = ipv4Address;
        this.port = port;
    }
}