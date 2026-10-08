using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    [SerializeField] float _powershootupmax = 5;
    public float _powershootcurrent = 0;
    [SerializeField] Slider _slider;
    [SerializeField] float _scalespeed = 1.25f;

    private bool _sclaeup = false;
    private bool _DeActive = true;

    void Start()
    {
        _DeActive = true;
        _slider = GetComponent<Slider>();
        _slider.maxValue = _powershootupmax;
        _slider.value = _powershootcurrent;
    }

    void Update()
    {
        if (_DeActive == true)
        {
            return;
        }
        ScaleUp();
    }

    public void ScaleUp()
    {
        if (_sclaeup == true)
        {
            _powershootcurrent += Time.deltaTime * _scalespeed;
            if (_powershootcurrent >= _powershootupmax)
            {
                _sclaeup = false;
                _powershootcurrent = _powershootupmax;
            }
        }
        else if (_sclaeup == false)
        {
            _powershootcurrent -= Time.deltaTime * _scalespeed;
            if (_powershootcurrent <= 0)
            {
                _sclaeup = true;
                _powershootcurrent = 0;
            }
        }

        _slider.value = _powershootcurrent;
    }

    public void setzero()
    {
        _powershootcurrent = 0;
        _slider.value = _powershootcurrent;
    }

    public void UnEnable()
    {
        _DeActive = true;
    }

    public void Enable()
    {
        _DeActive = false;
    }
}
