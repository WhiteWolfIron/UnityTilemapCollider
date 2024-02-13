using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapColliderCreator : MonoBehaviour
{
    private enum EDirection
    {
        None,
        Up,
        Right,
        Down,
        Left,
    }

    [SerializeField] int maxStepsPerPath = 10000;

    private readonly List<Vector2> _polygonPath = new();
    private readonly HashSet<Vector3Int> _used = new();

    private Vector3Int _currentPosition;
    private Tilemap _tilemap;
    private int _pathIndex = 0;

    [ContextMenu(nameof(CreateColliderFromTilemap))]
    public void CreateColliderFromTilemap()
    {
        _polygonPath.Clear();
        _used.Clear();

        _tilemap = GetComponent<Tilemap>();

        if (_tilemap == null)
            return;

        if (TryGetComponent<PolygonCollider2D>(out var polygon) == false)
        {
            polygon = gameObject.AddComponent<PolygonCollider2D>();
        }

        _pathIndex = 0;
        polygon.pathCount = 0;

        foreach (var tilePosition in _tilemap.cellBounds.allPositionsWithin)
        {
            if (_tilemap.HasTile(tilePosition))
            {
                _polygonPath.Clear();

                _currentPosition = tilePosition;

                if (_used.Contains(_currentPosition) || CanBeUsedAsOrigin() == false)
                    continue;

                _used.Add(tilePosition);

                EDirection direction = GetStartDirection();

                if (direction == EDirection.None)
                {
                    Debug.LogWarning($"Single tile. Position: {_currentPosition}.");

                    _polygonPath.Add(_currentPosition + Vector3.zero);
                    _polygonPath.Add(_currentPosition + Vector3.right);
                    _polygonPath.Add(_currentPosition + Vector3.right + Vector3.up);
                    _polygonPath.Add(_currentPosition + Vector3.up);

                    SetColliderPath(polygon);
                    continue;
                }

                _polygonPath.Add(_currentPosition + Vector3.zero);

                if (direction == EDirection.Up)
                {
                    _polygonPath.Add(_currentPosition + Vector3.right);
                }


                Debug.Log($"Start position: {direction}:{_currentPosition}");

                Vector3 startPosition = tilePosition;
                int itteration = 0;

                while (true)
                {
                    itteration++;

                    if (itteration > maxStepsPerPath)
                    {
                        Debug.LogError("Possible infinity loop. Breaking.");
                        break;
                    }

                    direction = Order(direction);

                    if (direction == EDirection.None)
                    {
                        Debug.LogError($"Unknown direction. Breaking loop. Position: {_currentPosition}");
                        break;
                    }

                    _used.Add(_currentPosition);

                    //Debug.Log($"{direction}:{_currentPosition}");

                    if (_currentPosition == startPosition)
                    {
                        if (!HasTileOnUpLeft() || direction == EDirection.Down)
                        {
                            if (direction == EDirection.Left)
                            {
                                _polygonPath.Add(_currentPosition + Vector3.up);
                            }

                            break;
                        }
                    }
                }

                for (int i = 1; i < _polygonPath.Count; i++)
                {
                    if (_polygonPath[i] == _polygonPath[i - 1])
                    {
                        _polygonPath.RemoveAt(i--);

                        Debug.Log($"Removed duplicated position at: {_polygonPath[i]}");
                    }
                }

                SetColliderPath(polygon);

                Debug.Log($"Finished path for index: {polygon.pathCount}. Path count: {_polygonPath.Count}");
            }
        }
    }

    private EDirection GetStartDirection()
    {
        if (HasTileOnRight() || HasTileOnUpRight()) return EDirection.Right;

        if (HasTileOnUp() || HasTileOnUpLeft()) return EDirection.Up;

        return EDirection.None;
    }

    private void SetColliderPath(PolygonCollider2D polygon)
    {
        polygon.pathCount++;
        polygon.SetPath(_pathIndex, _polygonPath.ToArray());

        _pathIndex++;
    }

    private bool CanBeUsedAsOrigin()
    {
        if (HasTileOnRight() && HasTileOnLeft() || HasTileOnUp() && HasTileOnDown())
            return false;

        if (HasTileOnLeft() && HasTileOnDown())
            return false;

        return true;
    }

    private EDirection Order(EDirection direction)
    {
        if (direction == EDirection.Right)
        {
            if (HasTileOnDownRight())
            {
                _polygonPath.Add(_currentPosition + Vector3.right);

                _currentPosition += Vector3Int.right - Vector3Int.up;

                return EDirection.Down;
            }

            if (HasTileOnRight())
            {
                _currentPosition += Vector3Int.right;

                return EDirection.Right;
            }

            if (HasTileOnUpRight())
            {
                _polygonPath.Add(_currentPosition + Vector3.right);
                _polygonPath.Add(_currentPosition + Vector3.up + Vector3.right);

                _currentPosition += Vector3Int.up + Vector3Int.right;

                return EDirection.Right;
            }

            if (HasTileOnUp())
            {
                _polygonPath.Add(_currentPosition + Vector3.right);

                return EDirection.Up;
            }

            if (HasTileOnUpLeft())
            {
                _polygonPath.Add(_currentPosition + Vector3.right);
                _polygonPath.Add(_currentPosition + Vector3.right + Vector3.up);
                _polygonPath.Add(_currentPosition + Vector3.up);

                _currentPosition += Vector3Int.up - Vector3Int.right;

                return EDirection.Up;
            }

            _polygonPath.Add(_currentPosition + Vector3.right);
            _polygonPath.Add(_currentPosition + Vector3.right + Vector3.up);

            return EDirection.Left;
        }

        if (direction == EDirection.Down)
        {
            if (HasTileOnDownLeft())
            {
                _polygonPath.Add(_currentPosition + Vector3.zero);

                _currentPosition += -Vector3Int.right - Vector3Int.up;

                return EDirection.Left;
            }

            if (HasTileOnDown())
            {
                _currentPosition += -Vector3Int.up;

                return EDirection.Down;
            }

            if (HasTileOnDownRight())
            {
                _polygonPath.Add(_currentPosition + Vector3.zero);
                _polygonPath.Add(_currentPosition + Vector3.right);

                _currentPosition += Vector3Int.right - Vector3Int.up;

                return EDirection.Down;
            }

            if (HasTileOnRight())
            {
                _polygonPath.Add(_currentPosition + Vector3.zero);

                _currentPosition += Vector3Int.right;

                return EDirection.Right;
            }

            if (HasTileOnUpRight())
            {
                _polygonPath.Add(_currentPosition + Vector3.zero);
                _polygonPath.Add(_currentPosition + Vector3.right);
                _polygonPath.Add(_currentPosition + Vector3.up + Vector3.right);

                _currentPosition += Vector3Int.up + Vector3Int.right;

                return EDirection.Right;
            }

            _polygonPath.Add(_currentPosition + Vector3.zero);
            _polygonPath.Add(_currentPosition + Vector3.right);

            return EDirection.Up;
        }

        if (direction == EDirection.Left)
        {
            if (HasTileOnUpLeft())
            {
                _polygonPath.Add(_currentPosition + Vector3.up);

                _currentPosition += Vector3Int.up - Vector3Int.right;

                return EDirection.Up;
            }

            if (HasTileOnLeft())
            {
                _currentPosition += -Vector3Int.right;

                return EDirection.Left;
            }

            if (HasTileOnDownLeft())
            {
                _polygonPath.Add(_currentPosition + Vector3.up);
                _polygonPath.Add(_currentPosition + Vector3.zero);

                _currentPosition += -Vector3Int.right - Vector3Int.up;

                return EDirection.Left;
            }

            if (HasTileOnDown())
            {
                _polygonPath.Add(_currentPosition + Vector3.up);

                _currentPosition += -Vector3Int.up;

                return EDirection.Down;
            }

            if (HasTileOnDownRight())
            {
                _polygonPath.Add(_currentPosition + Vector3.up);
                _polygonPath.Add(_currentPosition + Vector3.zero);
                _polygonPath.Add(_currentPosition + Vector3.right);

                _currentPosition += Vector3Int.right - Vector3Int.up;

                return EDirection.Down;
            }

            _polygonPath.Add(_currentPosition + Vector3.up);
            _polygonPath.Add(_currentPosition + Vector3.zero);

            return EDirection.Right;
        }

        if (direction == EDirection.Up)
        {
            if (HasTileOnUpRight())
            {
                _polygonPath.Add(_currentPosition + Vector3.right + Vector3.up);

                _currentPosition += Vector3Int.right + Vector3Int.up;

                return EDirection.Right;
            }

            if (HasTileOnUp())
            {
                _currentPosition += Vector3Int.up;

                return EDirection.Up;
            }

            if (HasTileOnUpLeft())
            {
                _polygonPath.Add(_currentPosition + Vector3.right + Vector3.up);
                _polygonPath.Add(_currentPosition + Vector3.up);

                _currentPosition += Vector3Int.up - Vector3Int.right;

                return EDirection.Up;
            }

            if (HasTileOnLeft())
            {
                _polygonPath.Add(_currentPosition + Vector3.up + Vector3.right);

                _currentPosition += -Vector3Int.right;

                return EDirection.Left;
            }

            if (HasTileOnDownLeft())
            {
                _polygonPath.Add(_currentPosition + Vector3.right + Vector3.up);
                _polygonPath.Add(_currentPosition + Vector3.up);
                _polygonPath.Add(_currentPosition + Vector3.zero);

                _currentPosition += -Vector3Int.right - Vector3Int.up;

                return EDirection.Left;
            }

            _polygonPath.Add(_currentPosition + Vector3.right + Vector3.up);
            _polygonPath.Add(_currentPosition + Vector3.up);

            return EDirection.Down;
        }

        return EDirection.None;
    }

    private bool HasTileOnUp()
    {
        return _tilemap.HasTile(_currentPosition + Vector3Int.up);
    }

    private bool HasTileOnUpRight()
    {
        return _tilemap.HasTile(_currentPosition + Vector3Int.up + Vector3Int.right);
    }

    private bool HasTileOnUpLeft()
    {
        return _tilemap.HasTile(_currentPosition + Vector3Int.up - Vector3Int.right);
    }

    private bool HasTileOnRight()
    {
        return _tilemap.HasTile(_currentPosition + Vector3Int.right);
    }

    private bool HasTileOnDown()
    {
        return _tilemap.HasTile(_currentPosition - Vector3Int.up);
    }

    private bool HasTileOnDownRight()
    {
        return _tilemap.HasTile(_currentPosition - Vector3Int.up + Vector3Int.right);
    }

    private bool HasTileOnDownLeft()
    {
        return _tilemap.HasTile(_currentPosition - Vector3Int.up - Vector3Int.right);
    }

    private bool HasTileOnLeft()
    {
        return _tilemap.HasTile(_currentPosition - Vector3Int.right);
    }
}