#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace MonogameFinal
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        BasicEffect effect;
        bool first = false;
        bool bounce = false;
        bool paused = false;
        float delta = 0;
        float shotDelay = 0;
        float pauseDelay = 0;
        Random r; 
        RasterizerState rasterizerState;
        World world;
        SpriteFont font;

        public Game1()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 720;   // set this value to the desired height of your window
            this.Window.Title = "Physics Sandbox";
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            graphics.ApplyChanges();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            rasterizerState.FillMode = FillMode.Solid;
            GraphicsDevice.RasterizerState = rasterizerState;

            font = Content.Load<SpriteFont>("Font/Verdana");

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //The camera 3D class represents a position and look vector as well as a field of view matrix and methods to simplify looking around. 
            Camera3D camera = new Camera3D(Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), graphics.PreferredBackBufferWidth / graphics.PreferredBackBufferHeight, 0.1f, 100.0f), new Vector3(8.0f, 8.0f, 2.0f), new Vector3(-1.0f, -1.0f, 0.0f));
            world = new World(camera);


            //The effect class defines how everything will be drawn, it contains simple lighting as well as the transformation matrices needed to draw to the screen
            effect = new BasicEffect(GraphicsDevice);
            effect.World = Matrix.Identity;
            effect.View = camera.getLookMatrix();
            effect.Projection = camera.getFOVMatrix();
            effect.VertexColorEnabled = true;
            effect.LightingEnabled = true;
            effect.EnableDefaultLighting();

            r = new Random();






            //Very messy, manually making a map for the player to interact with. While it should be in a file and loaded in, this was used for testing
            //and I had to cut planned features.

            Color c = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
            Color c1 = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
            Color c2 = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
            Color c3 = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
            Color c4 = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
            Color c5 = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
            List<Polygon> polygons = new List<Polygon>();
            Polygon p = new Polygon(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Color[] { c, c, c });
            polygons = Util.makeCube(0.5F, c);

            List<ModelMesh> meshes = new List<ModelMesh>();
            ModelBone rootBone = new ModelBone();
            rootBone.Transform = Matrix.CreateTranslation(0, 0, 0);
            rootBone.ModelTransform = Matrix.CreateTranslation(0, 0, 0);
            meshes.Add(Util.makeMeshFromPolygonList(polygons, GraphicsDevice, rootBone));
            
            Model m = Util.makeModel(meshes, GraphicsDevice, effect, rootBone);
            m.Tag = new PhysicsObject(m, 1.0f, world);
            ((PhysicsObject)m.Tag).addImpulse(new Vector3(0, 0.0f, 0.0f), 1);

            Projectile target = new Projectile(m, world);
            world.registerEntity(target);

            List<Polygon> polyList = new List<Polygon>();
            c = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
            polyList = Util.makeRectPrism(7.0f, 3.0f, 2.0f, c);

            List<ModelMesh> meshList = new List<ModelMesh>();

            meshList.Add(Util.makeMeshFromPolygonList(polyList, GraphicsDevice));

            Model modelObj = Util.makeModel(meshList, GraphicsDevice, effect);
            modelObj.Tag = new PhysicsObject(modelObj, 80.0f, world) { gravity = false , position = new Vector3(0, 0, -3.0f)};
            ((PhysicsObject)modelObj.Tag).addImpulse(new Vector3(0, -0.0f, -0.0f), 1);

            
            Obstacle wallHorizontal = new Obstacle(modelObj, world);
            world.registerEntity(wallHorizontal);

            List<Polygon> polygonList = new List<Polygon>();
            c = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
            polygonList = Util.makeRectPrism(4.0f, 4.0f, 2.0f, c);

            List<ModelMesh> modelMeshList = new List<ModelMesh>();

            modelMeshList.Add(Util.makeMeshFromPolygonList(polygonList, GraphicsDevice));

            Model modelObject = Util.makeModel(modelMeshList, GraphicsDevice, effect);
            modelObject.Tag = new PhysicsObject(modelObject, 80.0f, world) { gravity = false, position = new Vector3(0, 0, 3.0f) };

            Obstacle wallVertical = new Obstacle(modelObject, world);
            world.registerEntity(wallVertical);

            c = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());

            polygonList = Util.makeMultiColRectPrism(30.0f, 30.0f, 30.0f, c, c1, c2, c3, c4, c5);

            modelMeshList = new List<ModelMesh>();

            modelMeshList.Add(Util.makeMeshFromPolygonList(polygonList, GraphicsDevice));

            modelObject = Util.makeModel(modelMeshList, GraphicsDevice, effect);
            modelObject.Tag = new PhysicsObject(modelObject, 1.0f, world) { gravity = false , staticPhysics = true};

            Obstacle terrain = new Obstacle(modelObject, world);
            world.registerEntity(terrain);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public void shoot()
        {
            //Creates a projectile entity at the camera position
            Color c = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
            List<Polygon> polyList = Util.makeCube(0.5f, c);

            List<ModelMesh> meshList = new List<ModelMesh>();

            meshList.Add(Util.makeMeshFromPolygonList(polyList, GraphicsDevice));

            Model modelObj = Util.makeModel(meshList, GraphicsDevice, effect);
            PhysicsObject phys = new PhysicsObject(modelObj, 1.01f, world) { position = world.camera.position, gravity = false };
            phys.addImpulse(world.camera.look * 30.0f, 1.0f);
            Util.moveBone(Matrix.CreateTranslation(world.camera.position), modelObj.Root);
            modelObj.Tag = phys;
            Entity projectile = new Projectile(modelObj, world);
            world.registerEntity(projectile);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            

            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //The pause function is triggered here, and a pauseDelay is used to prevent pause toggling since the same hotkey is used.
            if (Keyboard.GetState().IsKeyDown(Keys.Back) && pauseDelay <= 0)
            {
                paused = paused ? false : true;
                pauseDelay = 0.25f;
            }
            pauseDelay -= timeDelta;

            //if (PhysicsObject.count > 1000)
                //paused = true;

            if (!paused)
            {
                if (!first)
                {
                    Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                    first = true;
                    return;
                }
                // TODO: Add your update logic here
                //Camera movement based on input and 
                Vector2 camera2D = world.camera.cameraYawPitch;
                float midX = GraphicsDevice.Viewport.Width / 2;
                float midY = GraphicsDevice.Viewport.Height / 2;
                float lookSpeed = 0.65f;
                float yMod = 1.5f;
                float xMod = 1.0f;


                if (Mouse.GetState().X < midX)
                    camera2D += new Vector2((lookSpeed * (1 - (Mouse.GetState().X / midX))), 0) * xMod;
                else if (Mouse.GetState().X > midX)
                    camera2D -= new Vector2((lookSpeed * ((Mouse.GetState().X - midX) / midX)), 0) * xMod;

                if (Mouse.GetState().Y < midY)
                    camera2D -= new Vector2(0, (-1 * lookSpeed * (1 - (Mouse.GetState().Y / midY)))) * yMod;
                else if (Mouse.GetState().Y > midY)
                    camera2D += new Vector2(0, (-1 * lookSpeed * ((Mouse.GetState().Y - midY) / midY))) * yMod;


                //At the beginning of the keyboard checking, pressing x to shoot calls the shoot method, with a global shotDelay to create a rate of fire.
                if (Keyboard.GetState().IsKeyDown(Keys.X) && shotDelay <= 0)
                {
                    shoot();
                    shotDelay = 0.25f;
                }

                shotDelay -= timeDelta;

                if (Keyboard.GetState().IsKeyDown(Keys.W))
                    world.camera.move(0.1f, 8);
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                    world.camera.move(0.1f, 2);
                if (Keyboard.GetState().IsKeyDown(Keys.A))
                    world.camera.move(0.1f, 4);
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                    world.camera.move(0.1f, 6);
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                    world.camera.move(0.1f, 0);
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                    world.camera.move(0.1f, 5);

                Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

                //Locking the camera, since moving past Math.PI in the Y value does not clamp properly.
                //Also helps promote that the player is a human, since we cannot continously turn our head vertically in circles.
                if (camera2D.Y >= Math.PI * 11 / 12)
                {
                    camera2D.Y = (float)Math.PI * 11 / 12;
                }
                else if (camera2D.Y <= -Math.PI * 11 / 12)
                {
                    camera2D.Y = (float)-Math.PI * 11 / 12;
                }

                world.camera.setLook(camera2D);

                world.update(timeDelta);

                
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            //In debug the FPS was counted for efficiency standards, however, it does not show up in the release build type of the game.
            #if DEBUG
                spriteBatch.Begin();



                double fps = (Math.Round(10000000.0f / gameTime.ElapsedGameTime.Ticks, 2));

                spriteBatch.DrawString(font, "FPS: " + fps, new Vector2(0.0f, 0.0f), Color.White);

                spriteBatch.End();

                GraphicsDevice.RasterizerState = rasterizerState;
                GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            #endif
            // TODO: Add your drawing code here

            //GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            world.draw();

            base.Draw(gameTime);
        }
    }
}
