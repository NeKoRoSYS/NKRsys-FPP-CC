using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    public GameObject camPrefab;
    [SerializeField] private PlayerAudio playerAudio;
    public PlayerAudio GetPlayerAudio() => playerAudio;
    [SerializeField] private CameraManager cameraManager;
    public CameraManager GetCameraManager() => cameraManager;
    [SerializeField] private CameraController cameraController;
    public CameraController GetCameraController() => cameraController;
    [SerializeField] private PlayerMovement playerMovement;
    public PlayerMovement GetPlayerMovement() => playerMovement;
    [SerializeField] private CharacterController controller;
    public CharacterController GetCharacterController() => controller;
    [SerializeField] private Transform orientation;
    public Transform GetOrientation() => orientation;
    [SerializeField] private Transform head;
    public Transform GetHead() => head;

    public IEnumerator Start()
    {
        InitializeCamera();
        yield return new WaitForEndOfFrame();
        UISettings.Instance.playerManager = this;
        PlayerInput.Instance.joystick = UISettings.Instance.joystick;
        PlayerInput.Instance.controlPad = UISettings.Instance.controlPad;
    }

    private void InitializeCamera()
    {
        if (cameraManager == null && cameraController == null)
        {
            cameraManager = Instantiate(camPrefab, Vector3.zero, Quaternion.identity).GetComponent<CameraManager>();
            cameraManager.playerManager = this;
            cameraController = cameraManager.cameraController;
        }
    }
}
