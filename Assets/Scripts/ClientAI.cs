using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public enum ServiceState
{
    None,       // Клиент еще ничего не делает
    Going, // Двигается к зданию
    Waiting,    // Стоит в очереди
    Served,     // Был обслужен (кликом игрока)
    Leaving     // Двигается к выходу
}

public class ClientAI : MonoBehaviour
{
    public Transform entryPoint; // точка старта
    public Transform exitPoint;  // точка выхода
    public ServiceState serviceState = ServiceState.None; // Текущее состояние клиента
    public bool isAttractionClient = false;        // Призван ли клиент аттракционом
    private List<Building> targets = new List<Building>(); // Список целей
    private int currentTargetIndex = 0; // Индекс текущей цели
    private NavMeshAgent agent;
    private float waitTimer = 0f; // Таймер ожидания в очереди
    public float happiness = 0f; // Уровень счастья клиента
    public float maxWaitTime = 5f; // Максимальное время ожидания в очереди

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        transform.position = entryPoint.position;
        // Помещаем клиента в точку входа
        SetTargetsList(); // Устанавливаем список целей
        GoToNextTarget(); // Двигаемся к первой цели
    }

    void Update()
    {
        //Debug.Log("Состояние клиента: " + serviceState);

        if (serviceState == ServiceState.Going)
        {
            // Если клиент движется к зданию, проверяем, достиг ли он цели
            if (!agent.pathPending && agent.remainingDistance <= 1f)
            {
                Building building = targets[currentTargetIndex - 1]; // Получаем текущее здание из списка целей
                bool enteredQueue = building.TryEnterQueue(this); // Пытаемся занять очередь в здании

                if (enteredQueue)
                {
                    serviceState = ServiceState.Waiting; // Меняем состояние на "Waiting" 
                    Debug.Log($"Клиент {gameObject.name} занял очередь в {building.gameObject.tag}");
                    waitTimer = 0f; // Сбрасываем таймер ожидания

                }
                else
                {
                    // Если очередь полна, клиент злится и уходит
                    ChangeHappiness(-1);
                    Debug.Log($"Клиент {gameObject.name} не попал в очередь в {building.gameObject.tag} и уходит");
                    building.RemoveFromQueue(this); // Удаляем клиента из очереди
                    GoToNextTarget(); // Переходим к следующей цели
                }
            }
        }

        if (serviceState == ServiceState.Waiting)
        {
            // Если клиент в очереди, увеличиваем таймер ожидания
            waitTimer += Time.deltaTime;

            if (waitTimer >= maxWaitTime) // Если ждал больше 5 секунд
            {
                ChangeHappiness(-1); // Уменьшаем счастье
                Debug.Log($"Клиент {gameObject.name} злится от долгого ожидания и уходит");
                GoToNextTarget(); // Переходим к следующей цели
            }

        }

        // Если клиент был обслужен (кликом игрока)
        if (serviceState == ServiceState.Served)
        {
            ChangeHappiness(1); // Увеличиваем счастье
            Building building = targets[currentTargetIndex - 1]; // Получаем здание, в котором обслужили
            building.RemoveFromQueue(this); // Удаляем клиента из очереди
            Debug.Log($"Клиент {gameObject.name} был обслужен и уходит");
            GoToNextTarget(); // Переходим к следующей цели     
        }

        if (serviceState == ServiceState.Leaving)
        {
            // Если клиент уходит, проверяем, достиг ли он точки выхода
            if (!agent.pathPending && agent.remainingDistance <= 0.3f)
            {
                Debug.Log($"Клиент {gameObject.name} покидает парк");
                Destroy(gameObject); // Удаляем клиента из игры
            }
        }

    }

    void SetTargetsList()
    {
        targets.Clear();
        Building atraction = FindBuildingWithTag("Atraction");
        Building shop = FindBuildingWithTag("Shop");
        if (atraction != null && isAttractionClient)
            targets.Add(atraction); // Добавляем аттракцион в список целей

        if (shop != null)
            targets.Add(shop); // Добавляем магазин в список целей

        targets.Add(null); // последний пункт — выход

        Debug.Log("Число целей: " + targets.Count);

    }

    Building FindBuildingWithTag(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag); // Находим все объекты с этим тэгом
        Debug.Log($"Найдено {objects.Length} зданий с тэгом {tag}"); 

        if (objects.Length == 0)
            return null; // Нет зданий с таким тэгом

        int idx = Random.Range(0, objects.Length); // Случайный индекс в массиве
        return objects[idx].GetComponent<Building>(); // Возвращаем Building случайного объекта
    }

    public void GoToNextTarget()
    {
        if (currentTargetIndex < targets.Count)
        {
            Building target = targets[currentTargetIndex];

            if (target != null)
            {
                // Двигаемся к зданию
                serviceState = ServiceState.Going; // Присваиваем состояние "Двигается"
                agent.SetDestination(target.transform.position);
                currentTargetIndex++; // Переходим к следующей цели на следующий вызов
                Debug.Log($"Цель {currentTargetIndex}: здание {target.gameObject.tag} — {target.transform.position}");
            }
            else
            {
                // Если цель null — это выход
                agent.SetDestination(exitPoint.position);
                serviceState = ServiceState.Leaving; // Присваиваем состояние "Уходит"
                Debug.Log($"Цель {currentTargetIndex}: выход — {exitPoint.position}");
            }

        }
    }

    void ChangeHappiness(int value)
    {
        happiness += value; // Изменяем уровень счастья
    }

}

