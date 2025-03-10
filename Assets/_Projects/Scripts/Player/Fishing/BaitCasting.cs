﻿using System;
using UnityEngine;

public class BaitCasting : MonoBehaviour
{
    [SerializeField] Animator anim; 
    [SerializeField] Transform startPoint;    
    [SerializeField] Transform targetPoint;    
    [SerializeField] AnimationCurve arcCurve;    
    [SerializeField] float castDuration = 1.5f;
    
    float castTimer = 0f;
    bool isCasting = false;
    [HideInInspector] public bool successfullyLanded = false;
    [HideInInspector] public bool hasLanded = false;

    public void StartCasting(float power)
    {
        startPoint.position = transform.position;
        targetPoint.position = startPoint.position + startPoint.forward * power;

        castTimer = 0f;
        isCasting = true;
        hasLanded = false;
    }

    public void ResetBait()
    {
        transform.position = startPoint.position;

        anim.Play("default");

        castTimer = 0f;
        isCasting = false;
        hasLanded = false;
        successfullyLanded = false;
    }

    void OnBaitLanded()
    {
        // 🔥 Raycast downward to check for water
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.0f))
        {
            if (hit.collider.CompareTag("Water"))
            {
                hasLanded = true;
                successfullyLanded = true;

                LandOnObjet(true, hit.point.y);
            }

            if (!hit.collider.CompareTag("Water"))
            {
                hasLanded = true;
                successfullyLanded = false;

                LandOnObjet(false, hit.point.y);
            }
        }
    }

    void Update()
    {
        if (isCasting && !hasLanded)
        {
            castTimer += Time.deltaTime / castDuration;
            if (castTimer >= 1f)
            {
                castTimer = 1f;
                isCasting = false;
            }

            Vector3 horizontalPos = Vector3.Lerp(startPoint.position, targetPoint.position, castTimer);
            float height = arcCurve.Evaluate(castTimer);
            transform.position = new Vector3(horizontalPos.x, startPoint.position.y + height, horizontalPos.z);
            
            OnBaitLanded();
        }
    }

    void LandOnObjet(bool onWater, float waterHeight)
    {
        isCasting = false;

        // Snap the bait to the water surface
        Vector3 landedPosition = transform.position;
        landedPosition.y = waterHeight + 0.1f; // Add slight offset to prevent sinking
        transform.position = landedPosition;

        if (onWater)
        {
            anim.Play("casted_idle");
            Debug.Log("Bait landed on water!");
        }
        else
        {
            anim.Play("default");
            Debug.Log("Bait landed on not water!");
        }
    }
}
