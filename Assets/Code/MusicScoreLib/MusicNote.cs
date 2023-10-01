using System.Collections;
using System.Collections.Generic;

/* External file containing properties of every note (obstacle)
 *   Note              --> The properties of the obstacle
 *                             + length: [whole, half, quarter, eighth, sixteenth]
 *                               in sub-beats
 *                             + location: the physical location [lane, height, etc.]
 *                             + voice: which part of song [bass, melody, etc]
 *                             + type: what kind of obstacle [projectile, beam, wall, 
 *                               etc.]
 */

public readonly struct Note
{
    private readonly NoteLength len;
    public readonly int Length { get { return (int)len;  } }

    private readonly List<NoteLocation> loc;
    public readonly List<NoteLocation> Location { get { return loc; } }

    private readonly NoteVoice voc;
    public readonly int Voice { get { return (int)voc; } }

    private readonly NoteType type;
    public readonly int Type { get { return (int)type; } }

    // Constructor
    public Note(NoteLength length, 
                List<NoteLocation> location, 
                NoteVoice voice, 
                NoteType type)
    {
        len = length;
        loc = location;
        voc = voice;
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
    Lane1,
    Lane2,
    Lane3,
    Lane4
}

public enum NoteVoice
{
    Bass,
    Harmony,
    Melody
}

public enum NoteType
{
    Rest,            // empty note, will not spawm object
    BlockObstacle,
    BulletProjectile,
    BeamProjectile,
    
    // Add additional obstacle types
    // as we develop
}

