using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles gesture inputs and emits high-level gesture signals such as taps, swipes, pinches, and twists.
/// </summary>
public partial class InputEventGestureHandler : Node
{

    public static InputEventGestureHandler Instance { get; private set; }

    #region Config

    // Configuration constants for gesture detection thresholds and limits.

    private const bool DEFAULT_BINDINGS = true;
    private const bool DEBUG = false;

    // Time threshold to consider a drag gesture.
    private const float DRAG_STARTUP_TIME = 0.02f;

    // Finger size used to define thresholds in gesture calculations.
    private const float FINGER_SIZE = 100.0f;

    // Time threshold to detect multi-finger release gestures.
    private const float MULTI_FINGER_RELEASE_THRESHOLD = 0.1f;

    // Time and distance thresholds for detecting tap gestures.
    private const float TAP_TIME_LIMIT = 0.2f;
    private const float TAP_DISTANCE_LIMIT = 25.0f;

    // Time and distance limits for detecting long press gestures.
    private const float LONG_PRESS_TIME_THRESHOLD = 0.75f;
    private const float LONG_PRESS_DISTANCE_LIMIT = 25.0f;

    // Time and distance thresholds for detecting swipe gestures.
    private const float SWIPE_TIME_LIMIT = 0.5f;
    private const float SWIPE_DISTANCE_THRESHOLD = 200.0f;

    #endregion

    #region Constants

    /// <summary>
    /// Maps swipe gesture names to their corresponding direction vectors.
    /// This allows for easy swipe gesture detection by comparing directions.
    /// </summary>
    private static readonly Dictionary<string, Vector2> Swipe2Dir = new Dictionary<string, Vector2>
    {
        { "swipe_up", Vector2.Up },
        { "swipe_up_right", (Vector2.Up + Vector2.Right).Normalized() },
        { "swipe_right", Vector2.Right },
        { "swipe_down_right", (Vector2.Down + Vector2.Right).Normalized() },
        { "swipe_down", Vector2.Down },
        { "swipe_down_left", (Vector2.Down + Vector2.Left).Normalized() },
        { "swipe_left", Vector2.Left },
        { "swipe_up_left", (Vector2.Up + Vector2.Left).Normalized() }
    };

    #endregion

    #region Signals

    // Signal declarations for various gesture events.

    [Signal]
    public delegate void TouchEventHandler(InputEventScreenTouch touchEvent);

    [Signal]
    public delegate void DragEventHandler(InputEventScreenDrag dragEvent);

    [Signal]
    public delegate void SingleTapEventHandler(InputEventSingleScreenTap tapEvent);

    [Signal]
    public delegate void SingleTouchEventHandler(InputEventSingleScreenTouch touchEvent);

    [Signal]
    public delegate void SingleDragEventHandler(InputEventSingleScreenDrag dragEvent);

    [Signal]
    public delegate void SingleSwipeEventHandler(InputEventSingleScreenSwipe swipeEvent);

    [Signal]
    public delegate void SingleLongPressEventHandler(InputEventSingleScreenLongPress longPressEvent);

    [Signal]
    public delegate void MultiDragEventHandler(InputEventMultiScreenDrag dragEvent);

    [Signal]
    public delegate void MultiTapEventHandler(InputEventMultiScreenTap tapEvent);

    [Signal]
    public delegate void MultiSwipeEventHandler(InputEventMultiScreenSwipe swipeEvent);

    [Signal]
    public delegate void MultiLongPressEventHandler(InputEventMultiScreenLongPress longPressEvent);

    [Signal]
    public delegate void PinchEventHandler(InputEventScreenPinch pinchEvent);

    [Signal]
    public delegate void TwistEventHandler(InputEventScreenTwist twistEvent);

    [Signal]
    public delegate void InputEventGestureEventHandler(InputEventGesture InputEventGesture);

    [Signal]
    public delegate void CancelEventHandler(InputEventScreenCancel cancelEvent);

    [Signal]
    public delegate void AnyGestureEventHandler(string gestureName, InputEvent inputEvent);

    #endregion

    #region Enum

    /// <summary>
    /// Enumeration of gesture types for internal use.
    /// These help in identifying what type of gesture is being performed.
    /// </summary>
    private enum Gesture
    {
        PINCH,
        MULTI_DRAG,
        TWIST,
        SINGLE_DRAG,
        NONE
    }

