using UnityEngine;

[ExecuteInEditMode]
public class CameraPerspectiveTricks : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    
    [Range(0,1)]
    [SerializeField] private float lerpAmount = 0;
    [SerializeField] private float fov = 90;
    [SerializeField] private float orthoSize = 10;

    
    static Matrix4x4 GenerateOrthographicMatrix(float orthoSize)
    {
        float width = Screen.width;
        float height = Screen.height;
        float aspect = width / height;
        
        float n = 0.1f;
        float f = 100.0f;
        
        float l = -orthoSize * aspect; // left
        float r = orthoSize * aspect; // right
        float t = orthoSize; // top
        float b = -orthoSize; // bottom

        Matrix4x4 orthographicMatrix = new Matrix4x4();
        orthographicMatrix[0, 0] =  2 / (r - l);
        orthographicMatrix[1, 1] =  2 / (t - b);
        orthographicMatrix[2, 2] = -2 / (f - n);   
        orthographicMatrix[0, 3] = -(r + l) / (r - l);
        orthographicMatrix[1, 3] = -(t + b) / (t - b);
        orthographicMatrix[2, 3] = -(f + n) / (f - n);
        orthographicMatrix[3, 3] =  1;
        return orthographicMatrix;
    }

    static Matrix4x4 GeneratePerspectiveMatrix(float fov_Deg)
    {
        float width = Screen.width;
        float height = Screen.height;
        float aspect = width / height;
        float near = 0.1f;
        float far = 100.0f;

        float f = 1.0f / Mathf.Tan(fov_Deg*Mathf.Deg2Rad * 0.5f);
        Matrix4x4 perspectiveMatrix = new Matrix4x4();
        perspectiveMatrix[0, 0] = f / aspect;
        perspectiveMatrix[1, 1] = f;
        perspectiveMatrix[2, 2] = (far + near) / (near - far);
        perspectiveMatrix[2, 3] = (2 * far * near) / (near - far);
        perspectiveMatrix[3, 2] = -1;
        perspectiveMatrix[3, 3] = 0;

        return perspectiveMatrix;
    }

    Matrix4x4 Matrix4x4_Lerp(Matrix4x4 a, Matrix4x4 b, float t)
    {
        Matrix4x4 result = new Matrix4x4();
        for (int i = 0; i < 16; i++)
            result[i] = Mathf.Lerp(a[i], b[i], t);
        return result;
    }
    
    void Update()
    {
        var perspectiveMatrix = GeneratePerspectiveMatrix(fov);
        var orthographicMatrix = GenerateOrthographicMatrix(orthoSize);
        var mixedMatrix = Matrix4x4_Lerp(perspectiveMatrix, orthographicMatrix, lerpAmount);
        targetCamera.projectionMatrix = mixedMatrix;
    }
    
}
