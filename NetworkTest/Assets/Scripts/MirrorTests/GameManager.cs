using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(Bigule))] public Color cubeColor = Color.black;
    public MeshRenderer cubeMesh;

    private void Update()
    {
        if (isServer)
        {
            if (Input.GetKeyDown(KeyCode.Space)) { cubeColor = Random.ColorHSV(); }
        }
    }

    private void Bigule(Color oldValue, Color newValue)
    {
        cubeMesh.material.color = cubeColor;
    }
}
