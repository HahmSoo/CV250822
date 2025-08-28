using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics; // HapticImpulsePlayer

[RequireComponent(typeof(Renderer))]
public class MaterialBlinker : MonoBehaviour
{
    [Header("Blink ����")]
    [Tooltip("�� �� ��/�� ���¸� �����ϴ� �ð�(��)")]
    public float blinkInterval = 0.2f;

    [Tooltip("�˺����� �Բ� ���������� �Ѱ�/��������")]
    public bool affectAlbedo = true;
    public Color blinkColor = Color.red;

    [Header("Emission(�߱�)")]
    [Tooltip("Emission�� ��ũ�� �������")]
    public bool useEmission = true;

    [ColorUsage(true, true)]
    [Tooltip("���� �� ������ Emission ��(HDR ����)")]
    public Color emissionColor = Color.red;

    [Tooltip("Emission ���� ��� (���� ���� emissionColor * intensity)")]
    public float emissionIntensity = 3f;

    [Header("Haptics(��Ʈ�ѷ� ����)")]
    [Tooltip("��ƽ�� ���� ��� ��Ʈ�ѷ�(HapticImpulsePlayer)")]
    public HapticImpulsePlayer hapticTarget;
    [Range(0f, 1f)]
    public float hapticStrength = 0.5f;
    [Tooltip("ON ������ ���� ��ƽ ���ӽð�(��)")]
    public float hapticDuration = 0.05f;

    [Header("�Է�")]
    [Tooltip("�������� ���ߴ� InputAction (��: XR Controller ��ư)")]
    public InputActionReference stopBlinkAction;

    private Renderer _rend;
    private Material _mat;
    private Coroutine _blinkCo;

    // ������/���� ����
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
        _mat = _rend.material; // �ν��Ͻ�ȭ(���� ��Ƽ���� ��ȣ)

        _hasColorProp = _mat.HasProperty(ID_Color);
        _hasEmissionProp = _mat.HasProperty(ID_EmissionColor);

        if (_hasColorProp) _origAlbedo = _mat.GetColor(ID_Color);
        if (_hasEmissionProp) _origEmission = _mat.GetColor(ID_EmissionColor);

        // ���� Ű���� ���¸� ����(���⼱ ������ ����)
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
        StopBlink(); // ��Ȱ��ȭ �� ���� ����
    }

    /// <summary>�ܺο��� ȣ��: ������ ����</summary>
    public void BlinkRed()
    {
        if (_blinkCo != null) StopCoroutine(_blinkCo);
        _blinkCo = StartCoroutine(BlinkLoop());
    }

    /// <summary>�Է����� ȣ��: ��� �ߴ�</summary>
    private void OnStopBlink(InputAction.CallbackContext _) => StopBlink();

    /// <summary>������ ��� �ߴ� + ���� ��/Ű����� ����</summary>
    public void StopBlink()
    {
        if (_blinkCo != null)
        {
            StopCoroutine(_blinkCo);
            _blinkCo = null;
        }
        ApplyOffState(); // ����
    }

    private IEnumerator BlinkLoop()
    {
        // ó�� ON
        ApplyOnState();
        yield return new WaitForSeconds(blinkInterval);

        while (true)
        {
            // OFF (����)
            ApplyOffState();
            yield return new WaitForSeconds(blinkInterval);

            // ON
            ApplyOnState();
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    // ===== ���� ���� =====

    // ON: ���� + Emission ��(�̶��� Ű���� ON) + ��ƽ �߻�
    private void ApplyOnState()
    {
        if (_hasColorProp && affectAlbedo)
            _mat.SetColor(ID_Color, blinkColor);

        if (useEmission && _hasEmissionProp)
        {
            _mat.EnableKeyword("_EMISSION"); // ON�� ���� ��
            _mat.SetColor(ID_EmissionColor, emissionColor * Mathf.Max(0f, emissionIntensity));
        }

        // ��ƽ: ON �������� �� �� �ڱ�
        if (hapticTarget != null && hapticDuration > 0f && hapticStrength > 0f)
        {
            hapticTarget.SendHapticImpulse(hapticStrength, hapticDuration);
        }
    }

    // OFF: ���� ��/Ű����� ���� (���� Emission �������� ��)
    private void ApplyOffState()
    {
        if (_hasColorProp)
            _mat.SetColor(ID_Color, _origAlbedo);

        if (_hasEmissionProp)
        {
            _mat.SetColor(ID_EmissionColor, _origEmission);

            if (_hadEmissionKeyword)
                _mat.EnableKeyword("_EMISSION");   // ���� ���� �ִ� ���� ����
            else
                _mat.DisableKeyword("_EMISSION");  // ���� ���� �ִ� ��쿣 ��
        }
    }
}