    #endregion

    #region Variables

    /// <summary>
    /// Stores the raw gesture data.
    /// This contains all the detailed information about the current gesture being detected.
    /// </summary>
    private InputEventGesture rawGestureData = new InputEventGesture();

    /// <summary>
    /// Stores the position where the mouse event was pressed.
    /// This helps in tracking mouse gestures like drag or twist.
    /// </summary>
    private Vector2 mouseEventPressPosition;

    /// <summary>
    /// Stores the current mouse gesture being performed.
    /// Tracks if the mouse is performing a drag, twist, or other gestures.
    /// </summary>
    private Gesture mouseEvent = Gesture.NONE;

    /// <summary>
    /// Timer for starting up single drag gestures.
    /// Helps to delay the start of a drag gesture to avoid false positives.
    /// </summary>
    private Timer dragStartupTimer = new Timer();

    /// <summary>
    /// Timer for detecting long press gestures.
    /// This timer allows the system to detect when a touch is held down long enough to be considered a long press.
    /// </summary>
    private Timer longPressTimer = new Timer();

    /// <summary>
    /// Indicates whether a single touch has been canceled.
    /// Helps to prevent single touch gestures from being emitted after cancellation.
    /// </summary>
    private bool singleTouchCancelled = false;

    /// <summary>
    /// Indicates whether single drag gestures are enabled.
    /// </summary>
    private bool singleDragEnabled = false;

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// Called when the node is added to the scene tree for the first time.
    /// Initializes timers and sets default input bindings if enabled.
    /// </summary>
    public override void _Ready()
    {
        // Set up timers for drag and long press gesture detection
        AddTimer(dragStartupTimer, OnDragStartupTimerTimeout);
        AddTimer(longPressTimer, OnLongPressTimerTimeout);

        if (DEFAULT_BINDINGS)
        {
            // Bind multi-finger swipe actions to specific keys
            SetDefaultAction("multi_swipe_up", NativeKeyEvent((int)Key.I));
            SetDefaultAction("multi_swipe_up_right", NativeKeyEvent((int)Key.O));
            SetDefaultAction("multi_swipe_right", NativeKeyEvent((int)Key.L));
            SetDefaultAction("multi_swipe_down_right", NativeKeyEvent((int)Key.Period));
            SetDefaultAction("multi_swipe_down", NativeKeyEvent((int)Key.Comma));
            SetDefaultAction("multi_swipe_down_left", NativeKeyEvent((int)Key.M));
            SetDefaultAction("multi_swipe_left", NativeKeyEvent((int)Key.J));
            SetDefaultAction("multi_swipe_up_left", NativeKeyEvent((int)Key.Q));

            // Bind single-finger swipe actions to specific keys
            SetDefaultAction("single_swipe_up", NativeKeyEvent((int)Key.W));
            SetDefaultAction("single_swipe_up_right", NativeKeyEvent((int)Key.E));
            SetDefaultAction("single_swipe_right", NativeKeyEvent((int)Key.D));
            SetDefaultAction("single_swipe_down_right", NativeKeyEvent((int)Key.C));
            SetDefaultAction("single_swipe_down", NativeKeyEvent((int)Key.X));
            SetDefaultAction("single_swipe_down_left", NativeKeyEvent((int)Key.Z));
            SetDefaultAction("single_swipe_left", NativeKeyEvent((int)Key.A));
            SetDefaultAction("single_swipe_up_left", NativeKeyEvent((int)Key.Q));

            // Bind multi-touch and gesture actions to mouse buttons
            SetDefaultAction("multi_touch", NativeMouseButtonEvent(MouseButton.Middle));
            SetDefaultAction("pinch_outward", NativeMouseButtonEvent(MouseButton.WheelUp));
            SetDefaultAction("pinch_inward", NativeMouseButtonEvent(MouseButton.WheelDown));
            SetDefaultAction("twist", NativeMouseButtonEvent(MouseButton.Right));
        }
    }

    /// <summary>
    /// Called when this node enters the scene tree.
    /// </summary>
    public override void _EnterTree()
    {
        Instance = this;  // Set the instance for singleton access.
    }

