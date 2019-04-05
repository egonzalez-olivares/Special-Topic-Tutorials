using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGeneration : MonoBehaviour {

    [SerializeField]
    NoiseMapGeneration noiseMapGeneration;

    [SerializeField]
    private MeshRenderer tileRenderer;

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private MeshCollider meshCollider;

    [SerializeField]
    private float levelScale;

    [SerializeField]
    private TerrainType[] terrainTypes;

    [SerializeField]
    private float heightMultiplier;

    [SerializeField]
    private AnimationCurve heightCurve;

    [SerializeField]
    private Wave[] waves;

    void Start()
    {
        GenerateTile();
    }

    void GenerateTile()
    {
        //calculate tile depth and width based on mesh vertices
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileDepth;

        //calculate offsets based on tile position
        float offsetX = -this.gameObject.transform.position.x;
        float offsetZ = -this.gameObject.transform.position.z;

        //generate height map using noise
        float[,] heightMap = this.noiseMapGeneration.GenerateNoiseMap(tileDepth, tileWidth,
            this.levelScale, offsetX, offsetZ, waves);

        //build Texture2D from height map
        Texture2D tileTexture = BuildTexture(heightMap);
        this.tileRenderer.material.mainTexture = tileTexture;

        //update tile mesh vertices according to height map
        UpdateMeshVertices(heightMap);
    }

    void UpdateMeshVertices(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

        Vector3[] meshVertices = this.meshFilter.mesh.vertices;

        //iterate through all height map coords
        int vertexIndex = 0;
        for(int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for(int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                float height = heightMap[zIndex, xIndex];

                Vector3 vertex = meshVertices[vertexIndex];
                //change vertex Y coord, proportional to height value
                //height value evaluated by heightCurve function
                meshVertices[vertexIndex] = new Vector3(vertex.x, this.heightCurve.Evaluate(height) * this.heightMultiplier, vertex.z);

                vertexIndex++;
            }
        }

        //update vertices in mesh and its properties
        this.meshFilter.mesh.vertices = meshVertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();

        //update mesh collider
        this.meshCollider.sharedMesh = this.meshFilter.mesh;
    }

    private Texture2D BuildTexture(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

        Color[] colorMap = new Color[tileDepth * tileWidth];

        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                // transform the 2D map index is an Array index
                int colorIndex = zIndex * tileWidth + xIndex;
                float height = heightMap[zIndex, xIndex];

                // choose a terrain type according to the height value
                TerrainType terrainType = ChooseTerrainType(height);

                // assign the color according to the terrain type
                colorMap[colorIndex] = terrainType.color;
            }
        }

        // create a new texture and set its pixel colors
        Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        return tileTexture;
    }

    TerrainType ChooseTerrainType(float height)
    {
        // for each terrain type, check if the height is lower than the one for the terrain type
        foreach (TerrainType terrainType in terrainTypes)
        {
            // return the first terrain type whose height is higher than the generated one
            if (height < terrainType.height)
            {
                return terrainType;
            }
        }
        return terrainTypes[0];
    }
}

[System.Serializable]
public class TerrainType
{
    public string name;
    public float height;
    public Color color;
}
