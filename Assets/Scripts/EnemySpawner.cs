using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] walls;

    private const float maxSpawnY = 15f;
    private const float minSpawnY = -15f;
    private const float maxSpawnX = 15f;
    private const float minSpawnX = -15f;

    [SerializeField]
    private GameObject[] bats;

    [SerializeField]
    private GameObject[] spiders;

    [SerializeField]
    private GameObject[] slimes;

    [SerializeField]
    private GameObject[] OneEyes;

    [SerializeField]
    private GameObject[] bosseEnemies;

    private float countTime;
    private int level;
    private float reduceSpawnTime;
    private const float bossTime = 180f; // 보스 등장 시간(초). 이 시간까지 생존하면 보스 출현.
    private bool bossSpawned = false;
    private bool bossActive = false;

    private Vector3[] DiagonalPosition = {new Vector3(maxSpawnX, maxSpawnY, 0f), 
                                                new Vector3(maxSpawnX, minSpawnY, 0f), 
                                                new Vector3(minSpawnX, maxSpawnY, 0f), 
                                                new Vector3(minSpawnX, minSpawnY, 0f)};
    private Vector3[] HoriVertiPosition = {new Vector3(maxSpawnX, 0f, 0f),
                                                new Vector3(minSpawnX, 0f, 0f),
                                                new Vector3(0f, maxSpawnY, 0f),
                                                new Vector3(0f, minSpawnY, 0f),};
    private Quaternion SpawnRotation;

    private void Start() {
        countTime = 0;
        level = 0;
        reduceSpawnTime = 0;
        SpawnRotation = Quaternion.identity;
    }

    public void StartSpawning() {
        SpawnBats();
        StartCoroutine("EnemySpawn");
    }

    IEnumerator EnemySpawn() {
        while (GameManager.Instance.GetPlaying()) {
            yield return new WaitForSeconds(1f);
            countTime += 1;
            // 생존 목표 시간 도달 -> 보스 1회 등장, 이후 일반 적 스폰 중단
            if (!bossSpawned && countTime >= bossTime) {
                SpawnBoss();
                bossSpawned = true;
                bossActive = true;
            }
            if (bossActive) continue;
            // 45초마다 난이도 상승: 3분(보스 전)까지 tier 0->1->2->3 으로 적이 강해지고 스폰도 빨라진다.
            if (countTime % 45 == 0 && level < 3)  {
                level++;
                reduceSpawnTime = level * 2;
            }
            if (countTime % (11 - reduceSpawnTime) == 0) SpawnBats();
            if (countTime % (17 - reduceSpawnTime) == 0) SpawnSpiders();
            if (countTime % (23 - reduceSpawnTime) == 0) SpawnSlimes();
            if (countTime % (37 - reduceSpawnTime) == 0) {
                SpawnOneEye();
            }
        }
    }

    private void SpawnBats() {
        foreach(Vector3 pos in DiagonalPosition) {
            GameObject bat = Instantiate(bats[level], pos, SpawnRotation);
        }
    }

    private void SpawnSpiders() {
        foreach(Vector3 pos in HoriVertiPosition) {
            Instantiate(spiders[level], pos, SpawnRotation);
        }
    }

    private void SpawnSlimes() {
        foreach(Vector3 pos in HoriVertiPosition) {
            Instantiate(slimes[level], pos, SpawnRotation);
            Instantiate(slimes[level], pos, SpawnRotation);
            Instantiate(slimes[level], pos, SpawnRotation);
            Instantiate(slimes[level], pos, SpawnRotation);
            Instantiate(slimes[level], pos, SpawnRotation);
        }
    }

    private void SpawnOneEye() {
        int random_index = Random.Range(0, 4);
        Instantiate(OneEyes[level], DiagonalPosition[random_index], SpawnRotation);
    }

    private void SpawnBoss() {
        if (bosseEnemies == null || bosseEnemies.Length == 0) return;
        GameObject prefab = bosseEnemies[0];
        if (prefab == null) return;

        GameObject boss = Instantiate(prefab, new Vector3(0f, 8f, 0f), SpawnRotation);

        // 이 프리팹은 3D BoxCollider라 2D 물리와 상호작용하지 않는다 -> 제거 후 2D 트리거로 교체.
        BoxCollider old3d = boss.GetComponent<BoxCollider>();
        if (old3d != null) Destroy(old3d);
        BoxCollider2D col = boss.GetComponent<BoxCollider2D>();
        if (col == null) col = boss.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.39f, 0.56f);
        col.offset = new Vector2(0.36f, -0.18f);

        // 트리거 콜백이 확실히 발동하도록 Rigidbody2D 추가(다른 적과 동일 패턴).
        Rigidbody2D rb = boss.GetComponent<Rigidbody2D>();
        if (rb == null) rb = boss.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        boss.tag = "Enemy_Boss";
        if (boss.GetComponent<EnemyBoss>() == null) boss.AddComponent<EnemyBoss>();
    }
}
