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
