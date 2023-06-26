using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour {
    [SerializeField] private PlayerManager playerManager;
    [HideInInspector] private PlayerMovement playerMovement;
    [SerializeField] public AudioSource source;
    [field : SerializeField] public AudioWrapper[] footsteps;
    [field : SerializeField] public AudioWrapper[] landSfx;
    [SerializeField] public AudioClip[] defaultFootsteps;
    [SerializeField] public AudioClip[] defaultLandSfx;
    [SerializeField] public AudioClip jumpSound;
    [SerializeField] public AudioClip crouchSound;
    [SerializeField] public float footstepRate;
    private float Distance;
    private int currentMaterial;

    private void Awake()
    {
        playerMovement = playerManager.GetPlayerMovement();
        playerMovement.OnStartJump.AddListener(PlayJumpSound);
        playerMovement.OnCrouchAction.AddListener(PlayCrouchSound);
        playerMovement.OnStartLand.AddListener(PlayLandSound);
    }

    private void Start()
    {
        currentFootsteps = defaultFootsteps;
        currentLandSfx = defaultLandSfx;
    }

    private void Update() => Footsteps();
    private void SetSound(float pitch, AudioClip clip, bool resetDistance)
    {
        source.pitch = pitch;
        source.clip = clip;
        source.PlayOneShot(source.clip);
        if (resetDistance) Distance = 0f;
    }

    public void PlayJumpSound() => SetSound(1f, jumpSound, false);

    public void PlayCrouchSound()
    {
        if (!playerMovement.jumped) SetSound(1f, crouchSound, false);
    }

    public void PlayFootsteps()
    {
        RaycastMaterial();
        SetSound(Random.Range(0.8f, 1f), currentFootsteps[Random.Range(0, currentFootsteps.Length)], true);
    }
    
    public void PlayLandSound()
    {
        RaycastMaterial();
        if (playerMovement.coyoteTime < playerMovement.moveData.fallTimeMax) return;
        SetSound(1f, currentLandSfx[Random.Range(0, currentLandSfx.Length)], true);
    }

    private AudioClip[] currentLandSfx;
    private AudioClip[] currentFootsteps;
    private void Footsteps()
	{
		if (playerMovement.isCrouching) return;
		if (!playerMovement.CoyoteGrounded()) return;
		float speed = playerMovement.velocity.magnitude;
		if (speed > 20f) speed = 20f;
        if (playerMovement.isMoving) Distance += speed * footstepRate * Time.deltaTime * 50f;
        else Distance = 0;
		if (Distance < 100f / 1f) return;
        PlayFootsteps();
	}

    private void RaycastMaterial()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit footHit, (playerManager.GetCharacterController().height / 2) +
        playerMovement.groundDistance, playerMovement.moveData.groundMask))
        {
            currentMaterial = System.Array.IndexOf(MaterialManager.Instance.materialTags, footHit.transform.gameObject.tag);
            currentFootsteps = currentMaterial >= 0 ? footsteps[currentMaterial].audioClip : defaultFootsteps;
            currentLandSfx = currentMaterial >= 0 ? landSfx[currentMaterial].audioClip : defaultLandSfx;
        }
    }
}

[System.Serializable]
public struct AudioWrapper
{
    public AudioClip[] audioClip;
}