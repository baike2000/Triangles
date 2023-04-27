using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.ES30;

namespace Triangles
{
    public class Shader
    {

        public enum Status { SUCCESS, FILE_NOT_FOUND, EMPTY_FILE, READ_ERROR };
        public int ShaderObject { get; private set; }
        public ShaderType ShaderType { get; private set; }
        public Shader() { }
        private Status Read(string filename, ShaderType type)
        {
            var strSource = "";
            try
            {
                strSource = File.ReadAllText(filename);
            }
            catch
            {
                return Status.FILE_NOT_FOUND;
            }

            ShaderType = type;
            ShaderObject = GL.CreateShader(ShaderType);
            if (strSource.Length > 0)
                GL.ShaderSource(ShaderObject, strSource);

            return Status.SUCCESS;

        }
        private int Compile()
        {
            GL.CompileShader(ShaderObject);
            GL.GetShaderInfoLog(ShaderObject, out var errorLog);
            if (!String.IsNullOrEmpty(errorLog))
            {
                Console.WriteLine($"Shader {ShaderType} compile error");
                Console.WriteLine(errorLog);
                GL.DeleteShader(ShaderObject);
                return 1;
            }
            else
                Console.WriteLine($"Shader compilation succeed");
            return 0;
        }
        public int ReadAndCompile(string filename, ShaderType type)
        {
            if (Read(filename, type) != Status.SUCCESS)
            {
                Console.WriteLine("Error while reading shader. Invalid name or empty file.");
                return 1;
            }
            return Compile();
        }
        public void Release()
        {
            if (ShaderObject > 0)
            {
                GL.DeleteShader(ShaderObject);
                ShaderObject = -1;
            }
        }

    }
}
