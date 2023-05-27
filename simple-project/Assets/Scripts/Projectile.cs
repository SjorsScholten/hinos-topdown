using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifeTime = 5f;
    public float moveSpeed = 21f;
    public float radius = 0.3f;

    public ParticleSystem collision_vfx;

    private float _lifeTime;

    private Transform _transform;

    public event System.Action<GameObject> OnHitEvent;

    private void Awake() {
        _transform = GetComponent<Transform>();
    }

    private void Update() {
        _lifeTime += Time.deltaTime;
        if(_lifeTime > lifeTime) {
            Destroy(this.gameObject);
            return;
        }

        var heading = (Vector2)_transform.up;
        var displacement = moveSpeed * Time.deltaTime;
        var velocity = heading * displacement;

        var hitInfo = new RaycastHit2D[3];
        var hits = Physics2D.CircleCastNonAlloc(_transform.position, radius, heading, hitInfo, displacement);
        for(var i = 0; i < hits; i++) {
            if (ProcessHit(hitInfo[i])) return;
        }

        _transform.position += (Vector3)velocity;
    }

    private void OnDrawGizmos() {
        Gizmos.color = new Color(0, 1f, 0, 0.4f);
        Gizmos.DrawSphere(transform.position, radius);
    }

    private bool ProcessHit(RaycastHit2D hitInfo) {
        if (hitInfo.collider.CompareTag(tag)) return false; //Cannot hit object with same tag
        OnHitEvent?.Invoke(hitInfo.collider.gameObject);
        Destroy(this.gameObject);
        return true;
    }

    private IEnumerator DestroyByCollisionRoutine() {
        this.gameObject.SetActive(false);
        collision_vfx.Play();
       
        yield return new WaitForSeconds(collision_vfx.main.duration);
        
        Destroy(this.gameObject);
    }
}

public interface IDamageHandler
{
    void HandleDamage(int damage);
}