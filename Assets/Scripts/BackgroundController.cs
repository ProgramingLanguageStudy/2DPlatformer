using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 카메라가 시작 위치부터 얼마만큼 떨어졌는지에 따라
// 배경을 반복시키는 역할
public class BackgroundController : MonoBehaviour
{
    public Transform CameraTransform;
    public MeshRenderer Renderer;
    public float ScrollFactor = 1.0f;

    // Update is called once per frame
    void Update()
    {
        Vector2 offset = Renderer.material.mainTextureOffset;
        offset.x = (1 / transform.localScale.x) * CameraTransform.position.x;
        offset.x *= ScrollFactor;
        Renderer.material.mainTextureOffset = offset;
    }
}
