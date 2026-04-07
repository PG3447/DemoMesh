using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static Hole;
using static UnityEngine.Rendering.DebugUI.Table;

public class Hole : ScriptableObject
{
    private int positionX;
    private int positionZ;
    private float size_Grid;
    private int sizeGridInX;
    private int wierzcholki;
    private float promien;
    private float maxOdlegloscSrodkowa;

    private Mesh holeInMesh;
    private Grid grid;
    private UnderHole underHole;

    private int newPositionX = -1;
    private int newPositionZ = -1;
    private float speed = 0.1f;
    private Vector3 direction = new Vector3(0.2f, 0f, 0.2f);

    private int poczatekVerticesHole;
    private int poczatekTrianglesHole;
    private Vector3 centerLocalMiddle;
    private Vector3 centerGlobalMiddle;
    private Vector3[] verticesHole; //Hole musi wiedziec jakie 

    private bool empty;

    public delegate void RecalculateHandler (int verticesHole, int trianglesHole, int lengthVertices, int lengthTriangles);

    public event RecalculateHandler? deleteHole;

    public Hole() {
        empty = true;
    }
    public void Initialize(int x, int z, int sizeXGrid, float sizeGrid, int points, float radius, Mesh mesh, Grid gridValue)
    {
        empty = false;
        positionX = x;
        positionZ = z;
        size_Grid = sizeGrid;
        sizeGridInX = sizeXGrid;
        wierzcholki = points;
        promien = radius;
        holeInMesh = mesh;
        grid = gridValue;
        underHole = new UnderHole(this, gridValue.getParent());
    }

    public bool getEmpty() {
        return empty;
    }

    public Delegate[]? GetDeleteHole()
    {
        return deleteHole?.GetInvocationList();
    }

    public void ClearAllDeleteHoleEvent()
    {
        deleteHole = null;
    }

    public Span<Vector3> getVertices()
    {
        return new Span<Vector3>(holeInMesh.vertices, poczatekVerticesHole, wierzcholki);
    }

    public Span<Vector3> getAllVertices()
    {
        return new Span<Vector3>(holeInMesh.vertices, 0, holeInMesh.vertices.Length);
    }

    public Vector3 getCenterMiddle()
    {
        return centerGlobalMiddle;
    }

    public Vector3 getCenterLocelMiddle()
    {
        return centerLocalMiddle;
    }

    public (int rUp, int rDown, int lDown, int lUp) getCalculateMeshPosition()
    {
        int rUp = (positionX + 1) + ((sizeGridInX + 1) * (positionZ + 1));
        int rDown = (positionX + 1) + ((sizeGridInX + 1) * positionZ);
        int lDown = positionX + ((sizeGridInX + 1) * positionZ);
        int lUp = positionX + ((sizeGridInX + 1) * (positionZ + 1));

        return (rUp, rDown, lDown, lUp);
    }

    public void UpdateMeshHole(Vector3[] vertices)
    {
        int[] triangles = holeInMesh.triangles;

        holeInMesh.Clear();

        holeInMesh.vertices = vertices;
        holeInMesh.triangles = triangles;
    }

    public void RecalculateStartValue(int verticesHole, int trianglesHole, int lengthVertices, int lengthTriangles) {

        //Debug.Log("Zmianny EVENT X " + positionX + " Z " + positionZ);

        if (poczatekVerticesHole > verticesHole)
        {
            poczatekVerticesHole -= lengthVertices;
        }

        if (poczatekTrianglesHole > trianglesHole)
        {
            poczatekTrianglesHole -= lengthTriangles;
        }
    }

    public void CalculateAndAddHoleToMesh() {
        if (empty) return;
        
        int rUp = (positionX + 1) + ((sizeGridInX + 1) * (positionZ + 1));
        int rDown = (positionX + 1) + ((sizeGridInX + 1) * positionZ);
        int lDown = positionX + ((sizeGridInX + 1) * positionZ); 
        int lUp = positionX + ((sizeGridInX + 1) * (positionZ + 1));

        //Debug.Log("Ustawione prawa gora " + rUp + " prawa dol " + rDown + " lewa dol " + lDown + " lewa gora " + lUp);
        
        CreateHoleInMesh(rUp, rDown, lDown, lUp);
    }

