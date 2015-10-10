using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex
        {
            get;
            set;
        }
    }

    [Serializable]
    public class GridGraph
    {
        public float _NodeRadius = 1f;
        public Vector2 _WorldSize = new Vector2(100, 100);
        public int angleLimit = 90;
        public Vector3 center;
        public string name = "New Grid";
        public Node[,] nodes;
        public bool Scanning;
        public LayerMask WalkableMask;
        private Vector2 _Size;

        public int maxSize
        {
            get
            {
                return Mathf.RoundToInt(Size.x * Size.y);
            }
        }

        public float NodeRadius
        {
            get
            {
                return _NodeRadius;
            }
            set
            {
                _NodeRadius = value;
                Update();
            }
        }

        public Vector2 Size
        {
            get
            {
                return _Size;
            }
        }

        public Vector2 WorldSize
        {
            get
            {
                return _WorldSize;
            }
            set
            {
                _WorldSize = value;
                Update();
            }
        }

        public int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.x - nodeB.x);
            int dstY = Mathf.Abs(nodeA.y - nodeB.y);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }

        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.x + x;
                    int checkY = node.y + y;

                    if (checkX >= 0 && checkX < Size.x && checkY >= 0 && checkY < Size.y)
                    {
                        neighbours.Add(nodes[checkX, checkY]);
                    }
                }
            }

            return neighbours;
        }

        public Vector3 GridToWorld(Vector3 OPPOS)
        {
            Vector3 World = new Vector3();

            World.x = OPPOS.x * (NodeRadius * 2) - WorldSize.x / 2;
            World.z = OPPOS.z * (NodeRadius * 2) - WorldSize.y / 2;

            return World;
        }

        public Node NearWalkable(Vector3 worldPosition)
        {
            if (nodes == null || nodes.Length == 0)
            {
                return null;
            }
            float percentX = Mathf.Clamp01((worldPosition.x) / WorldSize.x);
            float percentY = Mathf.Clamp01((worldPosition.z) / WorldSize.y);

            int x = Mathf.RoundToInt((Size.x - 1) * percentX);
            int y = Mathf.RoundToInt((Size.y - 1) * percentY);

            int MaxDistance = 50;
            if (x * y < nodes.Length)
            {
                for (int X = 0; X < MaxDistance; X++)
                {
                    for (int Y = 0; Y < MaxDistance; Y++)
                    {
                        int X1 = x + X;
                        int Y1 = y + Y;
                        X1 = Mathf.Clamp(X1, 0, nodes.GetLength(0) - 1);
                        Y1 = Mathf.Clamp(Y1, 0, nodes.GetLength(1) - 1);

                        int X2 = x - X;
                        int Y2 = y - Y;
                        X2 = Mathf.Clamp(X2, 0, nodes.GetLength(0) - 1);
                        Y2 = Mathf.Clamp(Y2, 0, nodes.GetLength(1) - 1);

                        if (nodes[X1, Y1] != null && nodes[X1, Y1].Walkable)
                        {
                            return nodes[X1, Y1];
                        }
                        else if (nodes[X2, Y2] != null && nodes[X2, Y2].Walkable)
                        {
                            return nodes[X2, Y2];
                        }
                    }
                }
            }

            return null;
        }

        public Node NearWaterable(Vector3 worldPosition, float WaterLevel)
        {
            if (nodes == null || nodes.Length == 0)
            {
                return null;
            }
            float percentX = Mathf.Clamp01((worldPosition.x) / WorldSize.x);
            float percentY = Mathf.Clamp01((worldPosition.z) / WorldSize.y);

            int x = Mathf.RoundToInt((Size.x - 1) * percentX);
            int y = Mathf.RoundToInt((Size.y - 1) * percentY);

            int MaxDistance = 500;
            List<Node> open = new List<Node>();

            Node lowest = null;

            if (x * y < nodes.Length)
            {
                for (int X = 0; X < MaxDistance; X++)
                {
                    for (int Y = 0; Y < MaxDistance; Y++)
                    {
                        if (lowest != null && lowest.height < WaterLevel)
                        {
                            break;
                        }
                        int X1 = x + X;
                        int Y1 = y + Y;
                        X1 = Mathf.Clamp(X1, 0, nodes.GetLength(0) - 1);
                        Y1 = Mathf.Clamp(Y1, 0, nodes.GetLength(1) - 1);

                        int X2 = x - X;
                        int Y2 = y - Y;
                        X2 = Mathf.Clamp(X2, 0, nodes.GetLength(0) - 1);
                        Y2 = Mathf.Clamp(Y2, 0, nodes.GetLength(1) - 1);

                        if (nodes[X1, Y1] != null)
                        {
                            foreach (Node n in GetNeighbours(nodes[X1, Y1]))
                            {
                                if (n != null && n.height > nodes[X1, Y1].height && open.Contains(n))
                                {
                                    lowest = nodes[X1, Y1];
                                    open.Add(lowest);
                                }
                            }

                            if (nodes[X1, Y1].height <= WaterLevel)
                            {
                                return nodes[X1, Y1];
                            }
                        }

                        if (nodes[X2, Y2] != null)
                        {
                            foreach (Node n in GetNeighbours(nodes[X2, Y2]))
                            {
                                if (n != null && n.height > nodes[X2, Y2].height && open.Contains(n))
                                {
                                    lowest = nodes[X2, Y2];
                                    open.Add(lowest);
                                }
                            }

                            if (nodes[X2, Y2].height <= WaterLevel)
                            {
                                return nodes[X2, Y2];
                            }
                        }
                    }
                }
            }
            Debug.Log("Lowest");
            return lowest;
        }

        public Node NodeFromWorldPos(Vector3 worldPosition)
        {
            if (nodes == null || nodes.Length == 0)
            {
                return null;
            }
            float percentX = Mathf.Clamp01((worldPosition.x) / WorldSize.x);
            float percentY = Mathf.Clamp01((worldPosition.z) / WorldSize.y);

            int x = Mathf.RoundToInt((Size.x - 1) * percentX);
            int y = Mathf.RoundToInt((Size.y - 1) * percentY);
            if (x * y > nodes.Length)
            {
                return nodes[0, 0];
            }
            else
            {
                return nodes[x, y];
            }
        }

        public Vector3 NodeToWorldPos(Node node)
        {
            Vector3 worldPos = new Vector3();

            worldPos.x = (node.x * (NodeRadius * 2)) - WorldSize.x / 2;
            worldPos.y = node.height;
            worldPos.z = (node.y * (NodeRadius * 2)) - WorldSize.y / 2;

            return center + worldPos;
        }

        public void OnScanDone()
        {
            Update();
        }

        public Path RetracePath(Node start, Node end)
        {
            Path P = new Path();
            Node Cur = end;

            while (Cur != start)
            {
                P.movementPath.Add(Cur);
                Cur = Cur.parent;
            }
            P.movementPath.Reverse();

            return P;
        }

        public void ScanNode(int x, int y)
        {
            if (nodes == null || nodes.Length == 0)
            {
                Update();
                nodes = new Node[Mathf.RoundToInt(Size.x), Mathf.RoundToInt(Size.y)];
            }
            bool Walk = false;
            RaycastHit hit = new RaycastHit();

            Vector3 endPos = center + Vector2ToVector3(_WorldSize) / 2 + GridToWorld(new Vector3(x, 0, y));

            Vector3 startPos = endPos;
            startPos.y = 500;
            endPos.y = 0;
            if (Physics.Linecast(startPos, endPos, out hit))
            {
                endPos.y = hit.point.y;
                if (Vector3.Angle(Vector3.up, hit.normal) < angleLimit)
                {
                    Walk = (Physics.CheckSphere(endPos, (NodeRadius * 2), WalkableMask));
                }
            }
            nodes[x, y] = new Node(x, y, Walk, endPos);
        }

        public Vector3 Vector2ToVector3(Vector2 vec)
        {
            return new Vector3(vec.x, 0, vec.y);
        }

        private void Update()
        {
            _Size.x = Mathf.FloorToInt(WorldSize.x / (NodeRadius * 2));
            _Size.y = Mathf.FloorToInt(WorldSize.y / (NodeRadius * 2));
        }
    }

    public class Heap<T> where T : IHeapItem<T>
    {
        private int currentItemCount;
        private T[] items;

        public Heap(int maxHeapSize)
        {
            items = new T[maxHeapSize];
        }

        public int Count
        {
            get
            {
                return currentItemCount;
            }
        }

        public void Add(T item)
        {
            item.HeapIndex = currentItemCount;
            items[currentItemCount] = item;
            SortUp(item);
            currentItemCount++;
        }

        public bool Contains(T item)
        {
            return Equals(items[item.HeapIndex], item);
        }

        public T RemoveFirst()
        {
            T firstItem = items[0];
            currentItemCount--;
            items[0] = items[currentItemCount];
            items[0].HeapIndex = 0;
            SortDown(items[0]);
            return firstItem;
        }

        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        private void SortDown(T item)
        {
            while (true)
            {
                int childIndexLeft = item.HeapIndex * 2 + 1;
                int childIndexRight = item.HeapIndex * 2 + 2;
                int swapIndex = 0;

                if (childIndexLeft < currentItemCount)
                {
                    swapIndex = childIndexLeft;

                    if (childIndexRight < currentItemCount)
                    {
                        if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                        {
                            swapIndex = childIndexRight;
                        }
                    }

                    if (item.CompareTo(items[swapIndex]) < 0)
                    {
                        Swap(item, items[swapIndex]);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        private void SortUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;

            while (true)
            {
                T parentItem = items[parentIndex];
                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(item, parentItem);
                }
                else
                {
                    break;
                }

                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }

        private void Swap(T itemA, T itemB)
        {
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;
            int itemAIndex = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = itemAIndex;
        }
    }

    public class Node : IHeapItem<Node>
    {
        public int gCost;
        public int hCost;
        public float height;
        public Node parent;
        public bool Walkable = false;
        public Vector3 WorldPosition = Vector3.zero;
        public int x;
        public int y;
        private int heapIndex;

        public Node(int _x, int _y, bool _Walk, Vector3 _WorldPos)
        {
            x = _x;
            y = _y;
            Walkable = _Walk;
            WorldPosition = _WorldPos;
            height = _WorldPos.y;
        }

        public int fCost
        {
            get
            {
                return gCost + hCost;
            }
        }

        public int HeapIndex
        {
            get
            {
                return heapIndex;
            }
            set
            {
                heapIndex = value;
            }
        }

        public int CompareTo(Node node1)
        {
            int compare = fCost.CompareTo(node1.fCost);
            if (compare == 0)
            {
                compare = hCost.CompareTo(node1.hCost);
            }
            return -compare;
        }
    }

    [Serializable]
    public class Path
    {
        public List<Node> movementPath = new List<Node>();
        public bool Success = true;
        public Vector3[] Vector3Path = new Vector3[0];

        public Vector3 Destination
        {
            get
            {
                return Vector3Path[Vector3Path.Length - 1];
            }
        }

        public void Update()
        {
            Vector3Path = VectorPath(movementPath);
        }

        public Vector3[] VectorPath(List<Node> p)
        {
            List<Vector3> v3 = new List<Vector3>();
            foreach (Node n in p)
            {
                v3.Add(n.WorldPosition);
            }
            return v3.ToArray();
        }
    }

    public class PathRequest
    {
        public Action<Path> callback;
        public Vector3 pathStart;

        public PathRequest(Vector3 _start, Action<Path> _callback)
        {
            pathStart = _start;
            callback = _callback;
        }
    }
}