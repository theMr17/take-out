using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlickeringLight : MonoBehaviour
{
  [SerializeField] private Light2D lightToFlicker;
  [SerializeField] private Light2D lightGlowToFlicker;

  [SerializeField, Range(0f, 3f)] private float minIntensity = 0.5f;
  [SerializeField, Range(0f, 3f)] private float maxIntensity = 1.2f;
  [SerializeField, Min(0f)] private float timeBetweenIntensity = 0.15f;

  private float currentTimer;

  private void Awake()
  {
    ValidateIntensityBounds();
  }

  private void Update()
  {
    currentTimer += Time.deltaTime;

    if (!(currentTimer >= timeBetweenIntensity)) return;

    var randomIntensity = Random.Range(minIntensity, maxIntensity);
    lightToFlicker.intensity = randomIntensity;
    lightGlowToFlicker.intensity = randomIntensity * 30f;
    currentTimer = 0;
  }

  private void ValidateIntensityBounds()
  {
    if (!(minIntensity > maxIntensity))
    {
      return;
    }

    Debug.LogWarning("Min Intensity is greater than max Intensity, Swapping values!");
    (minIntensity, maxIntensity) = (maxIntensity, minIntensity);
  }
}
