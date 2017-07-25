using PoncheToolkit.Core.Management.Screen;
using PoncheToolkit.Core.Services;
using PoncheToolkit.Graphics3D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core.Management.Threading
{
    /// <summary>
    /// Class in charge of managing the multi-threading rendering of the game.
    /// </summary>
    public class ThreadingManager
    {
        private static int currentRenderingThreads = 8;

        /// <summary>
        /// Max number of used threads for rendering.
        /// </summary>
        public const int MAX_RENDERING_THREADS_COUNT = 16;

        /// <summary>
        /// Get or set the current number of used threads for rendering.
        /// Default: 8.
        /// </summary>
        public static int CURRENT_RENDERING_THREADS_COUNT
        {
            get { return currentRenderingThreads; }
            set { currentRenderingThreads = Math.Max(1, Math.Min(value, MAX_RENDERING_THREADS_COUNT)); }
        }

        /// <summary>
        /// Get or set the current number of used threads for updating logic in a given screen.
        /// Default: 8.
        /// </summary>
        private const int MAX_UPDATING_THREADS_COUNT = 8;


        public static Dictionary<GameScreen, Task[]> UpdatingTaks { get; set; }

        static ThreadingManager()
        {
            UpdatingTaks = new Dictionary<GameScreen, Task[]>();
        }

        public static void WaitUpdatingTasks(GameScreen screen)
        {
            if (UpdatingTaks.ContainsKey(screen))
                Task.WaitAll(UpdatingTaks[screen]);
        }

        public static void WaitUpdatingTasksAll()
        {
            foreach (GameScreen screen in UpdatingTaks.Keys)
                Task.WaitAll(UpdatingTaks[screen]);
        }
    }
}
