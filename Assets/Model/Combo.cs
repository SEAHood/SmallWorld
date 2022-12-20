using Fusion;

namespace Assets.Model
{
    public struct Combo : INetworkStruct
    {
        public NetworkString<_128> Id { get; set; }
        public Power Power { get; set; }
        public Race Race { get; set; }
        public int CoinsPlaced { get; set; }
        public bool Claimed { get; set; }

        public int TotalTokens => Power.Tokens + Race.Tokens;

        public bool IsBlank => string.IsNullOrEmpty(Power.Name.ToString()) && string.IsNullOrEmpty(Race.Name.ToString());
        
        public override string ToString()
        {
            return $"{BeautifyPowerName()} {BeautifyRaceName()}";
        }

        private string BeautifyPowerName()
        {
            if (Power.Name == "DragonMaster") return "Dragon Master";
            return Power.Name.ToString();
        }

        private string BeautifyRaceName()
        {
            switch (Race.Name.ToString())
            {
                case "Amazon":
                    return "Amazons";
                case "Dwarf":
                    return "Dwarves";
                case "Elf":
                    return "Elves";
                case "Ghoul":
                    return "Ghouls";
                case "Giant":
                    return "Giants";
                case "Halfling":
                    return "Halflings";
                case "Human":
                    return "Humans";
                case "Orc":
                    return "Orcs";
                case "Ratmen":
                    return "Ratmen";
                case "Skeleton":
                    return "Skeletons";
                case "Sorcerer":
                    return "Sorcerers";
                case "Triton":
                    return "Tritons";
                case "Troll":
                    return "Trolls";
                case "Wizard":
                    return "Wizards";
                default:
                    return Race.Name.ToString();
            }
        }
    }
}
