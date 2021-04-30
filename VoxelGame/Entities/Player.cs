using OpenTK;
using OpenTK.Input;
using System;
using VoxelGame.Assets;
using VoxelGame.Blocks;
using VoxelGame.Worlds;
using VoxelGame.Containers;
using VoxelGame.Physics;
using VoxelGame.Rendering;
using VoxelGame.UI;
using VoxelGame.UI.Menus;

namespace VoxelGame.Entities
{
    /// <summary>
    /// Player entity
    /// </summary>
    public class Player : Entity, IDamageable
    {
        public const int MAX_HEALTH = 20;
        public const int MAX_HUNGER = 20;

        // Health attributes
        private float _currentHealth = MAX_HEALTH;
        private float _currentHunger = MAX_HUNGER;

        // Movement attributes
        private bool _hasHadInitialSet;     // Initial position set
        private World _currentWorld;        // World player is in
        private Rigidbody _rigidbody;       // Player colision model
        private Vector3 _vel;               // Player velocity
        private bool _isInWater;            // Is player in water
        private bool _isSprinting;          // Is player sprinting

        private float _walkSpeed = 3.5f;    // Default walk speed
        private float _runSpeed = 6f;       // Defualt running speed

        // Input attributes
        private static bool _controlsEnabled = true;
        private static bool _mouseHidden = true;

        // UI & Inventory
        private CraftingContainer _craftingInventory = new CraftingContainer();
        private PlayerInventory _inventory = new PlayerInventory();
        private PauseMenu _pauseMenu = new PauseMenu();

        // Textures
        private Texture _heartIcon;
        private Texture _heartHalfIcon;
        private Texture _heartEmptyIcon;

        private Texture _hungerIcon;
        private Texture _hungerHalfIcon;
        private Texture _hungerEmptyIcon;

        // Hunger & health update rates
        private float _hungerLossTickRate = .5f;    // Tick rate of hunger loss
        private float _hungerLossAmount = 0.01f;    // Amount of hunger lost per tick
        private float _lastHungerLossTick;          // Last time hunger loss ticked
        private float _healthIncreaseTickRate = 4;  // Tick rate of health regen
        private float _lastHealthIncreaseTick;      // Last time helath regen ticked

        public override Rigidbody Collision => _rigidbody;

        /// <summary>
        /// Players inventory
        /// </summary>
        public Container Inventory => _inventory;

        /// <summary>
        /// Players health
        /// </summary>
        public int Health
        {
            get => (int)Math.Ceiling(_currentHealth);
            set => _currentHealth = value;
        }

        public override void Begin()
        {
            // Setup player
            Name = "Player";
            _currentWorld = World.Instance;
            _hasHadInitialSet = false;

            // Load textures
            _heartIcon = AssetDatabase.GetAsset<Texture>("Textures/GUI/heart.png");
            _heartHalfIcon = AssetDatabase.GetAsset<Texture>("Textures/GUI/heart_half.png");
            _heartEmptyIcon = AssetDatabase.GetAsset<Texture>("Textures/GUI/heart_empty.png");

            _hungerIcon = AssetDatabase.GetAsset<Texture>("Textures/GUI/hunger.png");
            _hungerHalfIcon = AssetDatabase.GetAsset<Texture>("Textures/GUI/hunger_half.png");
            _hungerEmptyIcon = AssetDatabase.GetAsset<Texture>("Textures/GUI/hunger_empty.png");

            // Bind input actions
            Input.GetAction("Pause").KeyDown += InputPause;
            Input.GetAction("Jump").KeyDown += InputJump;
            Input.GetAction("Interact").KeyDown += InputInteract;
            Input.GetAction("Destroy Block").KeyDown += InputDestroyBlock;
            Input.GetAction("Inventory").KeyDown += InputInventory;
            Program.Window.MouseWheel += InputMouseWheel;
            Input.GetAction("Sprint").KeyDown += InputSprintDown;
            Input.GetAction("Sprint").KeyUp += InputSprintUp;
        }

