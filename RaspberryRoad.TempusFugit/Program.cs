using System;

namespace RaspberryRoad.TempusFugit
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (TempusFugitGame game = new TempusFugitGame())
            {
                game.Run();
            }
        }
    }
}

