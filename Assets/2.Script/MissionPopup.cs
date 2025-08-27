using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 버튼을 누를 때마다 미션 팝업을
/// - 하단(작게)  <->  중앙(크게)
/// 로 1초 동안 부드럽게 이동/확대 토글한다.
/// 팝업/포즈들은 메인 카메라의 자식으로 배치해두는 것을 권장.
/// </summary>
[AddComponentMenu("UI/Mission Popup Fancy Toggle")]
public class MissionPopup : MonoBehaviour
{
    [Header("팝업 오브젝트")]
    [Tooltip("토글할 패널 루트 (메인 카메라의 자식으로 배치 권장)")]
    public GameObject missionPanel;

    [Header("입력")]
    [Tooltip("팝업 토글 InputAction (performed 시 토글)")]
    public InputActionReference toggleAction;
    [Tooltip("OnEnable 시 자동 Enable 할지 여부")]
    public bool autoEnableAction = true;

    [Header("애니메이션(포즈 마커)")]
    [Tooltip("하단(숨김/축소) 포즈 (localPosition/localScale를 사용)")]
    public Transform bottomPose;
    [Tooltip("중앙(표시/확대) 포즈 (localPosition/localScale를 사용)")]
    public Transform centerPose;

    [Header("애니메이션 설정")]
    [Tooltip("이동/스케일 보간 시간(초)")]
    public float duration = 1.0f;
    [Tooltip("숨길 때 애니메이션 종료 후 비활성화할지")]
    public bool deactivateOnHide = true;

    [Header("초기 상태")]
    [Tooltip("시작 시 숨김 상태로 둘지 (하단 포즈로 세팅)")]
    public bool startHidden = true;

    // 내부
    bool _isShowing;
    Coroutine _animCo;
    Transform _panelT;

    void Awake()
    {
        if (missionPanel == null)
            Debug.LogWarning("[MissionPopup] missionPanel이 비어 있습니다.", this);
        if (toggleAction == null)
            Debug.LogWarning("[MissionPopup] toggleAction이 비어 있습니다.", this);
        if (bottomPose == null || centerPose == null)
            Debug.LogWarning("[MissionPopup] bottomPose/centerPose를 인스펙터에 지정하세요.", this);

        _panelT = missionPanel != null ? missionPanel.transform : null;
    }

    void OnEnable()
    {
        if (toggleAction != null)
        {
            if (autoEnableAction) toggleAction.action.Enable();
            toggleAction.action.performed += OnTogglePerformed;
        }

        // 초기 상태 적용
        if (_panelT != null && bottomPose != null && centerPose != null)
        {
            if (startHidden)
            {
                ApplyPose(bottomPose);
                _isShowing = false;
                if (deactivateOnHide) missionPanel.SetActive(false);
            }
            else
            {
                missionPanel.SetActive(true);
                ApplyPose(centerPose);
                _isShowing = true;
            }
        }
    }

    void OnDisable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.performed -= OnTogglePerformed;
            if (autoEnableAction) toggleAction.action.Disable();
        }
    }

    void OnTogglePerformed(InputAction.CallbackContext _)
    {
        Toggle();
    }

    public void Toggle()
    {
        if (missionPanel == null || _panelT == null || bottomPose == null || centerPose == null) return;

        // 보이게 전환
        if (!_isShowing)
        {
            missionPanel.SetActive(true); // 먼저 켠 뒤 애니 시작
            StartAnim(from: _panelT, to: centerPose, setShowing: true);
        }
        // 숨기게 전환
        else
        {
            StartAnim(from: _panelT, to: bottomPose, setShowing: false, after: () =>
            {
                if (deactivateOnHide) missionPanel.SetActive(false);
            });
        }
    }

    public void Show()
    {
        if (_isShowing) return;
        if (missionPanel == null || _panelT == null || centerPose == null) return;
        missionPanel.SetActive(true);
        StartAnim(_panelT, centerPose, true);
    }

    public void Hide()
    {
        if (!_isShowing) return;
        if (missionPanel == null || _panelT == null || bottomPose == null) return;
        StartAnim(_panelT, bottomPose, false, after: () =>
        {
            if (deactivateOnHide) missionPanel.SetActive(false);
        });
    }

    // --- 내부 유틸 ---

    void ApplyPose(Transform pose)
    {
        _panelT.localPosition = pose.localPosition;
        _panelT.localScale = pose.localScale;
        // 회전도 맞추고 싶으면 아래 주석 해제
        // _panelT.localRotation = pose.localRotation;
    }

    void StartAnim(Transform from, Transform to, bool setShowing, System.Action after = null)
    {
        if (_animCo != null) StopCoroutine(_animCo);
        _animCo = StartCoroutine(AnimRoutine(from, to, setShowing, after));
    }

    System.Collections.IEnumerator AnimRoutine(Transform from, Transform to, bool setShowing, System.Action after)
    {
        _isShowing = setShowing;

        Vector3 p0 = from.localPosition;
        Vector3 s0 = from.localScale;
        Vector3 p1 = to.localPosition;
        Vector3 s1 = to.localScale;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, duration);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            // 부드러운 이징
            float e = Mathf.SmoothStep(0f, 1f, t);

            _panelT.localPosition = Vector3.LerpUnclamped(p0, p1, e);
            _panelT.localScale = Vector3.LerpUnclamped(s0, s1, e);
            yield return null;
        }

        // 스냅 보정
        _panelT.localPosition = p1;
        _panelT.localScale = s1;

        _animCo = null;
        after?.Invoke();
    }
}
