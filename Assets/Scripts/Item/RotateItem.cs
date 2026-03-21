using UnityEngine;

/// <summary>
/// 아이템 제자리 회전
/// </summary>
public class RotateItem : MonoBehaviour
{
    [SerializeField] float _rotateSpeed = 90f;

    void Update()
    {
        transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime, Space.World);
    }
}
