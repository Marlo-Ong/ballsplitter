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
    private List<GameObject> ActivePool;
    private List<GameObject> InactivePool;
    public int ActiveBallCount
    {
        get => ActivePool.Count;
    }
    public int InactiveBallCount
    {
        get => InactivePool.Count;
    }

    private Coroutine _resetCoroutine;


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
        ActivePool = new();
        InactivePool = new();
        GlobalSplitChance = _startingSplitChance;

        for(int i = 0; i < _startingBallAmount; i++)
        {
            SetBallInactive(MakeNewBall());
        }

        _resetCoroutine ??= StartCoroutine(ResetSimulation());
    }

    # endregion

    # region Event Handlers

    void Ball_OnSplit(object sender, GameObject ball)
    {
        PlayActiveBall();
        OnGlobalSplit?.Invoke(null, gameObject);
    }

    void Ball_OnSplitFail(object sender, GameObject ball)
    {
        SetBallInactive(ball);
        OnGlobalSplitFail?.Invoke(null, gameObject);

        if (ActivePool.Count <= 0)
        {
            _resetCoroutine ??= StartCoroutine(ResetSimulation());
        }
    }

    public void UnityEvent_OnSliderValueChanged_SplitChance(float newChance)
    {
        GlobalSplitChance = newChance;
        if (_resetCoroutine != null)
        {
            StopCoroutine(_resetCoroutine);
        }
        _resetCoroutine = StartCoroutine(ResetSimulation());
    }

    public void UnityEvent_OnValueChanged_BallCollisionToggle(bool collide)
    {
        IsBallToBallCollisionOn = collide;
        _ballAmountLimit = (int)(_ballAmountLimit * (collide ? 0.5f : 2.0f));
        if (_resetCoroutine != null)
        {
            StopCoroutine(_resetCoroutine);
        }
        _resetCoroutine = StartCoroutine(ResetSimulation());
    }

    # endregion

    /// <summary>
    /// Kills all active balls, resets them with global parameters, and reactivates one to restart sim.
    /// </summary>
    private IEnumerator ResetSimulation()
    {   
        while (ActivePool.Count > 0)
        {
            SetBallInactive(ActivePool[ActiveBallCount - 1]);
        }

        yield return new WaitForSeconds(1);

        ResetAllBallParams();
        PlayActiveBall();

        OnResetSimulation?.Invoke(gameObject, gameObject);
        _resetCoroutine = null;
    }

    private void ResetAllBallParams()
    {
        InactivePool.Add(_ballPrefab);
        foreach(GameObject ball in InactivePool)
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
        InactivePool.RemoveAt(InactivePool.Count - 1);
    }

    /// <remarks>
    /// Currently no need to unsubscribe event handlers since ball objects aren't being destroyed
    /// </remarks>
    private GameObject MakeNewBall()
    {
        GameObject ball = Instantiate(_ballPrefab);
        ball.GetComponent<Ball>().OnSplit += Ball_OnSplit;
        ball.GetComponent<Ball>().OnSplitFail += Ball_OnSplitFail;
        return ball;
    }

    private GameObject SetBallActive(GameObject ball)
    {
        ball.transform.position = _ballPrefab.transform.position + new Vector3(Random.Range(-2f, 2f), Random.Range(-1f, 1f));
        ball.SetActive(true);
        ActivePool.Add(ball);
        if (InactiveBallCount > 0 && ball == InactivePool[InactiveBallCount - 1])
        {
            InactivePool.RemoveAt(InactiveBallCount - 1);
        }
        else
        {
            InactivePool.Remove(ball);
        }

        return ball;
    }

    private GameObject SetBallInactive(GameObject ball)
    {
        ball.SetActive(false);
        InactivePool.Add(ball);
        if (ActiveBallCount > 0 && ball == ActivePool[ActiveBallCount - 1])
        {
            ActivePool.RemoveAt(ActiveBallCount - 1);
        }
        else
        {
            ActivePool.Remove(ball);
        }

        return ball;
    }

    /// <summary>
    ///  Activates an available inactive ball or creates new one. Returns null if ball limit has been reached.
    /// </summary>
    public GameObject PlayActiveBall()
    {
        if (ActiveBallCount < _ballAmountLimit)
        {
            if (InactiveBallCount > 0)
            {
                return SetBallActive(InactivePool[InactiveBallCount - 1]);
            }
            else 
            {
                GameObject newBall = MakeNewBall();
                SetBallActive(newBall);
                return newBall;
            }
        }

        return null;
    }
}