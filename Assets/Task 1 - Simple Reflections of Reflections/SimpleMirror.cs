using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class implements realtime mirror reflection in a scene
 * using a second camera on the opposite side of the mirror to
 * the source camera.
 * 
 * For simplicity sake, we only support mirror meshes with the Y vector
 * pointing in the direction of the surface normal (e.g. planes), and
 * only mirroring in the shader
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class SimpleMirror : MonoBehaviour
{
    // The renderer of our mirror Object
    public Renderer mirrorRenderer;

    // The source camera which is being reflected
    // This may be the viewing camera in the scene, or the reflection camera 
    // of some other mirror
    public Camera sourceCamera;

    // The camera to use on the other side of our mirror
    public Camera reflectionCamera;

    // The render texture which will be attached to our reflecting camera
    protected RenderTexture reflectionRenderTexture;

    // We initialize our mirror by passing in a reference to the camera
    // we want to reflect - either the viewing camera in the scene, or the
    // reflection camera of some other mirror
    public void Initialize(Camera _sourceCamera)
    {
        // Store a reflect to the camera we will reflect
        sourceCamera = _sourceCamera;

        // Make a copy of the source camera's game object
        GameObject reflectionCameraGO = GameObject.Instantiate(sourceCamera.gameObject);

        // Rename it relevant to our Mirror, and set the mirror as it's parent
        reflectionCameraGO.name = name + " Camera";
        reflectionCameraGO.transform.SetParent(transform, false);

        // Point the camera looking out from the mirror by default
        reflectionCameraGO.transform.rotation = Quaternion.LookRotation(mirrorRenderer.transform.up, mirrorRenderer.transform.forward);

        // Get the camera game object attached to this copied object
        reflectionCamera = reflectionCameraGO.GetComponent<Camera>();

        // Set it's depth to render before the camera we are reflecting
        reflectionCamera.depth = _sourceCamera.depth - 1;

        // Use the source camera's culling mask for the reflected camera as well
        //reflectionCamera.cullingMask = _sourceCamera.cullingMask;
        reflectionCamera.cullingMask = name.EndsWith("2") ? LayerMask.GetMask("Default") : reflectionCamera.cullingMask;

        // Allocate a temporary render texture for our reflection camera
        reflectionRenderTexture = RenderTexture.GetTemporary(512, 512, 24, RenderTextureFormat.ARGB32);

        // Set our reflection camera to render to this rendertexture
        reflectionCamera.targetTexture = reflectionRenderTexture;

        // Set our mirror's material to use the reflection texture as it's main texture
        mirrorRenderer.material.mainTexture = reflectionRenderTexture;
    }

    // On destroy is called when this game object is destroyed or we stop
    // running the application
    private void OnDestroy()
    {
        // If we have allocated a render texture
        if (reflectionRenderTexture != null)
            // Release it
            RenderTexture.ReleaseTemporary(reflectionRenderTexture);
    }

    // Update is called once per frame
    public void UpdateMirror()
    {
        // Get the position of the mirror in world space
        Vector3 pos = mirrorRenderer.transform.position;

        // Get the direction of the normal in world space, depending on which
        // direction we have specified for this mesh
        Vector3 norm = mirrorRenderer.transform.up;

        // Create a 3D Plane using the normal and a point on the plane
        // (Point normal form)
        Plane mirrorPlane = new Plane(norm, pos);

        // We will store the distance from the camera to the mirror
        // in this variable
        float rayDistance;

        // Calculate the position of the camera in world space
        Vector3 cameraPos = sourceCamera.transform.position;

        // Calculate the position of an "up" point for the camera in world space
        // (which is just the world position plus the world up vector)
        Vector3 cameraUp = cameraPos + sourceCamera.transform.up;

        // Calculate the position of an "forward" point for the camera in world space
        // (which is just the world position plus the world forward vector)
        Vector3 cameraForward = cameraPos + sourceCamera.transform.forward;

        // Calculate the world position for the reflected camera
        Vector3 cameraPosReflect = cameraPos;
        // Cast a ray from the camera towards the plane (opposite the plane's normal)
        Ray cameraPosRay = new Ray(cameraPos, -norm);
        // If this ray hits the plane
        if (mirrorPlane.Raycast(cameraPosRay, out rayDistance))
            // Calculate the mirrored camera's position as on this
            // ray, but twice the distance as the mirror intersection
            cameraPosReflect = cameraPosRay.GetPoint(rayDistance * 2);

        // Calculate the world "up" position for the reflected camera
        Vector3 cameraUpReflect = cameraUp;
        // Cast a ray from the camera's "up" towards the plane (opposite the plane's normal)
        Ray cameraUpRay = new Ray(cameraUp, -norm);
        // If this ray hits the plane
        if (mirrorPlane.Raycast(cameraUpRay, out rayDistance))
            // Calculate the mirrored camera's "up" position as on this
            // ray, but twice the distance as the mirror intersection
            cameraUpReflect = cameraUpRay.GetPoint(rayDistance * 2);

        // Calculate the world "forward" position for the reflected camera
        Vector3 cameraForwardReflect = cameraForward;
        // Cast a ray from the camera's "forward" towards the plane (opposite the plane's normal)
        Ray cameraForwardRay = new Ray(cameraForward, -norm);
        // If this ray hits the plane
        if (mirrorPlane.Raycast(cameraForwardRay, out rayDistance))
            // Calculate the mirrored camera's "forward" position as on this
            // ray, but twice the distance as the mirror intersection
            cameraForwardReflect = cameraForwardRay.GetPoint(rayDistance * 2);

        // Set the reflection camera's position to be the reflected world position
        reflectionCamera.transform.position = cameraPosReflect;
        // Set the reflection camera's rotation to be a Quaternion looking from
        // the reflected camera's position in the direction of the reflected camera's
        // "forward" position, with the up direction being in the reflected camera's
        // "up" position minus the reflected camera's position
        reflectionCamera.transform.rotation = Quaternion.LookRotation(cameraForwardReflect - cameraPosReflect, cameraUpReflect - cameraPosReflect);

        // We will render from the reflection camera manually
        reflectionCamera.enabled = false;

        // Set the reflection cameras projection matrix to be the source
        // Camera's projection matrix mirrored in the X dimension
        reflectionCamera.projectionMatrix = sourceCamera.projectionMatrix * Matrix4x4.Scale(new Vector3(-1, 1, 1));
    }

    public void Render()
    {
        // Every camera has to mirror the camera it's reflecting
        // This means for every second camera we need to invert back face culling
        // Easiest way to check that is see if our projection matrix has had
        // it's x axis mirrored, and if so, invert culling
        bool revertBackFacing = (reflectionCamera.projectionMatrix.m00 < 0);

        // Invert backface culling
        GL.invertCulling = revertBackFacing;

        // Render the view from our reflection camera
        reflectionCamera.Render();

        // Set backface culling back to off
        GL.invertCulling = !revertBackFacing;

        
    }

}
