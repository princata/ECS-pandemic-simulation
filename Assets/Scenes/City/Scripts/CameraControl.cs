using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    public float zoomSpeed;
    private float targetOrtho;
    // private float smoothSpeed = 2000.0f;
    private float minOrtho = 25f;
    private float maxOrtho = 2151f ;
    public float defaultMoveSpeed ;

    public float mapMinX, mapMaX, mapMinY, mapMaxY;

    void Start()
    {
        
        targetOrtho = cam.orthographicSize;
              
    }

    void Update()
    {
        var movement = Vector3.zero;
        float moveZ = 0;
        

        if (Input.GetKey(KeyCode.W))
        {
            // moveY += moveSpeed;
            movement.y++;
        }

        if (Input.GetKey(KeyCode.S))
        {
            // moveY -= moveSpeed;
            movement.y--;
        }

        if (Input.GetKey(KeyCode.A))
        {
            //moveX -= moveSpeed;
            movement.x--;
        }

        if (Input.GetKey(KeyCode.D))
        {
            //moveX += moveSpeed;
            movement.x++;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            moveZ += zoomSpeed;
                
        }

        if (Input.GetKey(KeyCode.E))
        {
            moveZ -= zoomSpeed;
        }
        

        transform.Translate(movement  * defaultMoveSpeed * Time.deltaTime / (Time.timeScale/2f), Space.Self);
        //transform.position = new Vector3(
        //  Mathf.Clamp(transform.position.x, mapMinX, mapMaX),
        //  Mathf.Clamp(transform.position.y, mapMinY, mapMaxY),
        //  Mathf.Clamp(transform.position.z, -10, -10));
        // Zoom(moveZ);
        targetOrtho -= moveZ;
        targetOrtho = Mathf.Clamp(targetOrtho, minOrtho, maxOrtho);
        cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, targetOrtho, 2 * Mathf.Abs(moveZ * Time.deltaTime / Time.timeScale));
        transform.position = ClampCamera(transform.position);

        //transform.position = ClampCamera(transform.position + (new Vector3(moveX, moveY, 0)));
        //transform.position = Vector3.MoveTowards(transform.position, defaultMoveSpeed * target, 2 * defaultMoveSpeed * Time.deltaTime / Time.timeScale);

    }

    private Vector3 ClampCamera(Vector3 targetPosition)
    {
        float camHeight = cam.orthographicSize;
        float camWidth = cam.orthographicSize * cam.aspect;
   

        float minX = mapMinX + camWidth;
        float maxX = mapMaX - camWidth;
        float minY = mapMinY + camHeight;
        float maxY = mapMaxY - camHeight;

        float newX = Mathf.Clamp(targetPosition.x, minX, maxX);
        float newY = Mathf.Clamp(targetPosition.y, minY, maxY);

        return new Vector3(newX, newY, transform.position.z);
    }


    public void Zoom(float zoomStep)
    {
        float newSize = cam.orthographicSize - zoomStep;
        cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, newSize, 2 * Mathf.Abs(zoomStep * Time.deltaTime / Time.timeScale));
        cam.orthographicSize = Mathf.Clamp(newSize, minOrtho, maxOrtho);
    }

}
