using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(UpdateColor))] public Color cubeColor = Color.black;
    public MeshRenderer cubeMesh;
    public GameObject ballPrefab;

    private void Update()
    {
        if (isServer)
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                for (int i = 0; i < 5; i++)
                {
                    cubeColor = Random.ColorHSV();
                }
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SpawnBall();
            }
        }
    }

    private GameObject newBall;

    private void SpawnBall()
    {
        newBall = Instantiate(ballPrefab, Vector3.zero, Quaternion.Euler(0, 90, 0));
        NetworkServer.Spawn(newBall);
    }

    private void UpdateColor(Color oldValue, Color newValue)
    {
        cubeMesh.material.color = cubeColor;
    }
}
