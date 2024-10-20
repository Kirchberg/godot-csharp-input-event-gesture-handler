using Godot;
using System;
using System.Linq;

/// <summary>
/// Represents a single-screen swipe input event, extending InputEventAction.
/// </summary>
public partial class InputEventSingleScreenSwipe : InputEventAction
{
    /// <summary>
    /// The starting position of the swipe.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// The relative movement vector of the swipe.
    /// </summary>
    public Vector2 Relative { get; set; }

    /// <summary>
    /// The raw gesture data associated with this swipe event.
    /// </summary>
    public InputEventGesture InputEventGesture { get; set; }

    /// <summary>
    /// Initializes a new instance of the InputEventSingleScreenSwipe class.
    /// </summary>
    /// <param name="_raw_gesture">The raw gesture data. Optional.</param>
    public InputEventSingleScreenSwipe(InputEventGesture _raw_gesture = null)
    {
        InputEventGesture = _raw_gesture;
        if (InputEventGesture != null)
        {
            if (InputEventGesture.Presses.Count > 0 && InputEventGesture.Releases.Count > 0)
            {
                Position = InputEventGesture.Presses.Values.First().Position;
                Relative = InputEventGesture.Releases.Values.First().Position - Position;
            }
            else
            {
                Position = Vector2.Zero;
                Relative = Vector2.Zero;
            }
        }
        else
        {
            Position = Vector2.Zero;
            Relative = Vector2.Zero;
        }
    }

    /// <summary>
    /// Returns a string representation of the single-screen swipe event.
    /// </summary>
    /// <returns>A string detailing position and relative movement.</returns>
    public string AsString()
    {
        return $"position={Position}|relative={Relative}";
    }

    /// <summary>
    /// Overrides the default ToString method to provide a string representation of the event.
    /// </summary>
    /// <returns>A string detailing position and relative movement.</returns>
    public override string ToString()
    {
        return AsString();
    }
}
