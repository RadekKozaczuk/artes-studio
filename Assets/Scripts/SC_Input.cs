using System;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Input mono's job is to calculate the user's input represented by a pair of <see cref="Vector2Int"/> values
/// each representing the board's coordinates. These values should be interpreted as a pair of elements that should be swapped.
/// External system should call <see cref="TryGetInput"/> to retrieve the user's input for this frame.
/// </summary>
public class SC_Input : MonoBehaviour
{
    bool _mousePressed;
    Vector2 _firstTouchPosition;
    Vector2 _finalTouchPosition;
    
    [SerializeField]
    SC_GameLogic _scGameLogic;

    [SerializeField]
    Camera _camera;

    // this should be called by logic
    public bool TryGetInput(out Vector2Int current, out Vector2Int other)
    {
        Assert.IsTrue(_scGameLogic.CurrentState == GlobalEnums.GameState.Move, "Input should not be read during the animation");
        
        if (_mousePressed && Input.GetMouseButtonUp(0))
        {
            _mousePressed = false;
            _finalTouchPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            
            // todo: should be resistant to the gem slot size and the number of rows and columns
            // screen position to board coordinates mapping
            int firstX = (int)(_firstTouchPosition.x + 0.5f);
            int firstY = (int)(_firstTouchPosition.y + 0.5f);
            int finalX = (int)(_finalTouchPosition.x + 0.5f);
            int finalY = (int)(_finalTouchPosition.y + 0.5f);
            
            // distance in board elements cannot be bigger than 1
            int distanceX = Math.Abs(finalX - firstX);
            int distanceY = Math.Abs(finalY - firstY);

            if (distanceX == 1 && distanceY == 0 || distanceX == 0 && distanceY == 1)
                if (Vector3.Distance(_firstTouchPosition, _finalTouchPosition) > .5f)
                {
                    current = new Vector2Int(firstX, firstY);
                    other = new Vector2Int(finalX, finalY);
                    return true;
                }
        }
        
        current = Vector2Int.zero;
        other = Vector2Int.zero;
        return false;
    }
    
    void OnMouseDown()
    {
        if (_scGameLogic.CurrentState == GlobalEnums.GameState.Move)
        {
            _firstTouchPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            _mousePressed = true;
        }
    }
}
