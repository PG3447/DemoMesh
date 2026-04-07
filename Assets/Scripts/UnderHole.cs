using System;
using UnityEngine;

//[CreateAssetMenu(fileName = "UnderHole", menuName = "Scriptable Objects/UnderHole")]
public class UnderHole : ScriptableObject
{
    private Hole parentHole;
    private Vector3 center;
    private GameObject parentGameObject;
    private GameObject objectUnderHole;
    private Mesh meshUnderHole;

    private float length = 5f;
    private float resizing = 0.24f;
    private Vector3[] spanVertices;

    private Vector3[] vertices;
    private int[] triangles;

    public UnderHole(Hole valueHole, GameObject valueGameObject) {
        parentHole = valueHole;
        parentGameObject = valueGameObject;
    }
   

    private void CreateObject(Vector3 center) {
        objectUnderHole = new GameObject();
        objectUnderHole.name = "UnderHole";
        objectUnderHole.transform.parent = parentGameObject.transform;

        center = new Vector3((float)center.x, (float)parentGameObject.transform.position.y, (float)center.z);
        center.y -= length / 2f;

        objectUnderHole.transform.position = center;

        objectUnderHole.AddComponent<MeshFilter>();
        objectUnderHole.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        objectUnderHole.AddComponent<MeshCollider>();

        meshUnderHole = new Mesh();
    }

    public void CreateUnderHole(Vector3 center) {
        CreateObject(center);
        
        spanVertices = parentHole.getVertices().ToArray();

        vertices = new Vector3[spanVertices.Length * 2];
        triangles = new int[spanVertices.Length * 2 * 3];

        parentGameObject.transform.TransformPoints(spanVertices);

        objectUnderHole.transform.InverseTransformPoints(spanVertices);

        for (int i = 0; i < spanVertices.Length; i++)
        {
            vertices[i] = spanVertices[i];
        }

        for (int i = 0; i < vertices.Length - spanVertices.Length; i++)
        {
            vertices[i + spanVertices.Length] = new Vector3(vertices[i].x * resizing, vertices[i].y - length, vertices[i].z * resizing);
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            Debug.Log(vertices[i] + " i " + i);
        }

        int counter = 0;

        for (int i = 0; i < triangles.Length - 6; i += 6)
        {
            //prawy gora
            //lewy gora
            //prawy dol
            triangles[i] = counter;
            triangles[i + 1] = counter + 1;
            triangles[i + 2] = counter + spanVertices.Length;


            //lewy dol
            //prawy dol
            //lewy gora
            triangles[i + 3] = counter + 1 + spanVertices.Length;
            triangles[i + 4] = counter + spanVertices.Length;
            triangles[i + 5] = counter + 1;

            counter++;
        }

        //Ostatnie dwa kwadraty

        //prawy gora
        //lewy gora
        //prawy dol
        triangles[triangles.Length - 6] = spanVertices.Length - 1;
        triangles[triangles.Length - 5] = 0;
        triangles[triangles.Length - 4] = vertices.Length - 1;

        //lewy dol
        //prawy dol
        //lewy gora
        triangles[triangles.Length - 3] = spanVertices.Length;
        triangles[triangles.Length - 2] = vertices.Length - 1;
        triangles[triangles.Length - 1] = 0;

        Debug.Log("triangles");

        for (int i = 0; i < triangles.Length; i++)
        {
            Debug.Log(triangles[i] + " i " + i);
        }

        meshUnderHole.Clear();

        meshUnderHole.vertices = vertices;
        meshUnderHole.triangles = triangles;

        meshUnderHole.RecalculateNormals();
        objectUnderHole.GetComponent<MeshFilter>().mesh = meshUnderHole;
        objectUnderHole.GetComponent<MeshCollider>().sharedMesh = meshUnderHole;
    }

    public void MoveUnderHole()
    {
        spanVertices = parentHole.getVertices().ToArray();

        Vector3 globalHolePosition = parentHole.getCenterMiddle();

        globalHolePosition.y -= length / 2f;

        objectUnderHole.transform.position = globalHolePosition;

        vertices = new Vector3[spanVertices.Length * 2];

        parentGameObject.transform.TransformPoints(spanVertices);

        objectUnderHole.transform.InverseTransformPoints(spanVertices);

        for (int i = 0; i < spanVertices.Length; i++)
        {
            vertices[i] = spanVertices[i];
        }

        for (int i = 0; i < vertices.Length - spanVertices.Length; i++)
        {
            vertices[i + spanVertices.Length] = new Vector3(vertices[i].x * resizing, vertices[i].y - length, vertices[i].z * resizing);
        }

        meshUnderHole.Clear();

        meshUnderHole.vertices = vertices;
        meshUnderHole.triangles = triangles;

        meshUnderHole.RecalculateNormals();
        objectUnderHole.GetComponent<MeshFilter>().mesh = meshUnderHole;
        objectUnderHole.GetComponent<MeshCollider>().sharedMesh = meshUnderHole;

    }

    public void DestroyUnderHole()
    {
        parentHole = null;
        Destroy(objectUnderHole);
    }

}
