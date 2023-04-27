using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Triangles
{
    public class Branch : MyObject
    {
        public Branch()
        {

        }

        public override void initData()
        {

            int radialStep = 10;
            int heightStep = 10;
            float cylRadius = 1.0f;
            float cylHeight = 1.0f;

            //number of points
            var dataCount = (radialStep + 1) * heightStep + 2;
            //number of triangles
            var nTriangles = 2 * radialStep * (heightStep - 1) + 2 * radialStep;
            //number of indices
            var indicesCount = 3 * nTriangles;

            pData = new VertexData[dataCount];
            pIndices = new uint[indicesCount];

            //fill in pData array

            //generate elements on side
            for (int j = 0; j < heightStep; j++)
            {
                float zPos = cylHeight * j / (heightStep - 1);
                for (int i = 0; i < radialStep + 1; i++)
                {
                    int pointId = j * (radialStep + 1) + i;

                    var fi = 2 * Math.PI * i / radialStep; //from 0 to 360 degrees
                    var xPos = Math.Cos(fi);
                    var yPos = Math.Sin(fi);

                    pData[pointId].pos = new Vector3(cylRadius * (float)xPos, zPos, cylRadius * (float)yPos);
                    pData[pointId].nor = new Vector3((float)xPos, 0, (float)yPos);
                    pData[pointId].tex = new Vector2(((float)xPos + 1) / 2, ((float)yPos + 1) / 2);
                }
            }
            //generate north pole
            {
                int pointId = heightStep * (radialStep + 1);
                pData[pointId].pos = new Vector3(0, cylHeight, 0);
                pData[pointId].nor = new Vector3(0, 1, 0);
                pData[pointId].tex = new Vector2(0.5f, 0.5f);
            }
            //generate south pole
            {
                int pointId = heightStep * (radialStep + 1) + 1;
                pData[pointId].pos = new Vector3(0, 0, 0);
                pData[pointId].nor = new Vector3(0, -1, 0);
                pData[pointId].tex = new Vector2(0.5f, 0.5f);
            }
            //fill in pIndices array

            //fill in side triangles (first 6*radialStep*(heightStep-1))
            for (int j = 0; j < heightStep - 1; j++)
            {
                for (int i = 0; i < radialStep; i++)
                {
                    int pointId = j * (radialStep + 1) + i;
                    int indexId = j * radialStep + i;
                    //pData configuration
                    //------------------------
                    //--.(i,j+1)--.(i+1,j+1)--
                    //--.(i,  j)--.(i+1,  j)--
                    //------------------------

                    //pData indices
                    //------------------------
                    //--pointId+radialStep+1--pointId+radialStep+2----
                    //--pointId---------------pointId+1---------------

                    //triangle 1			
                    //   /|
                    //  / |
                    // /__|  
                    pIndices[6 * indexId + 0] = (uint)pointId;
                    pIndices[6 * indexId + 1] = (uint)pointId + 1;
                    pIndices[6 * indexId + 2] = (uint)pointId + (uint)radialStep + 2;
                    //triangle 2
                    // ____
                    // |  /
                    // | /
                    // |/  
                    pIndices[6 * indexId + 3] = (uint)pointId;
                    pIndices[6 * indexId + 4] = (uint)pointId + (uint)radialStep + 2;
                    pIndices[6 * indexId + 5] = (uint)pointId + (uint)radialStep + 1;
                }
            }
            //fill in north pole triangles (next 3*radialStep)
            {
                int startIndex = 6 * radialStep * (heightStep - 1);
                int northPoleId = heightStep * (radialStep + 1);
                for (int i = 0; i < radialStep; i++)
                {
                    //get last row
                    int pointId = (heightStep - 1) * (radialStep + 1) + i;
                    pIndices[startIndex + 3 * i + 0] = (uint)pointId;
                    pIndices[startIndex + 3 * i + 1] = (uint)pointId + 1;
                    pIndices[startIndex + 3 * i + 2] = (uint)northPoleId;
                }
            }

            //fill in south pole triangles (last 3*radialStep)
            {
                int startIndex = 6 * radialStep * (heightStep - 1) + 3 * radialStep;
                int southPoleId = heightStep * (radialStep + 1) + 1;

                for (int i = 0; i < radialStep; i++)
                {
                    //get first row
                    int pointId = i;
                    pIndices[startIndex + 3 * i + 0] = (uint)pointId;
                    pIndices[startIndex + 3 * i + 1] = (uint)southPoleId;
                    pIndices[startIndex + 3 * i + 2] = (uint)pointId + 1;
                }
            }
        }
    }
}
