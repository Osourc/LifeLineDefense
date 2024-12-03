using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CardTower : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    public int Cost;
    public GameObject TowerPrefab;
    public Vector3 Offset; // New property for specific offsets

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private GameObject currentTowerInstance;
    public LayerMask towerBaseLayer;
    private TextMeshProUGUI costText;
    private MonoBehaviour  uiManager;
    public static bool IsDraggingCard = false;

    private void Start()
    {
        costText = transform.Find("Cost").GetComponent<TextMeshProUGUI>();
        if (costText != null)
        {
            costText.text = Cost.ToString(); // Set the cost text
        }
        else
        {
            Debug.LogWarning("No TextMeshProUGUI component found named 'Cost'.");
        }

        uiManager = FindActiveUIManager();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

        private MonoBehaviour FindActiveUIManager()
    {
        // Try to find UI managers and return the active one
        SoloUIMasterScript soloUI = FindObjectOfType<SoloUIMasterScript>();
        EndlessUIMasterScript endlessUI = FindObjectOfType<EndlessUIMasterScript>();
        MultiUIMasterScript multiUI = FindObjectOfType<MultiUIMasterScript>();

        // Check which one is active and return it
        if (soloUI != null && soloUI.gameObject.activeInHierarchy)
        {
            return soloUI;
        }
        else if (endlessUI != null && endlessUI.gameObject.activeInHierarchy)
        {
            return endlessUI;
        }
        else if (multiUI != null && multiUI.gameObject.activeInHierarchy)
        {
            return multiUI;
        }
        else
        {
            Debug.LogWarning("No active UI manager found in the scene.");
            return null;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        IsDraggingCard = true;
        Debug.Log("Begin dragging card: " + gameObject.name);
        currentTowerInstance = Instantiate(TowerPrefab);
        currentTowerInstance.transform.SetParent(transform.parent, false);
        currentTowerInstance.transform.localScale = Vector3.one;
        currentTowerInstance.SetActive(true);

        SpriteRenderer towerRenderer = currentTowerInstance.GetComponentInChildren<SpriteRenderer>();
        if (towerRenderer != null)
        {
            Debug.Log("SpriteRenderer found and setting sorting layer.");
            towerRenderer.sortingLayerName = "Towers";
            towerRenderer.sortingOrder = 10;
        }
        else
        {
            Debug.LogWarning("No SpriteRenderer found on the tower prefab.");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentTowerInstance != null)
        {
            Vector2 mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out mousePosition);

            currentTowerInstance.transform.localPosition = mousePosition;
            canvasGroup.alpha = 0.6f;

            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 2.0f);
            if (Physics.Raycast(ray, out hit, 100f, towerBaseLayer))
            {
                TowerBase towerBase = hit.transform.GetComponent<TowerBase>();
                SpriteRenderer towerRenderer = currentTowerInstance.GetComponent<SpriteRenderer>();

                if (towerBase != null && towerBase.IsOccupied)
                {
                    if (towerRenderer != null)
                    {
                        towerRenderer.color = Color.red; // Change to red if occupied
                    }
                }
                else
                {
                    if (towerRenderer != null)
                    {
                        towerRenderer.color = Color.white; // Reset to white if not occupied
                    }
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDraggingCard = false;
        Debug.Log("End dragging card: " + gameObject.name);
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;

        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 2.0f);
        if (Physics.Raycast(ray, out hit, 100f, towerBaseLayer))
        {
            TowerBase towerBase = hit.transform.GetComponent<TowerBase>();

            if (towerBase != null && !towerBase.IsOccupied)
            {
                // Cast uiManager to the correct type based on the active UI
                var soloUIManager = uiManager as SoloUIMasterScript;
                var endlessUIManager = uiManager as EndlessUIMasterScript;
                var multiUIManager = uiManager as MultiUIMasterScript;

                if (soloUIManager != null && soloUIManager.O2 >= Cost)
                {
                    GameObject towerInstance = Instantiate(TowerPrefab);
                    towerInstance.SetActive(true);
                    towerInstance.transform.position = hit.transform.position + Offset;

                    towerBase.PlaceTower();
                    soloUIManager.O2 -= Cost;

                    Debug.Log($"Placed tower on TowerBase at: {hit.transform.position}, Cost deducted: {Cost}");
                }
                else if (endlessUIManager != null && endlessUIManager.O2 >= Cost)
                {
                    GameObject towerInstance = Instantiate(TowerPrefab);
                    towerInstance.SetActive(true);
                    towerInstance.transform.position = hit.transform.position + Offset;

                    towerBase.PlaceTower();
                    endlessUIManager.O2 -= Cost;

                    Debug.Log($"Placed tower on TowerBase at: {hit.transform.position}, Cost deducted: {Cost}");
                }
                else if (multiUIManager != null && multiUIManager.O2 >= Cost)
                {
                    GameObject towerInstance = Instantiate(TowerPrefab);
                    towerInstance.SetActive(true);
                    towerInstance.transform.position = hit.transform.position + Offset;

                    towerBase.PlaceTower();
                    multiUIManager.O2 -= Cost;

                    Debug.Log($"Placed tower on TowerBase at: {hit.transform.position}, Cost deducted: {Cost}");
                }
                else
                {
                    Debug.Log("Not enough O2 to place tower.");
                }
            }
            else
            {
                Debug.Log("TowerBase is occupied, cannot place tower.");
            }
        }
        else
        {
            Debug.Log("Raycast did not hit any object.");
        }

        if (currentTowerInstance != null)
        {
            Destroy(currentTowerInstance);
            currentTowerInstance = null;
        }
    }
}
