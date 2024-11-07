using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RD
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] UIHandler uiHandler;

        public Sprite customPlayerSprite;
        public Sprite customTailSprite;
        public Sprite customFoodSprite;

        public int maxHeight = 15;
        public int maxWidth = 17;

        public Color colour1;
        public Color colour2;
        public Color foodColour = Color.red;
        public Color playerColour;
        public Color obstacleColor = Color.black;  // Color of the obstacles

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

        bool up, left, right, down;

        int currentScore;    // probably won't have these and just complete a grid for victory
        int highScore;       // probably won't have these and just complete a grid for victory

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

        #region Init

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
        }

        public void StartNewGame()
        {
            ClearReferences();
            CreateMap();
            //method to calculate 10% of map 
            //convert to int for amount of food to spawn at start
            CreateObstacles(); 
            uiHandler.ResumeGame();
            PlacePlayer();
            PlaceCamera();
            
            CreateFood();

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


        void PlaceCamera()
        {
            Node n = GetNode(maxWidth / 2, maxHeight / 2);
            Vector3 p = n.worldPosition;
            p += Vector3.one * 0.5f;
            cameraHolder.position = p;
        }

        void CreateFood()
        {
            foodObject = new GameObject("Food");
            SpriteRenderer foodRenderer = foodObject.AddComponent<SpriteRenderer>();
            foodRenderer.sprite = customFoodSprite != null ? customFoodSprite : CreateSprite(foodColour);
            //foodRenderer.sprite = CreateSprite(foodColour);
            foodRenderer.sortingOrder = 1;
            PlaceFood();
        }

        void CreateObstacles()
        {
            obstacleParent = new GameObject("Obstacles");

            // Calculate 10% of the map area
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
                obstacleRenderer.sprite = CreateSprite(obstacleColor);
                obstacleRenderer.sortingOrder = 1;
            }
        }

        #endregion

        void SetSpeed(float speed)
        {
            moveRate = speed;
        }

        void LoadSpeedSettings()
        {
            int speedInt = PlayerPrefs.GetInt("speed", 3);  // Default is 3 (speed = 2)

            float speedToUse = GetMoveRateFromSpeed(speedInt);

            SetSpeed(speedToUse);
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


        void Update()
        {
            if (isGameOver)
            {
                return;
            }

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

        void GetInput()
        {
            up = Input.GetButtonDown("Up");
            down = Input.GetButtonDown("Down");
            left = Input.GetButtonDown("Left");
            right = Input.GetButtonDown("Right");
        }

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
            // If this is the first input, allow any direction.
            if (isFirstInput)
            {
                targetDirection = d;
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
                prevDirection = curDirection;  // Update previous direction
                curDirection = targetDirection; // Set current direction to the target direction
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
                    //carry on moving forward (prevdirection)
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

                if (targetNode == foodNode)
                {
                    isFood = true;
                    Destroy(foodObject);
                }

                Node previousNode = playerNode;
                availableNodes.Add(previousNode);

                if (isFood)
                {
                    tail.Add(CreateTailNode(previousNode.x, previousNode.y));
                    availableNodes.Remove(previousNode);
                    CreateFood();
                }

                MoveTail();

                playerObject.transform.rotation = Quaternion.Euler(0, 0, GetRotationForDirection(moveDirection));
                PlacePlayerObject(playerObject, targetNode.worldPosition);
                playerNode = targetNode;
                availableNodes.Remove(playerNode);

                if (isFood)
                {
                    if (availableNodes.Count > 0)
                    {
                        PlaceFood();
                    }
                    else
                    {
                        // Handle win condition if no available space
                    }
                }
            }
        }


        void MoveTail()
        {
            Node prevNode = null;

            for (int i = 0; i < tail.Count; i++)
            {
                SpecialNode tailSegment = tail[i];
                availableNodes.Add(tailSegment.node);

                // Set the direction and rotation for the tail segment based on its position relative to the next segment
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


        #region Utilities

        public void RestartGame()
        {
            onStart.Invoke();
            Time.timeScale = 1;
        }

        public void GameOver()
        {
            isGameOver = true;
            isFirstInput = false;
            uiHandler.GameEndMenu();
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

        void PlacePlayerObject(GameObject obj, Vector3 pos)
        {
            pos += Vector3.one * 0.5f;
            obj.transform.position = pos;
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

                foodObject.transform.localScale = Vector3.one * 0.7f; // Set to any desired scale factor
            }
            else
            {
                //No available space - Player wins!
            }
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

        #endregion
    }
}