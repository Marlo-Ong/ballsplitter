using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomsUtil : MonoBehaviour
{
    public static RandomsUtil Instance;
    private ulong[] _seed = new ulong[4];
    
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
        for(int i = 0; i < 4; i++)
        {
            _seed[i] = (ulong)Mathf.Floor(Random.Range(0f, 1f) * Mathf.Pow(2, 64));
        }
    }

    /// <remarks>
    /// While .NET 6 uses xoshiro256** and xoshiro128** for their random libraries,
    /// Unity (2022.x) still uses the older Xorshift128. Here xoshiro256+ is
    /// being used for its speed for generating 64-bit floating point numbers.
    /// </remarks>
    public ulong Xoshiro256Plus()
    {
        static ulong rotl(ulong x, int k) { return (x << k) | (x >> (64 - k)); }

        ulong result = _seed[0] + _seed[3];
        ulong t = _seed[1] << 17;

        _seed[2] ^= _seed[0];
        _seed[3] ^= _seed[1];
        _seed[1] ^= _seed[2];
        _seed[0] ^= _seed[3];

        _seed[2] ^= t;

        _seed[3] = rotl(_seed[3], 45);

        return result;
    }
}
