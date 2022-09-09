using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonOculusInputs : Fusion.Behaviour , INetworkRunnerCallbacks
{

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {

    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {

    }

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        OculusInputs inputs = new OculusInputs();

        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            inputs.Buttons.Set(OculusInputs.BUTTON_JUMP, true);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            inputs.Buttons.Set(OculusInputs.BUTTON_JUMP, true);
        }

        float X = Input.GetAxis("Horizontal");
        float Y = Input.GetAxis("Vertical");
        inputs.movement = new Vector2(X, Y);


        input.Set(inputs);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

    }

    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }
}

public struct OculusInputs : INetworkInput
{
    public const int BUTTON_JUMP = 0;

    public NetworkButtons Buttons;
    public Vector2 movement;

    public bool IsUp(int button)
    {
        return Buttons.IsSet(button) == false;
    }

    public bool IsDown(int button)
    {
        return Buttons.IsSet(button);
    }
}
