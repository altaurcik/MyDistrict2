using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Синглтон для доступа из других скриптов
    public GameObject clientPrefab;
    public Transform ground;         // Сам объект Ground (куб/плоскость)
    public Transform entryPoint;     // Точка входа
    public Transform exitPoint;      // Точка выхода

    public GameObject cellPrefab;    // Префаб клетки для размещения зданий
    public int cellsX = 5;           // Количество клеток по X
    public int cellsZ = 3;           // Количество клеток по Z

    public int clientsPerMinute = 10; // сколько клиентов в минуту
    public int clientsPerMinuteBase = 10; // базовое количество клиентов в минуту, можно менять в UI
    public int clientsPerMinuteAtraction = 0; // количество клиентов в минуту для аттракционов
    private float spawnInterval;      // интервал между спавном клиентов (секунды)
    private float spawnTimer = 0f;    // таймер для спавна

    void Awake() // Синглтон для GameManager
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // если нужно между сценами
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        SetEntryAndExitPoints(); // Устанавливаем точки входа и выхода
        SpawnClient(); // Спавним первого клиента сразу
        SpawnGridCells(); // Спавним сетку клеток для зданий
    }

    void Update()
    {
        // Вычисляем интервал спавна клиентов (секунды) исходя из желаемого количества клиентов в минуту
        spawnInterval = 60f / (clientsPerMinuteBase + clientsPerMinuteAtraction);

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnClient();
            spawnTimer = 0f;
        }
    }

    // Устанавливаем точки входа и выхода по центру верхней и нижней стороны ground
    void SetEntryAndExitPoints()
    {
        Vector3 groundPos = ground.position;
        Vector3 groundSize = ground.GetComponent<Renderer>().bounds.size;

        // Вход — середина верхнего края (по Z)
        entryPoint.position = new Vector3(groundPos.x, groundPos.y, groundPos.z + groundSize.z / 2f);
        // Выход — середина нижнего края (по Z)
        exitPoint.position = new Vector3(groundPos.x, groundPos.y, groundPos.z - groundSize.z / 2f);
    }

    // Спавн клиента в точке входа
    void SpawnClient()
    {
        GameObject client = Instantiate(clientPrefab, entryPoint.position, Quaternion.identity);
        ClientAI clientAI = client.GetComponent<ClientAI>();

        float pBase = (float)clientsPerMinuteBase / (clientsPerMinuteBase + clientsPerMinuteAtraction ); // Процент базовых клиентов
        if (Random.value < pBase)
        {
            clientAI.isAttractionClient = false; // Базовый клиент
            Debug.Log("Спавн базового клиента");
        }
        else
        {
            clientAI.isAttractionClient = true; // Клиент аттракциона
            Debug.Log("Спавн клиента аттракциона");
        }
        
        // Устанавливаем точки входа и выхода для клиента
        clientAI.entryPoint = entryPoint;
        clientAI.exitPoint = exitPoint;
    }

    public void ChangeClientsAtractionPerMinute(int newValue)
    {
        clientsPerMinuteAtraction += newValue; // Добавляем или убираем клиентов в минуту
        Debug.Log($"Количество привлеченных клиентов в минуту изменено на: {clientsPerMinute}");
    }

    // Генерация сетки клеток для зданий на ground
    void SpawnGridCells()
    {
        // Получаем позицию центра и размер ground
        Vector3 groundPos = ground.position;
        Vector3 groundSize = ground.GetComponent<Renderer>().bounds.size;
        // Вычисляем, какой размер должна иметь каждая клетка по X и Z (делим ground на количество клеток)
        float cellSizeX = groundSize.x / cellsX;
        float cellSizeZ = groundSize.z / cellsZ;

        // Получаем реальный размер (в юнитах) у cellPrefab (на случай, если он не 1x1)
        Vector3 prefabSize = cellPrefab.GetComponent<Renderer>().bounds.size;

        // Обходим все клетки по X
        for (int x = 0; x < cellsX; x++)
        {
            // ...и по Z
            for (int z = 0; z < cellsZ; z++)
            {
                // Считаем координату центра очередной клетки
                float cellPosX = groundPos.x - groundSize.x / 2f + cellSizeX * (x + 0.5f);
                float cellPosZ = groundPos.z - groundSize.z / 2f + cellSizeZ * (z + 0.5f);
                // Смещаем чуть выше ground, чтобы не было артефактов рендера
                Vector3 cellPos = new Vector3(cellPosX, groundPos.y + 0.01f, cellPosZ);

                // Создаём клетку
                GameObject cell = Instantiate(cellPrefab, cellPos, Quaternion.identity);

                // Считаем коэффициенты масштабирования по X и Z, чтобы cellPrefab встал ровно в нашу сетку, и уменьшаем дополнительно на 0.8 для зазора
                float scaleX = (cellSizeX / prefabSize.x) * 0.8f;
                float scaleZ = (cellSizeZ / prefabSize.z) * 0.8f;
                // Применяем масштаб, чтобы клетка ровно вписалась по ширине и глубине
                cell.transform.localScale = new Vector3(
                    cell.transform.localScale.x * scaleX,
                    cell.transform.localScale.y,
                    cell.transform.localScale.z * scaleZ
                );
            }
        }
    }
}