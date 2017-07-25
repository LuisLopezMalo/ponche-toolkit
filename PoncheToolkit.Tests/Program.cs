using System;
using PoncheToolkit.Core.Services;
using PoncheToolkit.Tests.GameTests;

namespace PoncheToolkit.Tests
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            TestGame game = new TestGame();
            game.Run();
        }
    }
}
