using Fusion;

namespace Assets.Model
{
    public struct Card : INetworkStruct
    {
        public Power Power { get; set; }
        public Race Race { get; set; }
        public int VictoryCoinsPlaced { get; set; }
    }
}
