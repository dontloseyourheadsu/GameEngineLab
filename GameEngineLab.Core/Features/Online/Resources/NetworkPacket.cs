using System;

namespace GameEngineLab.Core.Features.Online.Resources;

public enum PacketType : byte
{
    Handshake = 0,
    InputUpdate = 1,
    AssetTransfer = 2,
    Chat = 3,
    Heartbeat = 4
}

public sealed class NetworkPacket
{
    public PacketType Type { get; set; }
    
    public Guid SenderId { get; set; }
    
    public byte[] Payload { get; set; } = [];

    public static byte[] Serialize(NetworkPacket packet)
    {
        // Simple manual serialization for the lab
        var typeByte = (byte)packet.Type;
        var guidBytes = packet.SenderId.ToByteArray();
        var data = new byte[1 + 16 + packet.Payload.Length];
        
        data[0] = typeByte;
        Array.Copy(guidBytes, 0, data, 1, 16);
        Array.Copy(packet.Payload, 0, data, 17, packet.Payload.Length);
        
        return data;
    }

    public static NetworkPacket Deserialize(byte[] data)
    {
        if (data.Length < 17) throw new ArgumentException("Invalid packet data");
        
        var packet = new NetworkPacket
        {
            Type = (PacketType)data[0],
            SenderId = new Guid(data[1..17]),
            Payload = data[17..]
        };
        
        return packet;
    }
}
