using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIManager управляет интерфейсом и логикой покупки зданий.
/// </summary>
public class UIManager : MonoBehaviour
{
    // Кнопка Buy на Canvas
    public Button buyButton; // Кнопка для открытия меню выбора здания
    // Панель с выбором здания (Shop/Atraction)
    public GameObject buyMenuPanel; // Панель с двумя кнопками
    // Кнопка покупки магазина
    public Button shopBuyButton; // Кнопка для выбора магазина
    // Кнопка покупки аттракциона
    public Button atractionBuyButton; // Кнопка для выбора аттракциона

    // Префабы для размещения
    public GameObject shopPrefab; // Префаб магазина
    public GameObject atractionPrefab; // Префаб аттракциона

    // Флаг — ждём ли клик для размещения здания
    private bool isPlacingBuilding = false;
    // Тип выбранного здания
    private string selectedBuildingType = "";

    // Start вызывается при запуске сцены
    private void Start()
    {
        buyMenuPanel.SetActive(false); // Скрываем панель выбора здания
        buyButton.onClick.AddListener(OnBuyButtonClicked); // Добавляем обработчик клика на Buy
        shopBuyButton.onClick.AddListener(OnShopBuyButtonClicked); // Обработчик для магазина
        atractionBuyButton.onClick.AddListener(OnAtractionBuyButtonClicked); // Обработчик для аттракциона
    }

    // Открыть панель с выбором здания
    private void OnBuyButtonClicked()
    {
        buyMenuPanel.SetActive(true); // Показываем меню выбора
    }

    // Клик по кнопке "Магазин"
    private void OnShopBuyButtonClicked()
    {
        selectedBuildingType = "Shop"; // Запоминаем выбор
        isPlacingBuilding = true; // Активируем режим размещения
        buyMenuPanel.SetActive(false); // Скрываем меню
    }

    // Клик по кнопке "Аттракцион"
    private void OnAtractionBuyButtonClicked()
    {
        selectedBuildingType = "Atraction"; // Запоминаем выбор
        isPlacingBuilding = true; // Активируем режим размещения
        buyMenuPanel.SetActive(false); // Скрываем меню
    }

    // Update вызывается каждый кадр
    private void Update()
    {
        // Если активен режим размещения здания и был клик мышкой
        if (isPlacingBuilding && Input.GetMouseButtonDown(0))
        {
            // Проверяем, что не кликнули по элементу UI
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                // Формируем луч (Ray) из камеры по позиции мышки
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                // Проверяем, попал ли луч в объект с Collider
                if (Physics.Raycast(ray, out hit))
                {
                    // ПРОВЕРКА: клик именно по клетке
                    if (!hit.collider.CompareTag("Cell"))
                    {
                        return; // Клик был не по клетке
                    }

                    Vector3 placePosition = hit.collider.transform.position;
                    GameObject prefab = null;

                    // Размер клетки
                    Vector3 cellSize = Vector3.one;
                    if (hit.collider is BoxCollider box)
                    {
                        cellSize = Vector3.Scale(box.size, box.transform.lossyScale);
                    }
                    else
                    {
                        cellSize = hit.collider.bounds.size;
                    }

                    if (selectedBuildingType == "Shop")
                    {
                        prefab = shopPrefab;
                    }
                    else if (selectedBuildingType == "Atraction")
                    {
                        prefab = atractionPrefab;
                    }

                    if (prefab != null)
                    {
                        GameObject building = Instantiate(prefab, placePosition, Quaternion.identity);
                        Vector3 originalScale = prefab.transform.localScale;
                        building.transform.localScale = new Vector3(cellSize.x, originalScale.y, cellSize.z);

                        // Делаем клетку непроходимой для клиентов
                        hit.collider.enabled = false;
                        
                        if (GameManager.Instance == null)
                            Debug.LogError("GameManager.Instance is NULL!");

                        // Если это аттракцион — увеличиваем число клиентов
                        if (selectedBuildingType == "Atraction")
                        {
                            var atractionComp = building.GetComponent<Building>(); // Или Atraction, если отдельный класс!
                            if (atractionComp != null)
                            {
                                GameManager.Instance.ChangeClientsAtractionPerMinute(atractionComp.bonusClients);
                            }
                        }
                    }


                    isPlacingBuilding = false;
                    selectedBuildingType = "";
                }

            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Building building = hit.collider.GetComponent<Building>();
                if (building != null)
                {
                    building.ServeNextClient();
                }
            }
        }
    }

}