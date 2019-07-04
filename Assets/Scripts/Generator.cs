using System.Collections.Generic;
using UnityEngine;
using ProceduralGenerator;

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
    [Range(3f, 24f)]
    [Tooltip("Controls how far up the starting point will be generated.")]
    public float startingPointHeight = 4f;
    [Range(0.1f, 3f)]
    [Tooltip("Controls how straight or winding the course will be. Higher values = more winding course")]
    public float horizontalStretching = 2f;
    [Range(1, 5)]
    public int waypointSmoothingPasses = 3;
    [Range(5f, 40f)]
    public float roadWidth = 5f;
    public int currentWaypoint = 0;

    public bool useRandomSeed = true;
    public string seed;
    
    [SerializeField] GameObject road;
    [SerializeField] Transform waypoint;
    [SerializeField] Transform finish;
    [SerializeField] GameObject borderHolder;

    Node[,] map;

    Mesh terrainMesh, roadMesh;
    MeshFilter filter, roadFilter;
    MeshCollider meshCollider, roadCollider;

    Vector3[] vertices;
    Vector2[] uv;
    int[] triangles;

    Transform[] waypoints;
    public Transform[] Waypoints
    {
        get
        {
            return waypoints;
        }
    }
    List<Vector3> roadVertices;
    //List<Vector2> roadUV;
    List<int> roadTriangles;

    Transform player;
    LineRenderer minimapRoad;


    private void Awake()
    {
        filter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        roadFilter = road.GetComponent<MeshFilter>();
        roadCollider = road.GetComponent<MeshCollider>();

        terrainMesh = new Mesh();
        roadMesh = new Mesh();
        terrainMesh.name = "Terrain Mesh";
        roadMesh.name = "Road Mesh";

        GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(dimension, dimension);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        minimapRoad = GetComponentInChildren<LineRenderer>();
        
        foreach (Transform border in borderHolder.GetComponentsInChildren<Transform>())
        {
            border.position = border.forward * ((dimension - 2) * scale) / 2f;
        }
    }

    // Use this for initialization
    void Start() {
        map = new Node[dimension, dimension];

        Generate();

        //place player exactly one "segment" into the road
        player.position = waypoints[courseDetail].position + Vector3.up * 2f;
        player.rotation = waypoints[courseDetail].rotation;

        //place finish line exactly one "segment" from the end
        finish.position = waypoints[waypoints.Length - courseDetail].position;
        finish.rotation = Quaternion.Euler(0, waypoints[waypoints.Length - courseDetail].rotation.eulerAngles.y, waypoints[waypoints.Length - courseDetail].rotation.eulerAngles.z);
        player.GetComponentInChildren<WaypointUI>().waypoint = finish;
        player.GetComponent<PlayerMovement>().ResetPosition = waypoints[courseDetail].position;
        player.GetComponent<PlayerMovement>().ResetRotation = waypoints[courseDetail].rotation;
    }

    void Generate()
    {
        //wipe the array clean
        Reset();

        if (useRandomSeed)
            seed = System.DateTime.Now.ToLongTimeString();

        //generate the heightmap
        DiamondSquare ds = new DiamondSquare(map, heightScale);
        ds.GenerateHeightMap(dimension - 1);
        float[,] heightMapValues = ds.ReturnHeightMap();

        for (int x = 0; x < dimension; x++)
        {
            for (int y = 0; y < dimension; y++)
            {
                map[x, y].Height = heightMapValues[x, y];
            }
        }

        //create road course
        CourseCreator cc = new CourseCreator(dimension, numPathPoints, minStep, stepRange, courseDetail, startingPointHeight, horizontalStretching);
        cc.GeneratePath();
        for (int i = 0; i < chaikinPasses; i++)
        {
            cc.ChaikinSmoothing();
        }

        //convert points
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
                triangles[tris + 0] = i + 0;                    //0, 0
                triangles[tris + 1] = i + 1;                    //0, 1
                triangles[tris + 2] = i + dimension;            //1, 0
                //triangle
                triangles[tris + 3] = i + 1;                    //1, 0
                triangles[tris + 4] = i + dimension + 1;        //0, 1
                triangles[tris + 5] = i + dimension;            //1, 1

                tris += 6;
            }
        }

        terrainMesh.vertices = vertices;
        terrainMesh.uv = uv;
        terrainMesh.triangles = triangles;

        terrainMesh.RecalculateNormals();

        filter.sharedMesh = meshCollider.sharedMesh = null;
        filter.sharedMesh = meshCollider.sharedMesh = terrainMesh;

        cc.GenerateCourse();

        waypoints = new Transform[(cc.CoursePoints.Count - 1)/courseDetail];

        for (int i = 0; i < waypoints.Length; i++)
        {
            Debug.DrawLine(cc.CoursePoints[i * courseDetail], cc.CoursePoints[i * courseDetail] + (Vector3.up * 50f), Color.red, 1000f, false);
        }

        //generate and orient the waypoints
        for (int i = 0; i < waypoints.Length; i++)
        {
            int currCoursePoint = i * courseDetail;
            Quaternion newRot = Quaternion.LookRotation((cc.CoursePoints[currCoursePoint] - cc.CoursePoints[currCoursePoint + 1]).normalized);

            if (i == waypoints.Length - 1)
            {
                newRot = Quaternion.LookRotation((cc.CoursePoints[currCoursePoint] - cc.CoursePoints[currCoursePoint - 1]).normalized);
            }

            Transform nodeInst = Instantiate(waypoint, cc.CoursePoints[currCoursePoint], Quaternion.identity, road.transform) as Transform;
            //nodeInst.localScale = new Vector3(roadWidth, 2.5f, 1f);
            nodeInst.name += " " + i;
            nodeInst.localRotation = newRot;
            waypoints[i] = nodeInst;
        }
        
        //smooth rotations
        for (int j = 0; j < waypointSmoothingPasses; j++)
        {
            for (int i = 1; i < waypoints.Length - 1; i++)
            {
                NormaLizeWaypointRotation(waypoints[i], waypoints[i].position - waypoints[i - 1].position, waypoints[i + 1].position - waypoints[i].position);
            }
        }

        //add vertices to road
        for (int i = 0; i < waypoints.Length; i++)
        {
            roadVertices.Add(waypoints[i].position - (waypoints[i].right * (roadWidth / 1.5f)) - (waypoints[i].up * 2.5f));
            roadVertices.Add(waypoints[i].position - (waypoints[i].right * (roadWidth / 2f)));
            roadVertices.Add(waypoints[i].position);
            roadVertices.Add(waypoints[i].position + (waypoints[i].right * (roadWidth / 2f)));
            roadVertices.Add(waypoints[i].position + (waypoints[i].right * (roadWidth / 1.5f)) - (waypoints[i].up * 2.5f));
        }

        //add triangles to road
        //for (int i = 2; i < roadVertices.Count - 2; i += 2)
        for (int i = 5; i < roadVertices.Count-5; i += 5)
        {
            //left faces
            roadTriangles.Add(i);
            roadTriangles.Add(i - 4);
            roadTriangles.Add(i - 5);

            roadTriangles.Add(i);
            roadTriangles.Add(i + 1);
            roadTriangles.Add(i - 4);

            //mid left faces
            roadTriangles.Add(i + 1);
            roadTriangles.Add(i - 3);
            roadTriangles.Add(i - 4);

            roadTriangles.Add(i + 1);
            roadTriangles.Add(i + 2);
            roadTriangles.Add(i - 3);

            //mid right faces
            roadTriangles.Add(i + 2);
            roadTriangles.Add(i - 2);
            roadTriangles.Add(i - 3);

            roadTriangles.Add(i + 2);
            roadTriangles.Add(i + 3);
            roadTriangles.Add(i - 2);

            //right faces
            roadTriangles.Add(i + 3);
            roadTriangles.Add(i - 1);
            roadTriangles.Add(i - 2);

            roadTriangles.Add(i + 3);
            roadTriangles.Add(i + 4);
            roadTriangles.Add(i - 1);
        }

        roadMesh.vertices = roadVertices.ToArray();
        roadMesh.triangles = roadTriangles.ToArray();

        roadMesh.RecalculateNormals();

        roadFilter.sharedMesh = roadCollider.sharedMesh = null;
        roadFilter.sharedMesh = roadCollider.sharedMesh = roadMesh;

        minimapRoad.positionCount = cc.CoursePoints.Count - 1;
        for (int i = 0; i < cc.CoursePoints.Count - 1; i++)
        {
            Vector3 pointToAdd = cc.CoursePoints[i + 1];
            pointToAdd.y = 0;
            minimapRoad.SetPosition(i, pointToAdd + (Vector3.up * 25f));
        }
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

        roadVertices = new List<Vector3>();
        //roadUV = new List<Vector2>();
        roadTriangles = new List<int>();

        terrainMesh.Clear();
        roadMesh.Clear();
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

    //smooth node rotations in the road to prevent glitches
    void NormaLizeWaypointRotation(Transform waypoint, Vector3 angle1, Vector3 angle2)
    {
        Vector3 average = (angle1 + angle2).normalized;
        float yAngle = Mathf.Atan2(average.x, average.z) * Mathf.Rad2Deg;
        waypoint.localRotation = Quaternion.Euler(waypoint.transform.localRotation.eulerAngles.x, yAngle, waypoint.transform.localRotation.eulerAngles.z);
    }
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