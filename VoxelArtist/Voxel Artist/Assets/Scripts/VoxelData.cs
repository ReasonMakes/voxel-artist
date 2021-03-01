using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelData : MonoBehaviour
{
    public static readonly int CHUNK_DIMENSION_INDEX_X = 0;
    public static readonly int CHUNK_DIMENSION_INDEX_Y = 1;
    public static readonly int CHUNK_DIMENSION_INDEX_Z = 2;
    public static readonly int[] chunkDimensions = new int[3]
    {
        8, //X (width)
        8, //Y (height)
        8  //Z (depth)
    }; 

    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        new Vector3(0f, 0f, 0f),
        new Vector3(1f, 0f, 0f),
        new Vector3(1f, 1f, 0f),
        new Vector3(0f, 1f, 0f),
        new Vector3(0f, 0f, 1f),
        new Vector3(1f, 0f, 1f),
        new Vector3(1f, 1f, 1f),
        new Vector3(0f, 1f, 1f)
    };

    public static readonly Vector3[] faceChecks = new Vector3[6]
    {
        new Vector3(0f, 0f, -1f), //Back face
        new Vector3(0f, 0f, 1f),  //Front face
        new Vector3(0f, 1f, 0f),  //Top face
        new Vector3(0f, -1f, 0f), //Bottom face
        new Vector3(-1f, 0f, 0f), //Left face
        new Vector3(1f, 0f, 0f)   //Right face
    };

    public static readonly int[,] voxelTris = new int[6, 4]{

        {0, 3, 1, 2}, //Back face
        {5, 6, 4, 7}, //Front face
        {3, 7, 2, 6}, //Top face
        {1, 5, 0, 4}, //Bottom face
        {4, 7, 0, 3}, //Left face
        {1, 2, 5, 6}  //Right face

        //{0, 3, 1, 1, 3, 2}, //Back face
        //{5, 6, 4, 4, 6, 7}, //Front face
        //{3, 7, 2, 2, 7, 6}, //Top face
        //{1, 5, 0, 0, 5, 4}, //Bottom face
        //{4, 7, 0, 0, 7, 3}, //Left face
        //{1, 2, 5, 5, 2, 6}  //Right face
    };
}
