using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class implements realtime mirror reflection in a scene
 * using a second camera on the opposite side of the mirror to
 * the source camera. This is an advancement over the earlier example
 * as it uses frustums to determine what each mirror reflects. It inherits
 * all the base mirror behaviour from Simple Mirror, but adds Frustrums
 * to determine visibility
  * 
 * For simplicity sake, we only support mirror meshes with the Y vector
 * pointing in the direction of the surface normal (e.g. planes), and
 * only mirroring in the shader
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2023, University of Canterbury
 * Written by Adrian Clark
 */

public class FlexibleMirror: SimpleMirror
{
    // Store the minimum and maximum bounding vertices
    // for our mirror to create a frustum
    public Vector3 minBound { get; private set; }
    public Vector3 maxBound { get; private set; }

    // The frustum for this mirror's camera
    public Frustum cameraFrustum;

    // The GameObject which will contain the frustum for this mirror's camera
    GameObject cameraFrustumGO;

    // The material we will use for the frustum
    Material frustumMaterial;

    // The colour of this frustum
    public Color frustumColour;

    // We initialize our mirror by passing in a reference to the camera
    // we want to reflect - either the viewing camera in the scene, or the
    // reflection camera of some other mirror
    public void Initialize(Camera _sourceCamera, Transform frustumContainer, Material frustumMaterial, int frustumLayer)
    {
        // Use the Simple Camera's Initialize function to set up our reflection camera
        base.Initialize(_sourceCamera);

        /*****
         * The frustum functionality to determine whether any other mirrors are visible in this cameras view
         *****/

        // Store the frustum material we are using
        this.frustumMaterial = frustumMaterial;

        // Get the mesh filter which defines our mirror, and store it's bounding vertices
        MeshFilter meshFilter = mirrorRenderer.gameObject.GetComponent<MeshFilter>();
        minBound = meshFilter.mesh.bounds.min;
        maxBound = meshFilter.mesh.bounds.max;

        // Create a new GameObject for our camera frustum
        cameraFrustumGO = new GameObject(name + " Frustum");
        // Set it's parent to our frustum container (we will manually update this frustum)
        cameraFrustumGO.transform.SetParent(frustumContainer, false);
        // Set it's layer to the frustum layer so we can make it invisible to the cameras
        cameraFrustumGO.layer = frustumLayer;

        // Create the frustum for this camera
        cameraFrustum = Frustum.CreateFrustumFromMirror(this, _sourceCamera, cameraFrustumGO, frustumMaterial, frustumColour);

    }

    // UpdateFrustum is used to reupdate the frustum for this mirror
    // Because the reflecting camera can change, it's easier to just regenerate
    // A completely new frustum
    public void UpdateFrustum()
    {
        // Create the frustum for this camera
        cameraFrustum = Frustum.CreateFrustumFromMirror(this, reflectionCamera, cameraFrustumGO, frustumMaterial, frustumColour);

        // Update our new frustum
        cameraFrustum.UpdateFrustum();
    }

   
    // This function clears our render texture
    public void ClearRenderTexture()
    {
        // Store the current render texture
        RenderTexture current = RenderTexture.active;

        // Load the render texture that the camera is rendering too
        RenderTexture.active = reflectionRenderTexture;

        // Clear the render texture
        GL.Clear(true, true, Color.clear);

        // Restore the previously active render texture
        RenderTexture.active = current;

    }
}
