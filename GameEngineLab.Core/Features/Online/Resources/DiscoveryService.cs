using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameEngineLab.Core.Features.Online.Resources;

public sealed class DiscoveryService : IDisposable
{
    private const int DiscoveryPort = 8888;
    private const string DiscoveryMessage = "GAMELAB_HOST";
    
    private UdpClient? _udpClient;
    private bool _isListening;
    private readonly HashSet<string> _foundHosts = new();

    public IEnumerable<string> FoundHosts => _foundHosts;

    public void StartBroadcasting()
    {
        _udpClient = new UdpClient();
        _udpClient.EnableBroadcast = true;
        
        Task.Run(async () =>
        {
            var data = Encoding.UTF8.GetBytes(DiscoveryMessage);
            var endpoint = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);
            
            while (_udpClient != null)
            {
                await _udpClient.SendAsync(data, data.Length, endpoint);
                await Task.Delay(2000);
            }
        });
    }

    public void StartListening()
    {
        if (_isListening) return;
        
        _udpClient = new UdpClient(DiscoveryPort);
        _isListening = true;

        Task.Run(async () =>
        {
            while (_isListening && _udpClient != null)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync();
                    var message = Encoding.UTF8.GetString(result.Buffer);
                    
                    if (message == DiscoveryMessage)
                    {
                        var ip = result.RemoteEndPoint.Address.ToString();
                        lock (_foundHosts)
                        {
                            _foundHosts.Add(ip);
                        }
                    }
                }
                catch
                {
                    // Listener closed
                }
            }
        });
    }

    public void Dispose()
    {
        _isListening = false;
        _udpClient?.Dispose();
        _udpClient = null;
    }
}
