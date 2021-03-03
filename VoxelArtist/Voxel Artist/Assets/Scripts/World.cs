using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    //public Material material;
    public VoxelMaterial[] voxelMaterial;

    public Chunk chunk;

    private void Awake()
    {
        /*
        voxelMaterial = new VoxelMaterial[255];

        for (byte i = 0; i < voxelMaterial.Length; i++)
        {
            voxelMaterial[i].textureCoordX = i;
        }
        */
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        if (chunk.isVoxelInfoPopulated)
        {
            return voxelMaterial[chunk.GetVoxelAtPosition(pos)].textureCoordX != 0; // is solid?
        }

        Debug.Log("Not populated yet");
        return false;
    }
}

[System.Serializable]
public class VoxelMaterial
{
    public int textureCoordX;
}
