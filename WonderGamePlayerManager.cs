using System;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using WonderGameServer.Model;

namespace WonderGameServer {
    public class WonderGamePlayerManager : Plugin {
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
        }
    }
}