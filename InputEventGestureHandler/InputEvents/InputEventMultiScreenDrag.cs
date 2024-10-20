using Godot;
using System;

/// <summary>
/// Represents a multi-screen drag input event, extending InputEventAction.
/// </summary>
public partial class InputEventMultiScreenDrag : InputEventAction
{
    /// <summary>
    /// The centroid position of all drag points.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// The average relative movement of the drag points.
    /// </summary>
    public Vector2 Relative { get; set; }

    /// <summary>
    /// The number of fingers involved in the drag.
    /// </summary>
    public int Fingers { get; set; }

    /// <summary>
    /// The raw gesture data associated with this drag event.
    /// </summary>
    public InputEventGesture InputEventGesture { get; set; }

    /// <summary>
    /// Initializes a new instance of the InputEventMultiScreenDrag class.
    /// </summary>
    /// <param name="_raw_gesture">The raw gesture data.</param>
    /// <param name="eventDrag">The screen drag event.</param>
    public InputEventMultiScreenDrag(InputEventGesture _raw_gesture = null, InputEventScreenDrag eventDrag = null)
    {
        InputEventGesture = _raw_gesture;
        if (InputEventGesture != null)
        {
            Fingers = InputEventGesture.Size();
            Position = InputEventGesture.Centroid("drags", "position");
            Relative = eventDrag != null && Fingers != 0 ? eventDrag.Relative / Fingers : Vector2.Zero;
        }
        else
        {
            Fingers = 0;
            Position = Vector2.Zero;
            Relative = Vector2.Zero;
        }
    }

    /// <summary>
    /// Returns a string representation of the multi-screen drag event.
    /// </summary>
    /// <returns>A string detailing position, relative movement, and finger count.</returns>
    public string AsString()
    {
        return $"position={Position}|relative={Relative}|fingers={Fingers}";
    }

    /// <summary>
    /// Overrides the default ToString method to provide a string representation of the event.
    /// </summary>
    /// <returns>A string detailing position, relative movement, and finger count.</returns>
    public override string ToString()
    {
        return AsString();
    }
}
