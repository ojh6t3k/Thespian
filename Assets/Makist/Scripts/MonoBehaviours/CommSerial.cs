using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

#if (NET_2_0 && UNITY_STANDALONE)
using System.IO;
using System.IO.Ports;
#endif


namespace Makist.IO
{
	[AddComponentMenu("Makist/IO/CommSerial")]
	public class CommSerial : CommSocket
	{
		public int baudrate = 115200;
		public bool dtrReset = true;

		private bool _threadOnOpen = false;
		private bool _threadOnOpenFailed = false;
		private Thread _openThread;

		#if (NET_2_0 && UNITY_STANDALONE)
		private SerialPort _serialPort;
		#endif

		#region MonoBehaviour
		void Awake()
		{
			#if (NET_2_0 && UNITY_STANDALONE)
			_serialPort = new SerialPort();
			_serialPort.DataBits = 8;
			_serialPort.Parity = Parity.None;
			_serialPort.StopBits = StopBits.One;
			_serialPort.ReadTimeout = 1; // since on windows we *cannot* have a separate read thread            
			_serialPort.WriteTimeout = 1000;
			_serialPort.Handshake = Handshake.None;
			_serialPort.RtsEnable = false;
			#endif
		}

		void Update()
		{
			if (_threadOnOpen)
			{
				OnOpen.Invoke();
				_threadOnOpen = false;
			}

			if (_threadOnOpenFailed)
			{
				OnOpenFailed.Invoke();
				_threadOnOpenFailed = false;
			}
		}
		#endregion

		#region Override
		public override void Open()
		{
			if (IsOpen)
				return;

			_openThread = new Thread(openThread);
			_openThread.Start();
		}

		public override void Close()
		{
			if (!IsOpen)
				return;

			ErrorClose();
			OnClose.Invoke();
		}

		protected override void ErrorClose()
		{
			#if (NET_2_0 && UNITY_STANDALONE)
			try
			{
				_serialPort.Close();
			}
			catch(Exception)
			{
			}
			#endif
		}

		public override bool IsOpen
		{
			get
			{
				#if (NET_2_0 && UNITY_STANDALONE)
				if (_serialPort == null)
					return false;

				return _serialPort.IsOpen;
				#else
				return false;
				#endif
			}
		}

		public override bool IsSupport
		{
			get
			{
				#if (NET_2_0 && UNITY_STANDALONE)
				return true;
				#else
				return false;
				#endif
			}
		}

		public override void StartSearch()
		{
			foundDevices.Clear();
			OnStartSearch.Invoke();            

			#if NET_2_0
			#if UNITY_EDITOR
			#if UNITY_EDITOR_WIN
			// Windows port search
			string[] ports = SerialPort.GetPortNames();
			foreach (string port in ports)
			{
				CommDevice foundDevice = new CommDevice();
				foundDevice.name = port;
				foundDevice.address = "//./" + port;
				foundDevices.Add(foundDevice);
				OnFoundDevice.Invoke(foundDevice);
			}
			#elif UNITY_EDITOR_OSX
			// macOS port search
			string prefix = "/dev/";
			string[] ports = Directory.GetFiles(prefix, "*.*");
			foreach (string port in ports)
			{
				if (port.StartsWith("/dev/cu."))
				{
					CommDevice foundDevice = new CommDevice();
					foundDevice.name = port.Substring(prefix.Length);
					foundDevice.address = port;
					foundDevices.Add(foundDevice);
					OnFoundDevice.Invoke(foundDevice);
				}
			}
			#endif
			#elif UNITY_STANDALONE
			#if UNITY_STANDALONE_WIN
			// Windows port search
			string[] ports = SerialPort.GetPortNames();
			foreach (string port in ports)
			{
				CommDevice foundDevice = new CommDevice();
				foundDevice.name = port;
				foundDevice.address = "//./" + port;
				foundDevices.Add(foundDevice);
				OnFoundDevice.Invoke(foundDevice);
			}
			#elif UNITY_STANDALONE_OSX
			// macOS port search
			string prefix = "/dev/";
			string[] ports = Directory.GetFiles(prefix, "*.*");
			foreach (string port in ports)
			{
				if (port.StartsWith("/dev/cu."))
				{
					CommDevice foundDevice = new CommDevice();
					foundDevice.name = port.Substring(prefix.Length);
					foundDevice.address = port;
					foundDevices.Add(foundDevice);
					OnFoundDevice.Invoke(foundDevice);
				}
			}
			#endif
			#endif
			#endif

			OnStopSearch.Invoke();
		}

		public override void Write(byte[] data, bool getCompleted = false)
		{
			if (data == null)
				return;
			if (data.Length == 0)
				return;

			#if (NET_2_0 && UNITY_STANDALONE)
			try
			{
				_serialPort.Write(data, 0, data.Length);
				if(getCompleted)
					OnWriteCompleted.Invoke();
			}
			catch (Exception)
			{
				ErrorClose();
				OnErrorClosed.Invoke();
			}
			#endif
		}

		public override byte[] Read()
		{
			#if (NET_2_0 && UNITY_STANDALONE)
			List<byte> bytes = new List<byte>();

			while (true)
			{
				try
				{
					bytes.Add((byte)_serialPort.ReadByte());
				}
				catch (TimeoutException)
				{
					break;
				}
				catch (Exception)
				{
					ErrorClose();
					OnErrorClosed.Invoke();
					return null;
				}
			}

			if (bytes.Count == 0)
				return null;
			else
				return bytes.ToArray();
			#else
			return null;
			#endif
		}
		#endregion

		private void openThread()
		{
			#if (NET_2_0 && UNITY_STANDALONE)
			try
			{
				_serialPort.PortName = device.address;
				_serialPort.BaudRate = baudrate;
				_serialPort.DtrEnable = dtrReset;
				_serialPort.Open();
				_threadOnOpen = true;
			}
			catch (Exception)
			{
				_threadOnOpenFailed = true;
			}
			#else
			_threadOnOpenFailed = true;
			#endif

			_openThread.Abort();
			return;
		}
	}
}
