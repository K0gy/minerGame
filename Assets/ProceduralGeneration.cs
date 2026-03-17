using UnityEngine;
using UnityEngine.Tilemaps;
public class ProceduralGeneration : MonoBehaviour
{
    [SerializeField] int width, height;
    [SerializeField] float smoothness;
    [SerializeField] float seed;
    [SerializeField] TileBase pocTile,caveTile;
    [SerializeField] Tilemap pocTilemap,caveTilemap;

    [Header("Caves")]
    [Range(0,1)]
    [SerializeField] float modifier;

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
        pocTilemap.ClearAllTiles();
        map = GenerateArray(width, height, true);
        map = TerrainGeneration(map);
        RenderMap(map, pocTilemap, caveTilemap, pocTile, caveTile);
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
                //map[x, y] = 1;
                int caveValue = Mathf.RoundToInt(Mathf.PerlinNoise((x * modifier) + seed, (y * modifier) + seed));
                map[x,y] = (caveValue == 1) ? 2 : 1;
            }
        }
        return map;
    }

    public void RenderMap(int[,] map, Tilemap pocTileMap, Tilemap caveTileMap, TileBase pocTilebase, TileBase caveTilebase) 
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1)
                {
                    pocTileMap.SetTile(new Vector3Int(x, y, 0), pocTilebase);
                }else if(map[x, y] == 2)
                {
                    caveTileMap.SetTile(new Vector3Int(x, y, 0), caveTilebase);
                }
            }
        }
    }

    void clearMap() 
    {
        pocTilemap.ClearAllTiles();
        caveTilemap.ClearAllTiles();
    }

    //AI
    public bool IsCellSolid(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        return map[x, y] != 0; // 1 or 2 = solid, 0 = air
    }

    public void ClearCell(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;

        // Update the int map
        map[x, y] = 0;

        // Clear both tilemaps at this position
        Vector3Int cell = new Vector3Int(x, y, 0);
        pocTilemap.SetTile(cell, null);
        caveTilemap.SetTile(cell, null);
    }
    //AI

}