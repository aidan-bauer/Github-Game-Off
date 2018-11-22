using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour {

    [Tooltip("Values can only be 2^n+1 (5, 17, 33, etc.).")]
    public int dimension = 33;
    public int numPathPoints = 25;
    [Range(2, 20)]
    [SerializeField] int courseDetail = 10;     //how many linecasts this will cast between points
    public float minStep = 4f, stepRange = 5f;
    [Range(1, 10)]
    public int chaikinPasses = 3;
    [Range(1f, 50f)]
    public float scale = 1f;
    [Range(0.1f, 2f)]
    public float heightScale = 1f;

    public bool useRandomSeed = true;
    public string seed;

    public LineRenderer road;

    Node[,] map;

    Mesh terrainMesh;
    Material terrainMat;
    MeshFilter filter;
    MeshCollider meshCollider;

    Vector3[] vertices;
    Vector2[] uv;
    int[] triangles;


    private void Awake()
    {
        filter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        terrainMesh = new Mesh();
        terrainMesh.name = "Terrain Mesh";
        //filter.sharedMesh = meshCollider.sharedMesh = terrainMesh;

        GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(dimension, dimension);
    }

    // Use this for initialization
    void Start() {
        map = new Node[dimension, dimension];

        Generate();
    }

    void Generate()
    {
        //wipe the array clean
        Reset();

        if (useRandomSeed)
            seed = System.DateTime.Now.ToLongTimeString();

        //generate the heightmap
        DiamondSquare ds = new DiamondSquare(map, 5);
        ds.GenerateHeightMap(dimension - 1);
        float[,] heightMapValues = ds.ReturnHeightMap();

        for (int x = 0; x < dimension; x++)
        {
            for (int y = 0; y < dimension; y++)
            {
                map[x, y].Height = heightMapValues[x, y] * heightScale;
            }
        }

        //create road course
        CourseCreator cc = new CourseCreator(dimension, numPathPoints, minStep, stepRange, courseDetail);
        cc.GeneratePath();
        for (int i = 0; i < chaikinPasses; i++)
        {
            cc.ChaikinSmoothing();
        }

        //convert points
        //for (int i = 0; i < numPathPoints; i++)
        for (int i = 0; i < cc.PathPoints.Count; i++)
        {
            Vector2 convertedPoint = CenterAroundZero2D(cc.PathPoints[i]);
            cc.PathPoints[i] = convertedPoint;
        }

        //add data one square at a time
        //for each one add 4 verts, 4 UV points, and 2 triangles (6 entries)

        int tris = 0;

        for (int x = 0, i = 0; x < dimension - 1; x++, i++)
        {
            //Debug.Log(x + "; " + i + ", " + (i + 1) + ", " + (i + dimension) + ", " + (i + dimension + 1));
            for (int y = 0; y < dimension - 1; y++, i++)
            {
                //add verts
                vertices[i] = CenterAroundZero(map[x, y].ReturnPosition());                           //vertex in question                    (0, 0)
                vertices[i + 1] = CenterAroundZero(map[x + 1, y].ReturnPosition());                   //the one directly to the right of it   (1, 0)
                vertices[i + dimension] = CenterAroundZero(map[x, y + 1].ReturnPosition());           //the one directly above it             (0, 1)
                vertices[i + dimension + 1] = CenterAroundZero(map[x + 1, y + 1].ReturnPosition());   //the one above and to the right        (1, 1)

                uv[i] = new Vector2((float)x / dimension, (float)y / dimension);
                uv[i + 1] = new Vector2((float)(x + 1) / dimension, (float)(y) / dimension);
                uv[i + dimension] = new Vector2((float)(x) / dimension, (float)(y + 1) / dimension);
                uv[i + dimension + 1] = new Vector2((float)(x + 1) / dimension, (float)(y + 1) / dimension);

                //triangle
                /*triangles[tris + 0] = i + 0;                  //0, 0
                triangles[tris + 1] = i + dimension;            //0, 1
                triangles[tris + 2] = i + 1;                    //1, 0*/
                triangles[tris + 0] = i + 0;                    //0, 0
                triangles[tris + 1] = i + 1;                    //0, 1
                triangles[tris + 2] = i + dimension;            //1, 0
                //triangle
                triangles[tris + 3] = i + 1;                    //1, 0
                triangles[tris + 4] = i + dimension + 1;        //0, 1
                triangles[tris + 5] = i + dimension;            //1, 1

                tris += 6;

                /*if (x == 2)
                {
                    Debug.Log((i) + ", " + (x * dimension + y));
                }*/

                /*if (x > dimension-2)
                {
                    Debug.Log(x + ", " + y + ", " + "; " +i + ", " + (i + 1) + ", " + (i + dimension) + ", " + (i + dimension + 1));
                }*/
            }
        }

        terrainMesh.vertices = vertices;
        terrainMesh.uv = uv;
        terrainMesh.triangles = triangles;

        terrainMesh.RecalculateNormals();
        //terrainMesh.RecalculateTangents();

        filter.sharedMesh = meshCollider.sharedMesh = null;
        filter.sharedMesh = meshCollider.sharedMesh = terrainMesh;

        cc.GenerateCourse();

        for (int i = 0; i < cc.CoursePoints.Count; i++)
        {
            Debug.DrawLine(cc.CoursePoints[i], cc.CoursePoints[i] + (Vector3.up * 50f), Color.red, 1000f, false);
        }

        /*for (int i = 1; i < cc.CoursePoints.Count; i++)
        {
            Debug.DrawLine(cc.CoursePoints[i - 1], cc.CoursePoints[i], Color.green, 1000f, false);
        }*/

        road.positionCount = cc.CoursePoints.Count;
        road.SetPositions(cc.CoursePoints.ToArray());
    }

    private void Reset()
    {
        for (int x = 0; x < dimension; x++)
        {
            for (int y = 0; y < dimension; y++)
            {
                map[x, y] = new Node(x, y);     //completely reset the node
            }
        }

        vertices = new Vector3[(dimension) * (dimension)];
        uv = new Vector2[vertices.Length];
        triangles = new int[(dimension) * (dimension) * 6];

        terrainMesh.Clear();
    }

    //display a point as if the it's centered around zero
    Vector3 CenterAroundZero(Vector3 point)
    {
        return new Vector3(point.x - (dimension / 2) + 0.5f, point.y, point.z - (dimension / 2) + 0.5f) * scale;
    }

    public Vector2 CenterAroundZero2D(Vector2 point)
    {
        return new Vector2(point.x - (dimension / 2f) + 0.5f, point.y - (dimension / 2f) + 0.5f) * scale;
    }

    //one section of the grid
    public struct Node
    {
        int nodeX, nodeY;
        float height;

        public float Height
        {
            set
            {
                height = value;
            }
        }

        public Node(int x, int y)
        {
            nodeX = x;
            nodeY = y;
            height = 0;
        }

        public Vector3 ReturnPosition()
        {
            return new Vector3(nodeX, height, nodeY);
        }
    }

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
            heightMap[0, 0] = Random.Range(-randomHeight / 2, randomHeight);
            heightMap[0, heightMap.GetLength(0) - 1] = Random.Range(-randomHeight / 2, randomHeight);
            heightMap[heightMap.GetLength(1) - 1, 0] = UnityEngine.Random.Range(-randomHeight / 2, randomHeight);
            heightMap[heightMap.GetLength(0) - 1, heightMap.GetLength(0) - 1] = Random.Range(-randomHeight / 2, randomHeight);
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
                        DiamondStep(x, y, stepSize, Random.Range(-randomHeight, randomHeight));
                    }
                }

                for (int x = 0; x < heightMap.GetLength(0); x += halfStep)
                {
                    for (int y = 0; y < heightMap.GetLength(1); y += halfStep)
                    {
                        SquareStep(x, y, halfStep, Random.Range(-randomHeight, randomHeight));
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

    public class CourseCreator {

        int dimension, numPathPoints, detail;
        float segmentLength, minStep, maxStepRange;
        //Vector2[] pathPoints;
        //Vector3[] coursePoints;
        List<Vector2> pathPoints;
        List<Vector3> coursePoints;

        /*public Vector2[] PathPoints
        {
            get
            {
                //return pathPoints;
                return pathPoints.ToArray();
            }
            set
            {
                //pathPoints = value.OfType<Vector2>().ToList();
                pathPoints = new List<Vector2>(value);
            }
        }

        public Vector3[] CoursePoints
        {
            get
            {
                //return coursePoints;
                return coursePoints.ToArray();
            }
        }*/

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

        public CourseCreator(int _dimension, int _pathPoints, float _minStep, float _maxStepRange, int _detail)
        {
            dimension = _dimension;
            numPathPoints = _pathPoints;
            detail = _detail;
            maxStepRange = _maxStepRange;
            minStep = _minStep;
            segmentLength = Random.Range(minStep, maxStepRange);
            //pathPoints = new Vector2[_pathPoints + ((_pathPoints-1) * _detail)];
            //coursePoints = new Vector3[_pathPoints];
            pathPoints = new List<Vector2>();
            coursePoints = new List<Vector3>();

            //Vector2 randStart = Random.insideUnitCircle;
            //pathPoints[0] = Abs(Random.insideUnitCircle) * dimension;
            pathPoints.Add(Abs(Random.insideUnitCircle.normalized) * dimension);

            //we will raycast down to find the exact point on the generated terrain beneath the generated point
        }

        Vector2 Abs(Vector2 point)
        {
            return new Vector2(Mathf.Abs(point.x), Mathf.Abs(point.y));
        }

        bool IsPointInBounds(Vector2 point)
        {
            return point.x <= dimension && point.y <= dimension && point.x >= 0 && point.y >= 0;
        }

        bool IsInXBound(Vector2 point)
        {
            return point.x <= dimension && point.x >= 0;
        }

        bool IsInYBound(Vector2 point)
        {
            return point.y <= dimension && point.y >= 0;
        }

        public void GeneratePath()
        {
            //for (int i = 1; i < pathPoints.Length; i++)
            for (int i = 1; i < numPathPoints; i++)
            {
                Vector2 nextPos = pathPoints[i - 1] + (Random.insideUnitCircle.normalized * segmentLength);

                while (!IsPointInBounds(nextPos))
                {
                    nextPos = pathPoints[i - 1] + (Random.insideUnitCircle.normalized * segmentLength);
                }

                if (i >= 2)
                {
                    Vector2 previousAngle = pathPoints[i - 1] - pathPoints[i - 2];
                    Vector2 nextAngle = pathPoints[i - 1] - nextPos;

                    float dot = Vector2.Dot(previousAngle.normalized, nextAngle.normalized);
                    //Debug.Log(dot);

                    //if path doubles back on itself
                    /*while (dot > 0.1f)
                    {
                        nextPos = pathPoints[i - 1] + (Random.insideUnitCircle.normalized * segmentLength);

                        previousAngle = pathPoints[i - 1] - pathPoints[i - 2];
                        nextAngle = pathPoints[i - 1] - nextPos;
                        dot = Vector2.Dot(previousAngle.normalized, nextAngle.normalized);
                        
                        Debug.Log(dot + " revised");
                    }*/
                }

                /*if (!IsInXBound(nextPos))
                    nextPos.x *= -1;

                if (!IsInYBound(nextPos))
                    nextPos.y *= -1;*/
                //Debug.Log(segmentLength);

                //pathPoints[i] = nextPos;
                pathPoints.Add(nextPos);
                segmentLength = Random.Range(minStep, maxStepRange);
            }
        }

        //smooth out the 
        public void ChaikinSmoothing()
        {
            //Vector2[] newPathPoints = new Vector2[(pathPoints.Length - 2) * 2 + 2];
            List<Vector2> newPathPoints = new List<Vector2>();

            //first and last vertices are the same
            //newPathPoints[0] = pathPoints[0];
            //newPathPoints[newPathPoints.Length - 1] = pathPoints[pathPoints.Length - 1];
            newPathPoints.Insert(0, pathPoints[0]);
            newPathPoints.Add(pathPoints[pathPoints.Count - 1]);
            
            int j = 1;
            for (int i = 0; i < pathPoints.Count - 2; i++)
            {
                //newPathPoints[j] = pathPoints[i] + (pathPoints[i + 1] - pathPoints[i]) * 0.75f;
                //newPathPoints[j + 1] = pathPoints[i + 1] + (pathPoints[i + 2] - pathPoints[i + 1]) * 0.25f;
                newPathPoints.Insert(j, pathPoints[i] + (pathPoints[i + 1] - pathPoints[i]) * 0.75f);
                newPathPoints.Insert(j + 1, pathPoints[i + 1] + (pathPoints[i + 2] - pathPoints[i + 1]) * 0.25f);
                j += 2;
            }

            pathPoints = newPathPoints;
        }

        public void GenerateCourse()
        {
            //for (int i = 1; i < coursePoints.Length; i++)
            for (int i = 1; i < pathPoints.Count; i++)
            {
                RaycastHit hit;

                int cast = 0;
                while (cast < detail)
                {
                    //Vector3 castPos;
                    Vector2 dir = pathPoints[i - 1] + (pathPoints[i] - pathPoints[i - 1]) * (cast * (1f / detail));
                    //prevent first position from being multiplied by zero
                    /*if (cast == 0)
                    {
                        castPos = pathPoints[i - 1];
                    } else
                    {
                        castPos = dir * (cast * (1f / detail));
                    }*/

                    //raycast directly down from each path point
                    if (Physics.Raycast(new Vector3(dir.x, 100, dir.y), Vector3.down, out hit, 150f))
                    {
                        //coursePoints[i] = hit.point;
                        if (hit.collider.CompareTag("Terrain"))
                        {
                            //coursePoints[i] = hit.point;
                            coursePoints.Add(hit.point);
                        }
                    }

                    cast++;
                }

                //raycast directly down from each path point
                /*if (Physics.Raycast(new Vector3(pathPoints[i].x, 100, pathPoints[i].y), Vector3.down, out hit, 150f))
                {
                    //coursePoints[i] = hit.point;
                    if (hit.collider.CompareTag("Terrain"))
                    {
                        //coursePoints[i] = hit.point;
                        coursePoints.Add(hit.point);
                    }
                }*/
            }
        }
    }
}
