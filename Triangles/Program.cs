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
        //model for drawing: a square from two triangles
        private Branch pBranch = new Branch();
        private Leaf pLeaf = new Leaf();


        //struct for loading shaders
        private ShaderProgram shaderProgram = new ShaderProgram();

        //window size
        private static int windowWidth = 800;
        private static int windowHeight = 600;

        //last mouse coordinates
        private int mouseX, mouseY;

        //camera position
        private Vector3 eye = new Vector3(0, 0, 10);
        //reference point position
        private Vector3 cen = new Vector3(0, 0, 0);
        //up vector direction (head of observer)
        private Vector3 up = new Vector3(0, 1, 0);

        //matrices
        private Matrix4 modelMatrix = new Matrix4();
        private Matrix4 modelViewMatrix = new Matrix4();
        private Matrix4 projectionMatrix = new Matrix4();
        private Matrix4 modelViewProjectionMatrix = new Matrix4();
        private Matrix4 normalMatrix = new Matrix4();

        ///defines drawing mode
        static bool useTexture = true;

        //texture identificator
        static uint[] texId = new uint[1];

        //names of shader files. program will search for them during execution
        //don't forget place it near executable 
        static string VertexShaderName = @"Shaders\Vertex.vert";
        static string FragmentShaderName = @"Shaders\Fragment.frag";

        private void initTexture()
        {
            //generate as many textures as you need
            GL.GenTextures(1, texId);

            //enable texturing and zero slot
            GL.ActiveTexture(TextureUnit.Texture0);
            //bind texId to 0 unit
            GL.BindTexture(TextureTarget.Texture2D, texId[0]);

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
                 new NativeWindowSettings() { Title = "Test App", Size = new(windowWidth, windowHeight) })
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
            shaderProgram.init(VertexShaderName, FragmentShaderName);
            //use this shader program
            GL.UseProgram(shaderProgram.programObject);
            //create new branch
            pBranch = new Branch();
            //fill in data
            pBranch.initData();
            //initialize opengl buffers and variables inside of object
            pBranch.initGLBuffers((uint)shaderProgram.programObject, "pos", "nor", "tex");
            //create new leaf
            pLeaf = new Leaf();
            //fill in data
            pLeaf.initData();
            //initialize opengl buffers and variables inside of object
            pLeaf.initGLBuffers((uint)shaderProgram.programObject, "pos", "nor", "tex");
            //initializa texture
            initTexture();
            base.OnLoad();
        }

        /////////////////////////////////////////////////////////////////////
        ///called when window size is changed
        protected override void OnResize(ResizeEventArgs e)
        {
            windowWidth = e.Width;
            windowHeight = e.Height;
            //set viewport to match window size
            GL.Viewport(0, 0, e.Width, e.Height);

            float fieldOfView = 45.0f;
            float aspectRatio = (float)(e.Width) / (float)(e.Height);
            float zNear = 0.1f;
            float zFar = 100.0f;
            //set projection matrix
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fieldOfView), aspectRatio, zNear, zFar);
        }

        ////////////////////////////////////////////////////////////////////
        ///actions for single frame
        private void display()
        {
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            //Draw triangle with shaders (in screen coordinates)
            //need to set uniform in modelViewMatrix

            GL.UseProgram(shaderProgram.programObject);

            //we will need this uniform locations to connect them to our variables
            int locMV = GL.GetUniformLocation(shaderProgram.programObject, "modelViewMatrix");
            int locN = GL.GetUniformLocation(shaderProgram.programObject, "normalMatrix");
            int locP = GL.GetUniformLocation(shaderProgram.programObject, "modelViewProjectionMatrix");
            int texLoc = GL.GetUniformLocation(shaderProgram.programObject, "textureSampler");
            int locFlag = GL.GetUniformLocation(shaderProgram.programObject, "useTexture");
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
            Matrix4 viewMatrix = Matrix4.LookAt(eye, cen, up);


            ////////////////////////////////////////////
            /////////DRAW BRANCH///////////////////////
            //////////////////////////////////////////

            //modelMatrix is connected with current object
            modelMatrix = Matrix4.Identity;
            //3. Translate branch south pole to the point (-0.5, 1.0)
            modelMatrix = modelMatrix * Matrix4.CreateTranslation(new Vector3(-0.5f, 1.0f, 0.0f));
            //modelMatrix *= Matrix4.CreateTranslation(new Vector3(-0.5f, 1.0f, 0.0f));

            //2. Rotate cylinder 45 degrees to the left
            modelMatrix = Matrix4.CreateFromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), MathHelper.DegreesToRadians(45.0f)) * modelMatrix;
            //modelMatrix *= Matrix4.CreateFromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), MathHelper.DegreesToRadians(45.0f));

            //1. Scale. Make cylinder thinner to look like branch
            //modelMatrix *= Matrix4.CreateScale(new Vector3(0.05f, 1.0f, 0.05f));
            modelMatrix = Matrix4.CreateScale(new Vector3(0.05f, 1.0f, 0.05f)) * modelMatrix;

            //modelViewMatrix consists of viewMatrix and modelMatrix
            modelViewMatrix = modelMatrix * viewMatrix;
            //calculate normal matrix 
            //modelViewMatrix.Invert();
            //normalMatrix = modelViewMatrix;
            Matrix4.Transpose(Matrix4.Invert(modelViewMatrix), out normalMatrix);
            //finally calculate modelViewProjectionMatrix
            modelViewProjectionMatrix = modelViewMatrix * projectionMatrix;

            //bind texture
            GL.BindTexture(TextureTarget.Texture2D, texId[0]);


            //pass variables to the shaders
            GL.UniformMatrix4(locMV,false, ref modelViewMatrix);
            GL.UniformMatrix4(locN, false, ref normalMatrix);
            GL.UniformMatrix4(locP, false, ref modelViewProjectionMatrix);
            GL.Uniform1(texLoc, 0);
            GL.Uniform1(locFlag, Convert.ToInt32(useTexture));

            //draw branch
            pBranch.draw();

            //////////////////////////////////////////
            //////////////DRAW LEAF///////////////////
            //////////////////////////////////////////

            //modelMatrix is connected with current object
            modelMatrix = Matrix4.Identity;

            //3. Translate branch south pole to the north pole of branch
            modelMatrix = Matrix4.CreateTranslation(new Vector3(-1.0f / (float)Math.Sqrt(2.0f) - 0.5f, 1.0f / (float)Math.Sqrt(2.0f) + 1.0f, 0.0f)) * modelMatrix;

            //2. Scale. Make leaf smaller
            modelMatrix = Matrix4.CreateScale(0.3f * new Vector3(1.0f, 1.0f, 1.0f)) * modelMatrix;

            //1. Translate branch south pole to (0,0)
            modelMatrix = Matrix4.CreateTranslation(new Vector3(0.0f, 0.0f, 0.0f)) * modelMatrix;

            //modelViewMatrix consists of viewMatrix and modelMatrix
            modelViewMatrix = modelMatrix * viewMatrix;
            //calculate normal matrix 
            Matrix4.Transpose(Matrix4.Invert(modelViewMatrix), out normalMatrix);
            //finally calculate modelViewProjectionMatrix
            modelViewProjectionMatrix = modelViewMatrix * projectionMatrix;

            //pass variables to the shaders
            GL.UniformMatrix4(locMV, false, ref modelViewMatrix);
            GL.UniformMatrix4(locN, false, ref normalMatrix);
            GL.UniformMatrix4(locP, false, ref modelViewProjectionMatrix);

            //draw leaf
            pLeaf.draw();


            //end frame visualization
            Context.SwapBuffers();

        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            display();
        }



        //////////////////////////////////////////////////////////////////////////
        ///IdleFunction
        void update()
        {
            //make animation
            GLFW.PostEmptyEvent();
        } 
        /////////////////////////////////////////////////////////////////////////
        ///is called when key on keyboard is pressed
        ///use SPACE to switch mode
        ///TODO: place camera transitions in this function
        void keyboard(bool isSpace)
        {
            if (isSpace)
                useTexture = !useTexture;
        }

        ////////////////////////////////////////////////////////////////////////
        ///this function is used in case of InitializationError
        void emptydisplay()
        {
        }

        /////////////////////////////////////////////////////////////////////////
        ///is called when mouse button is pressed
        ///TODO: place camera rotations in this function
        void mouse(MouseState state)
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
            keyboard(input.IsKeyPressed(Keys.Space));
            var mouseState = MouseState;
            mouse(mouseState);
            update();
            base.OnUpdateFrame(args);
        }



        static void Main(string[] args)
        {
            //const char* slVer = (const char*) glGetString(GL_SHADING_LANGUAGE_VERSION);
            //cout << "GLSL Version: " << slVer << endl;
            using var window = new MyWindow();

            try
            {
                string ver = GL.GetString(StringName.ShadingLanguageVersion);
                Console.WriteLine($"GLSL Version: {ver}");
                window.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error During Main Loop: {ex.Message}");
            }
            finally
            {
                GL.DeleteTextures(1, texId);
            }
        }
    }
}


