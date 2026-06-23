using System;
using System.IO;
using System.Text;
using System.Linq;
using GameEngineLab.Core.Features.Ecs.Entities;
using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Identity.Components;
using GameEngineLab.Core.Features.Identity.Resources;
using GameEngineLab.Core.Features.Online.Resources;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Components;
using GameEngineLab.GolfIt.Features.Ball.Components;
using GameEngineLab.Core.Features.Physics.Resources;
using Microsoft.Xna.Framework;

namespace GameEngineLab.GolfIt.Features.Runtime;

public sealed class GolfItMultiplayerSystem : IGameSystem
{
    public int Order => -90; // Run early, right after core NetworkSystem

    private float _timeSinceLastSend;

    public void Update(World world, FrameContext frameContext)
    {
        if (!world.TryGetResource<NetworkResource>(out var net) || net == null) return;
        if (!world.TryGetResource<GameStateResource>(out var state) || state == null) return;
        if (!world.TryGetResource<UserAccountResource>(out var account) || account == null) return;

        // 1. Process Incoming Packets
        while (net.IncomingPackets.TryDequeue(out var packet))
        {
            ProcessIncomingPacket(world, state, net, account, packet);
        }

        // 2. Send Periodical Updates if Connected and Playing
        if (net.IsConnected && state.Current == GameState.Playing)
        {
            _timeSinceLastSend += (float)frameContext.GameTime.ElapsedGameTime.TotalSeconds;
            if (_timeSinceLastSend >= 0.05f) // 20 updates per second
            {
                _timeSinceLastSend = 0f;
                SendLocalUpdate(world, net, account, state);
            }
        }
    }

    public void Draw(World world, FrameContext frameContext) { }

    private void ProcessIncomingPacket(World world, GameStateResource state, NetworkResource net, UserAccountResource account, NetworkPacket packet)
    {
        switch (packet.Type)
        {
            case PacketType.Handshake:
                var payloadStr = Encoding.UTF8.GetString(packet.Payload);
                if (payloadStr.StartsWith("START_GAME"))
                {
                    // Start game message
                    state.Current = GameState.Playing;
                    state.Strokes = 0;
                    
                    // Trigger StartPlaying in game via the action queue
                    if (world.TryGetResource<ActionQueueResource>(out var actionQueue) && actionQueue != null)
                    {
                        var parts = payloadStr.Split(':');
                        var mapPath = parts.Length > 1 ? parts[1] : "";
                        actionQueue.Enqueue($"mp_start_game_trigger:{mapPath}");
                    }
                }
                else
                {
                    // Guest/Host info exchange
                    net.RemotePlayerId = packet.SenderId;
                    net.RemotePlayerName = payloadStr;
                    
                    if (net.IsHost)
                    {
                        // Host answers handshake with their own info
                        var response = new NetworkPacket
                        {
                            Type = PacketType.Handshake,
                            SenderId = account.UserId,
                            Payload = Encoding.UTF8.GetBytes(account.Username)
                        };
                        net.OutgoingPackets.Enqueue(response);
                        
                        net.State = NetworkState.ConnectedAsHost; // Maintain host connection state
                    }
                    else
                    {
                        net.State = NetworkState.ConnectedAsGuest; // Guest connection confirmed
                    }
                }
                break;

            case PacketType.InputUpdate:
                if (packet.Payload.Length >= 20)
                {
                    // Deserialize position (8 bytes), velocity (8 bytes), strokes (4 bytes)
                    var posX = BitConverter.ToSingle(packet.Payload, 0);
                    var posY = BitConverter.ToSingle(packet.Payload, 4);
                    var velX = BitConverter.ToSingle(packet.Payload, 8);
                    var velY = BitConverter.ToSingle(packet.Payload, 12);
                    var strokes = BitConverter.ToInt32(packet.Payload, 16);

                    net.RemotePlayerStrokes = strokes;

                    // Find or spawn the remote player's ball
                    var remoteBall = world.GetEntitiesWith<BallComponent, UserIdentityComponent>()
                        .FirstOrDefault(e => world.TryGetComponent<UserIdentityComponent>(e, out var id) && id.UserId == packet.SenderId);

                    if (remoteBall == default)
                    {
                        // Spawn remote ball
                        remoteBall = world.CreateEntity();
                        world.SetComponent(remoteBall, new BallComponent());
                        world.SetComponent(remoteBall, new RigidBodyComponent
                        {
                            Shape = RigidBodyShape.Circle,
                            BoundingRadius = 16f,
                            Restitution = 0.6f,
                            Friction = 0.99f,
                            Mass = 5.0f
                        });
                        world.SetComponent(remoteBall, new TransformComponent { Position = new Vector2(posX, posY) });
                        world.SetComponent(remoteBall, new VelocityComponent { Value = new Vector2(velX, velY) });
                        world.SetComponent(remoteBall, new UserIdentityComponent { UserId = packet.SenderId, Username = net.RemotePlayerName });
                        
                        // Give it a distinct Red color to visually differentiate it from the local ball
                        world.SetComponent(remoteBall, new DrawColorComponent(Color.Red));
                    }
                    else
                    {
                        // Update position and velocity
                        world.TryGetComponent<TransformComponent>(remoteBall, out var transform);
                        world.TryGetComponent<VelocityComponent>(remoteBall, out var velocity);
                        
                        transform.Position = new Vector2(posX, posY);
                        velocity.Value = new Vector2(velX, velY);
                        
                        world.SetComponent(remoteBall, transform);
                        world.SetComponent(remoteBall, velocity);
                    }
                }
                break;
        }
    }

    private void SendLocalUpdate(World world, NetworkResource net, UserAccountResource account, GameStateResource state)
    {
        // Find our local ball
        var localBall = world.GetEntitiesWith<BallComponent, UserIdentityComponent>()
            .FirstOrDefault(e => world.TryGetComponent<UserIdentityComponent>(e, out var id) && id.UserId == account.UserId);

        if (localBall == default) return;

        world.TryGetComponent<TransformComponent>(localBall, out var transform);
        world.TryGetComponent<VelocityComponent>(localBall, out var velocity);

        // Serialize position.X, position.Y, velocity.X, velocity.Y, strokes
        var payload = new byte[20];
        Array.Copy(BitConverter.GetBytes(transform.Position.X), 0, payload, 0, 4);
        Array.Copy(BitConverter.GetBytes(transform.Position.Y), 0, payload, 4, 4);
        Array.Copy(BitConverter.GetBytes(velocity.Value.X), 0, payload, 8, 4);
        Array.Copy(BitConverter.GetBytes(velocity.Value.Y), 0, payload, 12, 4);
        Array.Copy(BitConverter.GetBytes(state.Strokes), 0, payload, 16, 4);

        var packet = new NetworkPacket
        {
            Type = PacketType.InputUpdate,
            SenderId = account.UserId,
            Payload = payload
        };

        net.OutgoingPackets.Enqueue(packet);
    }
}