        public override void Destroyed()
        {
            // Unbind actions
            Input.GetAction("Pause").KeyDown -= InputPause;
            Input.GetAction("Jump").KeyDown -= InputJump;
            Input.GetAction("Interact").KeyDown -= InputInteract;
            Input.GetAction("Destroy Block").KeyDown -= InputDestroyBlock;
            Input.GetAction("Inventory").KeyDown -= InputInventory;
            Program.Window.MouseWheel -= InputMouseWheel;
            Input.GetAction("Sprint").KeyDown -= InputSprintDown;
            Input.GetAction("Sprint").KeyUp -= InputSprintUp;

            base.Destroyed();
        }

        /// <summary>
        /// Mouse wheel handler
        /// </summary>
        private void InputMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Scroll through the toolbar
            if (e.Delta > 0)
                _inventory.SelectedItemIndex--;
            else
                _inventory.SelectedItemIndex++;

            if (_inventory.SelectedItemIndex > _inventory.ContainerSize.X - 1)
                _inventory.SelectedItemIndex = 0;
            else if (_inventory.SelectedItemIndex < 0)
                _inventory.SelectedItemIndex = (int)_inventory.ContainerSize.X - 1;
        }

        /// <summary>
        /// "Sprint key down handler
        /// </summary>
        void InputSprintDown() => _isSprinting = true;

        /// <summary>
        /// "Sprint key up handler
        /// </summary>
        void InputSprintUp() => _isSprinting = false;

        /// <summary>
        /// "Inventory" action handler
        /// </summary>
        void InputInventory()
        {
            // Toggle inventory open/closed
            if (_inventory.IsOpen)
            {
                _inventory.Close();
                _craftingInventory.Close();
            }
            else
            {
                _inventory.Open();
                _craftingInventory.Open();
            }

            // Toggle cursor visibility
            SetMouseVisible(_inventory.IsOpen);

            // Toggle player input control
            SetControlsActive(!_inventory.IsOpen);
        }

        /// <summary>
        /// "Pause" action handler
        /// </summary>
        void InputPause()
        {
            // Ignore input if initial world loading is still in progress
            if (!_currentWorld.HasFinishedInitialLoading) return;

            if (_inventory.IsOpen) // Close the inventory if open
            {
                _inventory.Close();
                _craftingInventory.Close();
                SetControlsActive(true);
                SetMouseVisible(false);
            }
            else if (_pauseMenu.IsOpen) // Else close the pause menu if open
            {
                _pauseMenu.Close();
                SetControlsActive(true);
                SetMouseVisible(false);
            }
            else // Else open pause menu
                _pauseMenu.Show();
        }

        /// <summary>
        /// "Jump" action handler
        /// </summary>
        void InputJump()
        {
            // Cann't jump when in water or when controls are disabled
            if (!_controlsEnabled || _isInWater) return;

            // Check if is above the block
            if (Raycast.CastVoxel(_currentWorld.WorldCamera.Position, new Vector3(0, -1, 0), 2.1f, out _))
                _rigidbody.AddImpluse(new Vector3(0, 1, 0) * 600);
        }

        public void SetInitialPosition(Vector3 position, Vector3 rotation)
        {
            Position = position;
            Rotation = rotation;
            _hasHadInitialSet = true;
        }

