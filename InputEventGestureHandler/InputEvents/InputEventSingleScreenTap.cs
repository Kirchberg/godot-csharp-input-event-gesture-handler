using Godot;
using System;
using System.Linq;

/// <summary>
/// Represents a single-screen tap input event, extending InputEventAction.
/// </summary>
public partial class InputEventSingleScreenTap : InputEventAction
{
    /// <summary>
    /// The position where the tap occurred.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// The raw gesture data associated with this tap event.
    /// </summary>
    public InputEventGesture InputEventGesture { get; set; }

    /// <summary>
    /// Initializes a new instance of the InputEventSingleScreenTap class.
    /// </summary>
    /// <param name="_raw_gesture">The raw gesture data. Optional.</param>
    public InputEventSingleScreenTap(InputEventGesture _raw_gesture = null)
    {
        InputEventGesture = _raw_gesture;
        if (InputEventGesture != null && InputEventGesture.Presses.Count > 0)
        {
            Position = InputEventGesture.Presses.Values.First().Position;
        }
        else
        {
            Position = Vector2.Zero;
        }
    }

    /// <summary>
    /// Returns a string representation of the single-screen tap event.
    /// </summary>
    /// <returns>A string detailing the position.</returns>
    public string AsString()
    {
        return $"position={Position}";
    }

    /// <summary>
    /// Overrides the default ToString method to provide a string representation of the event.
    /// </summary>
    /// <returns>A string detailing the position.</returns>
    public override string ToString()
    {
        return AsString();
    }
}
