using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using Random = UnityEngine.Random;

public class SlidePuzzleController : MonoBehaviour
{
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;
    
    [Header("Puzzle Settings")]
    [SerializeField] private int rows = 2;
    [SerializeField] private int cols = 3;

    private List<Transform> _pieces;
    private int _emptyLocation;
    private bool _shuffling = false;
    private bool _isCompletedOnce = false;
    
    [Header("Game Settings")]
    private PuzzlePieceManager _puzzlePieceManager;
    [SerializeField] private Sprite puzzleImage;
    [SerializeField] private float durationForAutoComplete = 60f;
    
    public UnityEvent OnPuzzleComplete;

    private void Awake()
    {
        // _puzzlePieceManager = piecePrefab.GetComponent<PuzzlePieceManager>();
        // _pieces = new List<Transform>();
        // CreateGamePieces(0.01f);
    }

    private void Start()
    {
        // _puzzlePieceManager.ChangeImageMaterial(puzzleImage);
    }

    public void ActivateThis(bool activate)
    {
        gameObject.SetActive(activate);
    }

    public IEnumerator StartGame(Sprite image)
    {
        _puzzlePieceManager = piecePrefab.GetComponent<PuzzlePieceManager>();
        _pieces = new List<Transform>();
        CreateGamePieces(0.01f);
        
        yield return new WaitForSeconds(1f);
        _puzzlePieceManager.ChangeImageMaterial(image);

        yield return new WaitForSeconds(1f);
        StartCoroutine(WaitShuffle(0.5f));
        _isCompletedOnce = false;
        
        yield return new WaitForSeconds(1f);
        StartCoroutine(WaitAutoComplete(durationForAutoComplete));
    }
    
    public void SetAutoCompleteDuration(float duration)
    {
        durationForAutoComplete = duration;
    }
    

    public void ChangeImage(Sprite image)
    {
        puzzleImage = image;
        _puzzlePieceManager.ChangeImageMaterial(puzzleImage);
    }

    public void AddOnPuzzleCompleted(UnityAction action)
    {
        OnPuzzleComplete.AddListener(action);
    }

    public void ChangePiecePrefab(Transform prefab)
    {
        piecePrefab = prefab;
    }

    // Create the game setup with _rows x _cols pieces.
    private void CreateGamePieces(float gapThickness)
    {
        // This is the width and height of each tile.
        float width = 1 / (float)cols;
        float height = 1 / (float)rows;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameTransform);
                _pieces.Add(piece);
                
                // Add Event
                UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable pieceInteraction = piece.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
                pieceInteraction.activated.AddListener(TryMovePieceActivated);
                pieceInteraction.selectEntered.AddListener(TryMovePieceSelect);
                
