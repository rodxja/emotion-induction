using UnityEngine;

public class RigOffset : MonoBehaviour
{
    public GameObject Rig;
    public GameObject Anchor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log($"postion : {Rig.transform.position} anchor {Anchor.transform.position}");
        Debug.Log($"local postion : {Rig.transform.localPosition} anchor {Anchor.transform.localPosition}");
        Vector3 position = Rig.transform.position;
        Vector3 offset = Anchor.transform.position - position;

        Rig.transform.position = new Vector3(position.x + offset.x, position.y, position.z + offset.z);

        Debug.Log($"new  postion : {Rig.transform.position} anchor {Anchor.transform.position}");
        Debug.Log($"local postion : {Rig.transform.localPosition} anchor {Anchor.transform.localPosition}");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"current postion : {Rig.transform.position} anchor {Anchor.transform.position}");
        Debug.Log($"local postion : {Rig.transform.localPosition} anchor {Anchor.transform.localPosition}");
    }
}
