using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents a raw gesture input event, capturing presses, releases, and drags.
/// </summary>
public partial class InputEventGesture : InputEventAction
{
    #region Nested Classes

    /// <summary>
    /// Base class for different types of events within a gesture.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// The timestamp of the event in seconds.
        /// </summary>
        public float Time { get; set; } = -1f; // (secs)

        /// <summary>
        /// The index identifier for the event (e.g., finger index).
        /// </summary>
        public int Index { get; set; } = -1;

        /// <summary>
        /// Returns a string representation of the event.
        /// </summary>
        /// <returns>Formatted string with index and time.</returns>
        public virtual string AsString()
        {
            return $"ind: {Index} | time: {Time}";
        }
    }

    /// <summary>
    /// Represents a touch event within a gesture.
    /// </summary>
    public class Touch : Event
    {
        /// <summary>
        /// The position of the touch on the screen.
        /// </summary>
        public Vector2 Position { get; set; } = Vector2.Zero;

        /// <summary>
        /// Indicates whether the touch is pressed or released.
        /// </summary>
        public bool Pressed { get; set; }

        /// <summary>
        /// Returns a string representation of the touch event.
        /// </summary>
        /// <returns>Formatted string with position and pressed state.</returns>
        public override string AsString()
        {
            return $"{base.AsString()} | pos: {Position} | pressed: {Pressed}";
        }
    }

    /// <summary>
    /// Represents a drag event within a gesture.
    /// </summary>
    public class Drag : Event
    {
        /// <summary>
        /// The current position of the drag.
        /// </summary>
        public Vector2 Position { get; set; } = Vector2.Zero;

        /// <summary>
        /// The relative movement since the last update.
        /// </summary>
        public Vector2 Relative { get; set; } = Vector2.Zero;

        /// <summary>
        /// The velocity of the drag.
        /// </summary>
        public Vector2 Velocity { get; set; } = Vector2.Zero;

        /// <summary>
        /// Returns a string representation of the drag event.
        /// </summary>
        /// <returns>Formatted string with position and relative movement.</returns>
        public override string AsString()
        {
            return $"{base.AsString()} | pos: {Position} | relative: {Relative}";
        }
    }

    #endregion

    #region Constants

    /// <summary>
    /// Number of microseconds in a second, useful for converting time representations.
    /// </summary>
    private const int SEC_IN_USEC = 1000000;

    #endregion

    #region Private Variables

    /// <summary>
    /// Dictionary mapping touch indices to their press events.
    /// Each entry holds the index (e.g., finger) and the corresponding press event.
    /// </summary>
    private Dictionary<int, Touch> presses = new Dictionary<int, Touch>();

    /// <summary>
    /// Dictionary mapping touch indices to their release events.
    /// Similar to the presses dictionary, but for touch release actions.
    /// </summary>
    private Dictionary<int, Touch> releases = new Dictionary<int, Touch>();

    /// <summary>
    /// Dictionary mapping drag indices to their drag events.
    /// </summary>
    private Dictionary<int, Drag> drags = new Dictionary<int, Drag>();

    /// <summary>
    /// Nested dictionary storing the history of events categorized by type.
    /// Key: Touch Index
    /// Value: Dictionary where Key is Event Type ("presses", "releases", "drags") and Value is a List of Events.
    /// </summary>
    private Dictionary<int, Dictionary<string, List<Event>>> history = new Dictionary<int, Dictionary<string, List<Event>>>();

    /// <summary>
    /// The number of active touches currently detected.
    /// This value updates as touches are pressed and released.
    /// </summary>
    private int activeTouches = 0;

    /// <summary>
    /// The start time of the current gesture in seconds.
    /// Used to measure the duration of a gesture.
    /// </summary>
    private float startTime = -1f; // (secs)

    /// <summary>
    /// The elapsed time since the start of the current gesture in seconds.
    /// </summary>
    private float elapsedTime = -1f; // (secs)

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the dictionary of press events.
    /// </summary>
    public Dictionary<int, Touch> Presses => presses;

    /// <summary>
    /// Gets the dictionary of release events.
    /// </summary>
    public Dictionary<int, Touch> Releases => releases;

    /// <summary>
    /// Gets the dictionary of drag events.
    /// </summary>
    public Dictionary<int, Drag> Drags => drags;

    /// <summary>
    /// Gets the number of active touches currently detected.
    /// </summary>
    public int ActiveTouches => activeTouches;

    /// <summary>
    /// Gets the elapsed time since the start of the current gesture in seconds.
    /// </summary>
    public float ElapsedTime => elapsedTime;

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns the number of current presses.
    /// This can be used to determine how many touches are being pressed at a given moment.
    /// </summary>
    /// <returns>Count of current presses.</returns>
    public int Size()
    {
        return presses.Count;
    }

    /// <summary>
    /// Calculates the centroid (average position) of a set of events based on the specified event type and property.
    /// Useful for analyzing gesture data like the average position of touches.
    /// </summary>
    /// <param name="eventsName">The name of the events collection ("presses", "releases", "drags").</param>
    /// <param name="propertyName">The property name to extract from each event ("position", "relative").</param>
    /// <returns>The centroid as a Vector2.</returns>
    public Vector2 Centroid(string eventsName, string propertyName)
    {
        // Retrieve the list of events based on the event name (presses, releases, drags)
        List<Event> eventsList = GetEventsIndex(eventsName);
        List<Vector2> vectors = new List<Vector2>();

        // Iterate through each event and collect its relevant property (position, relative)
        foreach (var evt in eventsList)
        {
            switch (propertyName.ToLower())
            {
                case "position":
                    if (evt is Touch touch)
                        vectors.Add(touch.Position);
                    else if (evt is Drag drag)
                        vectors.Add(drag.Position);
                    break;
                case "relative":
                    if (evt is Drag dragRelative)
                        vectors.Add(dragRelative.Relative);
                    break;
                // Add additional properties if necessary
                default:
                    GD.PrintErr($"Unknown propertyName: {propertyName}");
                    break;
            }
        }

        // If no vectors were collected, return a zero vector
        if (vectors.Count == 0)
            return Vector2.Zero;

        // Calculate and return the centroid of the collected vectors
        return CalculateCentroid(vectors);
    }

    /// <summary>
    /// Calculates the centroid (average) of a list of Vector2 positions.
    /// </summary>
    /// <param name="vectors">List of Vector2 positions.</param>
    /// <returns>The centroid as a Vector2.</returns>
    private Vector2 CalculateCentroid(List<Vector2> vectors)
    {
        Vector2 sum = Vector2.Zero;
        foreach (var v in vectors)
        {
            sum += v;
        }
        return sum / vectors.Count;
    }

    /// <summary>
    /// Retrieves the end positions from presses, drags, and releases.
    /// This is useful for getting the final positions of each touch event at the end of a gesture.
    /// </summary>
    /// <returns>A dictionary mapping indices to their end positions.</returns>
    public Dictionary<int, Vector2> GetEnds()
    {
        Dictionary<int, Vector2> ends = new Dictionary<int, Vector2>();

        // Add end positions from presses
        foreach (var kvp in presses)
        {
            ends[kvp.Key] = kvp.Value.Position;
        }

        // Add end positions from drags
        foreach (var kvp in drags)
        {
            ends[kvp.Key] = kvp.Value.Position;
        }

        // Add end positions from releases
        foreach (var kvp in releases)
        {
            ends[kvp.Key] = kvp.Value.Position;
        }

        return ends;
    }

    /// <summary>
    /// Checks for gesture consistency by verifying if the difference between the start and end positions
    /// and the movement length are within defined limits. 
    /// This helps in determining if the gesture is valid or consistent.
    /// </summary>
    /// <param name="diffLimit">The maximum allowed difference between start and end positions.</param>
    /// <param name="lengthLimit">The maximum allowed length for relative positions. Defaults to -1 (no limit).</param>
    /// <returns>True if consistent; otherwise, false.</returns>
    public bool IsConsistent(float diffLimit, float lengthLimit = -1f)
    {
        // If no length limit is provided, set a default value
        if (lengthLimit == -1f)
        {
            lengthLimit = 100f; // Example default value
        }

        // Retrieve the end positions for the gesture
        var ends = GetEnds();

        // Calculate the centroids of the drag and press positions
        Vector2 endsCentroid = Centroid("drags", "position");
        Vector2 startsCentroid = Centroid("presses", "position");

        // Iterate through the end positions and verify if they meet the consistency criteria
        foreach (var kvp in ends)
        {
            int i = kvp.Key;
            Vector2 endPos = kvp.Value;

            // Check if there was a corresponding press for this touch
            if (!presses.ContainsKey(i))
                continue;

            Vector2 startRelative = presses[i].Position - startsCentroid;
            Vector2 endRelative = endPos - endsCentroid;

            // Verify if the relative positions and differences are within the specified limits
            bool valid = startRelative.Length() < lengthLimit &&
                         endRelative.Length() < lengthLimit &&
                         (endRelative - startRelative).Length() < diffLimit;

            // If any touch does not meet the criteria, return false
            if (!valid)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Rolls back the gesture history relative to a specified time.
    /// Useful for undoing or analyzing gesture events before a certain threshold.
    /// </summary>
    /// <param name="threshold">The relative time to roll back to.</param>
    /// <returns>The updated InputEventGesture after rollback.</returns>
    public InputEventGesture RollbackRelative(float threshold)
    {
        // Implement rollback logic based on the time threshold.
        // This function could clear or manipulate historical gesture data.
        // Placeholder implementation for demonstration purposes:
        return new InputEventGesture();
    }

    /// <summary>
    /// Rolls back the gesture history to an absolute time.
    /// Similar to RollbackRelative but uses an absolute timestamp.
    /// </summary>
    /// <param name="time">The absolute time to roll back to.</param>
    /// <returns>The updated InputEventGesture after rollback.</returns>
    public InputEventGesture RollbackAbsolute(float time)
    {
        // Implement rollback logic based on an absolute time.
        // Placeholder implementation for demonstration purposes:
        return new InputEventGesture();
    }

    /// <summary>
    /// Retrieves the latest event ID based on the specified time.
    /// Useful for finding the most recent event that occurred before a certain time.
    /// </summary>
    /// <param name="latestTime">The time to compare against. Defaults to -1.</param>
    /// <returns>A tuple containing the latest index and type, or null if none found.</returns>
    public Tuple<int, string> LatestEventId(float latestTime = -1f)
    {
        Tuple<int, string> res = null;
        float maxTime = latestTime;

        // Iterate through the history to find the latest event matching the criteria
        foreach (var index in history.Keys)
        {
            foreach (var type in history[index].Keys)
            {
                var eventTime = history[index][type][history[index][type].Count - 1].Time;
                if (eventTime >= maxTime)
                {
                    res = Tuple.Create(index, type);
                    maxTime = eventTime;
                }
            }
        }

        return res;
    }

    /// <summary>
    /// Provides a string representation of the InputEventGesture.
    /// It prints all the current press, drag, and release events in a human-readable format.
    /// </summary>
    /// <returns>A string detailing presses, drags, and releases.</returns>
    public override string ToString()
    {
        string txt = "presses:";
        foreach (var e in presses.Values)
        {
            txt += "\n" + e.AsString();
        }
        txt += "\ndrags:";
        foreach (var e in drags.Values)
        {
            txt += "\n" + e.AsString();
        }
        txt += "\nreleases:";
        foreach (var e in releases.Values)
        {
            txt += "\n" + e.AsString();
        }
        return txt;
    }

    /// <summary>
    /// Updates the gesture state based on a screen drag event.
    /// Adds the drag event to the history and updates the elapsed time of the gesture.
    /// </summary>
    /// <param name="event">The screen drag event.</param>
    /// <param name="time">The time of the event. Defaults to -1 (current time).</param>
    public void UpdateScreenDrag(InputEventScreenDrag @event, float time = -1f)
    {
        // If no specific time is provided, use the current system time
        if (time < 0f)
        {
            time = InputEventGestureHelpers.Now();
        }

        // Create a new Drag object and populate its properties
        Drag drag = new Drag
        {
            Position = @event.Position,
            Relative = @event.Relative,
            Velocity = @event.Velocity,
            Index = @event.Index,
            Time = time
        };

        // Add the drag event to history and update the current drag state
        AddHistory(@event.Index, "drags", drag);
        drags[@event.Index] = drag;

        // Update the elapsed time of the gesture
        elapsedTime = time - startTime;
    }

    /// <summary>
    /// Updates the gesture state based on a screen touch event.
    /// Adds the touch event to the history, updating the active touch and gesture state accordingly.
    /// </summary>
    /// <param name="event">The screen touch event.</param>
    /// <param name="time">The time of the event. Defaults to -1 (current time).</param>
    public void UpdateScreenTouch(InputEventScreenTouch @event, float time = -1f)
    {
        // If no specific time is provided, use the current system time
        if (time < 0f)
        {
            time = InputEventGestureHelpers.Now();
        }

        // Create a new Touch object and populate its properties
        Touch touch = new Touch
        {
            Position = @event.Position,
            Pressed = @event.Pressed,
            Index = @event.Index,
            Time = time
        };

        if (@event.Pressed)
        {
            // If the touch is pressed, add it to the press history and update the current state
            AddHistory(@event.Index, "presses", touch);
            presses[@event.Index] = touch;
            activeTouches += 1;
            releases.Remove(@event.Index);
            drags.Remove(@event.Index);
            
            // Start the timer for the gesture if it's the first touch
            if (activeTouches == 1)
            {
                startTime = time;
            }
        }
        else
        {
            // If the touch is released, add it to the release history and update the state
            AddHistory(@event.Index, "releases", touch);
            releases[@event.Index] = touch;
            activeTouches -= 1;
            drags.Remove(@event.Index);
        }

        // Update the elapsed time for the gesture
        elapsedTime = time - startTime;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Maps the event name to its corresponding list of events.
    /// Used to retrieve the relevant events for presses, releases, or drags.
    /// </summary>
    /// <param name="eventsName">The name of the events collection ("presses", "releases", "drags").</param>
    /// <returns>A list of events corresponding to the specified event name.</returns>
    private List<Event> GetEventsIndex(string eventsName)
    {
        List<Event> events = new List<Event>();
        // Populate the list based on the event type
        switch (eventsName.ToLower())
        {
            case "presses":
                foreach (var touch in presses.Values)
                    events.Add(touch);
                break;
            case "releases":
                foreach (var touch in releases.Values)
                    events.Add(touch);
                break;
            case "drags":
                foreach (var drag in drags.Values)
                    events.Add(drag);
                break;
            default:
                GD.PrintErr($"Unknown eventsName: {eventsName}");
                break;
        }
        return events;
    }

    /// <summary>
    /// Adds an event to the history.
    /// This ensures that each gesture event (press, release, drag) is recorded in the correct category.
    /// </summary>
    /// <param name="index">The index of the event.</param>
    /// <param name="type">The type of the event ("presses", "releases", "drags").</param>
    /// <param name="value">The event object.</param>
    private void AddHistory(int index, string type, Event value)
    {
        // Ensure the index exists in the history dictionary
        if (!history.ContainsKey(index))
        {
            history[index] = new Dictionary<string, List<Event>>();
        }

        // Ensure the type of event exists in the nested dictionary for that index
        if (!history[index].ContainsKey(type))
        {
            history[index][type] = new List<Event>();
        }

        // Add the event to the history
        history[index][type].Add(value);
    }

    #endregion
}
