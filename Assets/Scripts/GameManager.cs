using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType { None, Shop, Attraction }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scene References")]
    public GameObject shopPrefab;
    public GameObject attractionPrefab;
    public GameObject clientPrefab;
    public GameObject cellPrefab;

    public Transform entryPoint;
    public Transform exitPoint;
    public Transform ground;

    [Header("Grid Settings")]
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float cellSize = 2f;

    [Header("Client Settings")]
    public int clientsPerMinute = 10;

    [Header("UI")]
    public UIManager uiManager;

    private GameObject[,] gridCells;
    private BuildingType pendingBuildingType = BuildingType.None;

    private int money = 0;
    private float totalHappiness = 0;
    private int happinessCount = 0;
    private int clientsServed = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Entry и Exit по центру верхней и нижней стороны Ground
        Vector3 groundPos = ground.position;
        Vector3 groundSize = ground.GetComponent<Renderer>().bounds.size;

        entryPoint.position = new Vector3(groundPos.x, 0, groundPos.z + groundSize.z / 2f);
        exitPoint.position = new Vector3(groundPos.x, 0, groundPos.z - groundSize.z / 2f);

        SpawnGridCells();
        UpdateClientSpawnRate();
        StartCoroutine(ClientSpawnLoop());

        uiManager.UpdateUI();
    }

    void Update()
    {
        if (pendingBuildingType != BuildingType.None && Input.GetMouseButtonDown(0))
        {
            TryPlaceBuildingAtMouse();
        }
    }

    void SpawnGridCells()
    {
        gridCells = new GameObject[gridWidth, gridHeight];
        Vector3 startPos = ground.position - new Vector3(gridWidth / 2f * cellSize, 0, gridHeight / 2f * cellSize);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 pos = startPos + new Vector3(x * cellSize, 0, z * cellSize);
                GameObject cell = Instantiate(cellPrefab, pos, Quaternion.identity);
                gridCells[x, z] = cell;
            }
        }
    }

    IEnumerator ClientSpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f / clientsPerMinute);
            SpawnClient();
        }
    }

    void SpawnClient()
    {
        GameObject clientGO = Instantiate(clientPrefab, entryPoint.position, Quaternion.identity);
        ClientAI ai = clientGO.GetComponent<ClientAI>();
        if (ai == null)
        {
            Debug.LogError("На клиенте отсутствует компонент ClientAI!");
            return;
        }
        ai.GoToExit();
    }

    public void PrepareToPlaceBuilding(BuildingType type)
    {
        pendingBuildingType = type;
    }

    void TryPlaceBuildingAtMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject cell = hit.collider.gameObject;
            if (cell.CompareTag("Cell") && cell.transform.childCount == 0)
            {
                GameObject prefab = pendingBuildingType == BuildingType.Shop ? shopPrefab : attractionPrefab;
                GameObject building = Instantiate(prefab, cell.transform.position, Quaternion.identity);
                building.transform.localScale *= 0.8f; // чуть меньше, чем клетка
                building.transform.SetParent(cell.transform);

                pendingBuildingType = BuildingType.None;
            }
        }
    }

    public void AddMoney(int amount)
    {
        money += amount;
        uiManager.UpdateUI();
    }

    public void RegisterClientHappiness(float happiness)
    {
        totalHappiness += happiness;
        happinessCount++;
        uiManager.UpdateUI();
    }

    public void RegisterClientServed()
    {
        clientsServed++;
        uiManager.UpdateUI();
    }

    public int GetMoney() => money;
    public float GetAverageHappiness() => happinessCount == 0 ? 0 : totalHappiness / happinessCount;
    public int GetClientsServed() => clientsServed;
    public int GetTotalClientsPerMinute() => clientsPerMinute;
    public void UpdateClientSpawnRate() => uiManager.UpdateUI();

    public Transform FindRandomBuildingOfType(BuildingType type)
    {
        Building[] all = FindObjectsByType<Building>(FindObjectsSortMode.None);
        List<Transform> candidates = new();

        foreach (var b in all)
        {
            if (b.buildingType == type)
            {
                candidates.Add(b.transform);
            }
        }

        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

}
