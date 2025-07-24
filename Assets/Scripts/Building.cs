using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет очередью клиентов у здания.
/// Не управляет поведением клиентов — только меняет их состояние.
/// </summary>
public class Building : MonoBehaviour
{
    // Очередь клиентов у здания
    public Queue<ClientAI> waitingClients = new Queue<ClientAI>();

    // Максимальная длина очереди
    public int maxQueueLength = 5; // Максимальное количество клиентов в очереди

    public int bonusClients = 5; // Количество клиентов, которые добавляет здание при постройке, применимо для аттракционов

    /// <summary>
    /// Клиент пытается занять место в очереди у здания.
    /// </summary>
    /// <param name="client">Клиент, который подошёл к зданию</param>
    /// <returns>
    /// true — клиент добавлен в очередь;
    /// false — очередь полна, клиент не добавлен
    /// </returns>
    public bool TryEnterQueue(ClientAI client)
    {
        if (waitingClients.Count < maxQueueLength)
        {
            waitingClients.Enqueue(client); // добавляем клиента в очередь
            return true;
        }
        else
        {
            // очередь переполнена — клиент не попадает в очередь
            return false;
        }
    }

    /// <summary>
    /// Удаляет клиента из очереди.
    /// </summary>
    /// <param name="client">Клиент для удаления</param>
    public void RemoveFromQueue(ClientAI client)
    {
        // Формируем новую очередь без удаляемого клиента
        Queue<ClientAI> newQueue = new Queue<ClientAI>();
        foreach (var c in waitingClients)
        {
            if (c != client)
                newQueue.Enqueue(c);
        }
        waitingClients = newQueue;
    }

    /// <summary>
    /// Обслуживает первого клиента в очереди (по клику игрока).
    /// Не вызывает методов у клиента! Только меняет свойство serviceState.
    /// </summary>
    public void ServeNextClient()
    {
        if (waitingClients.Count > 0)
        {
            ClientAI client = waitingClients.Peek(); // Получаем первого в очереди
            client.serviceState = ServiceState.Served; // Сигнализируем клиенту об обслуживании

            // Всё остальное (счастье, таймер, движение и удаление из очереди) клиент делает САМ!
        }
    }
}
