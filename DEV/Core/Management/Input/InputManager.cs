using PoncheToolkit.Core.Services;
using PoncheToolkit.Util;
using SharpDX;
using SharpDX.DirectInput;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core.Management.Input
{
    /// <summary>
    /// Class that manage all the input from mouse, keyboard or peripherals.
    /// </summary>
    public class InputManager : GameService
    {
        /// <summary>
        /// List of buttons available for the Mouse.
        /// </summary>
        public enum MouseButton
        {
            /// <summary>
            /// The left button
            /// </summary>
            Left = 0,
            /// <summary>
            /// The right button.
            /// </summary>
            Right = 1,
            /// <summary>
            /// The wheel button.
            /// </summary>
            Middle = 2,
            /// <summary>
            /// One of the side buttons (typically the down side button).
            /// </summary>
            Side1 = 3,
            /// <summary>
            /// One of the side buttons (typically the top side button).
            /// </summary>
            Side2 = 4
        }

        #region Fields
        private DirectInput directInput;
        private List<Controller> controllers;
        private List<Controller> connectedControllers;
        private Keyboard keyboard;
        private Mouse mouse;
        private Joystick directGamepad;
        private KeyboardState currentKeyboardState;
        private KeyboardState lastKeyboardState;
        private Dictionary<Controller, State> currentControllersStates;
        private Dictionary<Controller, State> lastControllersStates;
        private MouseState currentMouseState;
        private MouseState lastMouseState;

        private Vector2 mousePosition;
        private Vector2 mouseDelta;
        public static short GAMEPAD_MIN_THUMB_THRESHOLD = 6000;
        public static byte GAMEPAD_MIN_TRIGGER_THRESHOLD = 30;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;

        /// <summary>
        /// Get or set the mouse position.
        /// </summary>
        public Vector2 MousePosition
        {
            get { return mousePosition; }
            set { mousePosition = value; }
        }

        /// <summary>
        /// Get the value for the last movement of the mouse.
        /// </summary>
        public Vector2 MouseDelta
        {
            get { return mouseDelta; }
        }

        /// <summary>
        /// Get if any XBox controllers are connected.
        /// </summary>
        public bool IsControllerConnected
        {
            get { return ConnectedControllers.Count > 0; }
        }

        /// <summary>
        /// Get if any XBox controllers are connected.
        /// </summary>
        public List<Controller> ConnectedControllers
        {
            get
            {
                connectedControllers.Clear();
                currentControllersStates.Clear();
                lastControllersStates.Clear();
                foreach (Controller controller in controllers)
                {
                    if (controller.IsConnected)
                    {
                        connectedControllers.Add(controller);
                        currentControllersStates.Add(controller, new State());
                        lastControllersStates.Add(controller, new State());
                    }
                }
                return connectedControllers;
            }
        }
        #endregion
        

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public InputManager(Game game)
            : base(game)
        {
            mousePosition = Vector2.Zero;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            connectedControllers = new List<Controller>();
            currentControllersStates = new Dictionary<Controller, State>();
            lastControllersStates = new Dictionary<Controller, State>();
            directInput = new DirectInput();
            keyboard = new Keyboard(directInput);
            currentKeyboardState = new KeyboardState();
            currentMouseState = new MouseState();

            // ===== KEYBOARD ===== Assign keyboard properties.
            //keyboard.SetCooperativeLevel(Game.Form.Handle, CooperativeLevel.Background | CooperativeLevel.Exclusive);
            keyboard.Properties.BufferSize = 128;
            keyboard.Acquire();

            // ===== MOUSE ===== Assign mouse properties.
            mouse = new Mouse(directInput);
            //mouse.SetCooperativeLevel(Game.Form.Handle, CooperativeLevel.Background | CooperativeLevel.NonExclusive);
            mouse.Acquire();

            // ===== GAMEPAD ====== Using XInput
            controllers = new List<Controller>();
            controllers.Add(new Controller(UserIndex.One));
            controllers.Add(new Controller(UserIndex.Two));
            controllers.Add(new Controller(UserIndex.Three));
            controllers.Add(new Controller(UserIndex.Four));
            if (IsControllerConnected)
                Log.Debug("Controllers detected: -{0}-", connectedControllers.Count);
            else
                Log.Warning("No Controllers detected.");

            //// ===== GAMEPAD ====== Search for joystick/gamepad.  Using DirectInput
            //var gamepadGuid = Guid.Empty;

            //foreach (var deviceInstance in directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
            //    gamepadGuid = deviceInstance.InstanceGuid;

            //if (gamepadGuid == Guid.Empty)
            //    Log.Warning("No Gamepads found.");
            //else
            //{
            //    // Instantiate the direct gamepad
            //    directGamepad = new Joystick(directInput, gamepadGuid);

            //    var gamepadEffects = directGamepad.GetEffects();
            //    foreach (var effectInfo in gamepadEffects)
            //        Log.Debug("Gamepad effect: {0}", effectInfo.Name);

            //    // Set BufferSize in order to use buffered data.
            //    directGamepad.Properties.BufferSize = 128;
            //    directGamepad.Acquire();
            //}

            

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void UpdateLogic(GameTime gameTime)
        {
            //keyboard.Poll();
            //mouse.Poll();

            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = keyboard.GetCurrentState();

            lastMouseState = currentMouseState;
            currentMouseState = mouse.GetCurrentState();

            mouseDelta = new Vector2(currentMouseState.X, currentMouseState.Y);
            mousePosition.X += mouseDelta.X;
            mousePosition.Y += mouseDelta.Y;


            //mousePosition.X = mousePosition.X + Game.Form.DesktopLocation.X + mouseState.X;
            //mousePosition.Y = mousePosition.Y + Game.Form.DesktopLocation.Y + mouseState.Y;

            //foreach (Controller controller in connectedControllers)
            //{
            //    lastControllersStates[controller] = currentControllersStates[controller];
            //    currentControllersStates[controller] = controller.GetState();
            //    if (lastControllersStates[controller].PacketNumber != currentControllersStates[controller].PacketNumber)
            //    {
            //        Log.Debug("Controller state changed: -{0}-", currentControllersStates[controller].Gamepad.ToString());
            //    }
            //}


            //if (mousePosition.X < Game.Form.Left)
            //    mousePosition.X = 0;
            //if (mousePosition.Y < Game.Form.Top)
            //    mousePosition.Y = 0;
        }

        /// <summary>
        /// Get the current gamepad state.
        /// Return null if the gamepad state has not changed.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Gamepad? GamepadState(UserIndex user)
        {
            int index = user == UserIndex.Any ? 0 : (int)user;
            if (connectedControllers.Count <= index)
                return null;

            Controller controller = connectedControllers[index];

            lastControllersStates[controller] = currentControllersStates[controller];
            currentControllersStates[controller] = controller.GetState();

            return currentControllersStates[connectedControllers[index]].Gamepad;

            //if (lastControllersStates[controller].PacketNumber != currentControllersStates[controller].PacketNumber)
            //{
            //    //Log.Debug("Controller state changed: -{0}-", currentControllersStates[controller].Gamepad.ToString());
            //    return currentControllersStates[connectedControllers[index]].Gamepad;
            //}

            //return null;
        }

        /// <summary>
        /// Return true when a button is being held.
        /// </summary>
        /// <param name="user">The user index for the controller.</param>
        /// <param name="buttons">The buttons flags to check for.</param>
        /// <returns></returns>
        public bool IsButtonHold(UserIndex user, GamepadButtonFlags buttons)
        {
            int index = user == UserIndex.Any ? 0 : (int)user;
            if (connectedControllers.Count <= index)
                return false;

            Controller controller = connectedControllers[index];
            if (currentControllersStates[controller].Gamepad.Buttons.HasFlag(buttons))
                return true;

            return false;
        }

        /// <summary>
        /// Return true when a button has been pressed. (even when it has not been released)
        /// </summary>
        /// <param name="user">The user index for the controller.</param>
        /// <param name="buttons">The buttons flags to check for.</param>
        /// <returns></returns>
        public bool IsButtonPressed(UserIndex user, GamepadButtonFlags buttons)
        {
            int index = user == UserIndex.Any ? 0 : (int)user;
            if (connectedControllers.Count <= index)
                return false;

            Controller controller = connectedControllers[index];
            if (lastControllersStates[controller].PacketNumber != currentControllersStates[controller].PacketNumber
                && !lastControllersStates[controller].Gamepad.Buttons.HasFlag(buttons)
                && currentControllersStates[controller].Gamepad.Buttons.HasFlag(buttons))
                return true;

            return false;
        }

        /// <summary>
        /// Return true when a button has been pressed and then released.
        /// </summary>
        /// <param name="user">The user index for the controller.</param>
        /// <param name="buttons">The buttons flags to check for.</param>
        /// <returns></returns>
        public bool IsButtonReleased(UserIndex user, GamepadButtonFlags buttons)
        {
            int index = user == UserIndex.Any ? 0 : (int)user;
            if (connectedControllers.Count <= index)
                return false;

            Controller controller = connectedControllers[index];
            if (lastControllersStates[controller].PacketNumber != currentControllersStates[controller].PacketNumber
                && lastControllersStates[controller].Gamepad.Buttons.HasFlag(buttons)
                && !currentControllersStates[controller].Gamepad.Buttons.HasFlag(buttons))
                return true;

            return false;
        }

        /// <summary>
        /// Poll for a key hold.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsKeyHold(Key key)
        {
            if (currentKeyboardState.PressedKeys.Contains(key))
                return true;

            return false;
        }

        /// <summary>
        /// Poll for a key press.
        /// Returns true if the key is pressed and then released.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsKeyPressed(Key key)
        {
            if (lastKeyboardState.PressedKeys.Contains(key) && !currentKeyboardState.PressedKeys.Contains(key))
                return true;

            return false;
        }

        /// <summary>
        /// Poll for a <see cref="MouseButton"/> hold.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool IsMouseHold(MouseButton button)
        {
            return currentMouseState.Buttons[(int)button];
        }

        /// <summary>
        /// Poll for a <see cref="MouseButton"/> press.
        /// Returns true if the button is pressed and then released.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool IsMousePressed(MouseButton button)
        {
            if (currentMouseState.Buttons[(int)button] == false && lastMouseState.Buttons[(int)button] == true)
                return true;

            return false;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Utilities.Dispose(ref keyboard);
            Utilities.Dispose(ref mouse);

            controllers.Clear();
            controllers = null;
        }
    }
}
