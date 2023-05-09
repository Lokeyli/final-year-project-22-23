using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchAvatarBehaviour : MonoBehaviour
{

    public GameObject[] avatarArr;
    int currentIdx = 0;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(avatarArr[currentIdx].name);
        avatarArr[currentIdx].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        bool isFinished = checkIsFinished(avatarArr[currentIdx]);
        // Debug.Log(isFinished);
        if (isFinished)
        {
            Switch();
        }
    }

    private void Switch()
    {
        Debug.Log(currentIdx);
        Debug.Log(avatarArr.Length - 1);
        if (currentIdx >= avatarArr.Length - 1)
        {
#if UNITY_EDITOR
            Debug.Log("Finish iterating all the avatars");
            UnityEditor.EditorApplication.isPlaying = false;
            return;
#endif            
        }
        Debug.Log("Switching avatars");
        copyTransform(avatarArr[currentIdx], avatarArr[currentIdx + 1]);
        adjustCameraPositition(avatarArr[currentIdx + 1]);
        // Debug.Log(height);
        avatarArr[currentIdx].SetActive(false);
        currentIdx++;
        avatarArr[currentIdx].SetActive(true);
    }

    private bool checkIsFinished(GameObject Avatar)
    {
        // Find the child where the blendshape controller is.
        int childIdx = 1;
        GameObject blendshapeController = Avatar.transform.GetChild(childIdx).gameObject;
        // Check if it is finished
        return blendshapeController.GetComponent<BlendShapeGenerator>().isFinished;
    }

    private void copyTransform(GameObject oldAvatar, GameObject newAvatar)
    {
        Transform oldTransform = oldAvatar.transform;
        newAvatar.transform.position = oldTransform.position;
        newAvatar.transform.rotation = oldTransform.transform.rotation;
        newAvatar.transform.localScale = oldTransform.transform.localScale;
    }

    private void adjustCameraPositition(GameObject newAvatar)
    {
        // Get the height of the new avatar,
        // we need to adjust the camera base on this.
        // Reference: https://stackoverflow.com/questions/54699670/unity-width-and-height-of-the-gameobject     
        SkinnedMeshRenderer render = avatarArr[currentIdx + 1].GetComponentInChildren<SkinnedMeshRenderer>();
        Vector3 size = render.bounds.size;
        float height = size[1];
        // https://docs.unity3d.com/ScriptReference/Camera-main.html
        Camera m_camera = Camera.main;
        Vector3 currentPos = m_camera.transform.position;
        double new_y = height * (6.75 / 7.5);
        m_camera.transform.position = new Vector3(currentPos.x, (float)new_y, currentPos.z);
    }

}
