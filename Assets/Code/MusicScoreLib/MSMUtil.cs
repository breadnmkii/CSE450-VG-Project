using System;
using System.Collections.Generic;
using UnityEngine;

// XML parser
using System.Xml;

// MusicNote
using MusicNote;

public class MSMUtil : MonoBehaviour
{
    /***** Private Members ***************************************************/
    // Distance b/w spawn and hitzone middle
    // private static double _spawnToZoneDist = Math.Abs(collisionChecker.transform.position[0] - lanes[0].transform.position[0]);
    private static double _spawnToZoneDist = 0.0;


    /***** Private Methods ***************************************************/



    /***** Public Members ***************************************************/
    // Outlets
    public GameObject[] lanes;
    public GameObject collisionChecker;
    // Public obstacle prefabs (Note: must contain Obstacles component)
    public GameObject ballProjectileA;
    public GameObject ballProjectileB;
    public GameObject wallObstacle;


    /***** Public Methods ***************************************************/
    /* MusicXML Parser Docs:
     * 
     * MusicXML Format: (export as SPECIFIC INSTRUMENT + "No sys or page breaks"
     * 
     * HEADER (metadata) BLOCK
     * <part id="<instrument-id>">
     *   <measure>              # special case for first measure 1, contains meta data
     *     <attributes>
     *       <time>             # timesignature
     *         <beats>          # beats per measure  (numerator)
     *         <beat-type>      # beat duration      (denominator)
     *         
     *       <direction>
     *         <direction-type>
     *           <metronome>
     *             <per-minute> # the BPM of the song
     * 
     * BODY (song content) BLOCK
     * <measure>
     *   <note>
     *     
     *     </chord>             # ignore chord notes
     *    
     *      <pitch>             # determines note location, compare ASCII value
     *        <step>            # of OCTAVE,STEP (0-9,A-G)
     *        <octave>
     *      <rest/>             # if contains "rest" tag, is rest note
     *        
     *      <staff>             # tells which "hand" note is on, important for
     *                          # piano instrument with "left/right" hands,
     *                          # for rhythm game want "right" (1) for treble melody
     *                          # i.e. ignore <staff>2</staff>
     *                          # NOTE: if instrument only has single staff, no tag
     *                          #       will be present
     *      
     *      <type>              # string equivalent of note length of values:
     *                          # whole, half, quarter, eight, 16th, 32nd, 64th, 128th, 256th
     *      </dot>              # if is a dotted note (length = type * 1.5)
     *      
     * Wrap-around Note Location Placement Method:
     * - automatically determine the lane position by wrapping around
     *   note steps up and down (ascending notes of 8 for 4 beat measure
     *   would be two series of four increasing notes per lane)
     */
    // Create music score from a XML file and specified instrument part
    public static MusicScore ProcessMusicScore(
        TextAsset xmlText,
        string instrumentPartID,
        Difficulty songDifficulty)
    {
        List<Note> notes = new();
        int BPM;

        XmlDocument doc = new();
        Debug.Log("(MSMUtil) Loading XML...");
        doc.LoadXml(xmlText.text); // might be super inefficient
        XmlNode root = doc.DocumentElement;
        Debug.Log("(MSMUtil) XML Loaded!");

        /* HEAD BLOCK PROCESSING */
        // get first measure metadata
        Debug.Log("(MSMUtil) XML MUSIC METADATA PROCESSING");
        XmlNode songMetadata = root.SelectSingleNode($"//part[@id='{instrumentPartID}']/measure[@number='1']/attributes");
        XmlNode songDirection = root.SelectSingleNode($"//part[@id='{instrumentPartID}']/measure[@number='1']/direction");

        if (songDirection.SelectSingleNode("./direction-type/metronome/per-minute") != null)
        {
            BPM = int.Parse(songDirection.SelectSingleNode("./direction-type/metronome/per-minute").InnerXml);
        }
        else
        {
            throw new Exception("(MSMUtil) Did not process BPM from score! Please enter manually");
        }
        
        int beatsPerMeasure     = int.Parse(songMetadata.SelectSingleNode("./time/beats").InnerXml);
        NoteLength beatDuration = (NoteLength)int.Parse(songMetadata.SelectSingleNode("./time/beat-type").InnerXml);
        bool isSingleStaff      = int.Parse(songMetadata.SelectSingleNode("./staves").InnerXml) == 1;


        // DEBUG
        Debug.Log("(MSMUtil) BPM is " + BPM 
                    + ", beats per measure is " + beatsPerMeasure 
                    + ", beat duration is " + beatDuration 
                    + ", and single staff is " + isSingleStaff);


        /* BODY BLOCK PROCESSING */
        Debug.Log("(MSMUtil) XML MUSIC BODY PROCESSSING");
        // inject n=beatsPerMeasure startup beats of beat-duration note length (rest notes with metronome sound)
        for (int i=0; i<beatsPerMeasure; i++)
        {
            Note startupNote = new(NoteType.Rest, beatDuration, false, NoteLocation.Lane1);
            notes.Add(startupNote);
        }

        // Get every measure of instrument part
        XmlNodeList pianoMeasures = root.SelectNodes($"//part[@id='{instrumentPartID}']/measure");
        foreach (XmlNode measure in pianoMeasures)
        {
            // Get every note of measure
            XmlNodeList measureNotes = measure.SelectNodes("./note");
            foreach (XmlNode note in measureNotes)
            {
                // In each note must:
                // 1. get note length
                //  a. get if dotted
                // 2. determine note projectile type
                // 3. determine note location
                // 4. create and add note to list

                // Get note length
                NoteLength currNoteLen = MxmlLengthToNoteLength(note.SelectSingleNode("./type").InnerXml);
                bool currIsDottedNote = false;
                if (note.SelectSingleNode("./dot") != null)
                {
                    currIsDottedNote = true;
                }

                // Determine note projectile type
                // TODO: idk how we want to automatically determine type,
                // this is a game-feel thing so maybe we reach out to our
                // resident game addicts for advice
                NoteType currNoteType = NoteType.BallProjectileA;

                // Determine note location
                // TODO: too lazy to implement automatic note location
                // placement. for now, just hardcodes lane 0.
                // Eventually, should implement the "Wrap Around" method
                NoteLocation currNoteLoc = NoteLocation.Lane1;

                // Add note to list
                Note currNote = new(currNoteType, currNoteLen, currIsDottedNote, currNoteLoc);
                notes.Add(currNote);

                // Debug.Log("(MSMUtil) Added note of " + currNoteType 
                //             + " with length " + currNoteLen + " and dotted " + currIsDottedNote
                //             + " at " + currNoteLoc);
            }
        }

        // Create music score
        MusicScore msc = new(notes, songDifficulty, BPM, beatDuration);

        return msc;
    }


