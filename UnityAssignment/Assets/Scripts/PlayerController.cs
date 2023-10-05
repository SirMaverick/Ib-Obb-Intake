using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The Player Controller that requires a CharacterController script.
/// All the 
/// </summary>

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;
    public Animator animator;

    enum AnimationState {
        IDLE,
        WALK_SLOW,
        WALK,
        RUN,
        JUMP,
        FALL
    };

    private AnimationState animState = AnimationState.IDLE;

    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float jumpSpeed = 0.01f;
    [SerializeField] private float gravity = 0.014f;
    [SerializeField] private float maxGravity = 0.03f;
    [SerializeField] private bool isSwitchable = true;
    [SerializeField] private float minimumSwitchVelocity = 0.005f;

    [SerializeField] bool pressJump, isGrounded, jumpIsReady = false;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundedLayers;
    [SerializeField] private float distToGround = 0.05f;
    [SerializeField] private float distToPushPlayer = 0.2f;
    [SerializeField] private float distToUpwardsPlayer = 0.5f;

    [SerializeField] private float maxJumpCooldown = 0.5f;
    [SerializeField] private float currentJumpCooldown;

    [SerializeField] private float verticalVelocity = 0.0f;
    [SerializeField] private float horizontalVelocity = 0.0f;
    [SerializeField] private float pushInput = 0.0f;
    [SerializeField] private float liftInput = 0.0f;

    [SerializeField] private Vector2 movementInput = Vector2.zero;

    PlayerController pushingPlayer, playerOnTop;

    public class PushContainer {
        public bool pushRight, pushLeft, pushUpwards;
        public PlayerController playerPushed, playerOnTop;
	}
    
    // Raycast to the ground to check if the character is standing on the ground.
    public bool IsGrounded()
    {
        bool _isGrounded = Physics.Raycast(groundCheck.position, -groundCheck.up, distToGround, groundedLayers);
        return _isGrounded;
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Get the movement input from Input Action
    public void OnMove(InputAction.CallbackContext context) {
        movementInput = context.ReadValue<Vector2>();
	}

    // Get the jump input from Input Action
    public void OnJump(InputAction.CallbackContext context) {
        pressJump = context.action.triggered;
	}

    // Calculate all movement vectors and timers before applying them to the character.
    // I like to have my update clean and prefer to use functions over huge code snippets and calculations in my Update().
    void Update()
    {
        CooldownTimer();
        CalculateHorizontalMovement();
        CalculateJump();
        CalculateGravity();
        MovePlayer();
        CalculateAnimatorState();
        AnimatePlayer();
    }

    void MovePlayer() {
        Vector3 _movement = new Vector3(horizontalVelocity, verticalVelocity, 0.0f);
        characterController.Move(_movement);
    }

    void CalculateAnimatorState() {

        if(Mathf.Abs(verticalVelocity) > 0.001f) {
            print("velocity > 0.001f");
            if(verticalVelocity > 0) {
                animState = AnimationState.JUMP;
            } else {
                animState = AnimationState.FALL;
            }
        } else if(Mathf.Abs(horizontalVelocity) != 0) {
            print("horizontalVelocity : " + horizontalVelocity);
            animator.SetBool("isMovingRight", true);
            if(horizontalVelocity > 0.0f * speed * Time.deltaTime) {
                animState = AnimationState.WALK_SLOW;
                if(horizontalVelocity > 0.3f * speed *Time.deltaTime) {
                    animState = AnimationState.WALK;
                    if(horizontalVelocity > 0.8f * speed * Time.deltaTime) {
                        animState = AnimationState.RUN;
					}
				}
			} else if(horizontalVelocity < 0.0f * speed * Time.deltaTime) {
                animator.SetBool("isMovingLeft", true);
                animState = AnimationState.WALK_SLOW;
                if(horizontalVelocity < -0.3f * speed * Time.deltaTime) {
                    animState = AnimationState.WALK;
                    if(horizontalVelocity < -0.8f * speed * Time.deltaTime) {
                        animState = AnimationState.RUN;
                    }
                    
                }
                
            }
            
		} else {
            animState = AnimationState.IDLE;
        }


    }

    void AnimatePlayer() {
        switch(animState) {
            case AnimationState.IDLE:
                ResetAnimatorValues();
                break;
            case AnimationState.WALK_SLOW:
                ResetAnimatorValues();
                animator.SetBool("isWalkingSlow", true);
                break;
            case AnimationState.WALK:
                ResetAnimatorValues();
                animator.SetBool("isWalking", true);
                break;
            case AnimationState.RUN:
                ResetAnimatorValues();
                animator.SetBool("isRunning", true);
                break;
            case AnimationState.JUMP:
                ResetAnimatorValues();
                animator.SetBool("isJumping", true);
                break;
            case AnimationState.FALL:
                ResetAnimatorValues();
                animator.SetBool("isFalling", true);
                break;
        }
	}

    void ResetAnimatorValues() {
        animator.SetBool("isWalkingSlow", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isJumping", false);
        animator.SetBool("isFalling", false);
        animator.SetBool("isMovingLeft", false);
        animator.SetBool("isMovingRight", false);
    }

    // The gravity switch will call this function on the player when the player enters a gravity switch
    public void Switch() {
        if(isSwitchable) {
            gravity = -gravity;
            maxGravity = -maxGravity;
            jumpSpeed = -jumpSpeed;
            transform.Rotate(new Vector3(180, 0, 0));
            if(Mathf.Sign(verticalVelocity) == 1) {
                if(verticalVelocity < minimumSwitchVelocity) {
                    verticalVelocity = minimumSwitchVelocity;
                } 
            } else {
                if(verticalVelocity > -minimumSwitchVelocity) {
                    verticalVelocity = -minimumSwitchVelocity;
                }
            }
        }
    }

    // Calculate the movement based on the input and add it to the horizontalVelocity
    void CalculateHorizontalMovement()
    {
        PushPlayer(movementInput.x);
        //If there is a positive input (right on Axis) detected.
        if(movementInput.x > 0) {
            //If the given input is bigger than the incoming input by another player
            if(movementInput.x >= pushInput) {
                horizontalVelocity = movementInput.x * speed * Time.deltaTime;
                pushInput = 0.0f;
            } else if(movementInput.x < pushInput) {
                horizontalVelocity = pushInput * speed * Time.deltaTime;
            }
        } //If there is a negative input (Left on Axis) detected. 
        else if(movementInput.x < 0) {
            if(movementInput.x <= pushInput) {
                horizontalVelocity = movementInput.x * speed * Time.deltaTime;
                pushInput = 0.0f;
            } else if(movementInput.x > pushInput) {
                horizontalVelocity = pushInput * speed * Time.deltaTime;
            }
        } //If there is no input detected 
        else if(movementInput.x == 0){
            horizontalVelocity = pushInput * speed * Time.deltaTime;
		}
    }

    // Calculate the jump force and if the player is being carried by another player and add it to the verticalVelocity.
    void CalculateJump() {
        LiftPlayer(verticalVelocity);
        isGrounded = IsGrounded();
        if(isGrounded) {
            if(jumpIsReady && pressJump) { 
                verticalVelocity = jumpSpeed * Time.deltaTime;
                LiftPlayer(jumpSpeed);
                jumpIsReady = false;
                isGrounded = false;
            } else if (liftInput > 0) {
                verticalVelocity = liftInput * Time.deltaTime;
			}
        }
    }

    // Calculate the gravity value and add it to the verticalVelocity.
    void CalculateGravity() {
        if(isGrounded && jumpIsReady) {
            verticalVelocity = 0.0f;
        } else {
            if(gravity > 0) {
                verticalVelocity -= gravity * Time.deltaTime;
                if(verticalVelocity < -maxGravity) {
                    verticalVelocity = -maxGravity;
                }
            } else if(gravity < 0) {
                verticalVelocity -= gravity * Time.deltaTime;
                if(verticalVelocity > -maxGravity) {
                    verticalVelocity = -maxGravity;
                }
            }
        }
    }

    // Prevent the player from multiple jumps (When jumping on narrow slopes)
    void CooldownTimer()
    {
        if (!jumpIsReady)
        {
            if (currentJumpCooldown < maxJumpCooldown)
            {
                currentJumpCooldown += Time.deltaTime;
            }
            else
            {
                jumpIsReady = true;
                currentJumpCooldown = 0;
            }
        }
    }

    // The enemy calls this function to tell the player that he hit him.
    public void GetHit() {
        Destroy(gameObject);
	}

    void PushPlayer (float pushValue) {
        PushContainer pushInfo = DetectPlayerCollision();
        if(pushInfo.playerPushed != null) {
            pushingPlayer = pushInfo.playerPushed;
            if(pushInfo.pushRight && pushValue > 0) {
                pushInfo.playerPushed.GetPushed(pushValue);
            } else if(pushInfo.pushLeft && pushValue < 0) {
                pushInfo.playerPushed.GetPushed(pushValue);
            }
        } else {
            if(pushingPlayer != null) {
                pushingPlayer.StopGettingPushed();
                pushingPlayer = null;
			}
		}
	}

    //Check if the player is lifting another player
    void LiftPlayer(float jumpValue) {
        PushContainer pushInfo = DetectPlayerCollision();
        if(pushInfo.playerOnTop != null) {
            playerOnTop = pushInfo.playerOnTop;
            if(pushInfo.pushUpwards && jumpValue > 0) {
                pushInfo.playerOnTop.GetLifted(jumpValue);
            }
        } else {
            if(playerOnTop != null) {
                playerOnTop.StopGettingLifted();
                playerOnTop = null;
            }
        }
    }

    //Called by the other player if you are being lifted
    void GetLifted(float jumpValue) {
        liftInput = jumpValue;
    }

    //Called by the other player if you are not on top of him anymore
    public void StopGettingLifted() {
        liftInput = 0.0f;
    }
    
    //Called by the other player if you are being pushed.
    public void GetPushed(float pushValue) {
        pushInput = pushValue;
	}
    
    //Called by the other player if you are not in reach anymore for the other player.
    public void StopGettingPushed() {
        pushInput = 0.0f;
	}


    //Check all sides if there is a player nearby that can be pushed/ lifted.
    PushContainer DetectPlayerCollision() {
        PushContainer _pushInfo = new PushContainer() {
            pushLeft = false,
            pushRight = false,
            pushUpwards = false,
            playerOnTop = null,
            playerPushed = null,
        };
        RaycastHit hitRight = new RaycastHit();
        RaycastHit hitLeft = new RaycastHit();
        RaycastHit hitUpwards = new RaycastHit();
        if(Physics.Raycast(transform.position, transform.right, out hitRight, distToPushPlayer)) {
            if(hitRight.collider.tag == "Player") {
                _pushInfo.playerPushed = hitRight.collider.gameObject.GetComponent<PlayerController>();
                _pushInfo.pushRight = true;
            } 
        } else if (Physics.Raycast(transform.position, -transform.right, out hitLeft, distToPushPlayer)) {
            if(hitLeft.collider.tag == "Player") {
                _pushInfo.playerPushed = hitLeft.collider.gameObject.GetComponent<PlayerController>();
                _pushInfo.pushLeft = true;
            }
        }

        if(Physics.Raycast(transform.position, transform.up, out hitUpwards, distToUpwardsPlayer)) {
            if(hitUpwards.collider.tag == "Player") {
                _pushInfo.playerOnTop = hitUpwards.collider.gameObject.GetComponent<PlayerController>();
                _pushInfo.pushUpwards = true;
			}
		}

        return _pushInfo;
    }
}
