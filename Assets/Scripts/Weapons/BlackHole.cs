using UnityEngine;

// 일정 시간 동안 반경 안의 적을 중심으로 끌어당기며 주기적으로 피해를 주고, 수명 후 사라진다.
// WeaponsAndBuffs가 런타임에 생성한다.
public class BlackHole : MonoBehaviour
{
    public float radius = 2.5f;
    public float damage = 2f;
    public float pullSpeed = 3f;
    public float tickInterval = 0.4f;
    public float lifetime = 2.5f;

    private float timer;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.GetPlaying()) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;
            // IDamageable(적)만 끌어당긴다. 플레이어/벽/픽업은 제외.
            if (hits[i].GetComponent<IDamageable>() == null) continue;
            hits[i].transform.position = Vector3.MoveTowards(
                hits[i].transform.position, transform.position, pullSpeed * Time.deltaTime);
        }

        timer += Time.deltaTime;
        if (timer >= tickInterval)
        {
            timer = 0f;
            WeaponVisuals.DamageInRadius(transform.position, radius, damage);
        }
    }
}
