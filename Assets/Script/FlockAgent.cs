using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FlockAgent : MonoBehaviour
{
    private Collider2D agentCollider;

    public Collider2D AgentCollider
    {
        get { return agentCollider; }
    }

    // Start is called before the first frame update
    void Start()
    {
        agentCollider = GetComponent<Collider2D>();

        // Create a new mesh
        Mesh triangleMesh = new()
        {
            name = "SingleTriangle"
        };

        // Define vertices of the triangle
        Vector3[] vertices = new Vector3[]
        {
            new(0, 1, 0),
            new(Mathf.Sqrt(3) / 2.0f, -0.5f, 0),
            new(-Mathf.Sqrt(3) / 2.0f, -0.5f, 0),
        };

        // Define the triangle indices
        int[] triangles = new int[]
        {
            0, 1, 2
        };

        // Assign data to the mesh
        triangleMesh.vertices = vertices;
        triangleMesh.triangles = triangles;

        // Optionally, define UVs for texturing
        Vector2[] uvs = new Vector2[]
        {
            new(0, 0),
            new(1, 0),
            new(0, 1),
        };
        triangleMesh.uv = uvs;

        // Assign the mesh to a MeshFilter or save it as an asset
        GetComponent<MeshFilter>().mesh = triangleMesh;
    }

    public void Move(Vector2 velocity)
    {
        transform.up = velocity;
        transform.position += (Vector3)velocity * Time.deltaTime;
    }
}
