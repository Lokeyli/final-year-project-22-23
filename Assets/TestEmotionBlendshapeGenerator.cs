using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;


public class TestEmotionBlendshapeGenerator : MonoBehaviour
{
    int blendShapeCount;

    SkinnedMeshRenderer skinnedMeshRenderer;
    Mesh skinnedMesh;
    // Because this is a test set, so we make the step larger
    float blendStep = 5.0f;
    public bool isFinished = false;
    int i = 0;
    List<int> blendshapes_i = new List<int>();
    float blendWeight = 0f;
    string avatar_name;
    string textFile = "emotion_blendshapes.csv";
    FileStream file;

    List<int[]> emotionAUs = new List<int[]>();

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

        // Add emotion combinations to the emotionAUs
        // 6 + 12
        emotionAUs.Add(new int[] { 8, 15 });
        // 1 + 4 + 15
        emotionAUs.Add(new int[] { 0, 4, 22 });
        // 1 + 2 + 5 + 26
        emotionAUs.Add(new int[] { 0, 2, 7, 31 });
        // 1 + 2 + 4 + 5 + 7 + 20 + 26
        emotionAUs.Add(new int[] { 0, 2, 4, 7, 11, 26, 31 });
        // 4 + 5 + 7 + 23
        emotionAUs.Add(new int[] { 4, 7, 11, 28 });
        // 9 + 15 + 16
        emotionAUs.Add(new int[] { 12, 22, 23 });
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
            blendWeight = 0f;
            var cur_emotion = emotionAUs[i];
            foreach (var idx in cur_emotion)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(blendshapes_i[idx], blendWeight);
            }
            i++;
        }
        // Save the screenshot to file with filename.
        // Save the filename to a txt for other pipeline to use.
        if (i < emotionAUs.Count)
        {
            var cur_emotion = emotionAUs[i];
            Debug.Log(cur_emotion);
            foreach (var idx in cur_emotion)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(blendshapes_i[idx], blendWeight);
            }
            string fileName = string.Format("{0}-emotion{1}-weight{2:f0}.png", avatar_name, i, blendWeight);
            ScreenCapture.CaptureScreenshot(fileName);
            string filePath = Path.GetFullPath(fileName);
            // We need this index 
            // because we might need to train a single model for each blendshape
            string line = string.Format("{0}, {1}, {2}\n", string.Join("/", cur_emotion), blendWeight, filePath);
            Debug.Log(line);
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
