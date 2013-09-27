using System;
using System.Collections.Generic;
using Lidgren.Network;

namespace SFML_Farseer_Network.Managers
{
    public enum NetRole
    {
        Undefined,
        Server,
        Client
    };

    public class NetManager
    {
        private Game _game;
        private NetRole _role;
        private NetPeer _peer;

        public NetRole role { get { return _role; } }
        public bool connected
        {
            get
            {
                if (_role == NetRole.Client)
                {
                    return (_peer as NetClient).ConnectionStatus == NetConnectionStatus.Connected;
                }
                else if (_role == NetRole.Server)
                {
                    return (_peer as NetServer).ConnectionsCount == 1;
                }
                else
                {
                    return false;
                }
            }
        }

        public NetManager(Game game)
        {
            _game = game;
            _role = NetRole.Undefined;
        }

        public void startServer()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("SFML_Farseer_Network");

            config.Port = 3456;
            _peer = new NetServer(config);
            _peer.Start();
            _role = NetRole.Server;
            _game.addMessage("Started server on port 3456.");
        }

        public void startClient()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("SFML_Farseer_Network");

            _peer = new NetClient(config);
            _peer.Start();
            _role = NetRole.Client;
            _game.addMessage("Started client.");
        }

        public void connectTo(string ip)
        {
            _peer.Connect(ip, 3456);
        }

        public void processIncomingMessages()
        {
            if (_peer != null)
            {
                NetIncomingMessage im;

                while ((im = _peer.ReadMessage()) != null)
                {
                    switch (im.MessageType)
                    {
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.ErrorMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.VerboseDebugMessage:
                            string text = im.ReadString();
                            _game.addMessage(text);
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                            if (status == NetConnectionStatus.Connected)
                            {
                                if (_role == NetRole.Client)
                                {
                                    _game.addMessage("Connected to server!");
                                }
                                else
                                {
                                    _game.addMessage("Client connected!");
                                }
                            }
                            else
                            {
                                _game.addMessage(status.ToString() + ": " + im.ReadString());
                            }
                            break;
                        default:
                            _game.addMessage("Unhandled message type: " + im.MessageType);
                            break;
                    }
                    _peer.Recycle(im);
                }
            }
        }

        public void shutdown()
        {
            if (_role == NetRole.Server)
            {
            }
            else if (_role == NetRole.Client)
            {
            }

            _peer.Shutdown("Bye");
        }
    }
}
