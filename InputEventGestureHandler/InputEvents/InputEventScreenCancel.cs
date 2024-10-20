using Godot;
using System;

/// <summary>
/// Represents a screen cancel input event, extending InputEventAction.
/// </summary>
public partial class InputEventScreenCancel : InputEventAction
{
    /// <summary>
    /// The raw gesture data associated with this cancel event.
    /// </summary>
    public InputEventGesture InputEventGesture { get; set; }

    /// <summary>
    /// The input event that triggered the cancel.
    /// </summary>
    public InputEvent Event { get; set; }

    /// <summary>
    /// Initializes a new instance of the InputEventScreenCancel class.
    /// </summary>
    /// <param name="_raw_gesture">The raw gesture data.</param>
    /// <param name="_event">The input event that triggered the cancel.</param>
    public InputEventScreenCancel(InputEventGesture _raw_gesture, InputEvent _event)
    {
        InputEventGesture = _raw_gesture;
        Event = _event;
    }

    /// <summary>
    /// Returns a string representation of the screen cancel event.
    /// </summary>
    /// <returns>A string indicating the gesture was canceled.</returns>
    public string AsString()
    {
        return "gesture canceled";
    }

    /// <summary>
    /// Overrides the default ToString method to provide a string representation of the event.
    /// </summary>
    /// <returns>A string indicating the gesture was canceled.</returns>
    public override string ToString()
    {
        return AsString();
    }
}
