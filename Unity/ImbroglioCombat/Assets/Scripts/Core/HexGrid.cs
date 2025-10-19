using UnityEngine;
using System.Collections.Generic;

namespace ImbroglioCombat.Core
{
    public class HexGrid : MonoBehaviour
    {
        [Header("Grid Settings")]
        public int gridWidth = 10;
        public int gridHeight = 10;
        public float hexSize = 1.0f;

        [Header("Prefabs")]
        public GameObject hexTilePrefab;
        public GameObject obstacleTilePrefab;
        public GameObject powerUpTilePrefab;

        [Header("Grid Data")]
        private Dictionary<Vector3Int, HexTile> tiles = new Dictionary<Vector3Int, HexTile>();
        
        // Hex grid directions (flat-top hexagons)
        private static readonly Vector3Int[] hexDirections = new Vector3Int[]
        {
            new Vector3Int(1, 0, -1),  // East
            new Vector3Int(1, -1, 0),  // Southeast
            new Vector3Int(0, -1, 1),  // Southwest
            new Vector3Int(-1, 0, 1),  // West
            new Vector3Int(-1, 1, 0),  // Northwest
            new Vector3Int(0, 1, -1)   // Northeast
        };

        void Start()
        {
            GenerateGrid();
        }

        public void GenerateGrid()
        {
            ClearGrid();

            for (int q = 0; q < gridWidth; q++)
            {
                for (int r = 0; r < gridHeight; r++)
                {
                    int s = -q - r;
                    Vector3Int hexCoord = new Vector3Int(q, r, s);
                    
                    // Determine tile type (you can customize this logic)
                    TileType tileType = DetermineTileType(q, r);
                    
                    CreateTile(hexCoord, tileType);
                }
            }
        }

        private TileType DetermineTileType(int q, int r)
        {
            // Example: Create some random obstacles
            float randomValue = Random.value;
            
            if (randomValue < 0.15f)
            {
                return TileType.Obstacle;
            }
            else if (randomValue > 0.95f)
            {
                return TileType.PowerUp;
            }
            
            return TileType.Empty;
        }

        private void CreateTile(Vector3Int hexCoord, TileType tileType)
        {
            Vector3 worldPos = HexToWorldPosition(hexCoord);
            
            GameObject tilePrefab = hexTilePrefab;
            if (tileType == TileType.Obstacle && obstacleTilePrefab != null)
            {
                tilePrefab = obstacleTilePrefab;
            }
            else if (tileType == TileType.PowerUp && powerUpTilePrefab != null)
            {
                tilePrefab = powerUpTilePrefab;
            }

            if (tilePrefab != null)
            {
                GameObject tileObj = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                tileObj.name = $"Hex_{hexCoord.x}_{hexCoord.y}_{hexCoord.z}";
                
                HexTile tile = tileObj.GetComponent<HexTile>();
                if (tile == null)
                {
                    tile = tileObj.AddComponent<HexTile>();
                }
                
                tile.Initialize(hexCoord, tileType);
                tiles[hexCoord] = tile;
            }
        }

        public void ClearGrid()
        {
            foreach (var tile in tiles.Values)
            {
                if (tile != null)
                {
                    Destroy(tile.gameObject);
                }
            }
            tiles.Clear();
        }

        public HexTile GetTile(Vector3Int hexCoord)
        {
            tiles.TryGetValue(hexCoord, out HexTile tile);
            return tile;
        }

        public HexTile GetTileAtWorldPosition(Vector3 worldPos)
        {
            Vector3Int hexCoord = WorldToHexPosition(worldPos);
            return GetTile(hexCoord);
        }

        public List<HexTile> GetNeighbors(HexTile tile)
        {
            List<HexTile> neighbors = new List<HexTile>();
            
            foreach (Vector3Int direction in hexDirections)
            {
                Vector3Int neighborCoord = tile.gridPosition + direction;
                HexTile neighbor = GetTile(neighborCoord);
                
                if (neighbor != null)
                {
                    neighbors.Add(neighbor);
                }
            }
            
            return neighbors;
        }

        public List<HexTile> GetTilesInRange(HexTile centerTile, int range, bool walkableOnly = false)
        {
            List<HexTile> tilesInRange = new List<HexTile>();
            
            foreach (var tile in tiles.Values)
            {
                int distance = GetDistance(centerTile.gridPosition, tile.gridPosition);
                
                if (distance <= range && distance > 0)
                {
                    if (!walkableOnly || (tile.isWalkable && !tile.IsOccupied()))
                    {
                        tilesInRange.Add(tile);
                    }
                }
            }
            
            return tilesInRange;
        }

        // Convert hex coordinates to world position
        public Vector3 HexToWorldPosition(Vector3Int hexCoord)
        {
            float x = hexSize * (3f/2f * hexCoord.x);
            float y = hexSize * (Mathf.Sqrt(3f)/2f * hexCoord.x + Mathf.Sqrt(3f) * hexCoord.y);
            
            return new Vector3(x, y, 0);
        }

        // Convert world position to hex coordinates
        public Vector3Int WorldToHexPosition(Vector3 worldPos)
        {
            float q = (2f/3f * worldPos.x) / hexSize;
            float r = (-1f/3f * worldPos.x + Mathf.Sqrt(3f)/3f * worldPos.y) / hexSize;
            
            return HexRound(q, r);
        }

        private Vector3Int HexRound(float q, float r)
        {
            float s = -q - r;
            
            int roundedQ = Mathf.RoundToInt(q);
            int roundedR = Mathf.RoundToInt(r);
            int roundedS = Mathf.RoundToInt(s);
            
            float qDiff = Mathf.Abs(roundedQ - q);
            float rDiff = Mathf.Abs(roundedR - r);
            float sDiff = Mathf.Abs(roundedS - s);
            
            if (qDiff > rDiff && qDiff > sDiff)
            {
                roundedQ = -roundedR - roundedS;
            }
            else if (rDiff > sDiff)
            {
                roundedR = -roundedQ - roundedS;
            }
            
            return new Vector3Int(roundedQ, roundedR, -roundedQ - roundedR);
        }

        public static int GetDistance(Vector3Int a, Vector3Int b)
        {
            return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2;
        }

        public Dictionary<Vector3Int, HexTile> GetAllTiles()
        {
            return tiles;
        }
    }
}

