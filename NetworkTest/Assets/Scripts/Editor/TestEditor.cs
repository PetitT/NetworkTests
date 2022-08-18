using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PlayFabIntegration;
using System;

[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{
    Test test;
    PlayFabManager manager => test.playFabManager;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        test = target as Test;

        GUILayout.Space(10);
        GUILayout.Label("----LOGIN----");
        test.autoLoginOnStart = EditorGUILayout.Toggle("Login on start", test.autoLoginOnStart); 
        if (GUILayout.Button("Login with device"))
        {
            test.LoginWithDeviceID();
        }
        if(GUILayout.Button("Random login"))
        {
            test.CreateNewRandomAccount();
        }

        GUILayout.Space(10);

        GUILayout.Label("----INFOS----");
        GUI.enabled = false;
        GUILayout.Toggle(manager.IsLoggedIn, "Is Connected");
        GUI.enabled = true;
        GUILayout.BeginHorizontal();
        string name = string.IsNullOrEmpty(manager.DisplayName) ? "Current user has no display name" : $"Display name : {manager.DisplayName}";
        GUILayout.Label(name);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("----DISPLAY NAME----");
        test.newDisplayName = EditorGUILayout.TextField("New Name", test.newDisplayName);
        if (GUILayout.Button("Update display name"))
        {
            manager.LoginManager.UpdateDisplayName(test.newDisplayName);
        }

        GUILayout.Space(10);
        GUILayout.Label("----TITLE DATAS----");
        test.specificTitleDataKey = EditorGUILayout.TextField("Specific key", test.specificTitleDataKey);
        if (GUILayout.Button("Get specific title data key"))
        {
            test.GetSpecificTitleData();
        }
        if (GUILayout.Button("Get all title datas"))
        {
            test.GetTitleDatas();
        }

        GUILayout.Space(10);
        GUILayout.Label("----PLAYER DATAS----");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Key");
        test.playerDataKey = GUILayout.TextField(test.playerDataKey);
        GUILayout.Label("Value");
        test.playerDataValue = GUILayout.TextField(test.playerDataValue);
        GUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Object");
        test.myClass.myString = EditorGUILayout.TextField(test.myClass.myString);
        test.myClass.myInt = EditorGUILayout.IntField(test.myClass.myInt);
        test.myClass.myfloat = EditorGUILayout.FloatField(test.myClass.myfloat);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Save Player Data key and value"))
        {
            test.SavePlayerData();
        }
        if (GUILayout.Button("Save player data key and object as JSON"))
        {
            test.SaveDataAsJSON();
        }
        if (GUILayout.Button("Get all player datas"))
        {
            test.GetAllPlayerDatas();
        }
        if (GUILayout.Button("Get single player data"))
        {
            test.GetSpecificPlayerData();
        }
        if (GUILayout.Button("Get generic player data"))
        {
            test.GetGenericData();
        }

        GUILayout.Space(10);
        GUILayout.Label("----LEADERBOARD----");
        test.leaderboardName = EditorGUILayout.TextField("Leaderboard", test.leaderboardName);
        test.score = EditorGUILayout.IntField("Score", test.score);
        test.maxResultsCount = EditorGUILayout.IntField("Max results count", test.maxResultsCount);
        test.startPosition = EditorGUILayout.IntField("Start position", test.startPosition);
        if(GUILayout.Button("Send Score to leaderboard"))
        {
            test.SendDataToLeaderboard();
        }
        if(GUILayout.Button("Get leaderboard"))
        {
            test.GetDataFromLeaderboard();
        }
        if(GUILayout.Button("Get leaderboard around player"))
        {
            test.GetDataFromLeaderboardAroundPlayer();
        }

        GUILayout.Space(10);
        GUILayout.Label("---LOBBIES---");
        test.lobbyID = EditorGUILayout.TextField("Lobby ID", test.lobbyID);
        test.lobbyName = EditorGUILayout.TextField("Lobby Name", test.lobbyName);
        test.lobbyArrangementString = EditorGUILayout.TextField("Lobby Arrangement String", test.lobbyArrangementString);
        if(GUILayout.Button("Set Lobby Name"))
        {
            test.SetLobbyName();
        }
        if(GUILayout.Button("Get Lobby Name"))
        {
            test.GetLobby();
        }

    }
}
