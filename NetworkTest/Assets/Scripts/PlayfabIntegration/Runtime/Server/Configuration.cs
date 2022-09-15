using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayfabConfiguration", menuName = "PlayFab/Configuration")]
public class Configuration : ScriptableObject
{
	public BuildType buildType;
	public string buildId = "";
	public bool playFabDebugging = false;
	public string[] preferredRegions;
}

public enum BuildType
{
	CLIENT,
	LOCAL_SERVER,
	REMOTE_SERVER
}