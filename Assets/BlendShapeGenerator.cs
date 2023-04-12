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
public class BlendShapeGenerator : MonoBehaviour
{
    int blendShapeCount;

    SkinnedMeshRenderer skinnedMeshRenderer;
    Mesh skinnedMesh;
    float blendStep = 1.0f;
    bool blendFinished = false;
    int i = 0;
    List<int> blendshapes_i = new List<int>();
    float blendWeight = 0f;
    string textFile = "blendshapes_name.csv";
    FileStream file;

    void Awake()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        skinnedMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
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
        // If exist, truncate the file.
        // Otherwise, create a file.
        if (!File.Exists(textFile))
        {
            File.Create(textFile);
        }
        file = new FileStream(textFile, FileMode.Truncate, FileAccess.Write);
        // Write the CSV header
        string header = "index, weight, filename\n";
        byte[] bytes = Encoding.UTF8.GetBytes(header);
        file.Write(bytes, 0, bytes.Length);
    }

    /*
    Loop through a set of blendshapes, and update the blendshape weight,
    with step = blendStep.

    Assume we are using the RocketBox contributed by Microsoft.
    blendShapeCount = 175.
    */
    void Update()
    {
        if (blendFinished)
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
            // skinnedMeshRenderer.SetBlendShapeWeight(blendshapes_i[i], blendWeight);
            // string fileName = string.Format("index{0}-weight{1:C0}.png", blendshapes_i[i], blendWeight);
            // ScreenCapture.CaptureScreenshot(fileName);
            // string filePath = Path.GetFullPath(fileName);
            // string line = string.Format("{0}, {1}, {2}\n", blendshapes_i[i], blendWeight, filePath);
            // byte[] bytes = Encoding.UTF8.GetBytes(line);
            // file.Write(bytes, 0, bytes.Length);
            // blendWeight += blendStep;
            // Debug.Log("Save to " + filePath);
            string blendShapeName = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(blendshapes_i[i]);
            string line = string.Format("{0}, {1}\n", blendshapes_i[i], blendShapeName);
            byte[] bytes = Encoding.UTF8.GetBytes(line);
            file.Write(bytes, 0, bytes.Length);
            i++;
        }
        else
        {
            blendFinished = true;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    void OnDestroy()
    {
        file.Close();
    }

}
