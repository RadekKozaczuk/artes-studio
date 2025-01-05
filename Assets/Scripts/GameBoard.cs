using System;
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
                    if (_matches[x, y] != GlobalEnums.MatchType.Nothing)
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
    readonly GlobalEnums.MatchType[,] _matches;
#endregion

    public GameBoard()
    {
        Height = SC_GameVariables.Instance.rowsSize;
        Width = SC_GameVariables.Instance.colsSize;
        _allGems = new SC_Gem[Width, Height];
        _matches = new GlobalEnums.MatchType[Width, Height];
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
    
    public GlobalEnums.MatchType GetMatch(Vector2Int pos) => _matches[pos.x, pos.y];
    
    public GlobalEnums.MatchType GetMatch(int x, int y) => _matches[x, y];
    
    public void SetMatch(Vector2Int pos, GlobalEnums.MatchType type) => _matches[pos.x, pos.y] = type;

    public void SetGem(int x, int y, SC_Gem gem) => _allGems[x, y] = gem;
    
    public void SetGem(Vector2Int pos, SC_Gem gem) => _allGems[pos.x, pos.y] = gem;
    
    public void SetType(Vector2Int pos, GlobalEnums.GemType type)
    {
        SC_Gem gem = _allGems[pos.x, pos.y];
        gem.type = type;
        gem.spriteRenderer.sprite = SC_GameVariables.Instance.gemSprites[(int)type];
    }

    public SC_Gem GetGem(int x, int y) => _allGems[x, y];
    
    public SC_Gem GetGem(Vector2Int pos) => _allGems[pos.x, pos.y];

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
    
    /// <summary>
    /// Marks matched elements as either <see cref="GlobalEnums.MatchType.ThreePiece"/>
    /// or <see cref="GlobalEnums.MatchType.FourPiece"/>.
    /// </summary>
    public void FindAllMatches()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                _matches[x, y] = GlobalEnums.MatchType.Nothing;
        
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
                            _matches[x, y] = GlobalEnums.MatchType.ThreePiece;
                            _matches[x - 1, y] = GlobalEnums.MatchType.ThreePiece;
                            _matches[x + 1, y] = GlobalEnums.MatchType.ThreePiece;
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
                    _matches[x, y] = GlobalEnums.MatchType.ThreePiece;
                    _matches[x, y - 1] = GlobalEnums.MatchType.ThreePiece;
                    _matches[x, y + 1] = GlobalEnums.MatchType.ThreePiece;
                }
            }

        // go vertically
        for (int x = 0; x < Width; x++)
            for (int y = 3; y < Height; y++)
            {
                GlobalEnums.MatchType threeAbove = _matches[x, y - 3];
                GlobalEnums.MatchType twoAbove = _matches[x, y - 2];
                GlobalEnums.MatchType oneAbove = _matches[x, y - 1];
                GlobalEnums.MatchType current = _matches[x, y];
                
                if (current == GlobalEnums.MatchType.ThreePiece 
                    && oneAbove == GlobalEnums.MatchType.ThreePiece
                    && twoAbove == GlobalEnums.MatchType.ThreePiece
                    && threeAbove == GlobalEnums.MatchType.ThreePiece)
                {
                    _matches[x, y] = GlobalEnums.MatchType.FourPiece;
                    _matches[x, y - 1] = GlobalEnums.MatchType.FourPiece;
                    _matches[x, y - 2] = GlobalEnums.MatchType.FourPiece;
                    _matches[x, y - 3] = GlobalEnums.MatchType.FourPiece;
                }
            }

        // go horizontally
        for (int y = 0; y < Height; y++)
            for (int x = 3; x < Width; x++)
            {
                GlobalEnums.MatchType threeLeft = _matches[x - 3, y];
                GlobalEnums.MatchType twoLeft = _matches[x - 2, y];
                GlobalEnums.MatchType oneLeft = _matches[x - 1, y];
                GlobalEnums.MatchType current = _matches[x, y];
                
                if (current == GlobalEnums.MatchType.ThreePiece 
                    && oneLeft == GlobalEnums.MatchType.ThreePiece
                    && twoLeft == GlobalEnums.MatchType.ThreePiece
                    && threeLeft == GlobalEnums.MatchType.ThreePiece)
                {
                    _matches[x, y] = GlobalEnums.MatchType.FourPiece;
                    _matches[x - 1, y] = GlobalEnums.MatchType.FourPiece;
                    _matches[x - 2, y] = GlobalEnums.MatchType.FourPiece;
                    _matches[x - 3, y] = GlobalEnums.MatchType.FourPiece;
                }
            }

        CheckForBombs();
    }
    
    /// <summary>
    /// Only <see cref="GlobalEnums.MatchType.Nothing"/> can be marked as <see cref="GlobalEnums.MatchType.Bomb"/>.<br/>
    /// Piece marked as <see cref="GlobalEnums.MatchType.ThreePiece"/>
    /// or <see cref="GlobalEnums.MatchType.FourPiece"/> are ignored.
    /// </summary>
    void CheckForBombs()
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                if (x > 0)
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
    
    /// <summary>
    /// Set corresponding <see cref="_matches"/> value to <see cref="GlobalEnums.MatchType.Bomb"/> if within the bomb range.
    /// </summary>
    void MarkBombArea(int posX, int posY)
    {
        int blastSize = SC_GameVariables.Instance.blastSize;
        
        for (int x = posX - blastSize; x <= posX + blastSize; x++)
            for (int y = posY - blastSize; y <= posY + blastSize; y++)
            {
                // skip if outside of map
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    continue;

                // skip if empty
                if (!_allGems[x, y])
                    continue;

                // skip if distance is greater than blastSize
                if (Math.Abs(posX - x) + Math.Abs(posY - y) > blastSize)
                    continue;

                // skip already matched
                if (_matches[x, y] == GlobalEnums.MatchType.Nothing)
                    _matches[x, y] = GlobalEnums.MatchType.Bomb;
            }
    }
#endregion
}

