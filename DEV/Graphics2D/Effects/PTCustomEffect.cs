using PoncheToolkit.Util;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Mathematics.Interop;
using SharpDX;
using PoncheToolkit.Graphics3D.Effects;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Management.Content;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace PoncheToolkit.Graphics2D.Effects
{
    /// <summary>
    /// Class that wraps the functionality of a Custom <see cref="SharpDX.Direct2D1.CustomEffect"/>.
    /// </summary>
    [CustomEffect("A custom effect", "CustomEffects", "PoncheToolkit", DisplayName = "Custom effect")]
    [CustomEffectInput("Source")]
    public abstract class PTCustomEffect : CustomEffectBase, DrawTransform, IUpdatableState, IUpdatableProperties, ILoggable
    {
        #region Implemented effects GUIDs
        /// <summary>
        /// The GUID to initialize this effect.
        /// </summary>
        public static readonly Guid GUID_ShadowMapEffect = Guid.NewGuid();

        /// <summary>
        /// The GUID to initialize this effect.
        /// </summary>
        public static readonly Guid GUID_RippleEffect = Guid.NewGuid();
        #endregion

        private string name;
        private bool isRegistered;
        private string pixelShaderPath;
        private Effect effect;
        private DrawInformation drawInformation;

        #region Properties
        /// <summary>
        /// Get or set the name of the effect.
        /// By default set the name to its class name.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Get if the Effect has been registered calling the <see cref="Register(Factory1, DeviceContext)"/> method.
        /// The specific implementantion of that method must set this value.
        /// </summary>
        public bool IsRegistered
        {
            get { return isRegistered; }
            set { isRegistered = value; }
        }

        /// <summary>
        /// Get or set the path for the pixel shader.
        /// </summary>
        public string PixelShaderPath
        {
            get { return pixelShaderPath; }
            set { pixelShaderPath = value; }
        }

        /// <summary>
        /// Get or set the assigned guid for this effect
        /// </summary>
        public abstract Guid Guid { get; set; }

        /// <summary>
        /// Get or set the effect used to be passed when rendering textures.
        /// This must be initialized in the <see cref="Register(Factory1, DeviceContext)"/> method.
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        /// <summary>
        /// Get the drawing information.
        /// </summary>
        public DrawInformation DrawInfo
        {
            get { return drawInformation; }
        }

        /// <inheritdoc/>
        public virtual int InputCount { get { return 1; } }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public PTCustomEffect()
        {
            name = this.GetType().Name;

            Log = new Logger(GetType());
            dirtyProperties = new Dictionary<string, object>();
            disposableObjects = new List<IDisposable>();

            // By default when a property is set, the IsUpdated property change to false.
            OnPropertyChangedEvent += (sender, e) =>
            {
                IsStateUpdated = false;
            };

            // Clear the dirty properties when the OnStateUpdated event has been called.
            OnStateUpdatedEvent += (sender, e) =>
            {
                dirtyProperties.Clear();
                Game11.RemoveDirtyObject(this);
            };
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PTCustomEffect(string pixelShaderPath)
            : this()
        {
            this.pixelShaderPath = pixelShaderPath;
        }

        /// <summary>
        /// Set the properties for rendering. By default this method use the <see cref="DrawInformation.SetPixelShader(Guid, PixelOptions)"/> for the given guid.
        /// Drawing info can be set like the properties of <see cref="InputDescription"/>.
        /// </summary>
        /// <param name="drawInfo"></param>
        public virtual void SetDrawProperties(DrawInformation drawInfo)
        {
            if (this.Guid == Guid.Empty)
                throw new ArgumentNullException("The GUID has not been assigned.");

            drawInfo.SetPixelShader(this.Guid, PixelOptions.None);
            drawInfo.SetInputDescription(0, new InputDescription(Filter.MinimumPointMagLinearMipPoint, 1));
        }

        /// <summary>
        /// Method used to update the constants sent to the effect.
        /// Here the specific created Struct must be updated to the <see cref="DrawInfo"/> SetPixelConstantBuffer, SetVertexConstantBuffer, etc.
        /// This method is called inside the <see cref="PrepareForRender(ChangeType)"/>
        /// </summary>
        /// <param name="changeType">The type that suffered a change.</param>
        public abstract void UpdateBuffer(ChangeType changeType);

        /// <summary>
        /// Register the effect using the given factory.
        /// This method must be manually called before using the effect.
        /// </summary>
        /// <param name="factory">The <see cref="Factory1"/> used to register the effect.</param>
        /// <param name="context">The <see cref="DeviceContext"/> used to render the effect.</param>
        public abstract void Register(Factory1 factory, DeviceContext context);

        /// <inheritdoc/>
        public override abstract void Initialize(EffectContext effectContext, TransformGraph transformGraph);

        /// <inheritdoc/>
        public override void PrepareForRender(ChangeType changeType)
        {
            UpdateBuffer(changeType);
        }

        /// <inheritdoc/>
        public virtual void SetDrawInformation(DrawInformation drawInfo)
        {
            this.drawInformation = drawInfo;
            SetDrawProperties(drawInformation);
        }

        /// <inheritdoc/>
        public virtual void MapOutputRectangleToInputRectangles(RawRectangle outputRect, RawRectangle[] inputRects)
        {
            if (inputRects.Length != 1)
                throw new ArgumentException("InputRects must be length of 1", "inputRects");

            if (name.ToLower() == "PTShadowRenderEffect".ToLower())
                Console.Write("algo");

            inputRects[0].Left = outputRect.Left;
            inputRects[0].Top = outputRect.Top;
            inputRects[0].Right = outputRect.Right;
            inputRects[0].Bottom = outputRect.Bottom;
        }

        /// <inheritdoc/>
        public virtual RawRectangle MapInputRectanglesToOutputRectangle(RawRectangle[] inputRects, RawRectangle[] inputOpaqueSubRects, out RawRectangle outputOpaqueSubRect)
        {
            if (inputRects.Length != 1)
                throw new ArgumentException("InputRects must be length of 1", "inputRects");
            outputOpaqueSubRect = default(Rectangle);
            return inputRects[0];
        }

        /// <inheritdoc/>
        public virtual RawRectangle MapInvalidRect(int inputIndex, RawRectangle invalidInputRect)
        {
            return invalidInputRect;
        }

        /// <summary>
        /// Operator to implicitly retrieve the <see cref="Effect{T}"/> object from the <see cref="PTCustomEffect"/>.
        /// </summary>
        /// <param name="effect"></param>
        public static implicit operator Effect (PTCustomEffect effect)
        {
            return effect.Effect;
        }


        #region ============ Updatable Object copy
        #region Fields
        private Dictionary<string, object> dirtyProperties;
        private bool isUpdated;
        private Game11 game;
        private List<IDisposable> disposableObjects;
        #endregion

        #region Properties
        /// <inheritdoc/>
        //public List<string> DirtyProperties { get { return dirtyProperties; } set { SetPropertyAsDirty(ref dirtyProperties, value); } }
        public Dictionary<string, object> DirtyProperties { get { return dirtyProperties; } set { SetPropertyAsDirty(ref dirtyProperties, value); } }

        /// <inheritdoc/>
        public bool IsStateUpdated { get { return isUpdated; } set { isUpdated = value; } }

        /// <inheritdoc/>
        public Logger Log { get; set; }
        #endregion

        #region Events
        /// <inheritdoc/>
        public virtual event EventHandler OnPropertyChangedEvent;
        /// <inheritdoc/>
        public virtual event EventHandler OnStateUpdatedEvent;
        #endregion


        #region Public Methods
        /// <inheritdoc/>
        public virtual bool UpdateState()
        {
            if (!IsStateUpdated)
            {
                IsStateUpdated = true;
                OnStateUpdatedEvent?.Invoke(this, null);
            }

            return IsStateUpdated;
        }

        /// <summary>
        /// Add an element to the list of objects that will be destroyed when the Dispose method is called.
        /// </summary>
        /// <param name="disposable"></param>
        public T ToDispose<T>(T disposable) where T : IDisposable
        {
            if (disposable != null && !disposableObjects.Contains(disposable))
                this.disposableObjects.Add(disposable);

            return disposable;
        }

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// If the property is new, it will be set as dirty, and added using the <see cref="Game11.AddDirtyObject(IUpdatableState)"/> method.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="field">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">
        ///     Name of the property used to notify listeners.  This
        ///     value is optional and can be provided automatically when invoked from compilers that
        ///     support CallerMemberName.
        /// </param>
        /// <param name="addDirtyObject">Set if when changing a property, it adds this UpdatableObject to the list of DirtyObjects
        /// from the <see cref="Game11"/> instance. There the dirty objects will automatically update their dirty properties.
        /// This can be set to false when changing properties in the <see cref="Game11"/> class, so it is not looped inside the UpdateState method.</param>
        /// <returns>
        ///     True if the value was changed, false if the existing value matched the
        ///     desired value.
        /// </returns>
        protected bool SetPropertyAsDirty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null, bool addDirtyObject = true)
        {
            if (Equals(field, value))
                return false;

            field = value;
            this.addDirtyProperty(propertyName, ref field, addDirtyObject);
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Just set the property without any other validation or notification.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="field">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">
        ///     Name of the property used to notify listeners.  This
        ///     value is optional and can be provided automatically when invoked from compilers that
        ///     support CallerMemberName.
        /// </param>
        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            field = value;
            this.OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Method to call the property changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(object sender = null, [CallerMemberName] string propertyName = null)
        {
            OnPropertyChangedEvent?.Invoke(sender == null ? this : sender, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Method to call the State Updated event.
        /// This event add the object to the DirtyObjects of the <see cref="Game"/> class.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnStateUpdated(EventArgs args = null)
        {
            OnStateUpdatedEvent?.Invoke(this, args);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            dirtyProperties.Clear();

            for (int i = 0; i < disposableObjects.Count; i++)
            {
                IDisposable disposable = disposableObjects[i];
                try
                {
                    if (disposable != null)
                        disposable.Dispose();
                    Utilities.Dispose(ref disposable);
                }
                catch (Exception ex)
                {
                    Log.Error("Error disposing object. {0}", ex, disposable);
                }
            }

            disposableObjects.Clear();
        }

        /// <summary>
        /// Clone the object calling the native <see cref="object.MemberwiseClone"/> method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T Clone<T>()
        {
            return (T)this.MemberwiseClone();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Add a new dirty property to the properties that will be updated the next time the UpdateState is called.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="addDirtyObject">See the description from the <see cref="SetPropertyAsDirty{T}(ref T, T, string, bool)"/> </param>
        /// <param name="newValue">The new value of the property.</param>
        private void addDirtyProperty<T>(string propertyName, ref T newValue, bool addDirtyObject)
        {
            if (!dirtyProperties.ContainsKey(propertyName))
            {
                dirtyProperties.Add(propertyName, newValue);

                if (addDirtyObject)
                    Game11.AddDirtyObject(this);
            }
        }
        #endregion
        #endregion
    }
}
