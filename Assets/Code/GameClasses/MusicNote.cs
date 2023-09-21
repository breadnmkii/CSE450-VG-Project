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

struct Note
{
    public NoteLength len;
    public NoteLocation loc;
    public NoteVoice voc;
    public NoteType type;
}

public class NoteLength
{
    public static readonly int Whole = 1;
    public static readonly int Half = 2;
    public static readonly int Quarter = 4;
    public static readonly int Eighth = 8;
    public static readonly int Sixteenth = 16;
}

// Define different physical locations an obstacle may be at
// currently only supports lane location
enum NoteLocation
{
    Lane1,
    Lane2,
    Lane3,
    Lane4
}

enum NoteVoice
{
    bass,
    harmony,
    melody
}

enum NoteType
{
    BlockObstacle,
    BallProjectile,
    BeamProjectile,
    
    // Add additional obstacle types
    // as we develop
}

