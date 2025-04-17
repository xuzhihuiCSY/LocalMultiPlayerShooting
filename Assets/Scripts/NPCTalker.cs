using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
public class NPCTalker : NetworkBehaviour
{
    [Header("Speech")]
    [TextArea] public string[] lines;
    public float talkInterval = 6f;

    [Header("Facing")]
    public float faceRange = 10f;
    public float turnSpeed = 5f;
    private Animation anim;
    private const string IdleClip = "idle";
    TMP_Text bubble;
    int lineIndex = -1;
    float faceCheckTimer;

    void Awake()
    {
        if (lines == null || lines.Length == 0)
        {
            lines = new[]
            {
                "Help me!!!!",
                "Kill the zombie first",
                "You have a gun, why are you running",
                "Your gun sucks",
                "Aim for the head!",
                "Just let them eat your head",
                "I'm not paying for this",
                "Behind you… just kidding",
                "Need ammo? Too bad."
            };
        }

        bubble = GetComponentInChildren<TMP_Text>(true);
        if (bubble != null) bubble.gameObject.SetActive(false);

        anim = GetComponentInChildren<Animation>();
    }
    public override void OnNetworkSpawn()
    {
        if (anim != null && anim.GetClip(IdleClip) != null)
            anim.Play(IdleClip);
        if (IsServer)
            StartCoroutine(SpeechLoop());
    }

    IEnumerator SpeechLoop()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            lineIndex = (lineIndex + 1) % lines.Length;
            ShowLineClientRpc(lineIndex);
            yield return new WaitForSeconds(talkInterval);
        }
    }

    [ClientRpc] void ShowLineClientRpc(int idx)
    {
        if (bubble == null) return;
        bubble.text = lines[idx];
        bubble.gameObject.SetActive(true);
    }
    void Update()
    {
        if (!IsServer) return;

        faceCheckTimer += Time.deltaTime;
        if (faceCheckTimer < 0.1f) return;
        faceCheckTimer = 0f;

        Transform p = FindClosestPlayer();
        if (p == null) return;

        if (Vector3.Distance(transform.position, p.position) > faceRange) return;

        Vector3 dir = (p.position - transform.position).normalized;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }

    Transform FindClosestPlayer()
    {
        var players = FindObjectsByType<NetworkPlayerController>(FindObjectsSortMode.None);
        float best = float.MaxValue;
        Transform bestT = null;

        foreach (var pl in players)
        {
            float d = Vector3.Distance(transform.position, pl.transform.position);
            if (d < best) { best = d; bestT = pl.transform; }
        }
        return bestT;
    }
}
