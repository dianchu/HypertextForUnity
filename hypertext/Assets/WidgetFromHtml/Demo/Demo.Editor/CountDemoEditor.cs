using System.Collections;
using System.Collections.Generic;
using UIWidgetsSample;
using Unity.UIWidgets.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CountDemo), true)]
[CanEditMultipleObjects]
public class CountDemoEditor : UIWidgetsPanelEditor
{
    private CountDemo _countDemo;

    // MyApp
    protected override void OnEnable()
    {
        base.OnEnable();
        _countDemo = this.serializedObject.targetObject as CountDemo;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("刷新"))
        {
            Debug.Log("更新CountDemo");
            _countDemo.RebuildApp();
        }
    }
}