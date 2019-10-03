using System;
using Godot;
using Refactor1.Game.Common;

namespace Refactor1.Game
{
    public class CameraController
    {
        private readonly Camera _camera;
        private bool _cameraOrbitActive = false;

        private Vector2 _lastMousePosition;

        private float angleX = 0f;

        private float angleY = 1f;

        private float distance = 30;

        private Vector3 target = new Vector3(0, 0, 0f);

        private bool _movingUp;
        
        private bool _movingLeft;
        
        private bool _movingDown;
        
        private bool _movingRight;
        
        public CameraController(Camera camera)
        {
            _camera = camera;
        }

        public void Process(float delta, Vector2 mousePosition)
        {
            if (_lastMousePosition == Vector2.Zero) _lastMousePosition = mousePosition;
            
            var diff = target - _camera.Translation;
            var flatDiff = new Vector3(diff.x, 0, diff.z);
            if (_movingUp)
            {
                target += flatDiff * delta;
            }
            if (_movingLeft)
            {
                target -= flatDiff.Cross(Vector3.Up) * delta;
            }
            if (_movingRight)
            {
                target += flatDiff.Cross(Vector3.Up) * delta;
            }
            if (_movingDown)
            {
                target -= flatDiff * delta;
            }

            if (_cameraOrbitActive)
            {
                var difference = mousePosition - _lastMousePosition;
                angleX += difference.x * delta;
                angleY += difference.y * delta;
                //GD.Print(angleX + " " + angleY);
            }


            var newVec3 = new Vector3(Mathf.Cos(angleX), 1f, Mathf.Sin(angleX));
            newVec3 *= distance;
            _camera.SetTranslation(target + newVec3);
            _camera.LookAtFromPosition(_camera.Translation, target, Vector3.Up);

            _lastMousePosition = mousePosition;
        }

        public bool HandleEvent(InputEvent inputEvent)
        {
            GD.Print(inputEvent);
            if (inputEvent is InputEventMouseButton mouseEvent) return HandleMouse(mouseEvent);
            if (inputEvent is InputEventKey keyEvent) return HandleKey(keyEvent);
            return false;
        }

        private bool HandleKey(InputEventKey keyEvent)
        {
            if (keyEvent.GetScancode() == (int) KeyList.W) { _movingUp = keyEvent.Pressed;
                return true;
            }
            if (keyEvent.GetScancode() == (int) KeyList.A) {_movingLeft = keyEvent.Pressed;
                return true;
            }
            if (keyEvent.GetScancode() == (int) KeyList.S) {_movingDown = keyEvent.Pressed;
                return true;
            }
            if (keyEvent.GetScancode() == (int) KeyList.D) {_movingRight = keyEvent.Pressed;
                return true;
            }

            return false;
        }

        private bool HandleMouse(InputEventMouseButton mouseEvent)
        {
            GD.Print(mouseEvent.ButtonIndex);
            if (mouseEvent.ButtonIndex == 3)
            {
                _cameraOrbitActive = mouseEvent.Pressed;
                return true;
            }

            if (mouseEvent.ButtonIndex == 4)
            {
                distance *= 0.8f;
                return true;
            }
            
            if (mouseEvent.ButtonIndex == 5)
            {
                distance *= 1.2f;
                return true;
            }

            return false;
        }
    }
}