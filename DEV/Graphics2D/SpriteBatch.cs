using Core.Management.Text;
using PoncheToolkit.Core;
using PoncheToolkit.Core.Components;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Graphics2D.Effects;
using PoncheToolkit.Util;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;

namespace PoncheToolkit.Graphics2D
{
    /// <summary>
    /// Enumeration for different default Text Brushes colors.
    /// </summary>
    public enum TextBrushes
    {
        /// <summary>
        /// Black text
        /// </summary>
        Black = 0,
        /// <summary>
        /// White text.
        /// </summary>
        White,
        /// <summary>
        /// Yellow text.
        /// </summary>
        Yellow,
        /// <summary>
        /// Red text.
        /// </summary>
        Red
    }

    /// <summary>
    /// Class that manages the rendering for 2D objects and texts.
    /// </summary>
    public class SpriteBatch : GameComponent
    {
        #region Fields
        private SharpDX.DirectWrite.Factory1 writeFactory;
        private static Bitmap1 target;
        private Matrix3x2 transform;

        private FontCollection fontCollection;
        private CustomFontLoader fontLoader;

        private List<PTFont> fonts;
        private PathGeometry path;
        private GeometrySink sink;

        private SharpDX.Direct2D1.Factory1 d2dFactory;

        /// <summary>
        /// List with the default color brushes.
        /// </summary>
        private static List<SolidColorBrush> textBrushesList;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;
        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;

        /// <summary>
        /// The default text format.
        /// <para/> Size: 18dpi, Font-Family: Segoe-UI
        /// </summary>
        public static TextFormat DEFAULT_TEXT_FORMAT;

        /// <summary>
        /// The default text format.
        /// <para/> Size: 18dpi, Font-Family: Segoe-UI, Weight: Bold
        /// </summary>
        public static TextFormat DEFAULT_TEXT_FORMAT_BOLD;

        /// <summary>
        /// The default text format.
        /// <para/> Size: 10dpi, Font-Family: Segoe-UI
        /// </summary>
        public static TextFormat INFO_TEXT_FORMAT;

        /// <summary>
        /// The default text format.
        /// <para/> Size: 10dpi, Font-Family: Segoe-UI
        /// </summary>
        public static TextFormat WARNING_TEXT_FORMAT;

        /// <summary>
        /// The default text format.
        /// <para/> Size: 10dpi, Font-Family: Segoe-UI
        /// </summary>
        public static TextFormat ERROR_TEXT_FORMAT;

        /// <summary>
        /// The default text format.
        /// <para/> Size: 20dpi, Font-Family: Pozotwo
        /// </summary>
        public static TextFormat POZO_BOLD;

        /// <summary>
        /// Get the current used target by the <see cref="SharpDX.Direct2D1.DeviceContext"/>.
        /// </summary>
        public Image Target
        {
            get { return target; }
        }

        /// <summary>
        /// Get the current used target by the <see cref="SharpDX.Direct2D1.DeviceContext"/>.
        /// </summary>
        internal SharpDX.DirectWrite.Factory1 WriteFactory
        {
            get { return writeFactory; }
        }

