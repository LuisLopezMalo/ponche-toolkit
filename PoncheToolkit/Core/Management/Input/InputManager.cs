using PoncheToolkit.Core.Services;
using SharpDX.DirectInput;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core.Management.Input
{
    /// <summary>
    /// Class that manage all the input from mouse, keyboard or peripherals.
    /// </summary>
    public class InputManager : GameService
    {
        #region Fields
        private DirectInput directInput;
        private List<Controller> controllers;
        private Keyboard keyboard;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override event OnInitializedHandler OnInitialized;

        /// <inheritdoc/>
        public new Game11 Game { get; set; }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public InputManager(Game11 game)
            : base(game)
        {
            this.Game = game;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            directInput = new DirectInput();
            keyboard = new Keyboard(directInput);
            controllers = new List<Controller>();

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void Update()
        {
            
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            
        }
    }
}
