using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.UI;

namespace RD
{
    public class GameManager : MonoBehaviour
    {
        ScoreManager scoreManager;
        [SerializeField] CustomisationManager customisationManager;
        [SerializeField] UIHandler uiHandler;
        GameOverUI gameOverUI;

        public Transform cameraHolder;
        GameObject playerObject;
        GameObject foodObject;
        GameObject tailParent;
        GameObject obstacleParent;

        bool isCameraAdjusting = false;

        #region CUSTOMISATION 

        Sprite customPlayerSprite;
        public Sprite customTailSprite;
        public Sprite customFoodSprite;
        public Sprite customObstacleSprite;
        public Sprite customTrapSprite;

        public Color colour1;
        public Color colour2;
        public Color foodColour;
        public Color playerColour;
        public Color snakeTailColour;
        public Color trapColour = Color.black;

        Sprite playerSprite;

        public SpriteRenderer tailSpriteRenderer;
        public SpriteRenderer foodSpriteRenderer;

        public List<Sprite> tailSprites;
        public List<Sprite> foodSprites;

        int playerSkinIndex;
        int playerTailIndex;
        int trapIndexGetter;
        int foodIndex;

        #endregion

        #region INPUT VARIABLES

        [SerializeField] GameObject inputPanel;
        public Button upButton;
        public Button downButton;
        public Button leftButton;
        public Button rightButton;
        bool up, left, right, down;

        Direction targetDirection;
        Direction curDirection;
        Direction prevDirection;

        public enum Direction
        {
            None, up, left, right, down
        }

        [SerializeField] GameObject buttonControl;
        public bool isButtonControl; // Must be public to access from SwipeInput

        public float moveRate = 0.05f;
        float timer;

        Vector2 touchStartPos;
        Vector2 touchEndPos;
        float minSwipeDistance = 25f;

        public float smoothSpeed = 0.5f;

        bool isMoving = false;
        float moveDuration = 0.09f;

        #endregion

        #region MAP
        GameObject mapObject;
        SpriteRenderer mapRenderer;
        public int maxHeight = 16;
        public int maxWidth = 16;

        Node[,] grid;
        List<Node> availableNodes = new List<Node>();
        List<SpecialNode> tail = new List<SpecialNode>();
        List<Node> obstacleNodes = new List<Node>();
        Node playerNode;
        Node prevPlayerNode;
        Node foodNode;

        // Map each occupied node to its food GameObject
        private Dictionary<Node, GameObject> foodMap = new Dictionary<Node, GameObject>();

        Queue<GameObject> foodPool = new Queue<GameObject>();

        bool obstaclesToggle;

        #endregion

        #region EVENTS

        public UnityEvent onStart;
        public UnityEvent onGameOver;
        public UnityEvent firstInput;
        public bool isGameOver;
        public bool isFirstInput;
        bool isPaused = false;

        #endregion

        bool cameraStartedAtMax = false;

        int initialFoodPoolSize;
        [SerializeField] GameObject foodPickupParticlePrefab;
        Dictionary<GameObject, Coroutine> runningFoodTweens = new Dictionary<GameObject, Coroutine>();

        Camera _mainCam;
        Dictionary<Direction, Vector2Int> _dirVectors;
        Dictionary<Direction, float> _dirAngles;

        List<Node> validNodesBuffer = new List<Node>();


        void Awake()
        {
            LoadSpritesBits();

            FetchColours();

            scoreManager = GetComponent<ScoreManager>();
            gameOverUI = GetComponent<GameOverUI>();

            isButtonControl = PlayerPrefs.GetInt("inputType", 1) == 1;
            Debug.Log("Input type is button control: " + isButtonControl);

            ToggleInputType(isButtonControl);

            _mainCam = Camera.main;

            _dirVectors = new Dictionary<Direction, Vector2Int>
{
    { Direction.up,    new Vector2Int( 0,  1) },
    { Direction.down,  new Vector2Int( 0, -1) },
    { Direction.left,  new Vector2Int(-1,  0) },
    { Direction.right, new Vector2Int( 1,  0) }
};

            _dirAngles = new Dictionary<Direction, float>
{
    { Direction.up,     0f },
    { Direction.left,  90f },
    { Direction.down, 180f },
    { Direction.right,270f },
    { Direction.None,   0f }
};

        }

