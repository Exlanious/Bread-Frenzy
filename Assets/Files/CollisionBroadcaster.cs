using System;
using UnityEngine;

[DisallowMultipleComponent]
public class CollisionBroadcaster : MonoBehaviour
{
    public event Action<Collider> OnTriggerEnterEvent;
    public event Action<Collider> OnTriggerStayEvent;
    public event Action<Collider> OnTriggerExitEvent;
    public event Action<Collision> OnCollisionEnterEvent;
    public event Action<Collision> OnCollisionStayEvent;
    public event Action<Collision> OnCollisionExitEvent;
    public event Action<Collider2D> OnTriggerEnter2DEvent;
    public event Action<Collider2D> OnTriggerStay2DEvent;
    public event Action<Collider2D> OnTriggerExit2DEvent;
    public event Action<Collision2D> OnCollisionEnter2DEvent;
    public event Action<Collision2D> OnCollisionStay2DEvent;
    public event Action<Collision2D> OnCollisionExit2DEvent;

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterEvent?.Invoke(other);
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerStayEvent?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        OnTriggerExitEvent?.Invoke(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEnterEvent?.Invoke(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        OnCollisionStayEvent?.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        OnCollisionExitEvent?.Invoke(collision);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        OnTriggerEnter2DEvent?.Invoke(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        OnTriggerStay2DEvent?.Invoke(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        OnTriggerExit2DEvent?.Invoke(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionEnter2DEvent?.Invoke(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        OnCollisionStay2DEvent?.Invoke(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        OnCollisionExit2DEvent?.Invoke(collision);
    }
}
