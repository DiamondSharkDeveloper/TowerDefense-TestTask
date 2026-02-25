using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    private GameObject _owner;
    private float _damage = 10.0f;
    private float _radius;
    private float _explosionDamage;

    public void Init(float damage, GameObject owner, float radius, float explosionDamage)
    {
        _damage = damage;
        _owner = owner;
        _radius = radius;
        _explosionDamage = explosionDamage;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destructible target = collision.gameObject.GetComponent<Destructible>();
        if (target)
        {
            if (!Equals(collision.gameObject, _owner))
            {
                target.Hit(_damage);
            }
        }

        if (_radius > 0)
        {
            CauseExplosionDamage();
            if (explosionPrefab)
            {
                Explosion.Create(transform.position, explosionPrefab);
            }
        }

        Destroy(gameObject);
    }

    private void CauseExplosionDamage()
    {
        Collider[] explosionVictims = Physics.OverlapSphere(transform.position, _radius);
        for (int i = 0; i < explosionVictims.Length; i++)
        {
            Vector3 vectorToVictim = explosionVictims[i].transform.position - transform.position;
            float decay = 1 - (vectorToVictim.magnitude / _radius);
            Destructible currentVictim = explosionVictims[i].gameObject.GetComponent<Destructible>();
            if (currentVictim)
            {
                currentVictim.Hit(_explosionDamage * decay);
            }

            Rigidbody victimRigidbidy = explosionVictims[i].gameObject.GetComponent<Rigidbody>();
            if (victimRigidbidy)
            {
                victimRigidbidy.AddForce(vectorToVictim.normalized * decay * 1000);
            }
        }
    }
}