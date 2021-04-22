using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crosshair2 : MonoBehaviour
{
    public int rotSpeed = -50;
    private void Update()
    {
        transform.Rotate(0, 0, rotSpeed * Time.deltaTime);
    }
}
