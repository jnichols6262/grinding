using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Spline : MonoBehaviour
{
    [Header("Attachments")]
    public Vector3[] splinePoint;
    public GameObject grindCube;
    public GameObject pGrindCube;
    public GameManager gm;
    sMovement player;
    

    [Header("Spline")]
    public int splineCount;
    float zVel;

    [Header("Grinding")]
    public float endJumpHeight = 0.1f;
    public bool isAttached;
    public bool isGrinding;
    public bool isGrindingBackwords;
    float speed;

    private void Awake()
    {

        ControllerManager.controllerManager.playerControls.Gameplay.Grinding.performed += Grinding;
        ControllerManager.controllerManager.playerControls.Gameplay.Grinding.canceled += CancelGrinding;
        ControllerManager.controllerManager.playerControls.Gameplay.GrindingBackwords.performed += BackwardsGrinding;
        ControllerManager.controllerManager.playerControls.Gameplay.GrindingBackwords.canceled += CancelBackwardsGrinding;
        ControllerManager.controllerManager.playerControls.Gameplay.Grinding.Enable();
        ControllerManager.controllerManager.playerControls.Gameplay.GrindingBackwords.Enable();
        splineCount = transform.childCount;
        splinePoint = new Vector3[splineCount];

        for (int i = 0; i < splineCount; i++)
        {
            splinePoint[i] = transform.GetChild(i).position;
        }

    }
    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Player") //Sets player for attaching
        {

            player = other.GetComponent<sMovement>();
            player.spline = this;
            zVel = player.rb.velocity.z;
            speed = player.rb.velocity.magnitude;
            grindCube = Instantiate(pGrindCube, WhereOnSpline(other.transform.position), Quaternion.identity);
            Debug.Log(grindCube);
            player.AttachToRail();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            player.DeattachRail();
        }
    }

    public void DestroyGrindCube()
    {
        Destroy(grindCube);
        player = null;
    }

    private void Update()
    {
        if (splineCount > 1)
        {
            for (int i = 1; i < splineCount; i++)
            {
                Debug.DrawLine(splinePoint[i - 1], splinePoint[i], Color.red);
            }
        }

        if (player != null)
        {
            if (grindCube.transform.position == splinePoint[splineCount - 1] || grindCube.transform.position == splinePoint[0])
            {
                player.rb.AddForce(transform.up * endJumpHeight, ForceMode.Impulse);

            }





        }
        
    }

    private void FixedUpdate()
    {
        if (player != null)
        {

            if (player != null)
            {

                if (zVel > 0)
                {
                    if (isAttached && isGrinding) // Once player is attached, move the grindCube
                    {
                        grindCube.transform.position = Vector3.MoveTowards(grindCube.transform.position, splinePoint[splineCount - 1], speed * Time.deltaTime);

                    }
                    else if (isAttached && isGrindingBackwords)
                    {
                        grindCube.transform.position = Vector3.MoveTowards(grindCube.transform.position, splinePoint[0], speed * Time.deltaTime);


                    }

                }
                else
                {
                    if (isAttached && isGrinding)
                    {


                        grindCube.transform.position = Vector3.MoveTowards(grindCube.transform.position, splinePoint[0], speed * Time.deltaTime);

                    }
                    else if (isAttached && isGrindingBackwords)
                    {
                        grindCube.transform.position = Vector3.MoveTowards(grindCube.transform.position, splinePoint[splineCount - 1], speed * Time.deltaTime);

                    }
                }

            }
        }
    }

    void Grinding(InputAction.CallbackContext context)
    {
        Debug.Log(context.performed);
        if (context.performed)
        {
            isGrinding = true;
        }

    }
    void CancelGrinding(InputAction.CallbackContext context)
    {
        Debug.Log(context.canceled);
        if (context.canceled)
        {
            isGrinding = false;
        }

    }

    void BackwardsGrinding(InputAction.CallbackContext context)
    {
        Debug.Log(context.performed);
        if (context.performed)
        {
            isGrindingBackwords = true;
        }
    }

    void CancelBackwardsGrinding(InputAction.CallbackContext context)
    {
        Debug.Log(context.canceled);
        if (context.canceled)
        {
            isGrindingBackwords = false;
        }
    }

    public Vector3 WhereOnSpline(Vector3 pos) //Sets the players exact posistion on the spline
    {
        int ClosestSplinePoint = GetClosestSplinePoint(pos);

        if (ClosestSplinePoint == 0)
        {
            return SplineSegment(splinePoint[0], splinePoint[1], pos);
        }
        else if (ClosestSplinePoint == splineCount - 1)
        {
            return SplineSegment(splinePoint[splineCount - 1], splinePoint[splineCount - 2], pos);

        }
        else
        {
            Vector3 leftSeg = SplineSegment(splinePoint[ClosestSplinePoint - 1], splinePoint[ClosestSplinePoint], pos);
            Vector3 rightSeg = SplineSegment(splinePoint[ClosestSplinePoint + 1], splinePoint[ClosestSplinePoint], pos);

            if ((pos - leftSeg).sqrMagnitude <= (pos - rightSeg).sqrMagnitude)
            {
                return leftSeg;
            }
            else
                return rightSeg;
        }
    }

    int GetClosestSplinePoint(Vector3 pos)
    {
        int closestPoint = -1;
        float shortestDistance = 0.0f;

        for (int i = 0; i < splineCount; i++)
        {
            float sqrDistance = (splinePoint[i] - pos).sqrMagnitude;
            if (shortestDistance == 0.0f || sqrDistance < shortestDistance)
            {
                shortestDistance = sqrDistance;
                closestPoint = i;
            }
        }
        return closestPoint;
    }

    public Vector3 SplineSegment(Vector3 v1, Vector3 v2, Vector3 pos)
    {
        Vector3 v1ToPos = pos - v1;
        Vector3 seqDirection = (v2 - v1).normalized;

        float distanceFromV1 = Vector3.Dot(seqDirection, v1ToPos);

        if (distanceFromV1 < 0.0f)
        {
            return v1;
        }
        else if (distanceFromV1 * distanceFromV1 > (v2 - v1).sqrMagnitude)
        {
            return v2;
        }
        else
        {
            Vector3 fromV1 = seqDirection * distanceFromV1;
            return v1 + fromV1;
        }
    }
}
