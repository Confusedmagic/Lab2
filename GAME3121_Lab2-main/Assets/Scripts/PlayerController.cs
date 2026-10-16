using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private float movementX;
    private float movementY;
    public float speed = 5.0f;
    private int count;
    public TextMeshProUGUI countText;
    public GameObject winText;

    [Header("Audio")]
    public AudioClip wallSound;   // boundary wall hit
    public AudioClip winSound;    // played once on win
    public AudioClip deathSound;  // played when destroyed by enemy
    public AudioClip pickupSound;
    private bool hasWon = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        count = 0;
        rb = GetComponent<Rigidbody>();
        SetCountText();
        winText.gameObject.SetActive(false);
    }
    void OnMove(InputValue value)
    {
        Vector2 inputVector = value.Get<Vector2>();
        movementX = inputVector.x;
        movementY = inputVector.y;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 movementVector = new Vector3(movementX, 0.0f, movementY);
        rb.AddForce(movementVector * speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PickUp"))
        {
            // Play pickup sound
            PlaySound(pickupSound);

            // Collect pickup
            other.gameObject.SetActive(false);

            count++;
            SetCountText();
        }
    }

    void SetCountText()
    {
        countText.text = "Count: " +count.ToString();
        if (count >= 12 && !hasWon)
        {
            hasWon = true;
            winText.gameObject.SetActive(true);
            PlaySound(winSound);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            PlaySound(wallSound);
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Play through SoundManager (not on the Player) so the sound survives Destroy.
            PlaySound(deathSound);
            Destroy(gameObject);
            winText.gameObject.SetActive(true);
            winText.GetComponent<TextMeshProUGUI>().text = "You Lose!";
        }
    }
    void PlaySound(AudioClip clip)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.Play(clip);
    }
}
