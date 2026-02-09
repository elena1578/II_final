using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleTransitionManager : MonoBehaviour
{
    [HideInInspector] public BattleTransitionManager instance;
    [SerializeField] private RoomData.RoomID storedRoomID;
    [SerializeField] private Vector3 storedPlayerPosition;

    private void Awake()
    {
        instance = this;
        
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);        
    }

    public void ReturnToOverworld()
    {
        // probably need to add a specific from-battle method
        // either here or roomchangemanager to account going to a vector3 vs. a spawn point
        ClearCurrentData();
    }

    public void ReturnToTitleScreen()
    {
        StartCoroutine(LoadTitleScreenRoutine());
        ClearCurrentData();
    }

    private IEnumerator LoadTitleScreenRoutine()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("TitleScreen");

        while (!asyncLoad.isDone)
            yield return null;
    }

    public void StoreCurrentData(RoomData.RoomID roomID, Vector3 playerPosition)
    {
        storedRoomID = roomID;
        storedPlayerPosition = playerPosition;
    }

    private void ClearCurrentData()
    {
        storedRoomID = RoomData.RoomID.Entrance153_01;  // default room
        storedPlayerPosition = Vector3.zero;
    }
}
