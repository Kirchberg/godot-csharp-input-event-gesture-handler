using Godot;
using System;

/// <summary>
/// Represents a multi-screen tap input event, extending InputEventAction.
/// </summary>
public partial class InputEventMultiScreenTap : InputEventAction
{
    /// <summary>
    /// The centroid position of all press points.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// The number of fingers involved in the tap.
    /// </summary>
    public int Fingers { get; set; }

    /// <summary>
    /// The raw gesture data associated with this tap event.
    /// </summary>
    public InputEventGesture InputEventGesture { get; set; }

    /// <summary>
    /// Initializes a new instance of the InputEventMultiScreenTap class.
    /// </summary>
    /// <param name="_raw_gesture">The raw gesture data. Optional.</param>
    public InputEventMultiScreenTap(InputEventGesture _raw_gesture = null)
    {
        InputEventGesture = _raw_gesture;
        if (InputEventGesture != null)
        {
            Fingers = InputEventGesture.Size();
            Position = InputEventGesture.Centroid("presses", "position");
        }
        else
        {
            Fingers = 0;
            Position = Vector2.Zero;
        }
    }

    /// <summary>
    /// Returns a string representation of the multi-screen tap event.
    /// </summary>
    /// <returns>A string detailing position and finger count.</returns>
    public string AsString()
    {
        return $"position={Position}|fingers={Fingers}";
    }

    /// <summary>
    /// Overrides the default ToString method to provide a string representation of the event.
    /// </summary>
    /// <returns>A string detailing position and finger count.</returns>
    public override string ToString()
    {
        return AsString();
    }
}