    private Mesh CreateHoleInMesh(int rightUp, int rightDown, int leftDown, int leftUp)
    {
        if (empty) return null;

        Vector3[] verticesMesh = holeInMesh.vertices;
        int[] trianglesMesh = holeInMesh.triangles;

        int verticesMeshLength = verticesMesh.Length;
        int trianglesMeshLength = trianglesMesh.Length;

        centerLocalMiddle = new Vector3(verticesMesh[rightUp].x - size_Grid/2f, 0f, verticesMesh[rightUp].z - size_Grid/2f);
        maxOdlegloscSrodkowa = Vector3.Distance(centerLocalMiddle, new Vector3(verticesMesh[rightUp].x, verticesMesh[rightUp].y, (verticesMesh[rightUp].z + verticesMesh[rightDown].z) / 2f)) - 0.1f;

        Array.Resize(ref verticesMesh, verticesMeshLength + wierzcholki);
        Array.Resize(ref trianglesMesh, trianglesMeshLength + (wierzcholki + 4) * 3);

        int iloscWierzcholkow = verticesMesh.Length - verticesMeshLength;

        verticesHole = new Vector3[iloscWierzcholkow];

        poczatekVerticesHole = verticesMeshLength;

        for (int i = 0; i < iloscWierzcholkow; i++)
        {
            verticesMesh[i + verticesMeshLength] = new Vector3(centerLocalMiddle.x + promien * Mathf.Sin(i / (wierzcholki / 2f) * 3.1415f), centerLocalMiddle.y, centerLocalMiddle.z + promien * Mathf.Cos(i / (wierzcholki / 2f) * 3.1415f));
            verticesHole[i] = verticesMesh[i + verticesMeshLength];
        }


        trianglesMesh = grid.DeleteSquare(trianglesMesh ,positionX, positionZ);

        int[] points = new int[4];

        points[0] = rightUp;
        points[1] = rightDown;
        points[2] = leftDown;
        points[3] = leftUp;


        int actualPoint = verticesMeshLength - 1;
        int point = 0;
        int add = 0;

        poczatekTrianglesHole = trianglesMeshLength;

        if (wierzcholki % 2 != 0)
        {
            add++;
        }

        for (int i = trianglesMeshLength; i < trianglesMesh.Length - 6; i += 3) // 15 // to do zrobienia
        {
            if (((i - trianglesMeshLength) - (3 * point)) + add >= (point + 1) * (wierzcholki / 4f * 3f))
            {
                trianglesMesh[i] = points[point];
                trianglesMesh[i + 1] = points[point + 1];
                trianglesMesh[i + 2] = actualPoint + 1;

                point++;
                i += 3;
            }

            trianglesMesh[i] = points[point];
            trianglesMesh[i + 1] = actualPoint + 2;
            trianglesMesh[i + 2] = actualPoint + 1;

            actualPoint++;
        }


        trianglesMesh[trianglesMesh.Length - 6] = points[point];
        trianglesMesh[trianglesMesh.Length - 5] = verticesMeshLength;
        trianglesMesh[trianglesMesh.Length - 4] = verticesMesh.Length - 1;

        trianglesMesh[trianglesMesh.Length - 3] = verticesMeshLength;
        trianglesMesh[trianglesMesh.Length - 2] = points[point];
        trianglesMesh[trianglesMesh.Length - 1] = points[0];

        holeInMesh.Clear();

        holeInMesh.vertices = verticesMesh;
        holeInMesh.triangles = trianglesMesh;

        centerGlobalMiddle = grid.getParent().transform.TransformPoint(centerLocalMiddle);
        underHole.CreateUnderHole(centerGlobalMiddle);

        return holeInMesh;
    }

    public void StartFire()
    {
        if (empty) return;

        GameObject prefabFire = Resources.Load<GameObject>("Prefabs/Fire");
        if (prefabFire != null)
        {
            for (int i = poczatekVerticesHole; i < poczatekVerticesHole + wierzcholki; i++)
            {
                GameObject instanceFire = Instantiate(prefabFire, new Vector3(0, 0, 0), Quaternion.identity);
                instanceFire.AddComponent<FireHole>().Initialize(this, grid.getParent().transform, i, maxOdlegloscSrodkowa);
            }
        }
    }

