using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Interface to implement the managing of input.
    /// </summary>
    public interface IInputReceivable
    {
        /// <summary>
        /// Method to put all the input updates.
        /// </summary>
        /// <param name="inputManager">The input manager used for the input updates.</param>
        void UpdateInput(Management.Input.InputManager inputManager);
    }
}
