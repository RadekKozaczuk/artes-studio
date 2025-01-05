using UnityEngine;

public class SC_GameVariables : MonoBehaviour
{
    public GameObject bgTilePrefabs;
    public SC_Gem bomb;
    public SC_Gem[] gems;
    public float bonusAmount = 0.5f;
    public float bombChance = 2f;
    public int dropHeight = 0;
    public float gemSpeed;
    public float scoreSpeed = 5;
    public int Score;
    
    [HideInInspector]
    public int rowsSize = 7;
    [HideInInspector]
    public int colsSize = 7;
    
    public int scoreValue = 10;
    public int blastSize = 1;

    public static SC_GameVariables Instance;

    void Awake() => Instance = this;
}
