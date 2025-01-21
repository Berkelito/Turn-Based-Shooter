using UnityEngine;

public class SpinScript : MonoBehaviour //this is very much a placeholder until i figure out what i want to do
{
    [SerializeField] float speed;

    void Update()
    {
        transform.Rotate(0,Time.deltaTime * speed, 0);
    }
}
