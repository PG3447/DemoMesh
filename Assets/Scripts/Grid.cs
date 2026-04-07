using System;
using System.Collections.Generic;
using UnityEngine;
using static Hole;

public class Grid : ScriptableObject
{
    private int wymiarX;
    private int wymiarZ;
    private Hole[,] gridHole;
    private float rozmiar;
    
    private float offsetRozmiarX;
    private float offsetRozmiarZ;


    private Mesh gridInMesh;
    private GameObject parent;

    public Grid(int x, int z, float size, float valueOffsetRozmiarX, float valueOffsetRozmiarZ, Mesh mesh, GameObject valueParent)
    {
        wymiarX = x;
        wymiarZ = z;
        gridHole = new Hole[x, z];
        rozmiar = size;
        offsetRozmiarX = valueOffsetRozmiarX;
        offsetRozmiarZ = valueOffsetRozmiarZ;
        gridInMesh = mesh;
        parent = valueParent;
        InitialHole();
    }

    public void UpdateCoordinate(int x, int z, float size, Mesh mesh) {
        wymiarX = x;
        wymiarZ = z;
        gridHole = new Hole[x, z];
        rozmiar = size;
        gridInMesh = mesh;
    }

    public GameObject getParent() {
        return parent;
    }

    public Mesh CreateGrid()
    {
        Vector3[] vertices = gridInMesh.vertices;
        int[] triangles = gridInMesh.triangles;

        int SizeX = wymiarX + 1;
        int SizeZ = wymiarZ + 1;

        float przesuniecieSizeX = wymiarX / 2f;
        float przesuniecieSizeZ = wymiarZ / 2f;


        vertices = new Vector3[SizeX * SizeZ];

        for (int z = 0; z < SizeZ; z++)
        {
            for (int x = 0; x < SizeX; x++)
            {
                vertices[x + SizeX * z] = new Vector3((x - przesuniecieSizeX) * (rozmiar + offsetRozmiarX), 0f, (z - przesuniecieSizeZ) * (rozmiar + offsetRozmiarZ));
            }
        }


        triangles = new int[2 * 3 * (SizeX * SizeZ - SizeX - SizeZ + 1)];

        int Countriangles = 0;

        //Ominiecie przechodzenia po ostatnim rzedzie
        for (int positiontriangle = 0; positiontriangle < SizeX * SizeZ - SizeX; positiontriangle++)
        {
            if (positiontriangle % SizeX != (SizeX - 1))
            {

                int A = positiontriangle; // lewy dolny
                int C = A + SizeX; // lewy gorny
                int B = C + 1;  // prawy gorny

                triangles[Countriangles] = A; // lewy dolny
                triangles[Countriangles + 1] = C; // lewy gorny
                triangles[Countriangles + 2] = B; // prawy gorny

                C -= (SizeX - 1); // prawy dolny ( 3 - 1 ) = 2 wiec  3 - 2 = 1

                triangles[Countriangles + 3] = C; // prawy dolny
                triangles[Countriangles + 4] = A; // lewy dolny
                triangles[Countriangles + 5] = B; // prawy gorny
                Countriangles += 6;
            }
        }

        gridInMesh.Clear();

        gridInMesh.vertices = vertices;
        gridInMesh.triangles = triangles;

        return gridInMesh;
    }

    private void InitialHole() {
        for (int x = 0; x < wymiarX; x++)
        {
            for (int z = 0; z < wymiarZ; z++)
            {
                gridHole[x, z] = ScriptableObject.CreateInstance<Hole>();

            }
        }
    }

    public (int, int) PositionToPositionInGrid(Vector3 position)
    {
        Vector3[] localVertices = gridInMesh.vertices;
        Vector3[] vertices = new Vector3[localVertices.Length];

        for (int i = 0; i < localVertices.Length; i++)
        {
            vertices[i] = parent.transform.TransformPoint(localVertices[i]);
        }

        int rightUp;
        int leftDown;

        int finalPositionX = -1;
        int finalPositionZ = -1;

        for (int x = 0; x < wymiarX; x++)
        {
            for (int z = 0; z < wymiarZ; z++) {
                rightUp = (x + 1) + ((wymiarX + 1) * (z + 1));
                leftDown = x + ((wymiarX + 1) * z);

                Debug.Log("Pozycjeee rozneee podana pozycja " + position + " pozycja prawego gornego " + vertices[rightUp] + " pozycja lewego dolnego " + vertices[leftDown]);

                if (position.x < vertices[rightUp].x && position.x > vertices[leftDown].x &&
                    position.z < vertices[rightUp].z && position.z > vertices[leftDown].z)
                {
                    finalPositionX = x;
                    finalPositionZ = z;
                    break;
                }
            }
        }

        return (finalPositionX, finalPositionZ);
    }

