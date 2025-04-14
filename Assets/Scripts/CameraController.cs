using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject Hero;

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        Vector3 heroPos = Hero.transform.position;
        pos.x = heroPos.x;
        transform.position = pos;
    }
}
