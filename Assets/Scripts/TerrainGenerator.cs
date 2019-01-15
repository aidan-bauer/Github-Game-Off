using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralGenerator;

public class TerrainGenerator : MonoBehaviour {

    Terrain terrain;
    Node[,] terrainMap;

    [SerializeField] Material terrainMat;

    [Tooltip("Values can only be 2^n+1 (5, 17, 33, etc.).")]
    public int dimension = 33;
    [Range(1f, 50f)]
    public float scale = 1f;
    [Tooltip("Controls how far up the starting point will be generated.")]
    public float startingPointHeight = 4f;

    private void Awake()
    {
        terrain = GetComponent<Terrain>();
    }

    // Use this for initialization
    void Start () {
        terrain.terrainData.heightmapResolution = 0;
        terrain.terrainData.size = Vector3.one;

        terrainMap = new Node[dimension, dimension];
        terrain.terrainData.size = new Vector3(dimension, startingPointHeight, dimension);
        terrain.terrainData.heightmapResolution = dimension;

        Generate();
	}
	
	void Generate()
    {
        DiamondSquare ds = new DiamondSquare(terrainMap, startingPointHeight);
        ds.GenerateHeightMap(dimension-1);
        float[,] heightMap = ds.ReturnHeightMap();
        terrain.terrainData.SetHeights(0, 0, heightMap);
    }
}
