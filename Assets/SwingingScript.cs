using UnityEngine;

public class SwingingScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public LineRenderer line;
    public Transform player;
    public LayerMask canSwingFrom;

    public float maxSwing;
    public Vector3 swingPoint;
    public SpringJoint joint;
    void Start()
    {
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.transform.gameObject.GetComponent<PlayerScript>().activeAbility == "Swing")
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("J");
                StartSwing();
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                StopSwing();
            }
        }
        else if (joint != null)
        {
            StopSwing();
        }
        
    }

    void StartSwing()
    {
        RaycastHit hit;
        Vector3 dir = (transform.position - player.transform.position).normalized;
        if (Physics.Raycast(player.transform.position, dir, out hit, maxSwing, canSwingFrom))
        {
            swingPoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;


            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            // the distance grapple will try to keep from grapple point. 
            joint.maxDistance = distanceFromPoint * 0.5f;
            joint.minDistance = distanceFromPoint * 0.05f;

            // customize values as you like
            joint.spring = 30f;
            joint.damper = 2f;
            joint.massScale = 4.5f;

            line.positionCount = 2;
            currentGrapplePosition = player.position;
        }
    }

    void StopSwing()
    {
        line.positionCount = 0;
        Destroy(joint);
    }

    private Vector3 currentGrapplePosition;

    private void DrawRope()
    {
        // if not grappling, don't draw rope
        if (!joint) return;

        currentGrapplePosition =
            Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        line.SetPosition(0, player.position);
        line.SetPosition(1, currentGrapplePosition);
    }
}
