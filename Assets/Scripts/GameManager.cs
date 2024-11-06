using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RD
{
    public class GameManager : MonoBehaviour
    {
        public int maxHeight = 15;
        public int maxWidth = 17;

        public Color colour1;
        public Color colour2;
        public Color foodColour = Color.red;
        public Color playerColour;

        public Transform cameraHolder;

        GameObject playerObject;
        GameObject foodObject;
        GameObject tailParent;
        Node playerNode;
        Node prevPlayerNode;
        Node foodNode;
        Sprite playerSprite;

        GameObject mapObject;
        SpriteRenderer mapRenderer;

        Node[,] grid;
        List<Node> availableNodes = new List<Node>();
        List<SpecialNode> tail = new List<SpecialNode>();

        bool up, left, right, down;

        int currentScore;    // probably won't have these and just complete a grid for victory
        int highScore;       // probably won't have these and just complete a grid for victory

        public bool isGameOver;
        public bool isFirstInput;
        public float moveRate = 0.5f;
        float timer;

        Direction targetDirection;
        Direction curDirection;

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
            onStart.Invoke();
            maxWidth = PlayerPrefs.GetInt("width");
            maxHeight = PlayerPrefs.GetInt("height");
            StartNewGame();
        }

        public void StartNewGame()
        {
            ClearReferences();
            CreateMap();
            PlacePlayer();
            PlaceCamera();
            CreateFood();

            // Reset movement-related state variables
            isGameOver = false;
            isFirstInput = false; // Ensure the first input is needed to start movement
            curDirection = Direction.up; // Start by facing upwards (or any default direction)
            targetDirection = curDirection; // Set the target direction to match the current direction at the start

            // Any additional game state reset logic you need can go here
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
            availableNodes.Clear();
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
            playerSprite = CreateSprite(playerColour);
            playerRenderer.sprite = playerSprite;
            playerRenderer.sortingOrder = 1;

            // Select a random available node for the player
            int randomIndex = Random.Range(0, availableNodes.Count);
            playerNode = availableNodes[randomIndex];

            // Remove the chosen node from the list of available nodes, as the player occupies it
            availableNodes.Remove(playerNode);

            // Place the player at the selected node's position
            PlacePlayerObject(playerObject, playerNode.worldPosition);
            playerObject.transform.localScale = Vector3.one * 1.2f;

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
            foodRenderer.sprite = CreateSprite(foodColour);
            foodRenderer.sortingOrder = 1;
            PlaceFood();
        }

        #endregion

        void Update()
        {
            if (isGameOver)
            {
                return;
            }

            // Get input from the player
            GetInput();

            // If this is the first input, start the game and allow any direction
            if (!isFirstInput)
            {
                if (up || down || left || right) // If any direction is pressed, start the game
                {
                    isFirstInput = true;
                    firstInput.Invoke(); // Trigger any first input logic if necessary
                    SetPlayerDirection(); // Set the direction based on the first input
                }
            }
            else
            {
                // If not the first input, check the direction and prevent reverse directions
                SetPlayerDirection();

                // Handle player movement
                timer += Time.deltaTime;
                if (timer > moveRate)
                {
                    timer = 0;
                    curDirection = targetDirection; // Move in the target direction
                    MovePlayer(); // Move the player based on the current direction
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
            // Allow any direction on the first move, or prevent reversing direction after that
            if (isFirstInput || !isOppositeDir(d))
            {
                targetDirection = d;
            }
        }


        void MovePlayer()
        {
            int x = 0;
            int y = 0;

            switch (curDirection)
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
            if (targetNode == null)
            {
                //game over
                onGameOver.Invoke();
            }
            else
            {
                if (isTailNode(targetNode))
                {
                    //game over.
                    onGameOver.Invoke();
                }
                else
                {
                    bool isScore = false;

                    if (targetNode == foodNode)
                    {
                        isScore = true;
                    }

                    Node previousNode = playerNode;
                    availableNodes.Add(previousNode);

                    if (isScore)
                    {
                        tail.Add(CreateTailNode(previousNode.x, previousNode.y));
                        availableNodes.Remove(previousNode);
                    }

                    MoveTail();

                    PlacePlayerObject(playerObject, targetNode.worldPosition);
                    playerNode = targetNode;
                    availableNodes.Remove(playerNode);

                    if (isScore)
                    {
                        if (availableNodes.Count > 0)
                        {
                            PlaceFood();
                        }
                        else { /*win */ }
                    }
                }
            }
        }

        void MoveTail()
        {
            Node prevNode = null;

            for (int i = 0; i < tail.Count; i++)
            {
                SpecialNode p = tail[i];
                availableNodes.Add(p.node);

                if (i == 0)
                {
                    prevNode = p.node;
                    p.node = playerNode;
                }
                else
                {
                    Node prev = p.node;
                    p.node = prevNode;
                    prevNode = prev;
                }

                availableNodes.Remove(p.node);
                PlacePlayerObject(p.obj, p.node.worldPosition);
            }
        }

        #region Utilities

        public void RestartGame()
        {
            onStart.Invoke();
        }

        public void GameOver()
        {
            isGameOver = true;
            isFirstInput = false;
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

        void PlacePlayerObject(GameObject obj, Vector3 pos)
        {
            pos += Vector3.one * 0.5f;
            obj.transform.position = pos;
        }

        void PlaceFood()
        {
            int ran = Random.Range(0, availableNodes.Count);
            Node n = availableNodes[ran];
            PlacePlayerObject(foodObject, n.worldPosition);
            foodNode = n;
        }

        Node GetNode(int x, int y)
        {
            if (x < 0 || x > maxWidth - 1 || y < -0 || y > maxHeight - 1)
            {
                return null;
            }

            return grid[x, y];
        }

        SpecialNode CreateTailNode(int x, int y )
        {
            SpecialNode s = new SpecialNode();
            s.node = GetNode(x, y);
            s.obj = new GameObject();
            s.obj.transform.parent = tailParent.transform;
            s.obj.transform.position = s.node.worldPosition;
            s.obj.transform.localScale = Vector3.one * 0.95f;
            SpriteRenderer r = s.obj.AddComponent<SpriteRenderer>();
            r.sprite = playerSprite;
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