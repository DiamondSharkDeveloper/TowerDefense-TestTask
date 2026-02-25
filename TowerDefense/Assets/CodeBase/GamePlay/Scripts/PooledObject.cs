using System;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    private Action<GameObject> _returnToPool;

    public void SetReturnAction(Action<GameObject> returnToPool) =>
        _returnToPool = returnToPool;

    public void Release()
    {
        if (_returnToPool != null)
            _returnToPool.Invoke(gameObject);
        else
            gameObject.SetActive(false);
    }
}