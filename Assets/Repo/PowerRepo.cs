using Assets.Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Repo
{
    public class PowerRepo
    {
        public List<Power> AvailablePowers { get; set; }

        public PowerRepo()
        {
            ResetPowers();
        }

        public List<Power> GetPowers(int count)
        {
            // TODO This will break if count > AvailablePowers.Count

            var powers = new List<Power>();
            var nums = Enumerable.Range(0, AvailablePowers.Count).ToArray();
            for (var i = 0; i < nums.Length; ++i)
            {
                var randomIndex = Random.Range(0, nums.Length);
                var temp = nums[randomIndex];
                nums[randomIndex] = nums[i];
                nums[i] = temp;
            }
            Debug.Log($"GetPowers() nums: {string.Join(',', nums)}");

            for (var i = 0; i < count; i++)
            {
                Debug.Log($"Adding power at index {nums[i]}");
                powers.Add(AvailablePowers[nums[i]]);
            }

            AvailablePowers = AvailablePowers.Except(powers).ToList();
            return powers;
        }

        public void ResetPowers()
        {
            AvailablePowers = AllPowers;
        }

        private List<Power> AllPowers = new List<Power>
        {
            new Power { Name = "Alchemist", RaceTokens = 4 },
            new Power { Name = "Berserk", RaceTokens = 4 },
            new Power { Name = "Bivouacking", RaceTokens = 5 },
            new Power { Name = "Commando", RaceTokens = 4 },
            new Power { Name = "Diplomat", RaceTokens = 5 },
            new Power { Name = "Dragon Master", RaceTokens = 5 },
            new Power { Name = "Flying", RaceTokens = 5 },
            new Power { Name = "Forest", RaceTokens = 4 },
            new Power { Name = "Fortified", RaceTokens = 3 },
            new Power { Name = "Heroic", RaceTokens = 5 },
            new Power { Name = "Hill", RaceTokens = 4 },
            new Power { Name = "Merchant", RaceTokens = 2 },
            new Power { Name = "Mounted", RaceTokens = 5 },
            new Power { Name = "Pillaging", RaceTokens = 5 },
            new Power { Name = "Seafaring", RaceTokens = 5 },
            new Power { Name = "Spirit", RaceTokens = 5 },
            new Power { Name = "Stout", RaceTokens = 4 },
            new Power { Name = "Swamp", RaceTokens = 4 },
            new Power { Name = "Underworld", RaceTokens = 5 },
            new Power { Name = "Wealthy", RaceTokens = 4 },
        };
    }
}
