    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class rgbController : MonoBehaviour
{
    //starter -> "https://www.youtube.com/watch?v=TokAIw3IWos"

    Rigidbody rb;
    [SerializeField] Transform orentation;

    [Header("movement")]
    [SerializeField] float movement_speed = 6;
    [SerializeField] float movement_multiplier_ground = 10, movement_multiplier_air = 3;
    [SerializeField] float movement_jumpForce;
    [SerializeField] float movement_drag_air = .5f, movement_drag_ground = 6;

    [Header("sprinting")]
    [SerializeField] float speed_walkSpeed = 4f;
    [SerializeField] float speed_sprintSpeed = 6f;
    [SerializeField] float speed_acceleration = 10f;

    float move_horizontal, move_vertical;
    Vector3 move_direciton, move_slope_direciton;

    [Header("ground detection")]
    [SerializeField] LayerMask _ground_Mask;
    [SerializeField] float _ground_Distance = .4f;
    [SerializeField] Transform _ground_Check;
    float _playerHeight = 2;
    bool isGrounded = false;

    RaycastHit _slopeHit;

    void Awake()
    {
        if (!rb) rb = transform.GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        input();
        controlDrag();
        controlSpeed();

        if (Input.GetKeyDown(KeyBinds.jump))
            jump();

        move_slope_direciton = Vector3.ProjectOnPlane(move_direciton, _slopeHit.normal);//makes the move direciton perpendicular to the slope of _slopeHit
    }
    void input()
    {
        move_horizontal = Input.GetAxis("Horizontal");
        move_vertical = Input.GetAxis("Vertical");
        move_direciton = orentation.forward * move_vertical + orentation.right * move_horizontal;
    }

    void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(_ground_Check.position, _ground_Distance, _ground_Mask);
        move();
    }

    void move()
    {
        bool onSlope = OnSlope();
        if (isGrounded && !onSlope)
        {
            rb.AddForce(move_direciton.normalized * movement_speed * movement_multiplier_ground, ForceMode.Acceleration);
        }
        else if(isGrounded && onSlope)
        {
            rb.AddForce(move_slope_direciton.normalized * movement_speed * movement_multiplier_ground, ForceMode.Acceleration);
        }
        else if(!isGrounded)
        {
            rb.AddForce(move_direciton.normalized * movement_speed * movement_multiplier_air, ForceMode.Acceleration);
        }
    }
    void controlDrag()
    {
        rb.drag = isGrounded ? movement_drag_ground : movement_drag_air;
    }
    void controlSpeed()
    {
        if(Input.GetKey(KeyBinds.sprint) && isGrounded)
        {
            movement_speed = Mathf.Lerp(movement_speed, speed_sprintSpeed, speed_acceleration * Time.deltaTime);
        }
        else
        {
            movement_speed = Mathf.Lerp(movement_speed, speed_walkSpeed, speed_acceleration * Time.deltaTime);
        }
    }
    void jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); 
            rb.AddForce(transform.up * movement_jumpForce, ForceMode.Impulse);
        }
    }
    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, _playerHeight / 2 + .5f))
            return _slopeHit.normal != Vector3.up;

        return false;
    }
}
