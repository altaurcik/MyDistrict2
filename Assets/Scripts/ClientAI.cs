// ClientAI.cs
using UnityEngine;
using UnityEngine.AI;

public class ClientAI : MonoBehaviour
{
    public bool isSummoned = false;
    public bool visitedAttraction = false;
    public bool visitedShop = false;
    public Transform targetBuilding;
    public bool isWaitingInQueue = false;
    public bool isServed = false;
    public int happiness = 1;

    private NavMeshAgent agent;
    public bool isAngry = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void MoveTo(Transform target)
    {
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent не найден у клиента!");
            return;
        }

        targetBuilding = target;
        agent.SetDestination(target.position);
    }

    void Update()
    {

        if (isAngry || isWaitingInQueue || isServed)
        {
            float distanceToExit = Vector3.Distance(transform.position, GameManager.Instance.exitPoint.position);
            Debug.Log($"[CLIENT] status: angry={isAngry}, served={isServed}, waiting={isWaitingInQueue}, distToExit={distanceToExit}");

            if (distanceToExit < 1f)
            {
                Debug.Log("[CLIENT] Destroyed due to close to exit.");
                Destroy(gameObject);
            }
            return;
        }

    // остальная логика


        if (targetBuilding == null) return;

        float distance = Vector3.Distance(transform.position, targetBuilding.position);
        if (distance < 2f)
        {
            CheckQueueAtDistance();
        }
    }


    public void CheckQueueAtDistance()
    {
        Building building = targetBuilding.GetComponent<Building>();
        if (building == null) return;

        if (building.GetQueueSize() >= building.maxQueueLength)
        {
            BecomeAngryAndLeave();
        }
        else
        {
            building.EnqueueClient(this);
            isWaitingInQueue = true;
        }
    }

    public void BecomeAngryAndLeave()
    {
        happiness = 0;
        isAngry = true;
        GoToExit();
    }

    public void GoToExit()
    {
        GameManager.Instance.RegisterClientHappiness(happiness);
        agent.SetDestination(GameManager.Instance.exitPoint.position);
        Destroy(gameObject, 5f);
    }
}