    public void ResizeHole(float factor)
    {
        if (empty) return;

        Vector3[] vertices = holeInMesh.vertices;
        int[] triangles = holeInMesh.triangles;  

        int rUp = (positionX + 1) + ((sizeGridInX + 1) * (positionZ + 1));
        int rDown = (positionX + 1) + ((sizeGridInX + 1) * positionZ);
        int lDown = positionX + ((sizeGridInX + 1) * positionZ);
        int lUp = positionX + ((sizeGridInX + 1) * (positionZ + 1));


        for (int i = poczatekVerticesHole; i < poczatekVerticesHole + wierzcholki; i++)
        {
            Vector3 tempVertices = centerLocalMiddle + (vertices[i] - centerLocalMiddle) * factor;
            if (tempVertices.x  < vertices[rUp].x &&
                tempVertices.x > vertices[lDown].x &&
                tempVertices.z < vertices[rUp].z &&
                tempVertices.z > vertices[rDown].z &&
                Vector3.Distance(centerLocalMiddle, tempVertices) < maxOdlegloscSrodkowa)
            {
                vertices[i] = tempVertices;
            }
        }

        holeInMesh.Clear();

        holeInMesh.vertices = vertices;
        holeInMesh.triangles = triangles;
    }

    public void MoveHole() {
        if (empty) return;

        Vector3[] vertices = holeInMesh.vertices;
        int[] triangles = holeInMesh.triangles;

        int rUp = (positionX + 1) + ((sizeGridInX + 1) * (positionZ + 1));
        int rDown = (positionX + 1) + ((sizeGridInX + 1) * positionZ);
        int lDown = positionX + ((sizeGridInX + 1) * positionZ);
        int lUp = positionX + ((sizeGridInX + 1) * (positionZ + 1));

        centerLocalMiddle += direction * speed;

        centerGlobalMiddle = grid.getParent().transform.TransformPoint(centerLocalMiddle);

        for (int i = poczatekVerticesHole; i < poczatekVerticesHole + wierzcholki; i++)
        {
            //Debug.Log("Punk w mesh i: " + i + " zawartosc: " + vertices[i]);
            vertices[i] += direction * speed;
        }


        bool outsideMovePoint = false;
        int newX = positionX;
        int newZ = positionZ;

        for (int i = poczatekVerticesHole; i < poczatekVerticesHole + wierzcholki; i++) {
            //Debug.Log("PozycjaZ w poszukiwaniu " + newPositionZ);


            if (newPositionX != -1 && positionX != newPositionX)
            {
                if (vertices[i].x <= vertices[rUp].x && direction.x > 0f)
                {
                    outsideMovePoint = true;
                }

                if (vertices[i].x >= vertices[lDown].x && direction.x < 0f)
                {
                    outsideMovePoint = true;
                }

                if (vertices[i].z > vertices[rUp].z && direction.z > 0f)
                {
                    direction.z *= -1f;
                }

                if (vertices[i].z < vertices[rDown].z && direction.z < 0f)
                {
                    direction.z *= -1f;;
                }
            }
            else if (newPositionZ != -1 && positionZ != newPositionZ)
            {
                if (vertices[i].z <= vertices[rUp].z && direction.z > 0f)
                {
                    outsideMovePoint = true;
                }

                if (vertices[i].z >= vertices[rDown].z && direction.z < 0f)
                {
                    outsideMovePoint = true;
                }

                if (vertices[i].x > vertices[rUp].x && direction.x > 0f)
                {
                    direction.x *= -1f;
                }

                if (vertices[i].x < vertices[lDown].x && direction.x < 0f)
                {
                    direction.x *= -1f;
                }
            }
            else
            {
                if (vertices[i].z > vertices[rUp].z)
                {
                    newZ = positionZ + 1;
                   // break;
                }

                if (vertices[i].z < vertices[rDown].z)
                {
                    newZ = positionZ - 1;
                   // break;
                }

                if (vertices[i].x > vertices[rUp].x)
                {
                    newX = positionX + 1;
                   // break;

                }

                if (vertices[i].x < vertices[lDown].x)
                {
                    newX = positionX - 1;
                    //break;
                }
            }
        }
        //Debug.Log("NORM Nowe pozycje x " + positionX + " nowa pozycja X " + newPositionX + " newX " + newX);
        //Debug.Log("NORM Nowe pozycje z " + positionZ + " nowa pozycja Z " + newPositionZ + " newZ " + newZ);

        if (positionX != newX || positionZ != newZ)
        {
            newPositionX = newX;
            newPositionZ = newZ;
            triangles = StartHoleTransaction(triangles);
        }
        else 
        {
            //Debug.Log("ELSE Nowe pozycje x " + positionX + " nowa pozycja X " + newPositionX + " newX " + newX);
            //Debug.Log("ELSE Nowe pozycje z " + positionZ + " nowa pozycja Z " + newPositionZ + " newZ " + newZ);
            //Debug.Log("Stan bool-a " + outsideMovePoint);

            if ((newPositionZ != -1 || newPositionX != -1) && !outsideMovePoint)
            {
                //Debug.Log("Przywracanie ");
                for (int j = poczatekTrianglesHole; j < poczatekTrianglesHole + (wierzcholki + 4) * 3; j++)
                {
                    if (direction.z > 0f && newPositionZ != positionZ)
                    {
                        if (triangles[j] == rDown)
                        {
                            triangles[j] = (newPositionX + 1) + ((sizeGridInX + 1) * newPositionZ);
                        }

                        if (triangles[j] == lDown)
                        {
                            triangles[j] = newPositionX + ((sizeGridInX + 1) * newPositionZ);
                        }
                    }

                    if (direction.z < 0f && newPositionZ != positionZ)
                    {
                        if (triangles[j] == rUp)
                        {
                            triangles[j] = (newPositionX + 1) + ((sizeGridInX + 1) * (newPositionZ + 1));
                        }

                        if (triangles[j] == lUp)
                        {
                            triangles[j] = newPositionX + ((sizeGridInX + 1) * (newPositionZ + 1));
                        }
                    }

                    if (direction.x > 0f && newPositionX != positionX)
                    {
                        if (triangles[j] == lUp)
                        {
                            triangles[j] = newPositionX + ((sizeGridInX + 1) * (newPositionZ + 1));
                        }

                        if (triangles[j] == lDown)
                        {
                            triangles[j] = newPositionX + ((sizeGridInX + 1) * newPositionZ);
                        }
                    }

                    if (direction.x < 0f && newPositionX != positionX)
                    {
                        if (triangles[j] == rUp)
                        {
                            triangles[j] = (newPositionX + 1) + ((sizeGridInX + 1) * ((newPositionZ) + 1));
                        }

                        if (triangles[j] == rDown)
                        {
                            triangles[j] = (newPositionX + 1) + ((sizeGridInX + 1) * newPositionZ);
                        }
                    }
                }

                grid.DeleteTwoHole(triangles, positionX, positionZ);

                positionX = newPositionX;
                positionZ = newPositionZ;
                newPositionX = -1;
                newPositionZ = -1;
            }
        }

        

        holeInMesh.Clear();

        holeInMesh.vertices = vertices;
        holeInMesh.triangles = triangles;


        underHole.MoveUnderHole();
    }

