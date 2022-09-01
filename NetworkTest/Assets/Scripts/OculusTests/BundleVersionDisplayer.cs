using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class BundleVersionDisplayer : MonoBehaviour
{
    // -- FIELDS

    [SerializeField, HideInInspector] private int BundleVersion = 0;

    // -- UNITY

    private void OnValidate()
    {
#if UNITY_EDITOR
        BundleVersion = PlayerSettings.Android.bundleVersionCode;
#endif
    }

    void Awake()
    {
        var credit_text = GetComponent<TMP_Text>();
        credit_text.text += $"Bundle Version #{BundleVersion}";
    }
}
