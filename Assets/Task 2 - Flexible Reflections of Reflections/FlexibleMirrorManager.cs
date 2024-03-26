using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* This class is responsible for managing all the mirrors in our scene.
 * This class supports much more flexible mirrors than Task 5, as it allows 
 * for arbitrary reflections with the cameras. This is accomplished
 * by using frustums to determine which mirror(s) the player can see
 * and in turn which mirrors they reflect, and so on and so on. We then render
 * each mirror in reverse back to the original player camera.
 * 
 * Your job is to implement the missing behaviour to make it all work
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2023, University of Canterbury
 * Written by Adrian Clark
 */

public class FlexibleMirrorManager : MonoBehaviour
{
    // The viewing camera in our scene
    public Camera mainCamera;

    // The list of all the mirrors in our scene
    public List<FlexibleMirror> mirrors;

    // The material we should use for our frustum
    public Material frustumMaterial;

    // This will store our main camera's frustum
    public Frustum mainCameraFrustum;

    // The name of the layer we should put our frustums in
    // so we can make them invisible to the camera
    public string frustumLayerName = "Frustums";

    // The colour we should use for the Main Camera's frustum
    public Color frustumColour;

    // A text box we will write out information about which camera
    // can see what in
    public Text reflectionLabel;

    // The maximum recursion depth to go for reflecting mirrors
    // this will stop the game from freezing if mirrors reflect each other
    public int maxRecursionDepth = 10;


    public GameObject frustumStorer;

    // Start is called before the first frame update
    void Start()
    {
        int frustumLayer = LayerMask.NameToLayer(frustumLayerName);

        // Initialize all our cameras
        for (int i = 0; i < mirrors.Count; i++)
        {
            mirrors[i].Initialize(mainCamera, transform, frustumMaterial, frustumLayer);
        }

        frustumStorer = new GameObject("FrustumStorer");

        frustumStorer.transform.SetParent(mainCamera.transform, false);

        frustumStorer.layer = frustumLayer;

        mainCameraFrustum = Frustum.CreateFrustumFromCamera(mainCamera, frustumStorer, frustumMaterial, frustumColour);

        /***** 
         * TODO: Create a frustum for the player camera. 
         * You should create a new game object to store the frustum, 
         * parent this to the mainCamera transform,
         * set the game object's layer to use the frustum layer, 
         * then call Frustum.CreateFrustumFromCamera to set up the actual Frustum
         * and store this in our mainCameraFrustum variable
         * 
         * Hint: Check the Initialize function in FlexibleMirror.cs to see how
         * Frustums are created for Mirrors
         *****/

    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Clear our reflection label text
        reflectionLabel.text = "";

        /***** 
         * TODO: Update our main camera's frustum
         * 
         * The Frustum class has an UpdateFrustum method you should call
         *****/
        mainCameraFrustum.UpdateFrustum();

        // Loop through every mirror and clear it's render texture
        foreach (FlexibleMirror mirror in mirrors)
            mirror.ClearRenderTexture();

        // Loop through every mirror in the scene
        foreach (FlexibleMirror mirror in mirrors)
        {
            /***** 
             * TODO: Check If the current mirror is in the main camera's view
             * 
             * The Frustum class has a containsMirror method you should call
             *****/

            if (mainCameraFrustum.containsMirror(mirror)) // ?????
            {
                // Update our reflection label text
                reflectionLabel.text += mainCamera.name + " can see " + mirror.reflectionCamera.name + "\n";

                // Set the current mirrors source camera to the main camera
                mirror.sourceCamera = mainCamera;

                RecursivelyDrawMirrors(mirror, maxRecursionDepth /*maxReflections*/);
                /***** 
                 * TODO: Recursively draw the current mirror and the mirrors it reflects
                 * 
                 * This class has a function RecursivelyDrawMirrors which will draw a mirror
                 * and all the mirrors it's reflecting to a depth of maxRecursionDepth reflections
                 *****/
            }
        }

    }

    // Recursively Draw Mirrors is a depth first recursive function which draws mirrors
    // starting from the furthest away and rendering forwards
    void RecursivelyDrawMirrors(FlexibleMirror currentMirror, int depthRemaining)
    {
        /***** 
         * TODO: Update the current mirror's reflected camera position
         * 
         * SimpleMirror (which FlexibleMirror inherits from) has a UpdateMirror
         * function
         *****/
        currentMirror.UpdateMirror();


        /***** 
         * TODO: Update the current mirror's visibility frustum
         * 
         * Flexible Mirror has an UpdateFrustum function 
         *****/

        currentMirror.UpdateFrustum();

        // Store the camera that is used for this mirror at at the moment, as
        // well as the position of the mirror's camera at the start of this method
        // as it's possible this will change throughout the recursion
        Camera originalSourceCamera = currentMirror.sourceCamera;
        Vector3 originalCameraPosition = currentMirror.sourceCamera.transform.position;
        Quaternion originalCameraOrientation = currentMirror.sourceCamera.transform.rotation;

        // Assuming we haven't reached the bottom of our stack
        if (depthRemaining > 0)
        {
            // Loop through all the mirrors in our scene
            foreach (FlexibleMirror mirrorToTest in mirrors)
            {
                // If the mirror we're testing isn't our current mirror
                if (mirrorToTest != currentMirror)
                {
                    /***** 
                     * TODO: Check if the mirror we're testing is within                    
                     * the frustum of our current mirror
                     * 
                     * We can access the frustum of a mirror using its cameraFrustum
                     * public variable, and test visiblity of a mirror in a frustum
                     * using the frustum's containsMirror function
                     *****/
                    if (currentMirror.cameraFrustum.containsMirror(mirrorToTest)) // ????
                    {
                        // Update our reflection label text
                        reflectionLabel.text += currentMirror.name + " can see " + mirrorToTest.reflectionCamera.name + "\n";

                        // Set the mirror to tests's source camera to be the current camera's reflection camera
                        mirrorToTest.sourceCamera = currentMirror.reflectionCamera;

                        /***** 
                         * TODO: Recursively draw the mirror to test and the mirrors it reflects
                         * Do this for a maximum of depthRemaining -1 reflections
                         * 
                         * We can call the RecursivelyDrawMirrors function again with the mirror
                         * to test, and reducing the depth remaining by 1
                         *****/
                        RecursivelyDrawMirrors(mirrorToTest, depthRemaining - 1);

                        // Restore the camera which was used, as well as the
                        // position and orientation of the camera
                        currentMirror.sourceCamera = originalSourceCamera;
                        currentMirror.sourceCamera.transform.position = originalCameraPosition;
                        currentMirror.sourceCamera.transform.rotation = originalCameraOrientation;

                        // Restore the current mirror's frustum
                        currentMirror.UpdateFrustum();
                    }
                }
            }
        }

        // Restore the camera which was used, as well as the
        // position and orientation of the camera
        currentMirror.sourceCamera = originalSourceCamera;
        currentMirror.sourceCamera.transform.position = originalCameraPosition;
        currentMirror.sourceCamera.transform.rotation = originalCameraOrientation;

        /***** 
         * TODO: Finally render the current mirror out
         * 
         * Mirrors have a Render function
         *****/
        currentMirror.Render();
    }
}