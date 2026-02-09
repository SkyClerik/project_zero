//#define THIS_DEBUG
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UserInterfaceRaycaster : MonoBehaviour
{
    private PointerEventData _pointer;
    private List<RaycastResult> _resultRaycast;

    private void Awake()
    {
        _pointer = new PointerEventData(EventSystem.current);
        _resultRaycast = new List<RaycastResult>();
    }

    public bool IsPickingMode
    {
        get
        {
            _pointer.position = Input.mousePosition;
            _resultRaycast.Clear();

            // ловим клик когда "picking mode" в позиции "position"
            EventSystem.current.RaycastAll(_pointer, _resultRaycast);

#if THIS_DEBUG
            foreach (var rayResult in _resultRaycast)
                Debug.Log($"name: {rayResult.gameObject.name}");
#endif

            if (_resultRaycast.Count > 0)
                return true;

            return false;
        }
    }
}
