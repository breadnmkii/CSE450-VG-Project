using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* Docs:
 * Goal of this class is to provide an OOP oriented method
 * of organizing our "songs" into actual game elements.
 * 
 * This class includes:
 *   baseDuration       --> base duration of song to indicate length of level (in frames?)
 *   BPM (song speed)   --> speed of background scroll and obstacle coming at player.
 *                          also defines a constant reference unit for when notes
 *                          (obstacles) will be played (spawned)
 *                          
 *                          i.e. BPM = 100 --> spawn a projectiles at a rate of every
 *                                             1000 frames * 1/BPM = 10 frames
 *                                             
 *                          BPM will also calculate the length of the actual song and
 *                          ensure length is evenly divisible by BPM, such as:
 *                          
 *                          i.e. SongDuration = Math.floor(BaseDuration / BPM) frames
 *                          
 *   instrument         --> which voice/instrument of song should the player be focused
 *                          on. i.e. drums or melody is being "played" on screen?
 *   notes              --> The types, number, and physical location of obstacles to be
 *                          spawned in level
 *   staff              --> The temporal location of notes (when will they be spawmed?)
 *   
 *   
 * The class should also be (maybe) responsible for:
 *   Obstacle lifetime (spawning and destroying outside of camera frame)
 *   
 *    
 * Exposing necessary song properties to other code
 *   - Duration in case other devs need to know how long level is
 *   - BPM for setting speed of other unrelated elements
 *      
 * 
 * 
 */

public class SceneMusicScore : MonoBehaviour
{
    // Private members for internal song properties
    private readonly int _songDuration;
    private readonly int _currentNote;   // index pointing to current note in staff
    private readonly List<Tuple<NoteType, NotePitch>> notes;
    private readonly List<int> staff;
    

    // Public members for song properties (readonly)
    public readonly int baseDuration;    // Num of frames
    public readonly int BPM;             // beats per min

    // Constructor
    SceneMusicScore(int baseDuration, int BPM, List<Tuple<NoteType, NotePitch>> notes, List<int> staff)
    {
        this.baseDuration = baseDuration;
        this.BPM = BPM;
        
        // Set private members
        _songDuration = baseDuration/BPM;
        this.notes = notes;
        this.staff = staff;

        // TODO: some sanity checking in making sure songDuration
        // can accomodate enough notes and staff is viable
        if (staff.Min() < 0 || staff.Max() > _songDuration)
        {
            throw new Exception("Incompatible staff and song duration!");
        }

        if (BPM < 0)
        {
            throw new Exception("BPM cannot be negative!");
        }
    }
                    



    // Update is called once per frame
    void Update()
    {
        spawnNote();
    }

    /* Object lifetime management functions */
    void spawnNote()
    {
        // Reads staff and select notes
        // TODO: figure out how to integrate staff and notes to spawn object
    }

    void cleanupNotes()
    {
        // Determines if any notes are outside camera view
        // and deletes
        // TODO: figure out...
    }

    
}
