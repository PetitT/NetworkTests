using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorMovement : NetworkBehaviour
{
    public float MovementSpeed = 5f;
    private bool active = true;
    public override void OnStartAuthority()
    {
        active = true;
    }

    private void Update()
    {
        if (active)
            Move();
    }

    [Client]
    private void Move()
    {
        float X = Input.GetAxisRaw("Horizontal");
        float Z = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(X, 0, Z);

        transform.Translate(movement * MovementSpeed * Time.deltaTime);
    }
}
