using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GridManager : MonoBehaviour
{
    #region Fields

    public static bool isScanning;
    public DebugLevel DebugLvl;
    public GridGraph grid;
    private static GridManager _instance;
    private PathRequest currentPathRequest;

    #endregion Fields

    #region Enums

    public enum DebugLevel
    {
        None = 0,
        Low = 1,
        High = 2,
    }

    #endregion Enums

    #region Properties

    public static GridGraph Grid
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance.grid;
        }
    }

    public static GridManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GridManager>();
            }
            return _instance;
        }
    }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Request a path from point pathStart to point pathEnd.
    /// </summary>
    /// <param name="pathStart">The starting point of the path</param>
    /// <param name="pathEnd">The ending point of the path</param>
    /// <param name="callback">A function with the parameters (Path)</param>
    /// <param name="Name">A random string, this is used for identifiing a queue in the queuesystem.</param>
    public static void RequestPath(Vector3 pathStart, Action<Path> callback, float wL)
    {
        instance.currentPathRequest = new PathRequest(pathStart, callback);
        instance.StartCoroutine(instance.FindThePath(instance.currentPathRequest.pathStart, wL));
    }

    /// <summary>
    /// Scan the first grid in the grid array
    /// </summary>
    public static void ScanGrid()
    {
        if (instance == null)
        {
            Debug.Log("A GridManager wasn't found in the scene!");
            return;
        }
        instance.StartCoroutine(instance.ScanAGrid(Grid));
    }

    private IEnumerator FindThePath(Vector3 startPos, float waterLevel)
    {
        if (grid.nodes == null || grid.nodes.Length == 0)
        {
            ScanGrid();
        }

        Path p = new Path();
        bool pathSuccess = false;

        Node startNode = grid.NearWalkable(startPos);
        Node targetNode = grid.NearWaterable(startPos, waterLevel);

        if (startNode == null || targetNode == null)
        {
            OnProccesingDone(p, pathSuccess);
            yield break;
        }

        Node last = null;
        if (startNode.Walkable)
        {
            Heap<Node> open = new Heap<Node>(grid.maxSize);
            HashSet<Node> closed = new HashSet<Node>();
            open.Add(startNode);

            while (open.Count > 0)
            {
                Node currentNode = open.RemoveFirst();
                closed.Add(currentNode);

                if (open.Count == 1)
                {
                    last = currentNode;
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (neighbour == null || !neighbour.Walkable || closed.Contains(neighbour) || neighbour.height > currentNode.height || currentNode.height <= waterLevel)
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + grid.GetDistance(currentNode, neighbour);
                    if ((newMovementCostToNeighbour < neighbour.gCost || !open.Contains(neighbour)))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = grid.GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!open.Contains(neighbour))
                        {
                            open.Add(neighbour);
                        }
                    }
                }
            }
        }
        if (pathSuccess)
        {
            p = grid.RetracePath(startNode, last);
        }
        OnProccesingDone(p, true);
    }

    private void OnDrawGizmos()
    {
        if (grid == null)
        {
            return;
        }
        if (!grid.Scanning)
        {
            if (grid.nodes == null)
            {
                ScanGrid();
                return;
            }
            if (DebugLvl == DebugLevel.High)
            {
                foreach (Node n in grid.nodes)
                {
                    if (n == null)
                    {
                        continue;
                    }
                    if (!n.Walkable)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(n.WorldPosition, Vector3.one * (grid.NodeRadius * 2f));
                    }
                    else
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawCube(n.WorldPosition, Vector3.one * (grid.NodeRadius * 2f));
                    }
                }
            }
            if (DebugLvl == DebugLevel.Low)
            {
                foreach (Node n in grid.nodes)
                {
                    if (n == null)
                    {
                        continue;
                    }
                    if (!n.Walkable)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(n.WorldPosition, Vector3.one * (grid.NodeRadius * 2f));
                    }
                }
            }
        }
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube((grid.center + (grid.Vector2ToVector3(grid.WorldSize) / 2)), grid.Vector2ToVector3(grid.WorldSize));
    }

    private void OnProccesingDone(Path p, bool Success)
    {
        p.Success = Success;
        p.Update();
        if (p.Vector3Path.Length == 0)
        {
            p.Success = false;
        }

        currentPathRequest.callback(p);
        currentPathRequest = null;
    }

    private IEnumerator ScanAGrid(GridGraph grid)
    {
        isScanning = true;
        for (int x = 0; x < grid.Size.x; x++)
        {
            for (int y = 0; y < grid.Size.y; y++)
            {
                grid.ScanNode(x, y);
            }
            if (x % 25 == 0)
            {
                yield return null;
            }
        }
        grid.OnScanDone();
        isScanning = false;
    }

    #endregion Methods
}