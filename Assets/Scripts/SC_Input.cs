using UnityEngine;

/// <summary>
/// Input mono's job is to calculate the user's input represented by a pair of <see cref="Vector2Int"/> values
/// each representing the board's coordinates. These values should be interpreted as a pair of elements that should be swapped.
/// External system should call <see cref="TryGetInput"/> to retrieve the user's input for this frame.
/// </summary>
public class SC_Input : MonoBehaviour
{
    [HideInInspector]
    public Vector2Int posIndex;
    
    bool _mousePressed;
    float _swipeAngle;
    Vector2 _firstTouchPosition;
    Vector2 _finalTouchPosition;
    SC_Gem _otherGem;
    Vector2Int _previousPos;
    
    [SerializeField]
    SC_GameLogic _scGameLogic;

    [SerializeField]
    Camera _camera;
    
    // todo: for now let's create a singleton to access this structure
    // todo: this will change once gem's logic is moved to game logic
    public static SC_Input Instance;

    public bool TryGetInput(out Vector2Int current, out Vector2Int other)
    {
        if (_mousePressed && Input.GetMouseButtonUp(0))
        {
            _mousePressed = false;
            if (_scGameLogic.CurrentState == GlobalEnums.GameState.Move)
            {
                _finalTouchPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                _swipeAngle = Mathf.Atan2(_finalTouchPosition.y - _firstTouchPosition.y, _finalTouchPosition.x - _firstTouchPosition.x);
                _swipeAngle = _swipeAngle * 180 / Mathf.PI;

                // so essentially we have first and final touch position
                // now we have to somehow map it to board coords
                
                // for now, we can hardcode that the board start from 0, 0 and end at 6, 6
                // 
                
                //Debug.Log($"INPUT V2 first: {_firstTouchPosition}, final: {_finalTouchPosition}");

                int firstX = (int)(_firstTouchPosition.x + 0.5f);
                int firstY = (int)(_firstTouchPosition.y + 0.5f);
                int finalX = (int)(_finalTouchPosition.x + 0.5f);
                int finalY = (int)(_finalTouchPosition.y + 0.5f);
                
                Debug.Log($"INPUT V2 first: {firstX},{firstY}, final: {finalX},{finalY}");

                /*if (Vector3.Distance(_firstTouchPosition, _finalTouchPosition) > .5f)
                {
                    _previousPos = posIndex;

                    if (_swipeAngle is < 45 and > -45 && posIndex.x < SC_GameVariables.Instance.rowsSize - 1)
                    {
                        //_otherGem = _scGameLogic.GetGem(posIndex.x + 1, posIndex.y);
                        //_otherGem.posIndex.x--;
                        //posIndex.x++;
                        Debug.Log($"V2 RIGHT posIndex.x: {posIndex.x}, _otherGem.posIndex.x: {_otherGem.posIndex.x}");
                        current = new Vector2Int(posIndex.x + 1, posIndex.y);
                        other = new Vector2Int(_otherGem.posIndex.x - 1, _otherGem.posIndex.y);
                        return true;
                    }

                    if (_swipeAngle is > 45 and <= 135 && posIndex.y < SC_GameVariables.Instance.colsSize - 1)
                    {
                        /*_otherGem = _scGameLogic.GetGem(posIndex.x, posIndex.y + 1);
                            _otherGem.posIndex.y--;
                            posIndex.y++;#1#
                        Debug.Log($"V2 TOP posIndex.x: {posIndex.x}, _otherGem.posIndex.x: {_otherGem.posIndex.x}");
                        current = new Vector2Int(posIndex.x, posIndex.y + 1);
                        other = new Vector2Int(_otherGem.posIndex.x, _otherGem.posIndex.y - 1);
                        return true;
                    }

                    if (_swipeAngle is < -45 and >= -135 && posIndex.y > 0)
                    {
                        /*_otherGem = _scGameLogic.GetGem(posIndex.x, posIndex.y - 1);
                        _otherGem.posIndex.y++;
                        posIndex.y--;#1#

                        Debug.Log($"V2 LEFT posIndex.x: {posIndex.x}, _otherGem.posIndex.x: {_otherGem.posIndex.x}");
                        current = new Vector2Int(posIndex.x, posIndex.y - 1);
                        other = new Vector2Int(_otherGem.posIndex.x, _otherGem.posIndex.y + 1);
                        return true;
                    }

                    if (_swipeAngle > 135 || (_swipeAngle < -135 && posIndex.x > 0))
                    {
                        /*_otherGem = _scGameLogic.GetGem(posIndex.x - 1, posIndex.y);
                        _otherGem.posIndex.x++;
                        posIndex.x--;#1#

                        Debug.Log($"V2 BOTTOM posIndex.x: {posIndex.x}, _otherGem.posIndex.x: {_otherGem.posIndex.x}");
                        current = new Vector2Int(posIndex.x - 1, posIndex.y);
                        other = new Vector2Int(_otherGem.posIndex.x, _otherGem.posIndex.y - 1);
                        return true;
                    }
                }*/
            }
        }
        
        current = Vector2Int.zero;
        other = Vector2Int.zero;
        return false;
    }
    
    void Awake()
    {
        Instance = this;
    }

    /*void Update()
    {
        if (_mousePressed && Input.GetMouseButtonUp(0))
        {
            _mousePressed = false;
            if (_scGameLogic.CurrentState == GlobalEnums.GameState.Move)
            {
                _finalTouchPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                CalculateAngle();
            }
        }
    }*/
    
    void OnMouseDown()
    {
        Debug.Log("OnMouseDown Input V2");
        
        if (_scGameLogic.CurrentState == GlobalEnums.GameState.Move)
        {
            _firstTouchPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            _mousePressed = true;
        }
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
            
            Debug.Log($"BOTTOM posIndex.x: {posIndex.x}, _otherGem.posIndex.x: {_otherGem.posIndex.x}");
        }
    }
}
