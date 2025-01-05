using UnityEngine;

public class GameBoard
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

    readonly SC_Gem[,] _allGems;
    readonly bool[,] _matches;
#endregion

    public GameBoard(int width, int height)
    {
        Height = height;
        Width = width;
        _allGems = new SC_Gem[Width, Height];
        _matches = new bool[Width, Height];
    }

#region Methods
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
    
    public bool MatchesAt(int x, int y, GlobalEnums.GemType gemType)
    {
        if (x > 1)
            if (_allGems[x - 1, y].type == gemType && _allGems[x - 2, y].type == gemType)
                return true;

        if (y > 1)
            if (_allGems[x, y - 1].type == gemType && _allGems[x, y - 2].type == gemType)
                return true;

        return false;
    }
    
    public bool MatchesAt(Vector2Int positionToCheck, GlobalEnums.GemType gemType)
    {
        int x = positionToCheck.x;
        int y = positionToCheck.y;
        
        if (x > 1)
            if (_allGems[x - 1, y].type == gemType && _allGems[x - 2, y].type == gemType)
                return true;

        if (y > 1)
            if (_allGems[x, y - 1].type == gemType && _allGems[x, y - 2].type == gemType)
                return true;

        return false;
    }

    // todo: consider renaming coordinates to pos
    public bool GetMatch(Vector2Int coordinates) => _matches[coordinates.x, coordinates.y];
    
    public bool GetMatch(int x, int y) => _matches[x, y];

    public void SetGem(int x, int y, SC_Gem gem) => _allGems[x, y] = gem;
    
    public void SetGem(Vector2Int coordinates, SC_Gem gem) => _allGems[coordinates.x, coordinates.y] = gem;

    public SC_Gem GetGem(int x, int y) => _allGems[x, y];

    public SC_Gem GetGem(Vector2Int coordinates) => _allGems[coordinates.x, coordinates.y];

    public void SwapGems(Vector2Int pos1, Vector2Int pos2)
    {
        SC_Gem currentGem = _allGems[pos1.x, pos1.y];
        SC_Gem otherGem = _allGems[pos2.x, pos2.y];
        
        // set positions
        (currentGem.posIndex, otherGem.posIndex) = (otherGem.posIndex, currentGem.posIndex);
                
        // set (swap) references
        _allGems[pos1.x, pos1.y] = otherGem;
        _allGems[pos2.x, pos2.y] = currentGem;
    }
    
    public void FindAllMatches()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                _matches[x, y] = false;
        
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
                            _matches[x, y] = true;
                            _matches[x - 1, y] = true;
                            _matches[x + 1, y] = true;
                        }
                    }
                }

                if (y <= 0 || y >= Height - 1)
                    continue;

                SC_Gem aboveGem = _allGems[x, y - 1];
                SC_Gem belowGem = _allGems[x, y + 1];

                //checking no empty spots
                if (!aboveGem || !belowGem)
                    continue;

                //Match
                if (aboveGem.type == currentGem.type && belowGem.type == currentGem.type)
                {
                    _matches[x, y] = true;
                    _matches[x, y - 1] = true;
                    _matches[x, y + 1] = true;
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
                if (x > 0) // todo: probably just x > 0
                {
                    SC_Gem otherGem = _allGems[x - 1, y];
                    
                    if (otherGem && otherGem.type == GlobalEnums.GemType.Bomb)
                        MarkBombArea(x - 1, y);
                }

                if (x + 1 < Width)
                {
                    SC_Gem otherGem = _allGems[x + 1, y];
                    
                    if (otherGem && otherGem.type == GlobalEnums.GemType.Bomb)
                        MarkBombArea(x + 1, y);
                }

                if (y > 0)
                {
                    SC_Gem otherGem = _allGems[x, y - 1];
                    
                    if (otherGem && otherGem.type == GlobalEnums.GemType.Bomb)
                        MarkBombArea(x, y - 1);
                }

                if (y + 1 < Height)
                {
                    SC_Gem otherGem = _allGems[x, y + 1];
                    
                    if (otherGem && otherGem.type == GlobalEnums.GemType.Bomb)
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

                if (!_allGems[x, y])
                    continue;

                _matches[x, y] = true;
            }
    }
#endregion
}

