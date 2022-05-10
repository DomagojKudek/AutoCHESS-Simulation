using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform target;
    [SerializeField] private float distanceToTarget = 5;
    private Vector3 previousPosition;
    private float ScrollSensitivity = 2.5f;

    IEnumerator Start()
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(1))
            {
                previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            }
                float ScrollAmount = Input.GetAxis("Mouse ScrollWheel") * ScrollSensitivity;
                distanceToTarget += ScrollAmount;
                distanceToTarget = Mathf.Clamp(distanceToTarget, 3.5f, 30f);
                Vector3 newPosition = cam.ScreenToViewportPoint(Input.mousePosition);

                Vector3 direction = previousPosition - newPosition;

                float rotationAroundYAxis = -direction.x * 360;
                float rotationAroundXAxis = direction.y * 360;

                cam.transform.position = target.position;

                cam.transform.Rotate(new Vector3(1, 0, 0), rotationAroundXAxis);
                cam.transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World);


                cam.transform.eulerAngles = new Vector3(Mathf.Clamp(cam.transform.eulerAngles.x, 10, 90), cam.transform.eulerAngles.y, cam.transform.eulerAngles.z);

                cam.transform.Translate(new Vector3(0, 0, -distanceToTarget));

                previousPosition = newPosition;
            yield return null;
        }
    }
}