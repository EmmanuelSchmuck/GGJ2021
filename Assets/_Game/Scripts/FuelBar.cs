using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FuelBar : MonoBehaviour
{
	public Image fuelbarImage;
	public Gradient gradient;

	public AnimationCurve easeCurve;
    public void SetFuel(float amount)
	{
		fuelbarImage.fillAmount = amount;
		fuelbarImage.color = gradient.Evaluate(easeCurve.Evaluate(amount));
	}
}
