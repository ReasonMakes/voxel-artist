using System;
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

    [System.NonSerialized] public byte[,,] voxelInfo = new byte[
        VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_X],
        VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Y],
        VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Z]
    ];

    public bool isVoxelInfoPopulated = false;

    //public Material paletteMat;
    public Color[] palette = new Color[byte.MaxValue];
    private Renderer rend;

    private void Awake()
    {
        //Renderer
        rend = GetComponent<Renderer>();

        //Default colours
        for (int x = 1; x < palette.Length; x++) //skip 0 as it's void
        {
            if (x == 1)
            {
                palette[x] = Color.gray;
            }
            else if (x == 2)
            {
                palette[x] = Color.red;
            }
            else if (x == palette.Length - 1)
            {
                palette[x] = Color.green;
            }
            else if (x == palette.Length)
            {
                palette[x] = Color.magenta;
            }
            else
            {
                palette[x] = Color.cyan;
            }
        }

        //Material
        UpdateMaterial();
    }

    private void Start()
    {
        //world = GameObject.Find("World").GetComponent<World>();

        SetDefaultVoxelInfo();
        UpdateChunkMesh();
    }

    void SetDefaultVoxelInfo()
    {
        for (int y = 0; y < VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Y]; y++)
        {
            for (int x = 0; x < VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_X]; x++)
            {
                for (int z = 0; z < VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Z]; z++)
                {
                    if (y == 4)
                    {
                        voxelInfo[x, y, z] = 2;
                    }
                    else if (y == 3)
                    {
                        voxelInfo[x, y, z] = 1;
                    }
                    else
                    {
                        voxelInfo[x, y, z] = 0;
                    }
                }
            }
        }

        isVoxelInfoPopulated = true;
    }

    public void SetVoxelInfo(Vector3 voxelPos, byte newVoxelInfo)
    {
        int checkX = Mathf.FloorToInt(voxelPos.x);
        int checkY = Mathf.FloorToInt(voxelPos.y);
        int checkZ = Mathf.FloorToInt(voxelPos.z);

        //Avoid out of range errors (we cannot access nearby chunks this way)
        if (AreCoordsOutOfChunk(checkX, checkY, checkZ))
        {
            //Exit the method; don't edit at all
            return;
        }

        voxelInfo[checkX, checkY, checkZ] = newVoxelInfo;

        UpdateChunkMesh();
    }

    /*
    void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 voxelPos = new Vector3(x, y, z);

        int totalFacesOnAVoxel = 6;
        for (int face = 0; face < totalFacesOnAVoxel; face++)
        {
            Vector3 currentVoxel = voxelPos + VoxelData.faceChecks[face];

            
        }
    }
    */

    public byte GetVoxelAtPosition(Vector3 voxelPos)
    {
        int checkX = Mathf.FloorToInt(voxelPos.x);
        int checkY = Mathf.FloorToInt(voxelPos.y);
        int checkZ = Mathf.FloorToInt(voxelPos.z);

        //Avoid out of range errors (we cannot access nearby chunks this way)
        if (AreCoordsOutOfChunk(checkX, checkY, checkZ))
        {
            //Return void voxel by default
            return 0;
        }

        return voxelInfo[checkX, checkY, checkZ];
    }

    void UpdateChunkMesh()
    {

        ClearMeshData();

        for (int y = 0; y < VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Y]; y++)
        {
            for (int x = 0; x < VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_X]; x++)
            {
                for (int z = 0; z < VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Z]; z++)
                {
                    if (voxelInfo[x, y, z] != 0) //0 is void
                    {
                        AddVoxelToSolidMesh(x, y, z);
                    }
                }
            }
        }

        BuildMesh();
    }

    void ClearMeshData()
    {
        vertIndex = 0;
        verts.Clear();
        tris.Clear();
        uvs.Clear();
    }

    bool IsVoxelAgainstThisFace(int voxelX, int voxelY, int voxelZ)
    {
        //Avoid out of range errors (we cannot access nearby chunks this way)
        if (AreCoordsOutOfChunk(voxelX, voxelY, voxelZ))
        {
            //Return no voxel against face by default
            return false;
        }

        //Return answer
        return voxelInfo[voxelX, voxelY, voxelZ] != 0; //Is solid?
        //return voxelInfo[x, y, z];
    }

    void AddVoxelToSolidMesh(int voxelX, int voxelY, int voxelZ)
    {
        int vertsPerFace = 6;
        for (int triDimension1 = 0; triDimension1 < vertsPerFace; triDimension1++)
        {
            //Cull non-visible faces
            if (!IsVoxelAgainstThisFace
                (
                    voxelX + (int)VoxelData.faceChecks[triDimension1].x,
                    voxelY + (int)VoxelData.faceChecks[triDimension1].y,
                    voxelZ + (int)VoxelData.faceChecks[triDimension1].z
                )
            )
            {
                byte voxelInfoIndex = voxelInfo[voxelX, voxelY, voxelZ];

                int uniqueVertices = 4;
                for(int vertIndex = 0; vertIndex < uniqueVertices; vertIndex++)
                {
                    verts.Add(new Vector3(voxelX, voxelY, voxelZ) + VoxelData.voxelVerts[VoxelData.voxelTris[triDimension1, vertIndex]]);

                    //Debug.Log(voxelInfoIndex);
                    //uvs.Add(new Vector2(world.voxelMaterial[voxelInfoIndex].textureCoordX, 0f)); //Texture
                    uvs.Add(new Vector2(voxelInfoIndex, 0f));
                    //uvs.Add(new Vector2(0.667f, 0f));

                    //0.666 start + 1 (grey)
                    //0.667 end - 1 (green)
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

    public static bool AreCoordsOutOfChunk(int x, int y, int z)
    {
        if
        (
               x < 0 || x > VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_X] - 1
            || y < 0 || y > VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Y] - 1
            || z < 0 || z > VoxelData.chunkDimensions[VoxelData.CHUNK_DIMENSION_INDEX_Z] - 1
        )
        {
            //Debug.LogError("Out of range");
            return true;
        }

        //Else
        return false;
    }

    public void UpdateMaterial()
    {
        //Texture
        Texture2D tex2D = new Texture2D(palette.Length, 1)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int x = 1; x < palette.Length; x++) //skip 0 as it's void
        {
            tex2D.SetPixel(x, 0, palette[x]);
        }
        //tex2D.SetPixel(0, 0, Color.blue);
        //tex2D.SetPixel(1, 0, Color.green);
        //tex2D.SetPixel(2, 0, Color.red);

        tex2D.Apply();

        //Assign
        rend.material.mainTexture = tex2D;
    }
}