        /// <summary>
        /// "Interact" action handler
        /// </summary>
        void InputInteract()
        {
            if (!_controlsEnabled) return;

            if (Raycast.CastVoxel(_currentWorld.WorldCamera.Position, _currentWorld.WorldCamera.ForwardVector, 5, out RayVoxelOut op))
            {
                int x = (int)Math.Floor(PositionInChunk.X);
                int y = (int)Math.Floor(PositionInChunk.Y);
                int z = (int)Math.Floor(PositionInChunk.Z);
                bool isPlayerAtPos = (int)op.PlacementPosition.X == x && (int)op.PlacementPosition.Z == z;

                // TODO: Allow for building above and below here
                // TODO: Add support for non-block items (weapons, tools, food, chests etc..)
                // Check if we're not trying to build at the same column as player is currently standing, if not get chunk we should place block to
                if (!isPlayerAtPos && _currentWorld.TryGetChunkAtPosition((int)op.PlacementChunk.X, (int)op.PlacementChunk.Y, out Chunk chunk))
                {
                    // Get currently selected stack
                    var stack = _inventory.GetItemStackByLocation(_inventory.SelectedItemIndex, 0);
                    if (stack != null)
                    {
                        stack.Item.OnInteract(op.PlacementPosition, chunk); // Interact with the item (build block, consume food etc..)
                        _inventory.RemoveItemFromStack(stack.Item, stack);  // Remove item
                        _currentWorld.RequestChunkUpdate(chunk, true, (int)op.BlockPosition.X, (int)op.BlockPosition.Z);
                    }
                }
            }
        }

        /// <summary>
        /// "Destroy" action handler
        /// </summary>
        void InputDestroyBlock()
        {
            if (!_controlsEnabled) return;

            if (Raycast.CastEntities(_currentWorld.WorldCamera.Position, _currentWorld.WorldCamera.ForwardVector, 5, out Entity entity))
            {
                if (entity is IDamageable damageable)
                {
                    damageable.TakeDamage(2);
                    Debug.Log("HIT, REMAINING: " + damageable.Health);
                    return;
                }
            }

            if (Raycast.CastVoxel(_currentWorld.WorldCamera.Position, _currentWorld.WorldCamera.ForwardVector, 5, out RayVoxelOut op))
            {
                if (_currentWorld.TryGetChunkAtPosition((int)op.ChunkPosition.X, (int)op.ChunkPosition.Y, out Chunk chunk))
                {
                    chunk.DestroyBlock((int)op.BlockPosition.X, (int)op.BlockPosition.Y, (int)op.BlockPosition.Z);

                    var chunkWp = op.ChunkPosition * Chunk.WIDTH;                       // Get chunk world position
                    var wp = new Vector3(chunkWp.X, 0, chunkWp.Y) + op.BlockPosition;   // Get block world position
                    BlockDatabase.GetBlock(op.BlockID).OnBreak(wp, op.ChunkPosition);   // Call block broken action

                    _currentWorld.RequestChunkUpdate(chunk, true, (int)op.BlockPosition.X, (int)op.BlockPosition.Z);
                }
            }
        }

        /// <summary>
        /// Handle player input
        /// </summary>
        void HandleInput()
        {
            _vel = Vector3.Zero;    // Horizonal input velocity

            if (_controlsEnabled)
            {
                KeyboardState kbdState = Keyboard.GetState();               // Keyboard snapshot
                var finalSpeed = _isSprinting ? _runSpeed : _walkSpeed;     // Get player speed based on current state

                // Handle WSAD movement
                if (kbdState.IsKeyDown(Key.S)) _vel += -ForwardVector * finalSpeed;
                if (kbdState.IsKeyDown(Key.W)) _vel += ForwardVector * finalSpeed;
                if (kbdState.IsKeyDown(Key.A)) _vel += -RightVector * finalSpeed;
                if (kbdState.IsKeyDown(Key.D)) _vel += RightVector * finalSpeed;

                // Handle swimming in water
                if (_isInWater && kbdState.IsKeyDown(Key.Space)) _rigidbody.AddForce(new Vector3(0, 1, 0) * 4000);

                // Update mouse input
                float x = Input.MouseDelta.X / 20f;
                float y = Input.MouseDelta.Y / 20f;

                // Limit Y movement to avoid gimbal lock
                if (y > 0 && _currentWorld.WorldCamera.Rotation.X >= 90) y = 0;
                if (y < 0 && _currentWorld.WorldCamera.Rotation.X <= -85) y = 0;

                // Update player and camera rotation
                Rotation = new Vector3(0, Rotation.Y + x, 0);
                _currentWorld.WorldCamera.Rotation = new Vector3(_currentWorld.WorldCamera.Rotation.X + y, Rotation.Y, 0);
            }

            // Update player physics velocity
            _rigidbody.Velocity = new Vector3(_vel.X, _rigidbody.Velocity.Y, _vel.Z);
        }

