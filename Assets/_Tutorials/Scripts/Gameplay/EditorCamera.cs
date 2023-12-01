using UnityEngine;

public class EditorCamera : MonoBehaviour
{
    [SerializeField] float mouseSensitivity = 600.0f;
    [SerializeField] float clampAngle = 80.0f;
 
    private float _rotY = 0.0f; // rotation around the up/y axis
    private float _rotX = 0.0f; // rotation around the right/x axis

    private Vector3 _initialRot;
    private bool _inControl;
 
    void Start ()
    {
        Cursor.lockState = CursorLockMode.Confined;
        var rot = _initialRot = transform.localRotation.eulerAngles;
        _rotY = rot.y;
        _rotX = rot.x;
    }
 
    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _inControl = !_inControl;
            Cursor.lockState = _inControl ? CursorLockMode.Locked : CursorLockMode.None;
        }
        
        if (!_inControl)
            return;
        
        var mouseX = Input.GetAxis("Mouse X");
        var mouseY = -Input.GetAxis("Mouse Y");
 
        _rotY += mouseX * mouseSensitivity * Time.deltaTime;
        _rotX += mouseY * mouseSensitivity * Time.deltaTime;
 
        _rotX = Mathf.Clamp(_rotX, -clampAngle, clampAngle);
 
        var localRotation = Quaternion.Euler(_rotX, _rotY, 0.0f);
        transform.rotation = localRotation;

        if (Input.GetKeyDown(KeyCode.Tilde))
        {
            transform.localRotation = Quaternion.Euler(_initialRot);
            _rotX = transform.localRotation.eulerAngles.x;
            _rotY = transform.localRotation.eulerAngles.y;
        }
    }
}
