using System.Collections;
using UnityEngine;
using MikeNspired.XRIStarterKit; // XRKnob ����

/// <summary>
/// XRKnob�� �ִ�( Value = 1.0 )�� �����ϸ� ��¦�� Y������ -150������ ������ ȸ��.
/// ��Ʋ(������)�� ���̰�, door�� knob�� �ν����Ϳ��� �Ҵ��ϼ���.
/// </summary>
[AddComponentMenu("XR/Door/Door Rotate By Knob Max")]
public class DoorMain : MonoBehaviour
{
    [Header("References")]
    [Tooltip("���� ��¦(���� Pivot�� ��ø ��ġ���� ��)")]
    [SerializeField] private Transform door;

    [Tooltip("���� �����ϴ� XRKnob (Value 0~1)")]
    [SerializeField] private XRKnob knob;

    [Header("Rotation")]
    [Tooltip("��갡 Max�� �� ��ǥ Y ����(��). ��: -150")]
    [SerializeField] private float targetYAngle = -150f;

    [Tooltip("���� ��ǥ �������� �����ϴ� �� �ɸ��� �ð�(��)")]
    [SerializeField] private float openDuration = 2.0f;

    [Tooltip("Ʈ���� �Ӱ谪(��� ���� �� �� �̻��̸� ���� ����)")]
    [Range(0.9f, 1.0f)]
    [SerializeField] private float triggerThreshold = 0.995f;

    [Tooltip("�� ���� ���� �ٽô� ���� ���� (true) / �Ӱ� ���Ϸ� �������ٰ� �ٽ� ������ �� �� (false)")]
    [SerializeField] private bool openOnce = true;

    // ���� ����
    private bool _openedOnce = false;
    private Coroutine _rotateCo;
    private Quaternion _initialLocalRotation;

    private void Reset()
    {
        // ���ǻ� �ڵ����� ã��
        if (!door && transform.childCount > 0) door = transform.GetChild(0);
        if (!knob) knob = GetComponentInChildren<XRKnob>();
    }

    private void Awake()
    {
        if (!door) Debug.LogWarning("[DoorRotateByKnobMax] door�� ��� �ֽ��ϴ�. �ν����Ϳ��� ��¦ Transform�� �Ҵ��ϼ���.", this);
        if (!knob) Debug.LogWarning("[DoorRotateByKnobMax] knob�� ��� �ֽ��ϴ�. �ν����Ϳ��� XRKnob�� �Ҵ��ϼ���.", this);

        if (door) _initialLocalRotation = door.localRotation;
    }

    private void OnEnable()
    {
        if (knob != null) knob.OnValueChange.AddListener(OnKnobValueChanged);
    }

    private void OnDisable()
    {
        if (knob != null) knob.OnValueChange.RemoveListener(OnKnobValueChanged);
    }

    private void OnKnobValueChanged(float _ignored) // ���ʰ� ����, ���� knob.Value ���
    {
        if (door == null || knob == null) return;

        // XRKnob�� ���� Value(0~1) �б�
        float v = knob.Value;

        // �̹� �� �� ������ openOnce�� �� �̻� Ʈ�������� ����
        if (_openedOnce && openOnce) return;

        // �Ӱ谪 �̻��̸� ���� ����
        if (v >= triggerThreshold)
        {
            StartOpen();
        }
        // (��û���׿� �ݱ�� �������Ƿ� �������� ����)
    }

    private void StartOpen()
    {
        _openedOnce = true;

        if (_rotateCo != null) StopCoroutine(_rotateCo);
        _rotateCo = StartCoroutine(RotateDoorTo(targetYAngle, openDuration));
    }

    private IEnumerator RotateDoorTo(float yAngle, float duration)
    {
        // ���� ȸ������ ��ǥ ȸ������ �ε巴��
        Quaternion start = door.localRotation;
        // ��ǥ Y���� ���� ���� ȸ��(���� X/Z�� ����)
        Vector3 e = door.localEulerAngles;
        // 0~360 ������ ���Ϸ��� Quaternion ���� ����
        Quaternion target = Quaternion.Euler(e.x, yAngle, e.z);

        float t = 0f;
        duration = Mathf.Max(0.0001f, duration);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            door.localRotation = Quaternion.Slerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        door.localRotation = target;
        _rotateCo = null;
    }

    // �ʿ�� �ʱ� ���·� �ǵ����� ��ƿ (���� ȣ���)
    public void ResetDoorRotation()
    {
        if (door == null) return;
        if (_rotateCo != null) StopCoroutine(_rotateCo);
        door.localRotation = _initialLocalRotation;
        _openedOnce = false;
    }
}
