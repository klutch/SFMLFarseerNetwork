using System;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;

namespace SFML_Farseer_Network.Managers
{
    public class EntityManager
    {
        private Game _game;
        private Dictionary<int, Body> _entities;

        public Dictionary<int, Body> entities { get { return _entities; } }

        public EntityManager(Game game)
        {
            _game = game;
            _entities = new Dictionary<int, Body>();
        }

        public int getUnusedId()
        {
            int current = 0;

            while (true)
            {
                if (_entities.ContainsKey(current))
                {
                    current++;
                }
                else
                {
                    return current;
                }
            }
        }

        public Body getEntity(int entityId)
        {
            Body body;

            _entities.TryGetValue(entityId, out body);
            return body;
        }

        public int createEntity(Body body)
        {
            int id = getUnusedId();

            _entities.Add(id, body);
            return id;
        }
    }
}
