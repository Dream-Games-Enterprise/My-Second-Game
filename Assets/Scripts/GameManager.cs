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
        [SerializeField] UIHandler uiHandler;
        GameOverUI gameOverUI;

        public Sprite customPlayerSprite;
        public Sprite customTailSprite;
        public Sprite customFoodSprite;
        public Sprite customObstacleSprite; 

        public int maxHeight = 15;
        public int maxWidth = 17;

        public Color colour1;
        public Color colour2;
        public Color foodColour = Color.red;
        public Color playerColour;
        public Color obstacleColor = Color.black; 

        public Transform cameraHolder;

        GameObject playerObject;
        GameObject foodObject;
        GameObject tailParent;
        GameObject obstacleParent;  // Parent object to hold all obstacles
        Node playerNode;
        Node prevPlayerNode;
        Node foodNode;
        Sprite playerSprite;

        GameObject mapObject;
        SpriteRenderer mapRenderer;

        Node[,] grid;
        List<Node> availableNodes = new List<Node>();
        List<SpecialNode> tail = new List<SpecialNode>();
        List<Node> obstacleNodes = new List<Node>();

        [SerializeField] GameObject inputPanel;
        public Button upButton;
        public Button downButton;
        public Button leftButton;
        public Button rightButton;
        bool up, left, right, down;
        bool obstaclesToggle;

        public bool isGameOver;
        public bool isFirstInput;
        public float moveRate = 0.2f;
        float timer;

        Direction targetDirection;
        Direction curDirection;
        Direction prevDirection;

        public enum Direction
        {
            up, left, right, down
        }

        public UnityEvent onStart;
        public UnityEvent onGameOver;
        public UnityEvent firstInput;

        [SerializeField] GameObject buttonControl;
        bool isButtonControl; 


        Vector2 touchStartPos;
        Vector2 touchEndPos;
        float minSwipeDistance = 50f;

        public List<GameObject> foodObjects;
        List<Node> foodNodes = new List<Node>();
        bool isPaused = false;

        public float smoothSpeed = 0.1f;
        bool isCameraAdjusting = false;

        void Awake()
        {
            scoreManager = GetComponent<ScoreManager>();
            gameOverUI = GetComponent<GameOverUI>();

            isButtonControl = PlayerPrefs.GetInt("inputType", 1) == 1;

            Debug.Log("Input type is button control: " + isButtonControl);

            ToggleInputType(isButtonControl); // Call to toggle swipeText visibility
        }

        void Start()
        {
            LoadSpeedSettings();
            int snakeIndex = PlayerPrefs.GetInt("SelectedSnakeIndex", 0);
            int tailIndex = PlayerPrefs.GetInt("SelectedTailIndex", 0);
            int foodIndex = PlayerPrefs.GetInt("SelectedFoodIndex", 0);

            CustomisationManager customisation = FindObjectOfType<CustomisationManager>(); // Or load it if it’s in a different scene

            if (customisation != null)
            {
                customPlayerSprite = customisation.snakeSprites[snakeIndex];
                customTailSprite = customisation.tailSprites[tailIndex];
                customFoodSprite = customisation.foodSprites[foodIndex];
            }

            onStart.Invoke();
            maxWidth = PlayerPrefs.GetInt("width");
            maxHeight = PlayerPrefs.GetInt("height");
            StartNewGame();

            upButton.onClick.AddListener(() => OnArrowButtonPressed(Direction.up));
            downButton.onClick.AddListener(() => OnArrowButtonPressed(Direction.down));
            leftButton.onClick.AddListener(() => OnArrowButtonPressed(Direction.left));
            rightButton.onClick.AddListener(() => OnArrowButtonPressed(Direction.right));
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
                AdjustCameraSize();
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
                    SetPlayerDirection();
                }
            }
            else
            {
                SetPlayerDirection();

                timer += Time.deltaTime;
                if (timer >= moveRate)
                {
                    timer = 0f;
                    curDirection = targetDirection;
                    MovePlayer();
                }
            }
        }

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
            AdjustCameraSize();

            float cameraSize = Camera.main.orthographicSize;
            Vector3 playerPosition = playerObject.transform.position;

            // Define thresholds for small, medium, and large maps
            float smallMapThreshold = 6f;
            float mediumMapThreshold = 8f;
            float largeMapThreshold = 10f;

            bool isSmallMap = maxWidth <= smallMapThreshold && maxHeight <= smallMapThreshold;
            bool isMediumMap = maxWidth <= mediumMapThreshold && maxHeight <= mediumMapThreshold;
            bool isLargeMap = maxWidth > largeMapThreshold || maxHeight > largeMapThreshold;

            if (isSmallMap)
            {
                // For small maps, center the camera on the map without following the player
                Vector3 mapCenter = new Vector3(maxWidth / 2f, maxHeight / 2f, cameraHolder.position.z);
                cameraHolder.position = Vector3.Lerp(cameraHolder.position, mapCenter, smoothSpeed);
            }
            else if (isMediumMap)
            {
                // For medium maps, adjust the camera to follow the player, and allow it to move beyond the map
                cameraHolder.position = Vector3.Lerp(cameraHolder.position, playerPosition, smoothSpeed);

                // Adjust the camera size based on the medium map size
                Camera.main.orthographicSize = Mathf.Lerp(cameraSize, Mathf.Max(maxWidth, maxHeight) / 2f, 0.1f);

                // Allow camera to go beyond the map boundaries like large maps
                float halfWidth = maxWidth * 0.5f;
                float halfHeight = maxHeight * 0.5f;

                // The camera should be able to move beyond the map boundaries, hence removing the clamp
                cameraHolder.position = new Vector3(
                    Mathf.Clamp(cameraHolder.position.x, -halfWidth, halfWidth),
                    Mathf.Clamp(cameraHolder.position.y, -halfHeight, halfHeight),
                    cameraHolder.position.z
                );
            }
            else if (isLargeMap)
            {
                // For large maps, fully follow the player with clamped camera boundaries
                Vector3 desiredPosition = playerPosition;

                // Define boundaries for camera movement based on the map size
                float halfWidth = maxWidth * 0.5f;
                float halfHeight = maxHeight * 0.5f;

                float cameraHorizontalLimit = halfWidth - cameraSize;
                float cameraVerticalLimit = halfHeight - cameraSize;

                // Ensure the camera doesn't go out of bounds
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, cameraHorizontalLimit, halfWidth + cameraSize);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, cameraVerticalLimit, halfHeight + cameraSize);


                cameraHolder.position = Vector3.Lerp(cameraHolder.position, desiredPosition, smoothSpeed);
            }
        }

        void AdjustCameraSize()
        {
            float cameraSize = Camera.main.orthographicSize;

            if (maxWidth > 20 && maxHeight > 20)
            {
                Camera.main.orthographicSize = Mathf.Lerp(cameraSize, 10f, 0.1f);
            }
            else if (maxWidth > 8 && maxHeight > 8)
            {
                Camera.main.orthographicSize = Mathf.Lerp(cameraSize, 8f, 0.1f);
            }
            else
            {
                Camera.main.orthographicSize = Mathf.Lerp(cameraSize, 6f, 0.1f);
            }
        }

        #endregion

        #region SETUP

        public void StartNewGame()
        {
            ClearReferences();
            CreateMap();

            if (obstaclesToggle)
            {
                CreateObstacles();
            }

            int totalMapNodes = maxWidth * maxHeight;
            int initialFoodCount = Mathf.FloorToInt(totalMapNodes * 0.05f);
            //int initialFoodCount = 3;

            SpawnInitialFood(initialFoodCount);

            uiHandler.ResumeGame();

            PlacePlayer();  // Place the player first
            PlaceCamera();  // Now place the camera to center on the player

            AdjustCameraSize();

            isGameOver = false;
            isFirstInput = false;
            curDirection = Direction.up;
            targetDirection = curDirection;
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

            // Use custom player sprite
            playerRenderer.sprite = customPlayerSprite != null ? customPlayerSprite : CreateSprite(playerColour);
            playerRenderer.sortingOrder = 1;

            int randomIndex = Random.Range(0, availableNodes.Count);
            playerNode = availableNodes[randomIndex];

            availableNodes.Remove(playerNode);
            PlacePlayerObject(playerObject, playerNode.worldPosition);
            playerObject.transform.localScale = Vector3.one * 1.05f;

            tailParent = new GameObject("TailParent");
        }

        void SpawnInitialFood(int foodToSpawn)
        {
            foodObjects.Clear();
            foodNodes.Clear();

            for (int i = 0; i < foodToSpawn; i++)
            {
                List<Node> validNodes = availableNodes.Where(n => !isTailNode(n)).ToList();

                if (validNodes.Count == 0)
                {
                    Debug.LogWarning("No valid nodes available to spawn more food items.");
                    break;
                }

                int randomIndex = Random.Range(0, validNodes.Count);
                Node foodNode = validNodes[randomIndex];

                availableNodes.Remove(foodNode);
                foodNodes.Add(foodNode);

                GameObject foodObject = new GameObject("Food");
                SpriteRenderer foodRenderer = foodObject.AddComponent<SpriteRenderer>();
                foodRenderer.sprite = customFoodSprite != null ? customFoodSprite : CreateSprite(foodColour);
                foodRenderer.sortingOrder = 1;

                PlacePlayerObject(foodObject, foodNode.worldPosition);
                foodObject.transform.localScale = Vector3.one * 0.6f;

                foodObjects.Add(foodObject);
                StartCoroutine(TweenFoodScale(foodObject));
            }
        }

        void CreateFood()
        {
            List<Node> validNodes = availableNodes.Where(n => !isTailNode(n)).ToList();
            if (validNodes.Count == 0)
            {
                Debug.LogWarning("No valid nodes available for food placement.");
                return;
            }

            int randomIndex = Random.Range(0, validNodes.Count);
            Node foodNode = validNodes[randomIndex];

            availableNodes.Remove(foodNode);
            foodNodes.Add(foodNode);

            GameObject foodObject = new GameObject("Food");
            SpriteRenderer foodRenderer = foodObject.AddComponent<SpriteRenderer>();
            foodRenderer.sprite = customFoodSprite != null ? customFoodSprite : CreateSprite(foodColour);
            foodRenderer.sortingOrder = 1;

            PlacePlayerObject(foodObject, foodNode.worldPosition);
            foodObject.transform.localScale = Vector3.one * 0.7f;

            foodObjects.Add(foodObject);

            StartCoroutine(TweenFoodScale(foodObject));
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
                foodNode = n; // Store the node of the food for tracking

                foodObject.transform.localScale = Vector3.one * 0.6f; // Set to any desired scale factor
            }
            else
            {
                TriggerVictory();
            }
        }

        void CreateObstacles()
        {
            obstacleParent = new GameObject("Obstacles");

            // Calculate 5% of the map area
            int obstacleCount = Mathf.FloorToInt(maxWidth * maxHeight * 0.05f);

            for (int i = 0; i < obstacleCount; i++)
            {
                if (availableNodes.Count == 0) break;

                int randomIndex = Random.Range(0, availableNodes.Count);
                Node obstacleNode = availableNodes[randomIndex];
                availableNodes.RemoveAt(randomIndex);
                obstacleNodes.Add(obstacleNode);

                GameObject obstacleObj = new GameObject("Obstacle");
                obstacleObj.transform.parent = obstacleParent.transform;
                PlacePlayerObject(obstacleObj, obstacleNode.worldPosition);

                SpriteRenderer obstacleRenderer = obstacleObj.AddComponent<SpriteRenderer>();

                obstacleRenderer.sprite = customObstacleSprite != null ? customObstacleSprite : CreateSprite(obstacleColor);

                obstacleRenderer.color = obstacleColor;

                obstacleRenderer.sortingOrder = 1;

                // Scale the obstacle to 0.9f
                obstacleObj.transform.localScale = Vector3.one * 0.9f;
            }
        }

        void PlacePlayerObject(GameObject obj, Vector3 pos)
        {
            pos += Vector3.one * 0.5f;
            obj.transform.position = pos;
        }

        #endregion

        #region INPUT

        void ToggleInputType(bool useButtons)
        {
            isButtonControl = useButtons;
            buttonControl.SetActive(useButtons);

            // Save the player’s input type preference in PlayerPrefs
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

            SetDirection(direction);
        }

        float GetMoveRateFromSpeed(int speed)
        {
            switch (speed)
            {
                case 1:
                    return 0.3f;
                case 2:
                    return 0.25f;
                case 3:
                    return 0.2f;
                case 4:
                    return 0.15f;
                case 5:
                    return 0.1f;
                default:
                    return 0.2f;  // Default speed if no valid value is found
            }
        }

        public void ToggleInputButtonPressed()
        {
            ToggleInputType(!isButtonControl);
        }

        void HandleTouchInput()
        {
            if (!isButtonControl && Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    touchStartPos = touch.position;
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    touchEndPos = touch.position;
                    Vector2 swipeDirection = touchEndPos - touchStartPos;

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

                        touchStartPos = touchEndPos;
                    }
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
            if (isFirstInput)
            {
                targetDirection = d;
                curDirection = targetDirection;
            }
            else if (!isOppositeDir(d))
            {
                targetDirection = d;
            }
        }

        float GetRotationForDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.up:
                    return 0f;
                case Direction.left:
                    return 90f;
                case Direction.down:
                    return 180f;
                case Direction.right:
                    return 270f;
                default:
                    return 0f;
            }
        }

        void MovePlayer()
        {
            int x = 0;
            int y = 0;

            Direction moveDirection = curDirection;

            if (isOppositeDir(targetDirection))
            {
                moveDirection = prevDirection; // Continue in the previous direction if reversing
            }
            else
            {
                prevDirection = curDirection;
                curDirection = targetDirection;
            }

            switch (moveDirection)
            {
                case Direction.up:
                    y = 1;
                    break;
                case Direction.down:
                    y = -1;
                    break;
                case Direction.left:
                    x = -1;
                    break;
                case Direction.right:
                    x = 1;
                    break;
            }

            Node targetNode = GetNode(playerNode.x + x, playerNode.y + y);

            if (tail.Count > 0 && isTailNode(targetNode) || isObstacleNode(targetNode))
            {
                if (targetNode == tail[0].node)
                {
                    // Carry on moving forward (prevdirection)
                    return;
                }
                else { onGameOver.Invoke(); }
            }
            else if (targetNode == null)
            {
                // If the player moves out of bounds (i.e., targetNode is null), game over.
                onGameOver.Invoke();
            }
            else
            {
                bool isFood = false;

                for (int i = 0; i < foodNodes.Count; i++)
                {
                    if (foodNodes[i] == targetNode)
                    {
                        isFood = true;

                        Destroy(foodObjects[i]);
                        foodObjects.RemoveAt(i);
                        foodNodes.RemoveAt(i);

                        scoreManager.AddScore();

                        break;
                    }
                }

                Node previousNode = playerNode;
                availableNodes.Add(previousNode);

                if (isFood)
                {
                    tail.Add(CreateTailNode(previousNode.x, previousNode.y));
                    availableNodes.Remove(previousNode);
                    foodNodes.Remove(targetNode);
                    CreateFood();
                }

                MoveTail();

                playerObject.transform.rotation = Quaternion.Euler(0, 0, GetRotationForDirection(moveDirection));
                PlacePlayerObject(playerObject, targetNode.worldPosition);
                playerNode = targetNode;
                availableNodes.Remove(playerNode);
            }
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

                // Calculate rotation for the tail segment based on its new position
                Vector2 direction = tailSegment.node.worldPosition - tail[i].node.worldPosition;
                tailSegment.obj.transform.rotation = Quaternion.Euler(0, 0, GetRotationForDirection(curDirection));

                availableNodes.Remove(tailSegment.node);
                PlacePlayerObject(tailSegment.obj, tailSegment.node.worldPosition);
            }
        }

        #endregion

        #region CHECKS

        bool IsDeadEnd(Node node)
        {
            int x = node.x;
            int y = node.y;

            bool upBlocked = IsBlocked(x, y + 1);
            bool downBlocked = IsBlocked(x, y - 1);
            bool leftBlocked = IsBlocked(x - 1, y);
            bool rightBlocked = IsBlocked(x + 1, y);

            return upBlocked && downBlocked && leftBlocked && rightBlocked;
        }

        bool IsBlocked(int x, int y)
        {
            Node targetNode = GetNode(x, y);

            if (targetNode == null) return true; // Out of bounds
            if (obstacleNodes.Contains(targetNode)) return true; // Blocked by obstacle
            if (tail.Exists(t => t.node == targetNode)) return true; // Blocked by tail

            return false; // Not blocked
        }

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

        #region Utilities

        public void RestartGame()
        {
            onStart.Invoke();
            Time.timeScale = 1;
        }

        public void GameOver()
        {
            isGameOver = true;
            Time.timeScale = 1f; //set to 0.3f once particle system of dying snake is added
            isFirstInput = false;
            scoreManager.ApplyEndMultipliers();
            uiHandler.GameEndMenu();
            gameOverUI.ActivateUI();
            inputPanel.SetActive(false);
        }

        void TriggerVictory()
        {
            isGameOver = true;
            Time.timeScale = 0.3f; // Slow down time to celebrate the win

            scoreManager.ApplyEndMultipliers(); 
            scoreManager.AddWinMultiplier    (); 

            gameOverUI.ActivateUI(true);
        }

        Node GetNode(int x, int y)
        {
            if (x < 0 || x > maxWidth - 1 || y < -0 || y > maxHeight - 1)
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

            // Use custom tail sprite
            r.sprite = customTailSprite != null ? customTailSprite : playerSprite;
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
            // Define the tweening scale values
            Vector3 minScale = Vector3.one * 0.6f;
            Vector3 maxScale = Vector3.one * 0.7f;

            float duration = 0.5f; // Duration of one tween cycle (expand or shrink)

            while (foodObject != null)  // Loop until foodObject is destroyed
            {
                // Scale up
                yield return TweenScale(foodObject.transform, minScale, maxScale, duration);

                // Scale down
                yield return TweenScale(foodObject.transform, maxScale, minScale, duration);
            }
        }

        IEnumerator TweenScale(Transform target, Vector3 start, Vector3 end, float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                target.localScale = Vector3.Lerp(start, end, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            target.localScale = end;
        }

        #endregion
    }
}