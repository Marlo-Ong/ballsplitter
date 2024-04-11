using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class TextUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI splitChanceText;
    private int totalSplits;
    private int totalRuns;
    private int totalSplitFails;
    private int highestSplits;
    private float accuracy;
    private List<int> runSplits;
    private double meanSplits;
    void Start()
    {
        runSplits = new();
        meanSplits = 0;
        BallPool.OnResetSimulation += BallPool_OnResetSimulation;
        BallPool.OnGlobalSplit += BallPool_OnGlobalSplit;
        BallPool.OnGlobalSplitFail += BallPool_OnGlobalSplitFail;
    }

    void UpdateStatsText()
    {
        accuracy = (totalSplitFails + totalSplits) == 0 ? 1.0f : (totalSplits / (float)(totalSplitFails + totalSplits));
        statsText.text = $"Splits: {totalSplits} \nFails: {totalSplitFails} \nAccuracy: {accuracy:P} \n\nHighest splits: {highestSplits} \nAverage # splits: {meanSplits:P} \nTotal runs: {totalRuns} \n\nActive balls: {BallPool.Instance.ActiveBallCount} \nInactive balls: {BallPool.Instance.InactiveBallCount}";
    }

    void BallPool_OnResetSimulation(object s, GameObject sender)
    {
        runSplits.Add(totalSplits);
        meanSplits = runSplits.Average();

        totalSplits = 0;
        totalSplitFails = 0;
        totalRuns++;

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
        UpdateStatsText();
    }

    public void UnityEvent_OnUpdateSplitChance(float chance)
    {
        splitChanceText.text = $"Split Chance: {chance:P0}";
        ResetHighestStats();
        
    }

    public void UnityEvent_OnToggleBallCollision(bool _)
    {
        ResetHighestStats();
    }

    private void ResetHighestStats()
    {
        highestSplits = 0;
        totalRuns = 0;
        runSplits.Clear();
        meanSplits = 0;
        UpdateStatsText();
    }
}
