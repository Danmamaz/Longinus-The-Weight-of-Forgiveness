using UnityEngine;
using UnityEngine.UI;

public class DeathScreenAnimatorBridge : MonoBehaviour
{
    [Range(0f, 1f)]
    [Tooltip("Keyframe this variable in the Animation Window")]
    public float revealProgress = 0f;
    [SerializeField] Image deathTextImage;

    private Material deathMaterial;

    void Awake()
    {
        if (deathTextImage.material != null)
        {
            deathTextImage.material = new Material(deathTextImage.material);
            deathMaterial = deathTextImage.material;
            
            deathMaterial.SetFloat("_RevealProgress", 0f);
        }
    }

    void Update()
    {
        if (deathMaterial != null)
        {
            deathMaterial.SetFloat("_RevealProgress", revealProgress);
        }
    }
}