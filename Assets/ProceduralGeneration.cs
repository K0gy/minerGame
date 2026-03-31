using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralGeneration : MonoBehaviour
{
    [SerializeField] int width, height;
    [SerializeField] float smoothness;
    [SerializeField] float seed;

    [SerializeField] TileBase pocTile, caveTile;
    [SerializeField] TileBase cobaltTile;

    [SerializeField] Tilemap pocTilemap, caveTilemap;
    [SerializeField] Tilemap cobaltTilemap;

    [Header("Caves")]
    [Range(0, 1)]
    [SerializeField] float modifier;

    [Header("Cobalt Ore")]
    [Range(0, 1)]
    [SerializeField] float cobaltRarity = 0.05f;
    [SerializeField] float cobaltSmoothness = 0.1f;

    [Tooltip("Tiles above this Y are too shallow for cobalt (0 = very bottom row).")]
    [SerializeField] int cobaltMinY = 20;   // no cobalt in rows < 20 (top 20 rows empty of cobalt)

    [Tooltip("Optional max depth; set to a large number to ignore.")]
    [SerializeField] int cobaltMaxY = 9999; // you can clamp depth if you want

    int[,] map;

    void Start()
    {
        Generation();
    }

    void Update()
    {

    }

    void Generation()
    {
        seed = Random.Range(-10000, 10000);

        clearMap();

        map = GenerateArray(width, height, true);
        map = TerrainGeneration(map);
        map = CobaltGeneration(map);

        RenderMap(map,
                  pocTilemap,
                  caveTilemap,
                  cobaltTilemap,
                  pocTile,
                  caveTile,
                  cobaltTile);
    }

    public int[,] GenerateArray(int width, int height, bool empty)
    {
        int[,] map = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = (empty) ? 0 : 1;
            }
        }
        return map;
    }

    public int[,] TerrainGeneration(int[,] map)
    {
        int perlinHeight;
        for (int x = 0; x < width; x++)
        {
            perlinHeight = Mathf.RoundToInt(Mathf.PerlinNoise(x / smoothness, seed) * height / 2);
            perlinHeight += height / 2;

            for (int y = 0; y < perlinHeight; y++)
            {
                int caveValue = Mathf.RoundToInt(
                    Mathf.PerlinNoise((x * modifier) + seed,
                                      (y * modifier) + seed));

                map[x, y] = (caveValue == 1) ? 2 : 1; // 1 = POC, 2 = cave wall
            }
        }
        return map;
    }

    // 3 = cobalt ore, only where map was 1 (POC), and within depth range
    public int[,] CobaltGeneration(int[,] map)
    {
        int minY = Mathf.Clamp(cobaltMinY, 0, height - 1);
        int maxY = Mathf.Clamp(cobaltMaxY, 0, height - 1);

        for (int x = 0; x < width; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (y < 0 || y >= height) continue;

                if (map[x, y] == 1) // only inside POC
                {
                    float n = Mathf.PerlinNoise(
                        (x * cobaltSmoothness) + seed * 2f,
                        (y * cobaltSmoothness) + seed * 2f);

                    if (n > 1f - cobaltRarity)
                    {
                        map[x, y] = 3;
                    }
                }
            }
        }
        return map;
    }

    public void RenderMap(int[,] map,
                          Tilemap pocTileMap,
                          Tilemap caveTileMap,
                          Tilemap cobaltTileMap,
                          TileBase pocTilebase,
                          TileBase caveTilebase,
                          TileBase cobaltTilebase)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (map[x, y] == 1)
                {
                    pocTileMap.SetTile(pos, pocTilebase);
                }
                else if (map[x, y] == 2)
                {
                    caveTileMap.SetTile(pos, caveTilebase);
                }
                else if (map[x, y] == 3)
                {
                    cobaltTileMap.SetTile(pos, cobaltTilebase);
                }
            }
        }
    }

    void clearMap()
    {
        pocTilemap.ClearAllTiles();
        caveTilemap.ClearAllTiles();
        cobaltTilemap.ClearAllTiles();
    }

    public bool IsCellSolid(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        return map[x, y] != 0; // 1,2,3 = solid, 0 = air
    }

    public void ClearCell(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;

        map[x, y] = 0;

        Vector3Int cell = new Vector3Int(x, y, 0);
        pocTilemap.SetTile(cell, null);
        caveTilemap.SetTile(cell, null);
        cobaltTilemap.SetTile(cell, null);
    }
}