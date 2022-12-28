using Assets.Repo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUi : MonoBehaviour
{
    public DescriptionUi DescriptionUi;
    public GameObject CoinPrefab;
    public Transform CoinTarget;

    public void GenerateNewCombo()
    {
        var comboRepo = new ComboRepo();
        var combo = comboRepo.GetCombos(1);
        DescriptionUi.Populate(combo[0]);
    }

    public void GenerateCoin()
    {
        for (var i = 0; i < 20; i++)
        {
            var coin = Instantiate(CoinPrefab, transform);
            coin.transform.position = new Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height), 0f);
            coin.GetComponent<CoinUi>().Initialise(Random.Range(1, 10), CoinTarget.position, true, () => { });
        }
        
    }
}
