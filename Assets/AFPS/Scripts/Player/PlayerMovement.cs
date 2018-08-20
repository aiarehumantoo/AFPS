using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Original Quake 3 port;
// https://github.com/Zinglish/quake3-movement-unity3d/blob/master/CPMPlayer.js



// Contains the command the user wishes upon the character
struct Inputs
{
    public float forwardMove;
    public float rightMove;
    public float upMove;
}

public class PlayerMovement : MonoBehaviour
{
    float gravity = 20.0f;      // Gravity
    float friction = 6;         // Ground friction

    // Q3: players can queue the next jump just before he hits the ground
    private bool wishJump = false;

    // Used to display real time friction values
    private float playerFriction = 0.0f;

    #region Audio
    // An array of sounds that will be randomly selected from
    public AudioClip[] m_PlaySounds;        // Used to play sounds
    public AudioClip[] m_JumpSounds;
    public AudioClip[] m_LandingSounds;
    public AudioClip[] m_FootStepSounds;
    private AudioSource m_AudioSource;
    float audioTimer;       //Timer for footsteps
    float timeBetweenFootSteps = 0.3f;
    #endregion

    // Player commands
    private Inputs _inputs;

    #region MouseControls

    //Camera
    public Transform playerView;
    public float playerViewYOffset = 0.6f; // The height at which the camera is bound to
    public float xMouseSensitivity = 20.0f;
    public float yMouseSensitivity = 20.0f;

    // Camera rotations
    private float rotX = 0.0f;
    private float rotY = 0.0f;
    private Vector3 moveDirectionNorm = Vector3.zero;
    private Vector3 playerVelocity = Vector3.zero;
    private float playerTopVelocity = 0.0f;

    float mouseYaw = 0.022f;     //mouse yaw/pitch. Overwatch = 0.0066, Quake 0.022

    #endregion

    #region MovementVariables
    //Variables for movement

    float moveSpeed = 7.0f;                // Ground move speed
    float runAcceleration = 10.0f;         // Ground accel
    float runDeacceleration = 6.0f;       // Deacceleration that occurs when running on the ground
    float airAcceleration = 0.1f;          // Air accel
    float airDecceleration = 0.1f;         // Deacceleration experienced when ooposite strafing
    float airControl = 0.3f;               // How precise air control is
    float sideStrafeAcceleration = 100.0f;  // How fast acceleration occurs to get up to sideStrafeSpeed when
    float sideStrafeSpeed = 1.0f;          // What the max speed to generate when side strafing
    float jumpSpeed = 7.0f;                // The speed at which the character's up axis gains when hitting jump
    float moveScale = 1.0f;

    bool m_PreviouslyGrounded = true;

    #endregion

    #region DoubleJumpVariables
    //Variables for doublejump. Second jump within is higher, expires after short period

    float timer = 1.0f;                             // Timer.
    float doubleJumpWindow = 0.6f;                  // How long player has time to perform second,  higher jump.
    float doubleJumpSpeed = 15.0f;

    #endregion

    private CharacterController _controller;

    private void Start()
    {
        // Hide the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Put the camera inside the capsule collider
        playerView.position = new Vector3(transform.position.x, transform.position.y + playerViewYOffset, transform.position.z);

        m_AudioSource = GetComponent<AudioSource>();
        _controller = GetComponent<CharacterController>();

        Settings();
    }

