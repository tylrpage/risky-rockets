using System.Collections;
using UnityEngine;

public class Trash : MonoBehaviour
{
    [field: SerializeField] public int FuelValue { get; private set; }

    public bool Dropped;

    private Rigidbody2D _body;
    public Rigidbody2D Body => _body != null ? _body : (_body = GetComponent<Rigidbody2D>());

    // private void OnCollisionEnter2D(Collision2D collision)
    // {
    //     float speed = collision.relativeVelocity.magnitude;
    //     SoundEffectManager.Instance.PlayCollisionSFX(speed);
    // }

    public void PrepareForLaunch(float gravityScale)
    {
        if (Body == null) return;
        // Never sleep, so pieces resting on a burning piece always wake up and fall.
        Body.sleepMode = RigidbodySleepMode2D.NeverSleep;
        Body.gravityScale = gravityScale;
    }

    public void Freeze()
    {
        if (Body == null) return;
        Body.linearVelocity = Vector2.zero;
        Body.angularVelocity = 0f;
        Body.simulated = false;
    }

    public void Burn(float duration)
    {
        StartCoroutine(ProcessBurn(duration));
    }

    private IEnumerator ProcessBurn(float duration)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector3 startScale = transform.localScale;
        Color startColor = sr != null ? sr.color : Color.white;

        float t = 0f;
        while (t < duration)
        {
            float p = t / duration;
            transform.localScale = startScale * (1f - p);
            if (sr != null)
            {
                sr.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a * (1f - p));
            }
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
