using Godot;
using System;

/// <summary>
/// Represents a screen twist input event, extending InputEventAction.
/// </summary>
public partial class InputEventScreenTwist : InputEventAction
{
    /// <summary>
    /// The centroid position of all drag points.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// The relative angular movement of the twist.
    /// </summary>
    public float Relative { get; set; }

    /// <summary>
    /// The number of fingers involved in the twist.
    /// </summary>
    public int Fingers { get; set; }

    /// <summary>
    /// The raw gesture data associated with this twist event.
    /// </summary>
    public InputEventGesture InputEventGesture { get; set; }

    /// <summary>
    /// Initializes a new instance of the InputEventScreenTwist class.
    /// </summary>
    /// <param name="_raw_gesture">The raw gesture data. Optional.</param>
    /// <param name="screenDragEvent">The screen drag event. Optional.</param>
    public InputEventScreenTwist(InputEventGesture _raw_gesture = null, InputEventScreenDrag screenDragEvent = null)
    {
        InputEventGesture = _raw_gesture;
        if (InputEventGesture != null)
        {
            Fingers = InputEventGesture.Drags.Count;
            Position = InputEventGesture.Centroid("drags", "position");

            if (screenDragEvent != null && Fingers != 0)
            {
                Vector2 centroidRelativePosition = screenDragEvent.Position - Position;
                Vector2 updatedRelativePosition = centroidRelativePosition + screenDragEvent.Relative;
                Relative = centroidRelativePosition.AngleTo(updatedRelativePosition) / Fingers;
            }
            else
            {
                Relative = 0f;
            }
        }
        else
        {
            Fingers = 0;
            Position = Vector2.Zero;
            Relative = 0f;
        }
    }

    /// <summary>
    /// Returns a string representation of the screen twist event.
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
