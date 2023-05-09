using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;


public class SetBlendshapeBehaviour : MonoBehaviour
{
    SkinnedMeshRenderer skinnedMeshRenderer;
    Mesh skinnedMesh;
    List<int> blendshapes_i = new List<int>();
    List<int> blendshapesWeight = new List<int>() { 1, 62, 0, 16, 31, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 61, 0, 0, 0, 0, 31, 0, 0, 0, 0, 0, 0, 0, 0, 0, 81, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 13, 0, 13, 12 };
    int blendShapeCount;

    void Awake()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        skinnedMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
    }

    // Start is called before the first frame update
    void Start()
    {
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
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < blendshapes_i.Count; i++)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(blendshapes_i[i], blendshapesWeight[i]);
        }
    }
}
