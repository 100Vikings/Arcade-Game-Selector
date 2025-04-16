using UnityEngine;

public class Comet : MonoBehaviour
{
    private Vector3 direction;
    private float speed;

    public void Initialize(Vector3 direction, float speed, float lifetime = 5f)
    {
        this.direction = direction.normalized;
        this.speed = speed;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += speed * Time.deltaTime * direction;
    }
}