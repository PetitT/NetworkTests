using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineThing : MonoBehaviour
{
    [ContextMenu("Call")]
    private void Call()
    {
        NetworkObject networkObject = GetComponent<NetworkObject>();
        string className = typeof( OfflineThing ).ToString();
        FindObjectOfType<PhotonOfflineTest>().RPC_Callback(className, nameof( MyMethod ), networkObject, "MyParameter" );
    }

    private void MyMethod(string parameter)
    {
        Debug.Log( parameter );
    }
}
