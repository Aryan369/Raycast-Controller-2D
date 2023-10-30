using UnityEngine;

[RequireComponent(typeof(Controller))]
public class Pushable : MonoBehaviour
{
	Controller controller;
	public Vector2 velocity;

	public float gravity = 24f;

	void Start ()
	{
		controller = GetComponent<Controller> ();
	}

	void Update ()
	{
		if (controller.collisionData.below)
		{
			velocity.y = 0f;
		}
        else
		{
			velocity += Vector2.down * gravity * Time.deltaTime;
			controller.Move(velocity * Time.deltaTime, false);
		}
	}

    public Vector2 Push(Vector2 amount)
    {
        return controller.Move(amount, false);
    }
}
