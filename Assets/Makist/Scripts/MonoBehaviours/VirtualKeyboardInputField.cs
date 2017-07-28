using UnityEngine;
using System.Collections;
using UnityEngine.UI;


namespace Makist.Input
{
	[RequireComponent(typeof(InputField))]
	[AddComponentMenu("Makist/Input/VirtualKeyboardInputField")]
	public class VirtualKeyboardInputField : MonoBehaviour
	{
		AutomateKR	mAutomateKR = new AutomateKR();

		InputField _inputField;

		void Awake()
		{
			_inputField = GetComponent<InputField>();
		}

	    void Start ()
		{
		}
		
		void Update ()
		{

		}

	    public void Clear()
	    {
	        mAutomateKR.Clear();

			_inputField.text = mAutomateKR.completeText + mAutomateKR.ingWord;
	    }

	    public void KeyDownHangul(char _key)
	    {
	        mAutomateKR.SetKeyCode(_key);

			_inputField.text = mAutomateKR.completeText + mAutomateKR.ingWord;
	    }

	    public void KeyDown(char _key)
	    {
	        mAutomateKR.SetKeyString(_key);

			_inputField.text = mAutomateKR.completeText + mAutomateKR.ingWord;
	    }

	    public void KeyDown(VirtualKey _key)
	    {
	        switch(_key.KeyType)
	        {
	            case VirtualKey.kType.kBackspace:
	                {
	                    mAutomateKR.SetKeyCode(AutomateKR.KEY_CODE_BACKSPACE);

	                }
	                break;
	            case VirtualKey.kType.kSpace:
	                {
	                    mAutomateKR.SetKeyCode(AutomateKR.KEY_CODE_SPACE);
	                }
	                break;
	        }

			_inputField.text = mAutomateKR.completeText + mAutomateKR.ingWord;
	    }
	}
}
