using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/* continue using this to test how to make the AI more organic, already started on creating classes for each 'state' */

public class EnemyController : MonoBehaviour
{

    [SerializeField]
    private float defaultSightDistance = 12f;
    [SerializeField]
    private float attackDistance = 1f;
    [SerializeField]
    private float attackCooldown = 3f;
    [SerializeField]
    private Text stunnedEnemySeconds;

    //Enemy Stats
    [HideInInspector]
    public float currentHealth = 1000000000000f;
    [HideInInspector]
    public int currentAttackDamage = 1; 

    //NavMeshAgent Variables
    [HideInInspector]
    public NavMeshAgent navAgent;
    private GameObject playerObject;
    private Transform playerTarget;
    [HideInInspector]
    public Vector3 pointTarget;

    private float currentHitDistance;
    private float currentSightDistance;

    //AI State Variables
    [HideInInspector]
    public int enemyState = -1;
    private float waitedTime = 0f;
    private float timeToWait = 0f;
    private float lastTimeAttacked;

    //Player Variables
    private PlayerHealth playerHealth;
   
    //AI Point Variables
    private int currentAIPoint;
    private List<GameObject> aiPoints;

    public int stunCountdown = 3;

    //Variables that update by triggers through director
    [HideInInspector]
    public bool gameIsPaused = false;
    [HideInInspector]
    public int playerProgress;
    [HideInInspector]
    public int playerFear;
    [HideInInspector]
    public bool finishedTutorial = false;


    // Start is called before the first frame update
    void Start()
    {
        stunnedEnemySeconds.gameObject.SetActive(false);
        navAgent = GetComponent<NavMeshAgent>();
        aiPoints = DirectorManager.instance.AIPoints();
        playerTarget = DirectorManager.instance.playerObject.transform;
        playerHealth = DirectorManager.instance.playerHealth;

        //Checking player progress to determine state
        if (DirectorManager.instance.player.finishedTutorial == true)
        {
            finishedTutorial = true;
            if (DirectorManager.instance.player.progressScore > 55)
            {
                enemyState = 6;
            }
            else
            {
                enemyState = 0;
            }
        }    
    }

    private void Update()
    {
        if (finishedTutorial)
        {
            if (!gameIsPaused)
            {
                switch (enemyState)
                {
                    case 0:
                        IdleSDefaultState();
                        break;
                    case 1:
                        IdleSAggresiveState();
                        break;
                    case 2:
                        WonderState();
                        break;
                    case 3:
                        ChaseState();
                        break;
                    case 4:
                        AttackState();
                        break;
                    case 5:
                        StunnedState();
                        break;
                    case 6:
                        SeekState();
                        break;
                    default:
                        Debug.LogError("Out of range :" + enemyState.ToString());
                        break;
                }
            }
        }
    }

    //The AI will stop for a while to "check" its surroundings
    private void IdleSDefaultState()
    {
       
        if (navAgent.isStopped == false)
        {
            navAgent.isStopped = true;
            waitedTime = 0f;
            timeToWait = Random.Range(3, 7);

        }
        waitedTime += Time.deltaTime;

        if (waitedTime > timeToWait)
        {
            enemyState = 2;
        }
    }

    
    private void IdleSAggresiveState()
    {
        if (navAgent.isStopped == false)
        {
            navAgent.isStopped = true;
            waitedTime = 0f;
            timeToWait = Random.Range(2, 5);

        }
        waitedTime += Time.deltaTime;

        if (waitedTime > timeToWait)
        {
            enemyState = 2;
        }
    }

    //The AI will wonder to the next point 
    private void WonderState()
    {
        if (navAgent.isStopped)
        {
            navAgent.isStopped = false;
            NewDestination();
            navAgent.destination = pointTarget;
        }


        if (Vector3.Distance(pointTarget, transform.position) <= navAgent.stoppingDistance)
        {
            enemyState = 0;
            
        }

        if (PlayerInSightDistance())
        {
            enemyState = 3;
        }
    }

    //If the enemy detects the Player, it will chase it
    private void ChaseState()
    {
        if (!PlayerInSightDistance() || DirectorManager.instance.player.isHidding)
        { 
            navAgent.isStopped = true;
            enemyState = 0;
            return;
        }
        else
        {
            navAgent.destination = playerTarget.transform.position;
            EnemyFacePlayer();

            if (currentHitDistance < attackDistance && Time.time - lastTimeAttacked > attackCooldown)
            {
                navAgent.isStopped = true;
                enemyState = 4;
            }
        }
    }

    //If the Enemy is within the attackDistance, it will start attacking the player
    private void AttackState()
    {
        DirectorManager.instance.audioManager.Play(ConstantVariables.PLAYER_HURT_SFX);
        bool playerIsDead = playerHealth.ReceivedDamage(currentAttackDamage);
        lastTimeAttacked = Time.time;
        if (playerIsDead)
        {
            DirectorManager.instance.audioManager.Play(ConstantVariables.PLAYER_HURT_SFX);
            playerTarget = null;
            navAgent.isStopped = true;
            //temporary solution, but make sure when you implement new AI mechanics to look up best ways to do this
            RelocateAI(41.17f, 0f, 187.7f, 0, 0, 0);
        }
        else
        {
            enemyState = 2;
        }
        
    }

