using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerButton : MonoBehaviour
{
    [SerializeField]
    private Transform wonPanel;
    [SerializeField]
    private DoorController playerBathroomDoor;
    [SerializeField]
    private DoorController laundryDoor;
    //Button Reference
    private Button playerButton;
    private Text playerButtonLabel;
    //Variables
    private GameObject interactableItem;
    private Item itemGame;
    private DoorController itemDoor;
    public AudioSource backgroundAudio;
    public Button playerRunButton;
    public Button pauseButton;
    public Text tutorialText;


    private void Start()
    {
        wonPanel.localScale = new Vector3(0, 0, 0);
        playerRunButton.gameObject.SetActive(false);

        GameObject parentObject = GameObject.FindGameObjectWithTag(ConstantVariables.PLAYER_HUD);
        foreach (Button button in parentObject.GetComponentsInChildren<Button>())
        {
            if (button.name == ConstantVariables.PLAYER_BUTTON)
            {
                playerButton = button;
                playerButtonLabel = playerButton.GetComponentInChildren<Text>();
            }
        }

        playerButton.gameObject.SetActive(false);

    }


    public void DetectedGameItem(GameItem gameItem, GameObject otherGameObject)
    {
        playerButton.onClick.RemoveAllListeners();
        interactableItem = otherGameObject;
        playerButton.gameObject.SetActive(true);
       

        switch (gameItem.itemObject.type)
        {
            case ItemType.DiaryPaper:
                playerButtonLabel.text = ConstantVariables.READ_DIARY_BUTTON_LABEL;
                break;
            case ItemType.Key:
                KeyObject keyObject = (KeyObject)gameItem.itemObject;
                itemGame = keyObject.CreateKey();
                playerButtonLabel.text = ConstantVariables.PICK_UP_KEY_BUTTON_LABEL;
                playerButton.onClick.AddListener(AddItemsToInventory);
                break;
            case ItemType.Weapon:
              
                WeaponObject weaponObject = (WeaponObject)gameItem.itemObject;
                itemGame = weaponObject.CreateWeapon();
                playerButtonLabel.text = ConstantVariables.EQUIP_WEAPON_BUTTON_LABEL;
                playerButton.onClick.AddListener(AddWeaponToInventory);
                break;
            default:
                Debug.Log("Item found was not a key, weapon, or diary : " + gameItem.itemObject.title);
                break;
        }
    }

    public void DectectedDoorItem(DoorController doorItem)
    {
        playerButton.onClick.RemoveAllListeners();
      
        itemDoor = doorItem;

        playerButton.gameObject.SetActive(true);
        
        if (!itemDoor.isOpen)
        {
            playerButtonLabel.text = ConstantVariables.OPEN_DOOR_BUTTON_LABEL;
            playerButton.onClick.AddListener(OpenDoor);
        }
        else
        {
            playerButtonLabel.text = ConstantVariables.CLOSE_DOOR_BUTTON_LABEL;
            playerButton.onClick.AddListener(CloseDoor);
        }
    }

    public void DectectedEnemy()
    {
        playerButton.gameObject.SetActive(true);
        playerButtonLabel.text = ConstantVariables.STUN_ENEMY_BUTTON_LABEL;
        playerButton.onClick.AddListener(StunEnemy);

    }

    //Disables the button
    public void Reset()
    {
        itemGame = null;
        interactableItem = null;
        playerButtonLabel.text = null;
        playerButton.gameObject.SetActive(false);
        playerButton.onClick.RemoveAllListeners();
    }

    public void AddWeaponToInventory()
    {
        if ((Weapon)itemGame is Weapon)
        {
            //Add sound effective
            switch (((Weapon)itemGame).weaponType)
            {
                case WeaponType.WeaponOne:
                    tutorialText.text = itemGame.description;
                    DirectorManager.instance.player.weaponOneNotFound = false;
                    DirectorManager.instance.UpdatePlayerFearMeter(10);
                    break;
                case WeaponType.WeaponTwo:
                    tutorialText.text = itemGame.description;
                    DirectorManager.instance.player.weaponTwoNotFound = false;
                    DirectorManager.instance.UpdatePlayerFearMeter(20);
                    break;
                default:
                    tutorialText.text = null;
                    break;
            }
        }

        if (!DirectorManager.instance.inventory.HasWeaponEquipped())
        {
            Destroy(interactableItem);
            DirectorManager.instance.inventory.AddItem(itemGame);
            DirectorManager.instance.slotsInventory.UpdateSlots();
            DirectorManager.instance.weaponMesh.enabled = DirectorManager.instance.inventory.HasWeaponEquipped();
            DirectorManager.instance.playerHoldingWeaponMesh.enabled = DirectorManager.instance.inventory.HasWeaponEquipped();
            Reset();

        }
        else
        {
            tutorialText.text = ConstantVariables.WEAPON_AMOUNT_ERROR;
            StartCoroutine(ResetTutorialText());
        }

    }

    //Activates the "pick up" button
    public void AddItemsToInventory()
        {
            tutorialText.text = null;

            if ((Key)itemGame is Key)
            {
                DirectorManager.instance.audioManager.Play(ConstantVariables.PICKING_UP_KEY_SFX);

                switch (((Key)itemGame).roomType)
                {
                    case KeyType.BathroomDoor:
                        tutorialText.text = itemGame.description;
                        DirectorManager.instance.player.bathroomDoorNotKeyFound = false;
                        DirectorManager.instance.UpdatePlayerProgressScore(5);
                        break;
                    case KeyType.ChildhoodRoomDoor:
                        tutorialText.text = itemGame.description;
                        DirectorManager.instance.player.childhoodDoorNotKeyFound = false;
                        DirectorManager.instance.UpdatePlayerProgressScore(15);
                        break;
                    case KeyType.KitchenDoor:
                        tutorialText.text = itemGame.description;
                        DirectorManager.instance.player.kitchenDoorKeyNotFound = false;
                        DirectorManager.instance.UpdatePlayerProgressScore(25);
                        break;
                    case KeyType.DiningRoomDoor:
                        tutorialText.text = itemGame.description;
                        DirectorManager.instance.player.diningDoorKeyNotFound = false;
                        DirectorManager.instance.UpdatePlayerProgressScore(35);
                        break;
                    case KeyType.StudyRoomDoor:
                        tutorialText.text = itemGame.description;
                        DirectorManager.instance.player.studyDoorKeyNotFound = false;
                        DirectorManager.instance.UpdatePlayerProgressScore(45);
                        break;
                    case KeyType.MasterDoor:
                        tutorialText.text = itemGame.description;
                        DirectorManager.instance.player.masterKeyNotFound = false;
                        DirectorManager.instance.UpdatePlayerProgressScore(55);
                    DirectorManager.instance.enemy.enemyState = 6;
                        break;
                    default:
                        tutorialText.text = null;

                        break;

                }

            }
        
        Destroy(interactableItem);
        DirectorManager.instance.inventory.AddItem(itemGame);
       
        DirectorManager.instance.slotsInventory.UpdateSlots();
       
        Reset();
    }

    //Activates the stun button
    public void StunEnemy()
    {
        tutorialText.text = null;
        DirectorManager.instance.audioManager.Play(ConstantVariables.PLAYER_ATTACK_SFX);
        DirectorManager.instance.audioManager.Play(ConstantVariables.ENEMY_HURT_SFX);
        DirectorManager.instance.enemy.Damage(1);
        DirectorManager.instance.inventory.RemoveWeapon();
        DirectorManager.instance.weaponMesh.enabled = DirectorManager.instance.inventory.HasWeaponEquipped();
        DirectorManager.instance.playerHoldingWeaponMesh.enabled = DirectorManager.instance.inventory.HasWeaponEquipped();
        DirectorManager.instance.slotsInventory.UpdateSlots();
        Reset();
    
    }
    IEnumerator ResetTutorialText()
    {
        yield return new WaitForSeconds(2);
        tutorialText.text = null;
    }

    public void OpenDoor()
    {
        if(itemDoor != null)
        {
           
            if (!itemDoor.isLocked)
            {

                itemDoor.OpenDoor();
                Reset();
                
            }
            else
            {
                //Tutorial
                if (itemDoor.keyType == KeyType.MasterDoor)
                {
                    if (DirectorManager.instance.player.progressScore == 0)
                    {
                       
                        DirectorManager.instance.audioManager.Play(ConstantVariables.DOOR_UNLOCKING_SFX);
                        DirectorManager.instance.audioManager.Play(ConstantVariables.FIND_KEY_SFX);
                        playerBathroomDoor.isLocked = false;
                        backgroundAudio.Play();
                        DirectorManager.instance.UpdatePlayerProgressScore(10);
                        tutorialText.text = ConstantVariables.TUTORIAL_DISCOVERING_MASTER_DOOR_LOCKED;
                    }
                }

                if (DirectorManager.instance.inventory.RetrieveAnyExistingKey() != null)
                {
                    
                    Key keyItem = DirectorManager.instance.inventory.RetrieveAnyExistingKey();
                    
                    if(keyItem.roomType == itemDoor.keyType)
                    {
                        tutorialText.text = null;

                        switch (keyItem.roomType)
                        {
                            case KeyType.BathroomDoor:
                                pauseButton.gameObject.SetActive(true);
                               
                                DirectorManager.instance.player.finishedTutorial = true;
                                DirectorManager.instance.audioManager.Play(ConstantVariables.WAILING_SFX);
                                DirectorManager.instance.audioManager.Play(ConstantVariables.PLAYER_WHO_IS_THERE);
                                DirectorManager.instance.player.bathroomDoorLocked = false;
                                DirectorManager.instance.UpdatePlayerProgressScore(10);
                                DirectorManager.instance.enemy.finishedTutorial = true;
                                DirectorManager.instance.inventory.RemoveKey();
                                break;
                            case KeyType.ChildhoodRoomDoor:
                                DirectorManager.instance.player.childhoodDoorLocked = false;
                                DirectorManager.instance.UpdatePlayerProgressScore(20);
                               
                                DirectorManager.instance.inventory.RemoveKey();
                                break;
                            case KeyType.KitchenDoor:
                               
                                DirectorManager.instance.player.kitchenDoorLocked = false;
                                laundryDoor.isLocked = false;
                                DirectorManager.instance.UpdatePlayerProgressScore(30);
                                DirectorManager.instance.inventory.RemoveKey();
                                break;
                            case KeyType.DiningRoomDoor:
                           
                                DirectorManager.instance.player.diningDoorLocked = false;
                                DirectorManager.instance.UpdateMiddleDoor();
                                DirectorManager.instance.UpdatePlayerProgressScore(40);
                                DirectorManager.instance.inventory.RemoveKey();
                                break;
                            case KeyType.StudyRoomDoor:
                              
                                DirectorManager.instance.player.studyDoorLocked= false;
                                DirectorManager.instance.UpdatePlayerProgressScore(50);
                                DirectorManager.instance.inventory.RemoveKey();
                                break;
                            case KeyType.MasterDoor:
                                DirectorManager.instance.player.masterDoorLocked = false;
                                DirectorManager.instance.UpdatePlayerProgressScore(100);
                                DirectorManager.instance.inventory.RemoveKey();
                                wonPanel.localScale = new Vector3(1, 1, 1);
                                DirectorManager.instance.enemy.RelocateAI(41.17f, 0f, 187.7f, 0, 0, 0);
                                DirectorManager.instance.enemy.navAgent.isStopped = true;

                                break;
                            default:
                                Debug.Log("player used key " + keyItem.roomType.ToString() + "with " + itemDoor.doorName + " and couldn't unlock door.");
                                break;

                        }

                        DirectorManager.instance.audioManager.Play(ConstantVariables.DOOR_UNLOCKING_SFX);
                        itemDoor.isLocked = false;
                       
                        DirectorManager.instance.slotsInventory.UpdateSlots();
                        itemDoor.OpenDoor();
                        Reset();
                    }
                    else
                    {
                        DirectorManager.instance.audioManager.Play(ConstantVariables.DOOR_LOCKED_SFX);
                    }
                }
                else
                {

                    DirectorManager.instance.audioManager.Play(ConstantVariables.DOOR_LOCKED_SFX);
                }
            }
        }
        else
        {
            Debug.Log("itemDoor is null");
        }
       
    }

    public void CloseDoor()
    {
        if (itemDoor != null)
        {
            itemDoor.CloseDoor();
            Reset();
        }
        else
        {
            Debug.Log("itemDoor is null");
        }

    }

}
