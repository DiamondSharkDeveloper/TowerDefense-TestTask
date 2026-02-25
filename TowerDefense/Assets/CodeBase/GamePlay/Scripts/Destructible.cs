using System;
using System.Collections;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;

    public Action OnHit;
    public Action<int> OnDie;

    protected bool IsAlive;
    private float _dieTime = 6;

    public float hitPointsCurrent;
    public float hitPoints = 100f;

    private int _coinsPerKill;

    protected void DestructibleInit(float health, float dieTime, int coinsPerKill)
    {
        IsAlive = true;
        hitPointsCurrent = hitPoints = health;
        _dieTime = dieTime;
        _coinsPerKill = coinsPerKill;
    }

    public void Hit(float damage)
    {
        if (!IsAlive)
            return;

        hitPointsCurrent -= damage;

        if (hitPointsCurrent <= 0)
            StartCoroutine(Die(_dieTime));

        OnHit?.Invoke();
    }

    protected IEnumerator Die(float delay)
    {
        IsAlive = false;
        OnDie?.Invoke(_coinsPerKill);

        if (delay > 0)
            yield return new WaitForSeconds(delay);

        if (explosionPrefab != null)
            Explosion.Create(transform.position, explosionPrefab);

        
        PooledObject pooled = GetComponent<PooledObject>();
        if (pooled != null)
        {
            pooled.Release();
            yield break;
        }

        
        Destroy(gameObject);
    }
}