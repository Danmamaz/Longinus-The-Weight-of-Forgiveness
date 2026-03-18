using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIUnscaledTimeProvider : MonoBehaviour
{
    private Material _materialInstance;
    private int _unscaledTimeID;

    private void Start()
    {
        _unscaledTimeID = Shader.PropertyToID("_UnscaledTime");
        Graphic graphicComponent = GetComponent<Graphic>();
        
        if (graphicComponent != null && graphicComponent.material != null)
        {
            _materialInstance = new Material(graphicComponent.material);
            
            graphicComponent.material = _materialInstance;
        }
    }

    private void Update()
    {
        if (_materialInstance != null)
        {
            _materialInstance.SetFloat(_unscaledTimeID, Time.unscaledTime);
        }
    }

    private void OnDestroy()
    {
        if (_materialInstance != null)
        {
            Destroy(_materialInstance);
        }
    }
}