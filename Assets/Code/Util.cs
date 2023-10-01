using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


// This is the class holding every util method for other scripts to use.
// Make sure methods here are for general purpose that might be needed for plural scripts.
public class Util
{
    // Usage: To create a delay in execution some codes.
    // Input: time -- The delay time (in seconds);
    //        action -- Actual codes/methods hope to be executed after the delay.
    public static IEnumerator WaitForSec(float time, Action action = null)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }

    

    // Usage:To change an obj's position from one to another.
    // Input: obj -- Gameobject to be moved or to change the collision layer;
    //        target -- Gameobject holding the target position info.
    //        layer -- The name of the target collision layer.
    // Todo: Modify to become more generalized.
    public static void Move(GameObject obj, GameObject target, string layer = null)
    {
        obj.transform.position = target.transform.position;
        return;
    }

    // Usage:To change an obj's velocity. (Rigidbody 2D required)
    // Input: rb -- Target rigidbody2D
    //        speed -- Vector2 object representing the target speed;
    public static void SetSpeed(Rigidbody2D rb, Vector2 speed)
    {
        rb.velocity = speed;
    }

    // Usage:To change an obj's layer from one to another.
    // Input: tar -- Gameobject to change the collision layer;
    //        layername -- The name of the target collision layer.
    public static void SetLayer(GameObject tar, string layername)
    {
        tar.layer = LayerMask.NameToLayer(layername);
    }
}
