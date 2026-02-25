using System;
using System.Collections;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;

    public Action OnHit;
    public Action<int> OnDie;
    public Action<float, float> OnHealthChanged;

    protected bool IsAlive;

    public float hitPointsCurrent;
    public float hitPoints = 100f;

    private float _dieTime = 6f;
    private int _coinsPerKill;

    private PooledObject _pooled;

    private void Awake()
    {
        _pooled = GetComponent<PooledObject>();
    }

    protected void DestructibleInit(float health, float dieTime, int coinsPerKill)
    {
        IsAlive = true;

        hitPointsCurrent = hitPoints = health;
        _dieTime = dieTime;
        _coinsPerKill = coinsPerKill;

        OnHealthChanged?.Invoke(hitPointsCurrent, hitPoints);
    }

    public void Hit(float damage)
    {
        if (!IsAlive)
            return;

        hitPointsCurrent -= damage;
        if (hitPointsCurrent < 0f)
            hitPointsCurrent = 0f;

        OnHealthChanged?.Invoke(hitPointsCurrent, hitPoints);
        OnHit?.Invoke();

        if (hitPointsCurrent <= 0f)
            StartCoroutine(Die(_dieTime));
    }

    protected IEnumerator Die(float delay)
    {
        if (!IsAlive)
            yield break;

        IsAlive = false;

        OnDie?.Invoke(_coinsPerKill);

        yield return new WaitForSeconds(delay);

        if (explosionPrefab != null)
            Explosion.Create(transform.position, explosionPrefab);

        if (_pooled != null)
        {
            _pooled.Release();
            yield break;
        }

        Destroy(gameObject);
    }
}