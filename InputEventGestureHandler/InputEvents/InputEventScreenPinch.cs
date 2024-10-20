using Godot;
using System;

/// <summary>
/// Represents a screen pinch input event, extending InputEventAction.
/// </summary>
public partial class InputEventScreenPinch : InputEventAction
{
    /// <summary>
    /// The centroid position of all drag points.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// The relative movement magnitude of the pinch.
    /// </summary>
    public float Relative { get; set; }

    /// <summary>
    /// The total distance covered during the pinch.
    /// </summary>
    public float Distance { get; set; }

    /// <summary>
    /// The number of fingers involved in the pinch.
    /// </summary>
    public int Fingers { get; set; }

    /// <summary>
    /// The raw gesture data associated with this pinch event.
    /// </summary>
    public InputEventGesture InputEventGesture { get; set; }

    /// <summary>
    /// Initializes a new instance of the InputEventScreenPinch class.
    /// </summary>
    /// <param name="_raw_gesture">The raw gesture data. Optional.</param>
    /// <param name="screenDragEvent">The screen drag event. Optional.</param>
    public InputEventScreenPinch(InputEventGesture _raw_gesture = null, InputEventScreenDrag screenDragEvent = null)
    {
        InputEventGesture = _raw_gesture;
        if (InputEventGesture != null)
        {
            Fingers = InputEventGesture.Drags.Count;
            Position = InputEventGesture.Centroid("drags", "position");

            Distance = 0f;
            foreach (var drag in InputEventGesture.Drags.Values)
            {
                Vector2 centroidRelativePosition = drag.Position - Position;
                Distance += centroidRelativePosition.Length();
            }

            if (screenDragEvent != null)
            {
                Vector2 centroidRelativePositionEvent = screenDragEvent.Position - Position;
                Relative = (centroidRelativePositionEvent + screenDragEvent.Relative).Length() - centroidRelativePositionEvent.Length();
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
            Distance = 0f;
            Relative = 0f;
        }
    }

    /// <summary>
    /// Returns a string representation of the screen pinch event.
    /// </summary>
    /// <returns>A string detailing position, relative movement, distance, and finger count.</returns>
    public string AsString()
    {
        return $"position={Position}|relative={Relative}|distance={Distance}|fingers={Fingers}";
    }

    /// <summary>
    /// Overrides the default ToString method to provide a string representation of the event.
    /// </summary>
    /// <returns>A string detailing position, relative movement, distance, and finger count.</returns>
    public override string ToString()
    {
        return AsString();
    }
}
