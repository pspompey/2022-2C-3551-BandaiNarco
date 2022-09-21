using System;
using System.Linq;
using BepuPhysics.Collidables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.BandaiNarco.TP.Cameras;

namespace BandaiNarco
{
    /// <summary>
    ///     Esta es la clase principal  del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "3D/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            // Descomentar para que el juego sea pantalla completa.
            // Graphics.IsFullScreen = true;
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }

        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }
        private Model Model { get; set; }
        private Effect Effect { get; set; }
        private float Rotation { get; set; }
        private Matrix World { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }

        private TargetCamera Camera { get; set; }
        private Texture2D GumTexture;
        private Matrix SphereWorld;
      
        /// <summary>
        /// A quad to draw the floor and transparent geometry
        /// </summary>
        private QuadPrimitive Quad { get; set; }

        /// <summary>
        /// A Tiling Texture Effect to draw the floor
        /// </summary>
        private Effect TilingFloorEffect { get; set; }
        private Matrix FloorWorld { get; set; }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.

            // Apago el backface culling.
            // Esto se hace por un problema en el diseno del modelo del logo de la materia.
            // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;
            // Seria hasta aca.

            // Configuro el tamaño de la pantalla
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            Graphics.ApplyChanges();

            var size = GraphicsDevice.Viewport.Bounds.Size;
            size.X /= 2;
            size.Y /= 2;

            // Create the Camera
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.One * 100f, Vector3.Zero);

            // Configuramos nuestras matrices de la escena.
            World = Matrix.Identity;
            View = Matrix.CreateLookAt(Vector3.UnitZ * 3000, Vector3.Zero, Vector3.Up);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 5, 5000);
            
            Quad = new QuadPrimitive(GraphicsDevice, Content);

            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Cargo el modelo del logo.
            Model = Content.Load<Model>(ContentFolder3D + "geometries/sphere");

            // Cargo un efecto basico propio declarado en el Content pipeline.
            // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
            Effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            SphereWorld = Matrix.CreateScale(20f) * Matrix.CreateRotationX(MathF.PI * 0.5f);
            FloorWorld = Matrix.CreateScale(200f, 0.001f, 250f) * Matrix.CreateTranslation(0, SphereWorld.Backward.Y, 0);

            GumTexture = Content.Load<Texture2D>(ContentFolderTextures + "goma");
            Effect.Parameters["ModelTexture"]?.SetValue(GumTexture);

            // Load the texture for the floor
            var floorTexture = Content.Load<Texture2D>(ContentFolderTextures + "floor/tierra");

            // Load the floor effect and set its parameters
            TilingFloorEffect = Content.Load<Effect>(ContentFolderEffects + "TextureTiling");

            TilingFloorEffect.Parameters["Texture"].SetValue(floorTexture);
            TilingFloorEffect.Parameters["Tiling"].SetValue(Vector2.One * 10f);

            base.LoadContent();
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logica de actualizacion del juego.
            var time = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                //Salgo del juego.
                Exit();

            Camera.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.Black);

            // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            var mesh = Model.Meshes.FirstOrDefault();

            if (mesh != null)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = Effect;
                }
                mesh.Draw();
            }

            Effect.Parameters["World"].SetValue(SphereWorld);
            Effect.Parameters["View"].SetValue(Camera.View);
            Effect.Parameters["Projection"].SetValue(Camera.Projection);
            Effect.Parameters["ModelTexture"].SetValue(GumTexture);

            var viewProjection = Camera.View * Camera.Projection;

            // Draw the floor
            TilingFloorEffect.Parameters["WorldViewProjection"].SetValue(FloorWorld * viewProjection);
            Quad.Draw(TilingFloorEffect);
        }

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }
    }
}