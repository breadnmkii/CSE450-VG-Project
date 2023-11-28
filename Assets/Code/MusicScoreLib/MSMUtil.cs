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
    


    /***** Private Methods ***************************************************/



    /***** Public Members ***************************************************/
    // Outlets
    public GameObject[] lanes;


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
        XmlNode songDirection = root.SelectSingleNode($"//metronome");

        if (songDirection.SelectSingleNode("./per-minute") != null)
        {
            BPM = int.Parse(songDirection.SelectSingleNode("./per-minute").InnerXml);
        }
        else
        {
            throw new Exception("(MSMUtil) Did not process BPM from score! Please enter manually");
        }
        
        int beatsPerMeasure     = int.Parse(songMetadata.SelectSingleNode("./time/beats").InnerXml);
        NoteLength beatDuration = (NoteLength)int.Parse(songMetadata.SelectSingleNode("./time/beat-type").InnerXml);
        bool isMultiStaff = false;
        if (songMetadata.SelectSingleNode("./staves") != null)
        {
            isMultiStaff = int.Parse(songMetadata.SelectSingleNode("./staves").InnerXml) != 1;
        }

        // DEBUG
        Debug.Log("(MSMUtil) BPM is " + BPM 
                    + ", beats per measure is " + beatsPerMeasure 
                    + ", beat duration is " + beatDuration 
                    + ", and is multi staff?: " + isMultiStaff);

        /* BODY BLOCK PROCESSING */
        Debug.Log("(MSMUtil) XML MUSIC BODY PROCESSSING");
        // TODO: inject n=beatsPerMeasure startup beats of beat-duration note length (rest notes with metronome sound)
        /*
        for (int i=0; i<beatsPerMeasure; i++)
        {
            Note startupNote = new(NoteType.Rest, beatDuration, false, NoteLocation.Lane1);
            notes.Add(startupNote);
        }
        */

        /* Automatic beatmapping property variables */
        //bool noteAtkType = false;       // "Alternate note type" method
        int noteAtkTypeCounter_max = 4;     // Maximum number of bits in counter (n = 3 bits)
        int noteAtkTypeCounter_mid = 2;            // Middle threshold value for switching between note atk type
        int noteAtkTypeCounter = noteAtkTypeCounter_mid;             // "Random n-bit Saturating counter" method
        bool inTiedGroup = false;   // Flag to indicate whether current group of notes are tied
        bool inTupletGroup = false;     // Flag to indicate whether current group is triplet
        bool firstRestOfMeasureSeen = false;    // Flag to limit "seen" (spawned) rest note powerups to one per
                                                // measure (if they occur)
        int measureCount = 0;

        // Get every measure of instrument part
        XmlNodeList measures = root.SelectNodes($"//part[@id='{instrumentPartID}']/measure");
        foreach (XmlNode measure in measures)
        {
            measureCount++;
            Debug.Log("Processing measure " + measureCount);
            firstRestOfMeasureSeen = false;
            string lastNotePitch = "";      // String containing "<octave-num><note-letter>" pitch of last note
                                            //  to determine if rise or fall in pitch
            int lastNoteLane = 0;           // In conjunction with `lastNotePitch` to determine to shift lane
                                            //  up or down

             // "Same random lane" note location method
            if (MusicScoreManager.difficulty != Difficulty.prodigy)
            {
                lastNoteLane = UnityEngine.Random.Range(0, 4);
            }
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

                /* Note Metadata Parse */
                // Only take notes on voice 1
                if (note.SelectSingleNode("./voice").InnerXml != "1")
                {
                    // Debug.Log("(MSMUtil) Skip non-primary voice note");
                    continue;
                }
                // If is a chord note, ignore
                if (note.SelectSingleNode("./chord") != null)
                {
                    // Debug.Log("(MSMUtil) Skipping chord note...");
                    continue;
                }

                // If is non-primary staff note, ignore
                if (isMultiStaff && note.SelectSingleNode("./staff").InnerXml != "1")
                {
                    // Debug.Log("(MSMUtil) Skipping non-primary staff note...");
                    continue;
                }

                // If is grace note, ignore
                if (note.SelectSingleNode("./grace") != null)
                {
                    // Debug.Log("(MSMUtil) Skipping grace note...");
                    continue;
                }

                // Check if dotted
                bool currIsDottedNote = false;
                if (note.SelectSingleNode("./dot") != null)
                {
                    currIsDottedNote = true;
                }

                // Check if tied/slur note
                bool currIsTiedNote = false;
                if (inTiedGroup)
                {
                    currIsTiedNote = true;
                }
                // Note that a note can have both end and start a new tie,
                // so "stop" has lower priority over a "start" tie
                if (note.SelectSingleNode("./tie[@type='stop']") != null ||
                    note.SelectSingleNode("./slur[@type='stop']") != null)
                {
                    inTiedGroup = false;
                }
                if (note.SelectSingleNode("./tie[@type='start']") != null ||
                    note.SelectSingleNode("./slur[@type='start']") != null)
                {
                    inTiedGroup = true;
                }

                // Check if tuplet group
                bool currIsTupletNote = false;
                if (inTupletGroup)
                {
                    currIsTupletNote = true;
                }
                // If <tuplet> type is 'open', beginning triplet group. Else, 'stop' is last triplet
                if (note.SelectSingleNode("./notations/tuplet[@type='start']") != null)
                {
                    inTupletGroup = true;
                    currIsTupletNote = true;
                }
                else if (note.SelectSingleNode("./notations/tuplet[@type='stop']") != null) {
                    inTupletGroup = false;
                }


                /* Note Type Parse */
                NoteType currNoteType;
                if (note.SelectSingleNode("./rest") != null)
                {
                    if (!firstRestOfMeasureSeen)
                    {
                        // Set as rest note
                        currNoteType = NoteType.Rest;
                        firstRestOfMeasureSeen = true;
                    }
                    else
                    {
                        // Disregard further rest ntoes
                        currNoteType = NoteType.Null;
                    }
                }
                else
                {
                    // TODO: idk how we want to automatically determine type,
                    // this is a game-feel thing so maybe we reach out to our
                    // resident game addicts for advice
                    // TODO: hardcoding all notes as ATK_A notes, but can alternate later

                    // Update random notetype saturating counter
                    int rand = UnityEngine.Random.Range(0, 2);
                    int change = (rand) * -1 + (1 - rand) * 1;
                    noteAtkTypeCounter += change;
                    //Debug.Log("Random result: " + rand);
                    //Debug.Log("Addition result: " + change);
                    //Debug.Log("Note counter now: " + noteAtkTypeCounter);

                    // Clamping
                    if (noteAtkTypeCounter >= noteAtkTypeCounter_max)
                    {
                        noteAtkTypeCounter = noteAtkTypeCounter_max - 1;
                    }
                    else if (noteAtkTypeCounter < 0)
                    {
                        noteAtkTypeCounter = 0;
                    }

                    // Determine note atk type
                    if (noteAtkTypeCounter <= noteAtkTypeCounter_mid)
                    {
                        currNoteType = NoteType.BallProjectileA;
                    }
                    else
                    {
                        currNoteType = NoteType.BallProjectileB;
                    }
                }
                

                /* Note Length Parse */
                NoteLength currNoteLen;
                if (note.SelectSingleNode("./rest[@measure='yes']") != null)
                {
                    // If rest measure, set length to one measure
                    currNoteLen = (NoteLength)((int)beatDuration / beatsPerMeasure);
                    // Debug.Log("(MSMUtil) Rest measure read as " + currNoteLen + " length rest note");
                }
                else
                {
                    // If is a triplet, set to triplet
                    if (currIsTupletNote)
                    {
                        currNoteLen = NoteLength.Triplet;
                    }
                    else
                    {
                        currNoteLen = MxmlLengthToNoteLength(note.SelectSingleNode("./type").InnerXml);
                    }
                }


                /* Note Location Parse */
                // "Wrap Around" method (only on prodigy)
                // Get current note pitch as string
                if (MusicScoreManager.difficulty == Difficulty.prodigy && 
                    note.SelectSingleNode("./pitch") != null)
                {

                    string currNotePitch = note.SelectSingleNode("./pitch").InnerText;

                    if (SumStringASCII(currNotePitch) > SumStringASCII(lastNotePitch))
                    {
                        lastNoteLane = (lastNoteLane + 1) % 4; // HARDCODE: 4 lanes
                        // Debug.Log("Up pitch at lane " + lastNoteLane);
                    }
                    else if (SumStringASCII(currNotePitch) < SumStringASCII(lastNotePitch))
                    {
                        --lastNoteLane;
                        if (lastNoteLane < 0)
                        {
                            lastNoteLane = 3;
                        }
                        // Debug.Log("Down pitch at lane " + lastNoteLane);
                    }
                    lastNotePitch = currNotePitch;      // update last note pitch
                }
                // Else, simple random lane location
                NoteLocation currNoteLoc = (NoteLocation)lastNoteLane;

                // Add note to list
                Note currNote = new(currNoteType, currNoteLen, currIsDottedNote, currIsTiedNote, currNoteLoc);
                notes.Add(currNote);
            }
        }

        // Create music score
        MusicScore msc = new(notes, songDifficulty, BPM, beatDuration);

        return msc;
    }


    // Calculate the time a Note will take to travel from spawn to hitzone
    // Note: distance is a parameter supplied as the distance the note must travel
    public static double TimeForNoteToTravelDistance(Note songNote, Difficulty diff, double distance)
    {
        // TODO: hardcoded base speed 5, maybe use controller instance for the info
        double songNoteSpeed = songNote.Type switch
        {
            NoteType.BallProjectileA => 5, //ballProjectileA.GetComponent<Obstacles>().baseSpeed,
            NoteType.BallProjectileB => 5, //ballProjectileB.GetComponent<Obstacles>().baseSpeed,
            _ => (double)0,
        };

        double noteSpeed = GetDifficultyFactor(diff) * songNoteSpeed;
        if (noteSpeed == 0)
        {
            // Case for rest note, takes 0 additional time to reach hitzone
            return 0;
        }
        return (distance) / noteSpeed;
    }


    // Obtains the specified value for a difficulty enum type
    public static float GetDifficultyFactor(Difficulty diff)
    {
        switch (diff)
        {
            case Difficulty.protege:
                return 0.5F;
            case Difficulty.concert:
                return 1;
            case Difficulty.virtuoso:
                return 2;
            case Difficulty.prodigy:
                return 1;           // difficulty comes from "wrap around" note spawn method
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

    private static int SumStringASCII(string str)
    {
        int sum = 0;
        foreach (char c in str)
        {
            sum *= 10;  // advance base for next char
            sum += (int)c;
        }

        return sum;
    }
}


