// See https://aka.ms/new-console-template for more information
using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.ES30;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;

namespace Triangles // Note: actual namespace depends on the project name.
{
    public class MyWindow : GameWindow
    {
        //window size
        private static int _windowWidth = 800;
        private static int _windowHeight = 600;

        //model for drawing: a square from two triangles
        private Branch _pBranch = new Branch();
        private Leaf _pLeaf = new Leaf();


        //struct for loading shaders
        private ShaderProgram _shaderProgram = new ShaderProgram();

        //last mouse coordinates
        private int mouseX, mouseY;

        //camera position
        private Vector3 _eye = new Vector3(0, 0, 10);
        //reference point position
        private Vector3 _cen = new Vector3(0, 0, 0);
        //up vector direction (head of observer)
        private Vector3 _up = new Vector3(0, 1, 0);

        //matrices
        private Matrix4 _modelMatrix = new Matrix4();
        private Matrix4 _modelViewMatrix = new Matrix4();
        private Matrix4 _projectionMatrix = new Matrix4();
        private Matrix4 _modelViewProjectionMatrix = new Matrix4();
        private Matrix4 _normalMatrix = new Matrix4();

        ///defines drawing mode
        static bool _useTexture = true;

        //texture identificator
        static uint[] _texId = new uint[1];

        //names of shader files. program will search for them during execution
        //don't forget place it near executable 
        static string VertexShaderName = @"Shaders\Vertex.vert";
        static string FragmentShaderName = @"Shaders\Fragment.frag";

        private void InitTexture()
        {
            //generate as many textures as you need
            GL.GenTextures(1, _texId);

            //enable texturing and zero slot
            GL.ActiveTexture(TextureUnit.Texture0);
            //bind texId to 0 unit
            GL.BindTexture(TextureTarget.Texture2D, _texId[0]);

            //don't use alignment
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            // Set nearest filtering mode for texture minification
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            //TODO: load texture from file 
            byte[] imgData = new byte[2 * 2 * 3]
            {
		        //row1: yellow,orange
		        255,255,0, 255,128,0,
		        //row2: green, dark green
		        0,255,0, 0,64,0
            };

            //set Texture Data
            GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.Rgb, 2, 2, 0, PixelFormat.Rgb, PixelType.UnsignedByte, imgData);
        }
        public MyWindow() :
            base(new GameWindowSettings() { RenderFrequency = 60, UpdateFrequency = 60 },
                 new NativeWindowSettings() { Title = "Test App", Size = new(_windowWidth, _windowHeight) })
        {

        }

        /////////////////////////////////////////////////////////////////////
        ///is called when program starts
        protected override void OnLoad()
        {
            //enable depth test
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.DepthMask(true);
            //initialize shader program
            _shaderProgram.Init(VertexShaderName, FragmentShaderName);
            //use this shader program
            GL.UseProgram(_shaderProgram.ProgramObject);
            //create new branch
            _pBranch = new Branch();
            //fill in data
            _pBranch.InitData();
            //initialize opengl buffers and variables inside of object
            _pBranch.InitGLBuffers((uint)_shaderProgram.ProgramObject, "pos", "nor", "tex");
            //create new leaf
            _pLeaf = new Leaf();
            //fill in data
            _pLeaf.InitData();
            //initialize opengl buffers and variables inside of object
            _pLeaf.InitGLBuffers((uint)_shaderProgram.ProgramObject, "pos", "nor", "tex");
            //initializa texture
            InitTexture();
            base.OnLoad();
        }

