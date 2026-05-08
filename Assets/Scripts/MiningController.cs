using UnityEngine;
using UnityEngine.Tilemaps;

public class MiningController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap pocTilemap;
    [SerializeField] private Tilemap caveTilemap;
    [SerializeField] private Tilemap cobaltTilemap;
    [SerializeField] private ProceduralGeneration proceduralGen;
    [SerializeField] private OreCounterUI oreCounterUI;

    [Header("Mining Settings")]
    [SerializeField] private float miningRadius = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource miningAudio;  // glisse ton AudioSource ici    

    [Header("Collapse Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float collapseChance = 0.3f;
    [SerializeField] private int collapseRadius = 4;
    [SerializeField] private bool collapseOnlyAbove = true;

    [Header("Falling Tile")]
    [SerializeField] private GameObject fallingTilePrefab;
    [SerializeField] private float fallingTileLifetime = 10f;

    [Header("Falling Tile Sprites")]
    [SerializeField] private Sprite pocFallingSprite;
    [SerializeField] private Sprite caveFallingSprite;
    [SerializeField] private Sprite cobaltFallingSprite;

    private Camera _cam;
    private int cobaltMinedCount = 0;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Start()
    {
        UpdateOreUI();
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

        if (miningAudio != null)
                miningAudio.Play();

        if (tileKind == TileKind.Cobalt)
        {
            
            cobaltMinedCount++;
            UpdateOreUI();

        
        }

        ClearTileAtCell(cellPos);

        if (unsupportedFromBelow && Random.value < collapseChance)
        {
            TriggerCollapse(cellPos);
        }
    }

    private enum TileKind
    {
        None,
        Poc,
        Cave,
        Cobalt
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

    private bool IsUnsupportedFromBelow(Vector3Int pos)
    {
        Vector3Int below = pos + Vector3Int.down;
        return GetTileKind(below) == TileKind.None;
    }

    private void TriggerCollapse(Vector3Int origin)
    {
        if (fallingTilePrefab == null) return;

        for (int dx = -collapseRadius; dx <= collapseRadius; dx++)
        {
            for (int dy = -collapseRadius; dy <= collapseRadius; dy++)
            {
                Vector3Int pos = new Vector3Int(origin.x + dx, origin.y + dy, 0);

                if (collapseOnlyAbove && pos.y < origin.y)
                    continue;

                TileKind kind = GetTileKind(pos);
                if (kind == TileKind.None) continue;

                Sprite spriteToUse = GetSpriteForTileKind(kind);
                Vector3 worldPos = pocTilemap.GetCellCenterWorld(pos);

                ClearTileAtCell(pos);
                SpawnFallingTile(worldPos, spriteToUse);
            }
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

    private bool IsSurface(Vector3Int pos)
    {
        if (GetTileKind(pos) == TileKind.None) return false;

        Vector3Int[] dirs =
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        foreach (var d in dirs)
        {
            if (GetTileKind(pos + d) == TileKind.None)
                return true;
        }

        return false;
    }

    private void UpdateOreUI()
    {
        if (oreCounterUI != null)
        {
            oreCounterUI.SetCobaltCount(cobaltMinedCount);
        }
    }
    public int GetCobaltCount()
    {
    return cobaltMinedCount;
    }
}