    //If the Enemy is stunned by the player, it will become idle
    private void StunnedState()
    {
       
        stunnedEnemySeconds.gameObject.SetActive(true);

        if (navAgent.isStopped == false)
        {
            navAgent.isStopped = true;
            waitedTime = 0f;
            timeToWait = 10;

        }

        StartCoroutine(CountdownStun());
    }


    //When the player has the last key, this will be the AI's state
    private void SeekState()
    {
        DirectorManager.instance.audioManager.Play(ConstantVariables.ENEMY_LAUGH_SFX);
        DirectorManager.instance.audioManager.Play(ConstantVariables.PLAYER_BREATHING_TWO_SFX);
        navAgent.destination = playerTarget.transform.position;
        navAgent.speed = 4;
        EnemyFacePlayer();

        if (currentHitDistance < attackDistance && Time.time - lastTimeAttacked > attackCooldown)
        {
            navAgent.isStopped = true;
            enemyState = 4;
        }
    }

    //Updates the AI's health after receiving damage
    public bool Damage(float damage)
    {
        navAgent.isStopped = false;
        enemyState = 5;
        currentHealth -= damage;
        if(currentHealth == 0)
        {
            //Enemy is dead -- create different ending?
            return true;
        }
        return false;
    }

    

    //Checks if the player is within the AI's sight radius
    private bool PlayerInSightDistance()
    {
        int layerMask = 1 << 11;
        layerMask = ~layerMask;
        if (Physics.Raycast(transform.position, GetPlayerDirection() + Vector3.up, out RaycastHit raycastHitInfo, currentSightDistance, layerMask))
        {
            if (raycastHitInfo.collider.gameObject.name == ConstantVariables.PLAYER)
            {
                playerObject = raycastHitInfo.transform.gameObject;
                currentHitDistance = raycastHitInfo.distance;
                currentSightDistance = 18f; //increase the AI's chances to chase player longer, might want to figure out a more dynamic way to improve this
                return true;
            }
            else
            {
                currentSightDistance = defaultSightDistance;
                playerObject = null;
                return false;
            }
        }
        else
        {
            currentSightDistance = defaultSightDistance;
            currentHitDistance = currentSightDistance;
            playerObject = null;
            return false;
        }
    }

    //Creates the next pointTarget for the AI
    private void NewDestination()
    {
        currentAIPoint = Random.Range(0, aiPoints.Count);
        pointTarget = aiPoints[currentAIPoint].transform.position;
    }

    //Gets the distance between the playerTarget if it's still alive
    private Vector3 GetPlayerDirection()
    {
        if (playerTarget != null)
        {
            return playerTarget.transform.position - transform.position;
        }
        return transform.forward;
    }

    //Faces the AI to the Player
    private void EnemyFacePlayer()
    {
        Vector3 direction = (playerTarget.transform.position - transform.position).normalized;
        Quaternion enemyLookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, enemyLookRotation, Time.deltaTime * 5f);
    }


    /*Stun display feedback*/
    IEnumerator CountdownStun()
    {
        while (timeToWait > 0)
        {
            stunnedEnemySeconds.text = timeToWait.ToString();
            yield return new WaitForSeconds(1f);

            timeToWait--;
        }

        stunnedEnemySeconds.gameObject.SetActive(false);
        enemyState = 3;
    }

    public void EnemyStunnedTime(float seconds)
    {
        if (seconds > 0)
        {
            seconds -= Time.deltaTime;

            if (seconds < 0)
            {
                seconds = 0f;
                stunnedEnemySeconds = null;
            }
            
            int initialStunTime = (int)seconds;
            int milliseconds = (int)(seconds * 100);
            milliseconds %= 100;

            stunnedEnemySeconds.text = string.Format("{0:00}:{1:00}", initialStunTime, milliseconds);
        }

        DirectorManager.instance.audioManager.Stop(ConstantVariables.ENEMY_HURT_SFX);
    }


    public void RelocateAI(float positionX, float positionY, float positonZ, float rotationX, float rotationY, float rotationZ)
    {
        Vector3 position;
        Vector3 rotation;
        position.x = positionX;
        position.y = positionY;
        position.z = positonZ;
        rotation.x = rotationX;
        rotation.y = rotationY;
        rotation.z = rotationZ;
        transform.position = position;
        transform.eulerAngles = rotation;
    }

}

[System.Serializable]
public class EnemySettings
{
    public float currentHealth;
    public int currentAttackDamage;
    public int enemyState;
    public float[] position;
    public float[] rotation;
    public float[] lastTargetPoint;
   

    public EnemySettings(EnemyController enemy)
    {
        currentHealth = enemy.currentHealth;
        currentAttackDamage = enemy.currentAttackDamage;
        enemyState = enemy.enemyState;
        position = new float[3];
        position[0] = enemy.transform.position.x;
        position[1] = enemy.transform.position.y;
        position[2] = enemy.transform.position.z;
        rotation = new float[3];
        rotation[0] = enemy.transform.localRotation.eulerAngles.x;
        rotation[1] = enemy.transform.localRotation.eulerAngles.y;
        rotation[2] = enemy.transform.localRotation.eulerAngles.z;
        lastTargetPoint = new float[3];
        lastTargetPoint[0] = enemy.pointTarget.x;
        lastTargetPoint[1] = enemy.pointTarget.y;
        lastTargetPoint[2] = enemy.pointTarget.z;
    }

}
