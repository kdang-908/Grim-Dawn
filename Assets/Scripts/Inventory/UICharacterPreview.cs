using UnityEngine;

public class UICharacterPreview : MonoBehaviour
{
    public Camera previewCam;
    public Transform previewRoot;
    public GameObject characterPrefab;
    public string previewLayerName = "UIPreview";
    public float rotateSpeed = 180f;

    GameObject current;
    int previewLayer;

    void Start()
    {
        previewLayer = LayerMask.NameToLayer(previewLayerName);
        if (previewLayer < 0)
        {
            Debug.LogError("Layer UIPreview not found");
            return;
        }

        if (previewRoot == null)
            previewRoot = transform;

        SpawnCharacter();
    }

    void Update()
    {
        if (current == null) return;

        if (Input.GetMouseButton(0))
        {
            float mx = Input.GetAxis("Mouse X");
            current.transform.Rotate(Vector3.up, -mx * rotateSpeed * Time.deltaTime, Space.World);
        }
    }

    void SpawnCharacter()
    {
        if (characterPrefab == null)
        {
            Debug.LogError("Character Prefab not assigned!");
            return;
        }

        foreach (Transform c in previewRoot)
            Destroy(c.gameObject);

        current = Instantiate(characterPrefab, previewRoot);
        current.name = "Preview_Remey";

        current.transform.localPosition = Vector3.zero;
        current.transform.localRotation = Quaternion.Euler(0, 180, 0);
        current.transform.localScale = Vector3.one;

        SetLayerRecursively(current, previewLayer);

        // Camera nhìn vào thân trên
        previewCam.transform.position = previewRoot.position + new Vector3(0, 2.4f, -3f);
        previewCam.transform.LookAt(previewRoot.position + new Vector3(0, 1.8f, 0));
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform c in obj.transform)
            SetLayerRecursively(c.gameObject, layer);
    }
}
