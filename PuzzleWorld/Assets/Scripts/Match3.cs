using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;


namespace PuzzleWorld
{
    public class Match3 : MonoBehaviour
    {
        [SerializeField] int width = 6;
        [SerializeField] int height = 5;
        [SerializeField] float cellSize = 1f;
        [SerializeField] Vector3 originPosition = Vector3.zero;
        [SerializeField] bool debug = true;
        [SerializeField] Orb orbPrefab;
        [SerializeField] OrbType[] orbTypes;
        [SerializeField] Ease ease = Ease.InQuad;
        [SerializeField] private int poolSize = 30;
        [SerializeField] private int totalScore = 0;

        //[SerializeField] GameObject explosion; // Explosion prefab for VFX    
        //private Queue<GameObject> explosionPool = new Queue<GameObject>();

        InputReader inputReader;
        //AudioManager audioManager;

        GridSystem2D<GridObject<Orb>> grid;

        Vector2Int selectedOrb = Vector2Int.one * -1;

        void Awake()
        {
            inputReader = GetComponent<InputReader>();
            //audioManager = GetComponent<AudioManager>();
        }

        void Start()
        {
            InitializeGrid();
            inputReader.Fire += OnSelectOrb;
            //InitializePool();
        }

        void OnDestroy()
        {
            inputReader.Fire -= OnSelectOrb;
        }

        //void InitializePool()
        //{
        //    for (int i = 0; i < poolSize; i++)
        //    {
        //        var fx = Instantiate(explosion, transform);
        //        fx.SetActive(false);
        //        explosionPool.Enqueue(fx);
        //    }
        //}

        //IEnumerator ReturnToPool(GameObject fx, float delay)
        //{
        //    yield return new WaitForSeconds(delay);
        //    fx.SetActive(false);
        //    explosionPool.Enqueue(fx);
        //}

        IEnumerator RunGameLoop(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            isProcessingInput = true;

            yield return StartCoroutine(SwapOrbs(gridPosA, gridPosB));

            //Matches?
            List<Vector2Int> matches = FindMatches();

            //No matches, swap back
            if (matches.Count == 0)
            {
                yield return StartCoroutine(SwapOrbs(gridPosA, gridPosB)); //Swap back
                //audioManager.PlayNoMatch();
                DeselectOrb();
                yield break;
            }
            int moveScore = 0;
            int comboMultiplier = 1; // For future combo scoring

            while (matches.Count > 0)
            {
                moveScore += matches.Count * 10 * comboMultiplier; // Base 10 points per orb, scaled by combo multiplier

                //Make orbs explode
                yield return StartCoroutine(ExplodeOrbs(matches));
                //Make orbs fall
                yield return StartCoroutine(MakeOrbsFall());
                //Fill empty orb slots
                yield return StartCoroutine(FillEmptySlots());

                matches = FindMatches(); //Check for new matches
                comboMultiplier++; //Increase for each combo chain
            }

            totalScore += moveScore;

            //Is game over?
            if (IsGameOver())
            {
                Debug.Log($"Game Over! Final Score: {totalScore}");
            }

            DeselectOrb();
            isProcessingInput = false;
            yield return null;
        }

        // New class method
        bool IsGameOver()
        {
            // Check horizontal pairs
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    if (SimulateSwapAndCheck(new Vector2Int(x, y), new Vector2Int(x + 1, y))) return false;
                }
            }

