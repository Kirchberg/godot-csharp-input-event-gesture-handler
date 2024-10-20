using Godot;
using System;
using System.Collections.Generic;

public static class InputEventGestureHelpers
{
    // Constants
    public const float SEC_IN_MSEC = 1000f; // Milliseconds in a second
    public const float SEC_IN_USEC = 1000000f; // Microseconds in a second

    /// <summary>
    /// Calculates the centroid (average) of a non-empty list of Vector2 objects.
    /// </summary>
    /// <param name="es">A list of Vector2 objects.</param>
    /// <returns>The centroid as a Vector2.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the input list is empty.</exception>
    public static Vector2 Centroid(List<Vector2> points)
    {
        if (points.Count == 0)
        {
            throw new InvalidOperationException("Cannot calculate centroid of an empty list.");
        }

        // Calculate the sum of all points
        Vector2 sum = Vector2.Zero;
        foreach (Vector2 point in points)
        {
            sum += point;
        }

        // Calculate the average (centroid)
        return sum / points.Count;
    }

    /// <summary>
    /// Retrieves the current time in seconds with millisecond or microsecond precision.
    /// </summary>
    /// <returns>The current time as a float.</returns>
    public static float Now()
    {
        // Use Time singleton instead of OS
        return (float)(Time.GetTicksMsec()) / SEC_IN_MSEC;
    }
}
