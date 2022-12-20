using Assets.Enum;
using Fusion;

namespace Assets.Model
{
    public struct TokenStack : INetworkStruct
    {
        public Power Power { get; set; }
        public Race Race { get; set; }
        public int Count { get; set; }
        public Team Team { get; set; }
        public bool Interactable { get; set; }
        public NetworkString<_128> OwnerId { get; set; }
        public bool InPlay { get; set; }
    }
}
