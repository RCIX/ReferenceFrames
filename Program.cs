using System;

namespace ReferenceFrames
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            bool noPostProcessing = false;
            if (args.Length > 0 && args[0] == "-nopostprocessing")
            {
                noPostProcessing = true;
            }
            using (MainGame game = new MainGame(noPostProcessing))
            {
                game.Run();
            }
        }
    }
}

