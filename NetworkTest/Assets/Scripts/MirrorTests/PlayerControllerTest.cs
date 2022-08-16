using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerTest : NetworkBehaviour
{
    private BallMovement ball;
    private BallMovement _ball => ball ??= FindObjectOfType<BallMovement>();


    void Update()
    {
        if (hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                BallHitInfo hitInfo = new BallHitInfo
                {
                    networkTime = NetworkTime.time,
                    hitPosition = _ball.transform.position
                };

                CmdRevertBall(hitInfo);
                _ball.HitBall(hitInfo);
            }
        }
    }

    [Command]
    void CmdRevertBall(BallHitInfo hitInfo)
    {
        _ball.HitBall(hitInfo);
        RPCRevertBall(hitInfo);
    }

    [ClientRpc(includeOwner = false)]
    void RPCRevertBall(BallHitInfo hitInfo)
    {
        _ball.HitBall(hitInfo);
    }

}

public struct BallHitInfo
{
    public double networkTime;
    public Vector3 hitPosition;
}
