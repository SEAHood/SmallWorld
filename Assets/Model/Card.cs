using Fusion;

namespace Assets.Model
{
    public struct Card : INetworkStruct
    {
        public NetworkString<_128> Id { get; set; }
        public Power Power { get; set; }
        public Race Race { get; set; }
        public int VictoryCoinsPlaced { get; set; }
        public bool Claimed { get; set; }

        public int TotalTokens => Power.Tokens + Race.Tokens;
    }
}
