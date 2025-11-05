using UnityEngine;
using UnityEngine.UI;

namespace PuzzleWorld {
    [RequireComponent(typeof(Image))]
    public class Orb : MonoBehaviour
    {
        public OrbType type;
        public Vector2Int gridPosition; // Position in the grid
        public void SetType(OrbType type)
        {
            this.type = type;
            GetComponent<Image>().sprite = type.sprite;
        }
        public OrbType GetOrbType() => type;
    }
}