using PoncheToolkit.Core.Services;
using PoncheToolkit.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core.Management.Menu
{
    /// <summary>
    /// Class that help to create and manage custom menus
    /// </summary>
    public class MenuManager : GameService
    {
        /// <inheritdoc/>
        public MenuManager(Game11 game) 
            : base(game)
        {
        }

        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;

        /// <inheritdoc/>
        public override void UpdateLogic(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
