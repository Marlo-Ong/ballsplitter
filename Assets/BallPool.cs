using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallPool : MonoBehaviour
{
    /// <summary>
    /// A List-based object pooling test to optimize instantition/creation of balls,
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
    [field: Range(0.0f, 1.0f)][field: SerializeField] private float _startingSplitChance;
    public float GlobalSplitChance;
    public bool IsBallToBallCollisionOn;
    private List<GameObject> AlivePool;
    private List<GameObject> DeadPool;
    public int ActiveBallCount
    {
        get => AlivePool.Count;
    }
    public int InactiveBallCount
    {
        get => DeadPool.Count;
    }

    private Coroutine _resetCoroutine;


    # region Instantiation Methods
    void Awake()
    {
        AlivePool = new();
        DeadPool = new();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }
    
    void Start()
    {
        GlobalSplitChance = _startingSplitChance;

        for(int i = 0; i < _startingBallAmount; i++)
        {
            SetBallDead(MakeNewBall());
        }

        _resetCoroutine ??= StartCoroutine(ResetSimulation());
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
            _resetCoroutine ??= StartCoroutine(ResetSimulation());
        }
    }

    /// <remarks>
    /// Called by OnSliderValueChanged UnityEvent
    /// </remarks>
    public void UpdateGlobalSplitChance(float newChance)
    {
        GlobalSplitChance = newChance;
        if (_resetCoroutine != null)
        {
            StopCoroutine(_resetCoroutine);
        }
        _resetCoroutine = StartCoroutine(ResetSimulation());
    }

    public void ToggleBallToBallCollision(bool collide)
    {
        IsBallToBallCollisionOn = collide;
        if (_resetCoroutine != null)
        {
            StopCoroutine(_resetCoroutine);
        }
        _resetCoroutine = StartCoroutine(ResetSimulation());
    }

    # endregion

    /// <summary>
    /// Kills all active balls, resets them to global parameters, and reactivates one.
    /// </summary>
    private IEnumerator ResetSimulation()
    {   
        yield return new WaitForSeconds(1);

        while (AlivePool.Count > 0)
        {
            SetBallDead(AlivePool[0]);
        }

        DeadPool.Add(_ballPrefab);
        foreach(GameObject ball in DeadPool)
        {
            ball.GetComponent<Ball>().SplitChance = GlobalSplitChance;
            if (IsBallToBallCollisionOn)
            {
                ball.GetComponent<Collider2D>().excludeLayers = LayerMask.GetMask("Nothing");
            }
            else
            {
                ball.GetComponent<Collider2D>().excludeLayers = LayerMask.GetMask("Balls");
            }
        }
        DeadPool.RemoveAt(DeadPool.Count - 1);
        GetBall();
        
        OnResetSimulation?.Invoke(gameObject, gameObject);
        _resetCoroutine = null;
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

    private GameObject SetBallAlive(GameObject ball)
    {
        ball.transform.position = (Vector2)_ballPrefab.transform.position + new Vector2(Random.Range(-2f, 2f), Random.Range(-1f, 1f));
        ball.SetActive(true);
        AlivePool.Add(ball);
        DeadPool.Remove(ball);

        return ball;
    }

    private GameObject SetBallDead(GameObject ball)
    {
        ball.SetActive(false);
        DeadPool.Add(ball);
        AlivePool.Remove(ball);

        return ball;
    }

    /// <summary>
    ///  Returns an alive ball if available; returns null if over ball limit.
    /// </summary>
    public GameObject GetBall()
    {
        if (DeadPool.Count > 0)
        {
            return SetBallAlive(DeadPool[0]);
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