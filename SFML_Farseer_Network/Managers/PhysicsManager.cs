using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace SFML_Farseer_Network.Managers
{
    public class PhysicsManager
    {
        private World _world;

        public PhysicsManager(Game game)
        {
            _world = new World(new Vector2(0, 9.8f));

            BodyFactory.CreateRectangle(_world, 30f, 2f, 1f, Vector2.Zero);
        }
    }
}
