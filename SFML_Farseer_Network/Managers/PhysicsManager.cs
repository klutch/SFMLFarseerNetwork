using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;

namespace SFML_Farseer_Network.Managers
{
    public class PhysicsManager
    {
        private Game _game;
        private World _world;
        private DebugView _debugView;
        private float _dt = 1f / 60f;
        private Random _rng;
        private FixedMouseJoint _mouseJoint;
        private FixedMouseJoint _remoteMouseJoint;

        public World world { get { return _world; } }
        public FixedMouseJoint remoteMouseJoint { get { return _remoteMouseJoint; } set { _remoteMouseJoint = value; } }

        public PhysicsManager(Game game)
        {
            _game = game;
            _world = new World(new Vector2(0, 9.8f));
            _debugView = new DebugView(_game, this);
            _rng = new Random(12345);
            
            BodyFactory.CreateRectangle(_world, 30f, 2f, 1f, new Vector2(0, 10));
            BodyFactory.CreateRectangle(_world, 30f, 2f, 1f, new Vector2(0, -10));
            BodyFactory.CreateRectangle(_world, 2f, 30f, 1f, new Vector2(13, 0));
            BodyFactory.CreateRectangle(_world, 2f, 30f, 1f, new Vector2(-13, 0));

            for (int i = 0; i < 50; i++)
            {
                bool isCircle = _rng.Next(0, 2) == 0;
                Vector2 position = new Vector2((float)(_rng.NextDouble() * 2 - 1), (float)(_rng.NextDouble() * 2 - 1)) * 8f;

                if (isCircle)
                {
                    float radius = (float)(_rng.NextDouble() + 0.4);
                    Body body = BodyFactory.CreateCircle(_world, radius, 1f, position);
                    int entityId = _game.entityManager.createEntity(body);

                    body.BodyType = BodyType.Dynamic;
                    body.UserData = entityId;
                    body.Restitution = 0.5f;
                }
                else
                {
                    float width = (float)(_rng.NextDouble() + 0.4);
                    float height = (float)(_rng.NextDouble() + 0.4);
                    Body body = BodyFactory.CreateRectangle(_world, width, height, 1f, position);
                    int entityId = _game.entityManager.createEntity(body);

                    body.Rotation = (float)(_rng.NextDouble() * 6.28 - 3.14);
                    body.BodyType = BodyType.Dynamic;
                    body.UserData = entityId;
                    body.Restitution = 0.5f;
                }
            }
        }

        public void createMouseJoint(Vector2 point)
        {
            Fixture fixture = _world.TestPoint(point);

            if (fixture != null)
            {
                _mouseJoint = JointFactory.CreateFixedMouseJoint(_world, fixture.Body, point);
                _mouseJoint.MaxForce = 1000f * fixture.Body.Mass;
                _game.netManager.sendCreateMouseJoint(_mouseJoint);
            }
        }

        public void moveMouseJoint(Vector2 point)
        {
            _mouseJoint.WorldAnchorB = point;
            _game.netManager.sendMoveMouseJoint(_mouseJoint);
        }

        public void moveRemoteMouseJoint(Vector2 worldAnchorB)
        {
            if (_remoteMouseJoint != null)
            {
                _remoteMouseJoint.WorldAnchorB = worldAnchorB;
            }
        }

        public void destroyMouseJoint()
        {
            _game.netManager.sendDestroyMouseJoint();
            _world.RemoveJoint(_mouseJoint);
            _mouseJoint = null;
        }

        public void createRemoteMouseJoint(Body body, Vector2 anchorA, Vector2 anchorB)
        {
            if (_remoteMouseJoint == null)
            {
                _remoteMouseJoint = JointFactory.CreateFixedMouseJoint(_world, body, anchorA);
                _remoteMouseJoint.WorldAnchorA = anchorA;
                _remoteMouseJoint.WorldAnchorB = anchorB;
            }
            else
            {
                _remoteMouseJoint.WorldAnchorB = anchorB;
            }
        }

        public void destroyRemoteMouseJoint()
        {
            if (_remoteMouseJoint != null)
            {
                _world.RemoveJoint(_remoteMouseJoint);
                _remoteMouseJoint = null;
            }
        }

        public void update()
        {
            if (_game.inFocus)
            {
                // Mouse joint
                if (_game.newMouseState.isLeftButtonPressed)
                {
                    if (_mouseJoint == null)
                    {
                        // Create mouse joint
                        createMouseJoint(new Vector2(_game.mouseWorld.X, _game.mouseWorld.Y));
                    }
                    else
                    {
                        // Move mouse joint
                        moveMouseJoint(new Vector2(_game.mouseWorld.X, _game.mouseWorld.Y));
                    }
                }
                else if (!_game.newMouseState.isLeftButtonPressed && _mouseJoint != null)
                {
                    // Destroy mouse joint
                    destroyMouseJoint();
                }
            }

            _world.Step(_dt);
        }

        public void drawDebugView()
        {
            _debugView.draw();
            _debugView.reset();
        }
    }
}
