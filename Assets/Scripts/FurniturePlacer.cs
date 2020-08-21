using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FurniturePlacer : MonoBehaviour
{
    public Transform placementIndicator;
    public GameObject selectionUI;

    private List<GameObject> furnitureObjectsInScene = new List<GameObject>();
    private GameObject currentlySelectedFurniture;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        selectionUI.SetActive(false);
    }

    void Update()
    {
        // first frame we touch the screen
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
        {
            // create a ray from where we're touching on the screen
            Ray ray = cam.ScreenPointToRay(Input.touches[0].position);
            RaycastHit hit;

            // shoot the raycast
            if (Physics.Raycast(ray, out hit))
            {
                // did we hit something?
                if (hit.collider.gameObject != null && furnitureObjectsInScene.Contains(hit.collider.gameObject))
                {
                    // select the touching object
                    if (currentlySelectedFurniture != null && hit.collider.gameObject != currentlySelectedFurniture)
                        Select(hit.collider.gameObject);
                    else if (currentlySelectedFurniture == null)
                        Select(hit.collider.gameObject);
                }
            }
            else
                Deselect();
        }

        if (currentlySelectedFurniture != null && Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Moved)
            MoveSelected();
    }

    void MoveSelected()
    {
        Vector3 curPos = cam.ScreenToViewportPoint(Input.touches[0].position);
        Vector3 lastPos = cam.ScreenToViewportPoint(Input.touches[0].position - Input.touches[0].deltaPosition);

        Vector3 touchDir = curPos - lastPos;

        Vector3 camRight = cam.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        currentlySelectedFurniture.transform.position += (camRight * touchDir.x + camForward * touchDir.y);
    }

    // called when we select a furnitureObjectsInScene piece
    void Select(GameObject selected)
    {
        if (currentlySelectedFurniture != null)
            ToggleSelectionVisual(currentlySelectedFurniture, false);

        currentlySelectedFurniture = selected;
        ToggleSelectionVisual(currentlySelectedFurniture, true);
        selectionUI.SetActive(true);
    }

    // called when we deselect a furnitureObjectsInScene piece
    void Deselect()
    {
        if (currentlySelectedFurniture != null)
            ToggleSelectionVisual(currentlySelectedFurniture, false);

        currentlySelectedFurniture = null;
        selectionUI.SetActive(false);
    }

    // called when we select/deselect a furnitureObjectsInScene piece
    void ToggleSelectionVisual(GameObject obj, bool toggle)
    {
        obj.transform.Find("Selected").gameObject.SetActive(toggle);
    }

    // called when we press the a furnitureObjectsInScene button - creates a new piece of furnitureObjectsInScene
    public void PlaceFurniture(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, placementIndicator.position, Quaternion.identity);
        furnitureObjectsInScene.Add(obj);
        Select(obj);
    }

    public void ScaleSelected(float rate)
    {
        currentlySelectedFurniture.transform.localScale += Vector3.one * rate;
    }

    public void RotateSelected(float rate)
    {
        currentlySelectedFurniture.transform.eulerAngles += Vector3.up * rate;
    }

    public void SetColor(Image buttonImage)
    {
        MeshRenderer[] meshRenderers = currentlySelectedFurniture.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer mr in meshRenderers)
        {
            if (mr.gameObject.name == "Selected")
                continue;

            mr.material.color = buttonImage.color;
        }
    }

    public void DeleteSelected()
    {
        furnitureObjectsInScene.Remove(currentlySelectedFurniture);
        Destroy(currentlySelectedFurniture);
        Deselect();
    }
}