using UnityEngine;

/// <summary>
/// Attach to object that the target will folow
/// </summary>
public class FakeParent : MonoBehaviour
{
    private bool active;
    private bool reset;
    public bool sip;

    [SerializeField] private bool matchPos;
    [SerializeField] private bool matchRot;
    [SerializeField] private bool propIsLiquid;
    [SerializeField] private bool matchScale;
    [SerializeField] private float rotOfMug;
    [SerializeField] private Vector3 posOffset;

    [SerializeField, Tooltip("The prop to be parented")]
    private Transform target;

    [SerializeField, Tooltip("The spot to place the prop when unparented")]
    private Transform origin;


    private void Update()
    {
        if (active)
        {
            if (sip)
            {
                posOffset = new Vector3(-.15f, .1f, .09f);
                rotOfMug = 180;
            }
            else
            {
                posOffset = new Vector3(.15f, 0, .09f);
                rotOfMug = -44;
            }

            Parent(target);
        }
        else
        {
            if (reset)
            {
                Unparent(origin);
                reset = false;
            }
        }
    }

    private void Parent(Transform targ)
    {
        if (matchPos)
            target.position = transform.position + posOffset;
        if (matchRot)
        {

            if (propIsLiquid)
                target.localRotation = Quaternion.Euler(0, 0, rotOfMug + transform.localEulerAngles.z);
            else
                target.rotation = transform.rotation;
        }

        if (matchScale)
            target.localScale = transform.localScale;
    }

    public void Unparent(Transform orig)
    {
        target.position = orig.position;
        target.rotation = orig.rotation;
        target.localScale = orig.localScale;
    }

    public void Parent()
    {
        active = true;
        reset = false;
    }

    public void Unparent()
    {
        active = false;
        reset = true;
    }
}
