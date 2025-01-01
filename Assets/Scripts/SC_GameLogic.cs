using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SC_GameLogic : MonoBehaviour
{
    public GlobalEnums.GameState CurrentState { get; private set; } = GlobalEnums.GameState.Move;

    const int Score = 0;
    float _displayScore;

    public TextMeshProUGUI score;
    public Transform gemHolder;
    
    GameBoard _gameBoard;

#region MonoBehaviour
    void Start()
    {
        score.text = Score.ToString("0");
        Init();
    }

    void Update()
    {
        _displayScore = Mathf.Lerp(_displayScore, _gameBoard.Score, SC_GameVariables.Instance.scoreSpeed * Time.deltaTime);
        score.text = _displayScore.ToString("0");
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
        gem.SetupGem(this, position);
    }

    public void SetGem(int x, int y, SC_Gem gem)
    {
        _gameBoard.SetGem(x, y, gem);
    }

    public SC_Gem GetGem(int x, int y)
    {
        return _gameBoard.GetGem(x, y);
    }

    public void SetState(GlobalEnums.GameState currentState)
    {
        CurrentState = currentState;
    }
    
    public void DestroyMatches()
    {
        foreach (SC_Gem gem in _gameBoard.CurrentMatches)
            if (gem != null)
            {
                _gameBoard.Score += gem.scoreValue;
                DestroyMatchedGemsAt(gem.posIndex);
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
                    curGem.posIndex.y -= nullCounter;
                    SetGem(x, y - nullCounter, curGem);
                    SetGem(x, y, null);
                }
            }
            nullCounter = 0;
        }

        StartCoroutine(FilledBoardCo());
    }

    void DestroyMatchedGemsAt(Vector2Int pos)
    {
        SC_Gem curGem = _gameBoard.GetGem(pos.x, pos.y);
        if (curGem == null)
            return;

        Instantiate(curGem.destroyEffect, new Vector2(pos.x, pos.y), Quaternion.identity);

        Destroy(curGem.gameObject);
        SetGem(pos.x, pos.y, null);
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
                if (curGem != null)
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
    
    public void FindAllMatches()
    {
        _gameBoard.FindAllMatches();
    }

#endregion
}
