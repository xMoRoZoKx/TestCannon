using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniTools;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : WindowBase
{
    [SerializeField] private TouchBar touchBar;
    [SerializeField] private Slider powerSlider;
    [SerializeField] private TMP_Text powerText;
    [SerializeField] private float sensative = 0.1f;

    private Coroutine fireCoroutine;
    public void Show(CannonView cannon)
    {
        powerSlider.minValue = 1;
        powerSlider.maxValue = 100;

        connections += powerSlider.onValueChanged.Subscribe(value =>
        {
            powerText.text = value.ToString("0.");
            cannon.FireForce = value;
        });
        powerSlider.value = cannon.FireForce;

        connections += touchBar.OnMoveX.Subscribe(value =>
        {
            cannon.RotateBody(value * sensative);
        });

        connections += touchBar.OnMoveY.Subscribe(value =>
        {
            cannon.RotateTrunk(-value * sensative);
        });

        connections += touchBar.OnPress.Subscribe(isPressed =>
        {
            if (isPressed)
            {
                fireCoroutine = StartCoroutine(cannon.AutomaticFire());
            }
            else if (fireCoroutine != null) StopCoroutine(fireCoroutine);
        });
    }
}
