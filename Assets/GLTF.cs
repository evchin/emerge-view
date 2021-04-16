using System.Collections;
using UnityEngine;
using Siccity.GLTFUtility;
using UnityEngine.Networking;

public class GLTF : MonoBehaviour
{
    GameObject model;
    string binFilePath;
    string fileLink;
    bool waiting = true;
    bool fileLinkSetUp = false;

    private void Start()
    {
        binFilePath = $"{Application.persistentDataPath}/Files/scene.glb";
        StartCoroutine(GetAllData());
    }

    public void RefreshModel()
    {
        fileLinkSetUp = false;
        Destroy(model);
        StartCoroutine(GetAllData());
    }

    IEnumerator GetAllData()
    {
        yield return GetDataRequest("https://emerge-view.herokuapp.com/most_recent");
        fileLinkSetUp = true;
        yield return GetDataRequest(fileLink);
        yield return LoadModel();
        if (waiting) waiting = false;
    }

    IEnumerator GetDataRequest(string api)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(api))
        {
            if (fileLinkSetUp) req.downloadHandler = new DownloadHandlerFile(binFilePath);

            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
                Debug.LogError($"{req.error}");

            if (!fileLinkSetUp)
            {
                fileLink = req.downloadHandler.text;
                fileLink = fileLink.Substring(1);
                fileLink = fileLink.Remove(fileLink.Length - 2, 2); 
            }
        }
    }

    IEnumerator LoadModel()
    {
        Destroy(model);
        model = Importer.LoadFromFile(binFilePath);
        Vector3 position = transform.position;
        position.z += 0.5f;
        position.y += 1.2f;
        model.transform.position = position;

        // make grabbable
        Rigidbody rigid = model.AddComponent<Rigidbody>();
        rigid.mass = 5;
        // rigid.isKinematic = true;
        // rigid.useGravity = false;

        MeshCollider collider = model.AddComponent<MeshCollider>();
        collider.enabled = true;
        collider.convex = true;
        // Collider[] grabPoints = {collider}; 
        // OVRGrabbable grabbable = model.AddComponent<OVRGrabbable>();
        // grabbable.enabled = true;
        // grabbable.Initialize(grabPoints);

        yield return model;
    }
}