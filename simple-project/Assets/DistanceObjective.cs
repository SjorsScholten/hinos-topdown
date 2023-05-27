using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceObjective : MonoBehaviour
{
    public Collider2D player;
    public Transform marker;
    public float minDistance;

    private bool _withinRange;

    private readonly Color _green = new Color(0, 1, 0, 0.5f);
    private readonly Color _red = new Color(1, 0, 0, 0.5f);

    private void Update() {
        var diff = Vector2.Distance(player.ClosestPoint(marker.position), marker.position);
        _withinRange = diff < minDistance;
    }

    private void OnDrawGizmos() {
        Gizmos.color = (_withinRange) ? _green : _red;
        Gizmos.DrawSphere(marker.position, minDistance);
    }
}
