using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            PlayByeah();
    }

    public void PlayByeah()
    {
        GetComponent<AudioSource>().Play();
    }
}
