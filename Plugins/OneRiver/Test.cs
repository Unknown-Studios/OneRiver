using Pathfinding;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Path path;
    public float WaterLevel = 10f;

    private void FindPath()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GridManager.RequestPath(hit.point, OnPath, WaterLevel);
        }
    }

    private void OnDrawGizmos()
    {
        foreach (Node n in path.movementPath)
        {
            Gizmos.DrawCube(n.WorldPosition, Vector3.one);
        }

        for (int i = 1; i < path.Vector3Path.Length; i++)
        {
            Gizmos.DrawLine(path.Vector3Path[i - 1], path.Vector3Path[i]);
        }
    }

    private void OnPath(Path p)
    {
        if (p.Success)
        {
            path = p;
        }
    }

    private void Start()
    {
        InvokeRepeating("FindPath", 0f, 5f);
    }
}