    // Calculate the time a Note will take to travel from spawn to hitzone
    public static double timeFromSpawnToHitzone(Note songNote, Difficulty diff)
    {
        // TODO: hardcoded base speed 5, maybe use controller instance for the info
        double songNoteSpeed = songNote.Type switch
        {
            (int)NoteType.BallProjectileA => 5, //ballProjectileA.GetComponent<Obstacles>().baseSpeed,
            (int)NoteType.BallProjectileB => 5, //ballProjectileB.GetComponent<Obstacles>().baseSpeed,
            (int)NoteType.WallObstacle => 5, //wallObstacle.GetComponent<Obstacles>().baseSpeed,
            _ => (double)0,
        };

        double noteVelocity = GetDifficultyFactor(diff) * songNoteSpeed;
        if (noteVelocity == 0)
        {
            // Case for rest note, takes 0 additional time to reach hitzone
            return 0;
        }
        return _spawnToZoneDist / noteVelocity;
    }


    // Obtains the specified value for a difficulty enum type
    public static float GetDifficultyFactor(Difficulty diff)
    {
        switch (diff)
        {
            case Difficulty.protege:
                return 1F;
            case Difficulty.concert:
                return 2;
            case Difficulty.virtuoso:
                return 4;
            case Difficulty.prodigy:
                return 6;
            default:
                break;
        }
        return 0;
    }


    /* Private Helper Methods */
    // Translate MusicXML <type> to NoteLength
    private static NoteLength MxmlLengthToNoteLength(string xmlLength)
    {
        return xmlLength switch
        {
            "whole" => NoteLength.Whole,
            "half" => NoteLength.Half,
            "quarter" => NoteLength.Quarter,
            "eighth" => NoteLength.Eighth,
            "16th" => NoteLength.Sixteenth,
            "32nd" => NoteLength.Thirtysecond,
            "64th" => NoteLength.Sixtyfourth,
            "128th" => NoteLength.OneTwentyEighth,
            "256th" => NoteLength.TwoFiftySixth,
            "512th" => NoteLength.FiveTwelfth,
            _ => throw new Exception("Invalid MXML Note Type to Note Length!"),
        };
    }
}


