using Godot;
using System;

public class Player : KinematicBody
{
    float cameraAngle = 0f;
    [Export] float mouseSensitivity = 0.3f;
    [Export] float speed = 30f;
    [Export] float acceleration = 3f;
    
    Camera camera;
    Spatial head;

    Vector3 velocity;
    Vector3 direction;

    public override void _Ready()
    {
        head = (Spatial) FindNode("Head");
        camera = (Camera) FindNode("Camera");
    }

    public override void _PhysicsProcess(float delta)
    {
        direction = Vector3.Zero;

        Basis cameraDirection = camera.GetGlobalTransform().basis;

        if (Input.IsActionPressed("move_forward")) direction -= cameraDirection.z;
        if (Input.IsActionPressed("move_backward")) direction += cameraDirection.z;
        if (Input.IsActionPressed("move_left")) direction -= cameraDirection.x;
        if (Input.IsActionPressed("move_right")) direction += cameraDirection.x;

        direction = direction.Normalized();

        Vector3 target = direction * speed;

        velocity = velocity.LinearInterpolate(target, acceleration * delta);
        MoveAndSlide(velocity);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            head.RotateY(Mathf.Deg2Rad(-mouseMotion.Relative.x * mouseSensitivity));

            cameraAngle = Mathf.Clamp(cameraAngle - mouseMotion.Relative.y * mouseSensitivity, -90f, 90f);
            camera.SetRotationDegrees(new Vector3(cameraAngle, 0f, 0f));
        }
    }
}
