using UnityEngine;
using UnityEngine.Tilemaps;

public class MiningController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap pocTilemap;
    [SerializeField] private Tilemap caveTilemap;
    [SerializeField] private Tilemap cobaltTilemap;          // NEW
    [SerializeField] private ProceduralGeneration proceduralGen;

    [Header("Mining Settings")]
    [SerializeField] private float miningRadius = 3f; // world units

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // left mouse button
        {
            TryMineAtMouse();
        }
    }

    private void TryMineAtMouse()
    {
        if (_cam == null) return;

        // 1) Mouse position (screen) -> world
        Vector3 mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        // 2) World -> tile cell (Grid coordinates)
        Vector3Int cellPos = pocTilemap.WorldToCell(mouseWorld);

        // 3) Radius check around player
        Vector3 tileWorldCenter = pocTilemap.GetCellCenterWorld(cellPos);
        float dist = Vector3.Distance(transform.position, tileWorldCenter);
        if (dist > miningRadius) return;

        // 4) Check if any solid tile exists at this cell (poc OR cave OR cobalt)
        TileBase pocTile = pocTilemap.GetTile(cellPos);
        TileBase caveTile = caveTilemap.GetTile(cellPos);
        TileBase cobaltTile = cobaltTilemap != null ? cobaltTilemap.GetTile(cellPos) : null;

        if (pocTile == null && caveTile == null && cobaltTile == null) return; // air, nothing to mine

        // 5) Only allow mining if tile is on the surface (touches air)
        if (!IsSurface(cellPos)) return;

        // 6) Clear the tile(s)
        pocTilemap.SetTile(cellPos, null);
        caveTilemap.SetTile(cellPos, null);
        if (cobaltTilemap != null)
            cobaltTilemap.SetTile(cellPos, null);

        // 7) Keep the ProceduralGeneration map in sync
        if (proceduralGen != null)
        {
            proceduralGen.ClearCell(cellPos.x, cellPos.y);
        }
    }

    // Surface = solid tile with at least one neighboring air tile (4 directions)
    private bool IsSurface(Vector3Int pos)
    {
        // Must be solid at this position (poc, cave, or cobalt)
        TileBase poc = pocTilemap.GetTile(pos);
        TileBase cave = caveTilemap.GetTile(pos);
        TileBase cobalt = cobaltTilemap != null ? cobaltTilemap.GetTile(pos) : null;

        if (poc == null && cave == null && cobalt == null) return false;

        Vector3Int[] dirs =
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        foreach (var d in dirs)
        {
            Vector3Int n = pos + d;
            TileBase nPoc = pocTilemap.GetTile(n);
            TileBase nCave = caveTilemap.GetTile(n);
            TileBase nCobalt = cobaltTilemap != null ? cobaltTilemap.GetTile(n) : null;

            if (nPoc == null && nCave == null && nCobalt == null)
            {
                // Neighbor is air => this is a surface tile
                return true;
            }
        }

        return false;
    }
}