using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FishingController : MonoBehaviour
{
    public BaitCasting bait;

    public float minPower = 2f;
    public float maxPower = 10f;
    public float chargeSpeed = 5f;

    public bool onCooldown {get; set;}

    public Animator anim;

    void Start()
    {
        onCooldown = false;
    }

    public void StartCooldown()
    {
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        onCooldown = true;
        anim.SetBool("isCasting", false);
        yield return new WaitForSeconds(1f);
        onCooldown = false;
    }
}