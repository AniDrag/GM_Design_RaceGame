using UnityEngine;

public class Camera : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform player;  // Reference to the player to follow

    [Header("Camera Settings")]
    [SerializeField]
    private float distance = 5f;  // Distance from the player
    [SerializeField]
    private float height = 2f;    // Height above the player
    [SerializeField]
    private float rotationSpeed = 5f;  // Speed of camera rotation with mouse input

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
    private float followSpeed = 4f;
    [SerializeField]
    private LayerMask collisionLayer; // LayerMask for objects that block the camera !USE IF NEEDED!

    private float currentDistance;
    private float currentX = 0f;  // Current horizontal rotation
    private float currentY = 0f;  // Current vertical rotation

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player object is not assigned!");
            return;
        }

        // Ensure the camera starts behind the player
        currentY = 10f; // Adjust this value for the desired vertical angle
        currentX = player.eulerAngles.y; // Align the horizontal angle with the player's rotation

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 anchorPosition = player.position;

        // Desired initial position
        Vector3 startPosition = anchorPosition - (rotation * Vector3.forward * distance) + Vector3.up * height;

        // Set the camera's position and rotation
        transform.position = startPosition;
        transform.rotation = Quaternion.LookRotation((player.position + Vector3.up * height * 0.5f) - transform.position);

        currentDistance = distance; // Initialize current distance
    }

    void Update()
    {
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
        if (player == null) return;

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensetivityY * Time.deltaTime;

        float totalMouseX = mouseX;
        float totalMouseY = mouseY;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            currentX += totalMouseX;
            currentY -= totalMouseY;
        }

        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 anchorPosition = player.position;

        // Desired position
        Vector3 desiredPosition = anchorPosition - (rotation * Vector3.forward * distance) + Vector3.up * height;

        Vector3 rayDirection = (transform.position - (anchorPosition + Vector3.up * height)).normalized;
        float rayDistance = Vector3.Distance(anchorPosition + Vector3.up * height, transform.position);

        RaycastHit hit;
        if (Physics.Raycast(anchorPosition + Vector3.up * height, rayDirection, out hit, rayDistance, collisionLayer))
        {
            currentDistance = Mathf.Lerp(currentDistance, hit.distance, Time.deltaTime * 10);
            Debug.DrawLine(anchorPosition + Vector3.up * height, transform.position, Color.blue);
            SetPlayerMaterialOpacity(0.3f); // Reduce player opacity
        }
        else
        {
            // Restore camera distance and opacity
            currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * 10);
            SetPlayerMaterialOpacity(1f); // Restore full opacity
        }

        // Update camera position based on current distance
        desiredPosition = anchorPosition - (rotation * Vector3.forward * currentDistance) + Vector3.up * height;
        transform.position = desiredPosition;

        Quaternion lookAtRotation = Quaternion.LookRotation((player.position + Vector3.up * height * 0.5f) - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookAtRotation, Time.deltaTime * rotationSpeed);
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
                        color.a = Mathf.Lerp(color.a, targetOpacity, Time.deltaTime * followSpeed);
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

    private void OnDrawGizmos()
    {
        // Visualize CheckSphere in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
