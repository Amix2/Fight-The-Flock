using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsList : MonoBehaviour
{
    public GameObject text;
    public GameObject slider;
    public GameObject inputField;
    public GameObject panel;

    static public GameObject Text { get; private set; }
    static public GameObject Slider { get; private set; }
    static public GameObject InputField { get; private set; }

    // Start is called before the first frame update
    private void Start()
    {
        Text = text;
        Slider = slider;
        InputField = inputField;

        Transform content = transform.GetChild(0).GetChild(0);
        AddPanel("targetForceStrength", Settings.Instance.Boid.Forces.targetForceStrength, 
            new SliderValueSetter(0, 50, (float val) => Settings.Instance.Boid.Forces.targetForceStrength = val), content);
        AddPanel("cohesionForceStrength", Settings.Instance.Boid.Forces.cohesionForceStrength, 
            new SliderValueSetter(0, 50, (float val) => Settings.Instance.Boid.Forces.cohesionForceStrength = val), content);
        AddPanel("alignmentForceStrength", Settings.Instance.Boid.Forces.alignmentForceStrength, 
            new SliderValueSetter(0, 50, (float val) => Settings.Instance.Boid.Forces.alignmentForceStrength = val), content);
        AddPanel("sharedAvoidanceForceStrength", Settings.Instance.Boid.Forces.sharedAvoidanceForceStrength, 
            new SliderValueSetter(0, 1000, (float val) => Settings.Instance.Boid.Forces.sharedAvoidanceForceStrength = val), content);
        AddPanel("wallRayAvoidForceStrength", Settings.Instance.Boid.Forces.wallRayAvoidForceStrength,
            new SliderValueSetter(0, 1000, (float val) => Settings.Instance.Boid.Forces.wallRayAvoidForceStrength = val), content);
        AddPanel("wallProximityAvoidForceStrength", Settings.Instance.Boid.Forces.wallProximityAvoidForceStrength, 
            new SliderValueSetter(0, 1000, (float val) => Settings.Instance.Boid.Forces.wallProximityAvoidForceStrength = val), content);
        AddPanel("maxBoidSpeed", Settings.Instance.Boid.maxSpeed, 
            new SliderValueSetter(0, 10, (float val) => Settings.Instance.Boid.maxSpeed = val), content);
        AddPanel("minBoidSpeed", Settings.Instance.Boid.minSpeed, 
            new SliderValueSetter(0, 10, (float val) => Settings.Instance.Boid.minSpeed = val), content);
        AddPanel("surroundingsViewRange", Settings.Instance.Boid.surroundingsViewRange,
            new SliderValueSetter(0, 10, (float val) => Settings.Instance.Boid.surroundingsViewRange = val), content);
        AddPanel("separationDistance", Settings.Instance.Boid.separationDistance,
            new SliderValueSetter(0, 10, (float val) => Settings.Instance.Boid.separationDistance = val), content);
        AddPanel("rayAvoidDistance", Settings.Instance.Boid.ObstacleAvoidance.rayAvoidDistance,
            new SliderValueSetter(0, 10, (float val) => Settings.Instance.Boid.ObstacleAvoidance.rayAvoidDistance = val), content);
        AddPanel("proxymityAvoidDistance", Settings.Instance.Boid.ObstacleAvoidance.proxymityAvoidDistance,
            new SliderValueSetter(0, 10, (float val) => Settings.Instance.Boid.ObstacleAvoidance.proxymityAvoidDistance = val), content);

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    private void AddPanel(string name, float initValue, IValueSetter valueSetter, Transform content)
    {
        GameObject gameObject = GameObject.Instantiate(panel, content);
        var text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        text.text = name + " - " + initValue;
        valueSetter.SetInitValue(initValue);
        GameObject value = valueSetter.MakeGameObject();
        valueSetter.AddAction((float val) => text.text = name + " - " + val);
        value.transform.SetParent(gameObject.transform);
    }

    public interface IValueSetter
    {
        GameObject MakeGameObject();

        void AddAction(Action<float> action);
        void SetInitValue(float initValue);
    }

    public class SliderValueSetter : IValueSetter
    {
        public float minValue, maxValue, initValue;
        public Action<float> setValue;

        public SliderValueSetter(float minValue, float maxValue, float initValue, Action<float> setValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.setValue = setValue;
            this.initValue = initValue;
        }

        public SliderValueSetter(float minValue, float maxValue, Action<float> setValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.setValue = setValue;
        }

        public void AddAction(Action<float> action)
        {
            setValue += action;
        }

        public GameObject MakeGameObject()
        {
            GameObject gameObject = GameObject.Instantiate(SettingsList.Slider);
            Slider slider = gameObject.GetComponent<Slider>();
            slider.onValueChanged.AddListener((float val) =>
            {
                setValue?.Invoke(val);
            });
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = initValue;
            return gameObject;
        }

        public void SetInitValue(float initValue)
        {
            this.initValue = initValue;
        }
    }
}