using Assets.Repo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUi : MonoBehaviour
{
    public DescriptionUi DescriptionUi;

    public void GenerateNewCombo()
    {
        var comboRepo = new ComboRepo();
        var combo = comboRepo.GetCombos(1);
        DescriptionUi.Populate(combo[0]);
    }
}
