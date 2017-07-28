using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

namespace Makist.IO
{
    [AddComponentMenu("Makist/IO/CommTCP")]
    public class CommTCP : CommSocket
    {
        public Toggle toggle;
        public bool server;
        public string ipAddress;
        public int port = 8080;

        public UnityEvent OnClientConnected;
        public UnityEvent OnClientDisconnected;

        private TcpListener _server;
        private TcpClient _client;
        private byte[] _buffer = new byte[1024];
        private List<byte> _readBuffer = new List<byte>();

        #region MonoBehaviour
        void Awake()
        {
        }

        void Start()
        {

        }

        void Update()
        {

        }
        #endregion

        #region Override
        public override void Open()
        {
            server = toggle.isOn;
            if (server)
            {
                if (_server != null)
                    return;

                try
                {
					_server = new TcpListener(IPAddress.Any, port);
					_server.Start();
					_server.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), _server);
                    Debug.LogError("Server Open");
                    OnOpen.Invoke();
                }
                catch(Exception e)
                {
                    Debug.LogError("Server Open Failed");
                    Debug.LogError(e);
                    _server.Stop();
                    _server = null;
                    OnOpenFailed.Invoke();
                }
            }
            else
            {
                if (_client != null)
                    return;

                try
                {
					_client = new TcpClient();
                    Debug.LogError("Client Begin Connect");
					_client.BeginConnect(IPAddress.Parse(ipAddress), port, new AsyncCallback(ConnectCallback), _client);
                }
                catch(Exception e)
                {
                    Debug.LogError("Client Begin Connect Failed");
                    Debug.LogError(e);
					_client.Close();
					_client = null;
                    OnOpenFailed.Invoke();
                }
            }
        }

        public override void Close()
        {
            if (server)
            {
                if (_server == null)
                    return;

                _server.Stop();
                if (_client != null)
                {
                    _client.Client.Disconnect(false);
                    _client.Close();
                    _client = null;
                }
                _server = null;
                Debug.LogError("Server Close");
            }
            else
            {
                if (_client == null)
                    return;

                _client.Client.Disconnect(false);
                _client.Close();
                _client = null;
                Debug.LogError("Client Close");
            }

            _readBuffer.Clear();
            OnClose.Invoke();
        }

        protected override void ErrorClose()
        {
            Debug.LogError("Error Close");
            OnErrorClosed.Invoke();
            Close();
        }

        public override bool IsOpen
        {
            get
            {
                if(server)
                {
                    if (_server == null)
                        return false;
                    else
                        return true;
                }
                else
                {
                    if (_client == null)
                        return false;

                    return _client.Connected;
                }
            }
        }

        public override bool IsSupport
        {
            get
            {
                return true;
            }
        }

        public override void StartSearch()
        {

        }

        public override void Write(byte[] data, bool getCompleted = false)
        {
            if (_client == null)
                return;

            try
            {
                if (getCompleted)
                    _client.GetStream().BeginWrite(data, 0, data.Length, new AsyncCallback(WriteCallback), _client);
                else
                    _client.GetStream().Write(data, 0, data.Length);
            }
            catch(Exception e)
            {
                Debug.LogError("Write Error");
                Debug.LogError(e);
                ErrorClose();
            }
        }

        public override byte[] Read()
        {
            if(_readBuffer.Count > 0)
            {
                byte[] data = _readBuffer.ToArray();
                _readBuffer.Clear();
                return data;
            }
            else
                return null;
        }
        #endregion

        #region Callback
        private void AcceptTcpClientCallback(IAsyncResult result)
        {
            try
            {
				_client = _server.EndAcceptTcpClient(result);
                _client.GetStream().BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(ReadCallback), _buffer);
				Debug.LogError("Client Connected");
				OnClientConnected.Invoke();
            }
            catch(Exception e)
            {
                Debug.LogError("AcceptCallback Error");
                Debug.LogError(e);
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                _client.EndConnect(result);
                _client.GetStream().BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(ReadCallback), _buffer);
                Debug.LogError("Client Open");
                OnOpen.Invoke();
            }
            catch(Exception e)
            {
                Debug.LogError("ConnectCallback Error");
                Debug.LogError(e);
				_client.Close();
				_client = null;
                OnOpenFailed.Invoke();
            }
        }

        private void ReadCallback(IAsyncResult result)
        {
            if (_client == null)
                return;
            
            int readSize = 0;
            try
            {
                readSize = _client.GetStream().EndRead(result);
            }
            catch(Exception e)
            {
                Debug.LogError("ReadCallback Exception");
                Debug.LogError(e);
                ErrorClose();
                return;
            }

            if(readSize == 0)
            {
                if(server)
                {
                    Debug.LogError("Client disconnected");
					_client.Client.Disconnect(false);
					_client.Close();
					_client = null;
					_readBuffer.Clear();
					OnClientDisconnected.Invoke();
                    _server.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), _server);
                }
                else
                {
                    
                }
				
                return;
            }
            else
            {
				for (int i = 0; i < readSize; i++)
					_readBuffer.Add(_buffer[i]);

				_client.GetStream().BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(ReadCallback), _buffer);
            }
        }

        private void WriteCallback(IAsyncResult result)
        {
            try
            {
                _client.GetStream().EndWrite(result);
                OnWriteCompleted.Invoke();
            }
            catch(Exception e)
            {
                Debug.LogError("WriteCallback Error");
                Debug.LogError(e);
                ErrorClose();
            }
        }
        #endregion

        public string LocalIPAddress
        {
            get
            {
                return Network.player.ipAddress;
            }
        }
    }
}
