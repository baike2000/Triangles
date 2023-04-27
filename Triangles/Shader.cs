using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.ES30;

namespace Triangles
{
    public class Shader
    {

        public enum STATUS { SUCCESS, FILE_NOT_FOUND, EMPTY_FILE, READ_ERROR };
        public int shaderObject;
        private STATUS iStatus;
        private string strName = "";
        private string strSource = "";
        private ShaderType shaderType;
        public Shader() { }
        //~Shader();
        public STATUS read(string filename, ShaderType type)
        {
            strName = filename;

            try
            {
                strSource = File.ReadAllText(strName);
            }
            catch
            {
                return STATUS.FILE_NOT_FOUND;
            }

            shaderType = type;
            shaderObject = GL.CreateShader(shaderType);
            if (strSource.Length > 0)
                GL.ShaderSource(shaderObject, strSource);

            return iStatus;

        }
        public int compile()
        {
            int success = 0;
            GL.CompileShader(shaderObject);
            GL.GetShaderInfoLog(shaderObject, out var errorLog);
            if (!String.IsNullOrEmpty(errorLog))
            {
                Console.WriteLine($"Shader {shaderType} compile error");
                Console.WriteLine(errorLog);
                GL.DeleteShader(shaderObject);
                success = 1;
            }
            else
                Console.WriteLine($"Shader compilation succeed");


            return success;
        }
        public int readAndCompile(string filename, ShaderType type)
        {
            read(filename, type);
            if (iStatus != STATUS.SUCCESS)
            {
                Console.WriteLine("Error while reading shader. Invalid name or empty file.");
                return 1;
            }
            return compile();
        }
        public void Release()
        {
            if (shaderObject > 0)
            {
                GL.DeleteShader(shaderObject);
                shaderObject = -1;
            }
        }

    }
}