                // Pieces will be in a game board going from -1 to +1.
                piece.localPosition = new Vector3(-1 + (2 * width * col) + width,
                    +1 - (2 * height * row) - height,
                    0);
                piece.localScale = new Vector3((2 * width) - gapThickness, (2 * height) - gapThickness, 1);
                piece.name = $"{(row * cols) + col}";
                // We want an empty space in the bottom right.
                if ((row == rows - 1) && (col == cols - 1))
                {
                    _emptyLocation = (rows * cols) - 1;
                    piece.gameObject.SetActive(false);
                }
                else
                {
                    // We want to map the UV coordinates appropriately, they are 0->1.
                    float gap = gapThickness / 2;
                    Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                    Vector2[] uv = new Vector2[4];
                    // UV coord order: (0, 1), (1, 1), (0, 0), (1, 0)
                    uv[0] = new Vector2((width * col) + gap, 1 - ((height * (row + 1)) - gap));
                    uv[1] = new Vector2((width * (col + 1)) - gap, 1 - ((height * (row + 1)) - gap));
                    uv[2] = new Vector2((width * col) + gap, 1 - ((height * row) + gap));
                    uv[3] = new Vector2((width * (col + 1)) - gap, 1 - ((height * row) + gap));
                    // Assign our new UVs to the mesh.
                    mesh.uv = uv;
                }
            }
        }
    }

    private void OnEnable()
    {
        // StartCoroutine(WaitShuffle(0.5f));
        // _isCompletedOnce = false;
        // Invoke(nameof(StartGame), 1f);
    }

    private void Reset()
    {
        _isCompletedOnce = false;
    }

    void Update()
    {
        if (CheckCompletion() && !_isCompletedOnce && _shuffling)
        {
            _isCompletedOnce = true;
            _shuffling = false;
            
            // Stop the coroutine.
            StopCoroutine(WaitAutoComplete(durationForAutoComplete));
            Invoke(nameof(TriggerPuzzleComplete), 5f);
            
            Debug.Log(gameObject.name + " : Puzzle Completed");
        }
    }
    
    void TriggerPuzzleComplete()
    {
        OnPuzzleComplete?.Invoke();
    }

    private void TryMovePieceActivated(ActivateEventArgs args)
    {
        if (_isCompletedOnce)
        {
            return;
        }
        
        UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor validInteractor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor;
        
        if (validInteractor != null)
        {
            if (CheckAndMovePiece(validInteractor))
            {
                // Additional actions after a successful move.
            }
        }
    }
    
    private void TryMovePieceSelect(SelectEnterEventArgs args)
    {
        if (_isCompletedOnce)
        {
            return;
        }
        
        UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor validInteractor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor;
        
        if (validInteractor != null)
        {
            if (CheckAndMovePiece(validInteractor))
            {
                // Additional actions after a successful move.
            }
        }
    }

    private bool CheckAndMovePiece(UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor interactor)
    {
        if (interactor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // Go through the list, the index tells us the position.
            for (int i = 0; i < _pieces.Count; i++)
            {
                if (_pieces[i] == hit.transform)
                {
                    // Check each direction to see if valid move.
                    // We break out on success so we don't carry on and swap back again.
                    if (SwapIfValid(i, -cols, cols)) { return true; }
                    if (SwapIfValid(i, +cols, cols)) { return true; }
                    if (SwapIfValid(i, -1, 0)) { return true; }
                    if (SwapIfValid(i, +1, cols - 1)) { return true; }
                }
            }
        }
        return false;
    }

    // colCheck is used to stop horizontal moves wrapping.
    private bool SwapIfValid(int i, int offset, int colCheck)
    {
        if (_isCompletedOnce)
        {
            return false;
        }
        
        if (((i % cols) != colCheck) && ((i + offset) == _emptyLocation))
        {
            // Swap them in game state.
            (_pieces[i], _pieces[i + offset]) = (_pieces[i + offset], _pieces[i]);
            // Swap their transforms.
            (_pieces[i].localPosition, _pieces[i + offset].localPosition) =
                ((_pieces[i + offset].localPosition, _pieces[i].localPosition));
            // Update empty location.
            _emptyLocation = i;
            return true;
        }

        return false;
    }

    // We name the pieces in order so we can use this to check completion.
    private bool CheckCompletion()
    {
        for (int i = 0; i < _pieces.Count; i++)
        {
            if (_pieces[i].name != $"{i}")
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerator WaitShuffle(float duration)
    {
        yield return new WaitForSeconds(duration);
        Shuffle();
        
        yield return new WaitForSeconds(2f);
        _shuffling = true;
    }

    // Brute force shuffling.
    private void Shuffle()
    {
        // Debug.Log("Start Shuffle");
        // int count = 0;
        // int last = 0;
        // while (count < (rows * cols * cols))
        // {
        //     // Pick a random location.
        //     int rnd = Random.Range(0, rows * cols);
        //     // Only thing we forbid is undoing the last move.
        //     if (rnd == last)
        //     {
        //         continue;
        //     }
        //
        //     last = _emptyLocation;
        //     // Try surrounding spaces looking for valid move.
        //     if (SwapIfValid(rnd, -cols, cols))
        //     {
        //         count++;
        //     }
        //     else if (SwapIfValid(rnd, +cols, cols))
        //     {
        //         count++;
        //     }
        //     else if (SwapIfValid(rnd, -1, 0))
        //     {
        //         count++;
        //     }
        //     else if (SwapIfValid(rnd, +1, cols - 1))
        //     {
        //         count++;
        //     }
        // }
        
        Debug.Log("Start Shuffle");
        int count = 0;
        int lastMove = -1; // Track the last move to avoid reversing
        int shuffleAttempts = 100; // Define the number of shuffle attempts to ensure randomness

        while (count < shuffleAttempts)
        {
            // Pick a random location.
            int rnd = Random.Range(0, rows * cols);
        
            // Ensure we are not just undoing the last move
            if (rnd == lastMove)
            {
                continue;
            }

            // Try to move the piece and ensure it's a valid move
            if (SwapIfValid(rnd, -cols, cols) || 
                SwapIfValid(rnd, +cols, cols) || 
                SwapIfValid(rnd, -1, 0) || 
                SwapIfValid(rnd, +1, cols - 1))
            {
                lastMove = _emptyLocation; // Update the last move
                count++;
            }
        }

        // Check if the puzzle is already solved, and if so, reshuffle
        if (CheckCompletion())
        {
            Debug.Log("Puzzle was solved after shuffle, reshuffling...");
            Shuffle(); // Recursive call to reshuffle
        }
    }
    
    IEnumerator WaitAutoComplete(float duration)
    {
        yield return new WaitForSeconds(duration);
        // StartCoroutine(AutoCompletePuzzleVisual());
        
        if (!_isCompletedOnce)
        {
            StartCoroutine(AutoCompletePuzzle());
            
        }
    }
    
    
    private IEnumerator AutoCompletePuzzle()
    {
        float width = 1f / cols;
        float height = 1f / rows;

        for (int i = 0; i < _pieces.Count; i++)
        {
            // Update posisi lokal
            _pieces[i].localPosition = new Vector3(-1 + (2 * width * (i % cols)) + width,
                +1 - (2 * height * (i / cols)) - height,
                0);
            _pieces[i].name = $"{i}";

            // Update UV koordinat jika ada gambar yang digunakan
            Mesh mesh = _pieces[i].GetComponent<MeshFilter>().mesh;
            Vector2[] uv = new Vector2[4];
            float gap = 0.01f;  // Sama seperti gap yang digunakan dalam pembuatan awal
            uv[0] = new Vector2((width * (i % cols)) + gap, 1 - ((height * ((i / cols) + 1)) - gap));
            uv[1] = new Vector2((width * ((i % cols) + 1)) - gap, 1 - ((height * ((i / cols) + 1)) - gap));
            uv[2] = new Vector2((width * (i % cols)) + gap, 1 - ((height * (i / cols)) + gap));
            uv[3] = new Vector2((width * ((i % cols) + 1)) - gap, 1 - ((height * (i / cols)) + gap));

            // Assign UVs to the mesh
            mesh.uv = uv;

            // Force Unity to refresh the mesh
            _pieces[i].GetComponent<MeshFilter>().mesh = mesh;
            _pieces[i].GetComponent<MeshRenderer>().SetPropertyBlock(null); // Refresh material/block updates
        }

        _isCompletedOnce = true;
        
        Debug.Log("Puzzle Auto Complete");
        yield return new WaitForSeconds(5f);
        
        OnPuzzleComplete?.Invoke();
    }

    // private void AutoComplete()
    // {
    //     for (int i = 0; i < _pieces.Count; i++)
    //     {
    //         _pieces[i].localPosition = new Vector3(
    //             -1 + (2 * (1f / cols) * (i % cols)) + (1f/ cols),
    //             1 - (2 * (1f / rows) * (i/cols)) - (1f/rows),
    //             0
    //         );
    //
    //         _pieces[i].name = $"{i}";
    //     }
    //
    //     _emptyLocation = (rows * cols) - 1;
    //     _pieces[_emptyLocation].gameObject.SetActive(false);
    //     
    //     _isCompletedOnce = true;
    //     _shuffling = false;
    // }
    //
    // IEnumerator AutoCompletePuzzleVisual()
    // {
    //     for (int i = 0; i < _pieces.Count; i++)
    //     {
    //         if (i == _emptyLocation) continue;
    //
    //         Vector3 targetPosition = new Vector3(
    //             -1 + (2 * (1f / cols) * (i % cols)) + (1f/ cols),
    //             1 - (2 * (1f / rows) * (i / cols)) - (1f/rows),
    //             0);
    //         
    //         float duration = 0.5f;
    //
    //         Vector3 startPosition = _pieces[i].localPosition;
    //         float elapsedTime = 0f;
    //
    //         while (elapsedTime < duration)
    //         {
    //             _pieces[i].localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime/duration);
    //             elapsedTime += Time.deltaTime;
    //             yield return null;
    //         }
    //
    //         _pieces[i].localPosition = targetPosition;
    //     }
    //     
    //     yield return new WaitForSeconds(0.1f);
    //     
    //     _isCompletedOnce = true;
    //     _shuffling = false;
    //     
    //     Debug.Log("Puzzle Autocompleted with Visual Animation");
    //     
    //     yield return new WaitForSeconds(2f);
    //     OnPuzzleComplete?.Invoke();
    // }
}
