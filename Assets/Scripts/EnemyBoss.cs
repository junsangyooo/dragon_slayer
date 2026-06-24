using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 보스(GhostBoss). 생존 목표 시간이 지나면 EnemySpawner가 스폰한다.
// 일반 적처럼 플레이어를 추격하고 기본 공격에 피격되며, 처치되면 GameManager.Win()을 호출한다.
// 이 프리팹에는 스크립트가 없어서 EnemySpawner가 런타임에 AddComponent로 붙인다(필드는 코드 기본값 사용).
public class EnemyBoss : MonoBehaviour, IDamageable
{
    private float hp;
    [SerializeField] private float max_hp = 250f; // 플레이테스트로 튜닝
    public float damage = 20f;                     // 플레이어 접촉 데미지 (Player가 읽음)
    private float speed = 1.5f;

    private Transform player;

    void Start()
    {
        hp = max_hp;
        GameObject pgo = GameObject.Find("Player");
        if (pgo != null) player = pgo.transform;
        // 플레이어 자동 조준이 보스를 노릴 수 있도록 적 목록에 등록
        if (GameManager.Instance != null) GameManager.Instance.enemies.Add(this.gameObject);
    }

    private void FixedUpdate()
    {
        if (player != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "basicAttack")
        {
            if (Player.Instance == null) return;
            TakeDamage(Player.Instance.calculateWeaponDamage());
        }
    }

    public void TakeDamage(float dmg)
    {
        if (hp <= 0f) return;
        hp -= dmg;
        if (hp <= 0f) Die();
    }

    private void Die()
    {
        // 보스는 사망음 대신 승리 팡파레(Win->PlayVictory)로 처리해 사운드 중첩을 피한다.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.enemies.Remove(this.gameObject);
            GameManager.Instance.Win();
        }
        Destroy(gameObject);
    }

    public float GetHpFraction()
    {
        return max_hp > 0f ? Mathf.Clamp01(hp / max_hp) : 0f;
    }
}
