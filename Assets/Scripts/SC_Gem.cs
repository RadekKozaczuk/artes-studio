using System;
using UnityEngine;
using UnityEngine.Assertions;

public class SC_Gem : MonoBehaviour
{
    /// <summary>
    /// For now, it is also the id of the element.
    /// In the future these to should be separated.
    /// </summary>
    public Vector2Int PosIndex
    {
        get => _posIndex; // todo: for now this has two meaning: id, and position
        set
        {
            Assert.IsFalse(_posIndex == value, "Assigning the same position is not allowed");
            
            _posIndex = value;
            SC_GameLogic.Movement[_posIndex.x, _posIndex.y] = true;
        }
    }
    Vector2Int _posIndex = new (int.MinValue, int.MinValue);

    public GlobalEnums.GemType type;
    public GameObject destroyEffect;

    Action<int, int> _movementFinishedCallback;

    public void UpdatePosition()
    {
        if (SC_GameLogic.Movement[_posIndex.x, _posIndex.y])
        {
            if (Vector2.Distance(transform.position, PosIndex) > 0.01f)
                transform.position = Vector2.Lerp(transform.position, PosIndex, SC_GameVariables.Instance.gemSpeed * Time.deltaTime);
            else
            {
                transform.position = new Vector3(PosIndex.x, PosIndex.y, 0);
                _movementFinishedCallback(PosIndex.x, PosIndex.y);
            }
        }
    }

    public void SetupGem(Vector2Int position, Action<int, int> movementFinishedCallback)
    {
        PosIndex = position;
        _movementFinishedCallback = movementFinishedCallback;
    }
}
