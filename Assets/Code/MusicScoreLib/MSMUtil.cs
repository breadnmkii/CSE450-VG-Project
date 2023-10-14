using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// XML parser
using System.Xml;

public class MSMUtil : MonoBehaviour
{

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
                                # whole, half, quarter, eight, 16th, 32nd, 64th, 128th, 256th
     *      </dot>              # if is a dotted note (length = type * 1.5)
     *      
     * Wrap around:
     * - automatically determine the lane position by wrapping around
     *   note steps up and down (ascending notes of 8 for 4 beat measure
     *   would be two series of four increasing notes per lane)
     */
    public static (Queue<Note>, int) ProcessMusicXML(TextAsset xmlText, string instrumentPartID)
    {
        Queue<Note> queue = new();

        XmlDocument doc = new();
        doc.LoadXml(xmlText.text); // might be super inefficient
        XmlNode root = doc.DocumentElement;

        instrumentPartID = "P1"; // TODO: hardcoded to piano 1 for now

        /* HEAD BLOCK PROCESSING */
        // get first measure metadata
        XmlNode songMetadata = root.SelectSingleNode($"//part[@id='{instrumentPartID}']/measure[@number='1']/attributes");
        int BPM = int.Parse(songMetadata.SelectSingleNode("./direction/direction-type/metronome/per-minute").Value);
        int beatsPerMeasure = int.Parse(songMetadata.SelectSingleNode("./time/beats").Value);
        int beatDuration = int.Parse(songMetadata.SelectSingleNode("./time/beat-type").Value);
        bool isSingleStaff = int.Parse(songMetadata.SelectSingleNode("./staves").Value) == 1;

        /* BODY BLOCK PROCESSING */
        // inject n=beatsPerMeasure startup beats of beat-duration note length (rest notes with metronome sound)
        for (int i=0; i<beatsPerMeasure; i++)
        {
            // TODO: inject rest notes
            Note startupNote = new();
            queue.Enqueue(startupNote);
        }

        // get every measure of instrument part
        XmlNodeList pianoMeasures = root.SelectNodes($"//part[@id='{instrumentPartID}']/measure");
        foreach (XmlNode measure in pianoMeasures)
        {
            // get every note of measure
            XmlNodeList measureNotes = measure.SelectNodes("./note");
            foreach (XmlNode note in measureNotes)
            {
                // In each note must:
                // 1. determine note length
                //  a. consider dotted note length
                //  b. calc absolute time with spawnAdvanceTime based on projectile speed
                // 2. determine lane location
            }
        }

    }
}