        /////////////////////////////////////////////////////////////////////
        ///called when window size is changed
        protected override void OnResize(ResizeEventArgs e)
        {
            _windowWidth = e.Width;
            _windowHeight = e.Height;
            //set viewport to match window size
            GL.Viewport(0, 0, e.Width, e.Height);

            float fieldOfView = 45.0f;
            float aspectRatio = (float)(e.Width) / (float)(e.Height);
            float zNear = 0.1f;
            float zFar = 100.0f;
            //set projection matrix
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fieldOfView), aspectRatio, zNear, zFar);
        }

        ////////////////////////////////////////////////////////////////////
        ///actions for single frame
        private void Display()
        {
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            //Draw triangle with shaders (in screen coordinates)
            //need to set uniform in modelViewMatrix

            GL.UseProgram(_shaderProgram.ProgramObject);

            //we will need this uniform locations to connect them to our variables
            int locMV = GL.GetUniformLocation(_shaderProgram.ProgramObject, "modelViewMatrix");
            int locN = GL.GetUniformLocation(_shaderProgram.ProgramObject, "normalMatrix");
            int locP = GL.GetUniformLocation(_shaderProgram.ProgramObject, "modelViewProjectionMatrix");
            int texLoc = GL.GetUniformLocation(_shaderProgram.ProgramObject, "textureSampler");
            int locFlag = GL.GetUniformLocation(_shaderProgram.ProgramObject, "useTexture");
            //if there is some problem
            if (locMV < 0 || locN < 0 || locP < 0 || texLoc < 0 || locFlag < 0)
            {
                //not all uniforms were allocated - show blue screen.
                //check your variables properly. May be there is unused?
                GL.ClearColor(0, 0, 1, 1);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                //end frame visualization
                Context.SwapBuffers();
                return;
            }

            //camera matrix. camera is placed in point "eye" and looks at point "cen".
            Matrix4 viewMatrix = Matrix4.LookAt(_eye, _cen, _up);


            ////////////////////////////////////////////
            /////////DRAW BRANCH///////////////////////
            //////////////////////////////////////////

            //modelMatrix is connected with current object
            _modelMatrix = Matrix4.Identity;
            //3. Translate branch south pole to the point (-0.5, 1.0)
            _modelMatrix = _modelMatrix * Matrix4.CreateTranslation(new Vector3(-0.5f, 1.0f, 0.0f));

            //2. Rotate cylinder 45 degrees to the left
            _modelMatrix = Matrix4.CreateFromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), MathHelper.DegreesToRadians(45.0f)) * _modelMatrix;

            //1. Scale. Make cylinder thinner to look like branch
            _modelMatrix = Matrix4.CreateScale(new Vector3(0.05f, 1.0f, 0.05f)) * _modelMatrix;

            //modelViewMatrix consists of viewMatrix and modelMatrix
            _modelViewMatrix = _modelMatrix * viewMatrix;
            //calculate normal matrix 
            Matrix4.Transpose(Matrix4.Invert(_modelViewMatrix), out _normalMatrix);
            //finally calculate modelViewProjectionMatrix
            _modelViewProjectionMatrix = _modelViewMatrix * _projectionMatrix;

            //bind texture
            GL.BindTexture(TextureTarget.Texture2D, _texId[0]);


            //pass variables to the shaders
            GL.UniformMatrix4(locMV,false, ref _modelViewMatrix);
            GL.UniformMatrix4(locN, false, ref _normalMatrix);
            GL.UniformMatrix4(locP, false, ref _modelViewProjectionMatrix);
            GL.Uniform1(texLoc, 0);
            GL.Uniform1(locFlag, Convert.ToInt32(_useTexture));

            //draw branch
            _pBranch.Draw();

            //////////////////////////////////////////
            //////////////DRAW LEAF///////////////////
            //////////////////////////////////////////

            //modelMatrix is connected with current object
            _modelMatrix = Matrix4.Identity;

            //3. Translate branch south pole to the north pole of branch
            _modelMatrix = Matrix4.CreateTranslation(new Vector3(-1.0f / (float)Math.Sqrt(2.0f) - 0.5f, 1.0f / (float)Math.Sqrt(2.0f) + 1.0f, 0.0f)) * _modelMatrix;

            //2. Scale. Make leaf smaller
            _modelMatrix = Matrix4.CreateScale(0.3f * new Vector3(1.0f, 1.0f, 1.0f)) * _modelMatrix;

            //1. Translate branch south pole to (0,0)
            _modelMatrix = Matrix4.CreateTranslation(new Vector3(0.0f, 0.0f, 0.0f)) * _modelMatrix;

            //modelViewMatrix consists of viewMatrix and modelMatrix
            _modelViewMatrix = _modelMatrix * viewMatrix;
            //calculate normal matrix 
            Matrix4.Transpose(Matrix4.Invert(_modelViewMatrix), out _normalMatrix);
            //finally calculate modelViewProjectionMatrix
            _modelViewProjectionMatrix = _modelViewMatrix * _projectionMatrix;

            //pass variables to the shaders
            GL.UniformMatrix4(locMV, false, ref _modelViewMatrix);
            GL.UniformMatrix4(locN, false, ref _normalMatrix);
            GL.UniformMatrix4(locP, false, ref _modelViewProjectionMatrix);

            //draw leaf
            _pLeaf.Draw();

            //end frame visualization
            Context.SwapBuffers();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            Display();
        }



        //////////////////////////////////////////////////////////////////////////
        ///IdleFunction
        private void Update()
        {
            //make animation
            GLFW.PostEmptyEvent();
        } 
        /////////////////////////////////////////////////////////////////////////
        ///is called when key on keyboard is pressed
        ///use SPACE to switch mode
        ///TODO: place camera transitions in this function
        private void Keyboard(bool isSpace)
        {
            if (isSpace)
                _useTexture = !_useTexture;
        }
        /////////////////////////////////////////////////////////////////////////
        ///is called when mouse button is pressed
        ///TODO: place camera rotations in this function
        private void Mouse(MouseState state)
        {
            if (state[0])
            {
                if (state.IsButtonDown(MouseButton.Left))
                {
                    mouseX = (int)state.X; mouseY = (int)state.Y;
                }
                else
                {
                    mouseX = -1; mouseY = -1;
                }
            }

        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            var input = KeyboardState;
            Keyboard(input.IsKeyPressed(Keys.Space));
            var mouseState = MouseState;
            Mouse(mouseState);
            Update();
            base.OnUpdateFrame(args);
        }



        static void Main(string[] args)
        {
            using var window = new MyWindow();

            try
            {
                var ver = GL.GetString(StringName.ShadingLanguageVersion);
                Console.WriteLine($"GLSL Version: {ver}");
                window.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error During Main Loop: {ex.Message}");
            }
            finally
            {
                GL.DeleteTextures(1, _texId);
            }
        }
    }
}


