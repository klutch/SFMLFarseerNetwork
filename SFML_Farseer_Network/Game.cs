using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;
using SFML_Farseer_Network.Managers;

namespace SFML_Farseer_Network
{
    using Key = Keyboard.Key;

    public enum GameState
    {
        Setup,
        Waiting,
        Ready
    }

    public class Game : IDisposable
    {
        private RenderWindow _window;
        private Font _font;
        private Text _title;
        private List<Text> _messages;
        private GameState _state;
        private List<Text> _setupOptions;
        private KeyboardState _newKeyState;
        private KeyboardState _oldKeyState;
        private NetManager _netManager;
        private PhysicsManager _physicsManager;
        private CameraManager _cameraManager;
        private Text _ipPrompt;
        private Text _ipAddressText;
        private string _ipAddressValue;

        public Game()
        {
            _state = GameState.Setup;
            _window = new RenderWindow(new VideoMode(800, 600), "Farseer Network Test");
            _window.Closed += new EventHandler(_window_Closed);
            _netManager = new NetManager(this);
            _ipAddressValue = "127.0.0.1";

            loadContent();
        }

        void _window_Closed(object sender, EventArgs e)
        {
            _window.Close();
        }

        public void Dispose()
        {
        }

        private void loadContent()
        {
            _font = new Font(@"resources\courbd.ttf");
            _messages = new List<Text>();
            _title = new Text("SFML_Farseer_Network", _font, 18);
            _title.Color = Color.White;
            _title.Position = new Vector2f(16, 16);

            Text clientOption = new Text("1. Start Client", _font, 14);
            clientOption.Position = new Vector2f(340, 300);
            clientOption.Color = Color.Red;

            Text serverOption = new Text("2. Start Server", _font, 14);
            serverOption.Position = new Vector2f(340, 332);
            serverOption.Color = Color.Red;

            _setupOptions = new List<Text>();
            _setupOptions.Add(clientOption);
            _setupOptions.Add(serverOption);

            _ipPrompt = new Text("Please enter an IP address:", _font, 14);
            _ipPrompt.Position = new Vector2f(340, 300);
            _ipPrompt.Color = Color.Red;

            _ipAddressText = new Text("", _font, 18);
            _ipAddressText.Position = new Vector2f(340, 332);
            _ipAddressText.Color = Color.Green;
        }

        public void run()
        {
            while (_window.IsOpen())
            {
                update();
                draw();
            }
        }

        public void addMessage(string str)
        {
            Text text = new Text(str, _font, 14);

            text.Color = Color.Yellow;
            _messages.Add(text);
        }

        public void startGame()
        {
            addMessage("Starting game...");
            _state = GameState.Ready;
            _cameraManager = new CameraManager(this);
            _physicsManager = new PhysicsManager(this);
        }

        public void update()
        {
            _window.DispatchEvents();
            _oldKeyState = _newKeyState;
            _newKeyState = KeyboardState.get();

            if (_state == GameState.Setup)
            {
                if (_newKeyState.isPressed(Key.Num1) && _oldKeyState.isReleased(Key.Num1))
                {
                    _netManager.startClient();
                    _state = GameState.Waiting;
                }
                else if (_newKeyState.isPressed(Key.Num2) && _oldKeyState.isReleased(Key.Num2))
                {
                    _netManager.startServer();
                    _state = GameState.Waiting;
                }
            }
            else if (_state == GameState.Waiting)
            {
                if (_netManager.role == NetRole.Client)
                {
                    if (!_netManager.connected)
                    {
                        // 0-9
                        for (int i = 0; i < 10; i++)
                        {
                            Key key = (Key)(i + 26);
                            if (_newKeyState.isPressed(key) && _oldKeyState.isReleased(key))
                            {
                                _ipAddressValue += i.ToString();
                            }
                        }

                        // .
                        if (_newKeyState.isPressed(Key.Period) && _oldKeyState.isReleased(Key.Period))
                        {
                            _ipAddressValue += ".";
                        }

                        // Backspace
                        if (_newKeyState.isPressed(Key.Back) && _oldKeyState.isReleased(Key.Back))
                        {
                            if (_ipAddressValue.Length > 0)
                            {
                                _ipAddressValue = _ipAddressValue.Substring(0, _ipAddressValue.Length - 1);
                            }
                        }

                        // Enter
                        if (_newKeyState.isPressed(Key.Return) && _oldKeyState.isReleased(Key.Return))
                        {
                            _netManager.connectTo(_ipAddressValue);
                            addMessage("Attempting to connect to " + _ipAddressValue + ":3456...");
                        }

                        _ipAddressText.DisplayedString = _ipAddressValue;
                    }
                    else
                    {
                        startGame();
                    }
                }
                else    // NetRole.Server
                {
                    if (_netManager.connected)
                    {
                        startGame();
                    }
                }
            }
            else if (_state == GameState.Ready)
            {
            }
        }

        public void draw()
        {
            _window.Clear(Color.Black);

            // Draw title
            _window.Draw(_title);

            // Draw messages
            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                Vector2f position = new Vector2f(16, (i + 1) * 16 + 32);
                Text message = _messages[i];

                message.Position = position;
                _window.Draw(message);
            }

            if (_state == GameState.Setup)
            {
                for (int i = 0; i < _setupOptions.Count; i++)
                {
                    _window.Draw(_setupOptions[i]);
                }
            }
            else if (_state == GameState.Waiting)
            {
                if (_netManager.role == NetRole.Client)
                {
                    _window.Draw(_ipPrompt);
                    _window.Draw(_ipAddressText);
                }

                _netManager.processIncomingMessages();
            }
            else if (_state == GameState.Ready)
            {
            }

            _window.Display();
        }
    }
}
