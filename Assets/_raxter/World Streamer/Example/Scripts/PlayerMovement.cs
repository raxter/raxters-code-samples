using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    float speed = 5f;
    
    [SerializeField]
    CameraPerspectiveTricks cameraPerspectiveTricks;

    [SerializeField]
    Transform rotatorTransform;
    [SerializeField]
    Vector3 perspectiveRotation;
    [SerializeField]
    Vector3 orthographicRotation;
    
    bool inPerspectiveMode = false;
    

    private Vector3 moveVector;
    
    void OnMove(InputValue value)
    {
        Vector2 inputVector = value.Get<Vector2>();
        //Debug.Log($"Input Vector: {inputVector}");
        moveVector = new Vector3(inputVector.x, 0, inputVector.y);
    }

    void OnJump(InputValue value)
    {
        //Debug.Log($"Jump: {value}");
        inPerspectiveMode = !inPerspectiveMode;
        
    }

    void Update()
    {

        Vector3 viewMoveVector = 
            Quaternion.Euler(0, rotatorTransform.localEulerAngles.y, 0) * moveVector;;
        
        transform.position += viewMoveVector * (Time.deltaTime * speed);
        
        cameraPerspectiveTricks.LerpAmount =
            Mathf.SmoothStep(cameraPerspectiveTricks.LerpAmount, inPerspectiveMode ? 1 : 0, 5*Time.deltaTime);
        
        rotatorTransform.localEulerAngles = 
            Vector3.Slerp(perspectiveRotation, orthographicRotation, cameraPerspectiveTricks.LerpAmount*cameraPerspectiveTricks.LerpAmount);
    }
}
