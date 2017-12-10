using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Makist.IO;

public class TestCommTCP : MonoBehaviour
{
    public CommTCP commTCP;
    public Text rcvData;
    public InputField sendData;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (commTCP.IsOpen)
        {
            byte[] readData = commTCP.Read();
            if(readData != null)
            {
                rcvData.text = Encoding.ASCII.GetString(readData);
            }
        }		
	}

    public void Send()
    {
        byte[] data = Encoding.ASCII.GetBytes(sendData.text);
        commTCP.Write(data);
    }
}
