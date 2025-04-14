using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    // 적 캐릭터 상태 enum 정의
    public enum EnemyState
    {
        Idle,
        Knockback,
        Dead
    }

    [Header("----- 컴포넌트 참조 -----")]
    public Transform GroundSensor;
    public Rigidbody2D Rigid;
    public Animator EnemyAnimator;
    public CapsuleCollider2D Collider;
    public Image HpBar;

    [Header("----- 이동 -----")]
    public float MoveSpeed;

    [Header("----- 공격 -----")]
    public float Damage;            // 공격 시 상대에게 입힐 대미지
    public float AttackMagnitude;   // 공격 시 상대에게 적용할 넉백 힘

    [Header("----- 피격 -----")]
    public float MaxHp;     // 최대 체력
    public float KnockbackDuration; // 넉백 지속 시간
    public float DeadDuration;  // 죽는 과정의 지속 시간
                                // (죽는 애니메이션이 재생 시작되고 게임오브젝트가 파괴되기 전까지의 시간)

    // ----- 상태 ----- //
    EnemyState _state = EnemyState.Idle;    // 적 상태 변수
    // ----- 상태 ----- //

    // ----- 이동 ----- //
    int _direction = 1;     // 1이면 오른쪽 방향, -1이면 왼쪽 방향
    // ----- 이동 ----- //

    // ----- 피격 ----- //
    public float _currentHp;        // 현재 체력
    float _knockbackTimer;          // 넉백 타이머
    float _deadTimer;               // 죽는 타이머
    // ----- 피격 ----- //

    // Start is called before the first frame update
    void Start()
    {
        _currentHp = MaxHp;
    }

    // Update is called once per frame
    void Update()
    {
        // GroundSensor의 위치에서
        // GroundSensor의 위치보다 아래로 한 칸 점까지
        // 빨간색의 선을 그린다.
        Debug.DrawLine(GroundSensor.position,
            GroundSensor.position + Vector3.down, Color.red);

        // 자신의 위치에서
        // 자신의 진행 방향으로 한 칸 떨어진 점까지
        // 초록색의 선을 그린다.
        Debug.DrawLine(transform.position,
            transform.position + Vector3.right * _direction, Color.green);
    }


    private void FixedUpdate()
    {
        switch (_state)
        {
            case EnemyState.Idle:
                HandleMove();       // 적 캐릭터의 이동 처리
                break;
            case EnemyState.Knockback:
                HandleKnockback();
                break;
            case EnemyState.Dead:
                HandleDead();
                break;
        }        
    }

    #region ----- 이동 -----
    void HandleMove()
    {
        // "Platform" 레이어인 게임오브젝트와 닿아 있지 않으면
        if(Rigid.IsTouchingLayers(LayerMask.GetMask("Platform")) == false)
        {
            return;
        }

        // 전방에 바닥이 있는지 여부 불리언 변수
        bool hasFrontGround =
            Physics2D.Raycast(GroundSensor.position, Vector2.down,
            1.0f, LayerMask.GetMask("Platform"));
        // Physics2D.Raycast(레이저 광선을 쏠 시작 위치(Vector2), 레이저 광선이 쏘아질 방향(Vector2),
        // 레이저 광선의 거리(float), 레이저 광선이 어떤 레이어를 감지할 것인가(LayerMask.GetMask()))
        // 레이저 광선이 "Platform"을 감지했으면 true,
        // 감지 못했으면 false
        // 레이저 광선이 감지를 할 수 있으려면 Collider가 있어야 한다.

        // 전방에 벽이 있는 여부 불리언 변수
        bool hasFrontWall =
            Physics2D.Raycast(transform.position, Vector2.right * _direction,
            1.0f, LayerMask.GetMask("Platform"));

        // 전방에 바닥이 없거나, 전방에 벽이 있으면
        if (hasFrontGround == false || hasFrontWall == true)
        {
            _direction = -_direction;       // _direction *= -1;
        }

        // 방향(_direction)에 따라 리지드바디2D의 velocity 값 설정
        Vector2 velocity = Rigid.velocity;
        velocity.x = _direction * MoveSpeed;
        Rigid.velocity = velocity;

        // 방향(_direction)에 따라 트랜스폼의 스케일 x를 변경해서 좌우 반전
        Vector3 scale = transform.localScale;
        scale.x = _direction;
        transform.localScale = scale;

        // 애니메이터에 파라미터 설정
        EnemyAnimator.SetFloat("MoveSpeed", MoveSpeed);
    }
    #endregion

    #region ----- 공격 -----
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 이번에 새로 충돌한 게임오브젝트의 레이어가
        // "Hero" 레이어이면
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hero"))
        {
            // 이번에 새로 충돌한 게임오브젝트에서
            // HeroController 컴포넌트 객체를 반환
            HeroController hero = collision.gameObject.GetComponent<HeroController>();

            // null: 해당 게임오브젝트에서 HeroController 컴포넌트를 못 찾은 경우

            if (hero != null)
            {
                Vector2 point = collision.GetContact(0).point;
                hero.TakeHit(Damage, point, AttackMagnitude);
            }
        }
    }
    #endregion

    #region ----- 피격 -----
    /// <summary>
    /// 공격을 받는 함수(대미지를 입고 넉백를 처리한다.)
    /// </summary>
    /// <param name="damage">입힐 대미지 수치</param>
    /// <param name="point">충돌이 일어난 지점 위치</param>
    /// <param name="magnitude">넉백 힘 강도</param>
    public void TakeHit(float damage, Vector2 point, float magnitude)
    {
        // 로직상 체력을 실제로 감소시키는 함수 호출
        TakeDamage(damage);

        // 넉백 처리
        Knockback(point, magnitude);

        // 애니메이션 처리
        EnemyAnimator.SetTrigger("OnHit");
    }

    public void TakeDamage(float damage)
    {
        _currentHp = Mathf.Clamp(_currentHp - damage, 0.0f, MaxHp);
        HpBar.fillAmount = _currentHp / MaxHp;

        // 현재 체력(_currentHp)이 0보다 작거나 같아지면
        if(_currentHp <= 0.0f)
        {
            Die();
        }
    }

    public void Knockback(Vector2 point, float magnitude)
    {
        if(_state == EnemyState.Dead) return;
        /*
        if (_state == EnemyState.Dead)
        {
            return;
        }
        */

        Rigid.velocity = Vector2.zero;

        // point(충돌 지점)에서 적 캐릭터의 위치로 향하는 방향으로 벡터
        // 넉백 방향
        Vector2 dir = Rigid.position - point;

        // 방향으로만 쓰기 위해서 크기를 1로.(Normalize)
        dir.Normalize();

        // 넉백 방향으로 힘을 가한다.
        Rigid.AddForce(dir * magnitude, ForceMode2D.Impulse);

        // 넉백 상태 설정
        _state = EnemyState.Knockback;
        _knockbackTimer = KnockbackDuration;
    }

    void HandleKnockback()
    {
        _knockbackTimer -= Time.fixedDeltaTime; // FixedUpdate()의 시간 간격
        // 넉백 시간이 다 되었으면
        if (_knockbackTimer < 0.0f)
        {
            // 적 캐리턱의 상태를 일반 상태로 설정한다.
            _state = EnemyState.Idle;
        }
    }

    // 죽음을 처리하는 함수
    void Die()
    {
        Rigid.simulated = false;    // 리지드바디2D 끄기
        Collider.enabled = false;   // 개별 컴포넌트 끄기

        _state = EnemyState.Dead;   // 죽음 상태로 전환
        _deadTimer = DeadDuration;  // 타이머 충전
        EnemyAnimator.SetTrigger("OnDead"); // 애니메이션 재생
    }

    void HandleDead()
    {
        _deadTimer -= Time.fixedDeltaTime;
        if(_deadTimer <= 0.0f)
        {
            Destroy(gameObject);
        }
    }
    #endregion
}
