using UnityEngine;

// 일정 방향으로 이동하며 닿는 적(IDamageable)에 피해를 주고, 수명이 다하면 사라지는 범용 발사체(관통).
// Horn Wave(검기) 등에 런타임으로 부착해 사용한다.
public class DamageProjectile : MonoBehaviour
{
    public Vector3 dir = Vector3.up;
    public float moveSpeed = 6f;
    public float damage = 5f;
    public float lifetime = 1.5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable d = other.GetComponent<IDamageable>();
        if (d != null) d.TakeDamage(damage);
    }
}
