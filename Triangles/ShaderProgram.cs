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
        private Shader _vertex, _fragment;
        public int ProgramObject { get; private set; }

        public ShaderProgram()
        {
            _vertex = new Shader();
            _fragment = new Shader();
            ProgramObject = -1;
        }
        public void Init(string vName, string fName)
        {
            //int success = 0; //local variable to check status
            //load and compile vertex shader
            var success = _vertex.ReadAndCompile(vName, ShaderType.VertexShader);
            if (success != 0)
            {
                throw new Exception("Vertex Compilation Error");
            }
            //load and compile fragment shader
            success = _fragment.ReadAndCompile(fName, ShaderType.FragmentShader); 
            if (success != 0)
            {
                throw new Exception("Fragment Compilation Error");
            }
            //create programObject
            ProgramObject = GL.CreateProgram();
            //attach shaders
            GL.AttachShader(ProgramObject, _vertex.ShaderObject);
            GL.AttachShader(ProgramObject, _fragment.ShaderObject);

            GL.Ext.BindFragDataLocation(ProgramObject, 0, "fragColor");

            //link shaders in program
            GL.LinkProgram(ProgramObject);
            GL.GetProgramInfoLog(ProgramObject, out var errorLog);
            if (!String.IsNullOrEmpty(errorLog))
            {
                Console.WriteLine(errorLog);

                GL.DetachShader(ProgramObject, _vertex.ShaderObject);
                GL.DetachShader(ProgramObject, _fragment.ShaderObject);
                _vertex.Release();
                _fragment.Release();

                throw new Exception("Link Error");
            }
        }
    }
}
