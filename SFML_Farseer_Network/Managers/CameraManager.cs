using System;
using System.Collections.Generic;
using SFML.Window;
using SFML.Graphics;

namespace SFML_Farseer_Network.Managers
{
    public class CameraManager
    {
        private Game _game;
        private View _worldView;

        public View worldView { get { return _worldView; } }

        public CameraManager(Game game)
        {
            _game = game;
            _worldView = new View(new Vector2f(0, 0), new Vector2f(800, 600));
            _worldView.Zoom(1f / 32f);
        }
    }
}
