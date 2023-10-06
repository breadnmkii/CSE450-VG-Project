using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

/* External file containing properties of every note (obstacle)
 *   Note              --> The properties of the obstacle
 *                             + length: [whole, half, quarter, eighth, sixteenth]
 *                               in sub-beats
 *                             + location: the physical location [lane, height, etc.]
 *                             + voice: which part of song [bass, melody, etc]
 *                             + type: what kind of obstacle [projectile, beam, wall, 
 *                               etc.]
 */

public class MusicNote
{
    public static LayerMask GetLayerFromNoteloc(NoteLocation loc)
    {
        return loc switch
        {
            NoteLocation.Lane0 => (LayerMask)LayerMask.NameToLayer("Lane0"),
            NoteLocation.Lane1 => (LayerMask)LayerMask.NameToLayer("Lane1"),
            NoteLocation.Lane2 => (LayerMask)LayerMask.NameToLayer("Lane2"),
            NoteLocation.Lane3 => (LayerMask)LayerMask.NameToLayer("Lane3"),
            _ => throw new System.Exception("(MusicNote.cs) Invalid note location!"),
        };
    }
}

public readonly struct Note
{
    private readonly NoteLength len;
    public readonly int Length { get { return (int)len;  } }

    private readonly List<NoteLocation> loc;
    public readonly List<NoteLocation> Location { get { return loc; } }

    private readonly NoteType type;
    public readonly int Type { get { return (int)type; } }

    // Constructor
    public Note(NoteLength length, 
                List<NoteLocation> location, 
                NoteType type)
    {
        len = length;
        loc = location;
        this.type = type;
    }
}

public enum NoteLength
{
    Whole = 1,
    Half = 2,
    Quarter = 4,
    Eighth = 8,
    Sixteenth = 16
}

// Define different physical locations an obstacle may be at
// currently only supports lane location
public enum NoteLocation
{
    Lane0,
    Lane1,
    Lane2,
    Lane3
}

public enum NoteType
{
    Rest,            // empty note, will not spawm object
    WallObstacle,
    BallProjectileA,
    BallProjectileB,
    
    // Add additional obstacle types
    // as we develop
}
