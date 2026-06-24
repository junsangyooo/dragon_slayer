using UnityEngine;

// 플레이어를 따라다니는 지속 피해 오라. 일정 간격으로 반경 안의 적에게 피해를 준다.
// WeaponsAndBuffs가 런타임에 생성하고 radius/damage를 레벨에 맞춰 갱신한다.
public class FireBarrier : MonoBehaviour
{
    public Transform target;
    public float radius = 1.5f;
    public float damage = 2f;
    public float tickInterval = 0.5f;

    private float timer;

    private void Update()
    {
        if (target != null) transform.position = target.position;
        // 원 스프라이트는 반경 80%까지만 불투명하므로, 보이는 오라가 실제 피해 반경과 맞도록 1/0.8 보정.
        float d = radius * 2.5f;
        transform.localScale = new Vector3(d, d, 1f);

        if (GameManager.Instance == null || !GameManager.Instance.GetPlaying()) return;

        timer += Time.deltaTime;
        if (timer >= tickInterval)
        {
            timer = 0f;
            WeaponVisuals.DamageInRadius(transform.position, radius, damage);
        }
    }
}
