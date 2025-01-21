using UnityEngine;
using TMPro;

public class WalkNode : MonoBehaviour
{
    public int Cost;

    [SerializeField] private TMP_Text text;

    private void Start()
    {
        text.text = "" + Cost;
    }

    private void Update()
    {
        transform.forward = transform.position - Camera.main.transform.position;
    }
}
