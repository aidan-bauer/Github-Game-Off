using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralGenerator
{
    public class DiamondSquare
    {
        float[,] heightMap;
        float randomHeight;

        public DiamondSquare(Node[,] _heightMap, float _randomHeight)
        {
            heightMap = new float[_heightMap.GetLength(0), _heightMap.GetLength(1)];

            for (int x = 0; x < _heightMap.GetLength(0); x++)
            {
                for (int y = 0; y < _heightMap.GetLength(1); y++)
                {
                    heightMap[x, y] = 0;
                }
            }

            randomHeight = _randomHeight;

            //set initial values of the four corners
            heightMap[0, 0] = Random.Range(-randomHeight / 2f, randomHeight);
            heightMap[0, heightMap.GetLength(0) - 1] = Random.Range(-randomHeight / 2f, randomHeight);
            heightMap[heightMap.GetLength(1) - 1, 0] = UnityEngine.Random.Range(-randomHeight / 2f, randomHeight);
            heightMap[heightMap.GetLength(0) - 1, heightMap.GetLength(0) - 1] = Random.Range(-randomHeight / 2f, randomHeight);
        }

        public void GenerateHeightMap(int startingStepSize)
        {
            int stepSize = startingStepSize;

            while (stepSize > 1)
            {
                int halfStep = Mathf.FloorToInt(stepSize / 2);

                //run diamond step first
                for (int x = 0; x < heightMap.GetLength(0) - 1; x += stepSize)
                {
                    for (int y = 0; y < heightMap.GetLength(1) - 1; y += stepSize)
                    {
                        DiamondStep(x, y, stepSize, Random.Range(-randomHeight / 2f, randomHeight));
                    }
                }

                for (int x = 0; x < heightMap.GetLength(0); x += halfStep)
                {
                    for (int y = 0; y < heightMap.GetLength(1); y += halfStep)
                    {
                        SquareStep(x, y, halfStep, Random.Range(-randomHeight / 2f, randomHeight));
                    }
                }

                stepSize /= 2;
                randomHeight /= 1.25f;
            }
        }

        void DiamondStep(int x, int y, int stepSize, float randomOffset)
        {
            //assume x and y are the top left corner of the square
            //find average value
            float averageValue = (heightMap[x, y] + heightMap[x + stepSize, y] + heightMap[x, y + stepSize] + heightMap[x + stepSize, y + stepSize]) / 4;
            //set midpoint to average of nodesToAverage + randomized amount
            heightMap[x + stepSize / 2, y + stepSize / 2] = averageValue + randomOffset;
        }

        void SquareStep(int x, int y, int stepSize, float randomOffset)
        {
            //assume x and y are the center of the diamond
            //find average value while making sure its inside the array bounds
            float left = x - stepSize < 0 ? 0 : heightMap[x - stepSize / 2, 0];
            float right = x + stepSize >= heightMap.GetLength(0) ? 0 : heightMap[x + stepSize / 2, 0];
            float up = y - stepSize < 0 ? 0 : heightMap[0, y - stepSize / 2];
            float down = y + stepSize >= heightMap.GetLength(0) ? 0 : heightMap[0, y + stepSize / 2];
            float averageValue = (left + right + up + down) / 4f;
            //set midpoint to average of nodesToAverage + randomized amount
            heightMap[x, y] = averageValue + randomOffset;
        }

        public float[,] ReturnHeightMap()
        {
            return heightMap;
        }
    }
}
