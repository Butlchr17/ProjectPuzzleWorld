using UnityEngine;

namespace PuzzleWorld {
    [RequireComponent(typeof(SpriteRenderer))]
    public class Orb : MonoBehaviour
    {
        public OrbType type;

        public void SetType(OrbType type)
        {
            this.type = type;
            GetComponent<SpriteRenderer>().sprite = type.sprite;
        }
        public OrbType GetType() => type;
    }
}