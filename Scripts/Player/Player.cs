using Godot;
using System;

public class Player : KinematicBody
{
    //Mouse variables
    [Export] float mouseSensitivity = 0.3f;

    //Fly variables
    [Export] float flySpeed = 5f;
    [Export] float flyAcceleration = 2f;
    bool flyMode = false;
    Vector3 flyVelocity;

    //Move variables
    [Export] float gravity = -9.8f;
    [Export] float gravityMultiplier = 3f;
    [Export] float moveSpeed = 5f;
    [Export] float moveWalkSpeed = 2f;
    [Export] float moveAcceleration = 2f;
    [Export] float moveDeceleration = 4f;
    bool isWalking = false;
    Vector3 velocity;
    
    //Jump variables
    [Export] float jumpSpeed = 15f;
    bool canJump = false;
    bool hasFloorContact = false;
    

    //View variables
    [Export] float maxAngleView = 90f;
    [Export] float minAngleView = -90f;
    float cameraAngle = 0f;
    float headRelativeAngle = 0f;

    //Slope variables
    [Export] float maxSlopeAngle = 35f;

    Camera camera;
    Spatial head;
    RayCast floorChecker;
    Vector3 direction;

    public override void _Ready()
    {
        GetNodes();
        gravity = gravity * gravityMultiplier;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (flyMode)
        {
            MoveCharacterFly(delta);
        }
        else
        {
            MoveCharacter(delta);
        }
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

        Basis headDirection = head.GetGlobalTransform().basis;
        Basis cameraDirection = camera.GetGlobalTransform().basis;

        if (flyMode)
        {
            CalculateFlyDirection(cameraDirection);
        }
        else
        {
            CalculateMoveDirection(cameraDirection, headDirection);
        }

        direction = direction.Normalized();

        isWalking = Input.IsActionPressed("move_walk");
        canJump = Input.IsActionJustPressed("jump") && hasFloorContact;
        if (Input.IsActionJustPressed("fly_mode")) flyMode = !flyMode;
    }

    void MoveCharacter(float delta)
    {
        CalculateMoveToFloor(delta);

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

        if (canJump)
        {
            velocity.y = jumpSpeed;
            hasFloorContact = false;
        }

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

            cameraAngle = Mathf.Clamp(cameraAngle - mouseMotion.Relative.y * mouseSensitivity, minAngleView, maxAngleView);
        }
    }

    void GetNodes()
    {
        head = (Spatial) FindNode("Head");
        camera = (Camera) FindNode("Camera");
        floorChecker = (RayCast) FindNode("FloorChecker");
    }

    void RotateView()
    {
        head.RotateY(headRelativeAngle);
        camera.SetRotationDegrees(new Vector3(cameraAngle, 0f, 0f));
        headRelativeAngle = 0f;
    }
    
    void CalculateFlyDirection(Basis cameraDirection)
    {
        if (Input.IsActionPressed("move_forward")) direction -= cameraDirection.z;
        if (Input.IsActionPressed("move_backward")) direction += cameraDirection.z;
        if (Input.IsActionPressed("move_left")) direction -= cameraDirection.x;
        if (Input.IsActionPressed("move_right")) direction += cameraDirection.x;
    }

    void CalculateMoveDirection(Basis cameraDirection, Basis headDirection)
    {
        if (Input.IsActionPressed("move_forward")) direction -= headDirection.z;
        if (Input.IsActionPressed("move_backward")) direction += headDirection.z;
        if (Input.IsActionPressed("move_left")) direction -= cameraDirection.x;
        if (Input.IsActionPressed("move_right")) direction += cameraDirection.x;
    }

    void CalculateMoveToFloor(float delta)
    {
        if (IsOnFloor())
        {
            hasFloorContact = true;

            Vector3 floorNormal = floorChecker.GetCollisionNormal();
            float floorAngle = Mathf.Rad2Deg(Mathf.Acos(floorNormal.Dot(Vector3.Up)));

            if (floorAngle > maxSlopeAngle) velocity.y += gravity * delta;
        }
        else
        {
            if (!floorChecker.IsColliding())
            {
                velocity.y += gravity * delta;
                hasFloorContact = false;
            }
        }

        if (hasFloorContact && !IsOnFloor()) MoveAndCollide(Vector3.Down);
    }
}
