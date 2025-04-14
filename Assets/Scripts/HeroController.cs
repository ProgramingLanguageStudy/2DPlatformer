using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    // 주인공 캐릭터 상태 enum 정의
    public enum HeroState
    {
        Idle,       // 0: 일반(기본) 상태
        Knockback,  // 1: 넉백 중인 상태
        Dash,       // 2: 대시 중인 상태
    }



    [Header("----- 매니저 -----")]
    public GameManager Manager;

    [Header("----- 컴포넌트 참조 -----")]
    public Rigidbody2D Rigid;
    public SpriteRenderer Renderer;
    public Animator HeroAnimator;

    [Header("----- 이동 -----")]
    public float MoveSpeed;     // 좌우 이동 속력

    [Header("----- 점프 -----")]
    public float JumpPower;     // 점프력

    [Header("----- 피격 -----")]
    public float MaxHp;         // 최대 체력
    public float KnockbackDuration;     // 넉백 지속 시간

    [Header("----- 공격 -----")]
    public float Damage;        // 공격 시 상대에게 입힐 대미지
    public float AttackMagnitude;       // 공격 시 상대에게 적용할 넉백 힘
    public float AttackDelay;   // 공격 속도(몇 초마다 공격이 가능하게 할지)
    public float AttackRange;   // 공격 사정거리

    [Header("----- 대시 -----")]
    public float DashSpeed;     // 대시 속력
    public float DashDuration;  // 대시 지속 시간
    public float DashCooltime;  // 대시 재사용 대기 시간

    // ----- 상태 ----- //
    public HeroState _state = HeroState.Idle;
    // ----- 상태 ----- //


    // ----- 점프 ----- //
    bool _isGround = false;     // 땅을 밟고 있는지 여부

    // 물리적인 콜리션 충돌 시 충돌 정보 배열 변수
    ContactPoint2D[] _contacts = new ContactPoint2D[3];
    // ----- 점프 ----- //

    // ----- 피격 ----- //
    float _currentHp;       // 현재 체력
    float _knockbackTimer;  // 넉백 타이머
    // ----- 피격 ----- //

    // ----- 공격 ----- //
    float _attackTimer;
    // ----- 공격 ----- //

    // ----- 대시 ----- //
    float _dashTimer;       // 대시 지속 시간에 대한 타이머
    float _dashCooltimer;   // 대시 쿨타임에 대한 타이머
    // ----- 대시 ----- //

    // Start is called before the first frame update
    void Start()
    {
        _currentHp = MaxHp;
    }

    // Update is called once per frame
    void Update()
    {
        switch (_state)
        {
            // 주인공의 상태가 일반 상태인 경우
            case HeroState.Idle:
                HandleMove();
                HandleJump();
                break;
            // 주인공의 상태가 넉백 상태인 경우
            case HeroState.Knockback:
                HandleKnockback();
                break;
            // 주인공의 상태가 대시 상태인 경우
            case HeroState.Dash:
                UpdateDash();
                break;
        }
        HandleAttack();
        HandleDash();
    }
    private void FixedUpdate()
    {
        CheckIsGround();
    }

    #region ----- 이동 -----
    void HandleMove()
    {
        // Rigidbody2D 컴포넌트의 velocity 값을 조절해서 좌우 이동
        Vector2 velocity = Rigid.velocity;
        // Input.GetAxis()는 -1.0f~1.0f 사이의 값을 반환
        // Input.GetAxisRaw()는 -1.0f, 0, 1.0f 세 개 값 중에서 하나만 반환
        velocity.x = Input.GetAxisRaw("Horizontal") * MoveSpeed;
        Rigid.velocity = velocity;

        // 좌우 이동에 따른 스프라이트렌더러(SpriteRenderer) 설정
        if (velocity.x > 0)
        {
            Renderer.flipX = false;
        }
        if (velocity.x < 0)
        {
            Renderer.flipX = true;
        }

        // 좌우 이동에 따른 애니메이터(Animator) 파라미터 설정
        HeroAnimator.SetFloat("MoveSpeed", Mathf.Abs(velocity.x));
    }
    #endregion

    #region ----- 점프 -----
    void HandleJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (_isGround)
            {
                Jump();
            }

            //// 이 게임오브젝트가
            //// "Platform"이라는 Layer를 가진 다른 게임오브젝트와 닿아 있으면
            //if (Rigid.IsTouchingLayers(LayerMask.GetMask("Platform")))
            //{
            //    Jump();
            //}
        }
    }

    void Jump()
    {
        // Rigidbody2D에 힘을 가해서 점프시키는 코드
        Rigid.AddForce(Vector2.up * JumpPower, ForceMode2D.Impulse);

        // 점프에 따른 애니메이터(Animator) 패러미터 설정
        HeroAnimator.SetTrigger("OnJump");
    }

    // 주인공 캐릭터 게임오브젝트가 바닥에 착지하고 있는 것이 맞는지
    // 판단하는 함수
    void CheckIsGround()
    {
        // GetContacts()는 기본적으로 현재 닿아 있는 게임오브젝트 수를 반환
        // 매개변수로 넣은 ContactPoint2D 배열이나 리스트에
        // 실제 Contact 정보를 넣어 준다.
        // ContactPoint2D 배열로 사용할 경우
        // 실제 Contact 정보가 몇 개든 해당 배열의 크기만큼만 정보를 넣어 준다.
        int contactCount = Rigid.GetContacts(_contacts);
        // 컨택 정보가 하나라도 있으면
        if(contactCount > 0)
        {
            foreach(ContactPoint2D contact in _contacts)
            {
                if(contact.normal.y > 0.7f)
                {
                    _isGround = true;
                    break;
                }
            }
        }
        // 컨택 정보가 전혀 없으면
        else
        {
            _isGround = false;
        }

        // 바닥 착지 여부에 따라 애니메이터(Animator) 패러미터 설정
        HeroAnimator.SetBool("IsOnAir", !_isGround);

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
        HeroAnimator.SetTrigger("OnHit");

        // 사운드 재생
        Manager.PlaySfx(SfxType.Hit);
    }

    public void TakeDamage(float damage)
    {
        _currentHp = Mathf.Clamp(_currentHp - damage, 0.0f, MaxHp);
        Manager.UpdateHpBar(_currentHp, MaxHp);
    }

    //// 콜리션 충돌이 일어났을 때 유니티에서 자동으로 호출하는 함수
    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    // 이번에 새로 콜리션 충돌을 한 상대 게임오브젝트가
    //    // "Obstacle" 레이어이면
    //    if(collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
    //    {
    //        TakeDamage(10.0f);

    //        // collision.GetContact()로 이번 충돌에 대한 정보를 가져올 수 있다.
    //        // collision.GetContact(0)은 0번 컨택. 이번 충돌에 대한 정보.

    //        // 충돌 지점
    //        Vector2 point = collision.GetContact(0).point;

    //        Knockback(point);
    //    }
    //    // 이번에 새로 콜리션 충돌을 한 상대 게임오브젝트가
    //    // "Enemy" 레이어이면
    //    else if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
    //    {
    //        TakeDamage(10.0f);

    //        Vector2 point = collision.GetContact(0).point;

    //        Knockback(point);
    //    }
    //}

    public void Knockback(Vector2 point, float magnitude)
    {
        Rigid.velocity = Vector2.zero;

        // point(충돌 지점)에서 주인공 캐릭터의 위치로 향하는 방향으로 벡터
        // 넉백 방향
        Vector2 dir = Rigid.position - point;

        // 방향으로만 쓰기 위해서 크기를 1로.(Normalize)
        dir.Normalize();

        // 넉백 방향으로 힘을 가한다.
        Rigid.AddForce(dir * magnitude, ForceMode2D.Impulse);

        // 넉백 상태 설정
        _state = HeroState.Knockback;
        _knockbackTimer = KnockbackDuration;
    }

    void HandleKnockback()
    {
        _knockbackTimer -= Time.deltaTime;
        // 넉백 시간이 다 되었으면
        if (_knockbackTimer < 0.0f)
        {
            // 주인공의 상태를 일반 상태로 설정한다.
            _state = HeroState.Idle;
        }
    }
    #endregion

    #region ----- 공격 -----
    void HandleAttack()
    {
        if(_attackTimer > 0.0f)
        {
            _attackTimer -= Time.deltaTime;
        }

        // 주인공이 일반 상태가 아니면
        if(_state != HeroState.Idle)
        {
            return;
        }

        // 유저가 "Fire1" 버튼을 눌렀으면
        if (Input.GetButtonDown("Fire1"))
        {
            // 공격 타이머가 0보다 작거나 같은 상태이면
            if(_attackTimer <= 0.0f)
            {
                _attackTimer = AttackDelay;
                Attack();
            }
        }
    }
    void Attack()
    {
        HeroAnimator.SetTrigger("OnAttack");

        // 레이저 광선을 발사할 시작 위치
        Vector2 origin = Rigid.position;

        // 레이저 광선을 발사할 방향
        Vector2 dir;

        // 주인공 캐릭터가 왼쪽을 보고 있으면
        if (Renderer.flipX)
        {
            // 레이저 광선 방향을 왼쪽으로 설정
            dir = Vector2.left;
        }
        // 주인공 캐릭터가 오른쪽을 보고 있으면
        else
        {
            // 레이저 광선 방향을 오른쪽으로 설정
            dir = Vector2.right;
        }

        // 우리가 발사한 레이저 광선이 "Enemy" 레이어를 가진 게임오브젝트와
        // 닿았을 때 그것에 대한 정보를 되돌려 준다.
        RaycastHit2D hit = 
            Physics2D.Raycast(origin, dir, AttackRange, LayerMask.GetMask("Enemy"));

        Manager.PlaySfx(SfxType.Attack);

        // RaycastHit2D가 자동으로 불리언 변수로 변환
        // 레이저 광선이 충돌한 정보가 있으면 true, 없으면 false
        if (hit)
        {
            // 레이저 광선이 출동한 상대 게임오브젝트에서
            // EnemyController 컴포넌트를 가져온 것
            EnemyController enemy =
                hit.collider.gameObject.GetComponent<EnemyController>();

            if(enemy != null)
            {
                // hit.point: 레이저 광선이 상대 콜라이더와 딱 닿은 지점 위치

                // 공격 처리
                enemy.TakeHit(Damage, hit.point, AttackMagnitude);
            }
        }
    }
    #endregion


    #region ----- 대시 -----
    void HandleDash()
    {
        if(_dashCooltimer > 0.0f)
        {
            _dashCooltimer -= Time.deltaTime;
        }

        // 주인공이 일반 상태가 아니면
        if(_state != HeroState.Idle)
        {
            return;
        }

        // "Fire2" 버튼을 누르면
        if (Input.GetButtonDown("Fire2"))
        {
            // 주인공 캐릭터에 지면에 닿아 있고
            // 대시 쿨타임 타이머가 다 소진된 상태이면
            if(_isGround && _dashCooltimer <= 0.0f)
            {
                BeginDash();
            }
        }
    }

    // 대시 동작을 시작하는 함수
    void BeginDash()
    {
        _state = HeroState.Dash;
        _dashCooltimer = DashCooltime;
        _dashTimer = DashDuration;

        gameObject.layer = LayerMask.NameToLayer("Enemy");
        //int dir = 1;
        //if(Renderer.flipX == true)
        //{
        //    dir = -1;
        //}
        int dir = Renderer.flipX ? -1 : 1;
        // 삼항 연산자
        // 조건 ? (true일 때 값) : (false일 때 값)

        Vector2 velocity = Rigid.velocity;
        velocity.x = dir * DashSpeed;
        Rigid.velocity = velocity;
    }

    // 대시 동작 중에 Update()에서 호출되는 함수
    void UpdateDash()
    {
        if(_dashTimer > 0.0f)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0.0f)
            {
                EndDash();
            }
        }
    }

    // 대시 동작 종료를 처리해 주는 함수
    void EndDash()
    {
        _state = HeroState.Idle;

        gameObject.layer = LayerMask.NameToLayer("Hero");
        Vector2 velocity = Rigid.velocity;
        velocity.x = 0.0f;
        Rigid.velocity = velocity;
    }
    #endregion
}
