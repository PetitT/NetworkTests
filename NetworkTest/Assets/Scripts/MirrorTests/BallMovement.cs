using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMovement : NetworkBehaviour
{
    [SyncVar] double spawnTime;
    Vector3 direction = Vector3.forward;
    float speed = 3;

    public void Initialize(double time)
    {
        spawnTime = time;
    }

    private void Start()
    {
        if (!isServer)
        {
            // double delta = NetworkTime.time - spawnTime;
            // Vector3 deltaMovement = speed * (float)delta * Vector3.forward;
            // transform.position += deltaMovement;
        }
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
        if (direction == Vector3.forward)
        {
            if (transform.position.x > 5)
            {
                Revert();
            }
        }
        else
        {
            if (transform.position.x < -5)
            {
                Revert();
            }
        }
    }

    [ContextMenu("Revert")]
    public void Revert()
    {
        direction = -direction;
    }

    [Server]
    public void ServerRevert(double networkTime)
    {
        RevertOnClients();
        Revert();
    }

    [ClientRpc]
    private void RevertOnClients()
    {
        Revert();
    }
}
