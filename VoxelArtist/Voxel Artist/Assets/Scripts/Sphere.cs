using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : MonoBehaviour
{
    private Renderer rend;

    private void Awake()
    {
        //Renderer
        rend = GetComponent<Renderer>();

        //Texture
        Texture2D tex2D = new Texture2D(3, 1)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        tex2D.SetPixel(0, 0, Color.blue);
        tex2D.SetPixel(1, 0, Color.green);
        tex2D.SetPixel(2, 0, Color.red);
        tex2D.Apply();

        //Sprite
        //Sprite spr = Sprite.Create(
        //    tex2D,
        //    new Rect(
        //        0,
        //        0,
        //        tex2D.width,
        //        tex2D.height
        //    ),
        //    new Vector2(
        //        0.5f,
        //        0.5f
        //    )
        //);

        //Assign
        rend.material.mainTexture = tex2D;
        //rend.sprite = spr;
    }
}
