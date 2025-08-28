using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 버튼을 누르면 미션 팝업을 켜고/끄는 매우 단순한 토글 스크립트.
/// 팝업 오브젝트는 메인카메라의 자식으로 미리 배치(위치 고정)해 둔다.
/// </summary>
[AddComponentMenu("UI/Mission Popup Simple Toggle")]
public class MissionPopup : MonoBehaviour
{
    [Header("팝업 오브젝트")]
    [Tooltip("켜고/끄고 할 패널 루트 오브젝트")]
    public GameObject missionPanel;

    [Header("입력")]
    [Tooltip("팝업 토글 InputAction (performed 시 토글)")]
    public InputActionReference toggleAction;

    [Tooltip("이 스크립트가 켜질 때 토글 액션을 자동 Enable 할지 여부")]
    public bool autoEnableAction = true;

    [Header("초기 상태")]
    [Tooltip("시작 시 팝업을 숨길지 여부")]
    public bool startHidden = true;

    private void Awake()
    {
        if (missionPanel == null)
            Debug.LogWarning("[MissionPopupSimple] missionPanel이 비었습니다. 인스펙터에서 지정하세요.", this);
        if (toggleAction == null)
            Debug.LogWarning("[MissionPopupSimple] toggleAction이 비었습니다. InputActionReference를 지정하세요.", this);
    }

    private void OnEnable()
    {
        if (missionPanel != null)
            missionPanel.SetActive(!startHidden);

        if (toggleAction != null)
        {
            if (autoEnableAction) toggleAction.action.Enable();
            toggleAction.action.performed += OnTogglePerformed;
        }
    }

    private void OnDisable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.performed -= OnTogglePerformed;
            if (autoEnableAction) toggleAction.action.Disable();
        }
    }

    private void OnTogglePerformed(InputAction.CallbackContext _)
    {
        if (missionPanel == null) return;
        missionPanel.SetActive(!missionPanel.activeSelf);
    }

    // 외부에서 호출하고 싶을 때 사용할 수 있는 유틸
    public void Show() { if (missionPanel) missionPanel.SetActive(true); }
    public void Hide() { if (missionPanel) missionPanel.SetActive(false); }
    public void Toggle() { if (missionPanel) missionPanel.SetActive(!missionPanel.activeSelf); }
}

