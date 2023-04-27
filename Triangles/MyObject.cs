using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Platform;
using OpenTK.Graphics.ES30;
using System.Reflection.PortableExecutable;

namespace Triangles
{
    //helper struct for Vertex
    //contains position, normal and texture coordinates
    public struct VertexData
    {
        public Vector3 pos;
        public Vector3 nor;
        public Vector2 tex;
    };
    //some object for drawing
    public class MyObject:IDisposable
    {
        protected VertexData[] _data = new VertexData[2];
        protected uint[] _indices = new uint[2]; //pointer to indexes (list of vetrices) 
        protected uint[] _vbo = new uint[2];//VertexBufferObject one for MeshVertexData, another for Indexes
        protected int _vao = 0;//one VertexArrayObject

        public MyObject()
        {

        }
    	//function for initialization
	    public void InitGLBuffers(uint programId, string posName,string norName,string texName)
        {
            unsafe
            {
                _vao = GL.GenVertexArray();
                GL.BindVertexArray(_vao);

                GL.GenBuffers(2, _vbo);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo[0]);
                GL.BufferData(BufferTarget.ArrayBuffer, _data.Length * sizeof(VertexData), _data, BufferUsageHint.StaticDraw);

                GL.Enable(EnableCap.VertexArray);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vbo[1]);
                GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

                var loc = GL.GetAttribLocation(programId, posName);
                if (loc > -1)
                {
                    GL.VertexAttribPointer(loc, 3, VertexAttribPointerType.Float, false, sizeof(VertexData),0);
                    GL.EnableVertexAttribArray(loc);
                }
                var loc2 = GL.GetAttribLocation(programId, norName);
                if (loc2 > -1)
                {
                    GL.VertexAttribPointer(loc2, 3, VertexAttribPointerType.Float, false, sizeof(VertexData), (0 + sizeof(float) * 3));
                    GL.EnableVertexAttribArray(loc2);
                }
                var loc3 = GL.GetAttribLocation(programId, texName);
                if (loc3 > -1)
                {
                    GL.VertexAttribPointer(loc3, 2, VertexAttribPointerType.Float, false, sizeof(VertexData), (0 + sizeof(float) * 6));
                    GL.EnableVertexAttribArray(loc3);
                }
                GL.BindVertexArray(0);
            }
        }
        //function for drawing
        public void Draw()
        {
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length,DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        //generates two triangles
        public virtual void InitData()
        {
            int nTriangles = 2;
            _data = new VertexData[4];
            _indices = new uint[3 * nTriangles];
            
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i].pos = new Vector3(Convert.ToSingle(i % 2), (i > 1) ? 1 : 0, 0);
                _data[i].nor = new Vector3(0, 0, 1);
                _data[i].tex = new Vector2(Convert.ToSingle(i % 2), (i > 1) ? 1 : 0);
            }
            _indices[0] = 0; _indices[1] = 1; _indices[2] = 3;
            _indices[3 + 0] = 0; _indices[3 + 1] = 2; _indices[3 + 2] = 3;
        }

        public void Dispose()
        {
            GL.DeleteBuffers(2, _vbo);
            GL.DeleteVertexArray(_vao);
        }
    }
}
