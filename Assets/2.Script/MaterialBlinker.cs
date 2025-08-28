using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics; // HapticImpulsePlayer

[RequireComponent(typeof(Renderer))]
public class MaterialBlinker : MonoBehaviour
{
    [Header("Blink 설정")]
    [Tooltip("한 번 켠/끈 상태를 유지하는 시간(초)")]
    public float blinkInterval = 0.2f;

    [Tooltip("알베도도 함께 빨간색으로 켜고/복원할지")]
    public bool affectAlbedo = true;
    public Color blinkColor = Color.red;

    [Header("Emission(발광)")]
    [Tooltip("Emission을 블링크에 사용할지")]
    public bool useEmission = true;

    [ColorUsage(true, true)]
    [Tooltip("켜질 때 적용할 Emission 색(HDR 가능)")]
    public Color emissionColor = Color.red;

    [Tooltip("Emission 강도 배수 (켜질 때는 emissionColor * intensity)")]
    public float emissionIntensity = 3f;

    [Header("Haptics(컨트롤러 진동)")]
    [Tooltip("햅틱을 보낼 대상 컨트롤러(HapticImpulsePlayer)")]
    public HapticImpulsePlayer hapticTarget;
    [Range(0f, 1f)]
    public float hapticStrength = 0.5f;
    [Tooltip("ON 순간에 보낼 햅틱 지속시간(초)")]
    public float hapticDuration = 0.05f;

    [Header("입력")]
    [Tooltip("깜빡임을 멈추는 InputAction (예: XR Controller 버튼)")]
    public InputActionReference stopBlinkAction;

    private Renderer _rend;
    private Material _mat;
    private Coroutine _blinkCo;

    // 원본값/상태 저장
    private Color _origAlbedo = Color.white;
    private Color _origEmission = Color.black;
    private bool _hadEmissionKeyword = false;
    private bool _hasColorProp = false;
    private bool _hasEmissionProp = false;

    static readonly int ID_Color = Shader.PropertyToID("_Color");
    static readonly int ID_EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        _rend = GetComponent<Renderer>();
        _mat = _rend.material; // 인스턴스화(공유 머티리얼 보호)

        _hasColorProp = _mat.HasProperty(ID_Color);
        _hasEmissionProp = _mat.HasProperty(ID_EmissionColor);

        if (_hasColorProp) _origAlbedo = _mat.GetColor(ID_Color);
        if (_hasEmissionProp) _origEmission = _mat.GetColor(ID_EmissionColor);

        // 원래 키워드 상태만 저장(여기선 켜지지 않음)
        _hadEmissionKeyword = _mat.IsKeywordEnabled("_EMISSION");
    }

    void OnEnable()
    {
        if (stopBlinkAction != null)
        {
            stopBlinkAction.action.performed += OnStopBlink;
            stopBlinkAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (stopBlinkAction != null)
        {
            stopBlinkAction.action.performed -= OnStopBlink;
            stopBlinkAction.action.Disable();
        }
        StopBlink(); // 비활성화 시 안전 복원
    }

    /// <summary>외부에서 호출: 깜빡임 시작</summary>
    public void BlinkRed()
    {
        if (_blinkCo != null) StopCoroutine(_blinkCo);
        _blinkCo = StartCoroutine(BlinkLoop());
    }

    /// <summary>입력으로 호출: 즉시 중단</summary>
    private void OnStopBlink(InputAction.CallbackContext _) => StopBlink();

    /// <summary>깜빡임 즉시 중단 + 원래 값/키워드로 복원</summary>
    public void StopBlink()
    {
        if (_blinkCo != null)
        {
            StopCoroutine(_blinkCo);
            _blinkCo = null;
        }
        ApplyOffState(); // 원복
    }

    private IEnumerator BlinkLoop()
    {
        // 처음 ON
        ApplyOnState();
        yield return new WaitForSeconds(blinkInterval);

        while (true)
        {
            // OFF (복원)
            ApplyOffState();
            yield return new WaitForSeconds(blinkInterval);

            // ON
            ApplyOnState();
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    // ===== 상태 적용 =====

    // ON: 빨강 + Emission 켬(이때만 키워드 ON) + 햅틱 발사
    private void ApplyOnState()
    {
        if (_hasColorProp && affectAlbedo)
            _mat.SetColor(ID_Color, blinkColor);

        if (useEmission && _hasEmissionProp)
        {
            _mat.EnableKeyword("_EMISSION"); // ON일 때만 켬
            _mat.SetColor(ID_EmissionColor, emissionColor * Mathf.Max(0f, emissionIntensity));
        }

        // 햅틱: ON 순간에만 한 번 자극
        if (hapticTarget != null && hapticDuration > 0f && hapticStrength > 0f)
        {
            hapticTarget.SendHapticImpulse(hapticStrength, hapticDuration);
        }
    }

    // OFF: 원래 값/키워드로 복원 (원래 Emission 없었으면 끔)
    private void ApplyOffState()
    {
        if (_hasColorProp)
            _mat.SetColor(ID_Color, _origAlbedo);

        if (_hasEmissionProp)
        {
            _mat.SetColor(ID_EmissionColor, _origEmission);

            if (_hadEmissionKeyword)
                _mat.EnableKeyword("_EMISSION");   // 원래 켜져 있던 상태 유지
            else
                _mat.DisableKeyword("_EMISSION");  // 원래 꺼져 있던 경우엔 끔
        }
    }
}
