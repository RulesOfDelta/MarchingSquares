using System;
using UnityEngine;
using System.Collections.Generic;

public class ScalarField : MonoBehaviour
{
    public float scale;
    public int repeat = 0;

    [Range(0.0f, 2.0f)] public float SurfaceValue = 0.5f;

    private int pixelWidth = 1024;
    private int pixelHeight = 1024;

    private float xOrigin = 0;
    private float yOrigin = 0;

    private int amountOfScalars = 30;

    public Texture2D _noiseTexture;
    private Color[] _pixels;
    private MeshRenderer _renderer;

    private List<GameObject> _scalarSpheres = new List<GameObject>();

    // Start is called before the first frame update
    private void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        //_noiseTexture = new Texture2D(pixelWidth, pixelHeight);
        _pixels = new Color[pixelWidth * pixelHeight];
        //_renderer.material.mainTexture = _noiseTexture;
        //AssembleNoiseTexture();
        AssembleScalarField();
    }

    private void AssembleNoiseTexture()
    {
        for (float y = 0; y < _noiseTexture.height; y++)
        {
            for (float x = 0; x < _noiseTexture.width; x++)
            {
                float xCoord = (x / _noiseTexture.width) * scale;
                float yCoord = (y / _noiseTexture.height) * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                _pixels[(int) x + (int) y * _noiseTexture.width] = new Color(sample, sample, sample);
            }
        }

        _noiseTexture.SetPixels(_pixels);
        _noiseTexture.Apply();
    }

    private float SampleTexture(Vector3 sampleCoordinates)
    {
        //Factor is: f(x)= 1 / (x - 1)
        float multiplicationFactor = 1.0f / (amountOfScalars - 1.0f);
        Vector3 normalizedSampleCoordinates = sampleCoordinates * multiplicationFactor;

        Debug.Log($"The normalized vector is {normalizedSampleCoordinates}");

        //float xyColorComponent = _pixels[(int) ((normalizedYValue * _noiseTexture.height) * (normalizedXValue * _noiseTexture.width))].maxColorComponent;
        //float xzColorComponent = _pixels[(int) ((normalizedZValue * _noiseTexture.height) * normalizedXValue * _noiseTexture.width)].maxColorComponent;

        float xzColorComponent = _noiseTexture.GetPixel((int) (normalizedSampleCoordinates.x * _noiseTexture.width), (int) (normalizedSampleCoordinates.z * _noiseTexture.height)).maxColorComponent;
        float xyColorComponent = _noiseTexture.GetPixel((int) (normalizedSampleCoordinates.x * _noiseTexture.width), (int) (normalizedSampleCoordinates.y * _noiseTexture.height)).maxColorComponent;
        float yzColorComponent = _noiseTexture.GetPixel((int) (normalizedSampleCoordinates.z * _noiseTexture.width), (int) (normalizedSampleCoordinates.y * _noiseTexture.height)).maxColorComponent;
        return (xyColorComponent + xzColorComponent + yzColorComponent) / 3.0f;
    }

    // This is a size 256 hash lookup table with randomly arranged numbers between 0-255.
    // This is the permutation table Ken Perlin used in his original implementation.
    private static readonly int[] HashTable = { 151,160,137,91,90,15,					
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,	
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
    };

    private static int[] DoubledHashTable;

    private static void DoublePermutationArray()
    {
        //This function simply doubles the existing array above, to avoid to prevent an IndexOutOfRangeException
        DoubledHashTable = new int[512];
        for (int i = 0; i < 512; i++)
        {
            DoubledHashTable[i] = HashTable[i % 256];
        }
    }

    private static float Fade(float t)
    {
        // This is the Fade function Ken Perlin used in his original implementation of the algorithm
        // It eases the coordinate values so that they ease towards the integral values
        // The function is: f(t) = 6t^5 - 15t^4 + 10t^3
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static int Increment(int number)
    {
        //Repeat here
        return ++number;
    }

    private static float GradientVector(int hash, float x, float y, float z)
    {
        float gradient = 0;
        
        switch (hash & 0xF)
        {
            case 0x0: gradient = x + y; break;
            case 0x1: gradient = -x + y; break;
            case 0x2: gradient = x - y; break;
            case 0x3: gradient = -x -y; break;
            case 0x4: gradient = x + z; break;
            case 0x5: gradient = -x + z; break;
            case 0x6: gradient = x - z; break;
            case 0x7: gradient = -x - z; break;
            case 0x8: gradient = y + z; break;
            case 0x9: gradient = -y + z; break;
            case 0xA: gradient = y - z; break;
            case 0xB: gradient = -y -z; break;
            case 0xC: gradient = y + x; break;
            case 0xD: gradient = -y + z; break;
            case 0xE: gradient = y - x; break;
            case 0xF: gradient = -y - z; break;
            default: Debug.LogError("Gradient vector went into default: forbidden!");
                break;
        }

        return gradient;
    }
    
    
    private float PerlinNoise3D(Vector3 coordinates)
    {
        //Summary
        // This algorithm takes a 3D vector as input and returns a scalar between 0.0 and 1.0.
        // It's used as input to assemble the scalar field, which is later used as input for the marching cubes algorithm.
        // The implementation used is based on the "Improved Perlin Noise" algorithm published on SIGGRAPH in 2002 by its inventor Ken Perlin.

        // 1. Double the permutation array to prevent IndexOutOfRangeExceptions
        // This is only done once
        if(DoubledHashTable == null) DoublePermutationArray();
        
        // 2. Find the position of the coordinates within the unit cube
        if(repeat > 0) coordinates = coordinates.Mod(repeat);

        // 3. The variables xCube, yCube and zCube determine, in which unit cube the coordinate lies
        // Furthermore, it limits the value to 255 inclusive to prevent indexes, that are out of range
        int xCube = (int) coordinates.x & 255;
        int yCube = (int) coordinates.y & 255;
        int zCube = (int) coordinates.z & 255;

        // 4. The variables xInner, yInner and zInner determine the position of the coordinates within the unit cube
        float xInner = coordinates.x - (int) coordinates.x;
        float yInner = coordinates.y - (int) coordinates.y;
        float zInner = coordinates.z - (int) coordinates.z;
        
        // 5. Here the Fade function is used to ease the values, to create smoother results
        float u = Fade(xInner);
        float v = Fade(yInner);
        float w = Fade(zInner);
        
        // This is the perlin noise hash function
        // It's used to get a unique value for every coordinate
        // A hash function, is, according to Wikipedia: 
        //
        // "A hash function is any function that can be used to map data of arbitrary size to fixed-size values.
        // The values returned by a hash function are called hash values, hash codes, digests, or simply hashes.
        // The values are usually used to index a fixed-size table called a hash table."
        
        // 6. Use the Perlin noise hash function to retrieve a integer value from the DoubledHashTable array
        // This value is then used to generate the gradient vectors
        // This is done for all 8 cube points
        int aaa = DoubledHashTable[DoubledHashTable[DoubledHashTable[xCube] + yCube] + zCube];
        int aba = DoubledHashTable[DoubledHashTable[DoubledHashTable[xCube] + Increment(yCube)] + zCube];
        int aab = DoubledHashTable[DoubledHashTable[DoubledHashTable[xCube] + yCube] + Increment(zCube)];
        int abb = DoubledHashTable[DoubledHashTable[DoubledHashTable[xCube] + Increment(yCube)] + Increment(zCube)];
        int baa = DoubledHashTable[DoubledHashTable[DoubledHashTable[Increment(xCube)] + yCube] + zCube];
        int bba = DoubledHashTable[DoubledHashTable[DoubledHashTable[Increment(xCube)] + Increment(yCube)] + zCube];
        int bab = DoubledHashTable[DoubledHashTable[DoubledHashTable[Increment(xCube)] + yCube] + Increment(zCube)];
        int bbb = DoubledHashTable[DoubledHashTable[DoubledHashTable[Increment(xCube)] + Increment(yCube)] + Increment(zCube)];
        
        
        // This takes the integer value from for all 8 corners, that I retrieved beforehand, to then use the
        // Gradient function to calculate the dot product between the gradient and distance vector and then interpolate the values
        float x1 = Mathf.Lerp(GradientVector(aaa, xInner, yInner, zInner), GradientVector(baa, xInner - 1, yInner, zInner), u);
        float x2 = Mathf.Lerp(GradientVector(aba, xInner, yInner - 1, zInner), GradientVector(bba, xInner - 1, yInner - 1, zInner), u);
        float y1 = Mathf.Lerp(x1, x2, v);

        //Same happens here just with the 4 other corners
        x1 = Mathf.Lerp(GradientVector(aab, xInner, yInner, zInner - 1), GradientVector(bab, xInner - 1, yInner, zInner - 1), u);
        x2 = Mathf.Lerp(GradientVector(abb, xInner, yInner - 1, zInner - 1), GradientVector(bbb, xInner - 1, yInner - 1, zInner - 1), u);
        float y2 = Mathf.Lerp(x1, x2, v);

        //Finally the two interpolated values are interpolated again using the z-axis value to obtain the final value
        return Mathf.Lerp(y1, y2, w);
    }
    
    private void AssembleScalarField()
    {
        //Get the local scale of the cube that I use the create the scalar field
        Vector3 currentLocalScale = transform.localScale;
        Vector3 startingPosition = transform.position - currentLocalScale / 2;

        //Calculate the size of a step for all dimensions
        float vertexStepSizeX = currentLocalScale.x / (amountOfScalars - 1);
        float vertexStepSizeY = currentLocalScale.y / (amountOfScalars - 1);
        float vertexStepSizeZ = currentLocalScale.z / (amountOfScalars - 1);

        //Create variables to save the current value
        float currentX = 0;
        float currentY = 0;
        float currentZ = 0;

        // Resetting all the values seems cumbersome,
        // but using floats as iteration variables and having
        // rounding errors is even more cumbersome
        
        for (int x = 0; x < amountOfScalars; x++)
        {
            for (int y = 0; y < amountOfScalars; y++)
            {
                for (int z = 0; z < amountOfScalars; z++)
                {
                    //Create primitive
                    GameObject newGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //Set position
                    newGameObject.transform.position = startingPosition + new Vector3(currentX, currentY, currentZ);
                    //Create a material whose 'blackness' represents the surface value
                    Material newMaterial = new Material(Shader.Find("HDRP/Lit"));
                    float test = PerlinNoise3D(newGameObject.transform.position);
                    newMaterial.color = Color.green * test;
                    newGameObject.GetComponent<MeshRenderer>().sharedMaterial = newMaterial;
                    //Destroy if surface value is bigger
                    if (newMaterial.color.g < SurfaceValue) Destroy(newGameObject);
                    //Otherwise add it to the list
                    else _scalarSpheres.Add(newGameObject);
                    currentZ += vertexStepSizeZ;
                }

                currentZ = 0;
                currentY += vertexStepSizeY;
            }

            currentY = 0;
            currentX += vertexStepSizeX;
        }
    }

    public void RebakeScalarField()
    {
        
        //Destroys all the GameObjects and executes 'AssembleScalarField' again
        foreach (var currentSphere in _scalarSpheres)
        {
            Destroy(currentSphere);
        }

        _scalarSpheres.Clear();
        AssembleScalarField();
    }
}

public static class VectorExtensions
{
    public static Vector3 Mod (this Vector3 vector, float modulo)
    {
        //This extension method takes the vector's components, uses modulo and returns the new vector
        //Had to it this way since C# currently does not support extension operator overloading
        var modVector = new Vector3 {x = vector.x % modulo, y = vector.y % modulo, z = vector.z % modulo};
        return modVector;
    }
}