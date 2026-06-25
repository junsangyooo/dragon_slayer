using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsAndBuffs : MonoBehaviour
{
    // 무기 사용을 위해 필요한 변수들
    private Vector3 direction;
    [SerializeField]
    private VariableJoystick variableJoystick;

    // 무기 종류들
    // 기본 공격 (Fireball)
    [SerializeField]
    private GameObject[] basic_attack;
    private int[] basic_attack_damages;
    public int basic_attack_level;

    // 파이어 배리어 (일정 반경 안의 적 지속적 데미지)
    [SerializeField]
    private GameObject[] fire_barrier;
    public int fire_barrier_level;

    // 블랙홀 (발사 후 처음 맞는 상대 주위에 블랙홀 생성, 주위의 상대편 이동 불가 상태 부여)
    [SerializeField]
    private GameObject[] black_hole;
    public int black_hole_level;

    // 천둥번개 (플레이어 주위에 일정 간격동안 번개 내려침)
    [SerializeField]
    private GameObject[] thunder;
    public int thunder_level;

    // 뿔로 검기 발사 (초승달 모양의 검기를 방출하여 일정 방향으로 움직임)
    [SerializeField]
    private GameObject[] horn_wave;
    public int horn_wave_level;

    // 버프 종류들
    // 플레이어 버프
    private int attack_speed_rate_buff_level;
    private int max_health_buff_level;
    private int player_move_speed_buff_level;
    private int magnetic_raius_buff_level;
    private int critical_rate_buff_level;
    private int critical_damage_buff_level;
    private int weapon_move_speed_rate_buff_level;
    private int weapon_damage_buff_level;
    private int invincibility_time_buff_level;


    // 능력치들
    // 플레이어 능력치
    private float attack_speed;
    private float max_health;
    private float player_move_speed;
    private float magnetic_radius;
    private float critical_rate;
    private float critical_damage;
    private float weapon_move_speed_rate;
    private float weapon_damage;
    private float invincibility_time;

    public void setAttackSpeed (float speed) {
        attack_speed = speed;
    }
    public void setMaxHealth(float hp) {
        max_health = hp;
    }
    public void setPlayerMoveSpeed(float speed) {
        player_move_speed = speed;
    }
    public void setMagneticRadius(float radius) {
        magnetic_radius = radius;
    }
    public void setCriticalRate(float rate) {
        critical_rate = rate;
    }
    public void setCriticalDamage(float damage) {
        critical_damage = damage;
    }
    public void setWeaponMoveSpeedRate(float rate) {
        weapon_move_speed_rate = rate;
    }
    public void setWeaponDamage(float rate) {
        weapon_damage = rate;
    }
    public void setInvincibilityTime(float time) {
        invincibility_time = time;
    }

    // 파이어볼 강화: 발사체 개수 +1 (부채꼴로 더 많이 발사)
    public void UpgradeFireball() {
        basic_attack_level++;
    }

    // ===================== 추가 무기 (레벨업 카드로 획득·강화) =====================
    private FireBarrier fireBarrierInstance;

    // --- Fire Barrier: 플레이어 주위 지속 피해 오라 ---
    public void UpgradeFireBarrier() {
        fire_barrier_level++;
        if (fireBarrierInstance == null) {
            GameObject go = WeaponVisuals.SpawnSprite(WeaponVisuals.Fx("firebarrier"), transform.position, 3f, -2);
            fireBarrierInstance = go.AddComponent<FireBarrier>();
            fireBarrierInstance.target = transform;
        }
        fireBarrierInstance.radius = 1.4f + 0.35f * fire_barrier_level;
        fireBarrierInstance.damage = 1f + fire_barrier_level;
    }

    // --- Thunder: 주기적으로 적에게 낙뢰 ---
    public void UpgradeThunder() {
        thunder_level++;
        if (thunder_level == 1) StartCoroutine(ThunderLoop());
    }
    private IEnumerator ThunderLoop() {
        while (true) {
            float interval = Mathf.Max(0.6f, 1.8f - 0.12f * thunder_level);
            yield return new WaitForSeconds(interval);
            if (GameManager.Instance == null || !GameManager.Instance.GetPlaying() || thunder_level <= 0) continue;
            int strikes = Mathf.Max(1, thunder_level);
            float dmg = 3f + 2f * thunder_level;
            for (int i = 0; i < strikes; i++) {
                Vector3 pos = RandomTargetPos();
                GameObject fx = WeaponVisuals.SpawnSprite(WeaponVisuals.Fx("thunder"), pos, 2.4f, 5);
                fx.AddComponent<FadeAndDie>().life = 0.28f;
                WeaponVisuals.DamageInRadius(pos, 1.1f, dmg);
            }
        }
    }

    // --- Black Hole: 주기적으로 블랙홀 생성(끌어당김 + 지속 피해) ---
    public void UpgradeBlackHole() {
        black_hole_level++;
        if (black_hole_level == 1) StartCoroutine(BlackHoleLoop());
    }
    private IEnumerator BlackHoleLoop() {
        while (true) {
            float interval = Mathf.Max(3.5f, 6f - 0.3f * black_hole_level);
            yield return new WaitForSeconds(interval);
            if (GameManager.Instance == null || !GameManager.Instance.GetPlaying() || black_hole_level <= 0) continue;
            float radius = 2f + 0.3f * black_hole_level;
            Vector3 pos = RandomTargetPos();
            GameObject go = WeaponVisuals.SpawnSprite(WeaponVisuals.Fx("blackhole"), pos, radius * 2f, -1);
            BlackHole bh = go.AddComponent<BlackHole>();
            bh.radius = radius;
            bh.damage = 1f + black_hole_level;
            bh.lifetime = 2.2f + 0.3f * black_hole_level;
        }
    }

    // --- Horn Wave: 주기적으로 조준 방향에 관통 검기 발사 ---
    public void UpgradeHornWave() {
        horn_wave_level++;
        if (horn_wave_level == 1) StartCoroutine(HornWaveLoop());
    }
    private IEnumerator HornWaveLoop() {
        while (true) {
            float interval = Mathf.Max(1.2f, 2.5f - 0.12f * horn_wave_level);
            yield return new WaitForSeconds(interval);
            if (GameManager.Instance == null || !GameManager.Instance.GetPlaying() || horn_wave_level <= 0) continue;
            int waves = Mathf.Clamp(horn_wave_level, 1, 5);
            float dmg = 4f + 2f * horn_wave_level;
            Vector3 aim = GetAimDirection();
            const float spread = 18f;
            float start = -(waves - 1) * spread * 0.5f;
            for (int i = 0; i < waves; i++) {
                Vector3 dir = Quaternion.Euler(0f, 0f, start + i * spread) * aim;
                SpawnHornWave(dir, dmg);
            }
        }
    }
    private void SpawnHornWave(Vector3 dir, float dmg) {
        GameObject go = new GameObject("HornWave");
        go.transform.position = transform.position;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        go.transform.rotation = Quaternion.Euler(0f, 0f, ang);
        go.transform.localScale = new Vector3(1.6f, 1.6f, 1f); // 초승달 검기 스프라이트
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = WeaponVisuals.Fx("hornwave");
        sr.sortingOrder = 4;
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = Vector2.one;
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        DamageProjectile p = go.AddComponent<DamageProjectile>();
        p.dir = dir.normalized;
        p.moveSpeed = 7f;
        p.damage = dmg;
        p.lifetime = 1.3f;
    }

    // 살아있는 적 중 무작위 하나의 위치(없으면 플레이어 주변 무작위 지점)
    private Vector3 RandomTargetPos() {
        GameObject e = RandomEnemy();
        if (e != null) return e.transform.position;
        Vector2 off = Random.insideUnitCircle * 4f;
        return transform.position + new Vector3(off.x, off.y, 0f);
    }
    private GameObject RandomEnemy() {
        if (GameManager.Instance == null) return null;
        List<GameObject> list = GameManager.Instance.enemies;
        List<GameObject> alive = new List<GameObject>();
        for (int i = 0; i < list.Count; i++) {
            if (list[i] != null) alive.Add(list[i]);
        }
        if (alive.Count == 0) return null;
        return alive[Random.Range(0, alive.Count)];
    }

    private void Start() {
        direction = new Vector3(0f, 1f, 0f);
        if (basic_attack_level < 1) basic_attack_level = 1; // 시작부터 파이어볼 1발
        StartCoroutine(BasicAttackLoop());
    }

    private void Update() {
        if (Player.Instance != null) {
            direction = Player.Instance.getActiveDir();
        }
    }

    // 일정 간격(공격 속도)으로 기본 공격을 자동 발사한다. (뱀파이어 서바이버즈식 오토 배틀)
    private IEnumerator BasicAttackLoop() {
        while (true) {
            if (GameManager.Instance != null && GameManager.Instance.GetPlaying()) {
                FireBasicAttack();
            }
            float interval = attack_speed > 0f ? 1f / attack_speed : 0.5f;
            yield return new WaitForSeconds(interval);
        }
    }

    // basic_attack_level 만큼의 발사체를 부채꼴로 생성해 가장 가까운 적 방향으로 쏜다.
    private void FireBasicAttack() {
        if (basic_attack == null || basic_attack.Length == 0) return;
        GameObject prefab = basic_attack[0]; // 발사체 프리팹은 0번 (레벨은 발사 개수로 사용)
        if (prefab == null) return;

        Vector3 aimDir = GetAimDirection();
        int count = Mathf.Max(1, basic_attack_level);
        const float spread = 14f; // 발사체 사이 각도(도)
        float startAngle = -(count - 1) * spread * 0.5f;
        float speedRate = (Player.Instance != null) ? Player.Instance.getWeaponMoveSpeedRate() : 1f;

        for (int i = 0; i < count; i++) {
            float ang = startAngle + i * spread;
            Vector3 dir = Quaternion.Euler(0f, 0f, ang) * aimDir;
            GameObject attack = Instantiate(prefab, transform.position, Quaternion.identity);
            BasicAttack b_attack = attack.GetComponent<BasicAttack>();
            if (b_attack != null) {
                b_attack.SetDirection(dir);
                if (speedRate > 0f) {
                    b_attack.setMoveSpeed(b_attack.getMoveSpeed() * speedRate);
                }
            }
        }
    }

    // 살아있는 적 중 가장 가까운 적 방향을 반환. 적이 없으면 현재 바라보는 방향으로 발사.
    private Vector3 GetAimDirection() {
        Vector3 origin = transform.position;
        GameObject nearest = null;
        float best = Mathf.Infinity;
        if (GameManager.Instance != null) {
            List<GameObject> enemies = GameManager.Instance.enemies;
            foreach (GameObject enemy in enemies) {
                if (enemy == null) continue;
                float sqr = (enemy.transform.position - origin).sqrMagnitude;
                if (sqr < best) {
                    best = sqr;
                    nearest = enemy;
                }
            }
        }
        if (nearest != null) {
            return (nearest.transform.position - origin).normalized;
        }
        return direction == Vector3.zero ? new Vector3(0f, 1f, 0f) : direction.normalized;
    }
}
