using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    int vertIndex = 0;
    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();
    List<Vector2> uvs = new List<Vector2>(); //Texture

    byte[,,] voxelInfo = new byte[
        VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_X],
        VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Y],
        VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Z]
    ];

    World world;

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();

        PopulateVoxelMap();
        AddAllVoxelsInChunkToMesh();
        BuildMesh();
    }

    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Y]; y++)
        {
            for (int x = 0; x < VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_X]; x++)
            {
                for (int z = 0; z < VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Z]; z++)
                {
                    voxelInfo[x, y, z] = 0;
                }
            }
        }
    }

    void AddAllVoxelsInChunkToMesh()
    {
        for (int y = 0; y < VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Y]; y++)
        {
            for (int x = 0; x < VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_X]; x++)
            {
                for (int z = 0; z < VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Z]; z++)
                {
                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    bool IsVoxelAgainstThisFace(Vector3 pos)
    {
        //Convert float positions to ints usable by arrays
        //Use this if unsure of position being an integer: Mathf.FloorToInt
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        //Avoid out of range errors (we cannot access nearby chunks this way)
        if
        (
               x < 0 || x > VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_X] - 1
            || y < 0 || y > VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Y] - 1
            || z < 0 || z > VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Z] - 1
        )
        {
            //Debug.LogError("Out of range");
            return false;
        }

        //Return answer
        return world.voxelMaterial[voxelInfo[x, y, z]].isSolid;
        //return voxelInfo[x, y, z];
    }

    void AddVoxelDataToChunk(Vector3 pos)
    {
        int vertsPerFace = 6;
        for (int triDimension1 = 0; triDimension1 < vertsPerFace; triDimension1++)
        {
            //Cull non-visible faces
            if (!IsVoxelAgainstThisFace(pos + VoxelData.faceChecks[triDimension1]))
            {
                int uniqueVertices = 4;
                for(int vertIndex = 0; vertIndex < uniqueVertices; vertIndex++)
                {
                    verts.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[triDimension1, vertIndex]]);

                    uvs.Add(Vector2.zero); //Texture
                }

                //Add each vertex per face
                tris.Add(vertIndex + 0);
                tris.Add(vertIndex + 1);
                tris.Add(vertIndex + 2);
                tris.Add(vertIndex + 2);
                tris.Add(vertIndex + 1);
                tris.Add(vertIndex + 3);

                vertIndex += 4;

                /*
                for (int triDimension2 = 0; triDimension2 < vertsPerFace; triDimension2++)
                {
                    int triIndex = VoxelData.voxelTris[triDimension1, triDimension2];

                    verts.Add(VoxelData.voxelVerts[triIndex] + pos);
                    tris.Add(vertIndex);
                    uvs.Add(Vector2.zero); //Texture

                    vertIndex++;
                }
                */
            }
        }
    }

    void BuildMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = verts.ToArray(),
            triangles = tris.ToArray(),
            uv = uvs.ToArray() //Texture
        };

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}
