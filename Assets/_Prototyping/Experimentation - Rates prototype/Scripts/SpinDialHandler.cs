using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpinDialHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetMouseButton(0))
            return;

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);

        foreach (RaycastResult thingHit in raycastResultList)
        {
            if (thingHit.gameObject.GetComponent<SpinnableDial>() != null)
                thingHit.gameObject.GetComponent<SpinnableDial>().MouseIsOverAndDown();
        }
    }
}
