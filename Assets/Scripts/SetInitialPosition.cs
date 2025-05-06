using UnityEngine;

public class SetInitialPosition : MonoBehaviour
{
    public Vector3 position = Vector3.zero;
    private Vector3 originalPosition = Vector3.zero;

    private void Awake()
    {
        Debug.Log($"transform.position : {transform.position}   -   offset : {position}");
        transform.position = position;
        Debug.Log($"transform.position : {transform.position}   -   offset : {position}");
    }

    private void Start()
    {
        originalPosition = transform.position;
    }

    private void Update()
    {
    }
}
