using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TextUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalSplitsText;
    [SerializeField] private TextMeshProUGUI totalSplitFailsText;
    [SerializeField] private TextMeshProUGUI highestSplitsText;
    [SerializeField] private TextMeshProUGUI highestSplitFailsText;
    [SerializeField] private TextMeshProUGUI splitChanceText;
    public float totalSplits;
    public float totalSplitFails;
    public float highestSplits;
    public float highestSplitFails;
    void Start()
    {
        BallPool.OnResetSimulation += BallPool_OnResetSimulation;
        BallPool.OnGlobalSplit += BallPool_OnGlobalSplit;
        BallPool.OnGlobalSplitFail += BallPool_OnGlobalSplitFail;
    }

    void BallPool_OnResetSimulation(object s, GameObject sender)
    {
        totalSplits = 0;
        totalSplitFails = 0;
        totalSplitsText.text = "Splits: 0";
        totalSplitFailsText.text = "Fails: 0";
    }

    void BallPool_OnGlobalSplit(object s, GameObject sender)
    {
        totalSplits++;
        highestSplits = Mathf.Max(highestSplits, totalSplits);
        totalSplitsText.text = "Splits: " + totalSplits.ToString();
        highestSplitsText.text = "Highest splits: " + highestSplits.ToString();
    }

    void BallPool_OnGlobalSplitFail(object s, GameObject sender)
    {
        totalSplitFails++;
        highestSplitFails = Mathf.Max(highestSplitFails, totalSplitFails);
        totalSplitFailsText.text = "Fails: " + totalSplitFails.ToString();
        highestSplitFailsText.text = "Highest fails: " + highestSplitFails.ToString();
    }

    public void UpdateSplitChanceText(float chance)
    {
        splitChanceText.text = $"Split Chance: {chance:F2}";
        highestSplits = 0;
        highestSplitFails = 0;
        highestSplitsText.text = "Highest splits: 0";
        highestSplitFailsText.text = "Highest fails: 0";
    }
}
