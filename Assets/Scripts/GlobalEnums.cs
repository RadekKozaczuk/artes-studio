using UnityEngine;

public class GlobalEnums : MonoBehaviour
{
    public enum PieceType
    {
        Blue, Green, Red, Yellow, Purple, Bomb
    };

    public enum GameState
    {
        /// <summary>
        /// Waiting for animation to finish.
        /// </summary>
        Wait,
        
        /// <summary>
        /// Waiting for player's input.
        /// </summary>
        Move
    }

    public enum MatchType
    {
        /// <summary>
        /// The piece did not match this frame.
        /// </summary>
        Nothing,
        
        /// <summary>
        /// The piece matched this frame, and it is a 3-element match. 
        /// </summary>
        ThreePiece,
        
        /// <summary>
        /// The piece matched this frame, and it is a 4-element match. 
        /// </summary>
        FourPiece,
        
        /// <summary>
        /// The piece was destroyed by a bomb. 
        /// </summary>
        Bomb,
        
        /// <summary>
        /// The piece self-destroyed. 
        /// </summary>
        BombItself
    }
}
