using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private int count;
    private float movementX;
    private float movementY;

    public float speed = 0;
    public TextMeshProUGUI countText;
    public GameObject winTextObject;

    [Header("Componente de Audio del Player")]
    public AudioSource audioSource; // El AudioSource de tu Player

    [Header("Clips de Sonido (Archivos .wav/.mp3)")]
    public AudioClip winClip;       // Sonido al ganar
    public AudioClip pickupClip;    // Sonido al tomar Pickups
    public AudioClip wallClip;      // Sonido al chocar con paredes
    public AudioClip loseClip;      // Sonido al perder con Enemy

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        SetCountText();
        winTextObject.SetActive(false);

        // Si no asignaste el AudioSource manualmente, intenta conseguirlo automáticamente
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);
        rb.AddForce(movement * speed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            other.gameObject.SetActive(false);
            count = count + 1;

            // Reproduce el sonido de agarrar ítem
            if (audioSource != null && pickupClip != null)
            {
                audioSource.PlayOneShot(pickupClip);
            }

            SetCountText();
        }
    }

    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();

        if (count >= 12)
        {
            winTextObject.SetActive(true);

            // Reproduce el sonido de victoria
            if (audioSource != null && winClip != null)
            {
                audioSource.PlayOneShot(winClip);
            }

            Destroy(GameObject.FindGameObjectWithTag("Enemy"));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Choque con pared (Tag Finish o el objeto Walls)
        if (collision.gameObject.CompareTag("Finish") || collision.gameObject.name.Contains("Wall"))
        {
            if (audioSource != null && wallClip != null)
            {
                audioSource.PlayOneShot(wallClip);
            }
        }

        // Choque con el Enemigo
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.name.Contains("Enemy"))
        {
            if (loseClip != null)
            {
                // Reproduce el sonido en la posición del jugador aunque este sea destruido
                AudioSource.PlayClipAtPoint(loseClip, transform.position);
            }

            winTextObject.gameObject.SetActive(true);
            winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";

            Destroy(gameObject);
        }
    }
}