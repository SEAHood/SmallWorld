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
            new Power { Name = "Alchemist", Tokens = 4 },
            new Power { Name = "Berserk", Tokens = 4 },
            new Power { Name = "Bivouacking", Tokens = 5 },
            new Power { Name = "Commando", Tokens = 4 },
            new Power { Name = "Diplomat", Tokens = 5 },
            new Power { Name = "DragonMaster", Tokens = 5 },
            new Power { Name = "Flying", Tokens = 5 },
            new Power { Name = "Forest", Tokens = 4 },
            new Power { Name = "Fortified", Tokens = 3 },
            new Power { Name = "Heroic", Tokens = 5 },
            new Power { Name = "Hill", Tokens = 4 },
            new Power { Name = "Merchant", Tokens = 2 },
            new Power { Name = "Mounted", Tokens = 5 },
            new Power { Name = "Pillaging", Tokens = 5 },
            new Power { Name = "Seafaring", Tokens = 5 },
            new Power { Name = "Spirit", Tokens = 5 },
            new Power { Name = "Stout", Tokens = 4 },
            new Power { Name = "Swamp", Tokens = 4 },
            new Power { Name = "Underworld", Tokens = 5 },
            new Power { Name = "Wealthy", Tokens = 4 },
        };
    }
}
