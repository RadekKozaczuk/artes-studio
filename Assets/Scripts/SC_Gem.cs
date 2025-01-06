using System;
using UnityEngine;
using UnityEngine.Assertions;

public class SC_Gem : MonoBehaviour
{
#region Properties
    /// <summary>
    /// For now, it is also the id of the element.
    /// In the future these to should be separated.
    /// </summary>
    public Vector2Int posIndex
    {
        get => _posIndex; // todo: for now this has two meaning: id, and position
        set
        {
            Assert.IsFalse(_posIndex == value, "Assigning the same position is not allowed");
            
            _posIndex = value;

            // when gems are reused (pooling) posIndex is assigned with (int.MinValue, int.MinValue)
            if (posIndex.x == int.MinValue)
                return;
            
            SC_GameLogic.Movement[_posIndex.x, _posIndex.y] = true;
        }
    }
    Vector2Int _posIndex = new (int.MinValue, int.MinValue);
#endregion

#region Variables
    public GlobalEnums.PieceType type;
    public SpriteRenderer spriteRenderer;
    public float dropDelay;

    Action<int, int> _movementFinishedCallback;
#endregion
    
    public void UpdatePosition()
    {
        float time = Time.deltaTime;

        dropDelay -= time;

        if (dropDelay > 0)
            return;
        
        if (SC_GameLogic.Movement[_posIndex.x, _posIndex.y])
        {
            // caching
            Vector3 pos = transform.position;
            
            if (Utils.FastDistance2D(pos, posIndex) > 0.01f)
                transform.position = Vector3.Lerp(pos, new Vector3(posIndex.x, posIndex.y, 0), SC_GameVariables.Instance.gemSpeed * time);
            else
            {
                transform.position = new Vector3(posIndex.x, posIndex.y, 0);
                _movementFinishedCallback(posIndex.x, posIndex.y);
            }
        }
    }
    
    public void SetupGem(int x, int y, GlobalEnums.PieceType gemType, Action<int, int> movementFinishedCallback)
    {
        posIndex = new Vector2Int(x, y);
        type = gemType;
        spriteRenderer.sprite = SC_GameVariables.Instance.gemSprites[(int)type];
        _movementFinishedCallback = movementFinishedCallback;
    }
}
