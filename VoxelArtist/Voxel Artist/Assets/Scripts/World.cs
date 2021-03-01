using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material material;
    public VoxelMaterial[] voxelMaterial;
}

[System.Serializable] 
public class VoxelMaterial
{
    public string name;
    public bool isSolid;
}