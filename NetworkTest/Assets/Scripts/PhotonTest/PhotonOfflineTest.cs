using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using Fusion.Sockets;
using System;
using System.Reflection;

public class PhotonOfflineTest : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [Networked( OnChanged = nameof( OnStringChanged ) )]
    public string MyString { get; set; }
    public NetworkBehaviour behaviour;

    public TestClass MyTestClass { get; set; }

    public void ChangeString()
    {
        MyString = "Eeee";
    }

    private static void OnStringChanged( Changed<PhotonOfflineTest> changed )
    {
        Debug.Log( changed.Behaviour.MyString );
    }

    private static void OnClassChanged( Changed<PhotonOfflineTest> changed )
    {
        Debug.Log( "Nice" );
    }

    [BehaviourButtonAction( "DoStuff" )]
    [Rpc]
    private void RPC_DoSomething()
    {
        Debug.Log( $"State authority = {behaviour.Object.HasStateAuthority}" );
        Debug.Log( $"Input authority = {behaviour.Object.HasInputAuthority}" );
        Debug.Log( $"Is server = {FindObjectOfType<NetworkRunner>().Simulation.IsServer}" );
        Debug.Log( $"Is client = {FindObjectOfType<NetworkRunner>().Simulation.IsClient}" );
        Debug.Log( $"Is player = {FindObjectOfType<NetworkRunner>().Simulation.IsPlayer}" );
        Debug.Log( $"Time = {FindObjectOfType<NetworkRunner>().Simulation.State.Time}" );
    }

    [Rpc]
    public void RPC_Callback( string className, string methodName, NetworkObject networkObject, string parameter )
    {
        object[] param = new object[] { parameter };
        SendCallback( className, methodName, networkObject, param );
    }

    private void SendCallback( string className, string methodName, NetworkObject networkObject, object[] parameters )
    {
        Type t = Type.GetType( className );
        object objectToCall = networkObject.GetComponent( t );
        MethodInfo method = t.GetMethod( methodName, BindingFlags.NonPublic | BindingFlags.Instance );
        method.Invoke( objectToCall, parameters );
    }

    public void PlayerLeft( PlayerRef player )
    {
        Debug.Log( "Player left" );
    }

    public void PlayerJoined( PlayerRef player )
    {
        Debug.Log( "Player joined " );
    }

}
public class TestClass : NetworkObject
{
    public string MyString;
}
