using UnityEngine;
using UnityEditor;
using Makist.IO;

[CustomEditor(typeof(CommSerial))]
public class CommSerialEditor : Editor
{
	bool foldout = false;

	SerializedProperty script;
	SerializedProperty device;
	SerializedProperty deviceName;
	SerializedProperty deviceAddress;
	SerializedProperty deviceArgs;
	SerializedProperty baudrate;
	SerializedProperty dtrReset;
	SerializedProperty OnOpen;
	SerializedProperty OnClose;
	SerializedProperty OnOpenFailed;
	SerializedProperty OnErrorClosed;
	SerializedProperty OnStartSearch;
	SerializedProperty OnStopSearch;

	void OnEnable()
	{
		script = serializedObject.FindProperty("m_Script");

		device = serializedObject.FindProperty("device");
		deviceName = device.FindPropertyRelative("name");
		deviceAddress = device.FindPropertyRelative("address");
		deviceArgs = device.FindPropertyRelative("args");

		baudrate = serializedObject.FindProperty("baudrate");
		dtrReset = serializedObject.FindProperty("dtrReset");
		OnOpen = serializedObject.FindProperty("OnOpen");
		OnClose = serializedObject.FindProperty("OnClose");
		OnOpenFailed = serializedObject.FindProperty("OnOpenFailed");
		OnErrorClosed = serializedObject.FindProperty("OnErrorClosed");
		OnStartSearch = serializedObject.FindProperty("OnStartSearch");
		OnStopSearch = serializedObject.FindProperty("OnStopSearch");
	}

	public override void OnInspectorGUI()
	{
		this.serializedObject.Update();

		GUI.enabled = false;
		EditorGUILayout.PropertyField(script, true, new GUILayoutOption[0]);
		GUI.enabled = true;

#if (NET_2_0 && UNITY_STANDALONE)
		CommSerial socket = (CommSerial)target;

		GUI.enabled = !socket.IsOpen;
#if UNITY_EDITOR_WIN
		EditorGUILayout.LabelField(string.Format("Port Name: {0}", socket.device.name));
#elif UNITY_EDITOR_OSX
		EditorGUILayout.LabelField(string.Format("Port Name: {0}", socket.device.address));
#endif

		EditorGUILayout.BeginHorizontal();
		int index = -1;
		string[] list = new string[socket.foundDevices.Count];
		for (int i = 0; i < list.Length; i++)
		{
			list[i] = socket.foundDevices[i].name;
			if (deviceName.stringValue.Equals(socket.foundDevices[i].name))
				index = i;
		}
		int newIndex = EditorGUILayout.Popup(" ", index, list);
		if (newIndex != index)
		{
			CommDevice newDevice = socket.foundDevices[newIndex];
			deviceName.stringValue = newDevice.name;
			deviceAddress.stringValue = newDevice.address;
			deviceArgs.ClearArray();
			deviceArgs.arraySize = newDevice.args.Count;
			for(int i=0; i < newDevice.args.Count; i++)
			{
				SerializedProperty arg = deviceArgs.GetArrayElementAtIndex(i);
				arg.stringValue = newDevice.args[i];
			}
		}

		if (GUILayout.Button("Search", GUILayout.Width(60f)) == true)
			socket.StartSearch();
		EditorGUILayout.EndHorizontal();
		GUI.enabled = true;
#elif (!NET_2_0 && UNITY_STANDALONE)
		EditorGUILayout.HelpBox("You must set as '.Net 2.0' for API Compatibility Level in PlayerSetting.", MessageType.Error);
#else
		EditorGUILayout.HelpBox("This component only can work on standalone platform(windows, osx)", MessageType.Error);
		#endif

		EditorGUILayout.PropertyField(baudrate, new GUIContent("Baudrate"));
		EditorGUILayout.PropertyField(dtrReset, new GUIContent("DTR Reset"));

		foldout = EditorGUILayout.Foldout(foldout, "Events");
		if (foldout)
		{
			EditorGUILayout.PropertyField(OnOpen, new GUIContent("OnOpen"));
			EditorGUILayout.PropertyField(OnClose, new GUIContent("OnClose"));
			EditorGUILayout.PropertyField(OnOpenFailed, new GUIContent("OnOpenFailed"));
			EditorGUILayout.PropertyField(OnErrorClosed, new GUIContent("OnErrorClosed"));
			EditorGUILayout.PropertyField(OnStartSearch, new GUIContent("OnStartSearch"));
			EditorGUILayout.PropertyField(OnStopSearch, new GUIContent("OnStopSearch"));
		}

		this.serializedObject.ApplyModifiedProperties();
	}
}
