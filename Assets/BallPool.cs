using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPool : MonoBehaviour
{
    /// <summary>
    /// A queue-based object pooling test to optimize instantition/creation of balls,
    /// where balls "split" with a 50/50 chance (adjustable in inspector).
    /// Could potentially add a ball lifetime to <see cref="Ball"/> for better memory? 
    /// </summary>

    public static BallPool Instance;
    public static event System.EventHandler<GameObject> OnResetSimulation;
    public static event System.EventHandler<GameObject> OnGlobalSplit;
    public static event System.EventHandler<GameObject> OnGlobalSplitFail;
    [field: SerializeField] private GameObject _ballPrefab;
    [field: SerializeField] private int _startingBallAmount;
    [field: SerializeField] private int _ballAmountLimit;
    private Queue<GameObject> AlivePool;
    private Queue<GameObject> DeadPool;


    # region Instantiation Methods
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }
    
    void Start()
    {
        // Instantiate an object pool with a number of balls to start.
        AlivePool = new Queue<GameObject>();
        DeadPool = new Queue<GameObject>();

        for(int i = 0; i < _startingBallAmount; i++)
        {
            SetBallDead(MakeNewBall());
        }

        GetBall();
    }

    /// <remarks>
    /// No need to unsubscribe since ball objects aren't being destroyed
    /// </remarks>
    private GameObject MakeNewBall()
    {
        GameObject ball = Instantiate(_ballPrefab);
        ball.GetComponent<Ball>().OnSplit += Split_OnSplit;
        ball.GetComponent<Ball>().OnSplitFail += Split_OnSplitFail;
        return ball;
    }

    # endregion

    # region Event Handlers

    void Split_OnSplit(object sender, GameObject ball)
    {
        GetBall();
        OnGlobalSplit?.Invoke(gameObject, gameObject);
    }

    void Split_OnSplitFail(object sender, GameObject ball)
    {
        SetBallDead(ball);
        OnGlobalSplitFail?.Invoke(gameObject, gameObject);

        if (AlivePool.Count <= 0)
        {
            StartCoroutine(ResetSimulation());
        }
    }

    # endregion

    private void SetBallAlive(GameObject ball)
    {
        ball.transform.position = new Vector2(Random.Range(-2f, 2f), Random.Range(-1f, 1f));
        ball.SetActive(true);
        AlivePool.Enqueue(ball);
        DeadPool.TryDequeue(out GameObject g);
    }

    private void SetBallDead(GameObject ball)
    {
        ball.SetActive(false);
        DeadPool.Enqueue(ball);
        AlivePool.TryDequeue(out GameObject g);
    }

    private IEnumerator ResetSimulation()
    {
        yield return new WaitForSeconds(2);
        OnResetSimulation?.Invoke(gameObject, gameObject);
        GetBall();
    }

    /// <summary>
    ///  Returns an alive ball if available; returns null if over ball limit.
    /// </summary>
    public GameObject GetBall()
    {
        if (DeadPool.TryPeek(out GameObject tempBall))
        {
            SetBallAlive(tempBall);
            return tempBall;
        }

        else if (AlivePool.Count < _ballAmountLimit)
        {
            GameObject newBall = MakeNewBall();
            SetBallAlive(newBall);
            return newBall;
        }

        return null;
    }
}