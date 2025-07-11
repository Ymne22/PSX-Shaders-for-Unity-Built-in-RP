using UnityEngine;

[System.Serializable]
public class MaterialSound
{
    public PhysicsMaterial surfaceMaterial;
    public AudioClip[] footstepSounds;
    public AudioClip[] landingSounds;
}

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Camera")]
    public Camera playerCamera;

    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float runSpeed = 10.0f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Crouch Settings")]
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float crouchTransitionSpeed = 10.0f;

    [Header("Look Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2.0f;

    [Header("Field of View (FOV)")]
    [SerializeField] private float runningFov = 75f;
    [SerializeField] private float fovTransitionSpeed = 10f;
    
    [Header("Head Bob Settings")]
    [SerializeField] private bool enableHeadbob = true;
    [SerializeField] private float walkBobFrequency = 1.5f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float runBobFrequency = 2.5f;
    [SerializeField] private float runBobAmount = 0.1f;
    [SerializeField] private float bobResetSpeed = 15.0f;

    [Header("Footstep Sounds")]
    [SerializeField] private float footstepIntervalWalk = 0.5f;
    [SerializeField] private float footstepIntervalRun = 0.3f;
    [SerializeField] private AudioClip[] defaultFootstepSounds;
    [SerializeField] private MaterialSound[] materialSounds;

    [Header("Action Sounds")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip crouchSound;
    [SerializeField] private AudioClip uncrouchSound;
    [SerializeField] private AudioClip[] defaultLandingSounds;

    private CharacterController characterController;
    private AudioSource audioSource;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isCrouching = false;
    private float cameraPitch = 0.0f;

    private float defaultFov;
    private Vector3 cameraDefaultPosition;
    private float bobTimer = 0f;
    private float footstepTimer = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        defaultFov = playerCamera.fieldOfView;
        cameraDefaultPosition = playerCamera.transform.localPosition;
        wasGrounded = true;
    }

    void Update()
    {
        isGrounded = characterController.isGrounded;

        HandleLanding();

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        HandleMouseLook();
        HandleCrouch();
        HandleFov();
        if (enableHeadbob)
        {
            HandleHeadbob();
        }

        HandleMovementAndJump();

        wasGrounded = isGrounded;
    }

    private void HandleMovementAndJump()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        bool isMoving = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f;
        bool isRunning = Input.GetButton("Run") && !isCrouching && isGrounded;
        float currentSpeed = isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);

        Vector3 horizontalMove = (transform.right * moveX + transform.forward * moveZ).normalized * currentSpeed;

        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
        }

        playerVelocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = horizontalMove + Vector3.up * playerVelocity.y;
        
        characterController.Move(finalMove * Time.deltaTime);

        if (isGrounded && isMoving)
        {
            HandleFootsteps(isRunning);
        }
    }

    private void HandleLanding()
    {
        if (!wasGrounded && isGrounded)
        {
            PlayLandingSound();
        }
    }

    private void HandleFootsteps(bool isRunning)
    {
        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0)
        {
            PlayFootstepSound();
            footstepTimer = isRunning ? footstepIntervalRun : footstepIntervalWalk;
        }
    }

    private void PlayFootstepSound()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3f))
        {
            PlaySurfaceSound(hit.collider.sharedMaterial, defaultFootstepSounds, (m) => m.footstepSounds);
        }
    }

    private void PlayLandingSound()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3f))
        {
            PlaySurfaceSound(hit.collider.sharedMaterial, defaultLandingSounds, (m) => m.landingSounds);
        }
    }

    private void PlaySurfaceSound(PhysicsMaterial surfaceMaterial, AudioClip[] defaultClips, System.Func<MaterialSound, AudioClip[]> getClips)
    {
        bool soundPlayed = false;

        foreach (var matSound in materialSounds)
        {
            if (matSound.surfaceMaterial == surfaceMaterial)
            {
                AudioClip[] clips = getClips(matSound);
                if (clips != null && clips.Length > 0)
                {
                    AudioClip clip = clips[Random.Range(0, clips.Length)];
                    audioSource.PlayOneShot(clip);
                    soundPlayed = true;
                }
                break;
            }
        }

        if (!soundPlayed && defaultClips != null && defaultClips.Length > 0)
        {
            AudioClip clip = defaultClips[Random.Range(0, defaultClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }

    private void HandleCrouch()
    {
        if (Input.GetButtonDown("Crouch"))
        {
            isCrouching = !isCrouching;
            AudioClip soundToPlay = isCrouching ? crouchSound : uncrouchSound;
            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }

        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        characterController.height = Mathf.Lerp(characterController.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
    }

    private void HandleFov()
    {
        bool isRunning = Input.GetButton("Run") && !isCrouching && isGrounded;
        float targetFov = isRunning ? runningFov : defaultFov;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, fovTransitionSpeed * Time.deltaTime);
    }

    private void HandleHeadbob()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        bool isRunning = Input.GetButton("Run") && !isCrouching;

        Vector3 targetCameraPosition;
        Vector3 crouchTargetPosition = isCrouching ? 
            new Vector3(cameraDefaultPosition.x, cameraDefaultPosition.y - (standingHeight - crouchHeight), cameraDefaultPosition.z) : 
            cameraDefaultPosition;

        if (isGrounded && (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f))
        {
            float frequency = isRunning ? runBobFrequency : walkBobFrequency;
            float amount = isRunning ? runBobAmount : walkBobAmount;
            
            bobTimer += Time.deltaTime * frequency;
            
            float bobX = Mathf.Cos(bobTimer) * amount;
            float bobY = Mathf.Sin(bobTimer * 2) * amount;

            Vector3 bobOffset = new Vector3(bobX, bobY, 0);
            targetCameraPosition = crouchTargetPosition + bobOffset;
        }
        else
        {
            bobTimer = 0;
            targetCameraPosition = crouchTargetPosition;
        }

        playerCamera.transform.localPosition = Vector3.MoveTowards(
            playerCamera.transform.localPosition, 
            targetCameraPosition, 
            bobResetSpeed * Time.deltaTime
        );
    }
}
