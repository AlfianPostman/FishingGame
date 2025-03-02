using UnityEngine;

public class Shake : MonoBehaviour {
    
    Animator anim;

    private void Start() 
    {
        anim = GetComponent<Animator>();   
    }

    public void CamShake()
    {
        anim.SetTrigger("Shake");
    }
}