            // Check vertical pairs
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    if (SimulateSwapAndCheck(new Vector2Int(x, y), new Vector2Int(x, y + 1))) return false;
                }
            }

            return true;  // No moves left
        }

        bool SimulateSwapAndCheck(Vector2Int posA, Vector2Int posB)
        {
            // Swap
            var objA = grid.GetValue(posA.x, posA.y);
            var objB = grid.GetValue(posB.x, posB.y);
            var orbA = objA.GetValue();
            var orbB = objB.GetValue();

            objA.SetValue(orbB);
            objB.SetValue(orbA);

            bool hasMatches = FindMatches().Count > 0;

            // Swap back
            objA.SetValue(orbA);
            objB.SetValue(orbB);

            return hasMatches;
        }

        IEnumerator FillEmptySlots()
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (grid.GetValue(x, y).GetValue() == null)
                    {
                        CreateOrb(x, y);
                        //audioManager.PlayPop();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }

        IEnumerator MakeOrbsFall()
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (grid.GetValue(x, y).GetValue() == null)
                    {
                        for (var i = y + 1; i < height; i++)
                        {
                            var orb = grid.GetValue(x, i).GetValue();
                            if (orb == null) continue;  // Skip empty grid objects (test and comment if needed)

                            grid.GetValue(x, y).SetValue(orb);
                            grid.GetValue(x, i).SetValue(null);
                            orb.transform
                                .DOLocalMove(grid.GetWorldPositionCenter(x, y), 0.5f)
                                .SetEase(ease);
                            //audioManager.PlayWoosh();
                            yield return new WaitForSeconds(0.1f);
                            break;
                        }
                    }
                }
            }
        }

        IEnumerator ExplodeOrbs(List<Vector2Int> matches)
        {
            //audioManager.PlayPop();

            foreach (var match in matches)
            {
                var orb = grid.GetValue(match.x, match.y).GetValue();
                grid.GetValue(match.x, match.y).SetValue(null);

                //ExplodeVFX(match);

                orb.transform
                    .DOPunchScale(Vector3.one * 0.1f, 0.1f, 1, 0.5f);

                yield return new WaitForSeconds(0.1f);

                Destroy(orb.gameObject, 0.1f);
            }
        }
        //void ExplodeVFX(Vector2Int match)
        //{
        //    if (explosionPool.Count == 0) return;

        //    var fx = explosionPool.Dequeue();
        //    fx.transform.position = grid.GetWorldPositionCenter(match.x, match.y);
        //    fx.SetActive(true);

        //    // Assume the explosion effect has a ParticleSystem component and OnParticleSystemStopped event or use a coroutine to return it to the pool
        //    StartCoroutine(ReturnToPool(fx, 5f)); // Adjust time as needed)
        //}

        List<Vector2Int> FindMatches()
        {
            HashSet<Vector2Int> matches = new();

            ////Horizontal matches
            //for (var y = 0; y < height; y++)
            //{
            //    for (var x = 0; x < width - 2; x++)
            //    {
            //        var orbA = grid.GetValue(x, y);
            //        var orbB = grid.GetValue(x + 1, y);
            //        var orbC = grid.GetValue(x + 2, y);

            //        if (orbA.GetValue() == null || orbB.GetValue() == null || orbC.GetValue() == null) continue;

            //        if (orbA.GetValue().GetOrbType() == orbB.GetValue().GetOrbType()
            //            && orbB.GetValue().GetOrbType() == orbC.GetValue().GetOrbType())
            //        {
            //            matches.Add(new Vector2Int(x, y));
            //            matches.Add(new Vector2Int(x + 1, y));
            //            matches.Add(new Vector2Int(x + 2, y));
            //        }
            //    }
            //}

            ////Vertical matches
            //for (var x = 0; x < width; x++)
            //{
            //    for (var y = 0; y < height - 2; y++)
            //    {
            //        var orbA = grid.GetValue(x, y);
            //        var orbB = grid.GetValue(x, y + 1);
            //        var orbC = grid.GetValue(x, y + 2);

            //        if (orbA.GetValue() == null || orbB.GetValue() == null || orbC.GetValue() == null) continue;

            //        if (orbA.GetValue().GetOrbType() == orbB.GetValue().GetOrbType()
            //            && orbB.GetValue().GetOrbType() == orbC.GetValue().GetOrbType())
            //        {
            //            matches.Add(new Vector2Int(x, y));
            //            matches.Add(new Vector2Int(x, y + 1));
            //            matches.Add(new Vector2Int(x, y + 2));
            //        }
            //    }
            //}


            // Horizontal matches
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var currentType = grid.GetValue(x, y).GetValue()?.GetOrbType();
                    if (currentType == null) continue;

                    int startX = x;
                    while (x + 1 < width && grid.GetValue(x + 1, y).GetValue()?.GetOrbType() == currentType)
                    {
                        x++;
                    }
                    int chainLength = x - startX + 1;
                    if (chainLength >= 3)
                    {
                        for (int i = startX; i <= x; i++)
                        {
                            matches.Add(new Vector2Int(i, y));
                        }
                    }
                }
            }

            // Vertical matches
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var currentType = grid.GetValue(x, y).GetValue()?.GetOrbType();
                    if (currentType == null) continue;

                    int startY = y;
                    while (y + 1 < height && grid.GetValue(x, y + 1).GetValue()?.GetOrbType() == currentType)
                    {
                        y++;
                    }
                    int chainLength = y - startY + 1;
                    if (chainLength >= 3)
                    {
                        for (int i = startY; i <= y; i++)
                        {
                            matches.Add(new Vector2Int(x, i));
                        }
                    }
                }
            }

            if (matches.Count == 0)
            {
                //audioManager.PlayNoMatch();
            }
            else
            {
                //audioManager.PlayMatch();
            }

            return new List<Vector2Int>(matches);
        }

        IEnumerator SwapOrbs(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            var gridObjectA = grid.GetValue(gridPosA.x, gridPosA.y);
            var gridObjectB = grid.GetValue(gridPosB.x, gridPosB.y);

            var orbA = gridObjectA.GetValue();
            var orbB = gridObjectB.GetValue();

            orbA.transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosB.x, gridPosB.y), 0.5f)
                .SetEase(ease);

            orbB.transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosA.x, gridPosA.y), 0.5f)
                .SetEase(ease);

            gridObjectA.SetValue(orbB);
            gridObjectB.SetValue(orbA);

            yield return new WaitForSeconds(0.5f);
        }

        void InitializeGrid()
        {
            grid = GridSystem2D<GridObject<Orb>>.VerticalGrid(width, height, cellSize, originPosition, debug);

            // Create fixed grid for all cells
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var gridObject = new GridObject<Orb>(grid, x, y);
                    grid.SetValue(x, y, gridObject);
                }
            }

            bool hasMatches;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        CreateOrb(x, y);
                    }
                }

                hasMatches = FindMatches().Count > 0;

                if (hasMatches)
                {
                    // clear grid orbs
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            var orb = grid.GetValue(x, y).GetValue();
                            if (orb != null)
                            {
                                Destroy(orb.gameObject);
                                grid.SetValue(x, y, null);
                            }
                        }
                    }
                }
                attempts++;
                if (attempts >= maxAttempts)
                {
                    Debug.LogWarning("Max attempts reached while initializing grid without matches.");
                    break;
                }
            } while (hasMatches);
        }

        void CreateOrb(int x, int y)
        {
            var orb = Instantiate(orbPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
            orb.SetType(orbTypes[Random.Range(0, orbTypes.Length)]);
            grid.GetValue(x, y).SetValue(orb);
        }

        // Input processing
        bool isProcessingInput = false;

        void OnSelectOrb()
        {
            if (isProcessingInput) return; // Prevent input during processing

            var gridPos = grid.GetXY(Camera.main.ScreenToWorldPoint(inputReader.Selected));

            // Validate grid position

            if (!IsValidPosition(gridPos) || IsEmptyPosition(gridPos)) return;

            if (selectedOrb == gridPos)
            {
                DeselectOrb();
                //audioManager.PlayDeselect();
            }
            else if (selectedOrb.x == -1 && selectedOrb.y == -1)
            {
                SelectOrb(gridPos);
                //audioManager.PlayClick();
            }
            else
            {
                // Check adjacency (Manhattan distance == 1, no diagonals)
                int dx = Mathf.Abs(selectedOrb.x - gridPos.x);
                int dy = Mathf.Abs(selectedOrb.y - gridPos.y);
                if (dx + dy == 1)
                {
                    StartCoroutine(RunGameLoop(selectedOrb, gridPos));
                }
                else
                {
                    // Invalid: deselect or play sound
                    //audioManager.PlayDeselect();
                    DeselectOrb();
                }
            }

        }
        void DeselectOrb() => selectedOrb = new Vector2Int(-1, -1);
        void SelectOrb(Vector2Int gridPos) => selectedOrb = gridPos;

        bool IsEmptyPosition(Vector2Int gridPosition) => grid.GetValue(gridPosition.x, gridPosition.y).GetValue() == null;

        bool IsValidPosition(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height;
        }
    }
}
