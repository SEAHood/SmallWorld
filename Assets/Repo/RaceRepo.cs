using Assets.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Repo
{
    public class RaceRepo
    {
        public List<Race> AvailableRaces { get; set; }

        public RaceRepo()
        {
            ResetRaces();
        }

        public List<Race> GetRaces(int count)
        {
            // TODO This will break if count > AvailablePowers.Count

            var races = new List<Race>();
            var nums = Enumerable.Range(0, AvailableRaces.Count).ToArray();
            for (var i = 0; i < nums.Length; ++i)
            {
                var randomIndex = Random.Range(0, nums.Length);
                var temp = nums[randomIndex];
                nums[randomIndex] = nums[i];
                nums[i] = temp;
            }
            Debug.Log($"GetRaces() nums: {string.Join(',', nums)}");

            for (var i = 0; i < count; i++)
            {
                Debug.Log($"Adding power at index {nums[i]}");
                races.Add(AvailableRaces[nums[i]]);
            }

            AvailableRaces = AvailableRaces.Except(races).ToList();
            return races;
        }

        public void ResetRaces()
        {
            AvailableRaces = AllRaces;
        }

        private List<Race> AllRaces = new List<Race>
        {
            new Race { Name = "Amazons", RaceTokens = 6 },
            new Race { Name = "Dwarves", RaceTokens = 3 },
            new Race { Name = "Elves", RaceTokens = 6 },
            new Race { Name = "Ghouls", RaceTokens = 5 },
            new Race { Name = "Giants", RaceTokens = 6 },
            new Race { Name = "Halflings", RaceTokens = 6 },
            new Race { Name = "Humans", RaceTokens = 5 },
            new Race { Name = "Orcs", RaceTokens = 5 },
            new Race { Name = "Ratmen", RaceTokens = 8 },
            new Race { Name = "Skeletons", RaceTokens = 6 },
            new Race { Name = "Sorcerers", RaceTokens = 5 },
            new Race { Name = "Tritons", RaceTokens = 6 },
            new Race { Name = "Trolls", RaceTokens = 5 },
            new Race { Name = "Wizards", RaceTokens = 5 },
        };

}
}
