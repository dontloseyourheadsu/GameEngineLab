using System;
using System.Collections.Generic;

namespace GameEngineLab.Core.Features.Online.Resources;

public interface INetworkTransport : IDisposable
{
    bool IsActive { get; }
    
    void StartHost(int port);
    
    void Connect(string address, int port);
    
    void Send(NetworkPacket packet);
    
    IEnumerable<NetworkPacket> Receive();
    
    void Stop();
}
