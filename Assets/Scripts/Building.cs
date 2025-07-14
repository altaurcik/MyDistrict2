using UnityEngine;
using System.Collections.Generic;

public class Building : MonoBehaviour
{
    public BuildingType buildingType;  // ← добавлено поле

    public int maxQueueLength = 3;
    private Queue<ClientAI> queue = new();

    void Update()
    {
        if (queue.Count > 0)
        {
            var client = queue.Peek();
            if (!client.isServed && !client.isAngry)
            {
                ServeClient(client);
            }
        }
    }

    public void EnqueueClient(ClientAI client)
    {
        if (!queue.Contains(client))
        {
            queue.Enqueue(client);
        }
    }

    public int GetQueueSize() => queue.Count;

    void ServeClient(ClientAI client)
    {
        client.isServed = true;
        queue.Dequeue();

        GameManager.Instance.AddMoney(50);
        GameManager.Instance.RegisterClientServed();
        GameManager.Instance.RegisterClientHappiness(1f);

        client.GoToExit();
    }
}
