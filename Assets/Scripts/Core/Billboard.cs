using UnityEngine;

/// <summary>
/// 빌보드 효과 - 항상 카메라를 바라보게 함
/// - HP 바 등 World Space UI에 부착
/// </summary>
public class Billboard : MonoBehaviour
{
    #region Private Fields
    Camera _mainCamera;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // 카메라와 동일한 방향을 바라보게 함
        transform.rotation = _mainCamera.transform.rotation;
    }
    #endregion
}
