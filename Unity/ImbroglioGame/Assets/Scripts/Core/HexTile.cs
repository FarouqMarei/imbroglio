using UnityEngine;
using System.Collections.Generic;

namespace ImbroglioCombat.Core
{
    public enum TileType
    {
        Empty,
        Obstacle,
        PowerUp
    }

    public class HexTile : MonoBehaviour
    {
        [Header("Tile Properties")]
        public Vector3Int gridPosition;
        public TileType tileType = TileType.Empty;
        public bool isWalkable = true;

        [Header("Visual References")]
        public SpriteRenderer spriteRenderer;
        public GameObject highlightObject;
        public Color normalColor = Color.white;
        public Color hoverColor = Color.yellow;
        public Color selectedColor = Color.green;
        public Color attackRangeColor = Color.red;
        public Color moveRangeColor = Color.blue;

        [Header("Tile Occupancy")]
        public Unit occupyingUnit;

        private Color currentHighlightColor;

        void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (highlightObject != null)
            {
                highlightObject.SetActive(false);
            }
        }

        public void Initialize(Vector3Int position, TileType type)
        {
            gridPosition = position;
            tileType = type;
            isWalkable = (type != TileType.Obstacle);
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
        }

        public void SetOccupyingUnit(Unit unit)
        {
            occupyingUnit = unit;
        }

        public bool IsOccupied()
        {
            return occupyingUnit != null;
        }

        public void ShowHighlight(HighlightType type)
        {
            if (highlightObject != null)
            {
                highlightObject.SetActive(true);
                SpriteRenderer highlightRenderer = highlightObject.GetComponent<SpriteRenderer>();
                
                if (highlightRenderer != null)
                {
                    switch (type)
                    {
                        case HighlightType.Hover:
                            highlightRenderer.color = hoverColor;
                            break;
                        case HighlightType.Selected:
                            highlightRenderer.color = selectedColor;
                            break;
                        case HighlightType.MoveRange:
                            highlightRenderer.color = moveRangeColor;
                            break;
                        case HighlightType.AttackRange:
                            highlightRenderer.color = attackRangeColor;
                            break;
                    }
                }
            }
        }

        public void HideHighlight()
        {
            if (highlightObject != null)
            {
                highlightObject.SetActive(false);
            }
        }

        public List<HexTile> GetNeighbors(HexGrid grid)
        {
            return grid.GetNeighbors(this);
        }

        // Calculate distance to another tile (hex grid distance)
        public int GetDistanceTo(HexTile other)
        {
            return HexGrid.GetDistance(this.gridPosition, other.gridPosition);
        }
    }

    public enum HighlightType
    {
        Hover,
        Selected,
        MoveRange,
        AttackRange
    }
}

