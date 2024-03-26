using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* This class implements two different antialiasing methods:
 * Super Sampled Antialiasing (SSAA), where the scene is rendered at a higher 
 * resolution than the screen and then downsampled, and
 * Multisampled Antialiasing (MSAA), where for each pixel multiple subsamples 
 * of the shape are taken and the colours averaged together to calculate the 
 * final colour for that pixel.
 * 
 * The implementations aren't really doing true antialiasing, as we are actually
 * sampling a higher resolution render of the final image, but it makes it a
 * bit easier to show what's actually happening this way. Full Res Image is
 * the image rendered at 500x500 resolution, while Sampled Image is the full
 * res image sampled at 50x50 resolution - which is an approximation of our
 * aliased image. The functions will also render boxes on the Full Res Image
 * to indicate the size of the pixels in the Sampled Image, and where any
 * sampling has taken place
 * 
 * You will be required to fill out the stub functions which implement 
 * SSAA and MSAA. All the functionality for multi-sampling and image resizing 
 * is provided, but you will need to use this functionality appropriately to 
 * anti alias the final image result.
 *
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2023, University of Canterbury
 * Written by Adrian Clark
 */

public class AntiAliasing : MonoBehaviour
{
    [Header("Image Containers")]
    // The size of the full res texture
    public Vector2Int fullResTextureSize = new Vector2Int(500, 500);

    // The UI Container which displays the full res image and how sampling
    // has happened
    public RawImage fullResImage;

    // The size of the sampled (texture to anti-alias) texture
    public Vector2Int sampledTextureSize = new Vector2Int(50, 50);

    // The UI container which displays the sampled (texture to anti-alias) texture
    public RawImage sampledImage;

    [Header("Triangle Details")]
    // The positions of the three vertices which make up the triangle
    // in view port coordinates (going from 0,0 to 1, 1)
    public Vector2 v1 = new Vector2(.1f, .4f);
    public Vector2 v2 = new Vector2(.6f, .1f);
    public Vector2 v3 = new Vector2(.9f, .9f);

    // The colours for the three vertices which make up the triangle
    public Color v1Colour = Color.red;
    public Color v2Colour = Color.green;
    public Color v3Colour = Color.blue;

    // An enum for the different AA Types we support
    public enum AAType
    {
        NoAA,
        MSAA,
        SSAA
    };

    // The current Anti-Aliasing Type used
    [Header("Anti-Aliasing Settings")]
    public AAType AntiAliasingType = AAType.NoAA;

    // The location of where to sample for NoAA
    public Vector2[] NoAASamplePoint = { new Vector2(0.5f, 0.5f) };

    // The locations of the points to sample for MSAA
    public Vector2[] MSAASamplePoints = {
            new Vector2(0.3f, 0.1f),
            new Vector2(0.9f, 0.3f),
            new Vector2(0.7f, 0.9f),
            new Vector2(0.1f, 0.7f)
        };

    // The scale of the rendered image for SSAA
    public int SSAAScale = 2;

    void Update()
    {
        // Create our full res texture
        Texture2D fullResTexture = InterpolateTriangleFullRes(fullResTextureSize);

        // Select the Antialiasing method to use: No Antialiasing, MSAA and SSAA
        Texture2D sampledTexture = null;
        switch (AntiAliasingType)
        {
            case AAType.NoAA:
                sampledTexture = SampleTextureNoAA(ref fullResTexture, sampledTextureSize);
                break;

            case AAType.MSAA:
                sampledTexture = SampleTextureMSAA(ref fullResTexture, sampledTextureSize);
                break;

            case AAType.SSAA:
                sampledTexture = SampleTextureSSAA(ref fullResTexture, sampledTextureSize, SSAAScale);
                break;
        }

        // Apply pixel changes to the full res texture and set it to the rawimage
        fullResTexture.Apply();
        fullResImage.texture = fullResTexture;

        // Set the sampled texture to point filter mode so the UI doesn't blur things when resizing
        sampledTexture.filterMode = FilterMode.Point;
        // Apply pixel changes to the sampled texture and set it to the rawimage
        sampledTexture.Apply();
        sampledImage.texture = sampledTexture;
    }

