using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[AddComponentMenu("UI/Popup Manager (Single Toggle, Multi Popups)")]
public class PopupManager : MonoBehaviour
{
    [Header("팝업 루트들 (5개 등)")]
    [Tooltip("각 팝업의 루트 GameObject를 순서대로 넣으세요. (활성/비활성 전환 대상)")]
    public List<GameObject> popupRoots = new List<GameObject>();

    [Header("입력")]
    [Tooltip("현재 팝업 열고/닫기 토글 InputAction")]
    public InputActionReference toggleAction;

    [Header("표시 규칙")]
    [Tooltip("팝업을 처음 열 때 TypewriterPagedText가 있으면 0페이지부터 다시 시작")]
    public bool restartTypewriterOnShow = true;

    [Tooltip("팝업을 열 때 나머지 팝업은 모두 비활성화")]
    public bool deactivateOthersOnShow = true;

    [Tooltip("시작 시 모든 팝업을 비활성화")]
    public bool hideAllOnStart = true;

    [Header("현재 미션 인덱스")]
    [Tooltip("현재 미션 팝업 인덱스 (0 기반). -1이면 토글 입력 무시")]
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

    /// <summary>현재 인덱스의 팝업을 토글</summary>
    public void ToggleCurrent()
    {
        if (!IsValidIndex(currentIndex)) return;

        var go = popupRoots[currentIndex];
        bool willShow = !go.activeSelf;

        if (willShow) ShowOnly(currentIndex);
        else go.SetActive(false);
    }

    /// <summary>해당 인덱스만 활성화(필요 시 나머지 비활성화)</summary>
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

                // 열기
                go.SetActive(true);

                // 열릴 때 Typewriter 초기화 옵션
                if (restartTypewriterOnShow)
                {
                    var tw = go.GetComponentInChildren<TypewriterPagedText>(true);
                    if (tw != null)
                    {
                        // 비활성→활성 직후 StartPage(0) 보장
                        tw.SetPages(tw.pages, restart: true);
                    }
                }
            }
        }
    }

    /// <summary>모든 팝업 비활성화</summary>
    public void CloseAll()
    {
        foreach (var go in popupRoots)
            if (go) go.SetActive(false);
    }

    /// <summary>현재 인덱스를 변경 (보여주지는 않음)</summary>
    public void SetCurrentIndex(int index)
    {
        if (!IsValidIndex(index)) return;
        currentIndex = index;
    }

    /// <summary>해당 인덱스를 현재로 설정하고 즉시 보여줌</summary>
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
