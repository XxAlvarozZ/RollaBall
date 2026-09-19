using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private AudioSource audioSource;

    private int count;

    private float movementX;
    private float movementY;

    public float speed = 0;

    public TextMeshProUGUI countText;
    public GameObject winTextObject;

    [Header("Música y Pistas de Audio (AudioSource)")]
    public AudioSource backgroundMusic;  // Referencia al AudioSource del ambiente
    public AudioSource winLoopSound;     // Referencia al AudioSource de la música en loop de victoria

    [Header("Efectos de Sonido (AudioClip)")]
    public AudioClip bounceSound;        // Sonido al rebotar contra la pared
    [Tooltip("Tiempo en segundos a saltar al inicio del audio de choque")]
    public float bounceOffset = 0f;

    public AudioClip pickupSound;        // Sonido al recoger un Pickup
    [Tooltip("Tiempo en segundos a saltar al inicio del audio de pickup")]
    public float pickupOffset = 0f;

    [Header("Sonidos de Victoria")]
    public AudioClip winSound;           // Jingle o sonido inicial de victoria (No loop)
    [Tooltip("Tiempo en segundos a saltar al inicio del audio de victoria inicial")]
    public float winSoundOffset = 0f;
    [Range(0f, 3f), Tooltip("Multiplicador de volumen de victoria (1 = Normal, 2 = Doble)")]
    public float winSoundVolume = 1.5f;

    [Header("Sonidos de Derrota")]
    public AudioClip deathSound;         // Sonido instantáneo al morir
    [Range(0f, 3f), Tooltip("Multiplicador de volumen de muerte (1 = Normal, 2 = Doble)")]
    public float deathSoundVolume = 1.5f;
    public AudioClip gameOverLoopSound;  // Sonido/Música en loop cuando aparece "You Lose!"

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        // Garantizar que la música de victoria no suene al empezar
        if (winLoopSound != null)
        {
            winLoopSound.Stop();
        }

        count = 0;

        SetCountText();

        winTextObject.SetActive(false);
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

            SetCountText();

            // Reproduce el sonido de recolección aplicando el desfase
            if (audioSource != null && pickupSound != null)
            {
                audioSource.clip = pickupSound;
                audioSource.time = Mathf.Clamp(pickupOffset, 0f, pickupSound.length - 0.01f);
                audioSource.Play();
            }
        }
    }

    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();

        if (count >= 12)
        {
            // 1. Detener la música de fondo
            if (backgroundMusic != null)
            {
                backgroundMusic.Stop();
            }

            winTextObject.SetActive(true);
            winTextObject.GetComponent<TextMeshProUGUI>().text = "You Win!";

            // 2. Iniciar la secuencia de audio de victoria (Jingle inicial en 2D -> Loop)
            StartCoroutine(PlayWinAudioSequence());

            Destroy(GameObject.FindGameObjectWithTag("Enemy"));
        }
    }

    private IEnumerator PlayWinAudioSequence()
    {
        AudioSource winAudio = winTextObject.GetComponent<AudioSource>();

        // Reproducir el sonido sin loop en 2D con su volumen independiente
        if (winSound != null)
        {
            if (winAudio != null)
            {
                winAudio.clip = winSound;
                winAudio.volume = winSoundVolume;
                winAudio.time = Mathf.Clamp(winSoundOffset, 0f, winSound.length - 0.01f);
                winAudio.Play();
            }

            // Esperar la duración restante del sonido antes de arrancar el loop
            float duration = winSound.length - winSoundOffset;
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }
        }

        // Una vez terminado el audio inicial, iniciar la música en bucle
        if (winLoopSound != null)
        {
            winLoopSound.Play();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Sonido de rebote con la pared aplicando el desfase
        if (collision.gameObject.CompareTag("Pared"))
        {
            if (audioSource != null && bounceSound != null)
            {
                audioSource.clip = bounceSound;
                audioSource.time = Mathf.Clamp(bounceOffset, 0f, bounceSound.length - 0.01f);
                audioSource.Play();
            }
        }

        // Impacto contra el enemigo (Game Over)
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Detener la música de fondo al perder
            if (backgroundMusic != null)
            {
                backgroundMusic.Stop();
            }

            // 1. Activar la pantalla de derrota
            winTextObject.SetActive(true);
            winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";

            // 2. Reproducir el sonido de muerte en 2D a través del AudioSource de winTextObject
            AudioSource gameOverAudio = winTextObject.GetComponent<AudioSource>();
            if (gameOverAudio != null)
            {
                if (deathSound != null)
                {
                    gameOverAudio.PlayOneShot(deathSound, deathSoundVolume);
                }

                if (gameOverLoopSound != null)
                {
                    gameOverAudio.clip = gameOverLoopSound;
                    gameOverAudio.loop = true;
                    // Programar el loop para que empiece justo cuando termine el sonido de muerte
                    float delay = deathSound != null ? deathSound.length : 0f;
                    gameOverAudio.PlayScheduled(AudioSettings.dspTime + delay);
                }
            }

            // Destruir la bola
            Destroy(gameObject);
        }
    }
}