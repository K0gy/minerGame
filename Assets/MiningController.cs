using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MiningController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap pocTilemap;
    [SerializeField] private Tilemap caveTilemap;
    [SerializeField] private Tilemap cobaltTilemap;
    [SerializeField] private ProceduralGeneration proceduralGen;

    [Header("Mining Settings")]
    [SerializeField] private float miningRadius = 3f;

    [Header("Collapse Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float collapseChance = 0.3f;
    [SerializeField] private int collapseRadius = 4;

    [Header("Falling Tile")]
    [SerializeField] private GameObject fallingTilePrefab;
    [SerializeField] private float fallingTileLifetime = 10f;

    [Header("Falling Tile Sprites")]
    [SerializeField] private Sprite pocFallingSprite;
    [SerializeField] private Sprite caveFallingSprite;
    [SerializeField] private Sprite cobaltFallingSprite;

    private Camera _cam;

    private enum TileKind
    {
        None,
        Poc,
        Cave,
        Cobalt
    }

    private readonly Vector3Int[] dirs =
    {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right
    };

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryMineAtMouse();
        }
    }

    private void TryMineAtMouse()
    {
        if (_cam == null) return;

        Vector3 mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector3Int cellPos = pocTilemap.WorldToCell(mouseWorld);

        Vector3 tileWorldCenter = pocTilemap.GetCellCenterWorld(cellPos);
        float dist = Vector3.Distance(transform.position, tileWorldCenter);
        if (dist > miningRadius) return;

        TileKind tileKind = GetTileKind(cellPos);
        if (tileKind == TileKind.None) return;

        if (!IsSurface(cellPos)) return;

        bool unsupportedFromBelow = IsUnsupportedFromBelow(cellPos);

        ClearTileAtCell(cellPos);

        if (unsupportedFromBelow && Random.value < collapseChance)
        {
            TriggerSmartCollapse(cellPos);
        }
    }

    private TileKind GetTileKind(Vector3Int pos)
    {
        if (pocTilemap != null && pocTilemap.GetTile(pos) != null) return TileKind.Poc;
        if (caveTilemap != null && caveTilemap.GetTile(pos) != null) return TileKind.Cave;
        if (cobaltTilemap != null && cobaltTilemap.GetTile(pos) != null) return TileKind.Cobalt;
        return TileKind.None;
    }

    private Sprite GetSpriteForTileKind(TileKind kind)
    {
        switch (kind)
        {
            case TileKind.Poc: return pocFallingSprite;
            case TileKind.Cave: return caveFallingSprite;
            case TileKind.Cobalt: return cobaltFallingSprite;
            default: return null;
        }
    }

    private bool IsSolid(Vector3Int pos)
    {
        return GetTileKind(pos) != TileKind.None;
    }

    private bool IsUnsupportedFromBelow(Vector3Int pos)
    {
        Vector3Int below = pos + Vector3Int.down;
        return !IsSolid(below);
    }

    private bool IsSurface(Vector3Int pos)
    {
        if (!IsSolid(pos)) return false;

        foreach (var d in dirs)
        {
            if (!IsSolid(pos + d))
                return true;
        }

        return false;
    }

    private void ClearTileAtCell(Vector3Int cellPos)
    {
        if (pocTilemap != null) pocTilemap.SetTile(cellPos, null);
        if (caveTilemap != null) caveTilemap.SetTile(cellPos, null);
        if (cobaltTilemap != null) cobaltTilemap.SetTile(cellPos, null);

        if (proceduralGen != null)
        {
            proceduralGen.ClearCell(cellPos.x, cellPos.y);
        }
    }

    private bool IsWithinCollapseRadius(Vector3Int origin, Vector3Int pos)
    {
        return Mathf.Abs(pos.x - origin.x) <= collapseRadius &&
               Mathf.Abs(pos.y - origin.y) <= collapseRadius;
    }

    private HashSet<Vector3Int> GetConnectedCluster(Vector3Int start)
    {
        HashSet<Vector3Int> cluster = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        foreach (var d in dirs)
        {
            Vector3Int neighbor = start + d;
            if (IsSolid(neighbor) && IsWithinCollapseRadius(start, neighbor))
            {
                queue.Enqueue(neighbor);
                cluster.Add(neighbor);
            }
        }

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            foreach (var d in dirs)
            {
                Vector3Int next = current + d;
                if (!IsWithinCollapseRadius(start, next)) continue;
                if (!IsSolid(next)) continue;
                if (cluster.Contains(next)) continue;

                cluster.Add(next);
                queue.Enqueue(next);
            }
        }

        return cluster;
    }

    private HashSet<Vector3Int> GetSupportedTiles(HashSet<Vector3Int> cluster)
    {
        HashSet<Vector3Int> supported = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        foreach (var pos in cluster)
        {
            Vector3Int below = pos + Vector3Int.down;

            if (!cluster.Contains(below) && IsSolid(below))
            {
                supported.Add(pos);
                queue.Enqueue(pos);
            }
        }

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            foreach (var d in dirs)
            {
                Vector3Int next = current + d;
                if (!cluster.Contains(next)) continue;
                if (supported.Contains(next)) continue;

                supported.Add(next);
                queue.Enqueue(next);
            }
        }

        return supported;
    }

    private void TriggerSmartCollapse(Vector3Int origin)
    {
        if (fallingTilePrefab == null) return;

        HashSet<Vector3Int> cluster = GetConnectedCluster(origin);
        if (cluster.Count == 0) return;

        HashSet<Vector3Int> supportedTiles = GetSupportedTiles(cluster);

        List<Vector3Int> fallingTiles = new List<Vector3Int>();

        foreach (var pos in cluster)
        {
            if (!supportedTiles.Contains(pos))
            {
                fallingTiles.Add(pos);
            }
        }

        foreach (var pos in fallingTiles)
        {
            TileKind kind = GetTileKind(pos);
            if (kind == TileKind.None) continue;

            Sprite spriteToUse = GetSpriteForTileKind(kind);
            Vector3 worldPos = pocTilemap.GetCellCenterWorld(pos);

            ClearTileAtCell(pos);
            SpawnFallingTile(worldPos, spriteToUse);
        }
    }

    private void SpawnFallingTile(Vector3 worldPos, Sprite spriteToUse)
    {
        GameObject obj = Instantiate(fallingTilePrefab, worldPos, Quaternion.identity);

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null && spriteToUse != null)
        {
            sr.sprite = spriteToUse;
        }

        Destroy(obj, fallingTileLifetime);
    }
}