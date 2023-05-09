using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

/*
References
File IO: https://learn.microsoft.com/en-us/dotnet/api/system.io.file?view=net-7.0
*/
public class TestBlendShapeGenerator : MonoBehaviour
{
    int blendShapeCount;

    SkinnedMeshRenderer skinnedMeshRenderer;
    Mesh skinnedMesh;
    float blendStep = 1.0f;
    public bool isFinished = false;
    int i = 0;
    List<int> blendshapes_i = new List<int>();
    float blendWeight = 0f;
    string avatar_name;
    string textFile = "test_blendshapes.csv";
    FileStream file;

    void Awake()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        skinnedMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
        // Assuming the avatar structure follows the initial import setting.
        // (Fe)male_xxxx_yy_facial
        //   |- Bip01
        //   |- xxx_hipoly_81_bones_opacity  < This script is here
        avatar_name = transform.parent.name;
    }

    void Start()
    {
        // Use the FACS blendshapes supported by RocketBox.
        // Which is called AU, short for Action Unit, in this lib.
        Regex FACSRegex = new Regex(".*AU.*");
        blendShapeCount = skinnedMesh.blendShapeCount;
        for (int i = 0; i < blendShapeCount; i++)
        {
            string blendShapeName = skinnedMesh.GetBlendShapeName(i);
            if (FACSRegex.IsMatch(blendShapeName))
            {
                blendshapes_i.Add(i);
            }
        }

        // Create a file to store the blendshape information.
        // If exist, append the file.
        // Otherwise, create a file, and write the CSV header.
        if (!File.Exists(textFile))
        {
            file = new FileStream(textFile, FileMode.CreateNew, FileAccess.Write);
            string header = "index, weight, filename\n";
            byte[] bytes = Encoding.UTF8.GetBytes(header);
            file.Write(bytes, 0, bytes.Length);
        }
        else
        {
            file = new FileStream(textFile, FileMode.Append, FileAccess.Write);
        }
    }

    /*
    Loop through a set of blendshapes, and update the blendshape weight,
    with step = blendStep.

    Assume we are using the RocketBox contributed by Microsoft.
    blendShapeCount = 175.
    */
    void Update()
    {
        if (isFinished)
        {
            return;
        }
        // Reset blendWeight when one blendshape is finished,
        // and move to next blendshape.
        if (blendWeight > 100f)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(blendshapes_i[i], 0f);
            blendWeight = 0f;
            i++;
        }
        // Save the screenshot to file with filename.
        // Save the filename to a txt for other pipeline to use.
        if (i < blendshapes_i.Count)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(blendshapes_i[i], blendWeight);
            string fileName = string.Format("{0}-index{1}-weight{2:f0}.png", avatar_name, blendshapes_i[i], blendWeight);
            ScreenCapture.CaptureScreenshot(fileName);
            string filePath = Path.GetFullPath(fileName);
            // We need this index 
            // because we might need to train a single model for each blendshape 
            string line = string.Format("{0}, {1}, {2}\n", blendshapes_i[i], blendWeight, filePath);
            byte[] bytes = Encoding.UTF8.GetBytes(line);
            file.Write(bytes, 0, bytes.Length);
            blendWeight += blendStep;
            Debug.Log("Save to " + filePath);
        }
        else
        {
            file.Close();
            isFinished = true;
        }
    }

    void OnDestroy()
    {
        file.Close();
    }

}
