using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[AddComponentMenu("UI/Typewriter Paged Text")]
public class TypewriterPagedText : MonoBehaviour
{
    [Header("UI ����")]
    public TMP_Text textTarget;
    public Button nextUIButton;

    [Header("��Ʈ�ѷ� �Է�")]
    public InputActionReference nextAction;

    [Header("������ ������")]
    [TextArea(3, 8)]
    public List<string> pages = new List<string>();

    [Header("Ÿ�ڱ� ȿ��")]
    public float charsPerSecond = 30f;
    public bool clearOnStartEachPage = true;
    public bool enableNextWhenFinished = true;

    [Header("������ ������ ó��")]
    [Tooltip("������ ���������� Next�� ������ �� ������Ʈ(�Ǵ� popupRoot)�� �ݽ��ϴ�.")]
    public bool closeOnLastPageNext = true;

    [Tooltip("���� �˾� ��Ʈ(������ �� �� ������Ʈ�� GameObject�� ����)")]
    public GameObject popupRoot;

    [Tooltip("�˾��� ���� �� ȣ��Ǵ� �̺�Ʈ")]
    public UnityEvent onPopupClosed;

    int _pageIndex = 0;
    Coroutine _typingCo;
    bool _isTyping = false;
    string _currentFullText = "";

    void Awake()
    {
        if (popupRoot == null) popupRoot = gameObject;
        if (textTarget == null)
            Debug.LogWarning("[TypewriterPagedText] TMP_Text�� �����ϼ���.", this);
    }

    void OnEnable()
    {
        if (pages.Count > 0)
            StartPage(0);

        if (nextUIButton != null)
        {
            nextUIButton.onClick.RemoveListener(OnClickNext);
            nextUIButton.onClick.AddListener(OnClickNext);
        }

        if (nextAction != null)
        {
            nextAction.action.performed += OnNextInput;
            nextAction.action.Enable();
        }
    }

    void OnDisable()
    {
        StopTyping();

        if (nextUIButton != null)
            nextUIButton.onClick.RemoveListener(OnClickNext);

        if (nextAction != null)
        {
            nextAction.action.performed -= OnNextInput;
            nextAction.action.Disable();
        }
    }

    public void SetPages(IEnumerable<string> newPages, bool restart = true)
    {
        pages = new List<string>(newPages);
        if (restart && pages.Count > 0)
            StartPage(0);
    }

    public void StartPage(int index)
    {
        if (pages == null || pages.Count == 0 || textTarget == null) return;

        _pageIndex = Mathf.Clamp(index, 0, pages.Count - 1);
        _currentFullText = pages[_pageIndex] ?? "";

        if (clearOnStartEachPage) SetTextImmediate("");
        StartTyping(_currentFullText);

        if (nextUIButton != null)
            nextUIButton.interactable = !enableNextWhenFinished;
    }

    void OnNextInput(InputAction.CallbackContext _) => OnClickNext();

    public void OnClickNext()
    {
        if (_isTyping)
        {
            // Ÿ���� ���̸� ��� ��ŵ
            SkipTypingToEnd();
            return;
        }

        // �̹� �������� �������� �� ���� ������ or �ݱ�
        int next = _pageIndex + 1;
        if (next < pages.Count)
        {
            StartPage(next);
        }
        else
        {
            // ������ ���������� Next �� �˾� �ݱ�
            if (closeOnLastPageNext && popupRoot != null)
            {
                popupRoot.SetActive(false);
                onPopupClosed?.Invoke();
            }
            // ��ư ��Ȱ��ȭ(����)
            if (nextUIButton != null) nextUIButton.interactable = false;
        }
    }

    void StartTyping(string fullText)
    {
        StopTyping();
        _typingCo = StartCoroutine(TypeRoutine(fullText));
    }

    void StopTyping()
    {
        if (_typingCo != null)
        {
            StopCoroutine(_typingCo);
            _typingCo = null;
        }
        _isTyping = false;
    }

    void SkipTypingToEnd()
    {
        StopTyping();
        textTarget.text = _currentFullText;
        textTarget.maxVisibleCharacters = int.MaxValue;
        _isTyping = false;

        if (nextUIButton && enableNextWhenFinished)
            nextUIButton.interactable = true;
    }

    IEnumerator TypeRoutine(string fullText)
    {
        _isTyping = true;

        textTarget.text = fullText;
        textTarget.ForceMeshUpdate();
        int total = textTarget.textInfo.characterCount;

        textTarget.maxVisibleCharacters = 0;

        float cps = Mathf.Max(1f, charsPerSecond);
        float t = 0f;
        int visible = 0;

        if (nextUIButton && enableNextWhenFinished)
            nextUIButton.interactable = false;

        while (visible < total)
        {
            t += Time.deltaTime * cps;
            int nextVisible = Mathf.FloorToInt(t);
            if (nextVisible != visible)
            {
                visible = Mathf.Min(total, nextVisible);
                textTarget.maxVisibleCharacters = visible;
            }
            yield return null;
        }

        textTarget.maxVisibleCharacters = total;
        _isTyping = false;

        if (nextUIButton && enableNextWhenFinished)
            nextUIButton.interactable = true;

        _typingCo = null;
    }

    void SetTextImmediate(string content)
    {
        textTarget.text = content ?? "";
        textTarget.maxVisibleCharacters = int.MaxValue;
    }
}
