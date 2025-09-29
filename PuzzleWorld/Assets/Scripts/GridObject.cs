using PuzzleWorld;
using UnityEngine;

namespace PuzzleWorld
{
    public class GridObject<T>
    {
        GridSystem2D<GridObject<T>> grid;
        int x;
        int y;

        // moveable object in grid
        T orb;

        public GridObject(GridSystem2D<GridObject<T>> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
        }
        public void SetValue(T orb)
        {
            this.orb = orb;
        }

        public T GetValue() => orb;

    }
}
