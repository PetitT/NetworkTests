using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerTest : NetworkBehaviour
{
    void Update()
    {
        if (hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CmdRevertBall(NetworkTime.time);
            }
        }
    }

    [Command]
    void CmdRevertBall(double networkTime)
    {
        FindObjectOfType<BallMovement>().ServerRevert(networkTime);
    }
}
