using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingUI : MonoBehaviour
{
    void Update()
    {
        z += Time.deltaTime * rotationRate;

        GetComponent< RectTransform >().rotation = Quaternion.Euler( 0.0f, 0.0f, z );
    }

    private float z = 0.0f;
    private float rotationRate = 360.0f;
}
