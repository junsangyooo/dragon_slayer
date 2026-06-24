using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttack : MonoBehaviour
{
    public  float moveSpeed;
    public Vector3 dir;

    // 화면 밖으로 날아간 발사체가 무한히 쌓이지 않도록 일정 시간 뒤 자동 소멸.
    [SerializeField]
    private float lifetime = 3f;

    private void Start() {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector3 dir) {
        this.dir = dir.normalized;
        setRotation();
    }

    // 발사체 스프라이트가 진행 방향을 바라보도록 회전.
    private void setRotation() {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public float getMoveSpeed() {
        return moveSpeed;
    }

    public void setMoveSpeed(float newSpeed) {
        moveSpeed = newSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += dir * moveSpeed * Time.deltaTime;
    }
}
