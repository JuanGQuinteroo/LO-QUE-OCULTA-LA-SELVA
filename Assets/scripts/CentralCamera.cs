using UnityEngine;
using UnityEngine.UIElements;

public class CentralCamera : MonoBehaviour
{
    public GameObject Miguel;

    void Update()
    {
        Vector3 position = transform.position;
        position.x = Miguel.transform.position.x;
        position.y = Miguel.transform.position.y;
        transform.position = position;
    }
}
