using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using Random = UnityEngine.Random;
using Vector2Int = UnityEngine.Vector2Int;

public class SC_GameLogic : MonoBehaviour
{
#region Properties
    public GlobalEnums.GameState CurrentState
    {
        get => _currentState;
        private set
        {
            Assert.IsFalse(_currentState == value, "Assigning the same state is not allowed.");
            _currentState = value;
        }
    } 
    GlobalEnums.GameState _currentState = GlobalEnums.GameState.Wait;
#endregion

#region Variables
    public Transform gemHolder;
    
    // which pieces are moving
    public static bool[,] Movement;
    
    float _displayScore;
    GameBoard _gameBoard;
    ObjectPool<SC_Gem> _gemPool;
    bool _coroutineRunning;

    [SerializeField]
    SC_Input _scInput;

    // we start in the random spot and then proceed forward in case of a match
    GlobalEnums.PieceType[] _gemTypes;
#endregion
    
#region MonoBehaviour
    void Start()
    {
        Movement = new bool[SC_GameVariables.Instance.rowsSize, SC_GameVariables.Instance.colsSize];
        
        _gemPool = new ObjectPool<SC_Gem>(
            () => Instantiate(SC_GameVariables.Instance.gemPrefab, gemHolder.transform),
            gem => gem.gameObject.SetActive(true),
            gem =>
            {
                gem.posIndex = new Vector2Int(int.MinValue, int.MinValue);
                gem.gameObject.SetActive(false);
                gem.spriteRenderer.color = SC_GameVariables.Instance.defaultGemColor;
                gem.dropDelay = 0f;
            });

        Array types = Enum.GetValues(typeof(GlobalEnums.PieceType));
        _gemTypes = new GlobalEnums.PieceType[types.Length - 1];

        for (int i = 0; i < types.Length - 1; i++)
            _gemTypes[i] = (GlobalEnums.PieceType)types.GetValue(i);
        
        Init();
    }

    bool AnythingMoves()
    {
        // check if stopped moving
        for (int x = 0; x < _gameBoard.Width; x++)
            for (int y = 0; y < _gameBoard.Height; y++)
                if (Movement[x, y])
                    return true;

        return false;
    }

    void Update()
    {
        // if in wait then check movement table 
        if (CurrentState == GlobalEnums.GameState.Wait)
        {
            _gameBoard.UpdateGems();

            if (!_coroutineRunning && !AnythingMoves())
                CurrentState = GlobalEnums.GameState.Move;
        }
        else if (CurrentState == GlobalEnums.GameState.Move)
        {
            if (_scInput.TryGetInput(out Vector2Int current, out Vector2Int other))
            {
                CurrentState = GlobalEnums.GameState.Wait;
                _gameBoard.SwapGems(current, other);
                StartCoroutine(CheckMoveCo(current, other));
            }
        }
    }

    IEnumerator CheckMoveCo(Vector2Int current, Vector2Int other)
    {
        _coroutineRunning = true;
        yield return new WaitForSeconds(.5f);

        _gameBoard.FindAllMatches();
        
        if (_gameBoard.GetMatch(current) == GlobalEnums.MatchType.Nothing 
            && _gameBoard.GetMatch(other) == GlobalEnums.MatchType.Nothing)
        {
            _gameBoard.SwapGems(current, other);
            yield return new WaitForSeconds(.5f);
            _coroutineRunning = false;
        }
        else
        {
            // Put a bomb in place of the piece that create a match
            if (_gameBoard.GetMatch(other) == GlobalEnums.MatchType.FourPiece)
            {
                SC_Gem piece = _gameBoard.GetGem(other);
                piece.spriteRenderer.color = SC_GameVariables.Instance.bombTilts[(int)piece.type];
                    
                _gameBoard.SetType(other, GlobalEnums.PieceType.Bomb);
                _gameBoard.SetMatch(other, GlobalEnums.MatchType.Nothing);
                
            }
            else if (_gameBoard.GetMatch(current) == GlobalEnums.MatchType.FourPiece)
            {
                SC_Gem piece = _gameBoard.GetGem(current);
                piece.spriteRenderer.color = SC_GameVariables.Instance.bombTilts[(int)piece.type];
                
                _gameBoard.SetType(current, GlobalEnums.PieceType.Bomb);
                _gameBoard.SetMatch(current, GlobalEnums.MatchType.Nothing);
            }
            
            DestroyMatches();
        }
    }
#endregion

#region Logic
    void Init()
    {
        _gameBoard = new GameBoard();
        Setup();
    }

