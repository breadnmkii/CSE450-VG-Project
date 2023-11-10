using System;
using System.Collections.Generic;
using UnityEngine;

/* External file containing properties of every note (obstacle)
 *   Note              --> The properties of the obstacle
 *                             + length: [whole, half, quarter, eighth, sixteenth]
 *                               in sub-beats
 *                             + location: the physical location [lane, height, etc.]
 *                             + dotted: whether note is dotted
 *                             + type: what kind of obstacle [projectile, beam, wall, 
 *                               etc.]
 */

namespace MusicNote
{
    public class MusicNoteHelper
    {
        public static LayerMask GetLayerFromNoteloc(NoteLocation loc)
        {
            return loc switch
            {
                NoteLocation.Lane0 => (LayerMask)LayerMask.NameToLayer("Lane0"),
                NoteLocation.Lane1 => (LayerMask)LayerMask.NameToLayer("Lane1"),
                NoteLocation.Lane2 => (LayerMask)LayerMask.NameToLayer("Lane2"),
                NoteLocation.Lane3 => (LayerMask)LayerMask.NameToLayer("Lane3"),
                _ => throw new Exception("(MusicNote.cs) Invalid note location!"),
            };
        }
    }
    
    public readonly struct Note
    {
        // Note member variables
        private readonly NoteType type;     // Projectile type of note
        private readonly NoteLength len;    // Subdivision length of note
        private readonly bool dotted;       // Whether note is dotted
        private readonly bool tied;         // Whether note is tied/slurred to previous
        private readonly List<NoteLocation> loc;    // List of lane locations


        // Getters
        public readonly NoteType Type { get { return type; } }
        public readonly int Length { get { return (int)len; } }
        public readonly bool isDotted { get { return dotted; } }
        public readonly bool isTied {  get { return tied; } }
        public readonly List<NoteLocation> Location { get { return loc; } }


        // Constructors
        public Note(
            NoteType noteType,
            NoteLength noteLength,
            bool isDottedNote,
            bool isTiedNote,
            NoteLocation noteLocation)
        {
            List<NoteLocation> noteLoc = new()
            {
            noteLocation
            };

            type = noteType;
            len = noteLength;
            dotted = isDottedNote;
            tied = isTiedNote;
            loc = noteLoc;
        }

        public Note(
            NoteType noteType,
            NoteLength noteLength,
            bool isDottedNote,
            bool isTiedNote,
            List<NoteLocation> noteLocation)
        {
            type = noteType;
            len = noteLength;
            dotted = isDottedNote;
            tied = isTiedNote;
            loc = noteLocation;
        }
    }

}

public enum NoteType
{
    Rest,            // empty note, will spawn wall obstacle
    BallProjectileA,
    BallProjectileB,

    // Add additional obstacle types
    // as we develop
}


// Various note subdivision lengths
public enum NoteLength
{
    Whole = 1,
    Half = 2,
    Quarter = 4,
    Eighth = 8,
    Triplet = 12,
    Sixteenth = 16,
    Thirtysecond = 32,
    Sixtyfourth = 64,
    OneTwentyEighth = 128,
    TwoFiftySixth = 256,
    FiveTwelfth = 512,
    TenTwentyFourth = 1024
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
