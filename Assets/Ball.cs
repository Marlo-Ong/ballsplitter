using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Range(0f, 1f)] public float splitProbability;
    public event System.EventHandler<GameObject> OnSplit;
    public event System.EventHandler<GameObject> OnSplitFail;
    void OnCollisionEnter2D(Collision2D collision) // ignores balls on same layer
    {
        if (Random.Range(0f, 1f) < splitProbability) 
        {
            OnSplit?.Invoke(gameObject, gameObject);
        }
        else
        {
            OnSplitFail?.Invoke(gameObject, gameObject);
        }
    }
}