    /// <summary>
    /// Called when this node is about to exit the scene tree.
    /// </summary>
    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;  // Clear the singleton instance when exiting the tree.
    }

    /// <summary>
    /// Called when an input event hasn't been handled by any other node.
    /// Processes various input events to detect gestures like touch, drag, and swipe.
    /// </summary>
    /// <param name="event">The input event.</param>
    public override void _UnhandledInput(InputEvent @event)
    {
        // Detect drag gestures from screen drag input events
        if (@event is InputEventScreenDrag screenDrag)
        {
            HandleScreenDrag(screenDrag);
        }
        // Detect touch gestures from screen touch input events
        else if (@event is InputEventScreenTouch screenTouch)
        {
            HandleScreenTouch(screenTouch);
        }
        // Detect mouse motion gestures for more complex gestures like twist
        else if (@event is InputEventMouseMotion mouseMotion)
        {
            HandleMouseMotion(mouseMotion);
        }
        // Handle custom input actions for gesture detection (e.g., swipes, pinches)
        else
        {
            HandleAction(@event);
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles mouse motion events for gesture detection.
    /// Detects gestures like single or multi-finger drag and twist.
    /// </summary>
    /// <param name="eventMotion">The mouse motion event.</param>
    private void HandleMouseMotion(InputEventMouseMotion eventMotion)
    {
        // If one drag is active, emit a single drag event
        if (rawGestureData.Drags.Count == 1 && mouseEvent == Gesture.SINGLE_DRAG)
        {
            EmitSingleDrag(new InputEventSingleScreenDrag(rawGestureData));
        }
        // If two drags are active, treat it as a multi-finger drag
        else if (rawGestureData.Drags.Count == 2 && mouseEvent == Gesture.MULTI_DRAG)
        {
            Vector2 offset = new Vector2(5, 5);
            var e0 = NativeDragEvent(0, eventMotion.Position - offset, eventMotion.Relative, eventMotion.Velocity);
            rawGestureData.UpdateScreenDrag(e0);
            var e1 = NativeDragEvent(1, eventMotion.Position + offset, eventMotion.Relative, eventMotion.Velocity);
            rawGestureData.UpdateScreenDrag(e1);
            EmitMultiDrag(new InputEventMultiScreenDrag(rawGestureData, e0));
            EmitMultiDrag(new InputEventMultiScreenDrag(rawGestureData, e1));
        }
        // Detect twist gesture if mouse event is recognized as a twist
        else if (mouseEvent == Gesture.TWIST)
        {
            Vector2 rel1 = eventMotion.Position - mouseEventPressPosition;
            Vector2 rel2 = rel1 + eventMotion.Relative;
            var twistEvent = new InputEventScreenTwist
            {
                Position = mouseEventPressPosition,
                Relative = rel1.AngleTo(rel2),
                Fingers = 2
            };
            EmitTwist(twistEvent);
        }
    }

    /// <summary>
    /// Handles screen touch events for gesture detection.
    /// Processes taps, long presses, and releases.
    /// </summary>
    /// <param name="eventTouch">The screen touch event.</param>
    private void HandleScreenTouch(InputEventScreenTouch eventTouch)
    {
        // If touch index is invalid, emit cancel signal and end gesture
        if (eventTouch.Index < 0)
        {
            EmitCancel(new InputEventScreenCancel(rawGestureData, eventTouch));
            EndGesture();
            return;
        }

        // Ignore release events that have no associated press
        if (!eventTouch.Pressed && !rawGestureData.Presses.ContainsKey(eventTouch.Index))
        {
            return;
        }

        // Update the raw gesture data with the touch event
        rawGestureData.UpdateScreenTouch(eventTouch);
        EmitRawGesture(rawGestureData);

        // Handle first and subsequent touch press events
        int index = eventTouch.Index;
        if (eventTouch.Pressed)
        {
            if (rawGestureData.Drags.Count == 1) // First and only touch
            {
                longPressTimer.Start(LONG_PRESS_TIME_THRESHOLD);
                singleTouchCancelled = false;
                EmitSingleTouch(new InputEventSingleScreenTouch(rawGestureData));
            }
            else if (!singleTouchCancelled)
            {
                singleTouchCancelled = true;
                CancelSingleDrag();
                EmitSingleTouch(new InputEventSingleScreenTouch(rawGestureData));
            }
        }
        // Handle touch release events
        else
        {
            int fingers = rawGestureData.ActiveTouches;
            if (index == 0)
            {
                EmitSingleTouch(new InputEventSingleScreenTouch(rawGestureData));
                if (!singleTouchCancelled)
                {
                    float distance = (rawGestureData.Releases[0].Position - rawGestureData.Presses[0].Position).Length();
                    // Check for tap gesture
                    if (rawGestureData.ElapsedTime < TAP_TIME_LIMIT && distance <= TAP_DISTANCE_LIMIT)
                    {
                        EmitSingleTap(new InputEventSingleScreenTap(rawGestureData));
                    }
                    // Check for swipe gesture
                    if (rawGestureData.ElapsedTime < SWIPE_TIME_LIMIT && distance > SWIPE_DISTANCE_THRESHOLD)
                    {
                        EmitSingleSwipe(new InputEventSingleScreenSwipe(rawGestureData));
                    }
                }
            }

            // If all touches are released, check for multi-finger tap or swipe gestures
            if (rawGestureData.ActiveTouches == 0)
            {
                if (singleTouchCancelled)
                {
                    Vector2 endsCentroid = InputEventGestureHelpers.Centroid(rawGestureData.GetEnds().Values.ToList());
                    Vector2 startsCentroid = rawGestureData.Centroid("presses", "position");
                    float distance = (endsCentroid - startsCentroid).Length();
                    // Check for multi-finger tap gesture
                    if (rawGestureData.ElapsedTime < TAP_TIME_LIMIT && distance <= TAP_DISTANCE_LIMIT &&
                        rawGestureData.IsConsistent(TAP_DISTANCE_LIMIT, FINGER_SIZE * rawGestureData.ActiveTouches) &&
                        ReleasedTogether(rawGestureData, MULTI_FINGER_RELEASE_THRESHOLD))
                    {
                        EmitMultiTap(new InputEventMultiScreenTap(rawGestureData));
                    }
                    // Check for multi-finger swipe gesture
                    if (rawGestureData.ElapsedTime < SWIPE_TIME_LIMIT && distance > SWIPE_DISTANCE_THRESHOLD &&
                        rawGestureData.IsConsistent(FINGER_SIZE, FINGER_SIZE * rawGestureData.ActiveTouches) &&
                        ReleasedTogether(rawGestureData, MULTI_FINGER_RELEASE_THRESHOLD))
                    {
                        EmitMultiSwipe(new InputEventMultiScreenSwipe(rawGestureData));
                    }
                }
                EndGesture();  // End gesture once all touches are released
            }
            CancelSingleDrag();  // Cancel the single drag gesture
        }
    }

    /// <summary>
    /// Handles screen drag events for gesture detection.
    /// Processes drag, pinch, and multi-drag gestures.
    /// </summary>
    /// <param name="eventDrag">The screen drag event.</param>
    private void HandleScreenDrag(InputEventScreenDrag eventDrag)
    {
        // If drag index is invalid, emit cancel signal and end gesture
        if (eventDrag.Index < 0)
        {
            EmitCancel(new InputEventScreenCancel(rawGestureData, eventDrag));
            EndGesture();
            return;
        }

        // Update raw gesture data with the drag event
        rawGestureData.UpdateScreenDrag(eventDrag);
        EmitRawGesture(rawGestureData);

        // Detect pinch, twist, or multi-drag gestures
        if (rawGestureData.Drags.Count > 1)
        {
            CancelSingleDrag();
            int gesture = IdentifyGesture(rawGestureData);
            switch ((Gesture)gesture)
            {
                case Gesture.PINCH:
                    EmitPinch(new InputEventScreenPinch(rawGestureData, eventDrag));
                    break;
                case Gesture.MULTI_DRAG:
                    EmitMultiDrag(new InputEventMultiScreenDrag(rawGestureData, eventDrag));
                    break;
                case Gesture.TWIST:
                    EmitTwist(new InputEventScreenTwist(rawGestureData, eventDrag));
                    break;
            }
        }
        // If only one drag is detected, emit single drag
        else
        {
            if (singleDragEnabled)
            {
                EmitSingleDrag(new InputEventSingleScreenDrag(rawGestureData));
            }
            else
            {
                // Start drag timer if drag gesture is not yet started
                if (dragStartupTimer.IsStopped())
                {
                    dragStartupTimer.Start(DRAG_STARTUP_TIME);
                }
            }
        }
    }

    /// <summary>
    /// Handles custom input actions for gesture emulation and detection.
    /// Detects gesture actions like pinch, swipe, and twist.
    /// </summary>
    /// <param name="event">The input event.</param>
    private void HandleAction(InputEvent @event)
    {
        // Handle single_touch action
        if (InputMap.HasAction("single_touch") &&
            (@event.IsActionPressed("single_touch") || @event.IsActionReleased("single_touch")))
        {
            var touchEvent = NativeTouchEvent(0, GetViewport().GetMousePosition(), @event.IsActionPressed("single_touch"));
            EmitTouch(touchEvent);

            if (@event.IsActionPressed("single_touch"))
            {
                mouseEvent = Gesture.SINGLE_DRAG;
            }
            else
            {
                mouseEvent = Gesture.NONE;
            }
        }
        // Handle multi_touch action
        else if (InputMap.HasAction("multi_touch") &&
                 (@event.IsActionPressed("multi_touch") || @event.IsActionReleased("multi_touch")))
        {
            var touchEvent0 = NativeTouchEvent(0, GetViewport().GetMousePosition(), @event.IsActionPressed("multi_touch"));
            var touchEvent1 = NativeTouchEvent(1, GetViewport().GetMousePosition(), @event.IsActionPressed("multi_touch"));
            EmitTouch(touchEvent0);
            EmitTouch(touchEvent1);

            if (@event.IsActionPressed("multi_touch"))
            {
                mouseEvent = Gesture.MULTI_DRAG;
            }
            else
            {
                mouseEvent = Gesture.NONE;
            }
        }
        // Handle twist action
        else if (InputMap.HasAction("twist") &&
                 (@event.IsActionPressed("twist") || @event.IsActionReleased("twist")))
        {
            mouseEventPressPosition = GetViewport().GetMousePosition();
            if (@event.IsActionPressed("twist"))
            {
                mouseEvent = Gesture.TWIST;
            }
            else
            {
                mouseEvent = Gesture.NONE;
            }
        }
        // Handle pinch actions
        else if ((InputMap.HasAction("pinch_outward") && @event.IsActionPressed("pinch_outward")) ||
                 (InputMap.HasAction("pinch_inward") && @event.IsActionPressed("pinch_inward")))
        {
            var pinchEvent = new InputEventScreenPinch
            {
                Fingers = 2,
                Position = GetViewport().GetMousePosition(),
                Distance = 400,
                Relative = 40
            };

            if (@event.IsActionPressed("pinch_inward"))
            {
                pinchEvent.Relative *= -1;
            }

            EmitPinch(pinchEvent);
        }
        // Detect swipe gestures using directional keys
        else
        {
            Vector2 swipeEmulationDir = Vector2.Zero;
            bool isSingleSwipe = false;

            foreach (var swipe in Swipe2Dir)
            {
                string singleSwipeAction = "single_" + swipe.Key;
                string multiSwipeAction = "multi_" + swipe.Key;

                if (InputMap.HasAction(singleSwipeAction) && @event.IsActionPressed(singleSwipeAction))
                {
                    swipeEmulationDir = swipe.Value;
                    isSingleSwipe = true;
                    break;
                }
                if (InputMap.HasAction(multiSwipeAction) && @event.IsActionPressed(multiSwipeAction))
                {
                    swipeEmulationDir = swipe.Value;
                    isSingleSwipe = false;
                    break;
                }
            }

            // Emit single or multi swipe gesture
            if (swipeEmulationDir != Vector2.Zero)
            {
                if (isSingleSwipe)
                {
                    var swipeEvent = new InputEventSingleScreenSwipe
                    {
                        Position = GetViewport().GetMousePosition(),
                        Relative = swipeEmulationDir * SWIPE_DISTANCE_THRESHOLD * 2
                    };
                    EmitSingleSwipe(swipeEvent);
                }
                else
                {
                    var swipeEvent = new InputEventMultiScreenSwipe
                    {
                        Fingers = 2,
                        Position = GetViewport().GetMousePosition(),
                        Relative = swipeEmulationDir * SWIPE_DISTANCE_THRESHOLD * 2
                    };
                    EmitMultiSwipe(swipeEvent);
                }
            }
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a native touch event.
    /// This is useful for emulating touch events based on input actions.
    /// </summary>
    /// <param name="index">The touch index.</param>
    /// <param name="position">The touch position.</param>
    /// <param name="pressed">Whether the touch is pressed.</param>
    /// <returns>A new InputEventScreenTouch.</returns>
    private InputEventScreenTouch NativeTouchEvent(int index, Vector2 position, bool pressed)
    {
        return new InputEventScreenTouch
        {
            Index = index,
            Position = position,
            Pressed = pressed
        };
    }

    /// <summary>
    /// Creates a native drag event.
    /// This is useful for emulating drag events based on input actions.
    /// </summary>
    /// <param name="index">The drag index.</param>
    /// <param name="position">The drag position.</param>
    /// <param name="relative">The relative movement.</param>
    /// <param name="velocity">The drag velocity.</param>
    /// <returns>A new InputEventScreenDrag.</returns>
    private InputEventScreenDrag NativeDragEvent(int index, Vector2 position, Vector2 relative, Vector2 velocity)
    {
        return new InputEventScreenDrag
        {
            Index = index,
            Position = position,
            Relative = relative,
            Velocity = velocity
        };
    }

    /// <summary>
    /// Creates a native mouse button event.
    /// This is useful for emulating mouse button events in gesture detection.
    /// </summary>
    /// <param name="button">The mouse button index.</param>
    /// <returns>A new InputEventMouseButton.</returns>
    private InputEventMouseButton NativeMouseButtonEvent(MouseButton button)
    {
        return new InputEventMouseButton
        {
            ButtonIndex = button
        };
    }

    /// <summary>
    /// Creates a native key event.
    /// This is useful for binding specific keys to actions in gesture detection.
    /// </summary>
    /// <param name="key">The key index.</param>
    /// <returns>A new InputEventKey.</returns>
    private InputEventKey NativeKeyEvent(int key)
    {
        return new InputEventKey
        {
            Keycode = (Key)key
        };
    }

    /// <summary>
    /// Sets a default action in the InputMap.
    /// Binds a specific input event to an action name.
    /// </summary>
    /// <param name="action">The name of the action.</param>
    /// <param name="event">The InputEvent to bind to the action.</param>
    private void SetDefaultAction(string action, InputEvent @event)
    {
        if (!InputMap.HasAction(action))
        {
            InputMap.AddAction(action);
            InputMap.ActionAddEvent(action, @event);
        }
    }

    /// <summary>
    /// Adds a Timer node and connects its timeout signal to a specified method.
    /// This is used for gesture detection delays (e.g., drag or long press).
    /// </summary>
    /// <param name="timer">The Timer node to add.</param>
    /// <param name="func">The method to connect to the timeout signal.</param>
    private void AddTimer(Timer timer, Action func)
    {
        timer.OneShot = true;  // Set timer to trigger only once
        if (func != null)
        {
            timer.Timeout += func;
        }
        AddChild(timer);
    }

    /// <summary>
    /// Cancels the single drag operation and stops the drag startup timer.
    /// This prevents the drag gesture from being recognized after cancellation.
    /// </summary>
    private void CancelSingleDrag()
    {
        singleDragEnabled = false;
        dragStartupTimer.Stop();
    }

    /// <summary>
    /// Checks if the gestures were released together within a threshold.
    /// Useful for detecting multi-touch releases.
    /// </summary>
    /// <param name="rawGestureData">The raw gesture data.</param>
    /// <param name="threshold">The time threshold.</param>
    /// <returns>True if released together; otherwise, false.</returns>
    private bool ReleasedTogether(InputEventGesture rawGestureData, float threshold)
    {
        var rolledBackGesture = rawGestureData.RollbackRelative(threshold);
        return rolledBackGesture != null && rolledBackGesture.ActiveTouches == rolledBackGesture.Drags.Count;
    }

    /// <summary>
    /// Identifies the gesture type based on the raw gesture data.
    /// This determines if the gesture is a pinch, twist, or multi-drag.
    /// </summary>
    /// <param name="rawGestureData">The raw gesture data.</param>
    /// <returns>An integer representing the gesture enum.</returns>
    private int IdentifyGesture(InputEventGesture rawGestureData)
    {
        Vector2 center = rawGestureData.Centroid("drags", "position");

        int sector = -1;
        foreach (var drag in rawGestureData.Drags.Values)
        {
            Vector2 adjustedPosition = center - drag.Position;
            float rawAngle = Mathf.PosMod(adjustedPosition.AngleTo(drag.Relative) + Mathf.Pi / 4, Mathf.Tau);
            float adjustedAngle = rawAngle >= 0 ? rawAngle : rawAngle + Mathf.Tau;
            int eSector = (int)Mathf.Floor(adjustedAngle / (Mathf.Pi / 2));

            if (sector == -1)
            {
                sector = eSector;
            }
            else if (sector != eSector)
            {
                return (int)Gesture.MULTI_DRAG;
            }
        }

        // Return gesture type based on sector
        if (sector == 0 || sector == 2)
        {
            return (int)Gesture.PINCH;
        }
        else if (sector == 1 || sector == 3)
        {
            return (int)Gesture.TWIST;
        }
        else
        {
            // Default to multi-drag if gesture is unrecognized
            return (int)Gesture.MULTI_DRAG;
        }
    }

    /// <summary>
    /// Handles the drag startup timer timeout.
    /// Enables single drag detection after the timeout.
    /// </summary>
    private void OnDragStartupTimerTimeout()
    {
        singleDragEnabled = rawGestureData.Drags.Count == 1;
    }

    /// <summary>
    /// Handles the long press timer timeout.
    /// Detects whether a long press gesture occurred.
    /// </summary>
    private void OnLongPressTimerTimeout()
    {
        Vector2 endsCentroid = InputEventGestureHelpers.Centroid(rawGestureData.GetEnds().Values.ToList());
        Vector2 startsCentroid = rawGestureData.Centroid("presses", "position");
        float distance = (endsCentroid - startsCentroid).Length();

        // Check if long press gesture is consistent with the given limits
        if (rawGestureData.Releases.Count == 0 &&
            distance <= LONG_PRESS_DISTANCE_LIMIT &&
            rawGestureData.IsConsistent(LONG_PRESS_DISTANCE_LIMIT, FINGER_SIZE * rawGestureData.ActiveTouches))
        {
            if (singleTouchCancelled)
            {
                EmitMultiLongPress(new InputEventMultiScreenLongPress(rawGestureData));
            }
            else
            {
                EmitSingleLongPress(new InputEventSingleScreenLongPress(rawGestureData));
            }
        }
    }

    /// <summary>
    /// Ends the current gesture by resetting variables and stopping timers.
    /// This is called when all touches are released or the gesture is canceled.
    /// </summary>
    private void EndGesture()
    {
        singleDragEnabled = false;
        longPressTimer.Stop();
        rawGestureData = new InputEventGesture();  // Reset raw gesture data for next gesture
    }

    #endregion

    #region Gesture Emission Methods

    /// <summary>
    /// Emits the 'any_gesture' signal along with the specific gesture signal.
    /// This allows for tracking any detected gesture.
    /// </summary>
    /// <param name="sig">The name of the specific gesture signal.</param>
    /// <param name="val">The InputEvent associated with the gesture.</param>
    
    private void EmitAnyGesture(string sig, InputEvent val)
    {
        EmitSignal(nameof(AnyGesture), sig, val);
        EmitSignal(sig, val);
    }

    /// <summary>
    /// Emits a touch gesture signal.
    /// </summary>
    /// <param name="touchEvent">The touch event.</param>
    private void EmitTouch(InputEventScreenTouch touchEvent)
    {
        EmitAnyGesture(nameof(Touch), touchEvent);
    }

    /// <summary>
    /// Emits a drag gesture signal.
    /// </summary>
    /// <param name="dragEvent">The drag event.</param>
    private void EmitDrag(InputEventScreenDrag dragEvent)
    {
        EmitAnyGesture(nameof(Drag), dragEvent);
    }

    /// <summary>
    /// Emits a multi-finger drag gesture signal.
    /// </summary>
    /// <param name="multiDragEvent">The multi-drag event.</param>
    private void EmitMultiDrag(InputEventMultiScreenDrag multiDragEvent)
    {
        EmitAnyGesture(nameof(MultiDrag), multiDragEvent);
    }

    /// <summary>
    /// Emits a single tap gesture signal.
    /// </summary>
    /// <param name="tapEvent">The tap event.</param>
    private void EmitSingleTap(InputEventSingleScreenTap tapEvent)
    {
        EmitAnyGesture(nameof(SingleTap), tapEvent);
    }

    /// <summary>
    /// Emits a single touch gesture signal.
    /// </summary>
    /// <param name="touchEvent">The touch event.</param>
    private void EmitSingleTouch(InputEventSingleScreenTouch touchEvent)
    {
        EmitAnyGesture(nameof(SingleTouch), touchEvent);
    }

    /// <summary>
    /// Emits a single drag gesture signal.
    /// </summary>
    /// <param name="dragEvent">The drag event.</param>
    private void EmitSingleDrag(InputEventSingleScreenDrag dragEvent)
    {
        EmitAnyGesture(nameof(SingleDrag), dragEvent);
    }

    /// <summary>
    /// Emits a single swipe gesture signal.
    /// </summary>
    /// <param name="swipeEvent">The swipe event.</param>
    private void EmitSingleSwipe(InputEventSingleScreenSwipe swipeEvent)
    {
        EmitAnyGesture(nameof(SingleSwipe), swipeEvent);
    }

    /// <summary>
    /// Emits a single long press gesture signal.
    /// </summary>
    /// <param name="longPressEvent">The long press event.</param>
    private void EmitSingleLongPress(InputEventSingleScreenLongPress longPressEvent)
    {
        EmitAnyGesture(nameof(SingleLongPress), longPressEvent);
    }

    /// <summary>
    /// Emits a multi-finger tap gesture signal.
    /// </summary>
    /// <param name="tapEvent">The tap event.</param>
    private void EmitMultiTap(InputEventMultiScreenTap tapEvent)
    {
        EmitAnyGesture(nameof(MultiTap), tapEvent);
    }

    /// <summary>
    /// Emits a multi-finger swipe gesture signal.
    /// </summary>
    /// <param name="swipeEvent">The swipe event.</param>
    private void EmitMultiSwipe(InputEventMultiScreenSwipe swipeEvent)
    {
        EmitAnyGesture(nameof(MultiSwipe), swipeEvent);
    }


    /// <summary>
    /// Emits a multi-finger long press gesture signal.
    /// </summary>
    /// <param name="longPressEvent">The long press event.</param>
    private void EmitMultiLongPress(InputEventMultiScreenLongPress longPressEvent)
    {
        EmitAnyGesture(nameof(MultiLongPress), longPressEvent);
    }

    /// <summary>
    /// Emits a pinch gesture signal.
    /// </summary>
    /// <param name="pinchEvent">The pinch event.</param>
    private void EmitPinch(InputEventScreenPinch pinchEvent)
    {
        EmitAnyGesture(nameof(Pinch), pinchEvent);
    }

    /// <summary>
    /// Emits a twist gesture signal.
    /// </summary>
    /// <param name="twistEvent">The twist event.</param>
    private void EmitTwist(InputEventScreenTwist twistEvent)
    {
        EmitAnyGesture(nameof(Twist), twistEvent);
    }

    /// <summary>
    /// Emits the raw gesture data signal.
    /// This can be used to observe the low-level details of the gesture.
    /// </summary>
    /// <param name="InputEventGesture">The raw gesture data.</param>
    private void EmitRawGesture(InputEventGesture InputEventGesture)
    {
        EmitSignal(nameof(InputEventGesture), InputEventGesture);
        EmitSignal(nameof(AnyGesture), "input_event_gesture", InputEventGesture);
    }

    /// <summary>
    /// Emits a gesture cancellation signal.
    /// This is used when a gesture is interrupted or canceled.
    /// </summary>
    /// <param name="cancelEvent">The cancel event.</param>
    private void EmitCancel(InputEventScreenCancel cancelEvent)
    {
        EmitAnyGesture(nameof(Cancel), cancelEvent);
    }

    #endregion
}
