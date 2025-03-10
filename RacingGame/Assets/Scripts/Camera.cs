using UnityEngine;

public class Camera : MonoBehaviour
{
    public GameObject mapColor;
    public static bool Multiplayer = false;
    [Header("References")]
    [SerializeField]
    private Transform player;  // Reference to the player to follow
    [SerializeField]
    private Rigidbody carRigidBody;

    [Header("Camera Settings")]
    [SerializeField]
    private float distance = 5f;  // Distance from the player
    [SerializeField]
    private float height = 2f;    // Height above the player
    [SerializeField]
    private float rotationSpeed = 50;  // Speed of camera rotation with mouse input

    [Header("Mouse Sensitivity")]
    [SerializeField]
    private float sensitivityX = 500f; // Horizontal sensitivity
    [SerializeField]
    private float sensetivityY = 500f; // Vertical sensitivity

    [Header("Clamping Vertical Angle")]
    [SerializeField]
    private float minYAngle = -20f;  // Minimum vertical angle
    [SerializeField]
    private float maxYAngle = 60f;   // Maximum vertical angle
    [SerializeField]
    private float minXAngle = -60f;
    [SerializeField]
    private float maxXAngle = 60f;

    [SerializeField]
    private float minFollowSpeed = 2;
    [SerializeField]
    private float maxFollowSpeed = 3;
    [SerializeField]
    private float maxCarSpeed = 10;
    [SerializeField]
    private LayerMask collisionLayer; // LayerMask for objects that block the camera !USE IF NEEDED!

    private float currentDistance;
    private float currentX = 0f;  // Current horizontal rotation
    private float currentY = 0f;  // Current vertical rotation

    private Vector3 lastKnownForward;
    private Transform mainCamera;

    private Alteruna.Avatar avatar;


    void Start()
    {
        if (Multiplayer)
        {
            avatar = GetComponent<Alteruna.Avatar>();
            if (!avatar.IsMe)
                return;

        }
        mapColor.SetActive(false);
        carRigidBody = GetComponent<Rigidbody>();
        player = gameObject.transform.Find("body");
        mainCamera = GameObject.FindWithTag("MainCamera").transform;


        if (player == null)
        {
            Debug.LogError("Player object is not assigned!");
            return;
        }

        lastKnownForward = Vector3.forward;

        // Ensure the camera starts behind the player
        currentY = 10f; // Adjust this value for the desired vertical angle
        currentX = player.eulerAngles.y; // Align the horizontal angle with the player's rotation

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 anchorPosition = player.position;

        // Desired initial position
        Vector3 startPosition = anchorPosition - (rotation * Vector3.forward * distance) + Vector3.up * height;

        // Set the camera's position and rotation
        mainCamera.position = startPosition;

        currentDistance = distance; // Initialize current distance
    }

    void Update()
    {
        if (Multiplayer)
        {
            if (!avatar.IsMe)
                return;
        }

        if (player == null || carRigidBody == null)
        {
            Debug.LogError("Player object is not assigned!");
            return;
        }
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void FixedUpdate()
    {
        if (Multiplayer)
        {
            if (!avatar.IsMe)
                return;
        }
        if (player == null || carRigidBody == null)
        {
            Debug.LogError("Player object is not assigned!");
            return;
        }

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensetivityY * Time.deltaTime;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            currentX += mouseX;
            currentY -= mouseY;
        }

        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
        currentX = Mathf.Clamp(currentX, minXAngle, maxXAngle);

        Vector3 carVelocity = carRigidBody.linearVelocity;
        float carSpeed = carVelocity.magnitude; // Get car speed

        // If speed is above a small threshold, update forward direction
        if (carSpeed > 0.5f) // 0.5f prevents jitter when stopped
        {
            lastKnownForward = carVelocity.normalized;
        }

        // Use last known direction if car is too slow
        Vector3 carForward = lastKnownForward;

        // Get the correct Y-rotation from the movement direction
        float carYRotation = Mathf.Atan2(carForward.x, carForward.z) * Mathf.Rad2Deg;

        // Apply rotation with mouse input
        Quaternion rotation = Quaternion.Euler(currentY, carYRotation + currentX, 0);
        Vector3 anchorPosition = player.position;

        // Desired position
        Vector3 desiredPosition = anchorPosition - (rotation * Vector3.forward * distance) + Vector3.up * height;

        Vector3 rayDirection = (mainCamera.position - (anchorPosition + Vector3.up * height)).normalized;
        float rayDistance = Vector3.Distance(anchorPosition + Vector3.up * height, mainCamera.position);

        RaycastHit hit;
        if (Physics.Raycast(anchorPosition + Vector3.up * height, rayDirection, out hit, rayDistance, collisionLayer))
        {
            currentDistance = Mathf.Lerp(currentDistance, hit.distance, Time.deltaTime * 10);
            Debug.DrawLine(anchorPosition + Vector3.up * height, mainCamera.position, Color.blue);
            SetPlayerMaterialOpacity(0.3f);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * 10);
            SetPlayerMaterialOpacity(1f);
        }

        // Calculate follow speed based on car speed
        float speedFactor = Mathf.Clamp01(carRigidBody.linearVelocity.magnitude / maxCarSpeed);
        float smoothFollowSpeed = Mathf.Lerp(minFollowSpeed, maxFollowSpeed, speedFactor);

        // Smoothly move camera to desired position
        mainCamera.position = Vector3.Lerp(mainCamera.position, desiredPosition, Time.deltaTime * smoothFollowSpeed);

        Quaternion lookAtRotation = Quaternion.LookRotation((player.position + Vector3.up * height * 0.5f) - mainCamera.position);
        mainCamera.rotation = Quaternion.Lerp(mainCamera.rotation, lookAtRotation, Time.deltaTime * rotationSpeed);
    }

    // Function to adjust the player's material opacity
    void SetPlayerMaterialOpacity(float targetOpacity)
    {
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>(); // Get all renderers in child objects
        if (renderers.Length > 0)
        {
            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.materials) // Loop through all materials
                {
                    if (mat.HasProperty("_Color")) // Ensure the material has a _Color property
                    {
                        Color color = mat.color;
                        color.a = Mathf.Lerp(color.a, targetOpacity, Time.deltaTime);
                        mat.color = color;

                        if (targetOpacity < 1) // Enable transparency mode
                        {
                            if (mat.shader.name.Contains("Universal Render Pipeline/Lit"))
                            {
                                mat.SetFloat("_Surface", 1); // Set Surface Type to Transparent
                                mat.SetInt("_Blend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                                mat.SetInt("_ZWrite", 0);
                                mat.DisableKeyword("_ALPHATEST_ON");
                                mat.EnableKeyword("_ALPHABLEND_ON");
                                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                                mat.renderQueue = 3000; // Transparent queue
                            }
                        }
                        else if (color.a > .95f) // Reset to Opaque mode
                        {
                            if (mat.shader.name.Contains("Universal Render Pipeline/Lit"))
                            {
                                mat.SetFloat("_Surface", 0); // Set Surface Type to Opaque
                                mat.SetInt("_Blend", (int)UnityEngine.Rendering.BlendMode.One);
                                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                                mat.SetInt("_ZWrite", 1);
                                mat.EnableKeyword("_ALPHATEST_ON");
                                mat.DisableKeyword("_ALPHABLEND_ON");
                                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                                mat.renderQueue = -1; // Default queue
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Material '{mat.name}' does not have a _Color property.");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No renderers found on player.");
        }
    }

    //private void OnDrawGizmos()
    //{
    //    // Visualize CheckSphere in the editor
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(mainCamera.position, 0.3f);
    //}
}
