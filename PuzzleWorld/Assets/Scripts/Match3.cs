using UnityEngine;

namespace PuzzleWorld
{
    public class Match3 : MonoBehaviour
    {
        [SerializeField] int width = 8;
        [SerializeField] int height = 8;
        [SerializeField] float cellSize = 1f;
        [SerializeField] Vector3 originPosition = Vector3.zero;
        [SerializeField] bool debug = true;

        [SerializeField] Orb orbPrefab;
        [SerializeField] OrbType[] orbTypes;

        GridSystem2D<GridObject<Orb>> grid;

        void Start()
        {
           InitializeGrid();
        }

        void InitializeGrid()
        { 
            grid = GridSystem2D<GridObject<Orb>>.VerticalGrid(width, height, cellSize, originPosition, debug);
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
            Orb orb = Instantiate(orbPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
            orb.SetType(orbTypes[Random.Range(0, orbTypes.Length)]);
            var gridObject = new GridObject<Orb>(grid, x, y);
            gridObject.SetValue(orb);
            grid.SetValue(x, y, gridObject);
        }

        // Init grid

        // Read player input and swap orbs

        // Start coroutine:
        // Swap animation
        // Check matches
        // Make Orbs Explode
        // Make Orbs fall down
        // Fill empty spaces with new orbs
        // Is game over?
    }
}
