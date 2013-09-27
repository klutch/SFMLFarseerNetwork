using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;

namespace SFML_Farseer_Network
{
    public class Game : IDisposable
    {
        private RenderWindow _window;

        public Game()
        {
            _window = new RenderWindow(new VideoMode(800, 600), "Farseer Network Test");
            _window.Closed += new EventHandler(_window_Closed);
        }

        void _window_Closed(object sender, EventArgs e)
        {
            _window.Close();
        }

        public void Dispose()
        {
        }

        public void run()
        {
            while (_window.IsOpen())
            {
                update();
                draw();
            }
        }

        public void update()
        {
            _window.DispatchEvents();
        }

        public void draw()
        {
            _window.Clear(Color.Black);
            _window.Display();
        }
    }
}
