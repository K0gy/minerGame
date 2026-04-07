using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralGeneration : MonoBehaviour
{
    [Header("World Size")]
    [SerializeField] int width, height;

    [Header("Terrain")]
    [SerializeField] float smoothness;
    [SerializeField] float seed;

    [SerializeField] TileBase pocTile, caveTile;
    [SerializeField] TileBase cobaltTile;
    [SerializeField] TileBase gasTile;

    [SerializeField] Tilemap pocTilemap, caveTilemap;
    [SerializeField] Tilemap cobaltTilemap;
    [SerializeField] Tilemap gasTilemap;

    [Header("Caves")]
    [Range(0, 1)]
    [SerializeField] float modifier;

    [Header("Cobalt Ore")]
    [Range(0, 1)]
    [SerializeField] float cobaltRarity = 0.05f;
    [SerializeField] float cobaltSmoothness = 0.1f;
    [SerializeField] int cobaltMinY = 20;
    [SerializeField] int cobaltMaxY = 9999;

    [Header("Inflammable Gas")]
    [Range(0, 1)]
    [SerializeField] float gasRarity = 0.08f;
    [SerializeField] float gasSmoothness = 0.08f;
    [SerializeField] int gasMinY = 10;
    [SerializeField] int gasMaxY = 9999;
    [SerializeField] bool gasCanSpawnInCaves = true;

    int[,] map;
    bool[,] gasMap;

    void Start()
    {
        Generation();
    }

    void Generation()
    {
        seed = Random.Range(-10000, 10000);

        ClearMaps();

        map = GenerateArray(width, height, true);
        gasMap = GenerateBoolArray(width, height, false);

        map = TerrainGeneration(map);
        map = CobaltGeneration(map);
        gasMap = GasGeneration(map, gasMap);

        RenderMap(
            map,
            gasMap,
            pocTilemap,
            caveTilemap,
            cobaltTilemap,
            gasTilemap,
            pocTile,
            caveTile,
            cobaltTile,
            gasTile
        );
    }

    public int[,] GenerateArray(int width, int height, bool empty)
    {
        int[,] newMap = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                newMap[x, y] = empty ? 0 : 1;
            }
        }

        return newMap;
    }

    public bool[,] GenerateBoolArray(int width, int height, bool defaultValue)
    {
        bool[,] array = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                array[x, y] = defaultValue;
            }
        }

        return array;
    }

    public int[,] TerrainGeneration(int[,] inputMap)
    {
        int perlinHeight;

        for (int x = 0; x < width; x++)
        {
            perlinHeight = Mathf.RoundToInt(Mathf.PerlinNoise(x / smoothness, seed) * height / 2);
            perlinHeight += height / 2;

            for (int y = 0; y < perlinHeight; y++)
            {
                int caveValue = Mathf.RoundToInt(
                    Mathf.PerlinNoise((x * modifier) + seed, (y * modifier) + seed)
                );

                inputMap[x, y] = (caveValue == 1) ? 2 : 1;
            }
        }

        return inputMap;
    }

    public int[,] CobaltGeneration(int[,] inputMap)
    {
        int minY = Mathf.Clamp(cobaltMinY, 0, height - 1);
        int maxY = Mathf.Clamp(cobaltMaxY, 0, height - 1);

        for (int x = 0; x < width; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (inputMap[x, y] == 1)
                {
                    float n = Mathf.PerlinNoise(
                        (x * cobaltSmoothness) + seed * 2f,
                        (y * cobaltSmoothness) + seed * 2f
                    );

                    if (n > 1f - cobaltRarity)
                    {
                        inputMap[x, y] = 3;
                    }
                }
            }
        }

        return inputMap;
    }

    public bool[,] GasGeneration(int[,] inputMap, bool[,] inputGasMap)
    {
        int minY = Mathf.Clamp(gasMinY, 0, height - 1);
        int maxY = Mathf.Clamp(gasMaxY, 0, height - 1);

        for (int x = 0; x < width; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                bool validCell;

                if (gasCanSpawnInCaves)
                    validCell = inputMap[x, y] != 0;
                else
                    validCell = inputMap[x, y] == 1 || inputMap[x, y] == 3;

                if (!validCell) continue;

                float n = Mathf.PerlinNoise(
                    (x * gasSmoothness) + seed * 3f,
                    (y * gasSmoothness) + seed * 3f
                );

                if (n > 1f - gasRarity)
                {
                    inputGasMap[x, y] = true;
                }
            }
        }

        return inputGasMap;
    }

    public void RenderMap(
        int[,] inputMap,
        bool[,] inputGasMap,
        Tilemap pocTileMap,
        Tilemap caveTileMap,
        Tilemap cobaltTileMap,
        Tilemap gasTileMap,
        TileBase pocTilebase,
        TileBase caveTilebase,
        TileBase cobaltTilebase,
        TileBase gasTilebase)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (inputMap[x, y] == 1)
                    pocTileMap.SetTile(pos, pocTilebase);
                else if (inputMap[x, y] == 2)
                    caveTileMap.SetTile(pos, caveTilebase);
                else if (inputMap[x, y] == 3)
                    cobaltTileMap.SetTile(pos, cobaltTilebase);

                if (inputGasMap[x, y])
                    gasTileMap.SetTile(pos, gasTilebase);
            }
        }
    }

    void ClearMaps()
    {
        pocTilemap.ClearAllTiles();
        caveTilemap.ClearAllTiles();
        cobaltTilemap.ClearAllTiles();
        gasTilemap.ClearAllTiles();
    }

    public bool IsCellSolid(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        return map[x, y] != 0;
    }

    public bool HasGas(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        return gasMap != null && gasMap[x, y];
    }

    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return gasTilemap.WorldToCell(worldPosition);
    }

    public void ClearGasCell(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;

        gasMap[x, y] = false;
        gasTilemap.SetTile(new Vector3Int(x, y, 0), null);
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

    public void ExplodeGasAtCell(int centerX, int centerY, int radius, bool destroyTerrain)
    {
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) continue;

                float dist = Vector2.Distance(new Vector2(centerX, centerY), new Vector2(x, y));
                if (dist > radius) continue;

                if (HasGas(x, y))
                {
                    ClearGasCell(x, y);
                }

                if (destroyTerrain && map[x, y] != 0)
                {
                    ClearCell(x, y);
                }
            }
        }
    }
}