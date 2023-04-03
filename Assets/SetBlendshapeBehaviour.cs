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
    List<int> blendshapesWeight = new List<int>() { 97, 30, 71, 50, 7, 5, 78, 12, 36, 8, 4, 3, 10, 5, 6, 2, 22, 20, 4, 14, 4, 38, 4, 17, 13, 2, 19, 3, 2, 11, 3, 29, 2, 10, 5, 26, 21, 21, 19, 14, 70, 23, 17, 10, 20, 9, 72, 8, };
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
