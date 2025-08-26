using System.Collections;
using UnityEngine;
using MikeNspired.XRIStarterKit; // XRKnob 참조

/// <summary>
/// XRKnob이 최댓값( Value = 1.0 )에 도달하면 문짝을 Y축으로 -150도까지 서서히 회전.
/// 문틀(프레임)에 붙이고, door와 knob을 인스펙터에서 할당하세요.
/// </summary>
[AddComponentMenu("XR/Door/Door Rotate By Knob Max")]
public class DoorMain : MonoBehaviour
{
    [Header("References")]
    [Tooltip("돌릴 문짝(힌지 Pivot이 경첩 위치여야 함)")]
    [SerializeField] private Transform door;

    [Tooltip("값을 제공하는 XRKnob (Value 0~1)")]
    [SerializeField] private XRKnob knob;

    [Header("Rotation")]
    [Tooltip("노브가 Max일 때 목표 Y 각도(도). 예: -150")]
    [SerializeField] private float targetYAngle = -150f;

    [Tooltip("문이 목표 각도까지 도달하는 데 걸리는 시간(초)")]
    [SerializeField] private float openDuration = 2.0f;

    [Tooltip("트리거 임계값(노브 값이 이 값 이상이면 오픈 시작)")]
    [Range(0.9f, 1.0f)]
    [SerializeField] private float triggerThreshold = 0.995f;

    [Tooltip("한 번만 열고 다시는 열지 않음 (true) / 임계 이하로 내려갔다가 다시 넘으면 또 염 (false)")]
    [SerializeField] private bool openOnce = true;

    // 내부 상태
    private bool _openedOnce = false;
    private Coroutine _rotateCo;
    private Quaternion _initialLocalRotation;

    private void Reset()
    {
        // 편의상 자동으로 찾기
        if (!door && transform.childCount > 0) door = transform.GetChild(0);
        if (!knob) knob = GetComponentInChildren<XRKnob>();
    }

    private void Awake()
    {
        if (!door) Debug.LogWarning("[DoorRotateByKnobMax] door가 비어 있습니다. 인스펙터에서 문짝 Transform을 할당하세요.", this);
        if (!knob) Debug.LogWarning("[DoorRotateByKnobMax] knob이 비어 있습니다. 인스펙터에서 XRKnob을 할당하세요.", this);

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

    private void OnKnobValueChanged(float _ignored) // 리맵값 무시, 실제 knob.Value 사용
    {
        if (door == null || knob == null) return;

        // XRKnob의 실제 Value(0~1) 읽기
        float v = knob.Value;

        // 이미 한 번 열었고 openOnce면 더 이상 트리거하지 않음
        if (_openedOnce && openOnce) return;

        // 임계값 이상이면 열기 시작
        if (v >= triggerThreshold)
        {
            StartOpen();
        }
        // (요청사항에 닫기는 없었으므로 구현하지 않음)
    }

    private void StartOpen()
    {
        _openedOnce = true;

        if (_rotateCo != null) StopCoroutine(_rotateCo);
        _rotateCo = StartCoroutine(RotateDoorTo(targetYAngle, openDuration));
    }

    private IEnumerator RotateDoorTo(float yAngle, float duration)
    {
        // 현재 회전에서 목표 회전까지 부드럽게
        Quaternion start = door.localRotation;
        // 목표 Y각도 기준 로컬 회전(기존 X/Z는 유지)
        Vector3 e = door.localEulerAngles;
        // 0~360 래핑을 피하려고 Quaternion 직접 생성
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

    // 필요시 초기 상태로 되돌리는 유틸 (수동 호출용)
    public void ResetDoorRotation()
    {
        if (door == null) return;
        if (_rotateCo != null) StopCoroutine(_rotateCo);
        door.localRotation = _initialLocalRotation;
        _openedOnce = false;
    }
}
