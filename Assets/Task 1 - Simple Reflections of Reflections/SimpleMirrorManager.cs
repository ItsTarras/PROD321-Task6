using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class is responsible for managing all the mirrors in our scene.
 * It's not very realistic because each mirror only reflects one other camera
 * in a predefined order (top left mirror reflects viewing camera, top right
 * mirror reflects top left mirror, bottom right mirror reflects top right
 * mirror). Although this is not very realistic, it does make the code much
 * easier. 
 * 
 * Your job in this task is to implement the code required to initialize, update
 * and render the mirrors in the correct order.  
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2023, University of Canterbury
 * Written by Adrian Clark
 */

public class SimpleMirrorManager : MonoBehaviour
{
    // The viewing camera in our scene
    public Camera mainCamera;

    // The list of all the mirrors in our scene
    public List<SimpleMirror> mirrors;

    // Start is called before the first frame update
    void Awake()
    {
        /*****
         * TODO: Initialize all our cameras, using the last camera in the chain
         * i.e. mirror[0] initialize with maincamera
         * mirror[1] initialize with mirror[0].reflectionCamera
         * mirror[2] initialize with mirror[1].reflectionCamera
         * ...
         * Each Mirror has an "Initialize" function which takes the camera it is
         * to reflect, you should call this function in order
         *****/
        if (mirrors.Count > 0)
        {
            //First camera is initialised with the main camera.
            SimpleMirror mirror = mirrors[0];
            mirror.Initialize(mainCamera);

            //Starting from the second element in the list, initialise the mirrors.
            if (mirrors.Count >= 1)
            {
                for (int i = 1; i < mirrors.Count; i++)
                {
                    mirrors[i].Initialize(mirrors[i - 1].reflectionCamera);
                }
            }
        }
        
        

    }

    // Update is called once per frame
    void LateUpdate()
    {
        /*****
         * TODO: Loop through all the mirrors and update their reflected camera,
         * starting from mirror[0] and going to mirror[n]
         * 
         * Each Mirror has a "UpdateMirror" function which will update the reflected
         * cameras position and orientation
         *****/
        foreach (SimpleMirror mirror in mirrors)
        {
            mirror.UpdateMirror();
        }
        


        /*****
         * TODO: Loop through all the mirrors in reverse order and render 
         * the view from that mirror, i.e. from mirror[n] to mirror[0]
         * 
         * Each Mirror has a "Render" function which will force its camera to render
         *****/
        for (int i = mirrors.Count - 1; i >= 0; i--)
        {
            mirrors[i].Render();
        }

    }
}
