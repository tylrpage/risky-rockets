using UnityEngine;

public class Trash : MonoBehaviour
{
    [SerializeField] private int FuelValue;

    public bool Dropped;
    public void Eject()
    {
        ScoreManager.Instance.Score += FuelValue;
        PolygonCollider2D polygonCollider2D = gameObject.GetComponent<PolygonCollider2D>();
        polygonCollider2D.enabled = false;
        Destroy(gameObject, 1.5f);
    }
}
