using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public bool collected = false;
    private void OnTriggerEnter(Collider other)
    {
        collected = true;
        gameObject.SetActive(!collected);
    }
}
