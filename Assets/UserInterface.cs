using UnityEngine;
using System.Collections;

/*
 * This is a simple illustration of the PBInterp functionality.
 *  
 * You can adjust the "Max Acceleration" and "Target Position" sliders, then observe how
 * the "Position" and "Speed" evolve over time to converge on target position.
 */

public class UserInterface : MonoBehaviour
{
    void OnGUI()
    {
        Rect rect = new Rect(titlePos, 10, titleWidth, lineHeight);
        GUI.Label(rect, "Max Acceleration");

        NewColumn(ref rect);
        maxAcceleration = GUI.HorizontalSlider(rect, maxAcceleration, 0.0f, 0.1f);

        NewLine(ref rect);
        GUI.Label(rect, "Target Position");

        NewColumn(ref rect);
        targetPosition = GUI.HorizontalSlider(rect, targetPosition, 0.0f, 100.0f);

        NewLine(ref rect);
        GUI.Label(rect, "Position");

        NewColumn(ref rect);
        GUI.HorizontalSlider(rect, position, 0.0f, 100.0f);

        NewLine(ref rect);
        GUI.Label(rect, "Speed");

        NewColumn(ref rect);
        GUI.HorizontalSlider(rect, speed, -2.0f, +2.0f);

        speed += PBInterp.GetAcceleration(position, targetPosition, speed, maxAcceleration);
        position += speed;

        Debug.Log("position=" + position + ", speed=" + speed);
    }

    private void NewLine(ref Rect rect)
    {
        rect.x = titlePos;
        rect.width = titleWidth;
        rect.y += lineHeight;
    }

    private void NewColumn(ref Rect rect)
    {
        rect.x += rect.width;
        rect.width = Screen.width - titleWidth - 20;
    }

    private float position = 0.0f;
    private float targetPosition = 50.0f;
    private float speed = 0.0f;
    private float maxAcceleration = 0.001f;

    private static readonly int titlePos = 10;
    private static readonly int titleWidth = 140;
    private static readonly int lineHeight = 32;
}
