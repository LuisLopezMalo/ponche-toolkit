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
    public class Square : Primitive
    {
        private Texture2D texture;

        /// <inheritdoc/>
        public override event OnInitializedHandler OnInitialized;
        /// <inheritdoc/>
        public override event OnFinishLoadContentHandler OnFinishLoadContent;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public Square(Game11 game)
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
            if (HasTexture)
                texture = Game.ContentManager.Load<Texture2D>("crate1.jpg");

            // Add the indices.
            //AddIndex(0);
            //AddIndex(1);
            //AddIndex(2);

            // --- Square
            AddVertex(new VertexMainStruct(new Vector3(-0.5f, 0.5f, 0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector2(0f, 1f), new Vector3(0, 0, -1))); // Down - left
            AddVertex(new VertexMainStruct(new Vector3(0.5f, -0.5f, 0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector2(0.5f, 0f), new Vector3(0, 0, -1)));
            AddVertex(new VertexMainStruct(new Vector3(-0.5f, -0.5f, 0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector2(0f, 0f), new Vector3(0, 0, -1)));

            AddVertex(new VertexMainStruct(new Vector3(-0.5f, 0.5f, 0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector2(0f, 1f), new Vector3(0, 0, -1))); // Up - right
            AddVertex(new VertexMainStruct(new Vector3(0.5f, 0.5f, 0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector2(1f, 1f), new Vector3(0, 0, -1)));
            AddVertex(new VertexMainStruct(new Vector3(0.5f, -0.5f, 0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector2(1f, 0f), new Vector3(0, 0, -1)));

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
            if (HasTexture)
            {
                Game.Renderer.DeviceContext.PixelShader.SetSampler(0, Sampler);
                Game.Renderer.DeviceContext.PixelShader.SetShaderResource(0, texture.ShaderResourceView);
            }

            base.Render();
        }

        private float moveSpeed = 0.5f;
        private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (!AcceptInput)
            {
                e.Handled = true;
                return;
            }

            if (e.KeyCode == System.Windows.Forms.Keys.Left)
                Position.X -= moveSpeed;

            if (e.KeyCode == System.Windows.Forms.Keys.Right)
                Position.X += moveSpeed;

            if (e.KeyCode == System.Windows.Forms.Keys.Down)
                Position.Y -= moveSpeed;

            if (e.KeyCode == System.Windows.Forms.Keys.Up)
                Position.Y += moveSpeed;

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
