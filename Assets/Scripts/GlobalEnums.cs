using UnityEngine;

public class GlobalEnums : MonoBehaviour
{
    public enum GemType
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
}
