using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    float speed = 5f;


    private Vector3 moveVector;
    
    void OnMove(InputValue value)
    {
        Vector2 inputVector = value.Get<Vector2>();
        Debug.Log($"Input Vector: {inputVector}");
        moveVector = new Vector3(inputVector.x, 0, inputVector.y);
    }

    void Update()
    {
        transform.position += moveVector * (Time.deltaTime * speed);
    }
}
