using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Threading;


public class IoXts : MonoBehaviour
{
	public class Parameter : IComparable<Parameter>
	{
		string _name;
		string _val;

		public Parameter(string name, string val)
		{
			_name = name;
			_val = val;
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public string Value
		{
			get
			{
				return _val;
			}
		}

		// Allows a list of parameters to be sorted alphabetically
		#region ICompare Members
		public int CompareTo(Parameter other)
		{
			return this.Name.CompareTo(other.Name);
		}
		#endregion

	}

	public class Command
	{
		private static int lastRequestID = 0;
		private int _requestID;
		private string _objectName;
		private string _functionName;
		private List<Parameter> _parameters;

		// Allow the client to rest the request id.
		// Perhaps useful when reconnecting to a socket.
		public static void ResetRequestID()
		{
			Command.lastRequestID = 0;
		}

		// Add any number of parameters to the command object.
		// A list of parameter objects can also be passed to the constructor.
		public void AddParameter(Parameter parameter)
		{
			_parameters.Add(parameter);
		}

		public Command(string objectName, string functionName)
			: this(objectName, functionName, null)
		{
		}

		public Command(string objectName, string functionName, params Parameter[] parameters)
		{
			_requestID = Command.lastRequestID++;
			_objectName = objectName;
			_functionName = functionName;
			_parameters = new List<Parameter>();

			if(parameters != null)
			{
				foreach(Parameter parameter in parameters)
				{
					_parameters.Add(parameter);
				}
			}
		}

		public int RequestID
		{
			get
			{
				return _requestID;
			}
		}

		private static string IndentXMLString(string xml)
		{
			string outXml = string.Empty;
			MemoryStream ms = new MemoryStream();
			// Create a XMLTextWriter that will send its output to a memory stream (file)
			XmlTextWriter xtw = new XmlTextWriter(ms, Encoding.Unicode);
			XmlDocument doc = new XmlDocument();

			try
			{
				// Load the unformatted XML text string into an instance
				// of the XML Document Object Model (DOM)
				doc.LoadXml(xml);

				// Set the formatting property of the XML Text Writer to indented
				// the text writer is where the indenting will be performed
				xtw.Formatting = Formatting.Indented;

				// write dom xml to the xmltextwriter
				doc.WriteContentTo(xtw);
				// Flush the contents of the text writer
				// to the memory stream, which is simply a memory file
				xtw.Flush();

				// set to start of the memory stream (file)
				ms.Seek(0, SeekOrigin.Begin);
				// create a reader to read the contents of
				// the memory stream (file)
				StreamReader sr = new StreamReader(ms);
				// return the formatted string to caller
				return sr.ReadToEnd();
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
				return null;
			}
		}

		public string Xml
		{
			get
			{
				_parameters.Sort();

				/*
                 * Construct the snippet of XML that IOServe expects. It should look like
                 * <ioxts>
                 *     <command>
                 *          <object>authentication_management</object>
                 *          <function_name>authenticate</function_name>
                 *          <parameters>
                 *              <access_level>3</access_level>
                 *              <password>foobar</password>
                 *              <user>flash</user>
                 *          </parameters>
                 *      </command>
                 *  </ioxts>\0
                 *
                 * Notice the NULL terminating character 
                 * In this example I return the Xml with the NULL character thought the property XmlWithNull
                 */
				XmlDocument xmldoc = new XmlDocument();

				XmlElement root = xmldoc.CreateElement("ioxts");
				xmldoc.AppendChild(root);

				XmlElement command = xmldoc.CreateElement("command");
				root.AppendChild(command);

				XmlElement objectNameNode = xmldoc.CreateElement("object");
				objectNameNode.InnerText = _objectName;
				command.AppendChild(objectNameNode);

				XmlElement requestorNode = xmldoc.CreateElement("requestor_id");
				requestorNode.InnerText = _requestID.ToString();
				command.AppendChild(requestorNode);

				XmlElement functionNameNode = xmldoc.CreateElement("function_name");
				functionNameNode.InnerText = _functionName;
				command.AppendChild(functionNameNode);

				if(_parameters.Count > 0)
				{
					XmlElement parametersNode = xmldoc.CreateElement("parameters");
					command.AppendChild(parametersNode);

					foreach (Parameter parameter in _parameters)
					{
						XmlElement parameterNode = xmldoc.CreateElement(parameter.Name);
						parameterNode.InnerText = parameter.Value;
						parametersNode.AppendChild(parameterNode);
					}
				}

				string str = IndentXMLString(root.OuterXml);
				str = str.Replace("\r", "");
				//str = str.Replace("\n", "");
				return str;
			}
		}

		public string XmlWithNull
		{
			get
			{
				return Xml + "\0";
			}
		}
	}

	public class Response
	{
		public enum ResposeType { Standard, Notification };

		private ResposeType _responseType;
		private int _requestID;
		private string _replyFunction;
		private string _serverObject;
		private string _type;
		private string _functionName;
		private string _return_value;
		private string _system_status;
		private string _system_mode;
		private string _result;
		private Command _command;

