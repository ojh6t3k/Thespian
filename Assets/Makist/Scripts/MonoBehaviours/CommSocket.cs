using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;


namespace Makist.IO
{
	[Serializable]
	public class CommDevice
	{
		[SerializeField]
		public string name = "";
		[SerializeField]
		public string address = "";
		[SerializeField]
		public List<string> args = new List<string>();

		public CommDevice()
		{

		}

		public CommDevice(CommDevice device)
		{
			name = device.name;
			address = device.address;
			for (int i = 0; i < device.args.Count; i++)
				args.Add(device.args[i]);
		}

		public bool Equals(CommDevice device)
		{
			if (device == null)
				return false;

			if (!name.Equals(device.name))
				return false;

			if (!address.Equals(device.address))
				return false;

			if (args.Count != device.args.Count)
				return false;

			for (int i = 0; i < args.Count; i++)
			{
				if (!args[i].Equals(device.args[i]))
					return false;
			}

			return true;
		}

		public void CopyFrom(CommDevice device)
		{
			name = device.name;
			address = device.address;
			args.Clear();
			for (int i = 0; i < device.args.Count; i++)
				args.Add(device.args[i]);
		}
	}

	[Serializable]
	public class CommDeviceEvent : UnityEvent<CommDevice> {}

	public class CommSocket : MonoBehaviour
	{
		[SerializeField]
		public List<CommDevice> foundDevices = new List<CommDevice>();
		[SerializeField]
		public CommDevice device = new CommDevice();

		public UnityEvent OnOpen;
		public UnityEvent OnClose;
		public UnityEvent OnOpenFailed;
		public UnityEvent OnErrorClosed;
		public UnityEvent OnStartSearch;
		public UnityEvent OnStopSearch;
		public UnityEvent OnWriteCompleted;
		public CommDeviceEvent OnFoundDevice;

		public virtual void Open()
		{
		}

		public virtual void Close()
		{
		}

		protected virtual void ErrorClose()
		{
		}

		public virtual void StartSearch()
		{
		}

		public virtual void StopSearch()
		{
		}

		public virtual void Write(byte[] data, bool getCompleted = false)
		{
		}

		public virtual byte[] Read()
		{
			return null;
		}

		public virtual bool IsOpen
		{
			get
			{
				return false;
			}
		}

		public virtual bool IsSupport
		{
			get
			{
				return false;
			}
		}
	}
}

