using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NetworkPlayerController : NetworkBehaviour
{   
    public int maxHealth = 15;
    private int currentHealth;
    private bool isDead = false;

    private PlayerHealthUI healthUI;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public float mouseSensitivity = 150f;
    public Transform cameraTransform;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isGrounded;
    private bool wasGrounded;

    // Local-only jump state
    private bool localIsJumping = false;

    private Animation anim;

    private NetworkVariable<int> netHealth = new NetworkVariable<int>(
        15,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<float> netMoveSpeed = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private NetworkVariable<bool> netIsJumping = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public NetworkVariable<bool> netIsDead = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        controller = GetComponent<CharacterController>();

        if (IsServer)
        {
            currentHealth = maxHealth;
        }
        healthUI = GetComponentInChildren<PlayerHealthUI>();
        if (healthUI != null)
        {
            healthUI.SetMaxHealth(maxHealth);
            healthUI.SetHealth(netHealth.Value);
            healthUI.faceCamera = Camera.main.transform;
        }

        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (cameraTransform != null)
        {
            cameraTransform.gameObject.SetActive(false);
        }

        anim = GetComponentInChildren<Animation>();
        if (anim != null && anim.GetClip("idle") != null)
        {
            anim.Play("idle");
        }

        netHealth.OnValueChanged += (oldValue, newValue) =>
        {
            if (healthUI != null)
            {
                healthUI.SetHealth(newValue);
            }
        };


        Debug.Log($"[{gameObject.name}] IsOwner: {IsOwner} | IsLocalPlayer: {IsLocalPlayer}");
    }

    void Update()
    {
        if (!IsOwner)
        {
            HandleRemoteAnimations();
            return;
        }

        if (isDead || netIsDead.Value)
        {
            if (anim && !anim.IsPlaying("die"))
            {
                anim.CrossFade("die");
            }
            return;
        }

        HandleMouseLook();
        HandleMovement();
        HandleLocalAnimations();
        HandleCursorUnlock();
    }
    public void TakeDamage(int damage)
    {
        if (!IsServer || isDead) return;

        currentHealth -= damage;
        netHealth.Value = currentHealth;
        // if (healthUI != null)
        // {
        //     healthUI.SetHealth(currentHealth);
        // }

        Debug.Log($"Player took {damage} damage, HP now {currentHealth}");
        if (currentHealth <= 0) { DieServer(); }
    }

    [ServerRpc]
    public void TakeDamageServerRpc(int damage)
    {
        if (!IsServer) return;
        TakeDamage(damage);
    }

    private void DieServer()
    {
        if (isDead) return;
        isDead = true;
        netIsDead.Value = true;
        Debug.Log($"[{name}] DieServer() called. Setting isDead = true. Playing 'die' anim if available...");

        if (anim && anim.GetClip("die") != null)
        {
            anim.CrossFade("die");
        }

        Debug.Log($"[{name}] Will attempt to respawn after 2 seconds...");
        Invoke(nameof(DespawnSelf), 0.27f);

        MyNetworkManager.Instance.ScheduleRespawn(OwnerClientId, 10f);
    }



    private void DespawnSelf()
    {
        Debug.Log($"{name} DespawnSelf() invoked...");
        NetworkObject.Despawn(true);
        Debug.Log($"{name} DespawnSelf() done");
    }
    public bool IsDead()
    {
        return isDead || netIsDead.Value;
    }

    
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (!wasGrounded && isGrounded && localIsJumping)
        {
            // Landing
            localIsJumping = false;
            netIsJumping.Value = false;
        }

        // Movement input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Sync horizontal speed for remote anim
        netMoveSpeed.Value = move.magnitude;

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            localIsJumping = true;
            netIsJumping.Value = true; // tell others we are jumping

            if (anim != null && anim.GetClip("jump") != null)
            {
                // local jump anim
                anim.CrossFade("jump");
            }
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleLocalAnimations()
    {
        if (anim == null) return;

        if (localIsJumping) return;

        // walk/idle
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(x, 0, z);

        if (move.magnitude > 0.1f)
        {
            if (!anim.IsPlaying("walk"))
            {
                anim.CrossFade("walk");
            }
        }
        else
        {
            if (!anim.IsPlaying("idle"))
            {
                anim.CrossFade("idle");
            }
        }
    }

    private void HandleRemoteAnimations()
    {
        // We are on a remote client
        if (anim == null) return;

        if (netIsDead.Value)
        {
            if (!anim.IsPlaying("die"))
            {
                anim.CrossFade("die");
            }
            return;
        }

        // read states from net vars
        bool isJumpingRemote = netIsJumping.Value;
        float remoteSpeed = netMoveSpeed.Value;

        if (isJumpingRemote)
        {
            // show jump
            if (!anim.IsPlaying("jump"))
            {
                anim.CrossFade("jump");
            }
            return;
        }

        // otherwise idle/walk
        if (remoteSpeed > 0.1f)
        {
            if (!anim.IsPlaying("walk"))
            {
                anim.CrossFade("walk");
            }
        }
        else
        {
            if (!anim.IsPlaying("idle"))
            {
                anim.CrossFade("idle");
            }
        }
    }

    private void HandleCursorUnlock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
