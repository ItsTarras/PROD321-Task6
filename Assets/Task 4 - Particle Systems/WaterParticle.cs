using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class encapsulates a "water" particle for a particle
 * system.
 *
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class WaterParticle : MonoBehaviour
{

    // The number of sprites in the sprite sheet for this particle
    public Vector2Int TileCount = new Vector2Int(5, 5);

    // Whether the sprite sheet goes top to bottom, rather than bottom to top
    public bool flipY = true;



    // The current sprite index
    public int frameIdx = 0;

    // The mesh render for this particle
    public MeshRenderer meshRenderer;

    // The time we last updated the mesh renderers sprite
    float timeLastUpdated = 0;

    // The amount of time to display each sprite
    public float timePerFrame = .1f;

    // The current sprite offset in the texture
    public Vector2 offset;

    // The scene camera so we can billboard this sprite
    Camera _camera;

    // The direction our particles will travel
    public Vector3 Direction = Vector3.up;

    public Vector3 Acceleration = Vector3.zero;


    // Start is called before the first frame update
    void Start()
    {
        
        // Update our material
        UpdateMaterial();

        // Store the main camera in our camera variable
        _camera = Camera.main;
    }

    // Update Material is called when we are updating the material's sprite
    void UpdateMaterial()
    {
        // Get the x and y indices for our current sprite index
        int x = frameIdx % TileCount.x;
        int y = frameIdx / TileCount.y;

        // Determine the normalized position in the sprite map
        offset = new Vector2((float)x / (float)TileCount.x, (float)y / (float)TileCount.y);

        // If we have gone off the end of the sprite map, destroy our particle
        if (offset.y >= 1) GameObject.Destroy(gameObject);

        // If we're flipping the Y offset, subtract the value from .8f
        if (flipY) offset.y = .8f - offset.y;

        // Set the mesh renderers material's texture offset to the offset we've
        // defined
        if (meshRenderer !=null && meshRenderer.material!=null && meshRenderer.material.mainTexture != null)
            meshRenderer.material.mainTextureOffset = offset;

        // Update the frame index
        frameIdx++;

        // Update the time last updated
        timeLastUpdated = Time.realtimeSinceStartup;

    }

    // Update is called once per frame
    void Update()
    {
        // If the time since we last changed sprite is longer the the time
        // we are waiting between sprite swaps
        if (Time.realtimeSinceStartup - timeLastUpdated > timePerFrame)
            // Update the material
            UpdateMaterial();

        // Rotate the particle to always face the camera and share the camera's
        // up direction for billboarding
       // transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position, _camera.transform.up);

        //TODO: Integrate the effects of acceleration on our particle
        Direction += Acceleration * Time.deltaTime;
        


        // Move the particle in the defined direction based on the time elapsed
        transform.position += Direction * Time.deltaTime;
    }
}
