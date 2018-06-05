using UnityEngine;

public class MeshReproject : MonoBehaviour
{
    public Camera ProjectionPerspective;

    Mesh mesh;
    Vector3[] vertices;
    Vector2[] uvs;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        mesh.MarkDynamic();
    }

    void Update()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        uvs = new Vector2[vertices.Length];
        Vector2 ScreenPosition;
        for (int i = 0; i < uvs.Length; i++)
        {
            ScreenPosition = ProjectionPerspective.WorldToViewportPoint(transform.TransformPoint(vertices[i]));
            uvs[i].Set(ScreenPosition.x, ScreenPosition.y);
        }
        mesh.uv = uvs;
    }
} 