    private int[] StartHoleTransaction(int[] triangles) {
        int rUp = (positionX + 1) + ((sizeGridInX + 1) * (positionZ + 1));
        int rDown = (positionX + 1) + ((sizeGridInX + 1) * positionZ);
        int lDown = positionX + ((sizeGridInX + 1) * positionZ);
        int lUp = positionX + ((sizeGridInX + 1) * (positionZ + 1));

        if (grid.checkPossibleSquare(newPositionX, newPositionZ) &&
            ((positionX == newPositionX && positionZ != newPositionZ) ||
            (positionX != newPositionX && positionZ == newPositionZ))) //&& (positionX != newPositionX || positionZ != newPositionZ) && ((positionX == newPositionX && positionZ != newPositionZ) || (positionX != newPositionX && positionZ == newPositionZ))
        {
            //Debug.Log("FUNKCJAAAA");

            triangles = grid.DeleteSquare(triangles, newPositionX, newPositionZ);

            grid.TwoHole(this, newPositionX, newPositionZ);

            for (int j = poczatekTrianglesHole; j < poczatekTrianglesHole + (wierzcholki + 4) * 3; j++)
            {
                if (direction.z > 0f && newPositionZ != positionZ)
                {
                    if (triangles[j] == rUp)
                    {
                        triangles[j] = (newPositionX + 1) + ((sizeGridInX + 1) * ((newPositionZ) + 1));
                    }

                    if (triangles[j] == lUp)
                    {
                        triangles[j] = newPositionX + ((sizeGridInX + 1) * (newPositionZ + 1));
                    }
                }

                if (direction.z < 0f && newPositionZ != positionZ)
                {
                    if (triangles[j] == rDown)
                    {
                        triangles[j] = (newPositionX + 1) + ((sizeGridInX + 1) * newPositionZ);
                    }

                    if (triangles[j] == lDown)
                    {
                        triangles[j] = newPositionX + ((sizeGridInX + 1) * newPositionZ);
                    }
                }


                if (direction.x > 0f && newPositionX != positionX)
                {
                    if (triangles[j] == rUp)
                    {
                        triangles[j] = (newPositionX + 1) + ((sizeGridInX + 1) * ((newPositionZ) + 1));
                    }

                    if (triangles[j] == rDown)
                    {
                        triangles[j] = (newPositionX + 1) + ((sizeGridInX + 1) * newPositionZ);
                    }
                }

                if (direction.x < 0f && newPositionX != positionX)
                {
                    if (triangles[j] == lUp)
                    {
                        triangles[j] = newPositionX + ((sizeGridInX + 1) * (newPositionZ + 1));
                    }

                    if (triangles[j] == lDown)
                    {
                        triangles[j] = newPositionX + ((sizeGridInX + 1) * newPositionZ);
                    }
                }


            }
        }
        else
        {

            if (newPositionZ != positionZ && newPositionX != positionX)
            {
                direction.x *= 1.0004f;

                if (direction.x > 1f)
                {
                    direction.x = 1f;
                }

                if (direction.z > 1f)
                {
                    direction.z = 1f;
                }


                if (direction.x < -1f)
                {
                    direction.z = -1f;
                }


                if (direction.z < -1f)
                {
                    direction.z = -1f;
                }
            }


            if (newPositionX != positionX)
            {
                direction.x *= -1f;
            }

            if (newPositionZ != positionZ)
            {
                direction.z *= -1f;
            }

            newPositionX = - 1;
            newPositionZ = - 1;
        }

        return triangles;
    }


