using Godot;
using System;
using System.Linq;

/// <summary>
/// Represents a single-screen drag input event, extending InputEventAction.
/// </summary>
public partial class InputEventSingleScreenDrag : InputEventAction
{
    /// <summary>
    /// The position of the drag.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// The relative movement of the drag.
    /// </summary>
    public Vector2 Relative { get; set; }

    /// <summary>
    /// The raw gesture data associated with this drag event.
    /// </summary>
    public InputEventGesture InputEventGesture { get; set; }

    /// <summary>
    /// Initializes a new instance of the InputEventSingleScreenDrag class.
    /// </summary>
    /// <param name="_raw_gesture">The raw gesture data. Optional.</param>
    public InputEventSingleScreenDrag(InputEventGesture _raw_gesture = null)
    {
        InputEventGesture = _raw_gesture;
        if (InputEventGesture != null && InputEventGesture.Drags.Count > 0)
        {
            // Retrieve the first drag event
            var dragEvent = InputEventGesture.Drags.Values.FirstOrDefault();
            if (dragEvent != null)
            {
                Position = dragEvent.Position;
                Relative = dragEvent.Relative;
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
    /// Returns a string representation of the single-screen drag event.
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
