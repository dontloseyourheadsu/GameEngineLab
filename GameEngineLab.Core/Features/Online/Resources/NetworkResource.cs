using System;
using System.Collections.Concurrent;

namespace GameEngineLab.Core.Features.Online.Resources;

public enum NetworkState
{
    Disconnected,
    Connecting,
    ConnectedAsHost,
    ConnectedAsGuest,
    Error,
}

public sealed class NetworkResource
{
    public NetworkState State { get; set; } = NetworkState.Disconnected;
    
    public string RemoteAddress { get; set; } = string.Empty;
    
    public int Port { get; set; } = 7777;

    public float LatencyMs { get; set; }
    
    public bool IsConnected => State is NetworkState.ConnectedAsHost or NetworkState.ConnectedAsGuest;
    
    public bool IsHost => State == NetworkState.ConnectedAsHost;

    // Concurrent queues for thread-safe networking communication
    public ConcurrentQueue<NetworkPacket> IncomingPackets { get; } = new();
    
    public ConcurrentQueue<NetworkPacket> OutgoingPackets { get; } = new();

    // Remote peer identity
    public Guid RemotePlayerId { get; set; } = Guid.Empty;
    
    public string RemotePlayerName { get; set; } = "Guest Player";

    public int RemotePlayerStrokes { get; set; } = 0;
}

