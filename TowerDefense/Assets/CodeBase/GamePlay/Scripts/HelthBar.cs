using UnityEngine;
using UnityEngine.UI;

public class HelthBar : MonoBehaviour
{
    public Image healthBar;

    public bool rotateBar = true;
    [SerializeField] private Destructible owner;

    private void OnEnable()
    {
        if (healthBar == null)
            healthBar = gameObject.GetComponent<Image>();

        if (owner != null)
        {
            owner.OnHit -= UpdateBar;
            owner.OnHit += UpdateBar;
            UpdateBar();
        }
    }

    private void OnDisable()
    {
        if (owner != null)
            owner.OnHit -= UpdateBar;
    }

    private void UpdateBar()
    {
        if (owner == null || healthBar == null)
            return;

        healthBar.fillAmount = Mathf.InverseLerp(0.0f, owner.hitPoints, owner.hitPointsCurrent);
    }

    private void Update()
    {
        if (rotateBar && Camera.main != null)
            transform.forward = Camera.main.transform.forward;
    }
}