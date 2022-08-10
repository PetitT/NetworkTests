using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Configuration", menuName = "PlayFab/Configuration")]
public class Configuration : ScriptableObject
{
	public BuildType buildType;
	public string buildId = "";
	public string ipAddress = "";
	public ushort port = 0;
	public bool playFabDebugging = false;
	public List<string> preferredRegions = new List<string>();
}

public enum BuildType
{
	CLIENT,
	LOCAL_SERVER,
	REMOTE_SERVER
}