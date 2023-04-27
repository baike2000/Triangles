using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Triangles
{
    public class Leaf:MyObject
    {
        private float equation(float x)
        {
            return Convert.ToSingle(Math.Sqrt(Math.Max(0.0f, 1.0f - 2 * x * x)));
        }
        public Leaf()
        {

        }

        public override void initData()
        {
            //example equation: y=+-sqrt(1 - x*x/100) (also is symmetric around x)
            int nInternalQuaterSteps = 5;

            //number of points
            var dataCount = 3 * (nInternalQuaterSteps * 2 + 1) + 2;
            //number of triangles
            var nTriangles = 2 * 4 * nInternalQuaterSteps + 4;
            //number of indices
            var indicesCount = 3 * nTriangles;

            pData = new VertexData[dataCount];
            pIndices = new uint[indicesCount];

            //fill in pData array
            //right up
            int startIndex = 0;
            for (int i = 0; i < nInternalQuaterSteps + 1; i++)
            {
                float xPos = (1.0f + i) / (nInternalQuaterSteps + 2);
                float yPos = equation(xPos);

                pData[startIndex + i].pos = new Vector3(xPos / 2, (yPos + 1) / 2, 0);
                pData[startIndex + i].nor = new Vector3(0, 0, -1);
                pData[startIndex + i].tex = new Vector2((xPos + 1) / 2, (yPos + 1) / 2);
            }
            //right down
            startIndex += nInternalQuaterSteps + 1;
            for (int i = 0; i < nInternalQuaterSteps; i++)
            {
                float xPos = (1.0f + nInternalQuaterSteps - i) / (nInternalQuaterSteps + 2);
                float yPos = -equation(xPos);

                pData[startIndex + i].pos = new Vector3(xPos / 2, (yPos + 1) / 2, 0);
                pData[startIndex + i].nor = new Vector3(0, 0, -1);
                pData[startIndex + i].tex = new Vector2((xPos + 1) / 2, (yPos + 1) / 2);
            }
            startIndex += nInternalQuaterSteps;
            //left up
            for (int i = 0; i < nInternalQuaterSteps + 1; i++)
            {
                float xPos = -(1.0f + i) / (nInternalQuaterSteps + 2);
                float yPos = equation(xPos);

                pData[startIndex + i].pos = new Vector3(xPos / 2, (yPos + 1) / 2, 0);
                pData[startIndex + i].nor = new Vector3(0, 0, -1);
                pData[startIndex + i].tex = new Vector2((xPos + 1) / 2, (yPos + 1) / 2);
            }
            //left down
            startIndex += nInternalQuaterSteps + 1;
            for (int i = 0; i < nInternalQuaterSteps; i++)
            {
                float xPos = -(1.0f + nInternalQuaterSteps - i) / (nInternalQuaterSteps + 2);
                float yPos = -equation(xPos);

                pData[startIndex + i].pos = new Vector3(xPos / 2, (yPos + 1) / 2, 0);
                pData[startIndex + i].nor = new Vector3(0, 0, -1);
                pData[startIndex + i].tex = new Vector2((xPos + 1) / 2, (yPos + 1) / 2);
            }
            startIndex += nInternalQuaterSteps;

            //center up
            for (int i = 0; i < nInternalQuaterSteps + 1; i++)
            {
                float xPos = (1.0f + i) / (nInternalQuaterSteps + 2);
                float yPos = equation(xPos);

                pData[startIndex + i].pos = new Vector3(0, (yPos + 1) / 2, 0);
                pData[startIndex + i].nor = new Vector3(0, 0, -1);
                pData[startIndex + i].tex = new Vector2(0.5f, 0.5f);
            }
            //center down
            startIndex += nInternalQuaterSteps + 1;
            for (int i = 0; i < nInternalQuaterSteps; i++)
            {
                float xPos = (1.0f + nInternalQuaterSteps - i) / (nInternalQuaterSteps + 2);
                float yPos = -equation(xPos);

                pData[startIndex + i].pos = new Vector3(0, (yPos + 1) / 2, 0);
                pData[startIndex + i].nor = new Vector3(0, 0, -1);
                pData[startIndex + i].tex = new Vector2(0.5f, 0.5f);
            }
            startIndex += nInternalQuaterSteps;


            //generate north pole
            pData[startIndex].pos = new Vector3(0, 1, 0);
            pData[startIndex].nor = new Vector3(0, 0, -1);
            pData[startIndex].tex = new Vector2(0.5f, 1.0f);

            //generate south pole
            pData[startIndex + 1].pos = new Vector3(0, 0, 0);
            pData[startIndex + 1].nor = new Vector3(0, 0, -1);
            pData[startIndex + 1].tex = new Vector2(0.5f, 0.0f);

            //fill in pIndices array

            //fill in side triangles (first 6*radialStep*(heightStep-1))

            //fill in pData array
            startIndex = 0;
            //center and right
            int startIndex1 = 0;//right
            int startIndex2 = 2 * (2 * nInternalQuaterSteps + 1);//center
            for (int i = 0; i < 2 * nInternalQuaterSteps; i++)
            {
                pIndices[startIndex + 6 * i + 0] = (uint)(startIndex2 + i);
                pIndices[startIndex + 6 * i + 1] = (uint)(startIndex1 + i);
                pIndices[startIndex + 6 * i + 2] = (uint)(startIndex1 + i + 1);

                pIndices[startIndex + 6 * i + 3] = (uint)(startIndex2 + i);
                pIndices[startIndex + 6 * i + 4] = (uint)(startIndex1 + i + 1);
                pIndices[startIndex + 6 * i + 5] = (uint)(startIndex2 + i + 1);
            }
            startIndex += 3 * 4 * nInternalQuaterSteps;
            //left and center
            startIndex1 = 2 * (2 * nInternalQuaterSteps + 1);//center
            startIndex2 = 1 * (2 * nInternalQuaterSteps + 1);//left
            for (int i = 0; i < 2 * nInternalQuaterSteps; i++)
            {
                pIndices[startIndex + 6 * i + 0] = (uint)(startIndex2 + i);
                pIndices[startIndex + 6 * i + 1] = (uint)(startIndex1 + i);
                pIndices[startIndex + 6 * i + 2] = (uint)(startIndex1 + i + 1);

                pIndices[startIndex + 6 * i + 3] = (uint)(startIndex2 + i);
                pIndices[startIndex + 6 * i + 4] = (uint)(startIndex1 + i + 1);
                pIndices[startIndex + 6 * i + 5] = (uint)(startIndex2 + i + 1);
            }
            startIndex += 3 * 4 * nInternalQuaterSteps;

            //connect north pole
            uint northPoleIndex = 3 * (2 * (uint)nInternalQuaterSteps + 1);
            pIndices[startIndex + 0] = 2 * (uint)nInternalQuaterSteps + 1;//left top
            pIndices[startIndex + 1] = 2 * (2 * (uint)nInternalQuaterSteps + 1);//center top
            pIndices[startIndex + 2] = northPoleIndex;

            pIndices[startIndex + 3] = 2 * (2 * (uint)nInternalQuaterSteps + 1);//center top
            pIndices[startIndex + 4] = 0;//right top
            pIndices[startIndex + 5] = northPoleIndex;
            startIndex += 3 * 2;

            //connect south pole
            uint southPoleIndex = 3 * (2 * (uint)nInternalQuaterSteps + 1) + 1;
            pIndices[startIndex + 0] = 2 * (uint)nInternalQuaterSteps + 1 + 2 * (uint)nInternalQuaterSteps;//left bottom
            pIndices[startIndex + 1] = southPoleIndex;
            pIndices[startIndex + 2] = 2 * (2 * (uint)nInternalQuaterSteps + 1) + 2 * (uint)nInternalQuaterSteps;//center bottom

            pIndices[startIndex + 3] = 2 * (2 * (uint)nInternalQuaterSteps + 1) + 2 * (uint)nInternalQuaterSteps;//center bottom
            pIndices[startIndex + 4] = southPoleIndex;
            pIndices[startIndex + 5] = 2 * (uint)nInternalQuaterSteps;//right bottom
            startIndex += 3 * 2;
        }
    }
}
