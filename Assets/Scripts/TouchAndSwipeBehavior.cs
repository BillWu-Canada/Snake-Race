#if UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8
using UnityEngine;
using System.Collections;

public enum Swipe
{
  Left,
  Right,
  Up,
  Down
}

public class TouchAndSwipeBehavior : MonoBehaviour
{
  private float screenDiagonalSize;
  private float minimumSwipeDistanceInPixels;

  private bool touchStarted;
  private Vector2 touchStartPosition;

  public float minSwipeDistance = 0.05f;

  public static event System.Action<Touch> OnTouchDetected;
  public static event System.Action<Swipe> OnSwipeDetected;

  private void Start ()
  {
    screenDiagonalSize = Mathf.Sqrt (Screen.width * Screen.width + Screen.height * Screen.height);
    minimumSwipeDistanceInPixels = minSwipeDistance * screenDiagonalSize; 
  }

  private void Update ()
  {
    if (Input.touchCount > 0)
    {
      var touch = Input.touches [0];
			
      switch (touch.phase)
      {
        case TouchPhase.Began:
          touchStarted = true;
          touchStartPosition = touch.position;
          break;
				
        case TouchPhase.Ended:
          if (touchStarted)
          {
            TestForTouchOrSwipeGesture (touch);
            touchStarted = false;
          }
          break;
				
        case TouchPhase.Canceled:
          touchStarted = false;
          break;
      }
    }
  }

  private void TestForTouchOrSwipeGesture (Touch touch)
  {
    Vector2 lastPosition = touch.position;
    float distance = Vector2.Distance (lastPosition, touchStartPosition);
		
    if (distance > minimumSwipeDistanceInPixels)
    {
      float dy = lastPosition.y - touchStartPosition.y;
      float dx = lastPosition.x - touchStartPosition.x;
			
      float angle = Mathf.Rad2Deg * Mathf.Atan2 (dx, dy);
			
      angle = (360 + angle - 45) % 360;
			
      if (angle < 90)
        OnSwipeDetected (Swipe.Right);
      else if (angle < 180)
        OnSwipeDetected (Swipe.Down);
      else if (angle < 270)
        OnSwipeDetected (Swipe.Left);
      else
        OnSwipeDetected (Swipe.Up);
    }
    else
    {
      OnTouchDetected (touch);
    }
  }
}
#endif