    // This function samples our input image by sampling the pixel in the
    // middle of each "sampling rect" (using the SampleTexture function)
    Texture2D SampleTextureNoAA(ref Texture2D fullResTexture, Vector2Int sampledTextureSize)
    {
        // Create a new sampled texture of the correct size
        Texture2D SampledTexture = new Texture2D(sampledTextureSize.x, sampledTextureSize.y);

        // Calculate the number of pixels which will be sampled for each pixel
        Vector2Int pixelsPerSample = new Vector2Int(fullResTexture.width / sampledTextureSize.x, fullResTexture.height / sampledTextureSize.y);

        // Loop through each sampling rect in the Y and X dimensions
        for (int y = 0; y < sampledTextureSize.y; y++)
        {
            for (int x = 0; x < sampledTextureSize.x; x++)
            {
                // Create the rect for this sample in the source texture
                RectInt sampleRect = new RectInt(x * pixelsPerSample.x, y * pixelsPerSample.y, pixelsPerSample.x, pixelsPerSample.y);

                // Set the resulting pixels in the output based on the single
                // sample taken from the current sampling rectangle
                SampledTexture.SetPixel(x, y, SampleTexture(ref fullResTexture, sampleRect, NoAASamplePoint));
            }
        }

        // Return the sampled texture
        return SampledTexture;
    }

    // This function implements Multisampled Antialiasing (MSAA), which
    // takes multiple sampled for inside each sampling rect
    Texture2D SampleTextureMSAA(ref Texture2D fullResTexture, Vector2Int sampledTextureSize)
    {
        // Create a new sampled texture of the correct size
        Texture2D SampledTexture = new Texture2D(sampledTextureSize.x, sampledTextureSize.y);
        
        
        Vector2Int pixelsPerSample = new Vector2Int(fullResTexture.width / sampledTextureSize.x, fullResTexture.height / sampledTextureSize.y);

        for (int y = 0; y < sampledTextureSize.y; y++)
        {
            for(int x = 0; x < sampledTextureSize.x; x++)
            {
                //Check multiple rectangle sizes?
                // Create the rect for this sample in the source texture
                RectInt sampleRect = new RectInt(x * pixelsPerSample.x, y * pixelsPerSample.y, pixelsPerSample.x, pixelsPerSample.y);

                // Set the resulting pixels in the output based on the single
                // sample taken from the current sampling rectangle
                SampledTexture.SetPixel(x, y, SampleTexture(ref fullResTexture, sampleRect, MSAASamplePoints));
            }
        }

        /******
         * TODO: Implement MSAA anti-aliasing
         * Look at how NoAA is implemented with its single sample point
         * and then think about what you would need to do to sample multiple points
         ******/

        // Return the sampled texture
        return SampledTexture;
    }

    // This function implements Super Sampled Antialiasing (SSAA), which
    // samples the original texture at a higher resolution, and then downscales
    // it to the original texture size
    Texture2D SampleTextureSSAA(ref Texture2D fullResTexture, Vector2Int sampledTextureSize, int scale)
    {
        // Create a new sampled texture of the correct size
        Texture2D SampledTexture = new Texture2D(sampledTextureSize.x, sampledTextureSize.y);

        Texture2D noAATexture = SampleTextureNoAA(ref fullResTexture, sampledTextureSize * scale);

        DownScaleImage(noAATexture, SampledTexture);


        /******
         * TODO: Implement SSAA anti-aliasing
         * SSAA will sample for a higher resolution image than our output (sampledTextureSize)
         * we define this resolution using a "scale" which we can multiply this by.
         * 
         * The actual sampling is the same process as NoAA (and in fact you should call this function)
         * 
         * After you're sampled the higher resolution texture, you can use teh DownScaleImage function
         * to return the correct size sampled texture
         ******/

        // Return the sampled texture
        return SampledTexture;
    }

