using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Makist.IO;

[CustomEditor(typeof(CommTCP))]
public class CommTCPEditor : Editor
{
    bool foldout = false;

	SerializedProperty script;
    SerializedProperty toggle;
	SerializedProperty server;
	SerializedProperty ipAddress;
	SerializedProperty port;
	SerializedProperty OnOpen;
	SerializedProperty OnClose;
	SerializedProperty OnOpenFailed;
	SerializedProperty OnErrorClosed;
	SerializedProperty OnClientConnected;
	SerializedProperty OnClientDisconnected;

	void OnEnable()
	{
		script = serializedObject.FindProperty("m_Script");

        toggle = serializedObject.FindProperty("toggle");

		server = serializedObject.FindProperty("server");
		ipAddress = serializedObject.FindProperty("ipAddress");
		port = serializedObject.FindProperty("port");

		OnOpen = serializedObject.FindProperty("OnOpen");
		OnClose = serializedObject.FindProperty("OnClose");
		OnOpenFailed = serializedObject.FindProperty("OnOpenFailed");
		OnErrorClosed = serializedObject.FindProperty("OnErrorClosed");
		OnClientConnected = serializedObject.FindProperty("OnClientConnected");
		OnClientDisconnected = serializedObject.FindProperty("OnClientDisconnected");
	}

	public override void OnInspectorGUI()
	{
		this.serializedObject.Update();

		GUI.enabled = false;
		EditorGUILayout.PropertyField(script, true, new GUILayoutOption[0]);
		GUI.enabled = true;

        CommTCP socket = (CommTCP)target;

        EditorGUILayout.PropertyField(toggle, new GUIContent("toggle"));
        EditorGUILayout.PropertyField(server, new GUIContent("Server"));
        if (socket.server)
            EditorGUILayout.LabelField("IP:  " + socket.LocalIPAddress);
        else
            EditorGUILayout.PropertyField(ipAddress, new GUIContent("IP"));
        EditorGUILayout.PropertyField(port, new GUIContent("port"));

		foldout = EditorGUILayout.Foldout(foldout, "Events");
		if (foldout)
		{
			EditorGUILayout.PropertyField(OnOpen, new GUIContent("OnOpen"));
			EditorGUILayout.PropertyField(OnClose, new GUIContent("OnClose"));
			EditorGUILayout.PropertyField(OnOpenFailed, new GUIContent("OnOpenFailed"));
			EditorGUILayout.PropertyField(OnErrorClosed, new GUIContent("OnErrorClosed"));
			EditorGUILayout.PropertyField(OnClientConnected, new GUIContent("OnClientConnected"));
			EditorGUILayout.PropertyField(OnClientDisconnected, new GUIContent("OnClientDisconnected"));
		}

        this.serializedObject.ApplyModifiedProperties();
	}
}
