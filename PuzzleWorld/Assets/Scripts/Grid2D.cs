using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI; 

namespace PuzzleWorld
{
    public class GridSystem2D<T>
    {
        readonly int width;
        readonly int height;
        readonly float cellSize;
        readonly Vector3 originPosition;
        readonly T[,] gridArray;

        readonly CoordinateConverter coordinateConverter;

        public event Action<int, int, T> OnValueChangeEvent;

        public static GridSystem2D<T> UIGrid(int width, int height, float cellSize, Vector3 originPosition, RectTransform parentTransform, bool debug = false)
        {
            return new GridSystem2D<T>(width, height, cellSize, originPosition, new UIConverter(parentTransform), debug);
        }

        public GridSystem2D(int width, int height, float cellSize, Vector3 originPosition, CoordinateConverter coordinateConverter, bool debug)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.originPosition = originPosition;
            this.coordinateConverter = coordinateConverter ?? new UIConverter(null); // Default to UI, but parent might need setting later

            gridArray = new T[width, height];

            if (debug)
            {
                DrawDebugLines();
            }
        }

        // Set a value in a grid position
        public void SetValue(Vector3 worldPosition, T value)
        {
            Vector2Int pos = coordinateConverter.WorldToGrid(worldPosition, cellSize, originPosition);
            SetValue(pos.x, pos.y, value);
        }

        public void SetValue(int x, int y, T value)
        {
            if (IsValid(x, y))
            {
                gridArray[x, y] = value;
                OnValueChangeEvent?.Invoke(x, y, value);
            }
        }

        // Get a value from a grid position
        public T GetValue(Vector3 worldPosition)
        {
            Vector2Int pos = GetXY(worldPosition);
            return GetValue(pos.x, pos.y);
        }

        public T GetValue(int x, int y)
        {
            return IsValid(x, y) ? gridArray[x, y] : default;
        }

        // Are the input position coordinates valid?
        bool IsValid(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

        public Vector2Int GetXY(Vector3 worldPosition) => coordinateConverter.WorldToGrid(worldPosition, cellSize, originPosition);

        public Vector3 GetWorldPositionCenter(int x, int y) => coordinateConverter.GridToWorldCenter(x, y, cellSize, originPosition);

        Vector3 GetWorldPosition(int x, int y) => coordinateConverter.GridToWorld(x, y, cellSize, originPosition);

        void DrawDebugLines()
        {
            if (coordinateConverter is not UIConverter uiConverter) return;

            var parent = uiConverter.parentTransform?.gameObject ?? new GameObject("Debugging"); // Fallback if no parent

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CreateUIWorldText(parent, x + "," + y, GetWorldPositionCenter(x, y));
                }
            }
        }

        TextMeshProUGUI CreateUIWorldText(GameObject parent, string text, Vector3 localPosition, float fontSize = 0.5f, Color color = default)
        {
            GameObject gameObject = new GameObject("DebugText_" + text, typeof(TextMeshProUGUI), typeof(RectTransform));
            gameObject.transform.SetParent(parent.transform);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchoredPosition = localPosition;
            rect.sizeDelta = new Vector2(100, 50); // Adjust as needed

            TextMeshProUGUI textMeshPro = gameObject.GetComponent<TextMeshProUGUI>();
            textMeshPro.text = text;
            textMeshPro.fontSize = fontSize;
            textMeshPro.color = color == default ? Color.white : color;
            textMeshPro.alignment = TextAlignmentOptions.Center;

            return textMeshPro;
        }

        public abstract class CoordinateConverter
        {
            public abstract Vector3 GridToWorld(int x, int y, float cellSize, Vector3 originPosition);
            public abstract Vector3 GridToWorldCenter(int x, int y, float cellSize, Vector3 originPosition);
            public abstract Vector2Int WorldToGrid(Vector3 worldPosition, float cellSize, Vector3 originPosition);
        }

        public class UIConverter : CoordinateConverter
        {
            public RectTransform parentTransform;

            public UIConverter(RectTransform parent)
            {
                parentTransform = parent;
            }

            public override Vector3 GridToWorld(int x, int y, float cellSize, Vector3 originPosition)
            {
                return new Vector3(x * cellSize, -y * cellSize, 0) + originPosition;
            }

            public override Vector3 GridToWorldCenter(int x, int y, float cellSize, Vector3 originPosition)
            {
                return new Vector3((x + 0.5f) * cellSize, -(y + 0.5f) * cellSize, 0) + originPosition;
            }

            public override Vector2Int WorldToGrid(Vector3 worldPosition, float cellSize, Vector3 originPosition)
            {
                Vector3 localPos = (worldPosition - originPosition);
                var x = Mathf.FloorToInt(localPos.x / cellSize);
                var y = Mathf.FloorToInt(-localPos.y / cellSize);
                return new Vector2Int(x, y);
            }
        }
    }
}