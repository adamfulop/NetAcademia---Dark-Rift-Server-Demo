namespace WonderGameServer.Model {
    public class Player {
        // játékos kliens azonosító
        public ushort Id { get; set; }
        // pozíció (elég az X és Z koordináta)
        public float PositionX { get; set; }
        public float PositionZ { get; set; }
        // Y orientáció
        public float RotationY { get; set; }
    }
}