    public void DeleteHoleFromMesh() {
        if (empty) return;
    
        Vector3[] vertices = holeInMesh.vertices;
        int[] triangles = holeInMesh.triangles;
        /*
        int[] points = new int[4];
    
        for (int i = 0; i < vertices.Length; i++)
        {
            Debug.Log("ver " + vertices[i]);// + " na " + vertices[i + wierzcholki]);
            Debug.Log("ver i " + i);// + " na " + (i + wierzcholki));
        }
    
        int rUp = (positionX + 1) + ((sizeGridInX + 1) * (positionZ + 1));
        int rDown = (positionX + 1) + ((sizeGridInX + 1) * positionZ);
        int lDown = positionX + ((sizeGridInX + 1) * positionZ);
        int lUp = positionX + ((sizeGridInX + 1) * (positionZ + 1));
        
        points[0] = rUp;
        points[1] = rDown;
        points[2] = lDown;
        points[3] = lUp;
    
        int counter = 0;
    
        foreach (Vector3 vertice in verticesHole)
        {
            Debug.Log("Punkt " + vertice + " ktory to " + counter);
            counter++;
        }*/
    
        int obliczenia = holeInMesh.vertices.Length - wierzcholki;

        int lengthVertices = holeInMesh.vertices.Length;
        int[] indexVerticesMap = new int[lengthVertices];

        for (int i = 0; i < lengthVertices; i++)
        {
            indexVerticesMap[i] = -1;
        }

        for (int i = poczatekVerticesHole; i < obliczenia; i++)
        {
            int oldVerticeIndex = i + wierzcholki;
            vertices[i] = vertices[oldVerticeIndex];
            indexVerticesMap[oldVerticeIndex] = i;
        }

        for (int i = 0; i < holeInMesh.triangles.Length; i++)
        {
            int oldVerticeIndexFromTriangles = triangles[i];

            if (oldVerticeIndexFromTriangles >= 0 && oldVerticeIndexFromTriangles < lengthVertices && indexVerticesMap[oldVerticeIndexFromTriangles] != -1)
            {
                triangles[i] = indexVerticesMap[oldVerticeIndexFromTriangles];
            }
        }

        //Stary sposob powolny
        //for (int i = poczatekVerticesHole; i < obliczenia; i++)
        //{
        //    int oldVerticesIndex = i + wierzcholki;
        //
        //
        //    //Debug.Log("Zamiana z " + vertices[i] + " na " + vertices[wartosc]);
        //    //Debug.Log("Zamiana i z " + i + " na " + wartosc);
        //
        //    vertices[i] = vertices[oldVerticesIndex];
        //
        //    for (int j = 0; j < holeInMesh.triangles.Length; j++)
        //    {
        //        if (triangles[j] == oldVerticesIndex) {
        //            triangles[j] = i;
        //        }
        //    }
        //
        //}
    
        int obliczenia2 = holeInMesh.triangles.Length - (wierzcholki + 4) * 3;
    
        for (int i = poczatekTrianglesHole; i < obliczenia2; i++)
        {
            triangles[i] = triangles[i + (wierzcholki + 4) * 3];
    
        }
    
        //Debug.Log("IlosccccV " + holeInMesh.vertices.Length + " poczatek " + poczatekVerticesHole + " wierzcholki " + wierzcholki + " wynik " + obliczenia);
        //Debug.Log("Iloscccc " + holeInMesh.triangles.Length + " poczatek " + poczatekTrianglesHole + " wierzcholki " + (wierzcholki + 4) * 3 + " wynik " + obliczenia2);
    
        //moze byc troche szybsze
        //var deleteHoleList = deleteHole?.GetInvocationList();
        //if (deleteHoleList != null)
        //{
        //    foreach (RecalculateHandler del in deleteHoleList)
        //    {
        //        del(poczatekVerticesHole, poczatekTrianglesHole, wierzcholki, (wierzcholki + 4) * 3);
        //    }
        //}
    
        deleteHole?.Invoke(poczatekVerticesHole, poczatekTrianglesHole, wierzcholki, (wierzcholki + 4) * 3);
    
        Array.Resize(ref vertices, vertices.Length - wierzcholki);
        Array.Resize(ref triangles, triangles.Length - (wierzcholki + 4) * 3);
    
    
        holeInMesh.Clear();
    
        holeInMesh.vertices = vertices;
        holeInMesh.triangles = triangles;
    
        
        underHole.DestroyUnderHole();
        underHole = null;
    }

