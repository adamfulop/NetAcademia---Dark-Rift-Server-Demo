using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using WonderGameServer.Model;

namespace WonderGameServer {
    public class WonderGamePlayerManager : Plugin {
        private readonly Dictionary<IClient, Player> _players = new Dictionary<IClient, Player>();
        
        public WonderGamePlayerManager(PluginLoadData pluginLoadData) : base(pluginLoadData) {
            ClientManager.ClientConnected += ClientConnected;
        }
        public override Version Version => new Version(1, 0);
        public override bool ThreadSafe => false;

        // új kliens csatlakozásakor callback függvény
        private void ClientConnected(object sender, ClientConnectedEventArgs e) {
            // játékos létrehozása
            var newPlayer = new Player {
                Id = e.Client.ID,
                PositionX = 145,
                PositionZ = 185,
                RotationY = 0
            };

            using (var writer = DarkRiftWriter.Create()) {
                writer.Write(newPlayer.Id);
                writer.Write(newPlayer.PositionX);
                writer.Write(newPlayer.PositionZ);
                writer.Write(newPlayer.RotationY);

                // az új klienst kivéve az összes többinek küldünk egy üzenetet az új játékosról
                using (var message = Message.Create(MessageTags.SpawnPlayerTag, writer)) {
                    var otherClients = ClientManager.GetAllClients().Where(c => c != e.Client);
                    foreach (var client in otherClients) {
                        client.SendMessage(message, SendMode.Reliable);
                    }
                }
            }
            
            _players.Add(e.Client, newPlayer);
            using (var writer = DarkRiftWriter.Create()) {
                foreach (var player in _players.Values) {
                    writer.Write(player.Id);
                    writer.Write(player.PositionX);
                    writer.Write(player.PositionZ);
                    writer.Write(player.RotationY);
                }

                using (var message = Message.Create(MessageTags.SpawnPlayerTag, writer)) {
                    e.Client.SendMessage(message, SendMode.Reliable);
                }
            }

            e.Client.MessageReceived += MovementMessageReceived;
        }

        private void MovementMessageReceived(object sender, MessageReceivedEventArgs e) {
            using (var message = e.GetMessage()) {
                if (message.Tag == MessageTags.MovePlayerTag) {
                    using (var reader = message.GetReader()) {
                        var positionX = reader.ReadSingle();
                        var positionZ = reader.ReadSingle();
                        var rotationY = reader.ReadSingle();

                        var player = _players[e.Client];
                        player.PositionX = positionX;
                        player.PositionZ = positionZ;
                        player.RotationY = rotationY;

                        using (var writer = DarkRiftWriter.Create()) {
                            writer.Write(player.Id);
                            writer.Write(player.PositionX);
                            writer.Write(player.PositionZ);
                            writer.Write(player.RotationY);
                            message.Serialize(writer);
                        }

                        foreach (var client in ClientManager.GetAllClients().Where(c => c != e.Client)) {
                            client.SendMessage(message, e.SendMode);
                        }
                    }
                }
            }
        }
    }
}