        public override void Update()
        {
            // If player has not yet found initial position
            if (!_hasHadInitialSet)
            {
                Position.Y = Chunk.HEIGHT; // Start at chunk max height as fallback

                // Cast straight down to find block at the top 
                if (Raycast.CastVoxel(new Vector3(Position), new Vector3(0, -1, 0), Chunk.HEIGHT, out RayVoxelOut hit))
                {
                    var chunkWp = new Vector3(hit.ChunkPosition.X * Chunk.WIDTH, 0, hit.ChunkPosition.Y * Chunk.WIDTH) + hit.BlockPosition;
                    Position = chunkWp + new Vector3(0.5f, 0, 0.5f);
                    Debug.Log("Hit block for y pos " + Position.Y);
                    _rigidbody = new Rigidbody(this, 70, new BoundingBox(-0.25f, 0.25f, 0, 2, -0.25f, 0.25f));
                    _hasHadInitialSet = true;
                }
            }

            _currentWorld.WorldCamera.Position = Position + new Vector3(0, 1.7f, 0);

            if (_hasHadInitialSet)
            {
                HandleInput();

                var chunkPos = Position.ToChunkPosition();
                if (World.Instance.TryGetChunkAtPosition((int)chunkPos.X, (int)chunkPos.Z, out Chunk chunk))
                {
                    var block = Position.ToChunkSpaceFloored();
                    _isInWater = chunk.GetBlockID((int)block.X, (int)block.Y, (int)block.Z) == GameBlocks.WATER.ID;
                    _rigidbody.Drag = _isInWater ? UnderWaterDrag : 0;
                }
            }

            if (_currentWorld.HasFinishedInitialLoading)
            {
                // Disabled health decay until food is implemented
                /*if (_lastHungerLossTick + _hungerLossTickRate <= Time.GameTime)
                {
                    _hungerLossAmount = _isSprinting ? 0.25f : 0.01f;

                    if (_currentHunger > 0)
                        _currentHunger -= _hungerLossAmount;
                    else
                    {
                        //_currentHealth -= 0.0625f; // 1.5
                        TakeDamage(1);
                    }
                    _lastHungerLossTick = Time.GameTime;
                }*/

                if (_lastHealthIncreaseTick + _healthIncreaseTickRate <= Time.GameTime)
                {
                    if (_currentHealth < MAX_HEALTH && _currentHunger == MAX_HUNGER)
                        Health += 1;

                    _lastHealthIncreaseTick = Time.GameTime;
                }
            }
        }

