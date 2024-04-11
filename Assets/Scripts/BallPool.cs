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
    private HashSet<GameObject> ActivePool;
    private HashSet<GameObject> InactivePool;
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

    void Ball_OnSplit(object sender, Collision2D e)
    {
        GameObject split = GetInactiveBall();
        if (split != null)
        {
            SetBallActive(split);
            split.GetComponent<Rigidbody2D>().position = e.otherRigidbody.position;
            split.GetComponent<Rigidbody2D>().velocity = e.otherRigidbody.velocity + new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f)).normalized;
        }
        OnGlobalSplit?.Invoke(null, gameObject);
    }

    void Ball_OnSplitFail(object sender, Collision2D e)
    {
        SetBallInactive(e.otherRigidbody.gameObject);
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
            SetBallInactive(ActivePool.ElementAt(0));
        }

        yield return new WaitForSeconds(1);

        ResetAllBallParams();
        GameObject ball = GetInactiveBall();
        ball.transform.position = _ballPrefab.transform.position;
        SetBallActive(ball);
        ball.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));

        OnResetSimulation?.Invoke(null, gameObject);
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
        InactivePool.Remove(_ballPrefab);
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

    private void SetBallActive(GameObject ball)
    {
        ball.SetActive(true);
        ActivePool.Add(ball);
        InactivePool.Remove(ball);
    }

    private void SetBallInactive(GameObject ball)
    {
        ball.SetActive(false);
        InactivePool.Add(ball);
        ActivePool.Remove(ball);
    }

    /// <summary>
    ///  Activates an available inactive ball or creates new one. Returns null if ball limit has been reached.
    /// </summary>
    public GameObject GetInactiveBall()
    {
        if (ActiveBallCount < _ballAmountLimit)
        {
            if (InactiveBallCount > 0)
            {
                return InactivePool.ElementAt(0);
            }
            else 
            {
                return MakeNewBall();
            }
        }

        return null;
    }
}