    public void AddHole(int positionX, int positionZ)
    {
        if (positionX < 0 || positionX >= wymiarX ||
            positionZ < 0 || positionZ >= wymiarZ)
        {
            return;
        }

        if (gridHole[positionX, positionZ] == null)
        {
            return;
        }

        if (gridHole[positionX, positionZ].getEmpty())
        {
            gridHole[positionX, positionZ] = ScriptableObject.CreateInstance<Hole>();
            gridHole[positionX, positionZ].Initialize(positionX, positionZ, wymiarX, rozmiar, 24, 2.0f, gridInMesh, this);

            gridHole[positionX, positionZ].CalculateAndAddHoleToMesh();

            //Czyszczenie();

            RecalculateHandler handler = gridHole[positionX, positionZ].RecalculateStartValue;

            for (int x = 0; x < wymiarX; x++)
            {
                for (int z = 0; z < wymiarZ; z++)
                {
                    //gridHole[x, z].deleteHole += gridHole[positionX, positionZ].RecalculateStartValue;
                    if (!gridHole[x, z].getEmpty() && gridHole[x, z] != null && !(positionX == x && positionZ == z))
                    {
                        Debug.Log("ZOSTALO DODANE DOOOOOOOOOOOOOOOOOOOOOO x " + x + " z " + z + " VALueREC w pozycji x " + positionX + " z " + positionZ);
                        Delegate[] deleteHoleList = gridHole[x, z].GetDeleteHole();

                        bool haveDelete = false;

                        if (deleteHoleList != null)
                        {

                            foreach (Delegate del in deleteHoleList)
                            {
                                if (handler.Method == del.Method && handler.Target == del.Target)
                                {
                                    Debug.Log("JUZ JEST TAKIE SAMOOOOO");
                                    haveDelete = true;
                                    break;
                                }
                            }
                        }

                        if (!haveDelete) {
                            gridHole[x, z].deleteHole += handler;
                        }
                    }
                }
            }


        }

        gridInMesh.RecalculateNormals();
    }

    private bool okada = false;

    public void ResizeHole()
    {
        if (!okada)
        {
            for (int x = 0; x < gridHole.GetLength(0); x++)
            {
                for (int y = 0; y < gridHole.GetLength(1); y++)
                {
                    if (!gridHole[x, y].getEmpty())
                    {
                        gridHole[x, y].StartFire();
                        return;
                    }
                }
            }
            okada = true;
        }
    }

    public void MoveHole()
    {
        var movedHoles = new HashSet<Hole>();

        for (int x = 0; x < wymiarX; x++)
        {
            for (int z = 0; z < wymiarZ; z++)
            {
                if (!movedHoles.Contains(gridHole[x, z]))
                {
                    gridHole[x, z].MoveHole();
                    movedHoles.Add(gridHole[x, z]);

                }
            }
        }

        gridInMesh.RecalculateNormals();
    }

    public void TwoHole(Hole hole, int positionX, int positionZ)
    {
        gridHole[positionX, positionZ] = hole;
    }

    public void DeleteTwoHole(int[] triangles,int positionX, int positionZ)
    {
        gridHole[positionX, positionZ] = null;
        RepairGrid(triangles);
    }

    public bool checkPossibleSquare(int positionX, int positionZ) {

        if (positionX < 0 || positionX >= wymiarX) {
            return false;
        }

        if (positionZ < 0 || positionZ >= wymiarZ) {
            return false;
        }

        if (gridHole[positionX, positionZ].getEmpty()) {
            return true;
        }

        return false;
    }

    public int[] DeleteSquare(int[] triangles, int positionX, int positionZ) {

        int rightUp = (positionX + 1) + ((wymiarX + 1) * (positionZ + 1));
        int rightDown = (positionX + 1) + ((wymiarX + 1) * positionZ);
        int leftDown = positionX + ((wymiarX + 1) * positionZ);
        int leftUp = positionX + ((wymiarX + 1) * (positionZ + 1));

        int wyliczenia = (positionX + wymiarX * positionZ) * 3 * 2;

        if (triangles[wyliczenia] == leftDown && triangles[wyliczenia + 1] == leftUp && triangles[wyliczenia + 2] == rightUp)
        {
            // lewy dolny
            // lewy gorny
            // prawy gorny

            triangles[wyliczenia] = 0;
            triangles[wyliczenia + 1] = 0;
            triangles[wyliczenia + 2] = 0;
        }

        if (triangles[wyliczenia + 3] == rightDown && triangles[wyliczenia + 4] == leftDown && triangles[wyliczenia + 5] == rightUp)
        {
            // prawy dolny
            // lewy dolny
            // prawy gorny

            triangles[wyliczenia + 3] = 0;
            triangles[wyliczenia + 4] = 0;
            triangles[wyliczenia + 5] = 0;

        }

        /*
        int counter = 0;

        for (int i = 0; i < triangles.Length; i++)
        {
            if (i + 3 < triangles.Length)
            {
                //Usuwanie
                if (triangles[i] == leftDown && triangles[i + 1] == leftUp && triangles[i + 2] == rightUp)
                {
                    // lewy dolny
                    // lewy gorny
                    // prawy gorny

                    triangles[i] = 0;
                    triangles[i + 1] = 0;
                    triangles[i + 2] = 0;
                    counter++;
                }

                if (triangles[i] == rightDown && triangles[i + 1] == leftDown && triangles[i + 2] == rightUp)
                {
                    // prawy dolny
                    // lewy dolny
                    // prawy gorny

                    triangles[i] = 0;
                    triangles[i + 1] = 0;
                    triangles[i + 2] = 0;
                    counter++;
                }

                if (counter >= 2)
                {
                    break;
                }
            }
        }
        */

        return triangles;
    }

