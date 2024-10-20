using Godot;
using System;

/// <summary>
/// Represents a multi-screen swipe input event, extending InputEventAction.
/// </summary>
public partial class InputEventMultiScreenSwipe : InputEventAction
{
    /// <summary>
    /// The centroid position of all press points.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// The relative movement vector of the swipe.
    /// </summary>
    public Vector2 Relative { get; set; }

    /// <summary>
    /// The number of fingers involved in the swipe.
    /// </summary>
    public int Fingers { get; set; }

    /// <summary>
    /// The raw gesture data associated with this swipe event.
    /// </summary>
    public InputEventGesture InputEventGesture { get; set; }

    /// <summary>
    /// Initializes a new instance of the InputEventMultiScreenSwipe class.
    /// </summary>
    /// <param name="_raw_gesture">The raw gesture data. Optional.</param>
    /// <param name="eventDrag">The screen drag event. Optional.</param>
    public InputEventMultiScreenSwipe(InputEventGesture _raw_gesture = null, InputEventScreenDrag eventDrag = null)
    {
        InputEventGesture = _raw_gesture;
        if (InputEventGesture != null)
        {
            Fingers = InputEventGesture.Size();
            Position = InputEventGesture.Centroid("presses", "position");
            Relative = InputEventGesture.Centroid("releases", "position") - Position;
        }
        else
        {
            Fingers = 0;
            Position = Vector2.Zero;
            Relative = Vector2.Zero;
        }
    }

    /// <summary>
    /// Returns a string representation of the multi-screen swipe event.
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
