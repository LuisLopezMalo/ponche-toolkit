using PoncheToolkit.Core;
using PoncheToolkit.Util;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Graphics3D.Primitives;
using System.Diagnostics;
using PoncheToolkit.Core.Services;

namespace PoncheToolkit.Graphics3D.Effects
{
    /// <summary>
    /// Class that maps the <see cref="LightStruct"/> functionality so it can be used as reference.
    /// </summary>
    public class PTLightTorch : PTLight
    {
        Stopwatch timer;
        Random rand;

        #region Properties

        #endregion

        #region Initialization
        /// <summary>
        /// Constructor. Create default values.
        /// </summary>
        /// <param name="index">The index of the light to be retrieved from the lights array.</param>
        public PTLightTorch()
        {
            timer = new Stopwatch();
            timer.Start();
            rand = new Random();
            
            this.MaxIntensity = 5f;
            this.MinIntensity = 0.1f;
        }
        #endregion
        
        /// <inheritdoc/>
        public override bool UpdateState()
        {
            return base.UpdateState();
        }

        float elapsed;
        int randomNumber;
        bool takeRandom = true;
        /// <summary>
        /// Updates the logic and rendering properties of the light.
        /// </summary>
        public override void UpdateLogic(GameTime gameTime)
        {
            elapsed += (float)gameTime.GameTimeElapsed.TotalMilliseconds;
            if (takeRandom)
            {
                randomNumber = rand.Next(1800);
                takeRandom = false;
            }
            if (elapsed > randomNumber)
            {
                float tempIntensity = (float)(rand.NextDouble() * rand.Next(-1, 2)) * 0.03f;
                
                this.Intensity = Math.Max(MinIntensity, Math.Min(MaxIntensity, Intensity + tempIntensity));
                tempIntensity = 0;
                elapsed = 0;
                takeRandom = true;
            }
        }
    }
}
