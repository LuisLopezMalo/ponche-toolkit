using System.Collections.Generic;
using SharpDX;
using SharpDX.DirectWrite;
using PoncheToolkit.Core;
using PoncheToolkit.Util;
using System;

namespace Core.Management.Text
{
    /// <summary>
    /// ResourceFont main loader. This classes implements FontCollectionLoader and FontFileLoader.
    /// It reads all fonts embedded as resource in the current assembly and expose them.
    /// </summary>
    public partial class CustomFontLoader : CallbackBase, FontCollectionLoader, FontFileLoader, IInitializable, ILoggable
    {
        private readonly List<CustomFontFileStream> fontStreams = new List<CustomFontFileStream>();
        private readonly List<CustomFontFileEnumerator> enumerators = new List<CustomFontFileEnumerator>();
        private DataStream keyStream;
        private readonly Factory factory;

        private string fontsPath;
        private Game game;

        /// <inheritdoc/>
        public event EventHandlers.OnInitializedHandler OnInitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomFontLoader"/> class.
        /// </summary>
        /// <param name="game">The game instance.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="fontsPath">The path to load the True Type (.ttf) fonts from. By default it looks in the "fonts" folder.</param>
        public CustomFontLoader(Game game, Factory factory, string fontsPath = "Fonts")
        {
            this.game = game;
            this.factory = factory;
            this.fontsPath = fontsPath;

            Log = new Logger(GetType());
        }

        /// <summary>
        /// Gets the key used to identify the FontCollection as well as storing index for fonts.
        /// </summary>
        /// <value>The key.</value>
        public DataStream Key
        {
            get { return keyStream; }
        }

        /// <inheritdoc/>
        public bool IsInitialized { get; set; }

        /// <inheritdoc/>
        public Logger Log { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            Log.Debug("Loading fonts from: -{0}-", System.IO.Path.Combine(game.ContentDirectoryName, fontsPath));

            // Only supports .ttf
            string[] files = System.IO.Directory.GetFiles(System.IO.Path.Combine(game.ContentDirectoryName, fontsPath), "*.ttf".Trim(), System.IO.SearchOption.AllDirectories);
            foreach (string fontName in files)
            {
                using (System.IO.Stream fileStream = System.IO.File.Open(fontName, System.IO.FileMode.Open))
                {
                    var fontBytes = Utilities.ReadStream(fileStream);
                    var stream = new DataStream(fontBytes.Length, true, true);
                    stream.Write(fontBytes, 0, fontBytes.Length);
                    stream.Position = 0;
                    fontStreams.Add(new CustomFontFileStream(stream));
                }
            }

            // Build a Key storage that stores the index of the font
            keyStream = new DataStream(sizeof(int) * fontStreams.Count, true, true);
            for (int i = 0; i < fontStreams.Count; i++)
                keyStream.Write(i);
            keyStream.Position = 0;

            // Register the font file loader
            this.factory.RegisterFontFileLoader(this);
            this.factory.RegisterFontCollectionLoader(this);

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <summary>
        /// Creates a font file enumerator object that encapsulates a collection of font files. The font system calls back to this interface to create a font collection.
        /// </summary>
        /// <param name="factory">Pointer to the <see cref="SharpDX.DirectWrite.Factory"/> object that was used to create the current font collection.</param>
        /// <param name="collectionKey">A font collection key that uniquely identifies the collection of font files within the scope of the font collection loader being used. The buffer allocated for this key must be at least  the size, in bytes, specified by collectionKeySize.</param>
        /// <returns>
        /// a reference to the newly created font file enumerator.
        /// </returns>
        /// <unmanaged>HRESULT IDWriteFontCollectionLoader::CreateEnumeratorFromKey([None] IDWriteFactory* factory,[In, Buffer] const void* collectionKey,[None] int collectionKeySize,[Out] IDWriteFontFileEnumerator** fontFileEnumerator)</unmanaged>
        FontFileEnumerator FontCollectionLoader.CreateEnumeratorFromKey(Factory factory, DataPointer collectionKey)
        {
            var enumerator = new CustomFontFileEnumerator(factory, this, collectionKey);
            enumerators.Add(enumerator);

            return enumerator;
        }

        /// <summary>
        /// Creates a font file stream object that encapsulates an open file resource.
        /// </summary>
        /// <param name="fontFileReferenceKey">A reference to a font file reference key that uniquely identifies the font file resource within the scope of the font loader being used. The buffer allocated for this key must at least be the size, in bytes, specified by  fontFileReferenceKeySize.</param>
        /// <returns>
        /// a reference to the newly created <see cref="SharpDX.DirectWrite.FontFileStream"/> object.
        /// </returns>
        /// <remarks>
        /// The resource is closed when the last reference to fontFileStream is released.
        /// </remarks>
        /// <unmanaged>HRESULT IDWriteFontFileLoader::CreateStreamFromKey([In, Buffer] const void* fontFileReferenceKey,[None] int fontFileReferenceKeySize,[Out] IDWriteFontFileStream** fontFileStream)</unmanaged>
        FontFileStream FontFileLoader.CreateStreamFromKey(DataPointer fontFileReferenceKey)
        {
            var index = Utilities.Read<int>(fontFileReferenceKey.Pointer);
            return fontStreams[index];
        }
    }
}