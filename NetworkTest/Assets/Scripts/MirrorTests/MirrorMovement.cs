using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MirrorMovement : NetworkBehaviour
{
    public float MovementSpeed = 5f;
    CharInputs charInputs;

    public override void OnStartAuthority()
    {
        charInputs = new CharInputs();
        charInputs.Character.Enable();
    }

    private void Update()
    {
        if (hasAuthority && !isServer)
        {
            Vector2 movement = charInputs.Character.Movement.ReadValue<Vector2>();
            transform.Translate(new Vector3(movement.x, 0, movement.y) * MovementSpeed * Time.deltaTime);
        }
    }
}
