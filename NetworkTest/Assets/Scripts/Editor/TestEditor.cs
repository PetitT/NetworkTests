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
        if (GUILayout.Button("Login"))
        {
            manager.LoginManager.LogInWithDeviceID();
        }

        GUILayout.Space(10);

        GUILayout.Label("----INFOS----");
        GUI.enabled = false;
        GUILayout.Toggle(manager.IsLoggedIn, "Is Connected");
        GUI.enabled = true;
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Current Name : {manager.DisplayName}");
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("----DISPLAY NAME----");
        test.newDisplayName = EditorGUILayout.TextField("New Name", test.newDisplayName );
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
        test.myClass.whatever = EditorGUILayout.TextField(test.myClass.whatever);
        test.myClass.number = EditorGUILayout.IntField(test.myClass.number);
        test.myClass.furotu = EditorGUILayout.FloatField(test.myClass.furotu);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Save Player Data key and value"))
        {
            test.SavePlayerData();
        }
        if (GUILayout.Button("Save player data key and object as JSON"))
        {
            test.SaveDataAsJSON();
        }
        if (GUILayout.Button("GetPlayerDatas"))
        {
            test.GetPlayerDatas();
        }
    }
}