        void Start()
        {
            LoadSpeedSettings();
            int snakeIndex = PlayerPrefs.GetInt("SelectedSnakeIndex", 0);
            int foodIndex = PlayerPrefs.GetInt("SelectedFoodIndex", 0);
            int trapIndex = PlayerPrefs.GetInt("SelectedTrapIndex", 0);

            playerSkinIndex = customisationManager.GetSelectedSnakeIndex();
            playerTailIndex = customisationManager.GetSelectedTailIndex();
            trapIndexGetter = customisationManager.GetSelectedTrapIndex();
            customPlayerSprite = customisationManager.snakeSkins[playerSkinIndex].sprite;

            onStart.Invoke();
            maxWidth = PlayerPrefs.GetInt("width");
            maxHeight = PlayerPrefs.GetInt("height");
            StartNewGame();

            ApplyInputListeners();
        }

        void Update()
        {
            if (isGameOver)
            {
                return;
            }

            if (!isCameraAdjusting)
            {
                isCameraAdjusting = true;
                UpdateCameraPosition();
                isCameraAdjusting = false;
            }

            HandleTouchInput();
            GetInput();

            if (!isFirstInput)
            {
                if (up || down || left || right)
                {
                    isFirstInput = true;
                    firstInput.Invoke();
                    QueueImmediateInput();
                }
            }
            else
            {
                QueueImmediateInput();

                timer += Time.deltaTime;
                if (timer >= moveRate)
                {
                    timer = 0f;

                    for (int i = 0; i < inputBuffer.Count; i++)
                    {
                        Direction d = inputBuffer[i];
                        if (!isOppositeDir(d))
                        {
                            targetDirection = d;
                            inputBuffer.RemoveAt(i);
                            break;
                        }
                    }

                    if (curDirection == Direction.None)
                    {
                        curDirection = targetDirection;
                    }

                    MovePlayer();
                }
            }
        }

        #region INPUT

        List<Direction> inputBuffer = new List<Direction>();

        void ToggleInputType(bool useButtons)
        {
            isButtonControl = useButtons;
            buttonControl.SetActive(useButtons);

            PlayerPrefs.SetInt("inputType", useButtons ? 1 : 0);
            PlayerPrefs.Save();
        }

        void OnArrowButtonPressed(Direction direction)
        {
            if (!isFirstInput)
            {
                isFirstInput = true;
                firstInput.Invoke();
            }

            // Clear and override swipe buffer with just the swipe (like a fresh input)
            inputBuffer.Insert(0, direction);
        }


        float GetMoveRateFromSpeed(int speed)
        {
            switch (speed)
            {
                case 1:
                    return 0.3f; // Was 0.3f / 1.4f (~0.214), now slightly slower (~0.25)
                case 2:
                    return 0.25f / 1.5f; // ~0.167
                case 3:
                    return 0.2f / 1.5f;  // ~0.133 (default)
                case 4:
                    return 0.15f / 1.5f; // ~0.100
                case 5:
                    return 0.1f / 1.5f;  // ~0.067
                default:
                    return 0.2f / 1.5f;  // fallback
            }
        }

        public void ToggleInputButtonPressed()
        {
            ToggleInputType(!isButtonControl);
        }

        void HandleTouchInput()
        {
            if (!isButtonControl)
            {
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);

                    if (touch.phase == TouchPhase.Began)
                    {
                        touchStartPos = touch.position;
                    }
                    else if (touch.phase == TouchPhase.Ended)
                    {
                        touchEndPos = touch.position;
                        DetectSwipe(touchStartPos, touchEndPos);
                        touchStartPos = Vector2.zero;
                    }

                    else if (touch.phase == TouchPhase.Ended)
                    {
                        touchStartPos = Vector2.zero;
                    }
                }

                #region MOUSE
                if (Input.GetMouseButtonDown(0))
                {
                    touchStartPos = Input.mousePosition;
                }
                else if (Input.GetMouseButton(0))
                {
                    touchEndPos = Input.mousePosition;
                    DetectSwipe(touchStartPos, touchEndPos);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    touchEndPos = Input.mousePosition;
                    DetectSwipe(touchStartPos, touchEndPos);
                    touchStartPos = Vector2.zero;
                }

