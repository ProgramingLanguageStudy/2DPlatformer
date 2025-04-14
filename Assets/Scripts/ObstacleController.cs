using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    public float Damage;    // 주인공이 부딪혔을 때 입을 대미지
    public float Magnitude; // 주인공이 부딪혔을 때 받을 넉백 힘

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 이번에 새로 충돌한 게임오브젝트의 레이어가
        // "Hero" 레이어이면
        if(collision.gameObject.layer == LayerMask.NameToLayer("Hero"))
        {
            // 이번에 새로 충돌한 게임오브젝트에서
            // HeroController 컴포넌트 객체를 반환
            HeroController hero = collision.gameObject.GetComponent<HeroController>();

            // null: 해당 게임오브젝트에서 HeroController 컴포넌트를 못 찾은 경우

            if(hero != null)
            {
                Vector2 point = collision.GetContact(0).point;
                hero.TakeHit(Damage, point, Magnitude);
            }
        }
    }
}
