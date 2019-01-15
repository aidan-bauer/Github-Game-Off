using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralGenerator
{
    public class CourseCreator
    {

        int dimension, numPathPoints, detail;
        float segmentLength, minStep, maxStepRange;
        float startingPointHeight, horizontalStretching;
        List<Vector2> pathPoints;
        List<Vector3> coursePoints;

        public List<Vector2> PathPoints
        {
            get
            {
                return pathPoints;
            }
            set
            {
                pathPoints = value;
            }
        }

        public List<Vector3> CoursePoints
        {
            get
            {
                return coursePoints;
            }
        }

        //generate path points
        //transform path points into world space
        //use those path points to generate the course

        public CourseCreator(int _dimension, int _pathPoints, float _minStep, float _maxStepRange, int _detail, float _startingPointHeight, float _horizontalStretching)
        {
            dimension = _dimension;
            numPathPoints = _pathPoints;
            detail = _detail;
            maxStepRange = _maxStepRange;
            minStep = _minStep;
            startingPointHeight = _startingPointHeight;
            horizontalStretching = _horizontalStretching;

            segmentLength = Random.Range(minStep, maxStepRange);
            pathPoints = new List<Vector2>();
            coursePoints = new List<Vector3>();

            //pathPoints.Add(Abs(Random.insideUnitCircle.normalized) * dimension);
            pathPoints.Add(new Vector2(Random.Range(10f, dimension - 10), Random.Range(3f, startingPointHeight)));

            //we will raycast down to find the exact point on the generated terrain beneath the generated point
        }

        Vector2 Abs(Vector2 point)
        {
            return new Vector2(Mathf.Abs(point.x), Mathf.Abs(point.y));
        }

        bool IsPointInBounds(Vector2 point, float buffer = 0)
        {
            return point.x <= dimension - buffer && point.y <= dimension - buffer && point.x >= buffer && point.y >= buffer;
        }

        bool IsInXBound(Vector2 point)
        {
            return point.x <= dimension && point.x >= 0;
        }

        bool IsInYBound(Vector2 point)
        {
            return point.y <= dimension && point.y >= 0;
        }

        Vector2 GenerateRandomPoint()
        {
            Vector2 randomPos = Random.insideUnitCircle;
            randomPos.y = Mathf.Abs(randomPos.y / horizontalStretching);
            return randomPos;
        }

        public void GeneratePath()
        {
            //for (int i = 1; i < pathPoints.Length; i++)
            for (int i = 1; i < numPathPoints; i++)
            {
                segmentLength = Random.Range(minStep, maxStepRange);
                Vector2 nextPos = pathPoints[i - 1] + (GenerateRandomPoint() * segmentLength);

                while (!IsPointInBounds(nextPos, 5f))
                {
                    nextPos = pathPoints[i - 1] + (GenerateRandomPoint() * segmentLength);
                }

                pathPoints.Add(nextPos);
                segmentLength = Random.Range(minStep, maxStepRange);
            }
        }

        //smooth out the 
        public void ChaikinSmoothing()
        {
            List<Vector2> newPathPoints = new List<Vector2>();

            //first and last vertices are the same
            newPathPoints.Insert(0, pathPoints[0]);
            newPathPoints.Add(pathPoints[pathPoints.Count - 1]);

            int j = 1;
            for (int i = 0; i < pathPoints.Count - 2; i++)
            {
                newPathPoints.Insert(j, pathPoints[i] + (pathPoints[i + 1] - pathPoints[i]) * 0.75f);
                newPathPoints.Insert(j + 1, pathPoints[i + 1] + (pathPoints[i + 2] - pathPoints[i + 1]) * 0.25f);
                j += 2;
            }

            pathPoints = newPathPoints;
        }

        public void GenerateCourse()
        {
            for (int i = 1; i < pathPoints.Count; i++)
            {
                RaycastHit hit;

                int cast = 0;
                while (cast < detail)
                {
                    //Vector3 castPos;
                    Vector2 dir = pathPoints[i - 1] + (pathPoints[i] - pathPoints[i - 1]) * (cast * (1f / detail));

                    //raycast directly down from each path point
                    if (Physics.Raycast(new Vector3(dir.x, 100, dir.y), Vector3.down, out hit, 150f))
                    {
                        if (hit.collider.CompareTag("Terrain"))
                        {
                            coursePoints.Add(hit.point + (Vector3.up * 2f));
                        }
                    }

                    cast++;
                }
            }
        }
    }
}
