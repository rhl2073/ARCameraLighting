using System;
using System.Collections.Generic;

namespace UnityEngine.XR.iOS
{
	public class ARKitHitTest : MonoBehaviour
	{
		public float maxRayDistance = 30.0f;
		public LayerMask collisionLayer = 1 << 10;  //ARKitPlane layer
        private Transform hitTransform;

        bool HitTestWithResultType (ARPoint point, ARHitTestResultType resultTypes)
        {
            List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, resultTypes);
            if (hitResults.Count > 0) {
                foreach (var hitResult in hitResults) {
                    Debug.Log ("Got hit!");

                    hitTransform.position = UnityARMatrixOps.GetPosition(hitResult.worldTransform);
                    hitTransform.rotation = UnityARMatrixOps.GetRotation(hitResult.worldTransform);
                    SendMessageUpwards("PlaneHit", hitTransform);

                    Debug.Log(string.Format("x:{0:0.######} y:{1:0.######} z:{2:0.######}", hitTransform.position.x, hitTransform.position.y, hitTransform.position.z));
                    return true;
                }
            }
            return false;
        }

        void Awake() {
            hitTransform = new GameObject("HitTransformObject").transform;
        }
		
		// Update is called once per frame
		void Update () {
#if UNITY_EDITOR   //we will only use this script on the editor side, though there is nothing that would prevent it from working on device
			if (Input.GetMouseButtonDown (0)) {
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				
				//we'll try to hit one of the plane collider gameobjects that were generated by the plugin
				//effectively similar to calling HitTest with ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent
				if (Physics.Raycast (ray, out hit, maxRayDistance, collisionLayer)) {
					//we're going to get the position from the contact point
					hitTransform.position = hit.point;
					Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", hitTransform.position.x, hitTransform.position.y, hitTransform.position.z));

					//and the rotation from the transform of the plane collider
					hitTransform.rotation = hit.transform.rotation;
				}
			}
#else
            if (Input.touchCount > 0)
			{
				var touch = Input.GetTouch(0);
				if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
				{
					var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
					ARPoint point = new ARPoint {
						x = screenPosition.x,
						y = screenPosition.y
					};

                    // prioritize reults types
                    ARHitTestResultType[] resultTypes = {
						//ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingGeometry,
                        ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, 
                        // if you want to use infinite planes use this:
                        //ARHitTestResultType.ARHitTestResultTypeExistingPlane,
                        //ARHitTestResultType.ARHitTestResultTypeEstimatedHorizontalPlane, 
						//ARHitTestResultType.ARHitTestResultTypeEstimatedVerticalPlane, 
						//ARHitTestResultType.ARHitTestResultTypeFeaturePoint
                    }; 
					
                    foreach (ARHitTestResultType resultType in resultTypes)
                    {
                        if (HitTestWithResultType (point, resultType))
                        {
                            return;
                        }
                    }
				}
			}
			#endif

		}

	
	}
}

