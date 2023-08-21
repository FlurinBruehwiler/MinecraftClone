﻿namespace RayLib3dTest;

public class InputManager
{
    private float _playerSpeed = 1f;
    
    public void HandleInput(IControlable controlable)
    {
        HandleSpeedChange();
        
        var moveDelta = GetMovementDelta();
        
        var right = Vector3.Normalize(new Vector3(controlable.Right.X, 0, controlable.Right.Z));
        var forward = Vector3.Normalize(new Vector3(-controlable.Right.Z, 0, controlable.Right.X));

        var xComponent = right * moveDelta.X;
        var zComponent = forward * moveDelta.Z;
    
        var globalMoveDelta = xComponent + zComponent;
        globalMoveDelta.Y = moveDelta.Y;
        
        
    }

    private void HandleSpeedChange()
    {
        if (GetMouseWheelMoveV().Y > 0)
        {
            _playerSpeed *= 1.1f;
        }else if (GetMouseWheelMoveV().Y < 0)
        {
            _playerSpeed *= 0.9f;
        }
    
        _playerSpeed = Math.Max(_playerSpeed, 0);
    }

    private Vector3 GetMovementDelta()
    {
        var moveDelta = new Vector3();
        
        if (IsKeyDown(KeyboardKey.KEY_W))
        {
            moveDelta.X += _playerSpeed;
        }

        if (IsKeyDown(KeyboardKey.KEY_S))
        {
            moveDelta.X -= _playerSpeed;
        }

        if (IsKeyDown(KeyboardKey.KEY_D))
        {
            moveDelta.Z += _playerSpeed;
        }

        if (IsKeyDown(KeyboardKey.KEY_A))
        {
            moveDelta.Z -= _playerSpeed;
        }

        if (IsKeyDown(KeyboardKey.KEY_SPACE))
        {
            moveDelta.Y += _playerSpeed;
        }

        if (IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL))
        {
            moveDelta.Y -= _playerSpeed;
        }
        
        return moveDelta;
    }
}