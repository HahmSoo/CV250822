using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class MissionStep
{
    [Tooltip("이 단계에서 기다릴 트리거 ID (MissionTrigger.triggerId와 일치해야 함)")]
    public string id;

    [Tooltip("이 단계에서 활성화할 트리거 오브젝트(없으면 null)")]
    public GameObject triggerObject;

    [Tooltip("해당 지점 도달 시 켤 연출(예: curvedLine). 필요 없으면 비워도 됨")]
    public GameObject curvedLine;

    [Header("Blink 옵션")]
    [Tooltip("이 단계가 시작될 때 BlinkRed() 호출")]
    public bool blinkOnStart = false;

    [Tooltip("이 단계가 완료될 때 BlinkRed() 호출")]
    public bool blinkOnComplete = false;

    [Tooltip("이 스텝만 별도의 MaterialBlinker를 쓰고 싶다면 지정 (비우면 GameManager 기본 blinkerTarget 사용)")]
    public MaterialBlinker overrideBlinker;

    [Header("이벤트(선택)")]
    public UnityEvent onStepStart;     // 단계 시작 시 실행할 UnityEvent
    public UnityEvent onStepComplete;  // 단계 완료 시 실행할 UnityEvent

    [Header("오브젝트 상태 제어 (완료 시)")]
    [Tooltip("이 스텝이 완료될 때 켜줄 오브젝트들")]
    public GameObject[] activateOnComplete;

    [Tooltip("이 스텝이 완료될 때 꺼줄 오브젝트들")]
    public GameObject[] deactivateOnComplete;
}

/// <summary>
/// GameManager: 게임 전반 흐름을 관리.
/// - 시작 지연 → BlinkRed 시작(옵션)
/// - 순차적으로 각 MissionStep 실행
/// - Step별 트리거 도달 시 curvedLine, 이벤트, 오브젝트 상태 제어, Blink 재호출 가능
/// </summary>
[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    [Header("기본 Blink 대상")]
    [Tooltip("BlinkRed()를 기본으로 호출할 MaterialBlinker (없으면 비워도 됨)")]
    public MaterialBlinker blinkerTarget;

    [Header("초기 지연(초)")]
    public float initialDelay = 5f;

    [Header("게임 시작 시 Blink 시작 여부")]
    public bool blinkAtGameStart = true;

    [Header("미션 단계(순서대로 수행)")]
    public MissionStep[] steps;

    private Coroutine _flowCo;
    private string _waitingId = null;
    private bool _received = false;

    private void Awake()
    {
        if (blinkerTarget == null)
            blinkerTarget = GetComponent<MaterialBlinker>();
    }

    private void OnEnable()
    {
        _flowCo = StartCoroutine(GameFlow());
    }

    private void OnDisable()
    {
        if (_flowCo != null) StopCoroutine(_flowCo);
    }

    private IEnumerator GameFlow()
    {
        // 1) 초기 지연
        yield return new WaitForSeconds(initialDelay);

        // 2) 시작 시 Blink 옵션
        if (blinkAtGameStart && blinkerTarget != null)
            blinkerTarget.BlinkRed();

        // 3) 단계 순차 실행
        if (steps != null && steps.Length > 0)
        {
            for (int i = 0; i < steps.Length; i++)
            {
                var s = steps[i];

                // --- Step Start ---
                if (s.triggerObject) s.triggerObject.SetActive(true);
                s.onStepStart?.Invoke();

                if (s.blinkOnStart)
                    GetBlinkerForStep(s)?.BlinkRed();

                // 트리거 ID 대기
                _waitingId = s.id;
                _received = false;
                yield return new WaitUntil(() => _received);

                // --- Step Complete ---
                if (s.curvedLine) s.curvedLine.SetActive(true);
                s.onStepComplete?.Invoke();

                // 완료 시 오브젝트 상태 제어
                if (s.activateOnComplete != null)
                    foreach (var go in s.activateOnComplete)
                        if (go) go.SetActive(true);

                if (s.deactivateOnComplete != null)
                    foreach (var go in s.deactivateOnComplete)
                        if (go) go.SetActive(false);

                if (s.blinkOnComplete)
                    GetBlinkerForStep(s)?.BlinkRed();

                if (s.triggerObject) s.triggerObject.SetActive(false);
            }
        }

        Debug.Log("[GameManager] 모든 단계 완료");
        while (true) yield return null;
    }

    /// <summary>
    /// MissionTrigger에서 호출: 현재 기다리는 ID와 일치하면 완료 처리
    /// </summary>
    public void NotifyTriggerEntered(string id)
    {
        if (!string.IsNullOrEmpty(_waitingId) && id == _waitingId)
            _received = true;
    }

    private MaterialBlinker GetBlinkerForStep(MissionStep s)
    {
        return (s != null && s.overrideBlinker != null) ? s.overrideBlinker : blinkerTarget;
    }
}