    // This function takes colour samples within a sample rect, and averages the colour out
    // It also updates the fullResTexture texture to show how where the samples were taken from
    Color SampleTexture(ref Texture2D fullResTexture, RectInt SampleRect, Vector2[] samplePoints)
    {
        // Start with the colour black
        Color colour = Color.black;

        // Loop over each sample to test
        foreach (Vector2 samplePoint in samplePoints)
            // Sample the colour at that point, and add the colour to the pixel colour
            colour += GetSampledColour(ref fullResTexture, SampleRect, samplePoint);

        //When we're done, divide by the number of samples to determine the final colour
        colour /= samplePoints.Length;

        // Draw a rectangle around our sample rect so we can see how the pixels are sampled.
        // Loop over the pixels on the top and bottom of the sample rect
        // and colour them in black
        for (int y = SampleRect.yMin; y < SampleRect.yMax; y++)
        {
            fullResTexture.SetPixel(SampleRect.xMin, y, Color.black);
            fullResTexture.SetPixel(SampleRect.xMax, y, Color.black);
        }

        // Loop over the pixels on the left and right of the sample rect
        // and colour them in black
        for (int x = SampleRect.xMin; x < SampleRect.xMax; x++)
        {
            fullResTexture.SetPixel(x, SampleRect.yMin, Color.black);
            fullResTexture.SetPixel(x, SampleRect.yMax, Color.black);
        }

        // Return the computed colour
        return colour;
    }
        
    // Get the colour within a sample rect based on the sample point, and update
    // the sampled texture to show where we took the sample from
    Color GetSampledColour(ref Texture2D fullResTexture, RectInt sampleRect, Vector2 samplePoint)
    {
        // Calculate where the sample will be relative to the original source
        // texture in pixel space
        float pointX = (float)sampleRect.x + (sampleRect.width * samplePoint.x);
        float pointY = (float)sampleRect.y + (sampleRect.height * samplePoint.y);

        // Calculate the normalized point relative to the sourceTexture size
        float normalizedX = pointX / fullResTexture.width;
        float normalizedY = pointY / fullResTexture.height;

        Color colour = fullResTexture.GetPixelBilinear(normalizedX, normalizedY);

        // Draw a red box on the sampled texture based on where we take the
        // sample from
        for (int y = Mathf.FloorToInt(pointY) - 1; y < Mathf.CeilToInt(pointY) + 1; y++)
            for (int x = Mathf.FloorToInt(pointX) - 1; x < Mathf.CeilToInt(pointX) + 1; x++)
                fullResTexture.SetPixel(x, y, Color.red);

        // return the pixel colour from the source texture
        return colour;
    }

    // This function downscales an image stored in input texture, into the texture
    // stored in output texture, using the scale of the output texture
    void DownScaleImage(Texture2D inputTexture, Texture2D outputTexture)
    {
        // Calculate the scale we need to use to resize the input texture
        // to the output texture
        int xScale = inputTexture.width / outputTexture.width;
        int yScale = inputTexture.height / outputTexture.height;

        // Loop over each pixel in the output texture
        for (int outY = 0; outY < outputTexture.height; outY++)
        {
            for (int outX = 0; outX < outputTexture.width; outX++)
            {
                // We will do the resize by just average the all the pixels
                // in the input image that should go into a single pixel in
                // the output image. To calculate the average, start with the
                // sum colour (which we will set to black as this is 0,0,0)
                Color colourSum = Color.black;

                // Loop over the block of pixels in the input image which will
                // shrink down to the single pixel in the output image we're
                // currently dealing with
                for (int inY = outY * yScale; inY < (outY + 1) * yScale; inY++)
                {
                    for (int inX = outX * xScale; inX < (outX + 1) * xScale; inX++)
                    {
                        // Add these pixels to the sum of pixel colours
                        colourSum += inputTexture.GetPixel(inX, inY);
                    }
                }

                // Once we've summed up all the pixels in the input image which
                // we be downscaled to a single pixel in the output image, set
                // that pixel in the output image to the sum of colours / the
                // number of pixels - i.e. the average pixel colour in that block
                outputTexture.SetPixel(outX, outY, colourSum / (xScale * yScale));
            }
        }

        // Apply the changed pixels to the output texture
        outputTexture.Apply();
    }

