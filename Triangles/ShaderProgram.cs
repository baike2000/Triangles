using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.ES30;

namespace Triangles
{
    public class ShaderProgram
    {
        private Shader vertex, fragment;
        public int ProgramObject { get; private set; }

        public ShaderProgram()
        {
            vertex = new Shader();
            fragment = new Shader();
            ProgramObject = -1;
        }
        public void Init(string vName, string fName)
        {
            //int success = 0; //local variable to check status
            //load and compile vertex shader
            var success = vertex.ReadAndCompile(vName, ShaderType.VertexShader);
            if (success != 0)
            {
                throw new Exception("Vertex Compilation Error");
            }
            //load and compile fragment shader
            success = fragment.ReadAndCompile(fName, ShaderType.FragmentShader); 
            if (success != 0)
            {
                throw new Exception("Fragment Compilation Error");
            }
            //create programObject
            ProgramObject = GL.CreateProgram();
            //attach shaders
            GL.AttachShader(ProgramObject, vertex.ShaderObject);
            GL.AttachShader(ProgramObject, fragment.ShaderObject);

            GL.Ext.BindFragDataLocation(ProgramObject, 0, "fragColor");

            //link shaders in program
            GL.LinkProgram(ProgramObject);
            GL.GetProgramInfoLog(ProgramObject, out var errorLog);
            if (!String.IsNullOrEmpty(errorLog))
            {
                Console.WriteLine(errorLog);

                GL.DetachShader(ProgramObject, vertex.ShaderObject);
                GL.DetachShader(ProgramObject, fragment.ShaderObject);
                vertex.Release();
                fragment.Release();

                throw new Exception("Link Error");
            }
        }
    }
}
