using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;


namespace Makist.Input
{
	[AddComponentMenu("Makist/Input/VirtualKeyboard")]
	public class VirtualKeyboard : MonoBehaviour
	{
	    public VirtualKeyboardInputField inputField = null;
		public kLanguage language = kLanguage.kKorean;

		public UnityEvent OnSubmit;

	    public enum kLanguage { kKorean, kEnglish};
	    protected bool mPressShift = false;
	    
	    protected Dictionary<char, char> CHARACTER_TABLE = new Dictionary<char, char>
	    {
	        {'1', '!'}, {'2', '@'}, {'3', '#'}, {'4', '$'}, {'5', '%'},{'6', '^'}, {'7', '&'}, {'8', '*'}, {'9', '('},{'0', ')'},
	        { '`', '~'},   {'-', '_'}, {'=', '+'}, {'[', '{'}, {']', '}'}, {'\\', '|'}, {',', '<'}, {'.', '>'}, {'/', '?'}
	    };

		static public VirtualKeyboard keyboard = null;
	    
	    void Awake()
	    {
			keyboard = this;
	    }
		// Use this for initialization
		void Start ()
		{
	        
		}
		
		// Update is called once per frame
		void Update ()
		{
		
		}

	    public void Clear()
	    {
			if(inputField != null)
	        {
				inputField.Clear();
	        }
	    }

	    void OnGUI()
	    {
	        //Event e = Event.current;
	        //if (e.isKey)
	        //  Debug.Log("Detected key code: " + e.keyCode);

	    }

		public bool pressedShift
		{
			get
			{
				return mPressShift;
			}
		}

		void RefreshVirtualKey()
		{
			VirtualKey[] vKeys = FindObjectsOfType<VirtualKey>();
			foreach(VirtualKey vk in vKeys)
				vk.RefreshDisplay();
		}

	    public void KeyDown(VirtualKey key)
	    {
			if(inputField != null)
	        {
	            switch(key.KeyType)
	            {
	                case VirtualKey.kType.kShift:
	                    {
							mPressShift = !mPressShift;
							RefreshVirtualKey();
	                    }
	                    break;
	                case VirtualKey.kType.kHangul:
	                    {
	                        if (language == kLanguage.kKorean)
								language = kLanguage.kEnglish;
	                        else
								language = kLanguage.kKorean;

							RefreshVirtualKey();
	                    }
	                    break;
	                case VirtualKey.kType.kSpace:
	                case VirtualKey.kType.kBackspace:
	                    {
							inputField.KeyDown(key);
	                    }
	                    break;
	                case VirtualKey.kType.kReturn:
	                    {
							OnSubmit.Invoke();
	                    }
	                    break;
	                case VirtualKey.kType.kCharacter:
	                    {
							char keyCharacter = key.keyCode;
	                        if (mPressShift)
								keyCharacter = char.ToUpper(key.keyCode);

	                        if (language == kLanguage.kKorean)
	                        {
								inputField.KeyDownHangul(keyCharacter);
	                        }
	                        else if (language == kLanguage.kEnglish)
	                        {
								inputField.KeyDown(keyCharacter);
	                        }
	                    }
	                    break;
	                case VirtualKey.kType.kOther:
	                    {
							char keyCharacter = key.keyCode;
	                        if (mPressShift)
	                            keyCharacter = CHARACTER_TABLE[keyCharacter];

							inputField.KeyDown(keyCharacter);
	                    }
	                    break;
	            }
	        }
	    }
	}
}
