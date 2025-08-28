using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[AddComponentMenu("UI/Popup Manager (Single Toggle, Multi Popups)")]
public class PopupManager : MonoBehaviour
{
    [Header("�˾� ��Ʈ�� (5�� ��)")]
    [Tooltip("�� �˾��� ��Ʈ GameObject�� ������� ��������. (Ȱ��/��Ȱ�� ��ȯ ���)")]
    public List<GameObject> popupRoots = new List<GameObject>();

    [Header("�Է�")]
    [Tooltip("���� �˾� ����/�ݱ� ��� InputAction")]
    public InputActionReference toggleAction;

    [Header("ǥ�� ��Ģ")]
    [Tooltip("�˾��� ó�� �� �� TypewriterPagedText�� ������ 0���������� �ٽ� ����")]
    public bool restartTypewriterOnShow = true;

    [Tooltip("�˾��� �� �� ������ �˾��� ��� ��Ȱ��ȭ")]
    public bool deactivateOthersOnShow = true;

    [Tooltip("���� �� ��� �˾��� ��Ȱ��ȭ")]
    public bool hideAllOnStart = true;

    [Header("���� �̼� �ε���")]
    [Tooltip("���� �̼� �˾� �ε��� (0 ���). -1�̸� ��� �Է� ����")]
    public int currentIndex = -1;

    void Awake()
    {
        if (hideAllOnStart) CloseAll();
    }

    void OnEnable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.performed += OnTogglePerformed;
            toggleAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.performed -= OnTogglePerformed;
            toggleAction.action.Disable();
        }
    }

    void OnTogglePerformed(InputAction.CallbackContext _)
    {
        ToggleCurrent();
    }

    /// <summary>���� �ε����� �˾��� ���</summary>
    public void ToggleCurrent()
    {
        if (!IsValidIndex(currentIndex)) return;

        var go = popupRoots[currentIndex];
        bool willShow = !go.activeSelf;

        if (willShow) ShowOnly(currentIndex);
        else go.SetActive(false);
    }

    /// <summary>�ش� �ε����� Ȱ��ȭ(�ʿ� �� ������ ��Ȱ��ȭ)</summary>
    public void ShowOnly(int index)
    {
        if (!IsValidIndex(index)) return;
        currentIndex = index;

        for (int i = 0; i < popupRoots.Count; i++)
        {
            var go = popupRoots[i];
            if (go == null) continue;

            bool shouldActive = (i == index);
            if (shouldActive)
            {
                if (deactivateOthersOnShow)
                    DeactivateAllExcept(i);

                // ����
                go.SetActive(true);

                // ���� �� Typewriter �ʱ�ȭ �ɼ�
                if (restartTypewriterOnShow)
                {
                    var tw = go.GetComponentInChildren<TypewriterPagedText>(true);
                    if (tw != null)
                    {
                        // ��Ȱ����Ȱ�� ���� StartPage(0) ����
                        tw.SetPages(tw.pages, restart: true);
                    }
                }
            }
        }
    }

    /// <summary>��� �˾� ��Ȱ��ȭ</summary>
    public void CloseAll()
    {
        foreach (var go in popupRoots)
            if (go) go.SetActive(false);
    }

    /// <summary>���� �ε����� ���� (���������� ����)</summary>
    public void SetCurrentIndex(int index)
    {
        if (!IsValidIndex(index)) return;
        currentIndex = index;
    }

    /// <summary>�ش� �ε����� ����� �����ϰ� ��� ������</summary>
    public void SetCurrentAndShow(int index)
    {
        currentIndex = index;
        ShowOnly(index);
    }

    void DeactivateAllExcept(int except)
    {
        for (int i = 0; i < popupRoots.Count; i++)
        {
            if (i == except) continue;
            var go = popupRoots[i];
            if (go) go.SetActive(false);
        }
    }

    bool IsValidIndex(int idx)
    {
        return popupRoots != null && idx >= 0 && idx < popupRoots.Count && popupRoots[idx] != null;
    }
}