		/*
         * Response object should like this
         * <ioxts>
         *   <response>
             *   <reply_func>ios_object_handler</reply_func>
             *   <object>authentication_management</object>
             *   <function_name>authenticate</function_name>
             *   <result>
             *       <access_level>10</access_level>
             *       <access>Granted</access>
             *   </result>
             *   <return_value>ok</return_value>
             *   <system_status>0</system_status>
             *   <system_mode>1</system_mode>
         *   </response>
         * </ioxts>\0
         */
		public Response(string xml)
		{
			try
			{
				xml = xml.Replace(Convert.ToChar(0x0).ToString(), "");
				xml = xml.Replace(" ", "");
				xml = xml.Replace("\t", "");
				xml = xml.Replace("\r", "");
				xml = xml.Replace("\n", "");

				XmlDocument doc = new XmlDocument();
				doc.InnerXml = xml;

				XmlNodeList list = doc.SelectNodes("/ioxts/response");

				if (list.Count == 0)
				{
					list = doc.SelectNodes("/ioxts/notification");

					XmlNode xn = list[0];

					_responseType = ResposeType.Notification;
					_serverObject = xn["object"].InnerText;
					_type = xn["type"].InnerText;
				}
				else
				{
					XmlNode xn = list[0];

					_responseType = ResposeType.Standard;
					_requestID = Convert.ToInt32(xn["requestor_id"].InnerText);
					_replyFunction = xn["reply_func"].InnerText;
					_serverObject = xn["object"].InnerText;
					_functionName = xn["function_name"].InnerText;
					_return_value = xn["return_value"].InnerText;
					_system_status = xn["system_status"].InnerText;
					_system_mode = xn["system_mode"].InnerText;
					_result = xn["result"].OuterXml;
				}
			}
			catch (Exception e)
			{
				Debug.Log("Response Error: " + e.Message);
			}
		}

		public ResposeType resposeType
		{
			get
			{
				return _responseType;
			}
		}

		public int RequestID
		{
			get
			{
				return _requestID;
			}
		}

		// Return the correspong Command for this response.
		public Command Command
		{
			get
			{
				return _command;
			}
			set
			{
				_command = value;
			}
		}

		public string ServerObject
		{
			get
			{
				return _serverObject;
			}
		}

		public string ReturnValue
		{
			get
			{
				return _return_value;
			}
		}

		public string ResultXml
		{
			get
			{
				return _result;
			}
		}

		public string SystemStatus
		{
			get
			{
				return _system_status;
			}
		}

		public override string ToString()
		{
			string response = "------------Response--------------------\n";
			response += "Reply function: " + _replyFunction + "\n";
			response += "Server object: " + _serverObject + "\n";
			response += "Function name: " + _functionName + "\n";
			response += "Return value: " + _return_value + "\n";
			response += "System status: " + _system_status + "\n";
			response += "System mode: " + _system_mode + "\n";

			return response;
		}
	}

	public string ipAddress;
	public string user = "flash";
	public string password = "foobar";

	public UnityEvent OnConnected;
	public UnityEvent OnConnectFailed;
	public UnityEvent OnDisconnected;
	public UnityEvent OnLostConnection;

	private TcpClient _tcpclnt;
	private NetworkStream _stream;
	private Dictionary<int, Command> _commandsPending;
	private AsyncCallback _messageReceivedCallback;
	private byte[] _receiveBytes = new byte[1024 * 1024];
	private string _id;
	private bool _connected = false;
	private bool _threadOnOpen = false;
	private bool _threadOnOpenFailed = false;
	private Thread _openThread;
	private float _time;

	void Awake()
	{
		_id = Guid.NewGuid().ToString();
		_commandsPending = new Dictionary<int, Command>();
		_messageReceivedCallback = new AsyncCallback(OnMessageReceived);
	}

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (_threadOnOpen)
		{
			Debug.Log("OnConnected");
			OnConnected.Invoke();
			_threadOnOpen = false;
		}

		if (_threadOnOpenFailed)
		{
			Debug.Log("OnConnectFailed");
			OnConnectFailed.Invoke();
			_threadOnOpenFailed = false;
		}

		if(_connected)
		{
			_time += Time.deltaTime;
			if(_time >= 1f)
			{
				Command command = new Command("authentication_management", "keep_alive");
				SendCommand(command);
				_time = 0f;
			}
		}
	}

	public void Connect()
	{
		if(_connected)
			return;

		_time = 0f;

		if(_openThread != null)
			_openThread.Abort();

		_openThread = new Thread(openThread);
		_openThread.Start();
	}

	public void Disconnect()
	{
		if(!_connected)
			return;

		_connected = false;
		_stream.Close();
		_tcpclnt.Close();
		_commandsPending.Clear();
		Command.ResetRequestID();

		Debug.Log("OnDisconnected");
		OnDisconnected.Invoke();
	}

