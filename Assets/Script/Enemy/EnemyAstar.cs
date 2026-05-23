using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton A* Pathfinder untuk grid 2D.
/// Letakkan satu GameObject di scene dengan script ini.
/// Gunakan: AStarPathfinder.Instance.FindPath(from, to)
/// </summary>
public class EnemyAstar : MonoBehaviour
{
    public static EnemyAstar Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private float nodeSize   = 0.5f;
    [SerializeField] private int   gridWidth  = 60;
    [SerializeField] private int   gridHeight = 60;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private bool drawGizmos = false;

    private Node[,] grid;
    private Vector2 gridOrigin;

    // ── Lifecycle ────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Hanya hapus KOMPONEN ini (bukan seluruh GameObject-nya!).
            // Jika EnemyAstar ada di dalam enemy prefab, Destroy(gameObject)
            // akan menghancurkan seluruh enemy ke-2. Destroy(this) hanya
            // menghapus komponen EnemyAstar duplikat saja.
            Destroy(this);
            return;
        }

        Instance = this;
        BuildGrid();
    }

    // ── Grid Building ────────────────────────────────────────

    private void BuildGrid()
    {
        gridOrigin = (Vector2)transform.position
            - new Vector2(gridWidth * nodeSize * 0.5f, gridHeight * nodeSize * 0.5f);

        grid = new Node[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        for (int y = 0; y < gridHeight; y++)
        {
            Vector2 worldPos = GridToWorld(x, y);
            bool walkable    = !Physics2D.OverlapCircle(worldPos, nodeSize * 0.4f, obstacleLayer);
            grid[x, y]       = new Node(walkable, worldPos, x, y);
        }
    }

    /// <summary>Panggil ini jika obstacle berubah saat runtime (pintu, destructible, dll).</summary>
    public void RebuildGrid() => BuildGrid();

    // ── Coordinate Helpers ───────────────────────────────────

    private Vector2 GridToWorld(int x, int y)
        => gridOrigin + new Vector2(x * nodeSize + nodeSize * 0.5f, y * nodeSize + nodeSize * 0.5f);

    private Node WorldToNode(Vector2 world)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt((world.x - gridOrigin.x) / nodeSize), 0, gridWidth  - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt((world.y - gridOrigin.y) / nodeSize), 0, gridHeight - 1);
        return grid[x, y];
    }

    // ── A* Core ──────────────────────────────────────────────

    /// <summary>
    /// Temukan jalur dari startPos ke endPos.
    /// Return: List Vector2 waypoints berurutan, atau null jika tidak ada jalur.
    /// </summary>
    public List<Vector2> FindPath(Vector2 startPos, Vector2 endPos)
    {
        Node startNode = WorldToNode(startPos);
        Node endNode   = WorldToNode(endPos);

        // Fallback jika node target/start tidak walkable
        if (!startNode.walkable) startNode = FindNearestWalkable(startNode);
        if (!endNode.walkable)   endNode   = FindNearestWalkable(endNode);
        if (startNode == null || endNode == null) return null;
        if (startNode == endNode) return null;

        // Reset cost sebelum pencarian baru
        foreach (Node n in grid) n.Reset();

        var openSet   = new List<Node>();
        var closedSet = new HashSet<Node>();

        startNode.gCost = 0;
        startNode.hCost = Heuristic(startNode, endNode);
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node current = PopLowestF(openSet);
            if (current == endNode) return BuildPath(startNode, endNode);

            closedSet.Add(current);

            foreach (Node nb in GetNeighbours(current))
            {
                if (!nb.walkable || closedSet.Contains(nb)) continue;

                // Diagonal cost lebih besar dari cardinal
                bool diagonal     = nb.gridX != current.gridX && nb.gridY != current.gridY;
                float moveCost    = current.gCost + (diagonal ? 1.414f : 1f);

                if (moveCost < nb.gCost)
                {
                    nb.gCost  = moveCost;
                    nb.hCost  = Heuristic(nb, endNode);
                    nb.parent = current;
                    if (!openSet.Contains(nb)) openSet.Add(nb);
                }
            }
        }

        return null; // tidak ada jalur
    }

    // ── Path Helpers ─────────────────────────────────────────

    private float Heuristic(Node a, Node b)
    {
        // Octile distance — optimal untuk 8-arah movement
        float dx = Mathf.Abs(a.gridX - b.gridX);
        float dy = Mathf.Abs(a.gridY - b.gridY);
        return Mathf.Max(dx, dy) + (1.414f - 1f) * Mathf.Min(dx, dy);
    }

    private Node PopLowestF(List<Node> list)
    {
        int best = 0;
        for (int i = 1; i < list.Count; i++)
            if (list[i].FCost < list[best].FCost
            || (list[i].FCost == list[best].FCost && list[i].hCost < list[best].hCost))
                best = i;

        Node node = list[best];
        list.RemoveAt(best);
        return node;
    }

    private List<Node> GetNeighbours(Node n)
    {
        var result = new List<Node>(8);
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        {
            if (dx == 0 && dy == 0) continue;
            int nx = n.gridX + dx, ny = n.gridY + dy;
            if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight)
                result.Add(grid[nx, ny]);
        }
        return result;
    }

    private List<Vector2> BuildPath(Node start, Node end)
    {
        var path    = new List<Vector2>();
        Node current = end;
        while (current != start && current != null)
        {
            path.Add(current.worldPosition);
            current = current.parent;
        }
        path.Reverse(); // dari start → end
        return path;
    }

    private Node FindNearestWalkable(Node origin)
    {
        for (int r = 1; r < 8; r++)
        for (int dx = -r; dx <= r; dx++)
        for (int dy = -r; dy <= r; dy++)
        {
            int nx = Mathf.Clamp(origin.gridX + dx, 0, gridWidth  - 1);
            int ny = Mathf.Clamp(origin.gridY + dy, 0, gridHeight - 1);
            if (grid[nx, ny].walkable) return grid[nx, ny];
        }
        return null;
    }

    // ── Gizmos ───────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (!drawGizmos || grid == null) return;
        foreach (Node n in grid)
        {
            Gizmos.color = n.walkable
                ? new Color(0f, 1f, 0f, 0.06f)
                : new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeSize * 0.85f));
        }
    }

    // ════════════════════════════════════════════════════════
    //  NODE CLASS
    // ════════════════════════════════════════════════════════

    public class Node
    {
        public bool    walkable;
        public Vector2 worldPosition;
        public int     gridX, gridY;
        public float   gCost, hCost;
        public Node    parent;

        public float FCost => gCost + hCost;

        public Node(bool walkable, Vector2 worldPosition, int gridX, int gridY)
        {
            this.walkable       = walkable;
            this.worldPosition  = worldPosition;
            this.gridX          = gridX;
            this.gridY          = gridY;
            Reset();
        }

        public void Reset()
        {
            gCost  = float.MaxValue;
            hCost  = 0f;
            parent = null;
        }
    }
}