using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerCamera;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -15f;

    [Header("Mouse")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float pitchMin = -80f;
    [SerializeField] private float pitchMax = 80f;

    private float velocityY;
    private float pitch;

    private void Reset()
    {
        // References
        characterController = GetComponent<CharacterController>();
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;
    }
    void Start()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (PlayerInteraction.IsDialogueOpen)
            return;
            
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 moveDir = forward * v + right * h;
        if (moveDir.sqrMagnitude > 0.0001f)
            moveDir.Normalize();

        if (characterController.isGrounded && velocityY < 0f)
            velocityY = -2f;
        else
            velocityY += gravity * Time.deltaTime;

        Vector3 motion = moveDir * moveSpeed * Time.deltaTime;
        motion.y = velocityY * Time.deltaTime;

        characterController.Move(motion);

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(0f, mouseX, 0f);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        if (playerCamera != null)
            playerCamera.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }
}
