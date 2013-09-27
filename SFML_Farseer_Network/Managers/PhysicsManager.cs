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

        public World world { get { return _world; } }

        public PhysicsManager(Game game)
        {
            _game = game;
            _world = new World(new Vector2(0, 9.8f));
            _debugView = new DebugView(_game, this);
            
            BodyFactory.CreateRectangle(_world, 30f, 2f, 1f, new Vector2(0, 10));
            BodyFactory.CreateRectangle(_world, 30f, 2f, 1f, new Vector2(0, -10));
            BodyFactory.CreateRectangle(_world, 2f, 30f, 1f, new Vector2(13, 0));
            BodyFactory.CreateRectangle(_world, 2f, 30f, 1f, new Vector2(-13, 0));
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
