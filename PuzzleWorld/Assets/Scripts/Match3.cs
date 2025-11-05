using System.Collections.Generic;   // For List<RaycastResult>
using UnityEngine;
using UnityEngine.EventSystems;     // For PointerEventData and raycasting
using UnityEngine.UI;
using static UnityEngine.InputManagerEntry;
using Random = UnityEngine.Random;

namespace PuzzleWorld
{
    public class Match3 : MonoBehaviour
    {
        [SerializeField] int width = 6;
        [SerializeField] int height = 5;
        [SerializeField] float cellSize = 100f;
        [SerializeField] Vector3 originPosition = new Vector3(-250, 250, 0); // Top-left corner relative to panel center, adjust as needed
        [SerializeField] bool debug = true;
        [SerializeField] Orb orbPrefab;
        [SerializeField] OrbType[] orbTypes;
        [SerializeField] RectTransform gridParent;
        [SerializeField] Canvas canvas;

        InputReader inputReader;
        GridSystem2D<GridObject<Orb>> grid;

        Vector2Int selectedPos = new Vector2Int(-1, -1);
        Vector3 originalPosition;
        Vector3 originalScale;

        // For UI raycasting
        GraphicRaycaster graphicRaycaster;
        PointerEventData pointerEventData;
        EventSystem eventSystem;

        void Awake()
        {
            inputReader = GetComponent<InputReader>();
            graphicRaycaster = canvas.GetComponent<GraphicRaycaster>(); // Get the GraphicRaycaster from the canvas, or assign it in the inspector
            eventSystem = FindAnyObjectByType<EventSystem>();           // Find the EventSystem in the scene
        }

        void Start()
        {
            InitializeGrid();
            inputReader.PointerPressed += OnPointerPressed;
            inputReader.PointerReleased += OnPointerReleased;
        }

        void OnDestroy()
        {
            inputReader.PointerPressed -= OnPointerPressed;
            inputReader.PointerReleased -= OnPointerReleased;
        }

        void InitializeGrid()
        {
            grid = GridSystem2D<GridObject<Orb>>.UIGrid(width, height, cellSize, originPosition, gridParent, debug);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var gridObject = new GridObject<Orb>(grid, x, y);
                    grid.SetValue(x, y, gridObject);
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CreateOrb(x, y);
                }
            }
        }

        void CreateOrb(int x, int y)
        {
            var orb = Instantiate(orbPrefab, gridParent);                           // Set parent to gridParent for UI
            orb.transform.localScale = Vector3.one;                                 // Reset scale to 1
            RectTransform rect = orb.GetComponent<RectTransform>();                 // Get RectTransform for UI positioning
            //rect.sizeDelta = new Vector2(cellSize, cellSize);                       // Set size for UI
            rect.anchoredPosition = (Vector2)grid.GetWorldPositionCenter(x, y);     // Set anchored position for UI
            orb.SetType(orbTypes[Random.Range(0, orbTypes.Length)]);                // Assign random orb type
            orb.gridPosition = new Vector2Int(x, y);                                // Set grid position
            grid.GetValue(x, y).SetValue(orb);                                      // Store orb in grid
        }

        void OnPointerPressed()
        {
            if (Camera.main == null)
            {
                Debug.LogError("No main camera found! Please tag your camera as 'MainCamera'.");
                return;
            }

            // Setup pointer data for raycasting
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = inputReader.Selected;                                               // Use the input reader's selected position

            // Raycast to check if we clicked on a UI element
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerEventData, results);

            if (results.Count > 0)
            {
                // Find first hit Orb
                foreach (var result in results)
                {
                    Orb hitOrb = result.gameObject.GetComponent<Orb>();
                    if (hitOrb != null)
                    {
                        Vector2Int gridPos = hitOrb.gridPosition;
                        if (IsValidPosition(gridPos) && !IsEmptyPosition(gridPos))
                        {
                            selectedPos = gridPos;
                            originalPosition = hitOrb.GetComponent<RectTransform>().anchoredPosition;
                            originalScale = hitOrb.transform.localScale;
                            hitOrb.transform.localScale *= 1.2f;
                            hitOrb.transform.SetAsLastSibling();                                            // Bring to front in UI
                            return;                                                                         // Stop at first orb hit
                        }
                    }
                }
            }
        }

        void OnPointerReleased()
        {
            if (selectedPos.x != -1 && selectedPos.y != -1)
            {
                var orb = grid.GetValue(selectedPos.x, selectedPos.y).GetValue();
                if (orb != null)
                {
                    orb.GetComponent<RectTransform>().anchoredPosition = originalPosition;
                    orb.transform.localScale = originalScale;
                }
                selectedPos = new Vector2Int(-1, -1);
            }
        }

        bool IsValidPosition(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height;
        }

        bool IsEmptyPosition(Vector2Int gridPosition) => grid.GetValue(gridPosition.x, gridPosition.y).GetValue() == null;
    }
}