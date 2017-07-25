using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Components;
using PoncheToolkit.Util;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using PoncheToolkit.Graphics3D.Effects;
using SharpDX.WIC;

namespace PoncheToolkit.Graphics3D.Primitives
{
    /// <summary>
    /// Draw a simple triangle.
    /// </summary>
    public class Triangle : Primitive
    {
        /// <inheritdoc/>
        public override event OnInitializedHandler OnInitialized;
        /// <inheritdoc/>
        public override event OnFinishLoadContentHandler OnFinishLoadContent;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public Triangle(Game11 game)
            : base(game)
        {
        }

        #region Public Methods
        /// <inheritdoc/>
        public override void Initialize()
        {
            Game.Form.KeyDown += Form_KeyDown;

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void LoadContent()
        {
            // Add the indices.
            //AddIndex(0);
            //AddIndex(1);
            //AddIndex(2);

            // --- Triangle

            // Position Color
            //AddVertex(new VertexPositionColorStruct(new Vector3(0.0f, 0.5f, 0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)));
            //AddVertex(new VertexPositionColorStruct(new Vector3(0.5f, -0.5f, 0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f)));
            //AddVertex(new VertexPositionColorStruct(new Vector3(-0.5f, -0.5f, 0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)));


            // Position Texture
            //AddVertex(new VertexPositionTextureStruct(new Vector3(0.0f, 0.5f, 0f), new Vector2(0f, 1)));
            //AddVertex(new VertexPositionTextureStruct(new Vector3(0.5f, -0.5f, 0f), new Vector2(1f, 0f)));
            //AddVertex(new VertexPositionTextureStruct(new Vector3(-0.5f, -0.5f, 0f), new Vector2(0f, 0f)));


            // Lighting
            AddVertex(new VertexMainStruct(new Vector3(0.0f, 0.5f, 0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector2(0f, 0f), new Vector3(0, 0, -1)));
            AddVertex(new VertexMainStruct(new Vector3(0.5f, -0.5f, 0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), new Vector2(1f, 0f), new Vector3(0, 0, -1)));
            AddVertex(new VertexMainStruct(new Vector3(-0.5f, -0.5f, 0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), new Vector2(0.5f, 1f), new Vector3(0, 0, -1)));

            base.LoadContent();

            OnFinishLoadContent?.Invoke();
        }

        /// <inheritdoc/>
        public override void Update()
        {
            
        }

        /// <inheritdoc/>
        public override void Render()
        {
            base.Render();
        }

        private float moveSpeed = 4f;
        private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (!AcceptInput)
            {
                e.Handled = true;
                return;
            }

            if (e.KeyCode == System.Windows.Forms.Keys.Left)
                Position.X -= moveSpeed * Game.GameTime.DeltaTime;

            if (e.KeyCode == System.Windows.Forms.Keys.Right)
                Position.X += moveSpeed * Game.GameTime.DeltaTime;

            if (e.KeyCode == System.Windows.Forms.Keys.Down)
                Position.Y -= moveSpeed * Game.GameTime.DeltaTime;

            if (e.KeyCode == System.Windows.Forms.Keys.Up)
                Position.Y += moveSpeed * Game.GameTime.DeltaTime;

            if (e.KeyCode == System.Windows.Forms.Keys.PageUp)
                Size += new Vector3(0.1f, 0.1f, 0.1f);

            if (e.KeyCode == System.Windows.Forms.Keys.PageDown)
                Size -= new Vector3(0.1f, 0.1f, 0.1f);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Game.Form.KeyDown -= Form_KeyDown;
            UnloadContent();
            base.Dispose();
        }
        #endregion
    }
}
