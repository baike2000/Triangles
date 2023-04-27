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
        public int programObject = 0;

        public ShaderProgram()
        {
            vertex = new Shader();
            fragment = new Shader();
        }
        public void init(string vName, string fName)
        {
            //int success = 0; //local variable to check status
            //load and compile vertex shader
            var success = vertex.readAndCompile(vName, ShaderType.VertexShader);
            if (success != 0)
            {
                throw new Exception("Vertex Compilation Error");
            }
            //load and compile fragment shader
            success = fragment.readAndCompile(fName, ShaderType.FragmentShader); 
            if (success != 0)
            {
                throw new Exception("Fragment Compilation Error");
            }
            //create programObject
            programObject = GL.CreateProgram();
            //attach shaders
            GL.AttachShader(programObject, vertex.shaderObject);
            GL.AttachShader(programObject, fragment.shaderObject);

            GL.Ext.BindFragDataLocation(programObject, 0, "fragColor");

            //link shaders in program
            GL.LinkProgram(programObject);
            GL.GetProgramInfoLog(programObject, out var errorLog);
            if (!String.IsNullOrEmpty(errorLog))
            {
                Console.WriteLine(errorLog);

                GL.DetachShader(programObject, vertex.shaderObject);
                GL.DetachShader(programObject, fragment.shaderObject);
                vertex.Release();
                fragment.Release();

                throw new Exception("Link Error");
            }
        }
    }
}
