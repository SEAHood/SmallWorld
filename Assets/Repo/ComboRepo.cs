using Assets.Model;
using System;
using System.Collections.Generic;

namespace Assets.Repo
{
    public class ComboRepo
    {
        public PowerRepo powerRepo;
        public RaceRepo raceRepo;

        public ComboRepo()
        {
            powerRepo = new PowerRepo();
            raceRepo = new RaceRepo();
        }

        public List<Combo> GetCombos(int count)
        {
            var powers = powerRepo.GetPowers(count);
            var races = raceRepo.GetRaces(count);
            var combos = new List<Combo>();

            for (int i = 0; i < count; i++)
            {
                combos.Add(new Combo 
                { 
                    Id = Guid.NewGuid().ToString(),
                    Power = powers[i], 
                    Race = races[i],
                    Claimed = false,
                    CoinsPlaced = 0
                });
            }

            return combos;
        }
    }
}
