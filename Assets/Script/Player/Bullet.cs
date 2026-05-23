using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Tooltip("Damage dealt by this bullet")]
    public int damage = 10;

    [Tooltip("Auto-destroy after seconds")]
    public float lifeTime = 5f;

    private void Start()
    {
        if (lifeTime > 0f)
            Destroy(gameObject, lifeTime);
    }
}