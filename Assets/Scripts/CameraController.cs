using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    Vector3 anchor;
    float freeMoveDist = 3f;
    public Transform mainPlayer;

	// Use this for initialization
	void Start () {
        transform.LookAt(anchor);
        anchor = mainPlayer.position;
	}
	
	// Update is called once per frame
	void Update () {
        float anchorDist = Vector3.Distance(anchor, mainPlayer.position);
        if (anchorDist > freeMoveDist)
        {
            Vector3 newAnchor = anchor + (mainPlayer.position - anchor).normalized * (anchorDist - freeMoveDist);
            transform.position = transform.position + (newAnchor - anchor);
            anchor = newAnchor;
        }
	}
}
