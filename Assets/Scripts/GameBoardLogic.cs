using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoardLogic
{
#region Properties
    public int MatchCount 
    { 
        get
        {
            int count = 0;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    if (_matches[x, y])
                        count++;
                }

            return count;
        }
    }
#endregion
    
#region Variables
    public int Height { get; }

    public int Width { get; }

    readonly GlobalEnums.GemType[,] _allGems;
    readonly bool[,] _matches;
#endregion

    public GameBoardLogic(int width, int height)
    {
        Height = height;
        Width = width;
        _allGems = new GlobalEnums.GemType[Width, Height];
        _matches = new bool[Width, Height];
    }
    
#region Methods
    public bool MatchesAt(Vector2Int coordinates, GlobalEnums.GemType type)
    {
        int x = coordinates.x;
        int y = coordinates.y;
        
        if (x > 1)
            if (_allGems[x - 1, y] == type && _allGems[x - 2, y] == type)
                return true;

        if (y > 1)
            if (_allGems[x, y - 1] == type && _allGems[x, y - 2] == type)
                return true;

        return false;
    }

    public bool GetMatch(Vector2Int coordinates) => _matches[coordinates.x, coordinates.y];
    
    public bool GetMatch(int x, int y) => _matches[x, y];

    public void SetGem(int x, int y, GlobalEnums.GemType type) => _allGems[x, y] = type;

    public GlobalEnums.GemType GetGem(int x, int y) => _allGems[x, y];

    public GlobalEnums.GemType GetGem(Vector2Int coordinates) => _allGems[coordinates.x, coordinates.y];

    public void FindAllMatches()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                _matches[x, y] = false;
        
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                GlobalEnums.GemType currentGem = _allGems[x, y];

                if (x > 0 && x < Width - 1)
                {
                    GlobalEnums.GemType leftGem = _allGems[x - 1, y];
                    GlobalEnums.GemType rightGem = _allGems[x + 1, y];
                    
                    if (leftGem == currentGem && rightGem == currentGem)
                    {
                        _matches[x, y] = true;     // current
                        _matches[x - 1, y] = true; // left
                        _matches[x + 1, y] = true; // right
                    }
                }

                if (y <= 0 || y >= Height - 1)
                    continue;

                GlobalEnums.GemType aboveGem = _allGems[x, y - 1];
                GlobalEnums.GemType belowGem = _allGems[x, y + 1];

                //Match
                if (aboveGem == currentGem && belowGem == currentGem)
                {
                    _matches[x, y] = true;     // current
                    _matches[x, y - 1] = true; // above
                    _matches[x, y + 1] = true; // below
                }
            }

        CheckForBombs();
    }
    
    void CheckForBombs()
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                if (x > 0)
                {
                    if (_allGems[x - 1, y] == GlobalEnums.GemType.Bomb)
                        MarkBombArea(x - 1, y);
                }

                if (x + 1 < Width)
                {
                    if (_allGems[x + 1, y] == GlobalEnums.GemType.Bomb)
                        MarkBombArea(x + 1, y);
                }

                if (y > 0)
                {
                    if (_allGems[x, y - 1] == GlobalEnums.GemType.Bomb)
                        MarkBombArea(x, y - 1);
                }

                if (y + 1 < Height)
                {
                    if (_allGems[x, y + 1] == GlobalEnums.GemType.Bomb)
                        MarkBombArea(x, y + 1);
                }
            }
    }
    
    void MarkBombArea(int posX, int posY)
    {
        int blastSize = SC_GameVariables.Instance.blastSize;
        
        for (int x = posX - blastSize; x <= posX + blastSize; x++)
            for (int y = posY - blastSize; y <= posY + blastSize; y++)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    continue;

                _matches[x, y] = true;
            }
    }
#endregion
}