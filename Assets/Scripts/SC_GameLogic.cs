using System;
using System.Collections;
using System.Collections.Generic;
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
            
            Debug.Log($"Current state set to: {value}");
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
        // todo: this is stupidly updated every frame, should on change 
        _displayScore = Mathf.Lerp(_displayScore, SC_GameVariables.Instance.Score, SC_GameVariables.Instance.scoreSpeed * Time.deltaTime);
        score.text = _displayScore.ToString("0");

        // if in wait then check movement table 
        if (CurrentState == GlobalEnums.GameState.Wait)
        {
            _gameBoard.UpdateGems();

            if (!AnythingMoves())
                CurrentState = GlobalEnums.GameState.Move;
        }
        else if (CurrentState == GlobalEnums.GameState.Move)
        {
            if (_scInput.TryGetInput(out Vector2Int current, out Vector2Int other))
            {
                // change state
                CurrentState = GlobalEnums.GameState.Wait;

                SC_Gem currentGem = _gameBoard.GetGem(current);
                SC_Gem otherGem = _gameBoard.GetGem(other);

                // set (swap) references
                _gameBoard.SetGem(current.x, current.y, currentGem);
                _gameBoard.SetGem(other.x, other.y, otherGem);

                StartCoroutine(CheckMoveCo(currentGem, otherGem));
            }
        }
    }
    
    IEnumerator CheckMoveCo(SC_Gem current, SC_Gem other)
    {
        yield return new WaitForSeconds(.5f);

        _gameBoard.FindAllMatches();

        if (current.isMatch == false && other.isMatch == false)
        {
            (other.PosIndex, current.PosIndex) = (current.PosIndex, other.PosIndex);

            _gameBoard.SetGem(current.PosIndex.x, current.PosIndex.y, current);
            _gameBoard.SetGem(other.PosIndex.x, other.PosIndex.y, other);

            yield return new WaitForSeconds(.5f);
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
                var pos = new Vector2(x, y);
                GameObject bgTile = Instantiate(SC_GameVariables.Instance.bgTilePrefabs, pos, Quaternion.identity);
                bgTile.transform.SetParent(gemHolder);
                bgTile.name = "BG Tile - " + x + ", " + y;

                int gemToUse = Random.Range(0, SC_GameVariables.Instance.gems.Length);

                int iterations = 0;
                while (_gameBoard.MatchesAt(new Vector2Int(x, y), SC_GameVariables.Instance.gems[gemToUse]) && iterations < 100)
                {
                    gemToUse = Random.Range(0, SC_GameVariables.Instance.gems.Length);
                    iterations++;
                }
                SpawnGem(new Vector2Int(x, y), SC_GameVariables.Instance.gems[gemToUse]);
            }
    }

    void SpawnGem(Vector2Int position, SC_Gem gemToSpawn)
    {
        if (Random.Range(0, 100f) < SC_GameVariables.Instance.bombChance)
            gemToSpawn = SC_GameVariables.Instance.bomb;

        SC_Gem gem = Instantiate(gemToSpawn, new Vector3(position.x, position.y + SC_GameVariables.Instance.dropHeight, 0f), Quaternion.identity);
        gem.transform.SetParent(gemHolder.transform);
        gem.name = "Gem - " + position.x + ", " + position.y;
        _gameBoard.SetGem(position.x, position.y, gem);
        gem.SetupGem(position, MovementFinished);
    }
    
    // if anything is moving then we are in Wait state (waiting for movement to finish)
    // otherwise Move state (player can perform a move)
    void MovementFinished(int x, int y)
    {
        Movement[x, y] = false;
    }

    void DestroyMatches()
    {
        foreach (SC_Gem gem in _gameBoard.CurrentMatches)
            if (gem)
            {
                SC_GameVariables.Instance.Score += gem.scoreValue;
                DestroyMatchedGemsAt(gem.PosIndex);
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
                    curGem.PosIndex = new Vector2Int(curGem.PosIndex.x, curGem.PosIndex.y - nullCounter);
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

        Instantiate(curGem.destroyEffect, new Vector2(pos.x, pos.y), Quaternion.identity);

        Destroy(curGem.gameObject);
        _gameBoard.SetGem(pos.x, pos.y, null);
    }

    IEnumerator FilledBoardCo()
    {
        yield return new WaitForSeconds(0.5f);
        RefillBoard();
        yield return new WaitForSeconds(0.5f);
        _gameBoard.FindAllMatches();
        if (_gameBoard.CurrentMatches.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);
            DestroyMatches();
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            CurrentState = GlobalEnums.GameState.Move;
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
                SpawnGem(new Vector2Int(x, y), SC_GameVariables.Instance.gems[gemToUse]);
            }

        CheckMisplacedGems();
    }

    void CheckMisplacedGems()
    {
        var foundGems = new List<SC_Gem>();
        foundGems.AddRange(FindObjectsOfType<SC_Gem>());
        for (int x = 0; x < _gameBoard.Width; x++)
        {
            for (int y = 0; y < _gameBoard.Height; y++)
            {
                SC_Gem curGem = _gameBoard.GetGem(x, y);
                if (foundGems.Contains(curGem))
                    foundGems.Remove(curGem);
            }
        }

        foreach (SC_Gem g in foundGems)
            Destroy(g.gameObject);
    }
#endregion
}
