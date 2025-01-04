using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard
{
#region Variables
    public int Height { get; }

    public int Width { get; }

    public List<SC_Gem> CurrentMatches { get; private set; } = new();
    readonly SC_Gem[,] _allGems;
#endregion

    public GameBoard(int width, int height)
    {
        Height = height;
        Width = width;
        _allGems = new SC_Gem[Width, Height];
    }

    public void UpdateGems()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                // todo: for now lets ignore nulls
                // todo: in the future, object will be reused
                if (_allGems[x, y])
                    _allGems[x, y].UpdatePosition();
            }
    }
    
    public bool MatchesAt(Vector2Int positionToCheck, SC_Gem gemToCheck)
    {
        if (positionToCheck.x > 1)
        {
            if (_allGems[positionToCheck.x - 1, positionToCheck.y].type == gemToCheck.type &&
                _allGems[positionToCheck.x - 2, positionToCheck.y].type == gemToCheck.type)
                return true;
        }

        if (positionToCheck.y > 1)
        {
            if (_allGems[positionToCheck.x, positionToCheck.y - 1].type == gemToCheck.type &&
                _allGems[positionToCheck.x, positionToCheck.y - 2].type == gemToCheck.type)
                return true;
        }

        return false;
    }

    public void SetGem(int x, int y, SC_Gem gem)
    {
        _allGems[x, y] = gem;
    }

    public SC_Gem GetGem(int x, int y)
    {
       return _allGems[x, y];
    }
    
    public SC_Gem GetGem(Vector2Int coordinates)
    {
        return _allGems[coordinates.x, coordinates.y];
    }
    
    public void FindAllMatches()
    {
        CurrentMatches.Clear();

        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                SC_Gem currentGem = _allGems[x, y];
                if (!currentGem)
                    continue;

                if (x > 0 && x < Width - 1)
                {
                    SC_Gem leftGem = _allGems[x - 1, y];
                    SC_Gem rightGem = _allGems[x + 1, y];
                    
                    //checking no empty spots
                    if (leftGem && rightGem)
                    {
                        //Match
                        if (leftGem.type == currentGem.type && rightGem.type == currentGem.type)
                        {
                            currentGem.isMatch = true;
                            leftGem.isMatch = true;
                            rightGem.isMatch = true;
                            CurrentMatches.Add(currentGem);
                            CurrentMatches.Add(leftGem);
                            CurrentMatches.Add(rightGem);
                        }
                    }
                }

                if (y <= 0 || y >= Height - 1)
                    continue;

                SC_Gem aboveGem = _allGems[x, y - 1];
                SC_Gem bellowGem = _allGems[x, y + 1];

                //checking no empty spots
                if (!aboveGem || !bellowGem)
                    continue;

                //Match
                if (aboveGem.type == currentGem.type && bellowGem.type == currentGem.type)
                {
                    currentGem.isMatch = true;
                    aboveGem.isMatch = true;
                    bellowGem.isMatch = true;
                    CurrentMatches.Add(currentGem);
                    CurrentMatches.Add(aboveGem);
                    CurrentMatches.Add(bellowGem);
                }
            }

        if (CurrentMatches.Count > 0)
            CurrentMatches = CurrentMatches.Distinct().ToList();

        CheckForBombs();
    }
    
    void CheckForBombs()
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (int i = 0; i < CurrentMatches.Count; i++)
        {
            SC_Gem gem = CurrentMatches[i];
            int x = gem.PosIndex.x;
            int y = gem.PosIndex.y;

            if (gem.PosIndex.x > 0)
            {
                if (_allGems[x - 1, y] && _allGems[x - 1, y].type == GlobalEnums.GemType.Bomb)
                    MarkBombArea(new Vector2Int(x - 1, y), _allGems[x - 1, y].blastSize);
            }

            if (gem.PosIndex.x + 1 < Width)
            {
                if (_allGems[x + 1, y] && _allGems[x + 1, y].type == GlobalEnums.GemType.Bomb)
                    MarkBombArea(new Vector2Int(x + 1, y), _allGems[x + 1, y].blastSize);
            }

            if (gem.PosIndex.y > 0)
            {
                if (_allGems[x, y - 1] && _allGems[x, y - 1].type == GlobalEnums.GemType.Bomb)
                    MarkBombArea(new Vector2Int(x, y - 1), _allGems[x, y - 1].blastSize);
            }

            if (gem.PosIndex.y + 1 < Height)
            {
                if (_allGems[x, y + 1] && _allGems[x, y + 1].type == GlobalEnums.GemType.Bomb)
                    MarkBombArea(new Vector2Int(x, y + 1), _allGems[x, y + 1].blastSize);
            }
        }
    }
    
    void MarkBombArea(Vector2Int bombPos, int blastSize)
    {
        for (int x = bombPos.x - blastSize; x <= bombPos.x + blastSize; x++)
            for (int y = bombPos.y - blastSize; y <= bombPos.y + blastSize; y++)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    continue;

                if (!_allGems[x, y])
                    continue;

                _allGems[x, y].isMatch = true;
                CurrentMatches.Add(_allGems[x, y]);
            }

        CurrentMatches = CurrentMatches.Distinct().ToList();
    }
}

