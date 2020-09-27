using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Xml.Schema;
using SunbirdMB.Core;
using MonoGame.Framework.WpfInterop.Input;
using SunbirdMB;
using SunbirdMB.Interfaces;

namespace Sunbird.Core
{
    public class KeyReleasedEventArgs : EventArgs
    {
        public Keys Key { get; set; }

        public KeyReleasedEventArgs(Keys key)
        {
            Key = key;
        }
    }

    public static class Peripherals
    {
        public static MouseState currentMouseState { get; set; }
        public static MouseState previousMouseState { get; set; }
        public static KeyboardState currentKeyboardState { get; set; }
        public static KeyboardState previousKeyboardState { get; set; }
        public static Keys[] currentPressedKeys { get; set; }
        public static Keys[] previousPressedKeys { get; set; }
        public static int currentScrollWheelValue { get; set; }
        public static int previousScrollWheelValue { get; set; }

        public static event EventHandler<KeyReleasedEventArgs> KeyReleased;
        public static event EventHandler<EventArgs> MiddleButtonReleased;
        public static event EventHandler<EventArgs> LeftButtonReleased;
        public static event EventHandler<EventArgs> RightButtonReleased;
        public static event EventHandler<EventArgs> ScrollWheelUp;
        public static event EventHandler<EventArgs> ScrollWheelDown;

        public static void PreUpdate(IMainGame mainGame)
        {
            currentMouseState = mainGame.Mouse.GetState();
            currentKeyboardState = mainGame.Keyboard.GetState();
            currentPressedKeys = currentKeyboardState.GetPressedKeys();
            currentScrollWheelValue = mainGame.Mouse.GetState().ScrollWheelValue;

            CheckForRelease();
            CheckForScroll();
        }

        public static void PostUpdate()
        {
            //CheckForRelease();
            //CheckForScroll();

            previousMouseState = currentMouseState;
            previousKeyboardState = currentKeyboardState;
            previousPressedKeys = currentPressedKeys;
            previousScrollWheelValue = currentScrollWheelValue;
        }

        public static void CheckForRelease()
        {
            if (previousPressedKeys != null)
            {
                foreach (var key in previousPressedKeys)
                {
                    if (currentPressedKeys.Contains(key) == false)
                    {
                        var args = new KeyReleasedEventArgs(key);
                        OnKeyReleased(args);
                    }
                }
            }
            if (previousMouseState.MiddleButton == ButtonState.Pressed && currentMouseState.MiddleButton == ButtonState.Released)
            {
                OnMiddleMouseButtonReleased();
            }
            if (previousMouseState.LeftButton == ButtonState.Pressed && currentMouseState.LeftButton == ButtonState.Released)
            {
                OnLeftMouseButtonReleased();
            }
            if (previousMouseState.RightButton == ButtonState.Pressed && currentMouseState.RightButton == ButtonState.Released)
            {
                OnRightMouseButtonReleased();
            }
        }

        public static void CheckForScroll()
        {
            var change = previousMouseState.ScrollWheelValue - currentScrollWheelValue;
            if (change > 0)
            {
                OnScrollWheelDown();
            }
            else if (change < 0)
            {
                OnScrollWheelUp();
            }
        }

        public static void OnScrollWheelUp()
        {
            EventHandler<EventArgs> handler = ScrollWheelUp;
            handler?.Invoke(null, null);
        }

        public static void OnScrollWheelDown()
        {
            EventHandler<EventArgs> handler = ScrollWheelDown;
            handler?.Invoke(null, null);
        }

        public static void OnKeyReleased(KeyReleasedEventArgs e)
        {
            EventHandler<KeyReleasedEventArgs> handler = KeyReleased;
            handler?.Invoke(null, e);
        }

        public static void OnMiddleMouseButtonReleased()
        {
            EventHandler<EventArgs> handler = MiddleButtonReleased;
            handler?.Invoke(null, null);
        }

        public static void OnLeftMouseButtonReleased()
        {
            EventHandler<EventArgs> handler = LeftButtonReleased;
            handler?.Invoke(null, null);
        }

        public static void OnRightMouseButtonReleased()
        {
            EventHandler<EventArgs> handler = RightButtonReleased;
            handler?.Invoke(null, null);
        }

        public static Point GetMouseWindowPosition(IMainGame mainGame)
        {
            MouseState state = mainGame.Mouse.GetState();
            return new Point(state.X, state.Y);
        }

        public static Point GetCornerWorldPosition(Camera camera)
        {
            if (camera.CurrentMode == CameraMode.Drag)
            {
                return new Point(-(int)camera.DragTransform.M41, -(int)camera.DragTransform.M42);
            }
            else
            {
                return Point.Zero;
            }
        }

        /// <summary>
        /// For mouse interactions with world objects, apply ZoomRatio scaling to object position, width, and height to correctly determine overlaps.
        /// </summary>
        public static Point GetMouseWorldPosition(IMainGame mainGame, Camera camera)
        {   
            if (camera.CurrentMode == CameraMode.Drag)
            {
                return new Point(-(int)camera.DragTransform.M41, -(int)camera.DragTransform.M42) + GetMouseWindowPosition(mainGame);
            }
            else
            {
                return Point.Zero;
            }
        }

        public static Point GetScaledMouseWorldPosition(IMainGame mainGame, Camera camera)
        {   
            if (camera.CurrentMode == CameraMode.Drag)
            {
                return World.ZoomScaledPoint(new Point(-(int)camera.DragTransform.M41, -(int)camera.DragTransform.M42) + GetMouseWindowPosition(mainGame));
            }
            else
            {
                return Point.Zero;
            }
        }

        public static bool KeyTapped(Keys key)
        {
            return (currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key));
        }

        public static bool KeyPressed(Keys key)
        {
            return currentPressedKeys.Contains(key);
        }

        public static bool KeysPressed(Keys key1, Keys key2)
        {
            return currentPressedKeys.Contains(key1) && currentPressedKeys.Contains(key2);
        }

        public static bool MouseButtonTapped(ButtonState currentButton, ButtonState previousButton)
        {
            return (currentButton == ButtonState.Pressed && previousButton == ButtonState.Released);
        }

        public static bool MouseButtonPressed(ButtonState currentButton)
        {
            return (currentButton == ButtonState.Pressed);
        }

        public static bool LeftButtonTapped()
        {
            return MouseButtonTapped(currentMouseState.LeftButton, previousMouseState.LeftButton);
        }

        public static bool LeftButtonPressed()
        {
            return MouseButtonPressed(currentMouseState.LeftButton);
        }

        public static bool RightButtonTapped()
        {
            return MouseButtonTapped(currentMouseState.RightButton, previousMouseState.RightButton);
        }

        public static bool RightButtonPressed()
        {
            return MouseButtonPressed(currentMouseState.RightButton);
        }

        public static bool MiddleButtonTapped()
        {
            return MouseButtonTapped(currentMouseState.MiddleButton, previousMouseState.MiddleButton);
        }

        public static bool MiddleButtonPressed()
        {
            return MouseButtonPressed(currentMouseState.MiddleButton);
        }

    }
}

