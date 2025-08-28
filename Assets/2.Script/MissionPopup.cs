using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ��ư�� ������ �̼� �˾��� �Ѱ�/���� �ſ� �ܼ��� ��� ��ũ��Ʈ.
/// �˾� ������Ʈ�� ����ī�޶��� �ڽ����� �̸� ��ġ(��ġ ����)�� �д�.
/// </summary>
[AddComponentMenu("UI/Mission Popup Simple Toggle")]
public class MissionPopup : MonoBehaviour
{
    [Header("�˾� ������Ʈ")]
    [Tooltip("�Ѱ�/���� �� �г� ��Ʈ ������Ʈ")]
    public GameObject missionPanel;

    [Header("�Է�")]
    [Tooltip("�˾� ��� InputAction (performed �� ���)")]
    public InputActionReference toggleAction;

    [Tooltip("�� ��ũ��Ʈ�� ���� �� ��� �׼��� �ڵ� Enable ���� ����")]
    public bool autoEnableAction = true;

    [Header("�ʱ� ����")]
    [Tooltip("���� �� �˾��� ������ ����")]
    public bool startHidden = true;

    private void Awake()
    {
        if (missionPanel == null)
            Debug.LogWarning("[MissionPopupSimple] missionPanel�� ������ϴ�. �ν����Ϳ��� �����ϼ���.", this);
        if (toggleAction == null)
            Debug.LogWarning("[MissionPopupSimple] toggleAction�� ������ϴ�. InputActionReference�� �����ϼ���.", this);
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

    // �ܺο��� ȣ���ϰ� ���� �� ����� �� �ִ� ��ƿ
    public void Show() { if (missionPanel) missionPanel.SetActive(true); }
    public void Hide() { if (missionPanel) missionPanel.SetActive(false); }
    public void Toggle() { if (missionPanel) missionPanel.SetActive(!missionPanel.activeSelf); }
}

