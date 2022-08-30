using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(UpdateColor))] public Color cubeColor = Color.black;
    public MeshRenderer cubeMesh;
    public GameObject ballPrefab;
    private GameObject newBall;
    CharInputs inputs;

    private void Start()
    {
        if (!isServer) return;
        inputs = new CharInputs();
        inputs.Enable();
        inputs.Server.ChangeColor.performed += ChangeColor_performed;
        inputs.Server.SpawnBall.performed += SpawnBall_performed;
    }

    private void SpawnBall_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        SpawnBall();
    }

    private void ChangeColor_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        for (int i = 0; i < 5; i++)
        {
            cubeColor = Random.ColorHSV();
        }
        UpdateColor(cubeColor, cubeColor);
    }


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
