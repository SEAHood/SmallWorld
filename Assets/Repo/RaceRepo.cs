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
            new Race { Name = "Amazon", Tokens = 6 },
            new Race { Name = "Dwarve", Tokens = 3 },
            new Race { Name = "Elf", Tokens = 6 },
            new Race { Name = "Ghoul", Tokens = 5 },
            new Race { Name = "Giant", Tokens = 6 },
            new Race { Name = "Halfling", Tokens = 6 },
            new Race { Name = "Human", Tokens = 5 },
            new Race { Name = "Orc", Tokens = 5 },
            new Race { Name = "Ratmen", Tokens = 8 },
            new Race { Name = "Skeleton", Tokens = 6 },
            new Race { Name = "Sorcerer", Tokens = 5 },
            new Race { Name = "Triton", Tokens = 6 },
            new Race { Name = "Troll", Tokens = 5 },
            new Race { Name = "Wizard", Tokens = 5 },
        };

}
}
