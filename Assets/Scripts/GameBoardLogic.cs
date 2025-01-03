using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoardLogic
{
#region Variables
    public int Height { get; }

    public int Width { get; }

    public List<GlobalEnums.GemType> CurrentMatches { get; private set; } = new();
    readonly GlobalEnums.GemType[,] _gems;
    readonly bool[,] _isMatch;
#endregion

    public GameBoardLogic(int width, int height)
    {
        Height = height;
        Width = width;
        _gems = new GlobalEnums.GemType[Width, Height];
    }
    
    public bool MatchesAt(Vector2Int positionToCheck, GlobalEnums.GemType gemToCheck)
    {
        int x = positionToCheck.x;
        int y = positionToCheck.y;
        
        if (x > 1)
        {
            if (_gems[x - 1, y] == gemToCheck && _gems[x - 2, y] == gemToCheck)
                return true;
        }

        if (y > 1)
        {
            if (_gems[x, y - 1] == gemToCheck && _gems[x, y - 2] == gemToCheck)
                return true;
        }

        return false;
    }
    
    public void SetGem(int x, int y, GlobalEnums.GemType gem)
    {
        _gems[x, y] = gem;
    }

    public GlobalEnums.GemType GetGem(int x, int y)
    {
        return _gems[x, y];
    }

    public void FindAllMatches_V2()
    {
        CurrentMatches.Clear();

        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                GlobalEnums.GemType currentGem = _gems[x, y];
                //if (currentGem == null)  // todo: will never be null because this method should not run when the pieces are animating
                    //continue;

                if (x > 0 && x < Width - 1)
                {
                    GlobalEnums.GemType leftGem = _gems[x - 1, y];
                    GlobalEnums.GemType rightGem = _gems[x + 1, y];
                    
                    //checking no empty spots
                    //if (leftGem != null && rightGem != null) // todo: will never be null because we do not try to find matches if the puzzle is animating
                    //{
                        //Match
                        if (leftGem == currentGem && rightGem == currentGem)
                        {
                            _isMatch[x, y] = true; // current
                            _isMatch[x - 1, y] = true; // left
                            _isMatch[x + 1, y] = true; // right
                            CurrentMatches.Add(currentGem);
                            CurrentMatches.Add(leftGem);
                            CurrentMatches.Add(rightGem);
                        }
                    //}
                }

                if (y <= 0 || y >= Height - 1)
                    continue;

                GlobalEnums.GemType aboveGem = _gems[x, y - 1];
                GlobalEnums.GemType bellowGem = _gems[x, y + 1];

                //checking no empty spots
                //if (aboveGem == null || bellowGem == null) // todo: will never be null because we do not try to find matches if the puzzle is animating
                //    continue;

                //Match
                if (aboveGem == currentGem && bellowGem == currentGem)
                {
                    _isMatch[x, y] = true;     // current
                    _isMatch[x, y - 1] = true; // above
                    _isMatch[x, y + 1] = true; // below
                    CurrentMatches.Add(currentGem);
                    CurrentMatches.Add(aboveGem);
                    CurrentMatches.Add(bellowGem);
                }
            }

        if (CurrentMatches.Count > 0)
            CurrentMatches = CurrentMatches.Distinct().ToList(); // todo: optimize to use less aloc

        //CheckForBombs(); todo: add this method
    }
}