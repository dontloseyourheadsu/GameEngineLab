using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace GameEngineLab.Core.Features.Online.Resources;

public sealed class SimpleUdpTransport : INetworkTransport
{
    private UdpClient? _udpClient;
    private IPEndPoint? _remoteEndPoint;
    private readonly ConcurrentQueue<NetworkPacket> _receiveQueue = new();
    private bool _isHost;

    public bool IsActive => _udpClient != null;

    public void StartHost(int port)
    {
        _udpClient = new UdpClient(port);
        _isHost = true;
        BeginReceive();
    }

    public void Connect(string address, int port)
    {
        _udpClient = new UdpClient();
        _remoteEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
        _isHost = false;
        BeginReceive();
    }

    public void Send(NetworkPacket packet)
    {
        if (_udpClient == null || _remoteEndPoint == null) return;
        
        var data = NetworkPacket.Serialize(packet);
        _udpClient.Send(data, data.Length, _remoteEndPoint);
    }

    public IEnumerable<NetworkPacket> Receive()
    {
        while (_receiveQueue.TryDequeue(out var packet))
        {
            yield return packet;
        }
    }

    private void BeginReceive()
    {
        _udpClient?.BeginReceive(OnReceive, null);
    }

    private void OnReceive(IAsyncResult ar)
    {
        if (_udpClient == null) return;

        try
        {
            IPEndPoint? senderEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = _udpClient.EndReceive(ar, ref senderEndPoint);
            
            // For P2P, the first person who talks to the host becomes the "remote"
            if (_isHost && _remoteEndPoint == null)
            {
                _remoteEndPoint = senderEndPoint;
            }

            var packet = NetworkPacket.Deserialize(data);
            _receiveQueue.Enqueue(packet);
            
            _udpClient.BeginReceive(OnReceive, null);
        }
        catch
        {
            // Socket closed
        }
    }

    public void Stop()
    {
        _udpClient?.Close();
        _udpClient = null;
        _remoteEndPoint = null;
    }

    public void Dispose()
    {
        Stop();
    }
}