	public void PlayMotion(int sequenceNumber)
	{
		PlayMotion(sequenceNumber, 1, 0, 0);
	}

	public void PlayMotion(int sequenceNumber, int loops, int offset, int duration)
	{
		// If we are not connected do no try to keep alive
		if (!_connected)
			return;

		Command command = new Command("sequence_management", "play");
		command.AddParameter(new Parameter("sequence_number", sequenceNumber.ToString()));
		command.AddParameter(new Parameter("loops", loops.ToString()));
		command.AddParameter(new Parameter("offset", offset.ToString()));
		command.AddParameter(new Parameter("duration", duration.ToString()));

		SendCommand(command);
	}

	public bool IsConnected
	{
		get
		{
			return _connected;
		}
	}

	private void ErrorDisconnect()
	{
		_connected = false;
		_stream.Close();
		_tcpclnt.Close();
		_commandsPending.Clear();
		Command.ResetRequestID();

		Debug.Log("OnLostConnection");
		OnLostConnection.Invoke();
		OnDisconnected.Invoke();
	}

	private string GetSendHeader()
	{
		try
		{
			ASCIIEncoding asen = new ASCIIEncoding();
			string header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";

			return header;
		}
		catch (Exception)
		{
			throw;
		}
	}

	// Send a str throug the open NetworkStram associated to the TCP socket
	private bool SendCommand(Command command, string str)
	{
		ASCIIEncoding asen = new ASCIIEncoding();

		//byte[] ba = asen.GetBytes(str);

		byte[] ba = Encoding.UTF8.GetBytes(str);

		try
		{
			lock (_stream)
			{
				_stream.Write(ba, 0, ba.Length);

				if (ba[ba.Length - 1] != 0)
				{
					Debug.Log("The last byte of the sent data needs to be a NULL byte ie 0");
					return false;
				}

				_commandsPending[command.RequestID] = command;
			}
		}
		catch (Exception e)
		{
			Debug.Log(e);
			return false;
		}

		return true;
	}

	private bool SendCommand(Command command)
	{
		return SendCommand(command, command.XmlWithNull);
	}

	private void OnMessageReceived(IAsyncResult ar)
	{
		if (!_connected)
			return;
		
		var readCount = 0;

		try
		{
			lock (_stream)
			{
				readCount = _stream.EndRead(ar);

				if (readCount <= 0)
				{
					Debug.Log("readCount zero");
					ErrorDisconnect();
					return;
				}

				if (readCount > 0)
				{
					var rcvBytes = _receiveBytes.Take<byte>(readCount);

					do
					{
						var messageBytes = rcvBytes.ToArray<byte>();
						rcvBytes = rcvBytes.Skip<byte>(readCount);

						string str = Encoding.UTF8.GetString(messageBytes, 0, messageBytes.Length);

						Response response = new Response(str);
						if (response.resposeType != Response.ResposeType.Notification)
						{
							Command command = _commandsPending[response.RequestID];
							response.Command = command;
						}

					} while (rcvBytes.Count<byte>() > 0);
				}

				if(_connected)
				{
					_stream.BeginRead(_receiveBytes, 0, _receiveBytes.Length, _messageReceivedCallback, null);
				}
			}
		}
		catch (Exception e)
		{
			if(_connected)
			{
				Debug.Log(e);
				ErrorDisconnect();
			}
		}
	}

	private void openThread()
	{
		#if UNITY_ANDROID
		AndroidJNI.AttachCurrentThread();
		#endif

		try
		{
			_tcpclnt = new TcpClient();
			_tcpclnt.SendTimeout = 5000;
			_tcpclnt.ReceiveTimeout = 10000;
			_tcpclnt.ReceiveBufferSize = 102400;
			_tcpclnt.SendBufferSize = 10240;
			_tcpclnt.NoDelay = true;

			var c = _tcpclnt.BeginConnect(ipAddress, 7766, null, null);
			var success = c.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
			if(!success)
			{
				_tcpclnt.Close();
				_threadOnOpenFailed = true;
			}
			_tcpclnt.EndConnect(c);

			_stream = _tcpclnt.GetStream();

			Command command = new Command("authentication_management", "authenticate");
			command.AddParameter(new Parameter("user", user));
			command.AddParameter(new Parameter("password", password));
			command.AddParameter(new Parameter("access_level", "1"));
			string str = GetSendHeader() + command.XmlWithNull;
			SendCommand(command, str);

			_stream.BeginRead(_receiveBytes, 0, _receiveBytes.Length, _messageReceivedCallback, null);

			_connected = true;
			_threadOnOpen = true;
		}
		catch (Exception e)
		{
			if(_tcpclnt != null)
				_tcpclnt.Close();
			
			Debug.Log("OpenThread: " + e);
			_threadOnOpenFailed = true;
		}

		#if UNITY_ANDROID
		AndroidJNI.DetachCurrentThread();
		#endif

		_openThread.Abort();
		return;
	}
}
