using UnityEngine;

public class Enemy : MonoBehaviour, IDamageHandler
{
    public int health = 50;
    public float moveSpeed = 2f;
    public float range = 2f;
    public Transform _playerTransform;

    private Transform _transform;
    private Rigidbody2D _rigidbody2D;

    private void Awake() {
        _transform = GetComponent<Transform>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        var diff = _playerTransform.position - _transform.position;
        if(diff.sqrMagnitude > range * range) {
            _rigidbody2D.velocity = diff.normalized * moveSpeed;
        }
        else {
            _rigidbody2D.velocity = Vector2.zero;
        }
    }

    public void HandleDamage(int damage) {
        health -= damage;
        if(health <= 0) {
            Destroy(this.gameObject);
        }
    }
}
