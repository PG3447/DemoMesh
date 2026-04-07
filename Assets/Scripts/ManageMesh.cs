using UnityEditor;
using UnityEngine;

public class ManageMesh : MonoBehaviour
{
    public int count_X = 4;
    public int count_Y = 2;
    public float size = 6.0f;

    public float offsetX = 0f;
    public float offsetZ = 0f;

    private float Height;
    private float Width;

    private Mesh mesh;
    private Grid grid;

    void Start()
    {
        Width = Screen.currentResolution.width;
        Height = Screen.currentResolution.height;

        Camera camera = Camera.main;

        float distanceObjectToCamera = Vector3.Distance(gameObject.transform.position, camera.transform.position);
        float heightMesh = 2.0f * Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad / 2f) * distanceObjectToCamera;
        float widthMesh =  heightMesh * camera.aspect;

        Debug.Log("Potencjalna wysokosc obiektu: " + heightMesh);
        Debug.Log("Potencjalna szerokosc obiektu: " + widthMesh);

        int ileSieZmiescNaWysokosc = (int)(heightMesh / size);
        int ileSieZmiescNaSzerokosc = (int)(widthMesh / size);

        Debug.Log("Potencjalnie tyle sie zmiesci na wysokosc " + ileSieZmiescNaWysokosc);
        Debug.Log("Potencjalnie tyle sie zmiesci na szerokosc " + ileSieZmiescNaSzerokosc);

        float resztaNaWysokosc = heightMesh - (ileSieZmiescNaWysokosc * size);
        float resztaNaSzerokosc = widthMesh - (ileSieZmiescNaSzerokosc * size);
        
        Debug.Log("Potencjalna reszta na wysokosc " + resztaNaWysokosc);
        Debug.Log("Potencjalna reszta na szerokosc " + resztaNaSzerokosc);

        /*
        float cameraFov = camera.fieldOfView;
        float cameraNear = camera.nearClipPlane;

        // Konwersja FOV do radianów
        float fovRad = cameraFov * Mathf.Deg2Rad;

        // Obliczanie szerokoœci i wysokoœci obiektu
        float objectWidth = 2f * cameraNear * Mathf.Tan(fovRad / 2f) * screenAspectRatio;
        float objectHeight = 2f * cameraNear * Mathf.Tan(fovRad / 2f);

        Debug.Log("Potencjalna szerokosc obiektu: " + objectWidth);
        Debug.Log("Potencjalna wysokosc obiektu: " + objectHeight);
        */


        //mesh = this.GetComponent<Mesh>();
        //
        //if (mesh == null) {
        //    mesh = new Mesh();
        //}

        //this.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        this.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Mesh");

        mesh = new Mesh();

        grid = new Grid(count_X, count_Y, size, offsetX, offsetZ, mesh, gameObject);

        grid.CreateGrid();

        //AddHoleInitial();
        AddAllHole();

        UpdateMeshPlane();
    }

    private void AddAllHole()
    {
        for (int z = 0; z < count_Y / 2; z++)
        {
            for (int x = 0; x < count_X / 2; x++)
            {
                grid.AddHole(x, z);
            }
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            grid.UpdateCoordinate(count_X, count_Y, size, mesh);
            grid.CreateGrid();

            UpdateMeshPlane();
        }

        //if (Input.GetKey(KeyCode.M)) {
        //    grid.MoveHole();
        //}

        grid.MoveHole();
        UpdateMeshPlane();


        //if (Input.GetKey(KeyCode.I)) {
        //    grid.DeleteHole(1, 1);
        //    //EditorApplication.isPaused = true;
        //}

        if (Input.GetKeyDown(KeyCode.F))
        {
            grid.ResizeHole();
            //EditorApplication.isPaused = true;
        }

        if (Input.GetKey(KeyCode.R)) {
            grid.RepairGrid(null);
        }

        if (Input.GetMouseButtonDown(0))
        {
            (int positionX, int positionZ) = grid.PositionToPositionInGrid(GetPositionMouse());
            Debug.Log("pozycja x "+ positionX + " pozycja Z " + positionZ);
            grid.AddHole(positionX, positionZ);
        }

        if (Input.GetMouseButtonDown(1))
        {
            (int positionX, int positionZ) = grid.PositionToPositionInGrid(GetPositionMouse());
            grid.DeleteHole(positionX, positionZ);
        }
    }

    private Vector3 GetPositionMouse()
    {
        Vector3 mousePosition = Input.mousePosition;
        float distance = Vector3.Distance(Camera.main.transform.position, this.transform.position);
        mousePosition.z = distance; //Camera.main.nearClipPlane
        Vector3 mouseInWorld = Camera.main.ScreenToWorldPoint(mousePosition);
        Debug.Log("Pozycja myszki " + mouseInWorld);

        return mouseInWorld;
    }

    void UpdateMeshPlane() {
        this.GetComponent<MeshFilter>().sharedMesh = mesh;
        this.GetComponent<MeshCollider>().sharedMesh = mesh;
        mesh.RecalculateNormals();
    }
}
