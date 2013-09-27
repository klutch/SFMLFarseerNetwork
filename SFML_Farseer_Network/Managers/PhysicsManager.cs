using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
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

        public World world { get { return _world; } }

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

            for (int i = 0; i < 40; i++)
            {
                bool isCircle = _rng.Next(0, 2) == 0;
                Vector2 position = new Vector2((float)(_rng.NextDouble() * 2 - 1), (float)(_rng.NextDouble() * 2 - 1)) * 8f;

                if (isCircle)
                {
                    float radius = (float)(_rng.NextDouble() * 2 + 1);
                    Body body = BodyFactory.CreateCircle(_world, radius, 1f, position);

                    body.BodyType = BodyType.Dynamic;
                }
                else
                {
                    float width = (float)(_rng.NextDouble() * 2 + 1);
                    float height = (float)(_rng.NextDouble() * 2 + 1);
                    Body body = BodyFactory.CreateRectangle(_world, width, height, 1f, position);

                    body.Rotation = (float)(_rng.NextDouble() * 6.28 - 3.14);
                    body.BodyType = BodyType.Dynamic;
                }
            }
        }

        public void update()
        {
            _world.Step(_dt);
        }

        public void drawDebugView()
        {
            _debugView.draw();
            _debugView.reset();
        }
    }
}
