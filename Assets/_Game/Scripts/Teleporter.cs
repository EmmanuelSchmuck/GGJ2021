using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform sourceObject;
    public Transform targetAnchor;

    public void TeleportAtChild()
	{
        sourceObject.parent = targetAnchor != null ? targetAnchor : this.transform;
        sourceObject.localPosition = Vector3.zero;
        sourceObject.localEulerAngles = Vector3.zero;
	}
}
