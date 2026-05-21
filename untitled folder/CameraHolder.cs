using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 1.6f, 0);
    public float smooth = 12f;

    void LateUpdate()
    {
        if (!player) return;

        Vector3 targetPos = player.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            smooth * Time.deltaTime
        );
    }
}