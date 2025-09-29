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

        GridSystem2D<GridObject<Orb>> grid;

        void Start()
        {
            grid = GridSystem2D<GridObject<Orb>>.VerticalGrid(width, height, cellSize, originPosition, debug);
        }
    }
}
