using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crosshair1 : MonoBehaviour
{
    public int rotSpeed = 10;
    private void Update()
    {
        transform.Rotate(0, 0, rotSpeed * Time.deltaTime);
    }
}