        public override void RenderGUI()
        {
            var rect = new Rect(8, 8, 1024, 32);
            GUI.Label($"{(int)Time.FramesPerSecond}fps", rect);
            rect.Y += 32f;
            GUI.Label($"Meshes culled: {Renderer.CulledCount}", rect);
            rect.Y += 32f;
            GUI.Label($"Drawcalls: {Renderer.DrawCalls}", rect);
            rect.Y += 32f;
            GUI.Label($"Chunks loaded: {World.Instance.LoadedChunks.Length}", rect);
            rect.Y += 32f;
            GUI.Label($"Loc in chunk: {PositionInChunk}", rect);
            rect.Y += 32f;
            GUI.Label($"Position: {Position}", rect);
            rect.Y += 32f;
            GUI.Label($"Rotation: {_currentWorld.WorldCamera.Rotation}", rect);
            rect.Y += 32f;
            GUI.Label($"Queue length: {_currentWorld.UpdateQueueLength}", rect);

            GUI.Image(_currentWorld.TexturePack.Crosshair, new Rect((Program.Window.Width / 2) - 16, (Program.Window.Height / 2) - 16, 32, 32));
            _inventory.RenderToolBar();

            float winWidth = Window.WindowWidth;
            float winHeight = Window.WindowHeight;

            float centerX = winWidth * 0.5f;
            float centerY = winHeight * 0.5f;

            float slotSize = ContainerRenderer.SLOT_SIZE;
            Vector2 size = new Vector2(Inventory.ContainerSize.X, 1) * slotSize;

            // Inventory toolbar position
            Rect toolbarRect = new Rect(centerX - size.X * 0.5f, winHeight - size.Y * 0.5f - slotSize, size.X, size.Y);

            float elementSize = 18;

            for (int i = 0; i < MAX_HEALTH; i++)
            {
                float xOffset = (i / 2) * (elementSize - 1);
                int curHealth = (int)Math.Ceiling(_currentHealth);

                // Heart container background
                if (i > curHealth)
                    GUI.Image(_heartEmptyIcon, new Rect(toolbarRect.X + xOffset, toolbarRect.Y - elementSize - 10, elementSize, elementSize));

                if (i % 2 != 0) // Half a heart
                {
                    if (i == curHealth)
                        GUI.Image(_heartHalfIcon, new Rect(toolbarRect.X + xOffset, toolbarRect.Y - elementSize - 10, elementSize, elementSize));

                }
                else if (i <= curHealth) // Full heart icon
                    GUI.Image(_heartIcon, new Rect(toolbarRect.X + xOffset, toolbarRect.Y - elementSize - 10, elementSize, elementSize));

            }

            for (int i = MAX_HUNGER; i > 0; i--)
            {
                float xOffset = (i / 2) * (elementSize - 1);
                int curHunger = (int)Math.Ceiling(_currentHunger);

                // Heart container background
                if (i > curHunger)
                    GUI.Image(_hungerEmptyIcon, new Rect(toolbarRect.X + toolbarRect.Width - xOffset, toolbarRect.Y - elementSize - 10, elementSize, elementSize));

                if (i % 2 != 0) // Half a heart
                {
                    if (i == curHunger)
                        GUI.Image(_hungerHalfIcon, new Rect(toolbarRect.X + toolbarRect.Width - xOffset, toolbarRect.Y - elementSize - 10, elementSize, elementSize));

                }
                else if (i < curHunger) // Full heart icon
                    GUI.Image(_hungerIcon, new Rect(toolbarRect.X + toolbarRect.Width - xOffset, toolbarRect.Y - elementSize - 10, elementSize, elementSize));

            }
        }

        public override void OnPreVoxelCollisionEnter()
        {
            //Fall damage here...
        }

        public static void SetControlsActive(bool active)
        {
            _controlsEnabled = active;
        }

        public static void SetMouseVisible(bool visible, bool resetPos = true)
        {
            if (_mouseHidden != visible)
                return;

            _mouseHidden = !visible;
            Program.Window.CursorVisible = visible;
            Program.Window.CursorGrabbed = !visible;

            if (resetPos)
                Mouse.SetPosition(Program.Window.X + Program.Window.Width / 2f, Program.Window.Y + Program.Window.Height / 2f);
        }


        public void Die()
        {
            //The player died
            Respawn();
        }

        public void TakeDamage(int damage)
        {
            _currentHealth -= damage;
            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                Die();
            }
        }

        public void Respawn()
        {
            _hasHadInitialSet = false;
            Position = Vector3.Zero;
            _currentHealth = MAX_HEALTH;
            _currentHunger = MAX_HUNGER;
            _lastHungerLossTick = Time.GameTime;
            _inventory.ItemsList.Clear();
            _craftingInventory.ItemsList.Clear();
        }
    }
}
