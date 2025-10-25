using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;


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
        [SerializeField] Ease ease = Ease.InQuad;
        [SerializeField] GameObject explosion;

        InputReader inputReader;
        AudioManager audioManager;

        GridSystem2D<GridObject<Orb>> grid;

        Vector2Int selectedOrb = Vector2Int.one * -1;

        void Awake()
        {
            inputReader = GetComponent<InputReader>();
            audioManager = GetComponent<AudioManager>();
        }

        void Start()
        {
            InitializeGrid();
            inputReader.Fire += OnSelectOrb;
        }

        void OnDestroy()
        {
            inputReader.Fire -= OnSelectOrb;
        }

       

        IEnumerator RunGameLoop(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            yield return StartCoroutine(SwapOrbs(gridPosA, gridPosB));

            //Matches?
            List<Vector2Int> matches = FindMatches();

            //TODO: Calculate Score

            //Make orbs explode
            yield return StartCoroutine(ExplodeOrbs(matches));
            //Make orbs fall
            yield return StartCoroutine(MakeOrbsFall());
            //Fill empty orb slots
            yield return StartCoroutine(FillEmptySlots());
            
            //Is game over?
            DeselectOrb();
            yield return null;
        }

        IEnumerator FillEmptySlots()
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (grid.GetValue(x, y) == null)
                    {
                        CreateOrb(x, y);
                        audioManager.PlayPop();
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
                    if (grid.GetValue(x, y) == null)
                    {
                        for (var i = y + 1; i < height; i++)
                        {
                            var orb = grid.GetValue(x, i).GetValue();
                            grid.SetValue(x, y, grid.GetValue(x, i));
                            grid.SetValue(x, i, null);
                            orb.transform
                                .DOLocalMove(grid.GetWorldPositionCenter(x, y), 0.5f)
                                .SetEase(ease);
                            audioManager.PlayWoosh();
                            yield return new WaitForSeconds(0.1f);
                            break;
                        }
                    }
                }
            }
        }

        IEnumerator ExplodeOrbs(List<Vector2Int> matches)
        {
            audioManager.PlayPop();

            foreach (var match in matches)
            {
                var orb = grid.GetValue(match.x, match.y).GetValue();
                grid.SetValue(match.x, match.y, null);

                ExplodeVFX(match);

                orb.transform
                    .DOPunchScale(Vector3.one * 0.1f, 0.1f, 1, 0.5f);

                yield return new WaitForSeconds(0.1f);

                Destroy(orb.gameObject, 0.1f);
            }
        }

        void ExplodeVFX(Vector2Int match)
        {
            //TODO: Pooling

            var fx = Instantiate(explosion, transform);
            fx.transform.position = grid.GetWorldPositionCenter(match.x, match.y);
            Destroy(fx, 5f);
        }

        List<Vector2Int> FindMatches()
        {
            HashSet<Vector2Int> matches = new();

            //Horizontal matches
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width - 2; x++)
                {
                    var orbA = grid.GetValue(x, y);
                    var orbB = grid.GetValue(x + 1, y);
                    var orbC = grid.GetValue(x + 2, y);

                    if (orbA == null || orbB == null || orbC == null) continue;
                    
                    if (orbA.GetValue().GetType() == orbB.GetValue().GetType() 
                        &&orbB.GetValue().GetType() == orbC.GetValue().GetType())
                    {
                        matches.Add(new Vector2Int(x, y));
                        matches.Add(new Vector2Int(x + 1, y));
                        matches.Add(new Vector2Int(x + 2, y));
                    }
                }
            }

            //Vertical matches
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height - 2; y++)
                {
                    var orbA = grid.GetValue(x, y);
                    var orbB = grid.GetValue(x, y + 1);
                    var orbC = grid.GetValue(x, y + 2);

                    if (orbA == null || orbB == null || orbC == null) continue;
                    
                    if (orbA.GetValue().GetType() == orbB.GetValue().GetType() 
                        &&orbB.GetValue().GetType() == orbC.GetValue().GetType())
                    {
                        matches.Add(new Vector2Int(x, y));
                        matches.Add(new Vector2Int(x, y + 1));
                        matches.Add(new Vector2Int(x, y + 2));
                    }
                }
            }

            if (matches.Count == 0)
            {
                audioManager.PlayNoMatch();
            }
            else
            {
                audioManager.PlayMatch();
            }

            return new List<Vector2Int>(matches);
        }

        IEnumerator SwapOrbs(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            var gridObjectA = grid.GetValue(gridPosA.x, gridPosA.y);
            var gridObjectB = grid.GetValue(gridPosB.x, gridPosB.y);

            gridObjectA.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosB.x, gridPosB.y), 0.5f)
                .SetEase(ease);

            gridObjectB.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosA.x, gridPosA.y), 0.5f)
                .SetEase(ease);

            grid.SetValue(gridPosA.x, gridPosA.y, gridObjectB);
            grid.SetValue(gridPosB.x, gridPosB.y, gridObjectA);

            yield return new WaitForSeconds(0.5f);
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
            var orb = Instantiate(orbPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
            orb.SetType(orbTypes[Random.Range(0, orbTypes.Length)]);
            var gridObject = new GridObject<Orb>(grid, x, y);
            gridObject.SetValue(orb);
            grid.SetValue(x, y, gridObject);
        }
         void OnSelectOrb()
        {
            var gridPos = grid.GetXY(Camera.main.ScreenToWorldPoint(inputReader.Selected));

            // Validate grid position

            if (!IsValidPosition(gridPos) || IsEmptyPosition(gridPos)) return;

            if (selectedOrb == gridPos)
            {
                DeselectOrb();
                audioManager.PlayDeselect();
            }
            else if (selectedOrb.x == -1 && selectedOrb.y == -1)
            {
                SelectOrb(gridPos);
                audioManager.PlayClick();
            }
            else
            {
                StartCoroutine(RunGameLoop(selectedOrb, gridPos));
            }
        }

        void DeselectOrb() => selectedOrb = new Vector2Int(-1, -1);
        void SelectOrb(Vector2Int gridPos) => selectedOrb = gridPos;
        
        bool IsEmptyPosition(Vector2Int gridPosition) => grid.GetValue(gridPosition.x, gridPosition.y) == null;

        bool IsValidPosition(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height;
        }
    }
}
