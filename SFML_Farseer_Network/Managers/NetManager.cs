﻿using System;
using System.Collections.Generic;
using Lidgren.Network;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;

namespace SFML_Farseer_Network.Managers
{
    using Vector2 = Microsoft.Xna.Framework.Vector2;

    public enum NetRole
    {
        Undefined,
        Server,
        Client
    };

    public enum MessageType
    {
        UpdateDynamicBodies,
        CreateMouseJoint,
        MoveMouseJoint,
        DestroyMouseJoint
    };

    public class NetManager
    {
        public const int SEND_DYNAMIC_BODIES_INTERVAL = 10;
        private Game _game;
        private NetRole _role;
        private NetPeer _peer;
        private int _sendDynamicBodiesCounter = 0;

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

        private void processIncomingMessages()
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
                        case NetIncomingMessageType.Data:
                            MessageType messageType = (MessageType)im.ReadInt32();

                            if (messageType == MessageType.UpdateDynamicBodies)
                            {
                                receiveDynamicBodiesUpdate(im);
                            }
                            else if (messageType == MessageType.CreateMouseJoint)
                            {
                                receiveCreateMouseJoint(im);
                            }
                            else if (messageType == MessageType.DestroyMouseJoint)
                            {
                                receiveDestroyMouseJoint(im);
                            }
                            else if (messageType == MessageType.MoveMouseJoint)
                            {
                                receiveMoveMouseJoint(im);
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

        public void sendCreateMouseJoint(FixedMouseJoint mouseJoint)
        {
            NetOutgoingMessage om = _peer.CreateMessage();
            Body body = mouseJoint.BodyA;
            Vector2 localAnchorA = body.GetLocalVector(mouseJoint.WorldAnchorA);
            Vector2 point = mouseJoint.WorldAnchorB;

            om.Write((int)MessageType.CreateMouseJoint);
            om.Write((int)body.UserData);   // entityId
            om.Write(localAnchorA.X);
            om.Write(localAnchorA.Y);
            om.Write(point.X);
            om.Write(point.Y);

            _peer.SendMessage(om, _peer.Connections[0], NetDeliveryMethod.ReliableUnordered);
        }

        private void receiveCreateMouseJoint(NetIncomingMessage im)
        {
            int entityId = im.ReadInt32();
            Body body = _game.entityManager.getEntity(entityId);
            Vector2 localAnchorA = new Vector2(im.ReadFloat(), im.ReadFloat());
            Vector2 point = new Vector2(im.ReadFloat(), im.ReadFloat());

            if (body != null)
            {
                _game.physicsManager.createRemoteMouseJoint(body, body.GetWorldVector(ref localAnchorA), point);
            }
        }

        public void sendDestroyMouseJoint()
        {
            NetOutgoingMessage om = _peer.CreateMessage();

            om.Write((int)MessageType.DestroyMouseJoint);
            _peer.SendMessage(om, _peer.Connections[0], NetDeliveryMethod.ReliableUnordered);
        }

        public void receiveDestroyMouseJoint(NetIncomingMessage im)
        {
            _game.physicsManager.destroyRemoteMouseJoint();
        }

        public void sendMoveMouseJoint(FixedMouseJoint joint)
        {
            NetOutgoingMessage om = _peer.CreateMessage();

            om.Write((int)MessageType.MoveMouseJoint);
            om.Write(joint.WorldAnchorB.X);
            om.Write(joint.WorldAnchorB.Y);
            _peer.SendMessage(om, _peer.Connections[0], NetDeliveryMethod.ReliableUnordered);
        }

        public void receiveMoveMouseJoint(NetIncomingMessage im)
        {
            Vector2 worldAnchorB = new Vector2(im.ReadFloat(), im.ReadFloat());

            _game.physicsManager.moveRemoteMouseJoint(worldAnchorB);
        }

        private void sendDynamicBodies()
        {
            NetOutgoingMessage om = _peer.CreateMessage();

            om.Write((int)MessageType.UpdateDynamicBodies);
            om.Write(_game.entityManager.entities.Count);

            foreach (KeyValuePair<int, Body> entity in _game.entityManager.entities)
            {
                int entityId = entity.Key;
                Body body = entity.Value;

                om.Write(entityId);
                om.Write(body.Position.X);
                om.Write(body.Position.Y);
                om.Write(body.LinearVelocity.X);
                om.Write(body.LinearVelocity.Y);
                om.Write(body.Rotation);
                om.Write(body.AngularVelocity);
            }

            _peer.SendMessage(om, _peer.Connections[0], NetDeliveryMethod.Unreliable);
        }

        public void receiveDynamicBodiesUpdate(NetIncomingMessage im)
        {
            int entityCount = im.ReadInt32();

            _game.addMessage(String.Format("Received updates for {0} entities.", entityCount));

            for (int i = 0; i < entityCount; i++)
            {
                int entityId = im.ReadInt32();
                float positionX = im.ReadFloat();
                float positionY = im.ReadFloat();
                float velocityX = im.ReadFloat();
                float velocityY = im.ReadFloat();
                float angle = im.ReadFloat();
                float angularVelocity = im.ReadFloat();
                Body body = _game.entityManager.getEntity(entityId);

                if (body != null)
                {
                    body.Position = new Microsoft.Xna.Framework.Vector2(positionX, positionY);
                    body.LinearVelocity = new Microsoft.Xna.Framework.Vector2(velocityX, velocityY);
                    body.Rotation = angle;
                    body.AngularVelocity = angularVelocity;
                }
            }
        }

        public void update()
        {
            processIncomingMessages();

            if (_game.state == GameState.Ready)
            {
                if (_role == NetRole.Server)
                {
                    if (_sendDynamicBodiesCounter >= SEND_DYNAMIC_BODIES_INTERVAL)
                    {
                        _sendDynamicBodiesCounter = 0;
                        sendDynamicBodies();
                        _game.addMessage("Sent dynamic body info.");
                    }

                    _sendDynamicBodiesCounter++;
                }
            }
        }
    }
}