    // This function generates our full res texture of a triangle
    Texture2D InterpolateTriangleFullRes(Vector2Int fullResTextureSize)
    {
        Texture2D fullResTexture = new Texture2D(fullResTextureSize.x, fullResTextureSize.y);

        // Calculate the position of the vertices in texture space (so the
        // actual pixel positions)
        Vector2 tex_v1 = new Vector2(v1.x * (float)fullResTextureSize.x, v1.y * (float)fullResTextureSize.y);
        Vector2 tex_v2 = new Vector2(v2.x * (float)fullResTextureSize.x, v2.y * (float)fullResTextureSize.y);
        Vector2 tex_v3 = new Vector2(v3.x * (float)fullResTextureSize.x, v3.y * (float)fullResTextureSize.y);


        // Calculating the minimum bounding rect (in pixel space) that our
        // Triangle fits into
        Rect triangleBoundingRect = new Rect();
        triangleBoundingRect.xMin = Mathf.Min(tex_v1.x, Mathf.Min(tex_v2.x, tex_v3.x));
        triangleBoundingRect.xMax = Mathf.Max(tex_v1.x, Mathf.Max(tex_v2.x, tex_v3.x));
        triangleBoundingRect.yMin = Mathf.Min(tex_v1.y, Mathf.Min(tex_v2.y, tex_v3.y));
        triangleBoundingRect.yMax = Mathf.Max(tex_v1.y, Mathf.Max(tex_v2.y, tex_v3.y));

        // Loop over our framebuffer
        for (int y = 0; y < fullResTextureSize.y; y++)
        {
            for (int x = 0; x < fullResTextureSize.x; x++)
            {
                // By default set the pixel to white
                Color pixelColour = Color.white;

                // Create a vector2 from our current pixel position
                Vector2 p = new Vector2(x, y);

                // If the bounding rect of the triangle contains the current pixel
                if (triangleBoundingRect.Contains(p))
                {

                    // Calculate the weights w1, w2 and w3 for the barycentric
                    // coordinates based on the positions of the three vertices
                    float denom = (tex_v2.y - tex_v3.y) * (tex_v1.x - tex_v3.x) + (tex_v3.x - tex_v2.x) * (tex_v1.y - tex_v3.y);
                    float w_v1 = ((tex_v2.y - tex_v3.y) * (p.x - tex_v3.x) + (tex_v3.x - tex_v2.x) * (p.y - tex_v3.y)) / denom;
                    float w_v2 = ((tex_v3.y - tex_v1.y) * (p.x - tex_v3.x) + (tex_v1.x - tex_v3.x) * (p.y - tex_v3.y)) / denom;
                    float w_v3 = 1 - w_v1 - w_v2;

                    // If w1, w2 and w3 are >= 0, we are inside the triangle (or
                    // on an edge, but either way, render the pixel)
                    if (w_v1 >= 0 && w_v2 >= 0 && w_v3 >= 0)
                    {
                        // Calculate the pixel colour based on the weighted vertex colours
                        pixelColour = v1Colour * w_v1 + v2Colour * w_v2 + v3Colour * w_v3;
                    }

                }

                // Set this pixel in the render texture
                fullResTexture.SetPixel(x, y, pixelColour);
            }
        }

        return fullResTexture;
    }
}
