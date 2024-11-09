using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    [SerializeField] private Material meshMaterial;

    private MeshRenderer roadMeshRenderer;
    private MeshFilter roadMeshFilter;
    private Mesh roadMesh;

    private List<Vector3> roadVerts;
    private List<int> roadTris;
    private int leftIndex = 0;

    private float currentAngle;
    [SerializeField] public float maxTurnAng = 5;

    private Vector2 currentLeftPoint;
    private Vector2 currentRightPoint;

    private Vector2 centerCircle;

    public void Start()
    {
        SetupComponents();
        GenerateRoad(.1f);
    }

    private void SetupComponents()
    {
        roadMeshRenderer = gameObject.AddComponent<MeshRenderer>();
        roadMeshFilter = gameObject.AddComponent<MeshFilter>();
        roadMeshRenderer.material = meshMaterial;

        roadMesh = new Mesh();
        roadTris = new List<int>();
    }

    private void GenerateRoad(float waitTime)
    {
        roadVerts = new List<Vector3>();

        currentLeftPoint = new Vector2(-.5f, 0);
        currentRightPoint = new Vector2(.5f, 0);

        roadVerts.Add(new Vector3(currentLeftPoint.x, 0, currentLeftPoint.y));
        roadVerts.Add(new Vector3(currentRightPoint.x, 0, currentRightPoint.y));

        StartCoroutine(CreateQuads(waitTime));
    }
    private IEnumerator CreateQuads(float waitTime)
    {
        // Quads for the road
        AddQuad(5, 0, 0);
        for (int i = 0; i < 100; i++)
        {
            if (i < 25)
            {
                AddQuad(0, i, 5);
            }
            else if (i >= 25 && i < 40)
            {
                AddQuad(0, -i, 5);
            }
            else if (i >= 40 && i < 50)
            {
                AddQuad(0, i, 10);
            }
            else if (i >= 50 && i < 92)
            {
                AddQuad(0, -i, 5);
            }
            else if (i == 92)
            {
                AddQuad(22, 0, 0);
            }
            else if (i > 92 && i < 99)
            {
                AddQuad(0, -i, 7);
            }
            else
            {
                AddQuad(9, 0, 0);
            }
            UpdateMesh();

            yield return new WaitForSeconds(waitTime);
        }
    }
    private void UpdateMesh()
    {
        roadMesh.vertices = roadVerts.ToArray();
        roadMesh.triangles = roadTris.ToArray();

        List<Vector2> uv = new List<Vector2>();
        for (int o = 0; o < roadVerts.Count; o++)
        {
            uv.Add(new Vector2(roadVerts[o].x, roadVerts[o].z));
        }
        roadMesh.uv = uv.ToArray();

        roadMeshFilter.mesh = roadMesh;
    }

    private void AddQuad(float distance, float angle, float radious)
    {
        // Clamp the angle to the max turn angle
        angle = Mathf.Clamp(angle, -maxTurnAng, maxTurnAng);
        currentAngle += angle * Mathf.Deg2Rad;

        if (angle == 0)
        {
            // -- Straight road part --
            float xDist = distance * Mathf.Sin(currentAngle);
            float yDist = distance * Mathf.Cos(currentAngle);

            currentRightPoint += new Vector2(xDist, yDist);
            currentLeftPoint += new Vector2(xDist, yDist);
        }
        else
        {
            // -- Curved road part --
            Vector2 diff = currentRightPoint - currentLeftPoint;
            int multiplier = angle > 0 ? 1 : -1;
            centerCircle = currentRightPoint + diff * multiplier * radious;

            // -- Calculate new left and right vertices --
            float xPos = radious * -multiplier * Mathf.Cos(currentAngle);
            float zPos = radious * multiplier * Mathf.Sin(currentAngle);
            Vector2 rightVert = centerCircle + new Vector2(xPos, zPos);

            float xPos0 = radious * -multiplier * Mathf.Cos(currentAngle) - Mathf.Cos(currentAngle);
            float zPos0 = radious * multiplier * Mathf.Sin(currentAngle) + Mathf.Sin(currentAngle);
            Vector2 leftVert = centerCircle + new Vector2(xPos0, zPos0);

            currentRightPoint = new Vector2(rightVert.x, rightVert.y);
            currentLeftPoint = new Vector2(leftVert.x, leftVert.y);
        }
        // -- Apply vertices and calculate its triangle indices --
        roadVerts.Add(new Vector3(currentLeftPoint.x, 0, currentLeftPoint.y));
        roadVerts.Add(new Vector3(currentRightPoint.x, 0, currentRightPoint.y));

        // First triangle
        roadTris.Add(leftIndex);
        roadTris.Add(leftIndex + 2);
        roadTris.Add(leftIndex + 1);

        // Second triangle
        roadTris.Add(leftIndex + 1);
        roadTris.Add(leftIndex + 2);
        roadTris.Add(leftIndex + 3);

        leftIndex += 2;
    }

    private void OnDrawGizmos()
    {
        // -- Draw road vertices --
        foreach (Vector3 vertex in roadVerts)
        {
            Gizmos.DrawSphere(vertex, .1f);
        }

        // -- Draw handle --
        Gizmos.DrawSphere(new Vector3(centerCircle.x, 0, centerCircle.y), .3f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(centerCircle.x, 0, centerCircle.y), new Vector3(currentRightPoint.x, 0, currentRightPoint.y));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(roadVerts[^1], roadVerts[^5]);
    }

}
