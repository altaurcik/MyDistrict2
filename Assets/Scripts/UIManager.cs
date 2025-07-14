using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Text")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI happinessText;
    public TextMeshProUGUI clientCountText;
    public TextMeshProUGUI spawnRateText;

    [Header("UI Panels")]
    public GameObject buyButton;         // Кнопка "Buy" (правый нижний угол)
    public GameObject buyMenuPanel;      // Панель с кнопками Shop / Atraction

    public void UpdateUI()
    {
        moneyText.text = "$" + GameManager.Instance.GetMoney();
        happinessText.text = GameManager.Instance.GetAverageHappiness().ToString("F1");
        clientCountText.text = GameManager.Instance.GetClientsServed().ToString();
        spawnRateText.text = GameManager.Instance.GetTotalClientsPerMinute() + "/min";
    }

    public void OpenBuyMenu()
    {
        buyButton.SetActive(false);
        buyMenuPanel.SetActive(true);
    }

    public void CloseBuyMenu()
    {
        buyButton.SetActive(true);
        buyMenuPanel.SetActive(false);
    }

    public void BuyShop()
    {
        GameManager.Instance.PrepareToPlaceBuilding(BuildingType.Shop);
        CloseBuyMenu();
    }

    public void BuyAttraction()
    {
        GameManager.Instance.PrepareToPlaceBuilding(BuildingType.Attraction);
        CloseBuyMenu();
    }
}
