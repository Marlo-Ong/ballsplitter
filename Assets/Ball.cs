using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Range(0f, 1f)] public float SplitChance;
    public event System.EventHandler<GameObject> OnSplit;
    public event System.EventHandler<GameObject> OnSplitFail;
    void Start()
    {
        SetRandomColor();
    }
    void OnCollisionEnter2D(Collision2D collision) // ignores balls on same layer
    {
        if (GetRandomSplit()) 
        {
            OnSplit?.Invoke(gameObject, gameObject);
        }
        else
        {
            OnSplitFail?.Invoke(gameObject, gameObject);
        }
    }

    void OnCollisionStay2D(Collision2D collision) => OnCollisionEnter2D(collision);

    /// <summary>
    /// Returns if ball should split based on correct probabilities from PRNG
    /// </summary>
    private bool GetRandomSplit()
    {
        // 50/50 split is reflected more accurately using custom PRNG
        if (Mathf.Approximately(SplitChance, 0.5f))
        {
            ulong random = RandomsUtil.Instance.Xoshiro256Plus();
            int chooseBit = Random.Range(10, 63); // lowest 3 bits from xoshiro+ are low quality
            ulong e = random & ((ulong)1 << chooseBit); // check if randomly chosen bit is 1 or 0
            return e != 0; 
        }

        return Random.Range(0f, 1f) < SplitChance;
    }

    protected void SetRandomColor()
    {
        GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }
}
