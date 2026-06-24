using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBat : MonoBehaviour, IDamageable
{
    // Enemy 체력 및 공격력
    private float hp;
    
    [SerializeField]
    private float max_hp;
    // {2f, 10f, 20f, 30f}
    public float damage;
    private const float speed = 1f;

    // player을 따라가기 위해 필요한 변수들
    private Transform player;
    private float distance;

    // 에너지 드랍을 위한 에너지 Prefabs 리스트
    [SerializeField]
    private GameObject[] energies;
    private int level;

    // 골드 드랍을 위한 골드 Prefab
    [SerializeField]
    private GameObject gold;

    // 다른 적들과 겹치는것을 방지하기 위한 변수들
    private float avoidanceRadius = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").transform;
        hp = max_hp;
        GameManager.Instance.enemies.Add(this.gameObject);
    }

    private void FixedUpdate() {
        if (player)
        {
            ChasePlayer();
        }
    }

    private void ChasePlayer() {
        distance = Vector2.Distance(transform.position, player.transform.position);
        Vector2 direction = (player.transform.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(this.transform.position, player.transform.position, speed * Time.deltaTime);
        
        List<GameObject> enemies = GameManager.Instance.enemies;
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && enemy != gameObject) 
            {
                float currentDistance = Vector2.Distance(transform.position, enemy.transform.position);
                if (currentDistance < avoidanceRadius)
                {
                    Vector3 dist = transform.position - enemy.transform.position;
                    transform.position += dist * Time.deltaTime;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "basicAttack") {
            if (Player.Instance == null) return;
            TakeDamage(Player.Instance.calculateWeaponDamage());
        }
    }

    public void TakeDamage(float dmg) {
        if (hp <= 0f) return;
        hp -= dmg;
        if (hp <= 0f) Die();
    }

    private void Die() {
        AudioManager.Instance.PlayDeath();
        // Dropping Energy
        int playTime = GameManager.Instance.getPlayTime();
        if (playTime < 60) level = 0;
        else if (playTime < 180) level = 1;
        else if (playTime < 360) level = 2;
        else if (playTime < 540) level = 3;
        else if (playTime < 720) level = 4;
        else {
            if (Random.Range(0,3) == 0) {
                Instantiate(energies[4], transform.position, Quaternion.identity);
            }
        }
        int luckyNum = Random.Range(0,5);
        if (luckyNum == 0) {
            Instantiate(energies[luckyNum], transform.position, Quaternion.identity);
        } else if (luckyNum == 1 && level > 0) {
            Instantiate(energies[luckyNum - 1], transform.position, Quaternion.identity);
        }

        // Dropping Gold
        if (Random.Range(0, 7) == 0) {
            Instantiate(gold, transform.position, Quaternion.identity);
        }

        // Delete this enemy
        GameManager.Instance.enemies.Remove(gameObject);
        Destroy(gameObject);
    }
}
