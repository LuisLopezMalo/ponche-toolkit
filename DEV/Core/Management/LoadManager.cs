using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core.Management
{
    /// <summary>
    /// Class that manages the loading of any component, screen, service, etc. in a different Thread.
    /// </summary>
    public class LoadManager
    {
        #region Fields
        public Task tasks;
        #endregion

        #region Properties
        public delegate void OnStartLoadingHandler();
        public event OnStartLoadingHandler OnStartLoading;

        public delegate void OnFinishLoadingHandler();
        public event OnFinishLoadingHandler OnFinishLoading;
        #endregion

        public void StartLoading()
        {
            OnStartLoading?.Invoke();
        }
    }
}
