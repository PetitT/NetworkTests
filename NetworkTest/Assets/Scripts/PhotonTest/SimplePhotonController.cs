using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePhotonController : NetworkBehaviour
{

    NetworkCharacterControllerPrototype ncc;

    private void Start()
    {
        ncc = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out OculusInputs inputs))
        {
            ncc?.Move(new Vector3(inputs.movement.x,0,inputs.movement.y));
        }
    }

    [BehaviourButtonAction("Say stuff")]
    private void SayStuff()
    {
        Debug.Log( $"State authority = {Object.HasStateAuthority}" );
        Debug.Log( $"Input authority = {Object.HasInputAuthority}" );
    }
}
