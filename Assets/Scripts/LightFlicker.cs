using UnityEngine;

class LightFlicker : MonoBehaviour
{
    [SerializeField] private Light[] _lights;
    [SerializeField] private float _minIntensity = 0.5f;
    [SerializeField] private float _maxIntensity = 1.5f;
    [SerializeField] private float _flickerSpeed = 0.1f;

    private void Update()
    {
        if (_lights.Length == 0) return;
        foreach (var _light in _lights)
        {
            _light.intensity = Mathf.Lerp(_minIntensity, _maxIntensity, Mathf.PingPong(Time.time * _flickerSpeed, 1));
        }
    }
}