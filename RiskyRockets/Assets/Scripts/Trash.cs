using UnityEngine;

public class Trash : MonoBehaviour
{
    [field: SerializeField] public int FuelValue { get; private set; }

    public bool Dropped;
    public void Eject()
    {
        ScoreManager.Instance.Score += FuelValue;
        PolygonCollider2D polygonCollider2D = gameObject.GetComponent<PolygonCollider2D>();
        polygonCollider2D.enabled = false;
        Rigidbody2D rigidbody2d = gameObject.GetComponent<Rigidbody2D>();
        rigidbody2d.linearVelocity = new Vector2(0, -80);
        Destroy(gameObject, 1.5f);
    }
}
