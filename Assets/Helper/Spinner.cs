using UnityEngine;

public class Spinner : MonoBehaviour
{
    public float Rate;

    void Update()
    {
        transform.Rotate(0, 0, Rate * Time.deltaTime);
    }
}
