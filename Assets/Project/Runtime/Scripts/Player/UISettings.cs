using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using NeKoRoSYS.InputManagement;

[System.Serializable]
public class UISettings : MonoBehaviour
{
    public static UISettings Instance;
    public ControlPad controlPad;
    public VariableJoystick joystick;
    public PlayerManager playerManager;
    public PlayerMovement playerMovement;
    public CameraManager cameraManager;
    public CameraController cameraController;
    public AudioMixer gameAudio;
    public Button saveButton;
    public bool savesInit = false;
    public int frameRate;
    public int joystickType;
    public Button[] buttons;
    public Button[] fpsButtons;
    public Button[] joystickButtons;
    public Toggle[] toggles;
    public Toggle toggleCrouch;
    public Toggle invertYAxis;
    public Toggle reduceMotion;
    public Toggle moveCamera;
    public Toggle bobX;
    public Toggle bobY;
    public Toggle tilt;
    public Slider[] sliders;
    public Slider sensX;
    public Slider sensY;
    public Slider smoothing;
    public Slider volume;

    private void Awake() {
        Instance = this;
        saveButton.interactable = false;
        if(toggles.Length == 0)
        {
            toggles = new Toggle[7];
            toggles[0] = toggleCrouch;
            toggles[1] = invertYAxis;
            toggles[2] = reduceMotion;
            toggles[3] = moveCamera;
            toggles[4] = bobX;
            toggles[5] = bobY;
            toggles[6] = tilt;
        }

        if(sliders.Length == 0)
        {
            sliders = new Slider[4];
            sliders[0] = sensX;
            sliders[1] = sensY;
            sliders[2] = smoothing;
            sliders[3] = volume;
        }
    }

    private IEnumerator Start()
    {
        while (playerManager == null) yield return new WaitForEndOfFrame();
        playerMovement = playerManager.GetPlayerMovement();
        cameraManager = playerManager.GetCameraManager();
        cameraController = playerManager.GetCameraController();
        StartCoroutine(LoadSettingsData(false));
        while (!savesInit) yield return null;
        InitializePlayerData();
        saveButton.interactable = false;
    }

    private void OnEnable()
    {
        saveButton.onClick.AddListener(() => ButtonCallback(saveButton));

        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => ButtonCallback(button));
        }

        foreach (Button button in fpsButtons)
        {
            button.onClick.AddListener(() => FrameRateCallback(button));
        }

        foreach (Button button in joystickButtons)
        {
            button.onClick.AddListener(() => JoystickCallback(button));
        }


        foreach (Toggle toggle in toggles)
        {
            toggle.onValueChanged.AddListener(delegate
                {
                    ToggleCallback(toggle);
                }
            );
        }

        foreach (Slider slider in sliders)
        {
            slider.onValueChanged.AddListener(delegate
                {
                    SliderCallback(slider, slider.value);
                }
            );
        }

        saveButton.interactable = false;
    }

    private void OnDisable()
    {
        saveButton.onClick.RemoveListener(() => ButtonCallback(saveButton));
        
        foreach (Button button in buttons)
        {
            button.onClick.RemoveListener(() => ButtonCallback(button));
        }

        foreach (Button button in fpsButtons)
        {
            button.onClick.RemoveListener(() => FrameRateCallback(button));
        }

        foreach (Button button in joystickButtons)
        {
            button.onClick.RemoveListener(() => JoystickCallback(button));
        }

        foreach (Toggle toggle in toggles)
        {
            toggle.onValueChanged.RemoveListener(delegate
                {
                    ToggleCallback(toggle);
                }
            );
        }

        foreach (Slider slider in sliders)
        {
            slider.onValueChanged.RemoveListener(delegate
                {
                    SliderCallback(slider, slider.value);
                }
            );
        }

        saveButton.interactable = false;
    }

    private void FrameRateCallback(Button selectedButton)
    {
        for (int i = 0; i < fpsButtons.Length; i++)
        {
            if (selectedButton == fpsButtons[i])
            {
                frameRate = (i + 1) * 30;
                break;
            }
        }
    }

    private void JoystickCallback(Button selectedButton)
    {
        for (int i = 0; i < joystickButtons.Length; i++)
        {
            if (selectedButton == joystickButtons[i])
            {
                joystickType = i;
                break;
            }
        }
    }


    private void ButtonCallback(Button selectedButton)
    {
        if (selectedButton == saveButton)
        {
            InitializePlayerData();
            saveButton.interactable = false;
        } else
        {
            saveButton.interactable = true;
        }
    }

    Toggle savedToggle;
    private void ToggleCallback(Toggle selectedToggle)
    {
        if (selectedToggle != null) savedToggle = selectedToggle;
        if (savedToggle != null) saveButton.interactable = true;
    }

    Slider savedSlider;
    private void SliderCallback(Slider selectedSlider, float value)
    {
        if (selectedSlider != null) savedSlider = selectedSlider;
        if (savedSlider != null) saveButton.interactable = true;
    }

    public void SaveSettingsData() => SaveManager.SaveSettingsJson(this);
    public void ResetSettingsData(bool reset) => StartCoroutine(LoadSettingsData(reset));
    public IEnumerator LoadSettingsData(bool reset)
    {
        SettingsData settingsData = SaveManager.LoadSettingsJson(reset);

        bool[] dataToggles = new bool[toggles.Length];
            dataToggles[0] = settingsData.ToggleCrouch;
            dataToggles[1] = settingsData.InvertYAxis;
            dataToggles[2] = settingsData.ReduceMotion;
            dataToggles[3] = settingsData.MoveCamera;
            dataToggles[4] = settingsData.BobX;
            dataToggles[5] = settingsData.BobY;
            dataToggles[6] = settingsData.Tilt;
        float[] dataSliders = new float[sliders.Length];
            dataSliders[0] = settingsData.SensX;
            dataSliders[1] = settingsData.SensY;
            dataSliders[2] = settingsData.Smoothing;
            dataSliders[3] = settingsData.Volume;
            
        frameRate = settingsData.FrameRate;
        joystickType = settingsData.JoystickType;

        for (int i = 0; i < toggles.Length; i++)
        {
            while (toggles[i].isOn != dataToggles[i])
            {
                toggles[i].isOn = dataToggles[i];
                yield return null;
            }
        }

        for (int i = 0; i < sliders.Length; i++)
        {
            while (sliders[i].value != dataSliders[i])
            {
                sliders[i].value = dataSliders[i];
                yield return null;
            }
        }
        savesInit = true;
    }

    private void InitializePlayerData()
    {
        Application.targetFrameRate = frameRate;
        joystick.SetMode(joystickType);
        playerMovement.toggleCrouch = toggleCrouch.isOn;
        cameraManager.invertYAxis = invertYAxis.isOn;
        cameraManager.reduceMotion = reduceMotion.isOn;
        cameraManager.moveCamera = moveCamera.isOn;
        cameraManager.moveBobX = bobX.isOn;
        cameraManager.moveBobY = bobY.isOn;
        cameraManager.Tilt = tilt.isOn;
        cameraManager.sensX = sensX.value;
        cameraManager.sensY = sensY.value;
        cameraController.damp = smoothing.value;
        if (gameAudio) gameAudio.SetFloat("master", volume.value);
    }
}