    public void DeleteHole(int positionX, int positionZ)
    {
        if (positionX < 0 || positionX >= wymiarX ||
            positionZ < 0 || positionZ >= wymiarZ)
        {
            return;
        }

        if (gridHole[positionX, positionZ] == null)
        {
            return;
        }

        Debug.Log("Wielkosc tego mesha w punktach w grid 1 " + gridInMesh.vertices.Length);

        if (!gridHole[positionX, positionZ].getEmpty())
        {
            for (int x = 0; x < wymiarX; x++)
            {
                for (int z = 0; z < wymiarZ; z++)
                {
                    if (gridHole[positionX, positionZ] == gridHole[x, z] && (positionX != x || positionZ != z))
                    {
                        //Debug.Log("PAUZAAAA TAKIE SAME"); 
                        //Debug.Log("PAUZAAAA X " + positionX + " x " + x);

                        gridHole[x, z] = null;
                    }
                }
            }

            Debug.Log("POZYCJA USUWANIAAAA X " + positionX + " Z " + positionZ);

            gridHole[positionX, positionZ].DeleteHoleFromMesh();


            for (int x = 0; x < wymiarX; x++)
            {
                for (int z = 0; z < wymiarZ; z++)
                {
                    if (gridHole[x, z] != null)
                    {
                        gridHole[x, z].deleteHole -= gridHole[positionX, positionZ].RecalculateStartValue;
                    }
                }
            }

            gridHole[positionX, positionZ] = null;

           

            Debug.Log("Wielkosc tego mesha w punktach w grid 2 " + gridInMesh.vertices.Length);

            //gridHole[0, 1].DeleteHoleFromMesh();
            //
            //Debug.Log("Wielkosc tego mesha w punktach w grid 3 " + gridInMesh.vertices.Length);
            //
            //gridHole[1, 1].DeleteHoleFromMesh();

            

            for (int x = 0; x < wymiarX; x++)
            {
                for (int z = 0; z < wymiarZ; z++)
                {
                    if (gridHole[x, z] == null)
                    {
                        Debug.Log("Puste pozycje x " + x + " z " + z);
                    }
                }
            }

            RepairGrid(null);
            gridInMesh.RecalculateNormals();
        }
    }

    public int[] RepairGrid(int[] trianglesArg)
    {
        int[] triangles;

        if (trianglesArg != null)
        {
            triangles = trianglesArg;
        }
        else {
            triangles = gridInMesh.triangles;
        }

        


        for (int x = 0; x < wymiarX; x++) // wymmiarX
        {
            for (int z = 0; z < wymiarZ; z++) // for (int z = 0; z < wymiarZ; z++) {
            {
                if (gridHole[x, z] == null)
                {
                    int rUp = (x + 1) + ((wymiarX + 1) * (z + 1));
                    int rDown = (x + 1) + ((wymiarX + 1) * z);
                    int lDown = x + ((wymiarX + 1) * z);
                    int lUp = x + ((wymiarX + 1) * (z + 1));

                    bool first = true;
                    //Debug.Log("Pozycja x " + x + " z " + z);
                    //Debug.Log("Pozycja prawa gora " + rUp + " prawy dol " + rDown + " lewy dol " + lDown + " lewy gora " + lUp);
                    //Debug.Log("pozycja XZ " + positionX * positionZ); (x + z * (wymiarX + 1)) * 2 * 3
                    for (int i = (lDown - z) * 2 * 3; i < triangles.Length - 3; i++)
                    {
                        //Debug.Log(triangles[i] + " " + triangles[i + 1] + " "+ triangles[i + 2] );
                        if (triangles[i] == 0 && triangles[i + 1] == 0 && triangles[i + 2] == 0)
                        {
                            //Debug.Log("zerrraaa dla " + i);
                            if (first)
                            {

                                //Tworzenie

                                // lewy dolny
                                // lewy gorny
                                // prawy gorny

                                triangles[i] = lDown;
                                triangles[i + 1] = lUp;
                                triangles[i + 2] = rUp;
                                first = false;
                            }
                            else
                            {
                                //Tworzenie

                                // prawy dolny
                                // lewy dolny
                                // prawy gorny

                                triangles[i] = rDown;
                                triangles[i + 1] = lDown;
                                triangles[i + 2] = rUp;
                                break;
                            }
                        }

                    }

                    gridHole[x, z] = ScriptableObject.CreateInstance<Hole>();
                }

            }
        }

        if (trianglesArg != null)
        {
            return triangles;
        }
        else
        {
            gridInMesh.triangles = triangles;
            gridInMesh.RecalculateNormals();

            return triangles;
        }
    }

}
