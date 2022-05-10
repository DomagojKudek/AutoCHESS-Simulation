using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float speed;

    void Update()
    {
        this.gameObject.transform.Rotate(new Vector3(0, 1, 0), Time.deltaTime*speed);
    }
}
