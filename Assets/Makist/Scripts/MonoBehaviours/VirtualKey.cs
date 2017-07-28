using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;


namespace Makist.Input
{
	[Serializable]
	public class VirtualKeyDisplay
	{
		public VirtualKeyboard.kLanguage language;
		public string normalText;
		public string shiftText;
		public Sprite normalSprite;
		public Sprite shiftSprite;
	}

	[RequireComponent(typeof(Button))]
	[AddComponentMenu("Makist/Input/VirtualKey")]
	public class VirtualKey : MonoBehaviour
	{
	    public enum kType { kCharacter, kOther, kReturn, kSpace, kBackspace, kShift, kTab, kCapsLock, kHangul}
	    public char keyCode;
	    public kType KeyType = kType.kCharacter;
		public Text graphicText;
		public Image graphicImage;
		[SerializeField]
		public VirtualKeyDisplay[] displays;
	    
	    private bool mKeepPresed;
	    public bool KeepPressed
	    {
	        set { mKeepPresed = value; }
	        get { return mKeepPresed; }
	    }

		// Use this for initialization
		void Start ()
		{
	        Button _button = gameObject.GetComponent<UnityEngine.UI.Button>();
	        if(_button != null)
	        {
	            _button.onClick.AddListener(onKeyClick);
	        }

			RefreshDisplay();
	    }

	    // Update is called once per frame
	    void Update ()
		{
		    if(KeepPressed)
	        {
	            //do something
	        }
		}

		void OnEnable()
		{
			RefreshDisplay();
		}

		void onKeyClick()
		{
			if(VirtualKeyboard.keyboard != null)
			{
				VirtualKeyboard.keyboard.KeyDown(this);
			}
		}

		public void RefreshDisplay()
		{
			if(VirtualKeyboard.keyboard != null)
			{
				foreach(VirtualKeyDisplay disp in displays)
				{
					if(disp.language == VirtualKeyboard.keyboard.language)
					{
						if(VirtualKeyboard.keyboard.pressedShift)
						{
							if(graphicText != null)
								graphicText.text = disp.shiftText;

							if(graphicImage != null)
								graphicImage.sprite = disp.shiftSprite;
						}
						else
						{
							if(graphicText != null)
								graphicText.text = disp.normalText;

							if(graphicImage != null)
								graphicImage.sprite = disp.normalSprite;
						}

						return;
					}
				}
			}
		}
	}
}
