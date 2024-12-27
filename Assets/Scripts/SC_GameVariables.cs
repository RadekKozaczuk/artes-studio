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
    
    [HideInInspector]
    public int rowsSize = 7;
    [HideInInspector]
    public int colsSize = 7;

#region Singleton
    static SC_GameVariables _instance;
    public static SC_GameVariables Instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.Find("SC_GameVariables").GetComponent<SC_GameVariables>();

            return _instance;
        }
    }
#endregion
}
