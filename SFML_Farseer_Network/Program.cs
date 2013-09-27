using System;

namespace SFML_Farseer_Network
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (Game game = new Game())
            {
                game.run();
            }
        }
    }
}
