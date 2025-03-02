using UnityEngine;
using System.Collections;

public class SimpleFlash : MonoBehaviour {
    
    [Tooltip("Material to switch to during the flash.")]
    [SerializeField] private Material flashMaterial;

    [Tooltip("Duration of the flash.")]
    [SerializeField] private float duration;


    private MeshRenderer meshRenderer;
    private Material originalMaterial;
    private Coroutine flashRoutine;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        originalMaterial = meshRenderer.material;
    }

    public void Flash()
    {
        if(flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        meshRenderer.material = flashMaterial;

        yield return new WaitForSeconds(duration);

        meshRenderer.material = originalMaterial;

        flashRoutine = null;
    }
}