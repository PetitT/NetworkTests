using Fusion;
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
            if (inputs.IsDown(OculusInputs.BUTTON_JUMP))
            {

            }


            ncc?.Move(new Vector3(inputs.movement.x,0,inputs.movement.y));
        }
    }
}
