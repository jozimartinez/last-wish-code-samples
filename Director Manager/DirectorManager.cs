
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DirectorManager : MonoBehaviour
{
    #region SingletonDirector
    public static DirectorManager instance;
    void Awake()
    {
        instance = this;
    }
    #endregion SingletonDirector

    [HideInInspector]
    public Button playerRunButton;
    [HideInInspector]
    public int playerCurrentProgress;
    [HideInInspector]
    public int playerCurrentFearScore;
    [HideInInspector]
    public bool gameIsPaused = false;
    [HideInInspector]
    public GameObject playerAttackObject;
    [HideInInspector]
    public GameObject playerObject;
    [HideInInspector]
    public Player player;
    [HideInInspector]
    public Camera playerCamera;
    [HideInInspector]
    public AudioListener playerListener;
    [HideInInspector]
    public GameObject playerAttackGameObject;
    [HideInInspector]
    public PlayerHealth playerHealth;
    public InventoryObject inventory;
    [HideInInspector]
    public PlayerButton playerButton;
    [HideInInspector]
    public SlotsInventory slotsInventory;

    //Enemy
    [HideInInspector]
    public EnemyController enemy;
    [HideInInspector]
    public GameOverPanel gameOverPanel;


    [HideInInspector]
    public PauseButton pausePanel;
    [HideInInspector]
    public AudioManager audioManager;

    [HideInInspector]
    public bool tutorialFinished = false;
    [HideInInspector]
    public int currentEnemyState;

    //Everything under here - get it programmatically or create another class for it, but this will work for now
    [Header("Player")]
    public SkinnedMeshRenderer weaponMesh;
    public SkinnedMeshRenderer playerHoldingWeaponMesh;

    [Header("Environment Set Up")]
    [SerializeField]
    private DoorController laundryDoor;
    [SerializeField]
    private DoorController masterOneDoor;
    [SerializeField]
    private DoorController masterTwoDoor;
    [SerializeField]
    private DoorController bathroonDoor;
    [SerializeField]
    private DoorController childhoodDoor;
    [SerializeField]
    private DoorController diningDoor;
    [SerializeField]
    private DoorController kitchenDoor;
    [SerializeField]
    private DoorController studyDoor;
    [SerializeField]
    private DoorController middleDoor;
    [SerializeField]
    private GameObject masterKey;
    [SerializeField]
    private GameObject bathroomKey;
    [SerializeField]
    private GameObject childhoodRoomKey;
    [SerializeField]
    private GameObject diningKey;
    [SerializeField]
    private GameObject kitchenKey;
    [SerializeField]
    private GameObject studyKey;
    [SerializeField]
    private DoorController playerBathroomDoor;
    [SerializeField]
    private GameObject weaponOne;
    [SerializeField]
    private GameObject weaponTwo;

  

    private void Start()
    {
        GetPlayerHUD();
        GetPlayer();
        GetEnemy();
        GetGameOver();
        GetPause();

        audioManager = gameObject.GetComponentInChildren<AudioManager>();
        weaponMesh.enabled = inventory.HasWeaponEquipped();
        playerHoldingWeaponMesh.enabled = inventory.HasWeaponEquipped();
    }

    public List<GameObject> AIPoints()
    {
        List<GameObject> doors = GameObject.FindGameObjectsWithTag(ConstantVariables.AI_POINT).ToList().OrderBy(w => w.name).ToList();

        return doors;
    }

    private void GetPlayerHUD()
    {
        GameObject parentObject = GameObject.FindGameObjectWithTag(ConstantVariables.PLAYER_HUD);
        foreach (Button buttonPlayer in parentObject.GetComponentsInChildren<Button>())
        {
            if (buttonPlayer.name == ConstantVariables.PLAYER_BUTTON)
            {
                playerButton = buttonPlayer.GetComponent<PlayerButton>();
            }

            if (buttonPlayer.name == ConstantVariables.PLAYER_RUN_BUTTON)
            {
                playerRunButton = buttonPlayer;
            }
        }

        playerHealth = parentObject.GetComponentInChildren<PlayerHealth>();
        slotsInventory = parentObject.GetComponentInChildren<SlotsInventory>();
    }

    public void GetPlayer()
    {
        playerObject = GameObject.FindGameObjectWithTag(ConstantVariables.PLAYER);
        player = playerObject.GetComponent<Player>();
   
        var children = player.GetComponentsInChildren<Transform>();
        foreach (var child in children) 
        {

            if (child.name == ConstantVariables.PLAYER_CAMERA)
            {
                playerCamera = child.gameObject.GetComponent<Camera>();
                playerListener = child.gameObject.GetComponent<AudioListener>();
            }
            if (child.name == ConstantVariables.ATTACK_ANIMATION)
            {
                playerAttackObject = child.gameObject;
            }
        }

    }

    private void GetEnemy()
    {
        GameObject parentObject = GameObject.FindGameObjectWithTag(ConstantVariables.ENEMY);
        enemy = parentObject.GetComponent<EnemyController>();
    }

    private void GetGameOver()
    {
        GameObject parentObject = GameObject.FindGameObjectWithTag(ConstantVariables.GAME_OVER_PANEL);
        gameOverPanel = parentObject.GetComponent<GameOverPanel>();
    }

    private void GetPause()
    {
        GameObject parentObject = GameObject.FindGameObjectWithTag(ConstantVariables.PAUSE_PANEL);
        pausePanel = parentObject.GetComponent<PauseButton>();
    }

    public void UpdatePlayerProgressScore(int score)
    {
        playerCurrentProgress = score;
        player.progressScore = playerCurrentProgress;
        enemy.playerProgress = playerCurrentFearScore;
    }

    public void UpdatePlayerFearMeter(int score)
    {

        playerCurrentFearScore = score;
        player.fearScore = playerCurrentFearScore;
        enemy.playerFear = playerCurrentFearScore;
    }

    public void SetUpItemAndDoors(Player playerData)
    {
        playerCurrentProgress = playerData.progressScore;
        playerCurrentFearScore = playerData.fearScore;
        tutorialFinished = player.finishedTutorial;
        enemy.finishedTutorial = tutorialFinished;

        if (playerData.finishedTutorial == false)
        {
            playerBathroomDoor.isLocked = true;
            laundryDoor.isLocked = true;
        }

        masterOneDoor.isLocked = playerData.masterDoorLocked;
        masterTwoDoor.isLocked = playerData.masterDoorLocked;
        masterKey.SetActive(playerData.masterKeyNotFound);

        bathroonDoor.isLocked = playerData.bathroomDoorLocked;
        bathroomKey.SetActive(playerData.bathroomDoorNotKeyFound);
        childhoodDoor.isLocked = playerData.childhoodDoorLocked;
        childhoodRoomKey.SetActive(playerData.childhoodDoorNotKeyFound);

        diningDoor.isLocked = playerData.diningDoorLocked;
        diningKey.SetActive(playerData.diningDoorKeyNotFound);

        kitchenDoor.isLocked = playerData.kitchenDoorLocked;
        kitchenKey.SetActive(playerData.kitchenDoorKeyNotFound);

        studyDoor.isLocked = playerData.studyDoorLocked;
        studyKey.SetActive(playerData.studyDoorKeyNotFound);

        weaponOne.SetActive(playerData.weaponOneNotFound);
        weaponTwo.SetActive(playerData.weaponTwoNotFound);
        UpdateMiddleDoor();
    }

    public void UpdateMiddleDoor()
    {
        if(diningDoor.isLocked == false && kitchenDoor.isLocked == false)
        {
            middleDoor.isLocked = false;
        }
        else
        {
            for (int i = 0; i < inventory.slots.items.Count; i++)
            {
                InventorySlot container = inventory.slots.items[i];
                if (inventory.database.getItem[container.itemId].type == ItemType.Key)
                {
                    KeyObject keyObject = (KeyObject)inventory.database.getItem[container.itemId];
                    if(keyObject.roomType == KeyType.DiningRoomDoor)
                    {
                        middleDoor.keyType = KeyType.KitchenDoor;
                    }else if(keyObject.roomType == KeyType.KitchenDoor)
                    {
                        middleDoor.keyType = KeyType.DiningRoomDoor;
                    }

                }

            }
        }
    }

    public void UpdatePaused(bool gameIsPaused)
    {
        enemy.gameIsPaused = gameIsPaused;
        enemy.navAgent.isStopped = gameIsPaused;
    }

    public void UpdateEnemyTutorial(bool update)
    {
        enemy.finishedTutorial = update;
    }
    
  
}