    /*

    int positionHole = -1;
    int emptyHole = 0;


    for (int i = 0; i < hole.Length; i++)
    {

        if (hole[i] == -1)
        {
            emptyHole++;
            continue;
        }

        if (hole[i] == deleteHole)
        {
            //hole[i] = -1;
            positionHole = i;
            break;
        }
    }

    if (positionHole == -1)
    {
        return;
    } 

    //Debug.Log("PRZED trojkaty");
    //
    //for (int i = 0; i < triangles.Length; i++)
    //{
    //
    //    Debug.Log("Trojkat: " + i + " wartosc: " + triangles[i]);
    //
    //}

    //Szukanie punkta
    Vector3[] verticsThisHole = new Vector3[wierzcholki];

    for (int i = vertices.Length - 1; i > 0; i--)
    {
        if (vertices[lDown].x < vertices[i].x && vertices[rDown].x > vertices[i].x)
        {
            //Debug.Log("Spoko punkt " + vertices[i]);

            if (vertices[lDown].z < vertices[i].z && vertices[lUp].z > vertices[i].z)
            {
                verticsThisHole.Append(vertices[i]);
                Debug.Log("Jeszcze lepszy spoko punkt " + vertices[i] + " w tablicy " + i);
            }
        }
    }

    //Zawartosc verticsThisHole

    for (int i = 0; i < verticsThisHole.Length; i++) {
        Debug.Log("Dzialanie zawartosc " + verticsThisHole[i]);
    }


    Debug.Log("POZYCJJAJA " + positionHole);

    for (int i = vertices.Length - (wierzcholki * positionHole); i < vertices.Length - wierzcholki; i++)
    {
        vertices[i] = vertices[i + wierzcholki];

        for (int j = triangles.Length - ((wierzcholki + 4) * 3 * positionHole); j < triangles.Length; j++)
        { //odjac koniec
            if (triangles[j] == i + wierzcholki)
            {
                triangles[j] = i;
            }
        }
    }

    //for (int i = 0; i < vertices.Length; i++) {
    //    Debug.Log("VER: " + i + " i: " +vertices[i]);
    //}

    Debug.Log("Wielkosc trojkatow: " + triangles.Length);

    Debug.Log("Wielkosc pointow: " + vertices.Length);


    for (int i = triangles.Length - (3 * (wierzcholki + 4) * (positionHole + 1 - emptyHole)); i < triangles.Length - (3 * (wierzcholki + 4)); i++)
    {

        //Debug.Log("Osobno i: " + i);
        //Debug.Log("Obliczenia: " + (i + (3 * (24 + 4))));

        triangles[i] = triangles[i + (3 * (wierzcholki + 4))];

    }

    //for (int i = 0; i < triangles.Length; i++)
    //{
    //    Debug.Log("Trojkat: " + i + " wartosc: " + triangles[i]);
    //
    //}

    bool first = true;

    Debug.Log("pozycja XZ " + positionX * positionZ);

    for (int i = (positionX + positionZ * (sizeGridInX+1)) * 2 * 3; i < triangles.Length; i++)
    {

        if (triangles[i] == 0 && triangles[i + 1] == 0 && triangles[i + 2] == 0)
        {

            if (first)
            {

                //Tworzenie

                // lewy dolny
                // lewy gorny
                // prawy gorny

                triangles[i] = points[2];
                triangles[i + 1] = points[3];
                triangles[i + 2] = points[0];
                first = false;
            }
            else
            {
                //Tworzenie

                // prawy dolny
                // lewy dolny
                // prawy gorny

                triangles[i] = points[1];
                triangles[i + 1] = points[2];
                triangles[i + 2] = points[0];
                break;
            }
        }

    }
    //hole[0] = -1;                                        // hole[0] = -1   // hole[0]
    //hole[1] = -1; // hole[1] = 3 96 - 119  // 336 - 419  // hole[1] = 3    // hole[1]
    //hole[2] = 5;  //72 - 95                // 252 - 335  // hole[2] = 5    //
    //hole[3] = 4;  //48 - 71                // 168 - 251  // hole[3] = 4    //
    //hole[4] = 2;  //24 - 47                // 84 - 167   // hole[4] = 2    //
    //hole[5] = 0;  //0 - 23                 // 0 - 83     // hole[5] = -1   //


    Array.Resize(ref vertices, vertices.Length - 24);
    Array.Resize(ref triangles, triangles.Length - 84);

    holeInMesh.vertices = vertices;
    holeInMesh.triangles = triangles;
    */

