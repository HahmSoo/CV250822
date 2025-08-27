using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ��ư�� ���� ������ �̼� �˾���
/// - �ϴ�(�۰�)  <->  �߾�(ũ��)
/// �� 1�� ���� �ε巴�� �̵�/Ȯ�� ����Ѵ�.
/// �˾�/������� ���� ī�޶��� �ڽ����� ��ġ�صδ� ���� ����.
/// </summary>
[AddComponentMenu("UI/Mission Popup Fancy Toggle")]
public class MissionPopup : MonoBehaviour
{
    [Header("�˾� ������Ʈ")]
    [Tooltip("����� �г� ��Ʈ (���� ī�޶��� �ڽ����� ��ġ ����)")]
    public GameObject missionPanel;

    [Header("�Է�")]
    [Tooltip("�˾� ��� InputAction (performed �� ���)")]
    public InputActionReference toggleAction;
    [Tooltip("OnEnable �� �ڵ� Enable ���� ����")]
    public bool autoEnableAction = true;

    [Header("�ִϸ��̼�(���� ��Ŀ)")]
    [Tooltip("�ϴ�(����/���) ���� (localPosition/localScale�� ���)")]
    public Transform bottomPose;
    [Tooltip("�߾�(ǥ��/Ȯ��) ���� (localPosition/localScale�� ���)")]
    public Transform centerPose;

    [Header("�ִϸ��̼� ����")]
    [Tooltip("�̵�/������ ���� �ð�(��)")]
    public float duration = 1.0f;
    [Tooltip("���� �� �ִϸ��̼� ���� �� ��Ȱ��ȭ����")]
    public bool deactivateOnHide = true;

    [Header("�ʱ� ����")]
    [Tooltip("���� �� ���� ���·� ���� (�ϴ� ����� ����)")]
    public bool startHidden = true;

    // ����
    bool _isShowing;
    Coroutine _animCo;
    Transform _panelT;

    void Awake()
    {
        if (missionPanel == null)
            Debug.LogWarning("[MissionPopup] missionPanel�� ��� �ֽ��ϴ�.", this);
        if (toggleAction == null)
            Debug.LogWarning("[MissionPopup] toggleAction�� ��� �ֽ��ϴ�.", this);
        if (bottomPose == null || centerPose == null)
            Debug.LogWarning("[MissionPopup] bottomPose/centerPose�� �ν����Ϳ� �����ϼ���.", this);

        _panelT = missionPanel != null ? missionPanel.transform : null;
    }

    void OnEnable()
    {
        if (toggleAction != null)
        {
            if (autoEnableAction) toggleAction.action.Enable();
            toggleAction.action.performed += OnTogglePerformed;
        }

        // �ʱ� ���� ����
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

        // ���̰� ��ȯ
        if (!_isShowing)
        {
            missionPanel.SetActive(true); // ���� �� �� �ִ� ����
            StartAnim(from: _panelT, to: centerPose, setShowing: true);
        }
        // ����� ��ȯ
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

    // --- ���� ��ƿ ---

    void ApplyPose(Transform pose)
    {
        _panelT.localPosition = pose.localPosition;
        _panelT.localScale = pose.localScale;
        // ȸ���� ���߰� ������ �Ʒ� �ּ� ����
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
            // �ε巯�� ��¡
            float e = Mathf.SmoothStep(0f, 1f, t);

            _panelT.localPosition = Vector3.LerpUnclamped(p0, p1, e);
            _panelT.localScale = Vector3.LerpUnclamped(s0, s1, e);
            yield return null;
        }

        // ���� ����
        _panelT.localPosition = p1;
        _panelT.localScale = s1;

        _animCo = null;
        after?.Invoke();
    }
}
