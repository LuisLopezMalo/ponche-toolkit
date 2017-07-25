using PoncheToolkit.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Interface that can update the logic of any component.
    /// </summary>
    public interface IUpdatableLogic
    {
        /// <summary>
        /// Main method to update the logic of any element.
        /// </summary>
        /// <param name="gameTime">The <see cref="GameTime"/> object that manages all the time elapsed in the game.</param>
        void UpdateLogic(GameTime gameTime);
    }
}
