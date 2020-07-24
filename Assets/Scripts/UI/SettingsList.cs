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
        AddPanel("targetForceStrength", Settings.Instance.targetForceStrength, new SliderValueSetter(0, 50, Settings.Instance.targetForceStrength, (float val) => Settings.Instance.targetForceStrength = val), content);
        AddPanel("cohesionForceStrength", Settings.Instance.cohesionForceStrength, new SliderValueSetter(0, 50, Settings.Instance.cohesionForceStrength, (float val) => Settings.Instance.cohesionForceStrength = val), content);
        AddPanel("alignmentForceStrength", Settings.Instance.alignmentForceStrength, new SliderValueSetter(0, 50, Settings.Instance.alignmentForceStrength, (float val) => Settings.Instance.alignmentForceStrength = val), content);
        AddPanel("sharedAvoidanceForceStrength", Settings.Instance.sharedAvoidanceForceStrength, new SliderValueSetter(0, 250, Settings.Instance.sharedAvoidanceForceStrength, (float val) => Settings.Instance.sharedAvoidanceForceStrength = val), content);
        AddPanel("wallAvoidanceForceStrength", Settings.Instance.wallAvoidanceForceStrength, new SliderValueSetter(0, 250, Settings.Instance.wallAvoidanceForceStrength, (float val) => Settings.Instance.wallAvoidanceForceStrength = val), content);
        AddPanel("maxBoidSpeed", Settings.Instance.maxBoidSpeed, new SliderValueSetter(0, 10, Settings.Instance.maxBoidSpeed, (float val) => Settings.Instance.maxBoidSpeed = val), content);
        AddPanel("minBoidSpeed", Settings.Instance.minBoidSpeed, new SliderValueSetter(0, 10, Settings.Instance.minBoidSpeed, (float val) => Settings.Instance.minBoidSpeed = val), content);
        AddPanel("maxBoidObstacleAvoidance", Settings.Instance.maxBoidObstacleAvoidance, new SliderValueSetter(0, 10, Settings.Instance.maxBoidObstacleAvoidance, (float val) => Settings.Instance.maxBoidObstacleAvoidance = val), content);
        AddPanel("minBoidObstacleDist", Settings.Instance.minBoidObstacleDist, new SliderValueSetter(0, 10, Settings.Instance.minBoidObstacleDist, (float val) => Settings.Instance.minBoidObstacleDist = val), content);
        AddPanel("boidObstacleProximityPush", Settings.Instance.boidObstacleProximityPush, new SliderValueSetter(0, 250, Settings.Instance.boidObstacleProximityPush, (float val) => Settings.Instance.boidObstacleProximityPush = val), content);
        AddPanel("boidSurroundingsViewRange", Settings.Instance.boidSurroundingsViewRange, new SliderValueSetter(0, 10, Settings.Instance.boidSurroundingsViewRange, (float val) => Settings.Instance.boidSurroundingsViewRange = val), content);
        AddPanel("boidSeparationDistance", Settings.Instance.boidSeparationDistance, new SliderValueSetter(0, 10, Settings.Instance.boidSeparationDistance, (float val) => Settings.Instance.boidSeparationDistance = val), content);

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    private void AddPanel(string name, float initValue, IValueSetter valueSetter, Transform content)
    {
        GameObject gameObject = GameObject.Instantiate(panel, content);
        var text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        text.text = name + " - " + initValue;
        GameObject value = valueSetter.MakeGameObject();
        valueSetter.AddAction((float val) => text.text = name + " - " + val);
        value.transform.SetParent(gameObject.transform);
    }

    public interface IValueSetter
    {
        GameObject MakeGameObject();

        void AddAction(Action<float> action);
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
    }
}