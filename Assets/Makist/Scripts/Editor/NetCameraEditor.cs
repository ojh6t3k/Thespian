using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Makist.Vision;

[CustomEditor(typeof(NetCamera))]
public class NetCameraEditor : Editor
{
	SerializedProperty script;
	SerializedProperty ipAddress;
	SerializedProperty port;
	SerializedProperty parameter;
	SerializedProperty targetTexture;

	void OnEnable()
	{
		script = serializedObject.FindProperty("m_Script");
		ipAddress = serializedObject.FindProperty("ipAddress");
		port = serializedObject.FindProperty("port");
		parameter = serializedObject.FindProperty("parameter");
		targetTexture = serializedObject.FindProperty("targetTexture");
	}

	public override void OnInspectorGUI()
	{
		this.serializedObject.Update();

		NetCamera netCamera = (NetCamera)target;

		GUI.enabled = false;
		EditorGUILayout.PropertyField(script, true, new GUILayoutOption[0]);
		GUI.enabled = true;

		EditorGUILayout.PropertyField(ipAddress, new GUIContent("IP Address"));
		EditorGUILayout.PropertyField(port, new GUIContent("port"));
		EditorGUILayout.PropertyField(parameter, new GUIContent("Parameter"));
		EditorGUILayout.PropertyField(targetTexture, new GUIContent("TargetTexture"));

		if(Application.isPlaying)
		{
			if(netCamera.isPlaying)
			{
				if(GUILayout.Button("Stop"))
					netCamera.Stop();

				EditorUtility.SetDirty(target);
			}
			else
			{
				if(GUILayout.Button("Play"))
					netCamera.Play();
			}
		}

		this.serializedObject.ApplyModifiedProperties();
	}
}
