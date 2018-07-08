using Godot;
using System;

public class Player : KinematicBody
{
    //Mouse variables
    [Export] float mouseSensitivity = 0.3f;

    //Fly variables
    [Export] float flySpeed = 5f;
    [Export] float flyAcceleration = 2f;
    Vector3 flyVelocity;

    //Move variables
    [Export] float gravity = -9.8f;
    [Export] float gravityMultiplier = 3f;
    [Export] float moveSpeed = 5f;
    [Export] float moveWalkSpeed = 2f;
    [Export] float moveAcceleration = 2f;
    [Export] float moveDeceleration = 4f;
    
    //Jump variables
    [Export] float jumpSpeed = 15f;
    bool canJump = false;

    bool isWalking = false;
    Vector3 velocity;

    //View variables
    float cameraAngle = 0f;
    float headRelativeAngle = 0f;
    Camera camera;
    Spatial head;
    Vector3 direction;

    public override void _Ready()
    {
        GetNodes();
        gravity = gravity * gravityMultiplier;
    }

    public override void _PhysicsProcess(float delta)
    {
        MoveCharacter(delta);
    }

    public override void _Process(float delta)
    {
        UpdateMovementInput();
        RotateView();
    }

    public override void _Input(InputEvent @event)
    {
        UpdateCameraInput(@event);
    }

    void UpdateMovementInput()
    {
        direction = Vector3.Zero;

        Basis cameraDirection = camera.GetGlobalTransform().basis;

        if (Input.IsActionPressed("move_forward")) direction -= cameraDirection.z;
        if (Input.IsActionPressed("move_backward")) direction += cameraDirection.z;
        if (Input.IsActionPressed("move_left")) direction -= cameraDirection.x;
        if (Input.IsActionPressed("move_right")) direction += cameraDirection.x;
        isWalking = Input.IsActionPressed("move_walk");
        canJump = Input.IsActionJustPressed("jump");

        direction = direction.Normalized();
    }

    void MoveCharacter(float delta)
    {
        velocity.y += gravity * delta;

        Vector3 tempVelocity = velocity;
        tempVelocity.y = 0f;

        float speed;
        speed = isWalking ? moveWalkSpeed : moveSpeed;

        Vector3 target = direction * speed;

        float acceleration;
        if (direction.Dot(tempVelocity) > 0)
        {
            acceleration = moveAcceleration;
        }
        else
        {
            acceleration = moveDeceleration;
        }

        tempVelocity = tempVelocity.LinearInterpolate(target, acceleration * delta);

        velocity.x = tempVelocity.x;
        velocity.z = tempVelocity.z;

        if (canJump) velocity.y = jumpSpeed;

        velocity = MoveAndSlide(velocity, Vector3.Up);
    }

    void MoveCharacterFly(float delta)
    {
        Vector3 target = direction * flySpeed;

        flyVelocity = flyVelocity.LinearInterpolate(target, flyAcceleration * delta);
        MoveAndSlide(flyVelocity);
    }

    void UpdateCameraInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            headRelativeAngle = Mathf.Deg2Rad(-mouseMotion.Relative.x * mouseSensitivity);

            cameraAngle = Mathf.Clamp(cameraAngle - mouseMotion.Relative.y * mouseSensitivity, -90f, 90f);
        }
    }

    void GetNodes()
    {
        head = (Spatial) FindNode("Head");
        camera = (Camera) FindNode("Camera");
    }

    void RotateView()
    {
        head.RotateY(headRelativeAngle);
        camera.SetRotationDegrees(new Vector3(cameraAngle, 0f, 0f));
        headRelativeAngle = 0f;
    }
}