    private void Update()
    {
        #region MouseControls

        /* Ensure that the cursor is locked into the screen */
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetButtonDown("Fire1"))
                Cursor.lockState = CursorLockMode.Locked;
        }

        /* Camera rotation stuff, mouse controls this shit */
        rotX -= Input.GetAxisRaw("Mouse Y") * xMouseSensitivity * mouseYaw;
        rotY += Input.GetAxisRaw("Mouse X") * yMouseSensitivity * mouseYaw;

        // Clamp the X rotation
        if (rotX < -90)
            rotX = -90;
        else if (rotX > 90)
            rotX = 90;

        this.transform.rotation = Quaternion.Euler(0, rotY, 0); // Rotates the collider
        playerView.rotation = Quaternion.Euler(rotX, rotY, 0); // Rotates the camera

        #endregion

        #region Movement

        QueueJump();

        // Add the time since Update was last called to the timer. Count up to 1 second.
        if (timer < 1.0f)
        {
            timer += Time.deltaTime;
        }

        if (audioTimer < timeBetweenFootSteps)
        {
            audioTimer += Time.deltaTime;
        }


        // Enable landing sound when falling velocity exceeds 4ups
        if (_controller.velocity.y <= -4 && m_PreviouslyGrounded)
        {
            //Debug.Log("landing sound enabled");
            m_PreviouslyGrounded = false;
        }

        if (_controller.isGrounded)
        {
            GroundMove();

            // Current code resets vertical velocity when player touches the ground. So its like ground --> air --> ground cycle, results in audio spam. FIX            Either change how gravity works or require set amount of air time before landing sound can be triggered
            // Landing
            if (!m_PreviouslyGrounded)
            {
                m_PreviouslyGrounded = true;
                PlayLandingSound();
                //Debug.Log("landing sound disabled");
            }
            else if ((playerVelocity.x >= 1 || playerVelocity.z >= 1) && audioTimer >= timeBetweenFootSteps)     // Quick & dirty check if player is moving + delay between sounds
            {
                audioTimer = 0f;
                PlayFootStepAudio();
            }
        }
        else if (!_controller.isGrounded)
        {
            //m_PreviouslyGrounded = false;
            AirMove();
        }

        // Move the controller
        _controller.Move(playerVelocity * Time.deltaTime);

        //Need to move the camera after the player has been moved because otherwise the camera will clip the player if going fast enough and will always be 1 frame behind.
        // Set the camera's position to the transform
        playerView.position = new Vector3(transform.position.x, transform.position.y + playerViewYOffset, transform.position.z);

        #endregion
    }

    private void SetMovementDir()
    {
        _inputs.forwardMove = Input.GetAxisRaw("Vertical");
        _inputs.rightMove = Input.GetAxisRaw("Horizontal");
    }

    private void QueueJump()
    {
        if (Input.GetButtonDown("Jump") && !wishJump)
        {
            wishJump = true;
        }
        if (Input.GetButtonUp("Jump"))
        {
            wishJump = false;
        }
    }

    private void GroundMove()
    {
        //TODO; remake or tweak for snappier controls.

        Vector3 wishdir;

        // Do not apply friction if the player is queueing up the next jump
        if (!wishJump)
            ApplyFriction(1.0f);
        else
            ApplyFriction(0);

        float scale = InputScale();

        wishdir = new Vector3(_inputs.rightMove, 0, _inputs.forwardMove);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        var wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        Accelerate(wishdir, wishspeed, runAcceleration);

        // Reset the gravity velocity
        playerVelocity.y = 0f;

        if (wishJump)
        {
            /*
            if (timer <= doubleJumpWindow)
            {
                // Doublejump
                playerVelocity.y = doubleJumpSpeed;
            }
            else if (timer >= doubleJumpWindow)
            {
                // Reset timer
                timer = 0f;
                // Normal jump
                playerVelocity.y = jumpSpeed;
            }
            */
            playerVelocity.y = jumpSpeed;               // doublejump disabled for now

            PlayJumpSound();
            wishJump = false;
        }
    }

    private void AirMove()
    {
        Vector3 wishdir;
        float wishvel = airAcceleration;
        float accel;

        float scale = InputScale();

        SetMovementDir();

        wishdir = new Vector3(_inputs.rightMove, 0, _inputs.forwardMove);
        wishdir = transform.TransformDirection(wishdir);

        float wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        wishdir.Normalize();
        moveDirectionNorm = wishdir;
        wishspeed *= scale;

        // CPM: Aircontrol
        float wishspeed2 = wishspeed;
        if (Vector3.Dot(playerVelocity, wishdir) < 0)
            accel = airDecceleration;
        else
            accel = airAcceleration;
        // If the player is ONLY strafing left or right
        if (_inputs.forwardMove == 0 && _inputs.rightMove != 0)
        {
            if (wishspeed > sideStrafeSpeed)
                wishspeed = sideStrafeSpeed;
            accel = sideStrafeAcceleration;
        }

        Accelerate(wishdir, wishspeed, accel);
        if (airControl > 0)
            AirControl(wishdir, wishspeed2);
        // !CPM: Aircontrol

        // Apply gravity
        playerVelocity.y -= gravity * Time.deltaTime;
    }

    private void AirControl(Vector3 wishdir, float wishspeed)
    {
        float zspeed;
        float speed;
        float dot;
        float k;

        // Can't control movement if not moving forward or backward
        if (Mathf.Abs(_inputs.forwardMove) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
            return;
        zspeed = playerVelocity.y;
        playerVelocity.y = 0;
        /* Next two lines are equivalent to idTech's VectorNormalize() */
        speed = playerVelocity.magnitude;
        playerVelocity.Normalize();

        dot = Vector3.Dot(playerVelocity, wishdir);
        k = 32;
        k *= airControl * dot * dot * Time.deltaTime;

        // Change direction while slowing down
        if (dot > 0)
        {
            playerVelocity.x = playerVelocity.x * speed + wishdir.x * k;
            playerVelocity.y = playerVelocity.y * speed + wishdir.y * k;
            playerVelocity.z = playerVelocity.z * speed + wishdir.z * k;

            playerVelocity.Normalize();
            moveDirectionNorm = playerVelocity;
        }

        playerVelocity.x *= speed;
        playerVelocity.y = zspeed; // Note this line
        playerVelocity.z *= speed;
    }

    private void ApplyFriction(float t)
    {
        Vector3 vec = playerVelocity; // Equivalent to: VectorCopy();
        float speed;
        float newspeed;
        float control;
        float drop;

        vec.y = 0.0f;
        speed = vec.magnitude;
        drop = 0.0f;

        /* Only if the player is on the ground then apply friction */
        if (_controller.isGrounded)
        {
            control = speed < runDeacceleration ? runDeacceleration : speed;
            drop = control * friction * Time.deltaTime * t;
        }

        newspeed = speed - drop;
        playerFriction = newspeed;
        if (newspeed < 0)
            newspeed = 0;
        if (speed > 0)
            newspeed /= speed;

        playerVelocity.x *= newspeed;
        playerVelocity.z *= newspeed;
    }

    private void Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        float addspeed;
        float accelspeed;
        float currentspeed;

        currentspeed = Vector3.Dot(playerVelocity, wishdir);
        addspeed = wishspeed - currentspeed;
        if (addspeed <= 0)
            return;
        accelspeed = accel * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
    }

    private float InputScale()
    {
        int max;
        float total;
        float scale;

        max = (int)Mathf.Abs(_inputs.forwardMove);
        if (Mathf.Abs(_inputs.rightMove) > max)
            max = (int)Mathf.Abs(_inputs.rightMove);
        if (max <= 0)
            return 0;

        total = Mathf.Sqrt(_inputs.forwardMove * _inputs.forwardMove + _inputs.rightMove * _inputs.rightMove);
        scale = moveSpeed * max / (moveScale * total);

        return scale;
    }



    public void Settings()
    {
        //Read & copy settings file OnStart + when closing menu
    }


    #region Sounds

    // Change audio + play sound

    private void PlayJumpSound()
    {
        //Debug.Log("jump sound");
        m_PlaySounds = m_JumpSounds;
        PlayRandomAudio();
    }

    private void PlayLandingSound()
    {
        //Debug.Log("landing sound");
        //m_PlaySounds = m_LandingSounds;       // no unique landing sounds yet
        m_PlaySounds = m_JumpSounds;
        PlayRandomAudio();
    }

    private void PlayFootStepAudio()
    {
        //Debug.Log("footstep sound");
        m_PlaySounds = m_JumpSounds;
        //m_PlaySounds = m_FootStepSounds;      // no unique footstep sounds yet
        PlayRandomAudio();
    }

    private void PlayLadderAudio()
    {

    }

    private void PlayRandomAudio()
    {
        // pick & play a random jump sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_PlaySounds.Length);
        m_AudioSource.clip = m_PlaySounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        m_PlaySounds[n] = m_PlaySounds[0];
        m_PlaySounds[0] = m_AudioSource.clip;
    }

    #endregion
}
