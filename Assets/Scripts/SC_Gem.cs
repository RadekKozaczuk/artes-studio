using System;
using UnityEngine;

public class SC_Gem : MonoBehaviour
{
    /// <summary>
    /// For now, it is also the id of the element.
    /// In the future these to should be separated.
    /// </summary>
    public Vector2Int PosIndex
    {
        get => _posIndex;
        set
        {
            _posIndex = value;
            SC_GameLogic.Movement[_posIndex.x, _posIndex.y] = true;
        }
    }
    Vector2Int _posIndex;

    public GlobalEnums.GemType type;
    public bool isMatch;
    public GameObject destroyEffect;
    public int scoreValue = 10;
    public int blastSize = 1;

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
