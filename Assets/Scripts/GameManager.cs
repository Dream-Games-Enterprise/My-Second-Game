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
        #region Inspector Stuff
        #region REFERENCES
        ScoreManager scoreManager;
        [SerializeField] CustomisationManager customisationManager;
        [SerializeField] UIHandler uiHandler;
        GameOverUI gameOverUI;
        AudioManager audioManager;
        #endregion

        #region SCENE OBJECTS
        public Transform cameraHolder;
        [SerializeField] GameObject inputPanel;
        [SerializeField] GameObject twoButtonControl;
        [SerializeField] GameObject fourButtonControl;
        [SerializeField] GameObject foodPickupParticlePrefab;
        [SerializeField] GameObject snakeDeathParticlePrefab;
        GameObject playerObject;
        GameObject foodObject;
        GameObject tailParent;
        GameObject obstacleParent;
        GameObject mapObject;
        SpriteRenderer mapRenderer;
        public SpriteRenderer tailSpriteRenderer;
        public SpriteRenderer foodSpriteRenderer;
        #endregion

        #region CUSTOMISATION
        Sprite customPlayerSprite;
        public Sprite customTailSprite;
        public Sprite customFoodSprite;
        public Sprite customObstacleSprite;
        public Sprite customTrapSprite;
        Sprite playerSprite;

        public Color colour1;
        public Color colour2;
        public Color foodColour;
        public Color playerColour;
        public Color snakeTailColour;
        public Color trapColour = Color.black;

        public List<Sprite> tailSprites;
        public List<Sprite> foodSprites;

        int playerSkinIndex;
        int playerTailIndex;
        int trapIndexGetter;
        int foodIndex;

        int _snakeSkinIndex;
        int _tailSkinIndex;
        int _foodSkinIndex;
        int _trapSkinIndex;

        int _playerColourIndex;
        int _tailColourIndex;
        int _foodColourIndex;
        int _trapColourIndex;
        int _mapPrimaryColourIndex;
        int _mapSecondaryColourIndex;
        #endregion

        #region INPUT
        public Button upButton;
        public Button downButton;
        public Button leftButton;
        public Button rightButton;
        public Button twoLeftButton;
        public Button twoRightButton;

        bool up, left, right, down;

        Direction targetDirection;
        Direction curDirection;
        Direction prevDirection;

        public enum Direction { None, up, left, right, down }

        public bool isButtonControl;
        public float moveRate = 0.05f;
        public float smoothSpeed = 0.5f;

        float timer;
        bool isMoving = false;
        float moveDuration = 0.09f;
        float minSwipeDistance = 5f;
        Vector2 touchStartPos;
        Vector2 touchEndPos;

        List<Direction> inputBuffer = new List<Direction>();
        #endregion

        #region MAP / GRID
        public int maxHeight = 16;
        public int maxWidth = 16;
        int _mapWidth;
        int _mapHeight;
        bool _obstaclesEnabled;
        bool _isButtonControl;
        bool obstaclesToggle;
        bool isFourButtons;

        Node[,] grid;
        Node playerNode;
        Node prevPlayerNode;
        Node foodNode;

        List<Node> availableNodes = new List<Node>();
        List<SpecialNode> tail = new List<SpecialNode>();
        List<Node> obstacleNodes = new List<Node>();
        Dictionary<(int, int), GameObject> foodMap = new Dictionary<(int, int), GameObject>();
        Queue<GameObject> foodPool = new Queue<GameObject>();
        List<Node> validNodesBuffer = new List<Node>();

        Texture2D _mapTex;
        Sprite _mapSprite;
        #endregion

        #region EVENTS / GAME STATE
        public UnityEvent onStart;
        public UnityEvent onGameOver;
        public UnityEvent firstInput;
        public bool isGameOver;
        public bool isFirstInput;
        bool isPaused = false;
        #endregion

        #region RUNTIME / MOVEMENT CONTROL
        int initialFoodPoolSize;
        int _speedInt;
        bool isCameraAdjusting = false;
        bool cameraStartedAtMax = false;
        #endregion

        #region INTERNAL HELPERS
        Camera _mainCam;
        Dictionary<Direction, Vector2Int> _dirVectors;
        Dictionary<Direction, float> _dirAngles;
        #endregion

        #region PARTICLES
        Material foodParticleMaterial;
        Queue<ParticleSystem> particlePool = new Queue<ParticleSystem>();
        private readonly List<Transform> animatedFoodList = new List<Transform>();
        #endregion

        bool swipeDetected = false;

        private List<Vector3> tailTargetPositions = new List<Vector3>();

        #endregion
        void Awake()
        {
            if (!PlayerPrefs.HasKey("inputType"))
            {
                PlayerPrefs.SetInt("inputType", (int)InputType.Swipe);
                PlayerPrefs.Save();
            }
            LoadPlayerPrefs();
            LoadSpritesBits();
            FetchColours();

            foodParticleMaterial = new Material(Shader.Find("Sprites/Default"));
            audioManager = FindFirstObjectByType<AudioManager>();

            scoreManager = GetComponent<ScoreManager>();
            gameOverUI = GetComponent<GameOverUI>();

            currentInputType = (InputType)PlayerPrefs.GetInt(
                "inputType",
                (int)InputType.Swipe
            );

            isButtonControl = currentInputType != InputType.Swipe;
            useFourButtonControl = currentInputType == InputType.FourButtons;

            ApplyInputMode(currentInputType);

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

            if (Screen.dpi > 0)
                minSwipeDistance = Screen.dpi * 0.15f;
            else
                minSwipeDistance = 8f;
        }

        void Start()
        {
            SetSpeed(GetMoveRateFromSpeed(_speedInt));

            playerSkinIndex = customisationManager.GetSelectedSnakeIndex();
            playerTailIndex = customisationManager.GetSelectedTailIndex();
            trapIndexGetter = customisationManager.GetSelectedTrapIndex();
            customPlayerSprite = customisationManager.snakeSkins[playerSkinIndex].sprite;

            onStart.Invoke();
            maxWidth = PlayerPrefs.GetInt("width");
            maxHeight = PlayerPrefs.GetInt("height");
            StartNewGame();
        }

        void Update()
        {
            if (isGameOver)
                return;

            if (!isCameraAdjusting)
            {
                isCameraAdjusting = true;
                UpdateCameraPosition();
                isCameraAdjusting = false;
            }

            if (isButtonControl)
                GetInput();
            else
                HandleTouchInput();

            AnimateFoodScale();

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

        void LateUpdate()
        {
            float followSpeed = 20f;

            for (int i = 0; i < tail.Count; i++)
            {
                if (tail[i].obj == null || i >= tailTargetPositions.Count)
                    continue;

                Vector3 targetPos = tailTargetPositions[i];
                tail[i].obj.transform.position = Vector3.Lerp(tail[i].obj.transform.position, targetPos, followSpeed * Time.deltaTime);
            }
        }

        [SerializeField] GameObject swipePrompt;

        void ApplyInputMode(InputType mode)
        {
            bool swipe = mode == InputType.Swipe;
            bool two = mode == InputType.TwoButtons;
            bool four = mode == InputType.FourButtons;

            inputPanel.SetActive(swipe);
            twoButtonControl.SetActive(two);
            fourButtonControl.SetActive(four);

            swipePrompt.SetActive(swipe);

            ApplyInputListeners();
        }

        #region INPUT
        public enum InputType { Swipe = 0, TwoButtons = 1, FourButtons = 2 }
        private InputType currentInputType;

        public void SetControlMode(bool fourWay)
        {
            useFourButtonControl = fourWay;
            ApplyInputListeners();

            upButton.gameObject.SetActive(fourWay);
            downButton.gameObject.SetActive(fourWay);
            leftButton.gameObject.SetActive(fourWay);
            rightButton.gameObject.SetActive(fourWay);

            twoLeftButton.gameObject.SetActive(!fourWay);
            twoRightButton.gameObject.SetActive(!fourWay);
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

        void HandleTouchInput()
        {
            if (UIHandler.IsPaused) return;
            if (isButtonControl) return;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    touchStartPos = touch.position;
                    swipeDetected = false;
                }
                else if (touch.phase == TouchPhase.Moved && !swipeDetected)
                {
                    touchEndPos = touch.position;
                    Vector2 swipeDelta = touchEndPos - touchStartPos;

                    if (swipeDelta.magnitude >= minSwipeDistance)
                    {
                        DetectSwipe(touchStartPos, touchEndPos);
                        swipeDetected = true;
                    }
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    swipeDetected = false;
                }
            }

            #region MOUSE (for PC testing)
            if (Input.GetMouseButtonDown(0))
            {
                touchStartPos = Input.mousePosition;
                swipeDetected = false;
            }
            else if (Input.GetMouseButton(0) && !swipeDetected)
            {
                touchEndPos = Input.mousePosition;
                Vector2 swipeDelta = touchEndPos - touchStartPos;

                if (swipeDelta.magnitude >= minSwipeDistance)
                {
                    DetectSwipe(touchStartPos, touchEndPos);
                    swipeDetected = true;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                swipeDetected = false;
            }
            #endregion
        }

        public void DetectSwipe(Vector2 startPos, Vector2 endPos)
        {
            Vector2 swipeDirection = endPos - startPos;

            if (swipeDirection.sqrMagnitude >= minSwipeDistance * minSwipeDistance) 
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
            if (UIHandler.IsPaused) return;

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
            if (inputBuffer.Count > 2) return;

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

            if (foodMap.TryGetValue((targetNode.x, targetNode.y), out consumed))
            {
                animatedFoodList.Remove(consumed.transform);

                foodPosition = consumed.transform.position;
                consumed.SetActive(false);
                foodPool.Enqueue(consumed);
                foodMap.Remove((targetNode.x, targetNode.y));

                scoreManager.AddScore();
                isFood = true;
                audioManager?.PlayFoodPickup();
            }

            foreach (Transform food in animatedFoodList.ToList())
            {
                if (food == null || !food.gameObject.activeSelf)
                {
                    animatedFoodList.Remove(food);
                    continue;
                }

                Vector3 dist = food.position - (targetNode.worldPosition + Vector3.one * 0.5f);
                if (dist.magnitude < 0.01f)
                {
                    Debug.LogWarning("Force removing stuck food.");
                    food.gameObject.SetActive(false);
                    animatedFoodList.Remove(food);
                    foodPool.Enqueue(food.gameObject);
                }
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
                    ParticleSystem ps = GetPooledParticle();
                    ps.transform.position = foodPosition;

                    SpriteRenderer consumedSR = consumed.GetComponent<SpriteRenderer>();
                    if (consumedSR != null)
                    {
                        var main = ps.main;
                        main.startColor = consumedSR.color;

                        var psr = ps.GetComponent<ParticleSystemRenderer>();
                        if (psr != null)
                        {
                            psr.material = foodParticleMaterial;
                            psr.material.mainTexture = consumedSR.sprite.texture;
                        }
                    }

                    ps.Play();
                    StartCoroutine(RecycleAfter(ps, 2f));
                }

                /*THIS INCREMENTS THE CAMERA (making it zoom out)
                 * if (cameraStartedAtMax && _mainCam.orthographicSize < 12f)
                    _mainCam.orthographicSize = Mathf.Min(_mainCam.orthographicSize + 0.005f, 12f);*/
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
            int tailCount = tail.Count;

            while (tailTargetPositions.Count < tailCount)
                tailTargetPositions.Add(Vector3.zero);

            float headSectionScale = 0.75f;
            float midSectionScale = 0.7f; 
            float tailSectionScale = 0.65f;

            for (int i = 0; i < tailCount; i++)
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

                Direction segmentDir = GetDirectionFromTo(tailSegment.node, prevNode);
                tailSegment.obj.transform.rotation = Quaternion.Euler(0, 0, GetRotationForDirection(segmentDir));
                availableNodes.Remove(tailSegment.node);

                Vector3 centrePos = tailSegment.node.worldPosition + Vector3.one * 0.5f;
                tailTargetPositions[i] = centrePos;

                int section = (i * 3) / tailCount;
                float scale = section == 0
                    ? headSectionScale
                    : (section == 1 ? midSectionScale : tailSectionScale);

                tailSegment.obj.transform.localScale = new Vector3(scale, scale, 1f);

                if (i == tailCount - 1)
                    tailSegment.obj.transform.position = centrePos;
            }
        }

        Direction GetDirectionFromTo(Node from, Node to)
        {
            Vector2Int diff = new Vector2Int(to.x - from.x, to.y - from.y);

            if (diff == Vector2Int.up) return Direction.up;
            if (diff == Vector2Int.down) return Direction.down;
            if (diff == Vector2Int.left) return Direction.left;
            if (diff == Vector2Int.right) return Direction.right;

            return Direction.None;
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

            obstaclesToggle = _obstaclesEnabled;

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
            if (_mapTex == null || _mapTex.width != maxWidth || _mapTex.height != maxHeight)
            {
                _mapTex = new Texture2D(maxWidth, maxHeight);
                _mapTex.filterMode = FilterMode.Point;

                Rect rect = new Rect(0, 0, maxWidth, maxHeight);
                _mapSprite = Sprite.Create(_mapTex, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            }

            if (mapObject == null)
            {
                mapObject = new GameObject("Map");
                mapRenderer = mapObject.AddComponent<SpriteRenderer>();
                mapRenderer.sprite = _mapSprite;
            }

            grid = new Node[maxWidth, maxHeight];
            availableNodes.Clear();

            Color[] colorBuffer = new Color[maxWidth * maxHeight];

            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    int index = y * maxWidth + x;

                    Vector3 worldPos = new Vector3(x, y, 0);
                    Node n = new Node()
                    {
                        x = x,
                        y = y,
                        worldPosition = worldPos
                    };
                    grid[x, y] = n;
                    availableNodes.Add(n);

                    bool odd = (x + y) % 2 != 0;
                    colorBuffer[index] = odd ? colour1 : colour2;
                }
            }

            _mapTex.SetPixels(colorBuffer);
            _mapTex.Apply(); 
            mapRenderer.sprite = _mapSprite;
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
                Node node = availableNodes[i];

                bool isValid =
                    !isTailNode(node) &&
                    !isObstacleNode(node) &&
                    !foodMap.ContainsKey((node.x, node.y)) &&
                    node != playerNode;

                if (isValid)
                    validNodesBuffer.Add(node);
            }

            for (int i = 0; i < foodToSpawn && validNodesBuffer.Count > 0; i++)
            {
                int idx = Random.Range(0, validNodesBuffer.Count);
                Node node = validNodesBuffer[idx];
                validNodesBuffer.RemoveAt(idx);
                availableNodes.Remove(node);

                GameObject f = foodPool.Count > 0 ? foodPool.Dequeue() : CreatePooledFood();
                SetupFoodObject(f, node, 0.6f);
            }
        }

        void CreateFood()
        {
            validNodesBuffer.Clear();

            for (int i = 0; i < availableNodes.Count; i++)
            {
                Node n = availableNodes[i];

                bool isValid =
                    !isTailNode(n) &&
                    !isObstacleNode(n) &&
                    !foodMap.ContainsKey((n.x, n.y)) &&
                    n != playerNode;

                if (isValid)
                    validNodesBuffer.Add(n);
            }

            if (validNodesBuffer.Count == 0) return;

            Node node = validNodesBuffer[Random.Range(0, validNodesBuffer.Count)];
            availableNodes.Remove(node);

            GameObject f = foodPool.Count > 0 ? foodPool.Dequeue() : CreatePooledFood();

            SetupFoodObject(f, node, 0.7f);
        }

        void SetupFoodObject(GameObject food, Node node, float scale)
        {
            food.SetActive(true);

            var sr = food.GetComponent<SpriteRenderer>();
            sr.sprite = customFoodSprite ?? CreateSprite(Color.white);
            sr.color = foodColour;
            sr.enabled = true;

            food.transform.position = node.worldPosition + Vector3.one * 0.5f;
            food.transform.localScale = Vector3.one * scale;

            foodMap[(node.x, node.y)] = food;
            animatedFoodList.Add(food.transform);
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

            // Replace LINQ with a manual loop
            List<Node> potentialNodes = new List<Node>(availableNodes.Count);
            for (int i = 0; i < availableNodes.Count; i++)
            {
                Node node = availableNodes[i];
                if (!IsEdge(node))
                    potentialNodes.Add(node);
            }

            while (obstacleCount > 0 && potentialNodes.Count > 0)
            {
                int randomIndex = Random.Range(0, potentialNodes.Count);
                Node candidateNode = potentialNodes[randomIndex];

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

                potentialNodes.RemoveAt(randomIndex);
            }
        }

        bool CreatesDeadEnd(Node candidateNode)
        {
            HashSet<Node> tempObstacles = new HashSet<Node>(obstacleNodes);
            tempObstacles.Add(candidateNode);

            for (int i = 0; i < EightDirections.Length; i++)
            {
                int dx = EightDirections[i].dx;
                int dy = EightDirections[i].dy;
                Node neighbor = GetNode(candidateNode.x + dx, candidateNode.y + dy);
                if (neighbor == null)
                    continue;

                bool isTailNeighbor = false;
                for (int t = 0; t < tail.Count; t++)
                {
                    if (tail[t].node == neighbor)
                    {
                        isTailNeighbor = true;
                        break;
                    }
                }

                if (!tempObstacles.Contains(neighbor) && !isTailNeighbor)
                {
                    int freeCount = 0;

                    for (int j = 0; j < EightDirections.Length; j++)
                    {
                        int dx2 = EightDirections[j].dx;
                        int dy2 = EightDirections[j].dy;
                        Node adjacent = GetNode(neighbor.x + dx2, neighbor.y + dy2);

                        if (adjacent != null && !tempObstacles.Contains(adjacent))
                        {
                            bool isTailAdjacent = false;
                            for (int t = 0; t < tail.Count; t++)
                            {
                                if (tail[t].node == adjacent)
                                {
                                    isTailAdjacent = true;
                                    break;
                                }
                            }

                            if (!isTailAdjacent)
                            {
                                freeCount++;
                            }
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
            if (x < 0 || y < 0 || x >= maxWidth || y >= maxHeight) return true;

            Node node = grid[x, y];
            for (int i = 0; i < tail.Count; i++)
            {
                if (tail[i].node == node)
                    return true;
            }

            return tempObstacles.Contains(node);
        }

        void PlacePlayerObject(GameObject obj, Vector3 pos)
        {
            pos += Vector3.one * 0.5f;
            obj.transform.position = pos;
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
            StartCoroutine(PlayDeathAnimation());
            Time.timeScale = 1f;
            isFirstInput = false;
            scoreManager.ApplyEndMultipliers();
            uiHandler.GameEndMenu();
            gameOverUI.ActivateUI();

            inputPanel.SetActive(false);
            twoButtonControl.SetActive(false);
            fourButtonControl.SetActive(false);

            swipePrompt.SetActive(false);
        }


        IEnumerator PlayDeathAnimation()
        {
            List<GameObject> tailObjects = tail.Select(t => t.obj).ToList();
            List<GameObject> parts = new List<GameObject> { playerObject };
            parts.AddRange(tailObjects);

            List<SpriteRenderer> renderers = parts
                .Select(p => p != null ? p.GetComponent<SpriteRenderer>() : null)
                .Where(r => r != null).ToList();

            List<Color> originalColors = renderers.Select(r => r.color).ToList();

            AudioManager.Instance?.PlayFlash();

            for (int i = 0; i < 3; i++)
            {
                AudioManager.Instance?.PlayFlash();

                for (int j = 0; j < renderers.Count; j++)
                    renderers[j].color = Color.clear;

                yield return new WaitForSeconds(0.1f);

                for (int j = 0; j < renderers.Count; j++)
                    renderers[j].color = originalColors[j];

                yield return new WaitForSeconds(0.1f);
            }


            AudioManager.Instance?.PlayDeathExplosion();

            for (int i = 0; i < parts.Count; i++)
            {
                GameObject part = parts[i];
                if (part == null) continue;

                GameObject particleObj = Instantiate(snakeDeathParticlePrefab);
                particleObj.transform.position = part.transform.position;

                var ps = particleObj.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.startColor = originalColors[i];

                    ps.Play();
                }

                Destroy(particleObj, 2f);
            }

            foreach (var t in tail)
                if (t.obj != null)
                    t.obj.SetActive(false);

            if (playerObject != null)
                playerObject.SetActive(false);
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
            PlacePlayerObject(s.obj, s.node.worldPosition);
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

        ParticleSystem GetPooledParticle()
        {
            if (particlePool.Count > 0)
            {
                var ps = particlePool.Dequeue();
                ps.gameObject.SetActive(true);
                return ps;
            }

            var newGO = Instantiate(foodPickupParticlePrefab);
            var psNew = newGO.GetComponent<ParticleSystem>();
            newGO.SetActive(true);
            return psNew;
        }

        void RecycleParticle(ParticleSystem ps)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.gameObject.SetActive(false);
            particlePool.Enqueue(ps);
        }

        IEnumerator RecycleAfter(ParticleSystem ps, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (ps != null)
                RecycleParticle(ps);
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
            Vector3 mapCentre = new Vector3(maxWidth / 2f, maxHeight / 2f, cameraHolder.position.z);
            cameraHolder.position = Vector3.Lerp(cameraHolder.position, mapCentre, smoothSpeed);
        }

        void AdjustCameraSize()
        {
            float verticalSize = maxHeight / 2f;
            float horizontalSize = (maxWidth / Camera.main.aspect) / 2f;
            float requiredSize = Mathf.Max(verticalSize, horizontalSize);

            float padding = 0.5f;
            float adjustedSize = requiredSize + padding;

            Camera.main.orthographicSize = adjustedSize;

            cameraStartedAtMax = true;
        }

        /* OLD CAMERA LOGIC WITH FOLLOW ON LARGER MAPS
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
         }*/

        #endregion

        #region REFACTORS

        void AnimateFoodScale()
        {
            float baseScale = 0.65f;
            float oscillation = Mathf.Sin(Time.time * 5f) * 0.05f;
            Vector3 scale = Vector3.one * (baseScale + oscillation);

            for (int i = 0; i < animatedFoodList.Count; i++)
            {
                if (animatedFoodList[i] != null)
                    animatedFoodList[i].localScale = scale;
            }
        }

        void LoadSpritesBits()
        {
            int playerSkinIndex = _snakeSkinIndex;
            if (playerSkinIndex >= 0 && playerSkinIndex < customisationManager.snakeSkins.Count)
            {
                customPlayerSprite = customisationManager.snakeSkins[playerSkinIndex].sprite;
                Debug.Log("Player skin sprite loaded: " + customPlayerSprite);
            }
            else
            {
                Debug.LogWarning("Invalid snake index. Using default sprite.");
            }

            int tailSkinIndex = _tailSkinIndex;
            if (tailSkinIndex >= 0 && tailSkinIndex < customisationManager.tailSkins.Count)
            {
                customTailSprite = customisationManager.tailSkins[tailSkinIndex].sprite;
                Debug.Log("Tail skin sprite loaded: " + customTailSprite);
            }
            else
            {
                Debug.LogWarning("Invalid tail index. Using default tail sprite.");
            }

            int foodIndex = _foodSkinIndex;
            if (foodIndex >= 0 && foodIndex < customisationManager.foodSkins.Count)
            {
                customFoodSprite = customisationManager.foodSkins[foodIndex].sprite;
                Debug.Log("Food skin sprite loaded: " + customFoodSprite);
            }
            else
            {
                Debug.LogWarning("Invalid food index. Using default food sprite.");
            }

            int trapIndex = _trapSkinIndex;
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

        [SerializeField] bool useFourButtonControl = true; // THIS SHOULD NOW BE FETCHED FROM PLAYERPREFS whether to use two or four or ignore if swipe is being used

        void ApplyInputListeners()
        {
            Debug.Log($"[GameManager] ApplyInputListeners(): currentInputType={currentInputType}, useFourButtonControl={useFourButtonControl}");

            if (upButton != null) { upButton.onClick.RemoveAllListeners(); upButton.gameObject.SetActive(false); }
            if (downButton != null) { downButton.onClick.RemoveAllListeners(); downButton.gameObject.SetActive(false); }
            if (leftButton != null) { leftButton.onClick.RemoveAllListeners(); leftButton.gameObject.SetActive(false); }
            if (rightButton != null) { rightButton.onClick.RemoveAllListeners(); rightButton.gameObject.SetActive(false); }
            if (twoLeftButton != null) { twoLeftButton.onClick.RemoveAllListeners(); twoLeftButton.gameObject.SetActive(false); }
            if (twoRightButton != null) { twoRightButton.onClick.RemoveAllListeners(); twoRightButton.gameObject.SetActive(false); }

            if (currentInputType == InputType.Swipe)
                return;

            if (useFourButtonControl)
            {
                upButton.gameObject.SetActive(true);
                downButton.gameObject.SetActive(true);
                leftButton.gameObject.SetActive(true);
                rightButton.gameObject.SetActive(true);

                upButton.onClick.AddListener(() => OnArrowButtonPressed(Direction.up));
                downButton.onClick.AddListener(() => OnArrowButtonPressed(Direction.down));
                leftButton.onClick.AddListener(() => OnArrowButtonPressed(Direction.left));
                rightButton.onClick.AddListener(() => OnArrowButtonPressed(Direction.right));
            }
            else
            {
                twoLeftButton.gameObject.SetActive(true);
                twoRightButton.gameObject.SetActive(true);

                twoLeftButton.onClick.AddListener(() => OnTurnButtonPressed(false));
                twoRightButton.onClick.AddListener(() => OnTurnButtonPressed(true));
            }
        }

        void OnArrowButtonPressed(Direction d)
        {
            if (UIHandler.IsPaused) return;

            if (!isFirstInput)
            {
                isFirstInput = true;
                firstInput.Invoke();
            }

            inputBuffer.Clear();
            inputBuffer.Insert(0, d);
        }

        void OnTurnButtonPressed(bool turnRight)
        {
            if (UIHandler.IsPaused) return;

            if (!isFirstInput)
            {
                isFirstInput = true;
                firstInput.Invoke();
            }

            inputBuffer.Clear();
            var newDir = turnRight ? GetRightOf(curDirection) : GetLeftOf(curDirection);
            inputBuffer.Insert(0, newDir);
        }

        Direction GetLeftOf(Direction d)
        {
            switch (d)
            {
                case Direction.up: return Direction.left;
                case Direction.left: return Direction.down;
                case Direction.down: return Direction.right;
                case Direction.right: return Direction.up;
                default: return Direction.up;
            }
        }

        Direction GetRightOf(Direction d)
        {
            switch (d)
            {
                case Direction.up: return Direction.right;
                case Direction.right: return Direction.down;
                case Direction.down: return Direction.left;
                case Direction.left: return Direction.up;
                default: return Direction.up;
            }
        }

        void LoadPlayerPrefs()
        {
            _speedInt = PlayerPrefs.GetInt("speed", 3);
            _snakeSkinIndex = PlayerPrefs.GetInt("SelectedSnakeIndex", 0);
            _tailSkinIndex = PlayerPrefs.GetInt("SelectedTailIndex", 0);
            _foodSkinIndex = PlayerPrefs.GetInt("SelectedFoodIndex", 0);
            _trapSkinIndex = PlayerPrefs.GetInt("SelectedTrapIndex", 0);

            _playerColourIndex = PlayerPrefs.GetInt("SelectedColourIndex", 0);
            _tailColourIndex = PlayerPrefs.GetInt("SelectedTailColourIndex", 0);
            _foodColourIndex = PlayerPrefs.GetInt("SelectedFoodColourIndex", 0);
            _trapColourIndex = PlayerPrefs.GetInt("SelectedTrapColourIndex", 0);
            _mapPrimaryColourIndex = PlayerPrefs.GetInt("SelectedMapPrimaryColorIndex", 0);
            _mapSecondaryColourIndex = PlayerPrefs.GetInt("SelectedMapSecondaryColorIndex", 1);

            _mapWidth = PlayerPrefs.GetInt("width", 16);
            _mapHeight = PlayerPrefs.GetInt("height", 16);

            _obstaclesEnabled = PlayerPrefs.GetInt("obstacles", 1) == 1;
            _isButtonControl = PlayerPrefs.GetInt("inputType", 1) == 1;
        }

        #endregion 
    }
}