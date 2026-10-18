using System.Collections;
using UnityEngine;

public class Background : MonoBehaviour
{
    private float _speed;
    public float MaxSpeed;
    private Coroutine _accelerateCoroutine;

    private void Start()
    {
        LaunchManager.Instance.LaunchStarted += StartPan;
        TrashManager.Instance.ShipEmptied += EndPan;
    }

    private void StartPan()
    {
        _accelerateCoroutine = StartCoroutine(Accelerate());
        StartCoroutine(UpdatePositionCoroutine());
    }

    private void EndPan()
    {
        StartCoroutine(Decelerate());
    }

    private void UpdatePosition()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y - _speed, transform.position.z);
    }

    private IEnumerator Accelerate()
    {
        while (_speed < MaxSpeed)
        {
            yield return new WaitForSeconds(0.02f);
            _speed += 0.03f;
            UpdatePosition();
        }
    }

    private IEnumerator Decelerate()
    {
        StopCoroutine(_accelerateCoroutine);
         while (_speed > 0)
        {
            yield return new WaitForSeconds(0.02f);
            _speed -= 0.03f;
        }
        _speed = 0;
    }
    
    private IEnumerator UpdatePositionCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.02f);
            UpdatePosition();
        }
    }
}
