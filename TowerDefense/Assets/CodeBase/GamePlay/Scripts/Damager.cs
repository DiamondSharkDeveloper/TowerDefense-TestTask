using UnityEngine;
using UnityEngine.PlayerLoop;

public class Damager : MonoBehaviour
{
    
    private float _damage = 10.0f;
    protected Destructible Target;
    private bool _canAttack;
    

    public void Init(float damage)
    {
        _damage = damage;
       
    }
    public void Hit()
    {
        if (_canAttack)
        {
            Target.Hit(_damage);
            _canAttack = false;
        }
        Collider[] bladeVictims = Physics.OverlapSphere(transform.position, 5);
        for (int i = 0; i < bladeVictims.Length; i++)
        {
            if (Equals(bladeVictims[i].gameObject,Target))
            {
               Target.Hit(_damage);
            }
        }
        
    }
}
