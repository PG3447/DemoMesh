using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class FireHole : MonoBehaviour
{
    private Hole parentHole;
    private Transform transformGrid;

    private float maxOdlegloscSrodkowa;

    private int idVerticeFire;

    private float timeFire;

    public void Initialize(Hole valueHole, Transform valueTransformGrid, int poczatekHole, float valueMaxOdlegloscSrodkowa) {
        parentHole = valueHole;
        transformGrid = valueTransformGrid;
        idVerticeFire = poczatekHole;
        maxOdlegloscSrodkowa = valueMaxOdlegloscSrodkowa;
        timeFire = 1f;

        this.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    private void FixedUpdate()
    {
        MovingFire();
    }

    private void MovingFire()
    {

        float factor = 1f + timeFire/60f * Time.deltaTime;

        Vector3[] vertices = parentHole.getAllVertices().ToArray();

        this.transform.position = transformGrid.TransformPoint(vertices[idVerticeFire]);

        Vector3 centerLocalMiddle = parentHole.getCenterLocelMiddle();
        var (rUp, rDown, lDown, lUp) = parentHole.getCalculateMeshPosition();

        Vector3 tempVertices = centerLocalMiddle + (vertices[idVerticeFire] - centerLocalMiddle) * factor;


        if (tempVertices.x < vertices[rUp].x &&
            tempVertices.x > vertices[lDown].x &&
            tempVertices.z < vertices[rUp].z &&
            tempVertices.z > vertices[rDown].z &&
            Vector3.Distance(centerLocalMiddle, tempVertices) < maxOdlegloscSrodkowa)
        {
            timeFire += Time.deltaTime;
            vertices[idVerticeFire] = tempVertices;
        }
        else
        {
            timeFire -= Time.deltaTime;

            if (timeFire <= 0f)
            {
                Destroy(gameObject);
            }
        }

        timeFire = Mathf.Clamp(timeFire, 0f, 6f);
        this.transform.localScale = Vector3.Lerp(new Vector3(0.1f, 0.1f, 0.1f), new Vector3(0.51f, 0.51f, 0.51f), timeFire / 6f);

        parentHole.UpdateMeshHole(vertices);
    }
}
