using System.Collections;
using UnityEngine;

public class SC_Gem : MonoBehaviour
{
    [HideInInspector]
    public Vector2Int posIndex;

    public GlobalEnums.GemType type;
    public bool isMatch;
    public GameObject destroyEffect;
    public int scoreValue = 10;
    public int blastSize = 1;
    
    Vector2 _firstTouchPosition;
    Vector2 _finalTouchPosition;
    bool _mousePressed;
    float _swipeAngle;
    SC_Gem _otherGem;
    Vector2Int _previousPos;
    SC_GameLogic _scGameLogic;

    void Update()
    {
        if (Vector2.Distance(transform.position, posIndex) > 0.01f)
            transform.position = Vector2.Lerp(transform.position, posIndex, SC_GameVariables.Instance.gemSpeed * Time.deltaTime);
        else
        {
            transform.position = new Vector3(posIndex.x, posIndex.y, 0);
            _scGameLogic.SetGem(posIndex.x, posIndex.y, this);
        }

        //bool temp = SC_Input.Instance.TryGetInput(out Vector2Int a, out Vector2Int b);
        
        // todo: the problem is that colliders 'collide' here
        // todo: and to move forward we have to rewire everything
        // todo: for safety reasons I would first commit all non-invasive changes
        // todo: and then try to refactor it for good
        if (_mousePressed && Input.GetMouseButtonUp(0))
        {
            _mousePressed = false;
            if (_scGameLogic.CurrentState == GlobalEnums.GameState.Move)
            {
                _finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CalculateAngle();
            }
        }
    }

    public void SetupGem(SC_GameLogic scGameLogic, Vector2Int position)
    {
        posIndex = position;
        _scGameLogic = scGameLogic;
    }

    void OnMouseDown()
    {
        if (_scGameLogic.CurrentState == GlobalEnums.GameState.Move)
        {
            _firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _mousePressed = true;
        }
    }

    void CalculateAngle()
    {
        _swipeAngle = Mathf.Atan2(_finalTouchPosition.y - _firstTouchPosition.y, _finalTouchPosition.x - _firstTouchPosition.x);
        _swipeAngle = _swipeAngle * 180 / Mathf.PI;

        if (Vector3.Distance(_firstTouchPosition, _finalTouchPosition) > .5f)
            MovePieces();
    }

    void MovePieces()
    {
        _previousPos = posIndex;

        if (_swipeAngle is < 45 and > -45 && posIndex.x < SC_GameVariables.Instance.rowsSize - 1)
        {
            _otherGem = _scGameLogic.GetGem(posIndex.x + 1, posIndex.y);
            _otherGem.posIndex.x--;
            posIndex.x++;
            
            Debug.Log($"RIGHT posIndex.x: {posIndex.x}, _otherGem.posIndex.x: {_otherGem.posIndex.x}");
        }
        else if (_swipeAngle is > 45 and <= 135 && posIndex.y < SC_GameVariables.Instance.colsSize - 1)
        {
            _otherGem = _scGameLogic.GetGem(posIndex.x, posIndex.y + 1);
            _otherGem.posIndex.y--;
            posIndex.y++;
            
            Debug.Log($"TOP posIndex.x: {posIndex.x}, _otherGem.posIndex.x: {_otherGem.posIndex.x}");
        }
        else if (_swipeAngle is < -45 and >= -135 && posIndex.y > 0)
        {
            _otherGem = _scGameLogic.GetGem(posIndex.x, posIndex.y - 1);
            _otherGem.posIndex.y++;
            posIndex.y--;
            
            Debug.Log($"LEFT posIndex.x: {posIndex.x}, _otherGem.posIndex.x: {_otherGem.posIndex.x}");
        }
        else if (_swipeAngle > 135 || _swipeAngle < -135 && posIndex.x > 0)
        {
            _otherGem = _scGameLogic.GetGem(posIndex.x - 1, posIndex.y);
            _otherGem.posIndex.x++;
            posIndex.x--;
            
            Debug.Log($"DOWN posIndex.x: {posIndex.x}, _otherGem.posIndex.x: {_otherGem.posIndex.x}");
        }

        _scGameLogic.SetGem(posIndex.x, posIndex.y, this);
        _scGameLogic.SetGem(_otherGem.posIndex.x, _otherGem.posIndex.y, _otherGem);

        StartCoroutine(CheckMoveCo());
    }

    IEnumerator CheckMoveCo()
    {
        _scGameLogic.SetState(GlobalEnums.GameState.Wait);

        yield return new WaitForSeconds(.5f);
        _scGameLogic.FindAllMatches();

        if (_otherGem == null)
            yield break;

        if (isMatch == false && _otherGem.isMatch == false)
        {
            _otherGem.posIndex = posIndex;
            posIndex = _previousPos;

            _scGameLogic.SetGem(posIndex.x, posIndex.y, this);
            _scGameLogic.SetGem(_otherGem.posIndex.x, _otherGem.posIndex.y, _otherGem);

            yield return new WaitForSeconds(.5f);
            _scGameLogic.SetState(GlobalEnums.GameState.Move);
        }
        else
        {
            _scGameLogic.DestroyMatches();
        }
    }
}
