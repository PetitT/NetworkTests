using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(UpdateColor))] public Color cubeColor = Color.black;
    public MeshRenderer cubeMesh;

    private void Update()
    {
        if (isServer)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                for (int i = 0; i < 5; i++)
                {
                    cubeColor = Random.ColorHSV();
                }
            }
        }
    }

    private void UpdateColor(Color oldValue, Color newValue)
    {
        cubeMesh.material.color = cubeColor;
    }
}
