using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Online.Resources;
using System.Linq;

namespace GameEngineLab.Core.Features.Online.Systems;

public sealed class NetworkSystem : IGameSystem
{
    public int Order => -100; // Run very early

    private INetworkTransport? _transport;

    public void Update(World world, FrameContext frameContext)
    {
        if (!world.TryGetResource<NetworkResource>(out var net) || net == null) return;

        // Initialize transport if needed
        if (net.IsConnected && _transport == null)
        {
            _transport = new SimpleUdpTransport();
            if (net.IsHost) _transport.StartHost(net.Port);
            else _transport.Connect(net.RemoteAddress, net.Port);
        }
        else if (!net.IsConnected && _transport != null)
        {
            _transport.Dispose();
            _transport = null;
        }

        if (_transport != null)
        {
            var incoming = _transport.Receive().ToList();
            foreach (var packet in incoming)
            {
                ProcessPacket(world, packet);
            }
        }
    }

    public void Draw(World world, FrameContext frameContext) { }

    private void ProcessPacket(World world, NetworkPacket packet)
    {
        switch (packet.Type)
        {
            case PacketType.AssetTransfer:
                // This would be handled by a specific social/asset system listening for this data
                break;
            case PacketType.InputUpdate:
                // Update remote player input components
                break;
        }
    }
}
