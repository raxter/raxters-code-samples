using System;
using TriInspector;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField]
    private string playerTag = "Player";

    [Header("References")]
    [ShowInInspector, ReadOnly]
    private GameObject player;

    public void Init()
    {
        // Find the player object by tag
        player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogError($"Player with tag '{playerTag}' not found.");
            return;
        }
    }

    void FixedUpdate()
    {
        if (player != null)
            transform.position = player.transform.position;
    }
}
