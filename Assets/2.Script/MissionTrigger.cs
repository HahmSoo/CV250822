using UnityEngine;

[AddComponentMenu("Mission/Mission Trigger (ID)")]
public class MissionTrigger : MonoBehaviour
{
    [Tooltip("�� Ʈ������ ���� ID (��: 'Bridge', 'EngineRoom', 'Deck')")]
    public string triggerId = "Step1";

    [Tooltip("�� �� �ߵ� �� �ڵ� ��Ȱ��ȭ")]
    public bool oneShot = true;

    [Tooltip("�÷��̾�� �ν��� �±�")]
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
            if (_col) _col.enabled = false;       // �浹�� ���ų�
            // gameObject.SetActive(false);      // ������Ʈ ��ü�� ���� ��(���ϸ� ����)
        }
    }
}
