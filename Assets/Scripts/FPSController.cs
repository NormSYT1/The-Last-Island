using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 7f;
    public float runSpeed = 12f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Key Bindings")]
    [Tooltip("İleri gitme tuşu (W yerine 2)")]
    public KeyCode forwardKey = KeyCode.Alpha2;
    public KeyCode forwardKeypad = KeyCode.Keypad2;
    public KeyCode backwardKey = KeyCode.S;

    [Header("Look Settings")]
    public Transform cameraHolder;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;

    [Header("Cleaning & Interaction")]
    public float cleanDistance = 7.5f;
    public float colliderInteractionRadius = 4.0f; // Collider alanında algılama yarıçapı
    public ParticleSystem waterSprayFX;            // Oyuncudan çıkan su jeti efekti
    public KeyCode plantKey = KeyCode.F;           // Fidan dikme tuşu (F)

    private CharacterController controller;
    private float verticalVelocity;
    private float pitch = 0f;

    public bool isTargetingCleanable { get; private set; } = false;
    public EcoMissionZone activeZoneInReach { get; private set; } = null;
    public PlantingSpot activePlantingSpot { get; private set; } = null;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Oyun bittiğinde veya duraklatıldığında hareket ve kamera tamamen durdurulur
        bool isBlocked = (EcoMissionGameManager.Instance != null && EcoMissionGameManager.Instance.isGameOver) ||
                         (EcoMissionCanvasUI.Instance != null && EcoMissionCanvasUI.Instance.isPaused);

        if (isBlocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        HandleMouseLook();
        HandleMovement();
        HandleCleaningInteraction();
        HandleCursorToggle();
    }

    private void HandleCursorToggle()
    {
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            // UI üzerine tıklanmadıysa ve oyun duraklatılmadıysa kilitle
            if (EcoMissionCanvasUI.Instance != null && EcoMissionCanvasUI.Instance.isPaused) return;
            if (EcoMissionGameManager.Instance != null && EcoMissionGameManager.Instance.isGameOver) return;

            if (UnityEngine.EventSystems.EventSystem.current == null ||
                !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
        if (cameraHolder != null)
        {
            cameraHolder.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");

        // İleri gitme: W yerine 2 tuşu (Alpha2 ve Keypad2)
        float moveZ = 0f;
        if (Input.GetKey(forwardKey) || Input.GetKey(forwardKeypad) || Input.GetKey(KeyCode.UpArrow))
        {
            moveZ += 1f;
        }
        if (Input.GetKey(backwardKey) || Input.GetKey(KeyCode.DownArrow))
        {
            moveZ -= 1f;
        }

        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
        if (moveDirection.magnitude > 1f) moveDirection.Normalize();

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private void HandleCleaningInteraction()
    {
        activeZoneInReach = null;
        activePlantingSpot = null;
        isTargetingCleanable = false;

        // 1. COLLIDER / ALAN ETKİLEŞİMİ (Oyuncu kirlilik veya fidan alanının içine girince)
        Collider[] overlaps = Physics.OverlapSphere(transform.position, colliderInteractionRadius, ~0, QueryTriggerInteraction.Collide);
        foreach (var col in overlaps)
        {
            var zone = col.GetComponentInParent<EcoMissionZone>();
            if (zone != null && !zone.isCleaned)
            {
                activeZoneInReach = zone;
                isTargetingCleanable = true;
            }

            var spot = col.GetComponentInParent<PlantingSpot>();
            if (spot != null && !spot.isPlanted)
            {
                activePlantingSpot = spot;
                isTargetingCleanable = true;
            }

            if (activeZoneInReach != null && activePlantingSpot != null) break;
        }

        // 2. RAYCAST YEDEK ETKİLEŞİMİ (Eğer alanın içinde değilse ama uzaktan bakıyorsa)
        if (cameraHolder != null)
        {
            Ray ray = new Ray(cameraHolder.position, cameraHolder.forward);
            if (Physics.Raycast(ray, out RaycastHit checkHit, cleanDistance, ~0, QueryTriggerInteraction.Collide))
            {
                if (activeZoneInReach == null)
                {
                    var missionZone = checkHit.collider.GetComponentInParent<EcoMissionZone>();
                    if (missionZone != null && !missionZone.isCleaned)
                    {
                        activeZoneInReach = missionZone;
                        isTargetingCleanable = true;
                    }
                }

                if (activePlantingSpot == null)
                {
                    var spot = checkHit.collider.GetComponentInParent<PlantingSpot>();
                    if (spot != null && !spot.isPlanted)
                    {
                        activePlantingSpot = spot;
                        isTargetingCleanable = true;
                    }
                }
            }
        }

        // 3. F TUŞU İLE FİDAN DİKME
        if (Input.GetKeyDown(plantKey))
        {
            if (activePlantingSpot != null && !activePlantingSpot.isPlanted)
            {
                activePlantingSpot.PlantSapling();
                activePlantingSpot = null;
                isTargetingCleanable = false;
                return;
            }
        }

        // 4. E TUŞU veya SOL TIK İLE TEMİZLEME / SÖNDÜRME
        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
        {
            if (activeZoneInReach != null)
            {
                // Sadece yangın söndürülürken su sık (çöp toplarken sıkma)
                bool isFireZone = activeZoneInReach.fireEffect != null || activeZoneInReach.waterSplashEffect != null;
                if (isFireZone && waterSprayFX != null)
                {
                    waterSprayFX.Stop();
                    waterSprayFX.Play();
                }

                activeZoneInReach.CleanZone();
                activeZoneInReach = null;
                isTargetingCleanable = false;
                return;
            }
        }
    }
}
