using TMPro;
using UnityEngine;

public class Speedometert : MonoBehaviour
{
    [Header("----Car details----")]
    [SerializeField] Rigidbody car;// get car forward vector magitude.
    [SerializeField] float maxSpeed;
    [Header("----Specific Values----")]
    [SerializeField] float arrowMinAngle;
    [SerializeField] float arrowMaxAngle;
    [Header("----UI elements----")]
    [SerializeField] TMP_Text speedLable;
    [SerializeField] RectTransform arrow;

    float speed;// conversion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(speedLable == null)
        {
            Debug.LogWarning(" Bro conect the flipn speed lable... igo man");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 3.6 is the conversion rate for KM/h
        speed = car.linearVelocity.magnitude * 3.5f;
        speedLable.text = (int)speed + "km/h";
        arrow.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(arrowMinAngle, arrowMaxAngle,speed /maxSpeed));
    }
}
