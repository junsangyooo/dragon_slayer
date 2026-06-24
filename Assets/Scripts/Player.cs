using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    // 하위 변수들은 플레이어 세팅을 위한 컴포넌트 가져오는 변수들
    [SerializeField]
    private VariableJoystick variableJoystick;
    private Animator anim;
    private Rigidbody2D rb;
    private WeaponsAndBuffs wab;
    private Vector3 ActiveDir;

    // 게임 내에서 필요한 플레이어 세팅값
    // (Inspector에서 조정 가능하도록 SerializeField + 코드 기본값을 함께 둔다. 기본값이 0이면 데미지가 0이 되어 적을 못 죽인다.)
    [SerializeField]
    private float max_health = 100f;
    [SerializeField]
    private float attack_speed = 2f;            // 초당 공격 횟수
    public float player_move_speed = 5f;
    [SerializeField]
    private float magnetic_radius = 1f;
    [SerializeField]
    private float critical_rate = 0.2f;         // 0~1 확률 (0.2 = 20%)
    [SerializeField]
    private float critical_damage = 2f;         // 크리티컬 시 데미지 배수
    [SerializeField]
    private float weapon_move_speed_rate = 1f;
    [SerializeField]
    private float weapon_damage = 2f;           // 데모 기본값. 적 체력 대비 플레이테스트로 튜닝 필요.
    private float invincibility_time;

    // 기본 값
    private float current_health;
    private int current_exp;
    private int max_exp;
    private int level = 1;
    private bool isDead = false;
    private bool gettingDamage;
    private bool facingRight;
    private bool isDashing;
    private bool dashAvailable;
    private float dashingPower = 9f;
    private float dashingTime = 0.25f;
    private float dashingCooldown = 2.5f;

    public void setDashAvailable(bool value) {
        dashAvailable = value;
    }

    
    private void Awake() {
        Instance = this;
    }
    private void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        invincibility_time = 0.2f;
        max_exp = 5;
        wab = GetComponent<WeaponsAndBuffs>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        gettingDamage = false;
        facingRight = true;
        isDashing = false;
        dashAvailable = true;
        current_health = max_health;
        wab.setAttackSpeed(attack_speed);
        wab.setCriticalDamage(critical_damage);
        wab.setCriticalRate(critical_rate);
        wab.setMagneticRadius(magnetic_radius);
        wab.setMaxHealth(max_health);
        wab.setPlayerMoveSpeed(player_move_speed);
        wab.setWeaponDamage(weapon_damage);
        wab.setWeaponMoveSpeedRate(weapon_move_speed_rate);
        wab.setInvincibilityTime(invincibility_time);
        ActiveDir = new Vector3(0f, 1f, 0f);
    }

    private void Update() {
        anim.SetBool("dashing", isDashing);
        MovePlayer();
        PullPickups();
    }

    public Vector3 getActiveDir() {return ActiveDir;}
    public int getCurrentExp() {return current_exp;}
    public int getMaxExp() {return max_exp;}
    public float getCurrentHP() {return current_health;}
    public float getMaxHP() {return max_health;}
    public float calculateWeaponDamage() {
        // critical_rate(0~1 확률)만큼 크리티컬. Random.value는 [0,1) 범위.
        if (Random.value <= critical_rate) {
            return weapon_damage * critical_damage;
        } else {return weapon_damage;}
    }
    public Transform getPlayerTransform() {
        return transform;
    }

    public float getAttackSpeed() {return attack_speed;}
    public float getWeaponMoveSpeedRate() {return weapon_move_speed_rate;}
    public float getMagneticRadius() {return magnetic_radius;}
    public int getLevel() {return level;}

    // ----- 업그레이드(버프) 적용: UpgradeManager가 호출 -----
    public void BuffMaxHealth(float addFlat) {
        max_health += addFlat;
        current_health += addFlat;
        if (GameManager.Instance != null) GameManager.Instance.UpdateHP();
    }
    public void BuffMoveSpeed(float pct) { player_move_speed *= (1f + pct); }
    public void BuffAttackSpeed(float pct) {
        attack_speed *= (1f + pct);
        if (wab != null) wab.setAttackSpeed(attack_speed);
    }
    public void BuffWeaponDamage(float pct) { weapon_damage *= (1f + pct); }
    public void BuffCritRate(float addFlat) { critical_rate = Mathf.Min(1f, critical_rate + addFlat); }
    public void BuffCritDamage(float addFlat) { critical_damage += addFlat; }
    public void BuffMagneticRadius(float pct) { magnetic_radius *= (1f + pct); }
    public void BuffWeaponMoveSpeed(float pct) { weapon_move_speed_rate *= (1f + pct); }
    public void BuffInvincibilityTime(float pct) { invincibility_time *= (1f + pct); }

    // magnetic_radius 안의 EXP/Gold를 플레이어 쪽으로 끌어당긴다.
    private void PullPickups() {
        if (magnetic_radius <= 0f) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, magnetic_radius);
        for (int i = 0; i < hits.Length; i++) {
            Collider2D c = hits[i];
            if (c == null) continue;
            string tg = c.gameObject.tag;
            if (tg == "EXP" || tg == "Gold") {
                c.transform.position = Vector3.MoveTowards(
                    c.transform.position, transform.position, 9f * Time.deltaTime);
            }
        }
    }

    private void MovePlayer() {
        if (!isDashing) {
            float toX = variableJoystick.Horizontal;
            float toY = variableJoystick.Vertical;
            Vector3 direction = new Vector3(toX, toY, 0f);
            direction.Normalize();
            anim.SetBool("IsRunning", direction != Vector3.zero);
            if (toX < 0 && facingRight) {
                FlipPlayer();
            } else if (toX > 0 && !facingRight) {
                FlipPlayer();
            }
            if (direction != Vector3.zero) {ActiveDir = direction;}
            if ((transform.position.x <= -11.2f && direction.x < 0) || (transform.position.x >= 11.7f && direction.x > 0)) {
                direction.x = 0f;
            }
            if ((transform.position.y <= -10.4f && direction.y < 0) || (transform.position.y >= 9.9f && direction.y > 0)) {
                direction.y = 0f;
            }
            transform.position += direction * player_move_speed * Time.deltaTime;
        }
    }

    void FlipPlayer() {
        facingRight = !facingRight;
        Vector3 tempLocalScale = transform.localScale;
        tempLocalScale.x *= -1; 
        transform.localScale = tempLocalScale;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (isDead) return; // 사망 후에는 데미지·아이템 획득 처리 안 함
        if (other.gameObject.tag == "Enemy_Bat") {
            if (!gettingDamage) {
                current_health -= other.GetComponent<EnemyBat>().damage;
                WhenPlayerDamaged();
            }
        }
        if (other.gameObject.tag == "Enemy_Spider") {
            if (!gettingDamage) {
                current_health -= other.GetComponent<EnemySpider>().damage;
                WhenPlayerDamaged();
            }
        }
        if (other.gameObject.tag == "Enemy_Slime") {
            if (!gettingDamage) {
                current_health -= other.GetComponent<EnemySlime>().damage;
                WhenPlayerDamaged();
            }
        }
        if (other.gameObject.tag == "Enemy_OneEye") {
            if (!gettingDamage) {
                current_health -= other.GetComponent<EnemyOneEye>().damage;
                WhenPlayerDamaged();
            }
        }
        if (other.gameObject.tag == "Enemy_Boss") {
            if (!gettingDamage) {
                current_health -= other.GetComponent<EnemyBoss>().damage;
                WhenPlayerDamaged();
            }
        }
        if (other.gameObject.tag == "EXP") {
            current_exp += other.GetComponent<EXP>().exp_amount;
            while (current_exp >= max_exp) {
                LevelUp();
            }
            GameManager.Instance.UpdateEXP();
            Destroy(other.gameObject);
        }
        if (other.gameObject.tag == "Gold") {
            GameManager.Instance.AddGold();
            Destroy(other.gameObject);
        }
    }

    private void WhenPlayerDamaged() {
        GameManager.Instance.UpdateHP();
        if (current_health <= 0) {
            PlayerDie();
        }
        Debug.Log(current_health);
        StartCoroutine("OnDamage");
    }

    IEnumerator OnDamage() {
        gettingDamage = true;
        yield return new WaitForSeconds(invincibility_time);
        gettingDamage = false;
    }

    public void PlayerDie() {
        if (isDead) return;
        isDead = true;
        GameManager.Instance.GameOver();
    }

    // EXP가 max_exp에 도달하면 레벨업: 넘친 EXP는 이월하고 다음 요구치를 올린다.
    // (실제 업그레이드 카드 선택은 UpgradeManager.QueueLevelUp이 처리한다.)
    private void LevelUp() {
        level++;
        current_exp -= max_exp;
        if (current_exp < 0) current_exp = 0;
        max_exp = Mathf.RoundToInt(max_exp * 1.3f) + 1;
        if (UpgradeManager.Instance != null) {
            UpgradeManager.Instance.QueueLevelUp();
        }
    }


    public void DashClicked() {
        if (dashAvailable) {
            StartCoroutine("Dash");
        }
    }
    IEnumerator Dash() {
        dashAvailable = false;
        isDashing = true;
        anim.SetTrigger("dash");
        float startTime = Time.time;
        while (Time.time < startTime + dashingTime) {
            transform.position += ActiveDir * dashingPower * Time.deltaTime;
            yield return null;
        }
        isDashing = false;
        startTime = Time.time;
        while (Time.time - startTime < dashingCooldown) {
            GameManager.Instance.UpdateDashCool(Time.time - startTime);
            yield return null;
        }
        dashAvailable = true;
    }
}
