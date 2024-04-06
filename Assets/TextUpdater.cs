using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TextUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI splitChanceText;
    private int totalSplits;
    private int totalSplitFails;
    private int highestSplits;
    private int highestSplitFails;
    private float accuracy;
    void Start()
    {
        BallPool.OnResetSimulation += BallPool_OnResetSimulation;
        BallPool.OnGlobalSplit += BallPool_OnGlobalSplit;
        BallPool.OnGlobalSplitFail += BallPool_OnGlobalSplitFail;
    }

    void UpdateStatsText()
    {
        accuracy = (totalSplitFails + totalSplits) == 0 ? 1.0f : (totalSplits / (float)(totalSplitFails + totalSplits));
        statsText.text = $"Splits: {totalSplits} \nFails: {totalSplitFails} \n\nAccuracy: {accuracy:P} \n\nHighest splits: {highestSplits} \nHighest fails: {highestSplitFails} \n\nActive balls: {BallPool.Instance.ActiveBallCount} \nInactive balls: {BallPool.Instance.InactiveBallCount}";
    }

    void BallPool_OnResetSimulation(object s, GameObject sender)
    {
        totalSplits = 0;
        totalSplitFails = 0;
        UpdateStatsText();
    }

    void BallPool_OnGlobalSplit(object s, GameObject sender)
    {
        totalSplits++;
        highestSplits = Mathf.Max(highestSplits, totalSplits);
        UpdateStatsText();
    }

    void BallPool_OnGlobalSplitFail(object s, GameObject sender)
    {
        totalSplitFails++;
        highestSplitFails = Mathf.Max(highestSplitFails, totalSplitFails);
        UpdateStatsText();
    }

    public void UpdateSplitChanceText(float chance)
    {
        splitChanceText.text = $"Split Chance: {chance:P0}";
        highestSplits = 0;
        highestSplitFails = 0;
        UpdateStatsText();
    }
}
