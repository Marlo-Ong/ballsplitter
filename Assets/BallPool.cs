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
    public float GlobalSplitChance;
    private Queue<GameObject> AlivePool;
    private Queue<GameObject> DeadPool;
    private Coroutine _simulationCoroutine;


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
        GlobalSplitChance = _ballPrefab.GetComponent<Ball>().SplitChance;

        for(int i = 0; i < _startingBallAmount; i++)
        {
            SetBallDead(MakeNewBall());
        }

        _simulationCoroutine ??= StartCoroutine(ResetSimulation());
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
        OnGlobalSplit?.Invoke(null, gameObject);
    }

    void Split_OnSplitFail(object sender, GameObject ball)
    {
        SetBallDead(ball);
        OnGlobalSplitFail?.Invoke(null, gameObject);

        if (AlivePool.Count <= 0)
        {
            _simulationCoroutine ??= StartCoroutine(ResetSimulation());
        }
    }

    # endregion

    private void SetBallAlive(GameObject ball)
    {
        ball.transform.position = (Vector2)_ballPrefab.transform.position + new Vector2(Random.Range(-2f, 2f), Random.Range(-1f, 1f));
        ball.GetComponent<Ball>().SplitChance = GlobalSplitChance;
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
        Time.timeScale = 0.0f;
        while (AlivePool.Count > 0)
        {
            SetBallDead(AlivePool.Peek());
        }
        foreach(GameObject ball in DeadPool)
        {
            ball.SetActive(false);
        }
        Time.timeScale = 1.0f;
        OnResetSimulation?.Invoke(gameObject, gameObject);
        yield return new WaitForSeconds(2);
        GetBall();

        _simulationCoroutine = null;
    }

    /// <remarks>
    /// Called by OnSliderValueChanged UnityEvent
    /// </remarks>
    public void UpdateGlobalSplitChance(float newChance)
    {
        GlobalSplitChance = newChance;
        _simulationCoroutine ??= StartCoroutine(ResetSimulation());
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