        /// <summary>
        /// The factory associated to the SpriteBatch.
        /// </summary>
        public SharpDX.Direct2D1.Factory1 D2dFactory
        {
            get { return d2dFactory; }
            internal set { d2dFactory = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The game instance.</param>
#if DX11
        public SpriteBatch(Game11 game)
#elif DX12
        public SpriteBatch(Game12 game)
#endif
            : base(game)
        {
        }
        #endregion

        private Effect gaussian;
        private Effect transformEffect;
        private Effect atlas;
        private Effect specular;
        private Effect composite;
        private Effect arithmeticComposite;
        private Effect shadow;

        /// <inheritdoc/>
        public override void Initialize()
        {
            if (!IsInitialized)
            {
                textBrushesList = new List<SolidColorBrush>();
                fonts = new List<PTFont>();

                // Create the DirectWrite Factory for Fonts
                writeFactory = new SharpDX.DirectWrite.Factory1(SharpDX.DirectWrite.FactoryType.Shared);
                try
                {
                    fontLoader = new CustomFontLoader(Game, writeFactory);
                    fontLoader.Initialize();
                    fontCollection = new FontCollection(writeFactory, fontLoader, fontLoader.Key);

                    // TODO: tests
                    path = new PathGeometry(Game.Renderer.Context2D.Factory);
                    sink = path.Open();

                    Vector2 starting = new Vector2(Game.Settings.Resolution.Width - 210, Game.Settings.Resolution.Height - 210);
                    sink.BeginFigure(starting, FigureBegin.Filled);
                    sink.AddLine(new Vector2(starting.X + 200, starting.Y));
                    sink.AddBezier(new BezierSegment()
                    {
                        Point1 = new Vector2(starting.X + 150, starting.Y + 50),
                        Point2 = new Vector2(starting.X + 150, starting.Y + 150),
                        Point3 = new Vector2(starting.X + 200, starting.Y + 200),
                    });
                    sink.AddLine(new Vector2(starting.X + 0, starting.Y + 200));
                    sink.AddBezier(new BezierSegment()
                    {
                        Point1 = new Vector2(starting.X + 50, starting.Y + 150),
                        Point2 = new Vector2(starting.X + 50, starting.Y + 50),
                        Point3 = new Vector2(starting.X + 0, starting.Y + 0),
                    });
                    sink.EndFigure(FigureEnd.Closed);
                    sink.Close();


                    //int[] codePoints = null;
                    //GlyphRun run = new GlyphRun();
                    //run.FontFace = fontFace;
                    //run.Indices = fontFace.GetGlyphIndices(codePoints);
                    //Game.Renderer.Context2D.DrawGlyphRun(new SharpDX.Mathematics.Interop.RawVector2(position.X, position.Y), run, textBrushesList[(int)TextBrushes.White], MeasuringMode.GdiClassic);

                    //short[] indices = null;
                    //fact = Game.Renderer.Context2D.QueryInterface<SharpDX.Direct2D1.Factory1>();
                    //PathGeometry1 path = new PathGeometry1(fact);
                    //fontFace.GetGlyphRunOutline(ConvertPointSizeToDIP(20), indices, null, null, false, false, );
                    //fontFace.GetDesignGlyphMetrics()
                }
                catch (Exception ex)
                {
                    Log.Error("Error loading fonts", ex);
                }

                DEFAULT_TEXT_FORMAT = new TextFormat(writeFactory, "Segoe UI", 18);
                DEFAULT_TEXT_FORMAT_BOLD = new TextFormat(writeFactory, "Segoe UI", FontWeight.Bold, FontStyle.Normal, 18);
                INFO_TEXT_FORMAT = new TextFormat(writeFactory, "Segoe UI", 10);
                WARNING_TEXT_FORMAT = new TextFormat(writeFactory, "Segoe UI", 10);
                ERROR_TEXT_FORMAT = new TextFormat(writeFactory, "Segoe UI", 10);
                //textFormat = new TextFormat(writeFactory, "a song for jennifer", fontCollection, FontWeight.Normal, FontStyle.Normal, FontStretch.Normal, ConvertPointSizeToDIP(20))
                POZO_BOLD = new TextFormat(writeFactory, "pozotwo", fontCollection, FontWeight.Normal, FontStyle.Normal, FontStretch.Normal, ConvertPointSizeToDIP(20))
                {
                    TextAlignment = TextAlignment.Center,
                    ParagraphAlignment = ParagraphAlignment.Center
                };

                textBrushesList.Add(new SolidColorBrush(Game.Renderer.Context2D, Color.Black));
                textBrushesList.Add(new SolidColorBrush(Game.Renderer.Context2D, Color.White));
                textBrushesList.Add(new SolidColorBrush(Game.Renderer.Context2D, Color.Yellow));
                textBrushesList.Add(new SolidColorBrush(Game.Renderer.Context2D, Color.Red));
            }

            //Surface surface = null;
            //if (Game.IsInterop)
            //    surface = Game.RenderTargetView.QueryInterface<Surface>();
            //else
            //    surface = Game.Renderer.SwapChain.GetBackBuffer<Surface>(0);

            //target = new Bitmap1(Game.Renderer.Context2D, surface, Game.Renderer.BitmapProperties2D);

            //Game.Renderer.Context2D.Target = target;
            //Utilities.Dispose(ref surface);
            ////Utilities.Dispose(ref target);

            RecreateTarget();

            ToDispose(writeFactory);


            gaussian = new Effect(Game.Renderer.Context2D, Effect.GaussianBlur);
            transformEffect = new Effect(Game.Renderer.Context2D, Effect.AffineTransform2D);
            atlas = new Effect(Game.Renderer.Context2D, Effect.Atlas);
            specular = new Effect(Game.Renderer.Context2D, Effect.PointSpecular);
            composite = new Effect(Game.Renderer.Context2D, Effect.Composite);
            arithmeticComposite = new Effect(Game.Renderer.Context2D, Effect.ArithmeticComposite);
            shadow = new Effect(Game.Renderer.Context2D, Effect.Shadow);

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void LoadContent(IContentManager contentManager)
        {
            IsContentLoaded = true;
            OnFinishLoadContent?.Invoke();
        }

        /// <summary>
        /// Create a <see cref="SharpDX.Direct2D1.DeviceContext"/> that will be used to render offline content
        /// for Post processing. It also creates a default <see cref="Bitmap1"/> as the <see cref="SharpDX.Direct2D1.DeviceContext.Target"/>.
        /// </summary>
        /// <returns></returns>
        public SharpDX.Direct2D1.DeviceContext CreateOfflineDeviceContext()
        {
            SharpDX.Direct2D1.Device d2dDevice = null;
            Size2F dpi;
            BitmapProperties1 bitProperties;

            try
            {
                SharpDX.DXGI.Device1 dxgiDevice = Game.Renderer.Device.QueryInterface<SharpDX.DXGI.Device1>();
                SharpDX.Direct2D1.Factory2 d2dFactory2 = new SharpDX.Direct2D1.Factory2(SharpDX.Direct2D1.FactoryType.MultiThreaded, SharpDX.Direct2D1.DebugLevel.None);
                d2dDevice = new SharpDX.Direct2D1.Device1(d2dFactory2, dxgiDevice);

                dpi = d2dFactory2.DesktopDpi;
                bitProperties = new BitmapProperties1(new PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                    dpi.Width, dpi.Height, BitmapOptions.CannotDraw | BitmapOptions.Target);

                Utilities.Dispose(ref d2dFactory2);
                Utilities.Dispose(ref dxgiDevice);
            }
            catch (Exception ex)
            {
                Log.Warning("Interface Factory2 not supported. Trying with Factory1", ex);
                SharpDX.DXGI.Device1 dxgiDevice = Game.Renderer.Device.QueryInterface<SharpDX.DXGI.Device1>();
                SharpDX.Direct2D1.Factory1 d2dFactory1 = new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.MultiThreaded, DebugLevel.None);
                d2dDevice = new SharpDX.Direct2D1.Device(dxgiDevice);

                dpi = d2dFactory1.DesktopDpi;
                bitProperties = new BitmapProperties1(new PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                    dpi.Width, dpi.Height, BitmapOptions.CannotDraw | BitmapOptions.Target);

                Utilities.Dispose(ref d2dFactory1);
                Utilities.Dispose(ref dxgiDevice);
            }

            DeviceContext context = new DeviceContext(d2dDevice, DeviceContextOptions.EnableMultithreadedOptimizations);
            context.PrimitiveBlend = PrimitiveBlend.SourceOver;
            context.AntialiasMode = AntialiasMode.Aliased;
            Utilities.Dispose(ref d2dDevice);

            Bitmap1 d2dRenderTarget = new Bitmap1(context, new Size2(Game.Settings.Resolution.Width, Game.Settings.Resolution.Width), bitProperties);
            context.Target = d2dRenderTarget;

            Utilities.Dispose(ref d2dRenderTarget);

            return context;
        }

        /// <summary>
        /// Realease the Render Target so the Back buffer can be resized.
        /// </summary>
        internal void ReleaseTarget()
        {
            if (Game.Renderer.Context2D != null)
            {
                Utilities.Dispose(ref target);
                Game.Renderer.Context2D.Target = null;
            }
        }

        internal void RecreateTarget()
        {
#if DX11
            Surface surface = null;
            //if (Game.IsInterop)
            //    surface = Game.RenderTarget.RenderTarget.QueryInterface<Surface>();
            //else
            surface = Game.Renderer.SwapChain.GetBackBuffer<Surface>(0);

            target = new Bitmap1(Game.Renderer.Context2D, surface, Game.Renderer.BitmapProperties2D);

            Game.Renderer.Context2D.Target = target;
            Utilities.Dispose(ref surface);
            //Utilities.Dispose(ref target);
#endif
        }

        /// <summary>
        /// Begin the drawing of 2D content.
        /// It is recommended to call this the fewest as possible
        /// </summary>
        public void Begin()
        {
            transform = Matrix3x2.Identity;
            Begin(transform);
        }

        /// <summary>
        /// Begin the drawing of 2D content.
        /// It is recommended to call this the fewest as possible
        /// </summary>
        public void Begin(Bitmap1 renderTarget)
        {
            Game.Renderer.SetRenderTarget2D(renderTarget);
            transform = Matrix3x2.Identity;
            Begin(transform);
        }

        /// <summary>
        /// Begin the drawing of 2D content.
        /// It is recommended to call this the fewest as possible
        /// </summary>
        /// <param name="transform">Set the transform for the rendering. It can define translation, scale.</param>
        public void Begin(Matrix3x2 transform)
        {
            if (!Game.IsInterop)
                Game.Renderer.Context2D.BeginDraw();

            Game.Renderer.Context2D.Transform = transform;

            this.transform = transform;
        }

        /// <summary>
        /// End the drawing of 2D content.
        /// </summary>
        public void End()
        {
            if (!Game.IsInterop)
                Game.Renderer.Context2D.EndDraw();
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (!IsStateUpdated)
            {
                IsStateUpdated = true;
                OnStateUpdated();
            }

            return IsStateUpdated;
        }

        /// <summary>
        /// Convert points to DIP's for Fonts.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public float ConvertPointSizeToDIP(float points)
        {
            return (points / 72.0f) * 96.0f;
        }

        #region Draw methods

        #region Texture
        /// <summary>
        /// Draw a texture with the specified parameters.
        /// </summary>
        /// <param name="texture">Texture to be drawn.</param>
        /// <param name="position">Position of the texture.</param>
        /// <param name="opacity">Opacity of the texture in the range 0-1.</param>
        public void DrawTexture(PTTexture2D texture, Vector2 position, float opacity = 1)
        {
            this.DrawTexture(texture,
                position: position,
                sourceRectangle: new RawRectangleF(0, 0, texture.Size.X, texture.Size.Y),
                color: Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: Vector2.One,
                opacity: 1);
        }

        /// <summary>
        /// Draw a texture with the specified parameters.
        /// </summary>
        /// <param name="texture">Texture to be drawn.</param>
        /// <param name="position">Position of the texture.</param>
        /// <param name="scale">Scale of the texture from the original.</param>
        /// <param name="opacity">Opacity of the texture in the range 0-1.</param>
        public void DrawTexture(PTTexture2D texture, Vector2 position, Vector2 scale, float opacity = 1)
        {
            transformEffect.SetInputEffect(0, texture.BitmapSourceEffect, false);
            RawMatrix3x2 transf;
            SharpDX.Direct2D1.D2D1.MakeRotateMatrix(0, Vector2.Zero, out transf);
            //transf = Matrix3x2.Rotation(MathUtil.DegreesToRadians(rotation));
            //Vector2 newPos = position;
            transf = Matrix3x2.Multiply(transf, Matrix3x2.Translation(position));
            transf = Matrix3x2.Multiply(transf, Matrix3x2.Scaling(scale));
            transformEffect.SetValue((int)AffineTransform2DProperties.TransformMatrix, transf);

            //Game.Renderer.Context2D.DrawImage(transformEffect.Output, null, new RectangleF(position.X, position.Y, texture.Size.X * scale.X, texture.Size.Y * scale.Y),
            //    InterpolationMode.Linear, CompositeMode.SourceOver);

            //Game.Renderer.Context2D.DrawImage(transformEffect.Output, null, new RectangleF(position.X, position.Y, texture.Size.X * scale.X, texture.Size.Y * scale.Y),
            //    InterpolationMode.Linear, CompositeMode.SourceOver);

            Game.Renderer.Context2D.DrawImage(transformEffect);
        }

        /// <summary>
        /// Draw a texture with the specified parameters.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap1"/> to be drawn.</param>
        /// <param name="position">Position of the texture.</param>
        /// <param name="scale">Scale of the texture from the original.</param>
        /// <param name="opacity">Opacity of the texture in the range 0-1.</param>
        public void DrawTexture(Bitmap1 bitmap, Vector2 position, Vector2 scale, float opacity = 1)
        {
            Game.Renderer.Context2D.DrawBitmap(bitmap,
                new RectangleF(position.X, position.Y, bitmap.Size.Width * scale.X, bitmap.Size.Height * scale.Y),
                opacity,
                BitmapInterpolationMode.Linear);
        }

        /// <summary>
        /// Draw a texture with the specified parameters.
        /// </summary>
        /// <param name="texture">Texture to be drawn.</param>
        /// <param name="position">Position of the texture.</param>
        /// <param name="sourceRectangle">Rectangle to retrieve from the original texture.</param>
        /// <param name="opacity">Opacity of the texture in the range 0-1.</param>
        public void DrawTexture(PTTexture2D texture, Vector2 position, RectangleF sourceRectangle, float opacity = 1)
        {
            Game.Renderer.Context2D.DrawBitmap(texture.Bitmap,
                new RectangleF(position.X, position.Y, texture.Width, texture.Height),
                opacity,
                BitmapInterpolationMode.Linear,
                sourceRectangle);
        }

        private RawVector3 lightSpecular = new RawVector3(-0.8f, 0.7f, 1f);
        Vector3 direction = Vector3.One;

        /// <summary>
        /// Draw a texture with the specified parameters.
        /// </summary>
        /// <param name="texture">Texture to be drawn.</param>
        /// <param name="position">Position of the texture.</param>
        /// <param name="sourceRectangle">Rectangle to retrieve from the original texture.</param>
        /// <param name="color">The color to tint the texture.</param>
        /// <param name="rotation">The rotation of the texture.</param>
        /// <param name="origin">The origin where the rotation is made.</param>
        /// <param name="scale">The scaling of the texture.</param>
        /// <param name="opacity">Opacity of the texture in the range 0-1.</param>
        public void DrawTexture(PTTexture2D texture, Vector2 position, RawRectangleF sourceRectangle, Color color, float rotation,
            Vector2 origin, Vector2 scale, float opacity = 1)
        {
            //// Apply Source Rectangle selection.
            ////atlas.SetInput(0, texture.Bitmap, false);
            ////atlas.SetValue((int)AtlasProperties.InputRectangle, sourceRectangle);

            ////// Apply transform
            ////transformEffect.SetInputEffect(0, atlas, false);
            ////RawMatrix3x2 transf;
            ////SharpDX.Direct2D1.D2D1.MakeRotateMatrix(rotation, origin, out transf);
            //////transf = Matrix3x2.Rotation(MathUtil.DegreesToRadians(rotation));
            //////Vector2 newPos = position;
            ////transf = Matrix3x2.Multiply(transf, Matrix3x2.Translation(position));
            ////transformEffect.SetValue((int)AffineTransform2DProperties.TransformMatrix, transf);

            //// Apply Gaussian
            //gaussian.SetInputEffect(0, texture.BitmapSourceEffect, true);
            //gaussian.SetValue((int)GaussianBlurProperties.StandardDeviation, 2.4f);
            ////gaussian.SetInputEffect(0, transformEffect, false);

            //// Apply Specular
            ////specular.SetInput(0, texture.Bitmap, true);
            //specular.SetInputEffect(0, gaussian, true);
            ////if (lightSpecular.X >= 10)
            ////    direction.X = -1;
            ////else if (lightSpecular.X <= -10)
            ////    direction.X = 1;
            ////if (lightSpecular.Y >= 10)
            ////    direction.Y = -1;
            ////else if (lightSpecular.Y <= -10)
            ////    direction.Y = 1;
            ////lightSpecular.X += 0.06f * direction.X;
            ////lightSpecular.Y += 0.06f * direction.Y;
            //specular.SetValue((int)PointSpecularProperties.LightPosition, new RawVector3(sourceRectangle.Left + lightSpecular.X, sourceRectangle.Top + lightSpecular.Y, lightSpecular.Z));
            //specular.SetValue((int)PointSpecularProperties.SpecularExponent, 6f);
            //specular.SetValue((int)PointSpecularProperties.SpecularConstant, 3f);
            ////specular.SetValue((int)PointSpecularProperties.SurfaceScale, 0.1f);
            //specular.SetValue((int)PointSpecularProperties.SurfaceScale, 2f);


            //shadow.SetValue((int)ShadowProperties.BlurStandardDeviation, 5f);
            //shadow.SetValue((int)ShadowProperties.Color, new RawVector4(0.1f, 0.1f, 0.1f, 1f));
            //shadow.SetInput(0, texture.Bitmap, true);
            //shadow.SetInputEffect(1, transformEffect);


            //composite.SetInput(0, texture.Bitmap, true);
            //composite.SetInputEffect(1, specular);
            //composite.SetInputEffect(2, shadow);
            ////composite.SetValue((int)CompositeProperties.Mode, (int)CompositeMode.SourceIn);

            //arithmeticComposite.SetValue((int)ArithmeticCompositeProperties.Coefficients, new RawVector4(0f, 1f, 1f, 0.0f));
            //arithmeticComposite.SetInput(0, texture.Bitmap, true);
            //arithmeticComposite.SetInputEffect(1, composite);

            //// Apply Source Rectangle selection.
            //atlas.SetValue((int)AtlasProperties.InputRectangle, sourceRectangle);
            //atlas.SetInputEffect(0, arithmeticComposite);

            //// Apply transform
            //transformEffect.SetInputEffect(0, atlas, true);
            //RawMatrix3x2 transf;
            //SharpDX.Direct2D1.D2D1.MakeRotateMatrix(rotation, origin, out transf);
            ////transf = Matrix3x2.Rotation(MathUtil.DegreesToRadians(rotation));
            ////Vector2 newPos = position;
            //transf = Matrix3x2.Multiply(transf, Matrix3x2.Translation(position));
            //transf = Matrix3x2.Multiply(transf, Matrix3x2.Scaling(scale));
            //transformEffect.SetValue((int)AffineTransform2DProperties.TransformMatrix, transf);

            ////Game.Renderer.Context2D.DrawImage(gaussian.Output, position, sourceRectangle,
            ////    InterpolationMode.Cubic, CompositeMode.SourceAtop);
            ////Game.Renderer.Context2D.DrawImage(gaussian, position);
            //Game.Renderer.Context2D.DrawImage(transformEffect);


            //Game.Renderer.Context2D.DrawImage(gaussian.GetInput(0), position, sourceRectangle,
            //    InterpolationMode.Anisotropic, CompositeMode.SourceAtop);

            RawMatrix? perspective = Matrix.Multiply(Matrix.Identity, rotation);
            Game.Renderer.Context2D.DrawBitmap(texture.Bitmap,
                new RectangleF(position.X, position.Y, texture.Size.X * scale.X, texture.Size.Y * scale.Y),
                opacity,
                InterpolationMode.Anisotropic,
                sourceRectangle,
                perspective);
        }

        /// <summary>
        /// Draw a texture with the specified parameters.
        /// </summary>
        /// <param name="texture">Texture to be drawn.</param>
        /// <param name="effects">List of effects to be applied to the shader, the order is important. The input of each Effect will be the previous Effect.
        /// The first input is the texture to be drawn.</param>
        /// <param name="position">Position of the texture.</param>
        /// <param name="sourceRectangle">Rectangle to retrieve from the original texture.</param>
        /// <param name="color">The color to tint the texture.</param>
        /// <param name="rotation">The rotation of the texture.</param>
        /// <param name="origin">The origin where the rotation is made.</param>
        /// <param name="scale">The scaling of the texture.</param>
        /// <param name="opacity">Opacity of the texture in the range 0-1.</param>
        public void DrawTexture(PTTexture2D texture, List<Effect> effects, Vector2 position, RawRectangleF sourceRectangle, Color color, float rotation,
            Vector2 origin, Vector2 scale, float opacity = 1)
        {
            effects[0].SetInputEffect(0, texture.BitmapSourceEffect, false);
            for (int i = 1; i < effects.Count; i++)
            {
                effects[i].SetInputEffect(0, effects[i - 1]);
            }

            Game.Renderer.Context2D.DrawImage(effects[effects.Count - 1]);

            //shadowMap.SetInput(0, bitmapToDraw, true);
            //Game.Renderer.Context2D.DrawImage(shadowMap);
        }

        /// <summary>
        /// Draw a texture with the specified parameters.
        /// </summary>
        /// <param name="texture">Texture to be drawn.</param>
        /// <param name="position">Position of the texture.</param>
        /// <param name="sourceRectangle">Rectangle to retrieve from the original texture.</param>
        /// <param name="scale">Scale of the texture (0-1).</param>
        /// <param name="opacity">Opacity of the texture in the range 0-1.</param>
        public void DrawTexture(PTTexture2D texture, Vector2 position, RawRectangleF sourceRectangle, Vector2 scale, float opacity = 1)
        {
            // Apply Source Rectangle selection.
            atlas.SetValue((int)AtlasProperties.InputRectangle, sourceRectangle);
            atlas.SetInputEffect(0, texture.BitmapSourceEffect);

            // Apply transform
            transformEffect.SetInputEffect(0, atlas, true);
            RawMatrix3x2 transf = Matrix3x2.Identity;
            transf = Matrix3x2.Multiply(transf, Matrix3x2.Translation(position));
            transf = Matrix3x2.Multiply(transf, Matrix3x2.Scaling(scale));
            transformEffect.SetValue((int)AffineTransform2DProperties.TransformMatrix, transf);

            Game.Renderer.Context2D.DrawImage(transformEffect);

            //Game.Renderer.Context2D.DrawBitmap(texture.Bitmap, 
            //    new RectangleF(position.X, position.Y, scale.X, scale.Y),
            //    opacity,
            //    BitmapInterpolationMode.Linear, 
            //    sourceRectangle);
        }
        #endregion

        #region Text
        /// <summary>
        /// Draw text with the specified parameters.
        /// </summary>
        /// <param name="text">The text to be drawn.</param>
        /// <param name="position">Position of the text.</param>
        /// <param name="brushColor">Color of the text.</param>
        public void DrawText(string text, Vector2 position, TextBrushes brushColor)
        {
            //Game.Renderer.Context2D.DrawText(text, 
            Game.Renderer.Context2D.DrawText(text,
                DEFAULT_TEXT_FORMAT,
                new RectangleF(position.X, position.Y, Game.Settings.Resolution.Width, Game.Settings.Resolution.Height),
                textBrushesList[(int)brushColor],
                DrawTextOptions.Clip,
                MeasuringMode.GdiClassic);
        }

        /// <summary>
        /// Draw text with the specified parameters.
        /// </summary>
        /// <param name="text">The text to be drawn.</param>
        /// <param name="position">Position of the text.</param>
        /// <param name="brushColor">Color of the text.</param>
        /// <param name="options">The drawing options.</param>
        public void DrawText(string text, Vector2 position, TextBrushes brushColor, DrawTextOptions options)
        {
            //Game.Renderer.Context2D.DrawText(text, 
            Game.Renderer.Context2D.DrawText(text,
                DEFAULT_TEXT_FORMAT,
                new RectangleF(position.X, position.Y, Game.Settings.Resolution.Width, Game.Settings.Resolution.Height),
                textBrushesList[(int)brushColor],
                options,
                MeasuringMode.GdiClassic);
        }

        /// <summary>
        /// Draw text with the specified parameters.
        /// </summary>
        /// <param name="text">The text to be drawn.</param>
        /// <param name="position">Position of the text.</param>
        /// <param name="format">The format of the text.</param>
        /// <param name="brushColor">Color of the text.</param>
        public void DrawText(string text, Vector2 position, TextFormat format, TextBrushes brushColor)
        {
            //Game.Renderer.Context2D.DrawText(text, 
            Game.Renderer.Context2D.DrawText(text,
                format,
                new RectangleF(position.X, position.Y, Game.Settings.Resolution.Width, Game.Settings.Resolution.Height),
                textBrushesList[(int)brushColor],
                DrawTextOptions.Clip,
                MeasuringMode.GdiClassic);
        }

        /// <summary>
        /// Draw text with the specified parameters.
        /// </summary>
        /// <param name="text">The text to be drawn.</param>
        /// <param name="position">Position of the text.</param>
        /// <param name="format">The format of the text.</param>
        /// <param name="brushColor">Color of the text.</param>
        /// <param name="options">The drawing options.</param>
        public void DrawText(string text, Vector2 position, TextFormat format, TextBrushes brushColor, DrawTextOptions options)
        {
            //Game.Renderer.Context2D.DrawText(text, 
            Game.Renderer.Context2D.DrawText(text,
                format,
                new RectangleF(position.X, position.Y, Game.Settings.Resolution.Width, Game.Settings.Resolution.Height),
                textBrushesList[(int)brushColor],
                options,
                MeasuringMode.GdiClassic);
        }

        /// <summary>
        /// Draw text with the specified parameters.
        /// </summary>
        /// <param name="text">The text to be drawn.</param>
        /// <param name="destination">Destination rectangle of the text.</param>
        /// <param name="brushColor">Color of the text.</param>
        public void DrawText(string text, RectangleF destination, TextBrushes brushColor)
        {
            //Game.Renderer.Context2D.DrawText(text, 
            Game.Renderer.Context2D.DrawText(text,
                DEFAULT_TEXT_FORMAT,
                destination, textBrushesList[(int)brushColor],
                DrawTextOptions.Clip,
                MeasuringMode.GdiClassic);
        }

        /// <summary>
        /// Draw text with the specified parameters.
        /// </summary>
        /// <param name="text">The text to be drawn.</param>
        /// <param name="destination">Destination rectangle of the text.</param>
        /// <param name="brushColor">Color of the text.</param>
        /// <param name="options">The drawing options.</param>
        public void DrawText(string text, RectangleF destination, TextBrushes brushColor, DrawTextOptions options)
        {
            //Game.Renderer.Context2D.DrawText(text, 
            Game.Renderer.Context2D.DrawText(text,
                DEFAULT_TEXT_FORMAT,
                destination,
                textBrushesList[(int)brushColor],
                options,
                MeasuringMode.GdiClassic);
        }
        #endregion

        #region Text Layout
        /// <summary>
        /// Draw text with the specified layout.
        /// </summary>
        /// <param name="text">The text to be drawn.</param>
        /// <param name="position">Position of the text.</param>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="textLayout">The layout of the text.</param>
        /// <param name="brushColor">Color of the text.</param>
        public void DrawTextLayout(string text, Vector2 position, float fontSize, TextLayout textLayout, TextBrushes brushColor)
        {
            TextRange range = new TextRange(0, text.Length);
            TextLayout layout = new TextLayout(writeFactory, "Soy una prueba de texto --    importado.", POZO_BOLD, 400, 400);
            textLayout.SetDrawingEffect(textBrushesList[(int)brushColor], range);
            textLayout.SetFontSize(ConvertPointSizeToDIP(fontSize), range);
            //Game.Renderer.Context2D.DrawTextLayout(position, 
            Game.Renderer.Context2D.DrawTextLayout(position,
                textLayout,
                textBrushesList[(int)brushColor]);
        }

        /// <summary>
        /// Draw text with the specified layout.
        /// </summary>
        /// <param name="text">The text to be drawn.</param>
        /// <param name="position">Position of the text.</param>
        /// <param name="textFormat">The Text Format of the text. There are some static default formats.</param>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="brushColor">Color of the text.</param>
        public void DrawTextLayout(string text, Vector2 position, TextFormat textFormat, float fontSize, TextBrushes brushColor)
        {
            TextRange range = new TextRange(0, text.Length);
            TextLayout layout = new TextLayout(writeFactory, text, textFormat, Game.Settings.Resolution.Width, Game.Settings.Resolution.Height);
            layout.SetDrawingEffect(textBrushesList[(int)brushColor], range);
            layout.SetFontSize(ConvertPointSizeToDIP(fontSize), range);

            //Game.Renderer.Context2D.DrawTextLayout(position, layout, textBrushesList[(int)brushColor]);
            Game.Renderer.Context2D.DrawTextLayout(position, layout, textBrushesList[(int)brushColor]);
            Utilities.Dispose(ref layout);
        }

        /// <summary>
        /// Draw text creating a TextLayout with the specified parameters and ranges.
        /// </summary>
        /// <param name="text">The text to be drawn.</param>
        /// <param name="position">Position of the text.</param>
        /// <param name="textFormat">The Text Format of the text. There are some static default formats.</param>
        /// <param name="ranges">Ranges to change the format of text.</param>
        /// <param name="fontSizes">Sizes for the text ranges.</param>
        /// <param name="brushColors">Color of the text.</param>
        public void DrawTextLayout(string text, Vector2 position, TextFormat textFormat, List<TextRange> ranges, List<float> fontSizes, List<TextBrushes> brushColors)
        {
            TextLayout layout = new TextLayout(writeFactory, text, textFormat, Game.Settings.Resolution.Width, Game.Settings.Resolution.Height);

            for (int i = 0; i < ranges.Count; i++)
            {
                layout.SetDrawingEffect(textBrushesList[(int)brushColors[i]], ranges[i]);
                layout.SetFontSize(ConvertPointSizeToDIP(fontSizes[i]), ranges[i]);
                layout.SetFontFamilyName(textFormat.FontFamilyName, ranges[i]);
            }

            Game.Renderer.Context2D.DrawTextLayout(position, layout, textBrushesList[(int)TextBrushes.White]);
            Utilities.Dispose(ref layout);
        }
        #endregion

        #region Draw Path
        public void DrawPath()
        {
            Game.Renderer.Context2D.DrawGeometry(path, textBrushesList[(int)TextBrushes.Black], 10);
            Game.Renderer.Context2D.FillGeometry(path, textBrushesList[(int)TextBrushes.White]);
        }
        #endregion

        #endregion

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
        }

        #region Old manual
        ///// <summary>
        ///// Prepare the GPU to render textures or text using the default <see cref="SpriteEffect"/>.
        ///// </summary>
        //public void Begin()
        //{
        //    game.ToggleAlphaBlending(true);
        //    game.ToggleZBuffer(false);


        //    //=============  Effect apply
        //    game.Renderer.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        //    // Set the shaders
        //    game.Renderer.Context.VertexShader.Set(effect.Shader.VertexShader);
        //    game.Renderer.Context.PixelShader.Set(effect.Shader.PixelShader);
        //    // Set the input layout
        //    game.Renderer.Context.InputAssembler.InputLayout = effect.Shader.InputLayout;
        //    // Set the matrices and lights constant buffers.
        //    game.Renderer.Context.VertexShader.SetConstantBuffer(0, effect.MatricesConstantBuffer);
        //    //game.Renderer.Context.PixelShader.SetConstantBuffer(0, LightningConstantBuffer);


        //    // Set the matrices constant buffer.
        //    //MatricesStruct matrices = game.CurrentCamera.GetMatrices(mesh.Model);
        //    MatricesStruct2D matrices = camera.GetMatrices();
        //    game.Renderer.Context.UpdateSubresource(ref matrices, effect.MatricesConstantBuffer);

        //    //MaterialStruct light = LightingBuffer;
        //    //Game.Renderer.Context.UpdateSubresource(ref light, Effect.LightningConstantBuffer);
        //}

        ///// <summary>
        ///// Prepare the GPU to render textures or text with a defined Effect.
        ///// </summary>
        //public void Begin(Graphics3D.Effects.Effect effect)
        //{
        //    game.ToggleAlphaBlending(true);
        //    game.ToggleZBuffer(false);


        //    //=============  Effect apply
        //    game.Renderer.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        //    // Set the shaders
        //    game.Renderer.Context.VertexShader.Set(effect.Shader.VertexShader);
        //    game.Renderer.Context.PixelShader.Set(effect.Shader.PixelShader);
        //    // Set the input layout
        //    game.Renderer.Context.InputAssembler.InputLayout = effect.Shader.InputLayout;
        //    // Set the matrices and lights constant buffers.
        //    game.Renderer.Context.VertexShader.SetConstantBuffer(0, effect.MatricesConstantBuffer);
        //    //game.Renderer.Context.PixelShader.SetConstantBuffer(0, LightningConstantBuffer);


        //    // Set the matrices constant buffer.
        //    MatricesStruct2D matrices = camera.GetMatrices();
        //    game.Renderer.Context.UpdateSubresource(ref matrices, effect.MatricesConstantBuffer);

        //    //MaterialStruct light = LightingBuffer;
        //    //Game.Renderer.Context.UpdateSubresource(ref light, Effect.LightningConstantBuffer);
        //}

        ///// <summary>
        ///// Ends the rendering cycle and present the textures or fonts.
        ///// </summary>
        //public void End()
        //{
        //    game.ToggleAlphaBlending(false);
        //    game.ToggleZBuffer(true);
        //}

        ///// <summary>
        ///// Set a texture to be rendered when the <see cref="End"/> method is called.
        ///// </summary>
        ///// <param name="texture"></param>
        //public void DrawTexture(Texture2D texture)
        //{
        //    //=============  Material apply
        //    game.Renderer.Context.PixelShader.SetSampler(0, texture.Sampler);
        //    game.Renderer.Context.PixelShader.SetShaderResource(0, texture.ShaderResourceView);

        //    //=============  Mesh Render
        //    // Set the vertices and indices buffers into the InputAssembler phase.
        //    //game.Renderer.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<VertexMainStruct2D>(), 0));
        //    //game.Renderer.Context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);

        //    // Draw
        //    game.Renderer.Context.DrawIndexed(6, 0, 0);
        //}

        ///// <summary>
        ///// Set a text to be rendered when the <see cref="End"/> method is called.
        ///// </summary>
        //public void DrawText()
        //{

        //}
        #endregion
    }
}