                #endregion
            }
        }

        public void DetectSwipe(Vector2 startPos, Vector2 endPos)
        {
            Vector2 swipeDirection = endPos - startPos;

            if (swipeDirection.magnitude >= minSwipeDistance)
            {
                swipeDirection.Normalize();

                if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
                {
                    if (swipeDirection.x > 0 && !isOppositeDir(Direction.right))
                        OnArrowButtonPressed(Direction.right);
                    else if (swipeDirection.x < 0 && !isOppositeDir(Direction.left))
                        OnArrowButtonPressed(Direction.left);
                }
                else
                {
                    if (swipeDirection.y > 0 && !isOppositeDir(Direction.up))
                        OnArrowButtonPressed(Direction.up);
                    else if (swipeDirection.y < 0 && !isOppositeDir(Direction.down))
                        OnArrowButtonPressed(Direction.down);
                }
            }
        }

        void GetInput()
        {
            up = Input.GetButtonDown("Up");
            down = Input.GetButtonDown("Down");
            left = Input.GetButtonDown("Left");
            right = Input.GetButtonDown("Right");
        }

        #endregion

        #region MOVEMENT

        void SetPlayerDirection()
        {
            if (up)
            {
                SetDirection(Direction.up);
            }
            else if (down)
            {
                SetDirection(Direction.down);
            }
            else if (left)
            {
                SetDirection(Direction.left);
            }
            else if (right)
            {
                SetDirection(Direction.right);
            }
        }

        void SetDirection(Direction d)
        {
            inputBuffer.Add(d);
        }

        void QueueImmediateInput()
        {
            if (up) inputBuffer.Add(Direction.up);
            else if (down) inputBuffer.Add(Direction.down);
            else if (left) inputBuffer.Add(Direction.left);
            else if (right) inputBuffer.Add(Direction.right);
        }

        float GetRotationForDirection(Direction direction)
        {
            return _dirAngles.TryGetValue(direction, out float angle) ? angle : 0f;
        }

        void MovePlayer()
        {
            if (curDirection == Direction.None || isMoving)
                return;

            curDirection = targetDirection;

            Vector2Int delta = _dirVectors.TryGetValue(curDirection, out var d) ? d : Vector2Int.zero;
            Node targetNode = GetNode(playerNode.x + delta.x, playerNode.y + delta.y);

            if ((tail.Count > 0 && isTailNode(targetNode)) || isObstacleNode(targetNode))
            {
                if (targetNode == tail[0].node) return;
                onGameOver.Invoke();
                return;
            }
            if (targetNode == null)
            {
                onGameOver.Invoke();
                return;
            }

            bool isFood = false;
            Vector3 foodPosition = Vector3.zero;
            GameObject consumed = null;

            if (foodMap.TryGetValue(targetNode, out consumed))
            {
                if (runningFoodTweens.TryGetValue(consumed, out var tween))
                {
                    StopCoroutine(tween);
                    runningFoodTweens.Remove(consumed);
                }

                foodPosition = consumed.transform.position;

                consumed.SetActive(false);

                foodPool.Enqueue(consumed);
                foodMap.Remove(targetNode);

                scoreManager.AddScore();
                isFood = true;
            }

            Node previousNode = playerNode;
            availableNodes.Add(previousNode);

            if (isFood)
            {
                tail.Add(CreateTailNode(previousNode.x, previousNode.y));
                availableNodes.Remove(previousNode);

                CreateFood();

                if (foodPickupParticlePrefab != null && consumed != null)
                {
                    GameObject fx = Instantiate(foodPickupParticlePrefab, foodPosition, Quaternion.identity);
                    var ps = fx.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        var main = ps.main;
                        main.startColor = consumed.GetComponent<SpriteRenderer>().color;

                        var psr = fx.GetComponent<ParticleSystemRenderer>();
                        if (psr != null)
                        {
                            var mat = new Material(Shader.Find("Sprites/Default"));
                            mat.mainTexture = consumed.GetComponent<SpriteRenderer>().sprite.texture;
                            psr.material = mat;
                        }
                    }
                    Destroy(fx, 2f);
                }

                if (cameraStartedAtMax && _mainCam.orthographicSize < 12f)
                    _mainCam.orthographicSize = Mathf.Min(_mainCam.orthographicSize + 0.005f, 12f);
            }

            MoveTail();

            playerObject.transform.rotation = Quaternion.Euler(0, 0, GetRotationForDirection(curDirection));
            StartCoroutine(SmoothMove(playerObject, playerNode.worldPosition, targetNode.worldPosition));

            playerNode = targetNode;
            availableNodes.Remove(playerNode);
        }

        IEnumerator SmoothMove(GameObject obj, Vector3 startPos, Vector3 endPos)
        {
            isMoving = true;

            startPos += Vector3.one * 0.5f;
            endPos += Vector3.one * 0.5f;

            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                obj.transform.position = Vector3.Lerp(startPos, endPos, elapsed / moveDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            obj.transform.position = endPos;
            isMoving = false;
        }

        void MoveTail()
        {
            Node prevNode = null;

            for (int i = 0; i < tail.Count; i++)
            {
                SpecialNode tailSegment = tail[i];
                availableNodes.Add(tailSegment.node);

                if (i == 0)
                {
                    prevNode = tailSegment.node;
                    tailSegment.node = playerNode;
                }
                else
                {
                    Node previousSegmentNode = tailSegment.node;
                    tailSegment.node = prevNode;
                    prevNode = previousSegmentNode;
                }

                Vector2 direction = tailSegment.node.worldPosition - tail[i].node.worldPosition;
                tailSegment.obj.transform.rotation = Quaternion.Euler(0, 0, GetRotationForDirection(curDirection));

                availableNodes.Remove(tailSegment.node);
                PlacePlayerObject(tailSegment.obj, tailSegment.node.worldPosition);
            }
        }

        #endregion

        #region SETUP
        void InitializePools()
        {
            int totalTiles = maxWidth * maxHeight;
            initialFoodPoolSize = Mathf.FloorToInt(totalTiles * 0.05f);
            initialFoodPoolSize = Mathf.Max(initialFoodPoolSize, 5);

            for (int i = 0; i < initialFoodPoolSize; i++)
            {
                foodPool.Enqueue(CreatePooledFood());

            }
        }

        public void StartNewGame()
        {
            ClearReferences();
            CreateMap();
            InitializePools();

            if (obstaclesToggle)
            {
                CreateObstacles();
            }

            int totalMapNodes = maxWidth * maxHeight;
            int initialFoodCount = Mathf.FloorToInt(totalMapNodes * 0.05f);

            SpawnInitialFood();

            uiHandler.ResumeGame();

            PlacePlayer();
            PlaceCamera();
            AdjustCameraSize();

            isGameOver = false;
            isFirstInput = false;
            curDirection = Direction.None;
            targetDirection = Direction.None;
        }

        public void ClearReferences()
        {
            if (mapObject != null)
                Destroy(mapObject);

            if (playerObject != null)
                Destroy(playerObject);

            if (foodObject != null)
                Destroy(foodObject);

            foreach (var t in tail)
            {
                Destroy(t.obj);
            }
            tail.Clear();

            if (obstacleParent != null)
            {
                foreach (Transform obstacle in obstacleParent.transform)
                {
                    Destroy(obstacle.gameObject);
                }
                Destroy(obstacleParent);
            }

            availableNodes.Clear();
            availableNodes.AddRange(obstacleNodes);
            obstacleNodes.Clear();

            grid = null;
        }

        void SetSpeed(float speed)
        {
            moveRate = speed;
        }

        void LoadSpeedSettings()
        {
            int speedInt = PlayerPrefs.GetInt("speed", 3);  // Default is 3 (speed = 2)
            float speedToUse = GetMoveRateFromSpeed(speedInt);
            SetSpeed(speedToUse);

            bool obstaclesEnabled = PlayerPrefs.GetInt("obstacles", 1) == 1;  // Default is 1 (enabled)
            Debug.Log("Obstacles Enabled: " + obstaclesEnabled);

            obstaclesToggle = obstaclesEnabled;
        }

        void CreateMap()
        {
            mapObject = new GameObject("Map");
            mapRenderer = mapObject.AddComponent<SpriteRenderer>();

            grid = new Node[maxWidth, maxHeight];

            Texture2D txt = new Texture2D(maxWidth, maxHeight);
            for (int x = 0; x < maxWidth; x++)
            {
                for (int y = 0; y < maxHeight; y++)
                {
                    Vector3 tp = Vector3.zero;
                    tp.x = x;
                    tp.y = y;

                    Node n = new Node()
                    {
                        x = x,
                        y = y,
                        worldPosition = tp
                    };

                    grid[x, y] = n;

                    availableNodes.Add(n);

                    #region Visual

                    if (x % 2 != 0)
                    {
                        if (y % 2 != 0)
                        {
                            txt.SetPixel(x, y, colour1);
                        }
                        else { txt.SetPixel(x, y, colour2); }
                    }
                    else
                    {
                        if (y % 2 != 0)
                        {
                            txt.SetPixel(x, y, colour2);
                        }
                        else { txt.SetPixel(x, y, colour1); }
                    }

                    #endregion
                }
            }

            txt.filterMode = FilterMode.Point;

            txt.Apply();
            Rect rect = new Rect(0, 0, maxWidth, maxHeight);
            Sprite sprite = Sprite.Create(txt, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            mapRenderer.sprite = sprite;
        }

        void PlacePlayer()
        {
            playerObject = new GameObject("Player");
            SpriteRenderer playerRenderer = playerObject.AddComponent<SpriteRenderer>();

            playerRenderer.sprite = customPlayerSprite;
            playerRenderer.color = playerColour;

            playerRenderer.sortingOrder = 1;

            int randomIndex = Random.Range(0, availableNodes.Count);
            playerNode = availableNodes[randomIndex];

            availableNodes.Remove(playerNode);
            PlacePlayerObject(playerObject, playerNode.worldPosition);
            playerObject.transform.localScale = Vector3.one * 1.05f;

            tailParent = new GameObject("TailParent");
        }

        GameObject CreatePooledFood(string name = "PooledFood")
        {
            GameObject food = new GameObject(name);
            SpriteRenderer renderer = food.AddComponent<SpriteRenderer>();
            renderer.sprite = customFoodSprite != null ? customFoodSprite : CreateSprite(Color.white);
            renderer.color = foodColour;
            renderer.sortingOrder = 1;

            food.transform.localScale = Vector3.one * 0.6f;
            food.SetActive(false);
            return food;
        }

        void SpawnInitialFood()
        {
            foodMap.Clear();
            int totalTiles = maxWidth * maxHeight;
            int foodToSpawn = Mathf.Max(Mathf.FloorToInt(totalTiles * 0.05f), 1);

            validNodesBuffer.Clear();
            for (int i = 0; i < availableNodes.Count; i++)
            {
                var node = availableNodes[i];
                if (!isTailNode(node))
                    validNodesBuffer.Add(node);
            }

            for (int i = 0; i < foodToSpawn && validNodesBuffer.Count > 0; i++)
            {
                int idx = Random.Range(0, validNodesBuffer.Count);
                Node node = validNodesBuffer[idx];
                validNodesBuffer.RemoveAt(idx);
                availableNodes.Remove(node);

                GameObject f = foodPool.Count > 0
                    ? foodPool.Dequeue()
                    : CreatePooledFood();

                f.SetActive(true);

                var sr = f.GetComponent<SpriteRenderer>();
                sr.sprite = customFoodSprite ?? CreateSprite(Color.white);
                sr.color = foodColour;
                sr.enabled = true;

                f.transform.position = node.worldPosition + Vector3.one * 0.5f;
                f.transform.localScale = Vector3.one * 0.6f;

                foodMap[node] = f;

                if (runningFoodTweens.ContainsKey(f))
                {
                    StopCoroutine(runningFoodTweens[f]);
                    runningFoodTweens.Remove(f);
                }
                var tween = StartCoroutine(TweenFoodScale(f));
                runningFoodTweens[f] = tween;
            }
        }




        void CreateFood()
        {
            validNodesBuffer.Clear();
            for (int i = 0; i < availableNodes.Count; i++)
            {
                var n = availableNodes[i];
                if (!isTailNode(n))
                    validNodesBuffer.Add(n);
            }
            if (validNodesBuffer.Count == 0) return;

            Node node = validNodesBuffer[Random.Range(0, validNodesBuffer.Count)];


            availableNodes.Remove(node);

            GameObject f = foodPool.Count > 0
                ? foodPool.Dequeue()
                : CreatePooledFood();

            f.SetActive(true);

            var sr = f.GetComponent<SpriteRenderer>();
            sr.sprite = customFoodSprite ?? CreateSprite(Color.white);
            sr.color = foodColour;
            sr.enabled = true;

            f.transform.position = node.worldPosition + Vector3.one * 0.5f;
            f.transform.localScale = Vector3.one * 0.7f;

            foodMap[node] = f;

            if (runningFoodTweens.ContainsKey(f))
            {
                StopCoroutine(runningFoodTweens[f]);
                runningFoodTweens.Remove(f);
            }
            var tween = StartCoroutine(TweenFoodScale(f));
            runningFoodTweens[f] = tween;
        }

        void PlaceFood()
        {
            List<Node> validNodes = new List<Node>(availableNodes);

            validNodes.Remove(playerNode);
            foreach (var t in tail)
            {
                validNodes.Remove(t.node);
            }

            foreach (var obstacle in obstacleNodes)
            {
                validNodes.Remove(obstacle);
            }

            if (validNodes.Count > 0)
            {
                int ran = Random.Range(0, validNodes.Count);
                Node n = validNodes[ran];
                PlacePlayerObject(foodObject, n.worldPosition);
                foodNode = n; 

                foodObject.transform.localScale = Vector3.one * 0.6f; 
            }
            else
            {
                TriggerVictory();
            }
        }

        void PlaceObstacles(int obstacleCount)
        {
            List<Node> potentialNodes = new List<Node>(availableNodes);

            while (obstacleCount > 0 && potentialNodes.Count > 0)
            {
                Node candidateNode = potentialNodes[Random.Range(0, potentialNodes.Count)];

                obstacleNodes.Add(candidateNode);


                availableNodes.Remove(candidateNode);
                obstacleCount--;


                potentialNodes.Remove(candidateNode);
            }
        }

        bool IsEdge(Node node)
        {
            return node.x == 0 || node.y == 0 || node.x == maxWidth - 1 || node.y == maxHeight - 1;
        }

        private static readonly (int dx, int dy)[] EightDirections = new (int, int)[]
        {
            (-1, -1), (0, -1), (1, -1),
            (-1,  0),          (1,  0),
            (-1,  1), (0,  1), (1,  1)
        };

        void CreateObstacles()
        {
            obstacleParent = new GameObject("Obstacles");

            int obstacleCount = Mathf.FloorToInt(maxWidth * maxHeight * 0.05f);
            // Exclude edge nodes from potential placement
            List<Node> potentialNodes = availableNodes.Where(node => !IsEdge(node)).ToList();

            while (obstacleCount > 0 && potentialNodes.Count > 0)
            {
                int randomIndex = Random.Range(0, potentialNodes.Count);
                Node candidateNode = potentialNodes[randomIndex];

                // Only place the obstacle if it does not create a dead end
                if (!CreatesDeadEnd(candidateNode))
                {
                    obstacleNodes.Add(candidateNode);
                    availableNodes.Remove(candidateNode);
                    obstacleCount--;

                    GameObject obstacleObj = new GameObject("Obstacle");
                    obstacleObj.transform.parent = obstacleParent.transform;
                    PlacePlayerObject(obstacleObj, candidateNode.worldPosition);

                    SpriteRenderer obstacleRenderer = obstacleObj.AddComponent<SpriteRenderer>();
                    obstacleRenderer.sprite = customObstacleSprite != null ? customObstacleSprite : CreateSprite(trapColour);
                    obstacleRenderer.color = trapColour;
                    obstacleRenderer.sortingOrder = 1;
                    obstacleObj.transform.localScale = Vector3.one * 0.9f;
                }

                potentialNodes.Remove(candidateNode);
            }
        }

        bool CreatesDeadEnd(Node candidateNode)
        {
            HashSet<Node> tempObstacles = new HashSet<Node>(obstacleNodes);
            tempObstacles.Add(candidateNode);

            foreach (var (dx, dy) in EightDirections)
            {
                Node neighbor = GetNode(candidateNode.x + dx, candidateNode.y + dy);
                if (neighbor == null)
                    continue;

                if (!tempObstacles.Contains(neighbor) && !tail.Any(t => t.node == neighbor))
                {
                    int freeCount = 0;

                    foreach (var (dx2, dy2) in EightDirections)
                    {
                        Node adjacent = GetNode(neighbor.x + dx2, neighbor.y + dy2);

                        if (adjacent != null && !tempObstacles.Contains(adjacent) && !tail.Any(t => t.node == adjacent))
                        {
                            freeCount++;
                        }
                    }

                    if (freeCount < 6)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool IsBlocked(int x, int y, HashSet<Node> tempObstacles)
        {
            // Check grid boundaries
            if (x < 0 || y < 0 || x >= maxWidth || y >= maxHeight) return true;

            // Check if the node is an obstacle or part of the snake (using the temporary obstacles set)
            Node node = grid[x, y];
            return tempObstacles.Contains(node) || tail.Any(t => t.node == node);
        }

        void PlacePlayerObject(GameObject obj, Vector3 pos)
        {
            pos += Vector3.one * 0.5f;
            obj.transform.position = pos;
        }

        #endregion

        #region CHECKS

        bool isOppositeDir(Direction d)
        {
            switch (d)
            {
                case Direction.up:
                    return curDirection == Direction.down;
                case Direction.down:
                    return curDirection == Direction.up;
                case Direction.left:
                    return curDirection == Direction.right;
                case Direction.right:
                    return curDirection == Direction.left;
                default:
                    return false;
            }
        }

        bool isTailNode(Node n)
        {
            for (int i = 0; i < tail.Count; i++)
            {
                if (tail[i].node == n)
                {
                    return true;
                }
            }

            return false;
        }

        bool isObstacleNode(Node node)
        {
            return obstacleNodes.Contains(node);
        }

        #endregion

        #region UTILITIES

        public void RestartGame()
        {
            onStart.Invoke();
            Time.timeScale = 1;
        }

        public void GameOver()
        {
            isGameOver = true;
            Time.timeScale = 1f;
            isFirstInput = false;
            scoreManager.ApplyEndMultipliers();
            uiHandler.GameEndMenu();
            gameOverUI.ActivateUI();
            inputPanel.SetActive(false);
        }

        void TriggerVictory()
        {
            isGameOver = true;
            Time.timeScale = 0.3f;

            scoreManager.ApplyEndMultipliers();
            scoreManager.AddWinMultiplier();

            gameOverUI.ActivateUI(true);
        }

        Node GetNode(int x, int y)
        {
            if (x < 0 || x >= maxWidth || y < 0 || y >= maxHeight)
            {
                return null;
            }

            return grid[x, y];
        }

        SpecialNode CreateTailNode(int x, int y)
        {
            SpecialNode s = new SpecialNode();
            s.node = GetNode(x, y);
            s.obj = new GameObject();
            s.obj.transform.parent = tailParent.transform;
            s.obj.transform.position = s.node.worldPosition;
            s.obj.transform.localScale = Vector3.one * 0.75f;
            SpriteRenderer r = s.obj.AddComponent<SpriteRenderer>();

            r.sprite = customTailSprite != null ? customTailSprite : playerSprite;

            r.color = snakeTailColour;

            r.sortingOrder = 1;

            return s;
        }

        Sprite CreateSprite(Color targetColour)
        {
            Texture2D txt = new Texture2D(1, 1);
            txt.SetPixel(0, 0, targetColour);
            txt.Apply();
            txt.filterMode = FilterMode.Point;
            Rect rect = new Rect(0, 0, 1, 1);
            return Sprite.Create(txt, rect, Vector2.one * 0.5f, 1, 0, SpriteMeshType.FullRect);
        }

        IEnumerator TweenFoodScale(GameObject foodObject)
        {
            Vector3 minScale = Vector3.one * 0.6f;
            Vector3 maxScale = Vector3.one * 0.7f;
            float duration = 0.5f;

            while (foodObject != null && foodObject.transform != null)
            {
                yield return TweenScale(foodObject.transform, minScale, maxScale, duration);

                if (foodObject == null || foodObject.transform == null)
                    yield break;

                yield return TweenScale(foodObject.transform, maxScale, minScale, duration);
            }
        }

        IEnumerator TweenScale(Transform target, Vector3 start, Vector3 end, float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                if (target == null)
                    yield break;

                target.localScale = Vector3.Lerp(start, end, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (target != null)
                target.localScale = end;
        }

        void FetchColours()
        {
            int playerColourIndex = PlayerPrefs.GetInt("SelectedColourIndex", 0);
            if (playerColourIndex >= 0 && playerColourIndex < customisationManager.snakeColours.Count)
            {
                playerColour = customisationManager.snakeColours[playerColourIndex];
                Debug.Log("Player color loaded: " + playerColour);
            }
            else
            {
                Debug.LogWarning("Invalid player color index. Using default color.");
                playerColour = new Color(0.980f, 0.976f, 0.965f, 1.000f);
            }

            int tailColourIndex = PlayerPrefs.GetInt("SelectedTailColourIndex", 0);
            if (tailColourIndex >= 0 && tailColourIndex < customisationManager.snakeColours.Count)
            {
                snakeTailColour = customisationManager.snakeColours[tailColourIndex];
                Debug.Log("Tail color loaded: " + snakeTailColour);
            }
            else
            {
                Debug.LogWarning("Invalid tail color index. Using default color.");
                snakeTailColour = new Color(0.980f, 0.976f, 0.965f, 1.000f);
            }

            int foodColourIndex = PlayerPrefs.GetInt("SelectedFoodColourIndex", 0);
            if (foodColourIndex >= 0 && foodColourIndex < customisationManager.snakeColours.Count)
            {
                foodColour = customisationManager.snakeColours[foodColourIndex];
                Debug.Log("Food color loaded: " + foodColour);
            }
            else
            {
                Debug.LogWarning("Invalid food color index. Using default color.");
                foodColour = new Color(0.980f, 0.976f, 0.965f, 1.000f);
            }

            int trapColourIndex = PlayerPrefs.GetInt("SelectedTrapColourIndex", 0);
            if (trapColourIndex >= 0 && trapColourIndex < customisationManager.snakeColours.Count)
            {
                trapColour = customisationManager.snakeColours[trapColourIndex];
                Debug.Log("Trap color loaded: " + trapColour);
            }
            else
            {
                Debug.LogWarning("Invalid trap color index. Using default color.");
                trapColour = new Color(0.980f, 0.976f, 0.965f, 1.000f);
            }

            int mapPrimaryIndex = PlayerPrefs.GetInt("SelectedMapPrimaryColorIndex", 0);
            if (mapPrimaryIndex >= 0 && mapPrimaryIndex < customisationManager.mapColours.Count)
            {
                colour1 = customisationManager.mapColours[mapPrimaryIndex];
                Debug.Log("Map Primary color loaded: " + colour1);
            }
            else
            {
                Debug.LogWarning("Invalid map primary color index. Using default color.");
                colour1 = Color.gray;
            }

            int mapSecondaryIndex = PlayerPrefs.GetInt("SelectedMapSecondaryColorIndex", 1);
            if (mapSecondaryIndex >= 0 && mapSecondaryIndex < customisationManager.mapColours.Count)
            {
                colour2 = customisationManager.mapColours[mapSecondaryIndex];
                Debug.Log("Map Secondary color loaded: " + colour2);
            }
            else
            {
                Debug.LogWarning("Invalid map secondary color index. Using default color.");
                colour2 = Color.black;
            }
        }

        #endregion

        #region CAMERA

        void PlaceCamera()
        {
            Node n = GetNode(maxWidth / 2, maxHeight / 2);
            Vector3 p = n.worldPosition;
            p += Vector3.one * 0.5f;
            cameraHolder.position = p;
        }

        void UpdateCameraPosition()
        {
            if (!cameraStartedAtMax)
            {
                Vector3 mapCenter = new Vector3(maxWidth / 2f, maxHeight / 2f, cameraHolder.position.z);
                cameraHolder.position = Vector3.Lerp(cameraHolder.position, mapCenter, smoothSpeed);
                return;
            }

            float cameraSize = Camera.main.orthographicSize;
            Vector3 playerPosition = playerObject.transform.position;
            Vector3 desiredPosition = playerPosition;

            float halfWidth = maxWidth * 0.5f;
            float halfHeight = maxHeight * 0.5f;

            float cameraHorizontalLimit = halfWidth - cameraSize;
            float cameraVerticalLimit = halfHeight - cameraSize;

            desiredPosition.x = Mathf.Clamp(desiredPosition.x, cameraHorizontalLimit, halfWidth + cameraSize);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, cameraVerticalLimit, halfHeight + cameraSize);

            cameraHolder.position = Vector3.Lerp(cameraHolder.position, desiredPosition, smoothSpeed);
        }

        void AdjustCameraSize()
        {
            float verticalSize = maxHeight / 2f;
            float horizontalSize = (maxWidth / Camera.main.aspect) / 2f;
            float requiredSize = Mathf.Max(verticalSize, horizontalSize);

            float padding = 0.5f;
            float adjustedSize = requiredSize + padding;

            if (adjustedSize > 8.2f)
            {
                Camera.main.orthographicSize = 8.2f;
                cameraStartedAtMax = true;
            }
            else
            {
                Camera.main.orthographicSize = adjustedSize;
                cameraStartedAtMax = false;
            }
        }

        #endregion

        #region REFACTORS

        void LoadSpritesBits()
        {
            int playerSkinIndex = PlayerPrefs.GetInt("SelectedSnakeIndex", 0);
            if (playerSkinIndex >= 0 && playerSkinIndex < customisationManager.snakeSkins.Count)
            {
                customPlayerSprite = customisationManager.snakeSkins[playerSkinIndex].sprite;
                Debug.Log("Player skin sprite loaded: " + customPlayerSprite);
            }
            else
            {
                Debug.LogWarning("Invalid snake index. Using default sprite.");
            }

            int tailSkinIndex = PlayerPrefs.GetInt("SelectedTailIndex", 0);
            if (tailSkinIndex >= 0 && tailSkinIndex < customisationManager.tailSkins.Count)
            {
                customTailSprite = customisationManager.tailSkins[tailSkinIndex].sprite;
                Debug.Log("Tail skin sprite loaded: " + customTailSprite);
            }
            else
            {
                Debug.LogWarning("Invalid tail index. Using default tail sprite.");
            }

            int foodIndex = PlayerPrefs.GetInt("SelectedFoodIndex", 0);
            if (foodIndex >= 0 && foodIndex < customisationManager.foodSkins.Count)
            {
                customFoodSprite = customisationManager.foodSkins[foodIndex].sprite;
                Debug.Log("Food skin sprite loaded: " + customFoodSprite);
            }
            else
            {
                Debug.LogWarning("Invalid food index. Using default food sprite.");
            }

            int trapIndex = PlayerPrefs.GetInt("SelectedTrapIndex", 0);
            if (trapIndex >= 0 && trapIndex < customisationManager.trapSkins.Count)
            {
                customTrapSprite = customisationManager.trapSkins[trapIndex].sprite;
                customObstacleSprite = customTrapSprite;
                Debug.Log("Trap skin sprite loaded: " + customTrapSprite);
            }
            else
            {
                Debug.LogWarning("Invalid trap index. Using default trap sprite.");
            }
        }

        void ApplyInputListeners()
        {
            upButton.onClick.AddListener(() => OnArrowButtonPressed(Direction.up));
            downButton.onClick.AddListener(() => OnArrowButtonPressed(Direction.down));
            leftButton.onClick.AddListener(() => OnArrowButtonPressed(Direction.left));
            rightButton.onClick.AddListener(() => OnArrowButtonPressed(Direction.right));
        }

        #endregion 
    }
}