[System.Serializable]
public class SettingsData
{
    public int FrameRate { get { return frameRate; } set { frameRate = value; } }
    public int frameRate;
    public int JoystickType { get { return joystickType; } set { joystickType = value; } }
    public int joystickType;
    public bool ToggleCrouch { get { return toggleCrouch; } set { toggleCrouch = value; } }
    public bool toggleCrouch;
    public bool InvertYAxis { get { return invertYAxis; } set { invertYAxis = value; } }
    public bool invertYAxis;
    public bool ReduceMotion { get { return reduceMotion; } set { reduceMotion = value; } }
    public bool reduceMotion;
    public bool MoveCamera { get { return moveCamera; } set { moveCamera = value; } }
    public bool moveCamera;
    public bool BobX { get { return bobX; } set { bobX = value; } }
    public bool bobX;
    public bool BobY { get { return bobY; } set { bobY = value; } }
    public bool bobY;
    public bool Tilt { get { return tilt; } set { tilt = value; } }
    public bool tilt;
    public float SensX { get { return sensX; } set { sensX = value; } }
    public float sensX;
    public float SensY { get { return sensY; } set { sensY = value; } }
    public float sensY;
    public float Smoothing { get { return smoothing; } set { smoothing = value; } }
    public float smoothing;
    public float Volume { get { return volume; } set { volume = value; } }
    public float volume;

    public static SettingsData FromDefault ()
    {
        return new SettingsData
        {
            FrameRate = 60,
            JoystickType = 1,
            ToggleCrouch = true,
            InvertYAxis = false,
            ReduceMotion = false,
            MoveCamera = true,
            BobX = true,
            BobY = true,
            Tilt = true,
            SensX = 8.0f,
            SensY = 8.0f,
            Smoothing = 20f,
            Volume = 1.0f
        };
    }

    public static SettingsData FromSettings (UISettings uISettings)
    {
        return new SettingsData
        {
            FrameRate = uISettings.frameRate,
            JoystickType = uISettings.joystickType,
            ToggleCrouch = uISettings.toggleCrouch.isOn,
            InvertYAxis = uISettings.invertYAxis.isOn,
            ReduceMotion = uISettings.reduceMotion.isOn,
            MoveCamera = uISettings.moveCamera.isOn,
            BobX = uISettings.bobX.isOn,
            BobY = uISettings.bobY.isOn,
            Tilt = uISettings.tilt.isOn,
            SensX = uISettings.sensX.value,
            SensY = uISettings.sensY.value,
            Smoothing = uISettings.smoothing.value,
            Volume = uISettings.volume.value
        };
    }
}