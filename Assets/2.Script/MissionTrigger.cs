using UnityEngine;

[AddComponentMenu("Mission/Mission Trigger (ID)")]
public class MissionTrigger : MonoBehaviour
{
    [Tooltip("이 트리거의 고유 ID (예: 'Bridge', 'EngineRoom', 'Deck')")]
    public string triggerId = "Step1";

    [Tooltip("한 번 발동 후 자동 비활성화")]
    public bool oneShot = true;

    [Tooltip("플레이어로 인식할 태그")]
    public string playerTag = "Player";

    Collider _col;

    void Awake() { _col = GetComponent<Collider>(); if (_col) _col.isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        var gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null) gm.NotifyTriggerEntered(triggerId);

        if (oneShot)
        {
            if (_col) _col.enabled = false;       // 충돌만 끄거나
            // gameObject.SetActive(false);      // 오브젝트 자체를 꺼도 됨(원하면 이쪽)
        }
    }
}
