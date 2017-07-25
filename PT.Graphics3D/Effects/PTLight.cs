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

namespace PT.Graphics3D.Effects
{
    /// <summary>
    /// Class that maps the <see cref="LightStruct"/> functionality so it can be used as reference.
    /// </summary>
    public class PTLight : UpdatableStateObject
    {
        private Vector4 color;
        private Vector3 position;
        private Vector3 direction;
        private float spotAngle;
        private float constantAttenuation;
        private float linearAttenuation;
        private float quadraticAttenuation;
        private float intensity;
        private float range;
        private LightType type;
        private bool isEnabled;
        private int index;
        private PTModel debugModel;

        internal LightStruct LightBuffer;

        /// <summary>
        /// The maximun lights per effect.
        /// The Engine accepts this fixed number of lights per Effect by default.
        /// <para>
        /// Default: 16.
        /// </para>
        /// </summary>
        public const int MAX_LIGHTS = 16;

        /// <inheritdoc/>
        public event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;

        #region Properties
        /// <summary>
        /// The model to be rendered where the light is.
        /// This will be null until the <see cref="LoadDebugContent(Game11)"/> method is called.
        /// For release, the lights should not call the <see cref="LoadDebugContent(Game11)"/> method.
        /// </summary>
        public PTModel DebugModel
        {
            get { return debugModel; }
        }

        /// <summary>
        /// The index of the light in the lights array.
        /// </summary>
        public int Index
        {
            get { return index; }
            set { SetPropertyAsDirty(ref index, value); }
        }

        /// <summary>
        /// The position of the light.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set
            {
                SetPropertyAsDirty(ref position, value);
                LightBuffer.Position = new Vector4(position.X, position.Y, position.Z, 1);
            }
        }
        /// <summary>
        /// The direction of the light.
        /// </summary>
        public Vector3 Direction
        {
            get { return direction; }
            set
            {
                SetPropertyAsDirty(ref direction, value);
                LightBuffer.Direction = new Vector4(direction.X, direction.Y, direction.Z, 1);
            }
        }
        /// <summary>
        /// Color of the Light.
        /// </summary>
        public Vector4 Color
        {
            get { return color; }
            set
            {
                SetPropertyAsDirty(ref color, value);
                LightBuffer.Color = color;
            }
        }
        /// <summary>
        /// Get or set the angle for the spot lights.
        /// Bigger the value, the cone gets narrower.
        /// <para>
        /// Default: 45. (This is the radius, make a 90 degree spot light).
        /// </para>
        /// </summary>
        public float SpotAngle
        {
            get { return spotAngle; }
            set
            {
                SetPropertyAsDirty(ref spotAngle, value);
                LightBuffer.SpotAngle = spotAngle;
            }
        }
        /// <summary>
        /// Get or set the attenuation for the <see cref="LightType.Point"/> and <see cref="LightType.Spot"/> types.
        /// <para>
        /// Default: 1
        /// </para>
        /// </summary>
        public float ConstantAttenuation
        {
            get { return constantAttenuation; }
            set
            {
                SetPropertyAsDirty(ref constantAttenuation, value);
                LightBuffer.ConstantAttenuation = constantAttenuation;
            }
        }
        /// <summary>
        /// Get or set the attenuation for the <see cref="LightType.Point"/> and <see cref="LightType.Spot"/> types.
        /// <para>
        /// Default: 0.08
        /// </para>
        /// </summary>
        public float LinearAttenuation
        {
            get { return linearAttenuation; }
            set
            {
                SetPropertyAsDirty(ref linearAttenuation, value);
                LightBuffer.LinearAttenuation = linearAttenuation;
            }
        }
        /// <summary>
        /// Get or set the attenuation for the <see cref="LightType.Point"/> and <see cref="LightType.Spot"/> types.
        /// <para>
        /// Default: 0
        /// </para>
        /// </summary>
        public float QuadraticAttenuation
        {
            get { return quadraticAttenuation; }
            set
            {
                SetPropertyAsDirty(ref quadraticAttenuation, value);
                LightBuffer.QuadraticAttenuation = quadraticAttenuation;
            }
        }
        /// <summary>
        /// Get or set the intensity of the light.
        /// Default: 1
        /// </summary>
        public float Intensity
        {
            get { return intensity; }
            set
            {
                SetPropertyAsDirty(ref intensity, value);
                LightBuffer.Intensity = intensity;
            }
        }
        /// <summary>
        /// Get or set range of the light before it is unseeable.
        /// Default: 10
        /// </summary>
        public float Range
        {
            get { return range; }
            set
            {
                SetPropertyAsDirty(ref range, value);
                LightBuffer.Range = range;
            }
        }
        /// <summary>
        /// Get or set the <see cref="LightType"/> of the light.
        /// Default: <see cref="LightType.Directional"/>
        /// </summary>
        public LightType Type
        {
            get { return type; }
            set
            {
                SetPropertyAsDirty(ref type, value);
                LightBuffer.Type = (int)type;
            }
        }
        /// <summary>
        /// Get or set if the light is enabled to be rendered in GPU.
        /// Default: true.
        /// </summary>
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                SetPropertyAsDirty(ref isEnabled, value);
                LightBuffer.IsEnabled = isEnabled == true ? 1 : 0;
            }
        }

        /// <inheritdoc/>
        public bool IsContentLoaded { get; set; }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor. Create default values.
        /// </summary>
        /// <param name="index">The index of the light to be retrieved from the lights array.</param>
        public PTLight(int index)
        {
            this.Color = Vector4.One;
            this.Position = Vector3.Zero;
            this.Direction = new Vector3(0, 0, 1);
            this.SpotAngle = 45;
            this.ConstantAttenuation = 1;
            this.LinearAttenuation = 0.08f;
            this.QuadraticAttenuation = 0;
            this.Intensity = 1;
            this.Range = 10;
            this.type = LightType.Directional;
            this.isEnabled = true;

            this.index = index;
        }
        #endregion

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (IsStateUpdated)
                return IsStateUpdated;

            if (debugModel != null && DirtyProperties.ContainsKey(nameof(Position)))
            {
                debugModel.Position = this.position;
            }

            IsStateUpdated = true;
            OnStateUpdated();
            return IsStateUpdated;
        }

        /// <summary>
        /// This method should be used if the model for the light wants to be loaded.
        /// Will load a primitive to be rendered where the light is positioned.
        /// </summary>
        public void LoadDebugContent(Game11 game)
        {
            switch (type)
            {
                case LightType.Directional:
                    debugModel = new Cube(game);
                    break;
                case LightType.Point:
                    debugModel = new Sphere(game);
                    break;
                case LightType.Spot:
                    debugModel = new Triangle(game);
                    break;
            }
            
            debugModel.Size = new Vector3(0.4f);
            debugModel.Initialize();
            debugModel.LoadContent(game.ContentManager);
            OnFinishLoadContent?.Invoke();
        }
    }
}