    void Setup()
    {
        for (int x = 0; x < _gameBoard.Width; x++)
            for (int y = 0; y < _gameBoard.Height; y++)
            {
                GameObject bgTile = Instantiate(SC_GameVariables.Instance.bgTilePrefabs, new Vector3(x, y), Quaternion.identity);
                bgTile.transform.SetParent(gemHolder);
                bgTile.name = "BG Tile - " + x + ", " + y;

                GlobalEnums.PieceType gem = GetRandomPiece(x, y);
                SpawnGem(x, y, gem);
            }
    }

    /// <summary>
    /// Select a random piece type. It tries to avoid returning a type that would result in a match but there is no guarantee.
    /// </summary>
    GlobalEnums.PieceType GetRandomPiece(int x, int y)
    {
        // first decide if a bomb
        if (Random.Range(0, 100f) < SC_GameVariables.Instance.bombChance)
            return GlobalEnums.PieceType.Bomb;
        
        // if not go for a normal piece
        // pick random
        int length = _gemTypes.Length;
        int rand = Random.Range(0, length);

        for (int i = 0; i < length; i++)
        {
            int index = rand + i;

            if (index >= length)
                index -= length;
            
            // return if it does not match
            if (!_gameBoard.MatchesAt(x, y, _gemTypes[index]))
                return _gemTypes[index];
        }
        
        // if it couldn't find anything return first
        return _gemTypes[rand];
    }
    
    void SpawnGem(int x, int y, GlobalEnums.PieceType type, float delay = 0f)
    {
        SC_Gem gem = _gemPool.Get();

        gem.transform.position = new Vector3(x, y + SC_GameVariables.Instance.dropHeight, 0f);
        gem.name = "Gem - " + x + ", " + y;
        gem.SetupGem(x, y, type, MovementFinished);
        gem.dropDelay = delay;
        _gameBoard.SetGem(x, y, gem);
    }
    
    // if anything is moving then we are in Wait state (waiting for movement to finish)
    // otherwise Move state (player can perform a move)
    static void MovementFinished(int x, int y) => Movement[x, y] = false;
    
    void DestroyMatches()
    {
        for (int x = 0; x < _gameBoard.Width; x++)
            for (int y = 0; y < _gameBoard.Height; y++)
                if (_gameBoard.GetMatch(x, y) != GlobalEnums.MatchType.Nothing)
                {
                    SC_Gem gem = _gameBoard.GetGem(x, y);
                    if (gem)
                    {
                        SC_GameVariables.Instance.score += SC_GameVariables.Instance.scoreValue;
                        DestroyMatchedGemsAt(gem.posIndex);
                    }
                }
        
        StartCoroutine(DecreaseRowCo());
    }

    IEnumerator DecreaseRowCo()
    {
        yield return new WaitForSeconds(.2f);

        int nullCounter = 0;
        for (int x = 0; x < _gameBoard.Width; x++)
        {
            for (int y = 0; y < _gameBoard.Height; y++)
            {
                SC_Gem curGem = _gameBoard.GetGem(x, y);
                if (!curGem)
                {
                    nullCounter++;
                }
                else if (nullCounter > 0)
                {
                    curGem.posIndex = new Vector2Int(curGem.posIndex.x, curGem.posIndex.y - nullCounter);
                    _gameBoard.SetGem(x, y - nullCounter, curGem);
                    _gameBoard.SetGem(x, y, null);
                }
            }
            nullCounter = 0;
        }

        StartCoroutine(FilledBoardCo());
    }
    
    void DestroyMatchedGemsAt(Vector2Int pos)
    {
        SC_Gem curGem = _gameBoard.GetGem(pos);
        if (!curGem)
            return;

        // todo: destroy effect could be pulled too
        Instantiate(SC_GameVariables.Instance.effects[(int)curGem.type], new Vector3(pos.x, pos.y), Quaternion.identity);

        _gemPool.Release(curGem);
        _gameBoard.SetGem(pos, null);
    }

    IEnumerator FilledBoardCo()
    {
        yield return new WaitForSeconds(0.5f);
        RefillBoard();
        yield return new WaitForSeconds(0.5f);
        _gameBoard.FindAllMatches();

        if (_gameBoard.MatchCount > 0)
        {
            yield return new WaitForSeconds(0.5f);
            DestroyMatches();
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            CurrentState = GlobalEnums.GameState.Move;
            _coroutineRunning = false;
        }
    }

    void RefillBoard()
    {
        for (int x = 0; x < _gameBoard.Width; x++)
        {
            float delay = 0f;
            for (int y = 0; y < _gameBoard.Height; y++)
            {
                SC_Gem curGem = _gameBoard.GetGem(x, y);
                if (curGem)
                    continue;

                GlobalEnums.PieceType type = GetRandomPiece(x, y);
                SpawnGem(x, y, type, delay);
                delay += SC_GameVariables.Instance.dropDelay;
            }
        }
    }
#endregion
}
