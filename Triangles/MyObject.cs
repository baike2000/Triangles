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
        protected VertexData[] pData = new VertexData[2];
        protected uint[] pIndices = new uint[2]; //pointer to indexes (list of vetrices) 
        protected uint[] vbo = new uint[2];//VertexBufferObject one for MeshVertexData, another for Indexes
        protected int vao = 0;//one VertexArrayObject

        public MyObject()
        {

        }
    	//function for initialization
	    public void initGLBuffers(uint programId, string posName,string norName,string texName)
        {
            unsafe
            {
                vao = GL.GenVertexArray();
                GL.BindVertexArray(vao);

                GL.GenBuffers(2, vbo);

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo[0]);
                GL.BufferData(BufferTarget.ArrayBuffer, pData.Length * sizeof(VertexData), pData, BufferUsageHint.StaticDraw);

                GL.Enable(EnableCap.VertexArray);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo[1]);
                GL.BufferData(BufferTarget.ElementArrayBuffer, pIndices.Length * sizeof(uint), pIndices, BufferUsageHint.StaticDraw);

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
        public void draw()
        {
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, pIndices.Length,DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        //generates two triangles
        public virtual void initData()
        {
            int nTriangles = 2;
            pData = new VertexData[4];
            pIndices = new uint[3 * nTriangles];
            
            for (int i = 0; i < pData.Length; i++)
            {
                pData[i].pos = new Vector3(Convert.ToSingle(i % 2), (i > 1) ? 1 : 0, 0);
                pData[i].nor = new Vector3(0, 0, 1);
                pData[i].tex = new Vector2(Convert.ToSingle(i % 2), (i > 1) ? 1 : 0);
            }
            pIndices[0] = 0; pIndices[1] = 1; pIndices[2] = 3;
            pIndices[3 + 0] = 0; pIndices[3 + 1] = 2; pIndices[3 + 2] = 3;
        }

        public void Dispose()
        {
            GL.DeleteBuffers(2, vbo);
            GL.DeleteVertexArray(vao);
        }
    }
}
