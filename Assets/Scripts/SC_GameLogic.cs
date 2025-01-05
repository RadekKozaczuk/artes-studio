using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
using Vector2Int = UnityEngine.Vector2Int;

public class SC_GameLogic : MonoBehaviour
{
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

    const int Score = 0;
    float _displayScore;

    public TextMeshProUGUI score;
    public Transform gemHolder;
    
    GameBoard _gameBoard;

    [SerializeField]
    SC_Input _scInput;

    // which pieces are moving
    public static readonly bool[,] Movement = new bool[7, 7];
    
#region MonoBehaviour
    void Start()
    {
        score.text = Score.ToString("0");
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
        // todo: this should not be updated every frame, should on change 
        _displayScore = Mathf.Lerp(_displayScore, SC_GameVariables.Instance.Score, SC_GameVariables.Instance.scoreSpeed * Time.deltaTime);
        score.text = _displayScore.ToString("0");

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

    static bool _coroutineRunning;
    
    IEnumerator CheckMoveCo(Vector2Int current, Vector2Int other)
    {
        _coroutineRunning = true;
        yield return new WaitForSeconds(.5f);
        _gameBoard.FindAllMatches();
        
        if (!_gameBoard.GetMatch(current) && !_gameBoard.GetMatch(other))
        {
            _gameBoard.SwapGems(current, other);
            yield return new WaitForSeconds(.5f);
            _coroutineRunning = false;
        }
        else
        {
            DestroyMatches();
        }
    }
#endregion

#region Logic
    void Init()
    {
        _gameBoard = new GameBoard(7, 7);
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

                int gemToUse = Random.Range(0, SC_GameVariables.Instance.gems.Length);

                int iterations = 0;
                while (_gameBoard.MatchesAt(x, y, SC_GameVariables.Instance.gems[gemToUse].type) && iterations < 100)
                {
                    gemToUse = Random.Range(0, SC_GameVariables.Instance.gems.Length);
                    iterations++;
                }
                SpawnGem(x, y, SC_GameVariables.Instance.gems[gemToUse]);
            }
    }
    
    void SpawnGem(int x, int y, SC_Gem gemToSpawn)
    {
        if (Random.Range(0, 100f) < SC_GameVariables.Instance.bombChance)
            gemToSpawn = SC_GameVariables.Instance.bomb;

        SC_Gem gem = Instantiate(gemToSpawn, new Vector3(x, y + SC_GameVariables.Instance.dropHeight, 0f), Quaternion.identity);
        gem.transform.SetParent(gemHolder.transform);
        gem.name = "Gem - " + x + ", " + y;
        _gameBoard.SetGem(x, y, gem);
        gem.SetupGem(x, y, MovementFinished);
    }
    
    // if anything is moving then we are in Wait state (waiting for movement to finish)
    // otherwise Move state (player can perform a move)
    static void MovementFinished(int x, int y) => Movement[x, y] = false;

    void DestroyMatches()
    {
        for (int x = 0; x < _gameBoard.Width; x++)
            for (int y = 0; y < _gameBoard.Height; y++)
                if (_gameBoard.GetMatch(x, y))
                {
                    SC_Gem gem = _gameBoard.GetGem(x, y);
                    if (gem)
                    {
                        SC_GameVariables.Instance.Score += SC_GameVariables.Instance.scoreValue;
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
                if (curGem == null)
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
        SC_Gem curGem = _gameBoard.GetGem(pos.x, pos.y);
        if (!curGem)
            return;

        Instantiate(curGem.destroyEffect, new Vector3(pos.x, pos.y), Quaternion.identity);

        Destroy(curGem.gameObject);
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
            for (int y = 0; y < _gameBoard.Height; y++)
            {
                SC_Gem curGem = _gameBoard.GetGem(x, y);
                if (curGem)
                    continue;

                int gemToUse = Random.Range(0, SC_GameVariables.Instance.gems.Length);
                SpawnGem(x, y, SC_GameVariables.Instance.gems[gemToUse]);
            }
    }
#endregion
}