    /*
    private void DeleteHoleAndCreateMesh(int deleteHole)
    {
        Vector3[] vertices = holeMesh.vertices;
        int[] triangles = holeMesh.triangles;
        int[] points;

        points = CalculateCreateHole(deleteHole);

        int positionHole = -1;
        int emptyHole = 0;

        for (int i = 0; i < hole.Length; i++)
        {

            if (hole[i] == -1)
            {
                emptyHole++;
                continue;
            }

            if (hole[i] == deleteHole)
            {
                //hole[i] = -1;
                positionHole = i;
                break;
            }
        }

        if (positionHole == -1)
        {
            return;
        }

        //Debug.Log("PRZED trojkaty");
        //
        //for (int i = 0; i < triangles.Length; i++)
        //{
        //
        //    Debug.Log("Trojkat: " + i + " wartosc: " + triangles[i]);
        //
        //}

        for (int i = vertices.Length - (24 * (positionHole + 1 - emptyHole)); i < vertices.Length - 24; i++)
        {
            vertices[i] = vertices[i + 24];

            for (int j = triangles.Length - ((24 + 4) * 3 * (positionHole + 1 - emptyHole)); j < triangles.Length; j++)
            { //odjac koniec
                if (triangles[j] == i + 24)
                {
                    triangles[j] = i;
                }
            }
        }

        //for (int i = 0; i < vertices.Length; i++) {
        //    Debug.Log("VER: " + i + " i: " +vertices[i]);
        //}

        Debug.Log("Wielkosc trojkatow: " + triangles.Length);

        Debug.Log("Wielkosc pointow: " + vertices.Length);


        for (int i = triangles.Length - (3 * (24 + 4) * (positionHole + 1 - emptyHole)); i < triangles.Length - (3 * (24 + 4)); i++)
        {

            //Debug.Log("Osobno i: " + i);
            //Debug.Log("Obliczenia: " + (i + (3 * (24 + 4))));

            triangles[i] = triangles[i + (3 * (24 + 4))];

        }

        //for (int i = 0; i < triangles.Length; i++)
        //{
        //    Debug.Log("Trojkat: " + i + " wartosc: " + triangles[i]);
        //
        //}

        bool first = true;

        for (int i = deleteHole * 2 * 3; i < triangles.Length; i++)
        {

            if (triangles[i] == 0 && triangles[i + 1] == 0 && triangles[i + 2] == 0)
            {

                if (first)
                {

                    //Tworzenie

                    // lewy dolny
                    // lewy gorny
                    // prawy gorny

                    triangles[i] = points[2];
                    triangles[i + 1] = points[3];
                    triangles[i + 2] = points[0];
                    first = false;
                }
                else
                {
                    //Tworzenie

                    // prawy dolny
                    // lewy dolny
                    // prawy gorny

                    triangles[i] = points[1];
                    triangles[i + 1] = points[2];
                    triangles[i + 2] = points[0];
                    break;
                }
            }

        }
        //hole[0] = -1;                                        // hole[0] = -1   // hole[0]
        //hole[1] = -1; // hole[1] = 3 96 - 119  // 336 - 419  // hole[1] = 3    // hole[1]
        //hole[2] = 5;  //72 - 95                // 252 - 335  // hole[2] = 5    //
        //hole[3] = 4;  //48 - 71                // 168 - 251  // hole[3] = 4    //
        //hole[4] = 2;  //24 - 47                // 84 - 167   // hole[4] = 2    //
        //hole[5] = 0;  //0 - 23                 // 0 - 83     // hole[5] = -1   //

        //Przesuwanie holes
        for (int i = positionHole; i > 0; i--)
        {

            hole[i] = hole[i - 1];
            direction[i] = direction[i - 1];
            control[i] = control[i - 1];
        }

        for (int i = 0; i < control.Length; i++)
        {
            //Debug.Log("ZAWARTOSC CONTROL PRZY USUWANIU: " + control[i] + " POZYCJA I: " + i +  " deleteHole: " + deleteHole);

            if (control[i] == deleteHole)
            {
                control[i] = -1;
            }
        }

        hole[0] = -1;
        direction[0] = new Vector3(0f, 0f, 0f);
        //control[0] = -1;

        Array.Resize(ref vertices, vertices.Length - 24);
        Array.Resize(ref triangles, triangles.Length - 84);


        holeMesh.Clear();

        holeMesh.vertices = vertices;
        holeMesh.triangles = triangles;
        this.GetComponent<MeshFilter>().sharedMesh = holeMesh;
        this.GetComponent<MeshCollider>().sharedMesh = holeMesh;
        holeMesh.RecalculateNormals();
    }*/
}
