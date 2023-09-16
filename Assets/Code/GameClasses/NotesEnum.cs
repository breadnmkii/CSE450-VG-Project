using System.Collections;
using System.Collections.Generic;

// External file containing properties of every note (obstacle)
// - NoteType: the type of obstacle representing the note
// - NotePitch: the physical location (e.g. height) of the note
enum NoteType
{
    // ReservedObstacle = 0,
    BlockObstacle = 1,
    BallProjectile = 2,
    
    // Add additional obstacle types
    // as we develop
}

// Define different physical locations an obstacle may be at
enum NotePitch
{
    Low,
    Mid,
    High
    // we might want more later on,
    // maybe even like slur note slide
    // types?
}