using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using UnityEngine.Video;

[Serializable]
public class MotionData
{
    [Serializable]
    public class Motion
    {
		public string name;
		public float durationTime;
		public string description;
    }

    public string fileName;
    public string fullPath;
    public Motion[] motions;

    public MotionData(string path)
    {
        fullPath = path;
        fileName = Path.GetFileName(path);

        List<Motion> list = new List<Motion>();

        StreamReader sr = new StreamReader(path, Encoding.UTF8);
        while(true)
        {
            string line = sr.ReadLine();
            if (line == null)
                break;

            string[] tokens = line.Split('|');
            if(tokens.Length > 1)
            {
                Motion m = new Motion();
                m.name = tokens[0];
                m.durationTime = float.Parse(tokens[1]) / 1000f;

				if (tokens.Length > 2)
					m.description = tokens[2];

				list.Add(m);
            }
        }

        motions = list.ToArray();

        sr.Close();
    }
}

[Serializable]
public class SenarioData
{
	[Serializable]
	public class Action
	{
		public string name;
        public string description;
		public float startTime;
        public float durationTime;
		public string motionFile;
	}

	[Serializable]
	public class Actor
	{
		public string name;
		public string description;
        public Action[] actions;
        public int nextActionIndex;

        public Action nextAction
        {
            get
            {
                if (actions == null)
                    return null;
                
                if (nextActionIndex < 0 || nextActionIndex >= actions.Length)
                    return null;
                
                return actions[nextActionIndex];
            }
        }
	}

    public string fileName;
    public string fullPath;
    public Actor[] actors;

	public SenarioData(string path)
	{
        fullPath = path;
        fileName = Path.GetFileName(path);

        List<Actor> list = new List<Actor>();
		StreamReader sr = new StreamReader(path, Encoding.UTF8);
		while (true)
		{
			string line = sr.ReadLine();
			if (line == null)
				break;
            
            string[] tokens = line.Split('|');
            if(tokens.Length == 4)
            {
                Actor at = new Actor();

				string[] tokens2 = tokens[2].Split(';');
                at.name = tokens2[0];
                at.description = tokens2[1];

                List<Action> list2 = new List<Action>();
                tokens2 = tokens[3].Split(';');
                for (int i = 0; i < tokens2.Length; i++)
                {
                    string[] tokens3 = tokens2[i].Split(',');
                    if(tokens3.Length > 3)
                    {
                        Action a = new Action();
                        a.name = tokens3[0];
                        a.description = tokens3[1];
                        a.startTime = float.Parse(tokens3[2]) / 10f;
                        a.durationTime = float.Parse(tokens3[3]) / 10f;
                        a.motionFile = tokens3[4];

                        list2.Add(a);
                    }
                }
                at.actions = list2.ToArray();

                list.Add(at);
            }
		}
        actors = list.ToArray();
		sr.Close();
	}
}

public class SenarioPlayer : MonoBehaviour
{
    public IoXts ioXts;
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;

    private string _contentPath;
    private List<string> _videos;
    private List<string> _sounds;
    private List<MotionData> _motions;
    private List<SenarioData> _senarios;
    private bool _playing = false;
    private float _time = 0f;
    private SenarioData _currentSenario;

    void Awake()
    {
        _videos = new List<string>();
        _sounds = new List<string>();
        _motions = new List<MotionData>();
        _senarios = new List<SenarioData>();

        DirectoryInfo rootDir = Directory.GetParent(Application.dataPath);
        if (Application.isEditor)
            rootDir = new DirectoryInfo(Path.Combine(rootDir.FullName, "Build"));
        else
        {
            while(rootDir != null)
            {
                DirectoryInfo[] dirs = rootDir.GetDirectories();
                bool find = false;
                for (int i = 0; i < dirs.Length; i++)
                {
					if (dirs[i].Name.Equals("senarios"))
                    {
                        find = true;
                        break;
                    }
                }

                if (find)
                    break;
                else
                    rootDir = rootDir.Parent;
            }
        }

		_contentPath = Path.Combine(rootDir.FullName, "contents");
		DirectoryInfo contentDir = new DirectoryInfo(_contentPath);
		FileInfo[] files = contentDir.GetFiles("*.mp4");
		for (int i = 0; i < files.Length; i++)
			_videos.Add(files[i].Name);

		files = contentDir.GetFiles("*.wav");
		for (int i = 0; i < files.Length; i++)
			_sounds.Add(files[i].Name);

		DirectoryInfo motionDir = new DirectoryInfo(Path.Combine(rootDir.FullName, "motions"));
		files = motionDir.GetFiles("*.txt");
		for (int i = 0; i < files.Length; i++)
			_motions.Add(new MotionData(files[i].FullName));

		DirectoryInfo senarioDir = new DirectoryInfo(Path.Combine(rootDir.FullName, "senarios"));
		files = senarioDir.GetFiles("*.spf");
		for (int i = 0; i < files.Length; i++)
			_senarios.Add(new SenarioData(files[i].FullName));
    }

	// Use this for initialization
	void Start ()
    {
    //    StartCoroutine("PlaySound", Path.Combine(_contentPath, _sounds[0]));

    //    Play("Junbuk_170217.spf");
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(_playing)
        {
            _time += Time.deltaTime;

			for (int i = 0; i < _currentSenario.actors.Length; i++)
            {
                SenarioData.Action action = _currentSenario.actors[i].nextAction;
                if (action != null)
                {
                    if (_time >= action.startTime)
                    {
                        if(_videos.IndexOf(action.name) >= 0)
                        {
                            videoPlayer.url = Path.Combine(_contentPath, action.name);
                            videoPlayer.Play();
                        }
                        else if(_sounds.IndexOf(action.name) >= 0)
                        {
                            StartCoroutine("PlaySound", Path.Combine(_contentPath, action.name));
                        }
                        else
                        {
                            if(ioXts.IsConnected)
                                ioXts.PlayMotion(int.Parse(action.name));
                        }
                        _currentSenario.actors[i].nextActionIndex++;
                    }
                }
            }
        }
	}

    public bool IsPlaying
    {
        get
        {
            return _playing;
        }
    }

    public void Play(string fileName)
    {
        _currentSenario = null;
        for (int i = 0; i < _senarios.Count; i++)
        {
            if(_senarios[i].fileName.Equals(fileName))
            {
                _currentSenario = _senarios[i];
                break;
            }
        }

        if(_currentSenario != null)
        {
            for (int i = 0; i < _currentSenario.actors.Length; i++)
                _currentSenario.actors[i].nextActionIndex = 0;

			_time = 0f;
			_playing = true;
        }
    }

    private IEnumerator PlaySound(string path)
    {
        string url = "file://" + path;
		WWW www = new WWW(url);
        yield return www;
        
        AudioClip audioClip = www.GetAudioClip(false, false);
        if(audioClip != null && audioSource != null)
        {
            audioClip.name = Path.GetFileNameWithoutExtension(path);
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }
}
