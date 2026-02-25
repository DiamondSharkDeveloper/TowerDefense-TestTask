using CodeBase.GamePlay.Enemys;
using UnityEngine;

public class EnemyShooter : Enemy
{
    [SerializeField] private float shootDelay;
    [SerializeField] private Transform weapon;
    [SerializeField] private float bulletDamage = 10.0f;
    [SerializeField] private float shootPower = 300.0f;
    [SerializeField] private Projectile bulletPrefab;

    private bool _isInit;

    public Enemy Init(float speed, int coinsPerKill,
        float health,
        float attackPower,
        float attackDelay,
        float stopDistance,
        Destructible target,
        float dieTime = 6)
    {
        bulletDamage = attackPower;
        shootDelay = attackDelay;

        EnemyInit(speed, stopDistance, target);
        DestructibleInit(health, dieTime, coinsPerKill);

        if (!_isInit)
        {
            animationController.OnAttack += Shoot;
            _isInit = true;
        }

        CancelInvoke(nameof(TryShoot));
        InvokeRepeating(nameof(TryShoot), shootDelay, shootDelay);

        return this;
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(TryShoot));
    }

    private bool CheckTargetVisibility()
    {
        if (Target == null)
            return false;

        Vector3 targetDirection = Target.transform.position - transform.position;

        Ray ray = new Ray(transform.position, targetDirection);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
            return hit.transform == Target.transform;

        return false;
    }

    private void TryShoot()
    {
        if (IsAlive && CheckTargetVisibility() && !IsMove)
            animationController.PlayThrowAnimation();
    }

    private void Shoot()
    {
        if (Target == null || weapon == null || bulletPrefab == null)
            return;

        Vector3 targetDirection = Target.transform.position - weapon.transform.position;
        targetDirection.Normalize();

        Projectile newBullet = Instantiate(bulletPrefab, weapon.position, weapon.rotation);
        newBullet.Init(bulletDamage, gameObject, 0, 0);

        Rigidbody rb = newBullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(targetDirection * shootPower);

        Destroy(newBullet.gameObject, 10);
    }
}