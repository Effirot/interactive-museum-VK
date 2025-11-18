using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatSpriteRenderer : MonoBehaviour
{
    //public Texture2D texture;
    public Material material;
    public Mesh mesh;
    public float rotateZ;
    public bool useShaderFlipbook;
    public int frame;
    public float glowIntensity;

    //public void Start()
    //{
    //    //mesh = createMesh();
    //}

    public void Update()
    {

        render();
    }

    private void OnRenderObject()
    {
        
    }

    private void OnPostRender()
    {
    }

    //public void OnDrawGizmos()
    //{
    //    render();
    //}

    public void render()
    {
        //Debug.Log("render");
        //mesh = createMesh();
        MaterialPropertyBlock materialProperty = new MaterialPropertyBlock();
        
        if (useShaderFlipbook)
        {
            materialProperty.SetFloat("_frame", frame);
            materialProperty.SetFloat("_GlowIntensity", glowIntensity);

        }
        Vector3 f = (UserInterface.positionCamera - this.transform.position).normalized;
        Vector2 pw = MyUtils.Vector3ToPitchYaw(f);
        Quaternion quaternion = Quaternion.Euler(pw.x, -pw.y, rotateZ);//Quaternion.identity     , null, 0, null
        
        Graphics.DrawMesh(mesh, Matrix4x4.TRS(this.transform.position, quaternion, this.transform.lossyScale), material, 0, null, 0, materialProperty);
    }

    //public static Mesh createMesh()
    //{
    //    Mesh mesh = new Mesh();
    //    mesh.vertices = new Vector3[4];
    //    mesh.triangles = new int[6];
    //    mesh.uv = new Vector2[4];
    //    mesh.colors = new Color[4];

    //    float size = 1000;
    //    mesh.vertices[0] = new Vector3(-size, -size, 0); //  . 
    //    mesh.vertices[1] = new Vector3(-size, size, 0);  //  ^
    //    mesh.vertices[2] = new Vector3(size, size, 0);   //    ^
    //    mesh.vertices[3] = new Vector3(size, -size, 0);  //    .

    //    mesh.triangles[0] = 0;
    //    mesh.triangles[1] = 1;
    //    mesh.triangles[2] = 2;

    //    mesh.triangles[3] = 0;
    //    mesh.triangles[4] = 2;
    //    mesh.triangles[5] = 3;

    //    mesh.uv[0] = new Vector2(0, 0);
    //    mesh.uv[1] = new Vector2(0, 1);
    //    mesh.uv[2] = new Vector2(1, 1);
    //    mesh.uv[3] = new Vector2(1, 0);

    //    mesh.colors[0] = Color.white;
    //    mesh.colors[1] = Color.white;
    //    mesh.colors[2] = Color.white;
    //    mesh.colors[3] = Color.white;

    //    mesh.RecalculateBounds();
    //    mesh.RecalculateNormals();
    //    return mesh;

    //}
}
