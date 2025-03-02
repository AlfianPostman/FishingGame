using UnityEngine;
using TMPro;

public class PlayerGeneral : MonoBehaviour 
{
    PlayerStateMachine PlayerStateMachine;
    public PlayerStateMachine PLAYERSTATEMACHINE => PlayerStateMachine;

    PlayerStateCollection States;
    public PlayerStateCollection STATES => States;
    
    InputProcessor InputProcessor;
    public InputProcessor INPUTPROCESSOR => InputProcessor;

    PlayerMovement PlayerMovement;
    public PlayerMovement MOVEMENT => PlayerMovement;

    public PlayerCollision COLLISION => playerCollision;

    CameraController cameraController;
    public CameraController CAMERACONTROLLER => cameraController;

    CameraTarget cameraTarget;
    public CameraTarget CAMERATARGET => cameraTarget;

    public DialogueManager DIALOGUEMANAGER => dialogueManager;
    public FishingController FISHINGCONTROLLER => fishingController;

    [Header("Components")]
    [SerializeField] Transform playerTransform;
    [SerializeField] Rigidbody rb;
    [SerializeField] PlayerConfiguration PlayerConfiguration;
    [SerializeField] PlayerCollision playerCollision;
    [SerializeField] DialogueManager dialogueManager;
    [SerializeField] FishingController fishingController;

    [Header("UI")]
    [SerializeField] TextMeshPro currentState;

    private void Awake() 
    {
        PlayerStateMachine = new PlayerStateMachine();
        States = new PlayerStateCollection(this, PlayerStateMachine);
        InputProcessor = new InputProcessor();
        PlayerMovement = new PlayerMovement(rb, PlayerConfiguration, playerTransform);
        cameraController = Camera.main.GetComponentInParent<CameraController>();
        cameraTarget = GetComponent<CameraTarget>();
    }

    void Start()
    {
        PlayerStateMachine.Init(States.IDLESTATE);
    }

    void Update()
    {
        PlayerStateMachine.state.Update();
        currentState.text = PlayerStateMachine.state.ToString();
    }

    void FixedUpdate()
    {
        PlayerStateMachine.state.FixedUpdate();
    }

    void LateUpdate()
    {
        PlayerStateMachine.state.LateUpdate();
    }
}