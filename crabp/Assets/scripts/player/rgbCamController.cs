using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rgbCamController : MonoBehaviour
{
    [SerializeField] Vector2 _sensitivity;
    Vector2 _mouseInput, _rotation;

    [SerializeField] Transform cam, orentation;

    [SerializeField] float _multiplier = .01f;

    void Start()
    {
        cursor_lock(true);
    }

    void Update()
    {
        input();

        cam.transform.localRotation = Quaternion.Euler(_rotation.x, _rotation.y, 0);
        orentation.transform.rotation = Quaternion.Euler(0, _rotation.y, 0);
    }
    void input()
    {
        //the axies are swapped since rotationg the Y turns it with the horision
        _mouseInput.y = Input.GetAxisRaw("Mouse X");
        _mouseInput.x = -Input.GetAxisRaw("Mouse Y");
        //Debug.Log(_mouseInput.y + "\n" + _mouseInput.x);

        _rotation += _mouseInput * _sensitivity * _multiplier;
        _rotation.x = Mathf.Clamp(_rotation.x, -90, 90);
    }

    public static void cursor_lock(bool l)
    {
        Cursor.visible = !l;

        if(l)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

}
