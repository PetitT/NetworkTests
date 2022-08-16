using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMovement : NetworkBehaviour
{
    [SyncVar(hook = nameof(SyncSpawnTime))] double spawnTime;
    Vector3 direction;
    float speed = 1;

    Vector3 targetPosition;
    float lerpSpeed = 10;

    public override void OnStartServer()
    {
        spawnTime = NetworkTime.time;
    }

    private void Start()
    {
        direction = transform.forward;
    }

    private void SyncSpawnTime(double oldTime, double newTime)
    {
        spawnTime = newTime;
        direction = transform.forward;
        if (!isServer)
        {
            Reconcile(spawnTime, Vector3.zero);
        }
    }

    private void Reconcile(double reconcileTime, Vector3 initialPosition)
    {
        double delta = NetworkTime.time - reconcileTime;
        Vector3 deltaMovement = speed * (float)delta * direction;
        targetPosition = initialPosition + deltaMovement;
    }

    private void Update()
    {
        UpdateTargetPosition();
        MoveToTargetPosition();
    }

    private void UpdateTargetPosition()
    {
        targetPosition += direction * speed * Time.deltaTime;
        //if (direction == transform.forward)
        //{
        //    if (transform.position.x > 5)
        //    {
        //        Revert();
        //    }
        //}
        //else
        //{
        //    if (transform.position.x < -5)
        //    {
        //        Revert();
        //    }
        //}
    }

    private void MoveToTargetPosition()
    {
        if (isServer)
        {
            transform.position = targetPosition;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
        }
    }

    [ContextMenu("Revert")]
    public void Revert()
    {
        direction = -direction;
    }


    public void HitBall(BallHitInfo hitInfo)
    {
        Revert();
        Reconcile(hitInfo.networkTime, hitInfo.hitPosition);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(300, 300, 50, 50), spawnTime.ToString());
    }


}
