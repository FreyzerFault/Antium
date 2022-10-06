using UnityEngine;

public class Hearts : MonoBehaviour
{
    public void OnEnd()
    {
        Destroy(this.gameObject);
    }
}
