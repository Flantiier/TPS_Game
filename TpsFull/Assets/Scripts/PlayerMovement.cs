using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    #region Variables
    [Header("Motion Parameters")]
    /// <summary>
    /// Camera du joueur
    /// </summary>
    [SerializeField]
    private Transform _cam;
    /// <summary>
    /// vitesse de déplacements du player
    /// </summary>
    [SerializeField]
    private float walkSpeed;
    /// <summary>
    /// vitesse quand le joueur est accroupi
    /// </summary>
    [SerializeField]
    private float crouchSpeed;
    /// <summary>
    /// vitesse de course du player
    /// </summary>
    [SerializeField]
    private float runningSpeed;
    /// <summary>
    /// vitesse de rotation du player
    /// </summary>
    [SerializeField]
    private float rotateSpeedPlayer;
    /// <summary>
    /// ratio pour smooth la valeur de la vitesse
    /// </summary>
    [SerializeField]
    private float smoothSpeedRatio;

    [Header("Jump Parameters")]
    /// <summary>
    /// hauteur du saut du joueur
    /// </summary>
    [SerializeField]
    private float jumpHeight;
    /// <summary>
    /// valeur de la gravité appliquée au joueur
    /// </summary>
    [SerializeField]
    private float gravityValue;

    [Header("Animations Parameters")]
    /// <summary>
    /// Animator controller du player
    /// </summary>
    [SerializeField]
    private RuntimeAnimatorController playerAnimController;
    /// <summary>
    /// Avatar du player
    /// </summary>
    [SerializeField]
    private Avatar playerAvatar;

    //Components To Get, player
    private PlayerInput _inputActions;
    private CharacterController _cc;
    private Animator _animator;

    //Inputs Actions
    /// <summary>
    /// Move Inputs
    /// </summary>
    private InputAction Move => _inputActions.actions["Move"];
    /// <summary>
    /// Jump Inputs
    /// </summary>
    private InputAction Jump => _inputActions.actions["Jump"];
    /// <summary>
    /// Run Inputs
    /// </summary>
    private InputAction Run => _inputActions.actions["Run"];
    /// <summary>
    /// Crouch Inputs
    /// </summary>
    private InputAction Crouch => _inputActions.actions["Crouch"];

    //Déplacements du joueur
    /// <summary>
    /// Déplacement complet du joueur, motion et jump
    /// </summary>
    private Vector3 _motion;
    /// <summary>
    /// Motion du joueur
    /// </summary>
    private Vector3 _direction;
    /// <summary>
    /// vitesse du joueur sur l'axe Y
    /// </summary>
    private float _verticalVelocity;

    //Vitesse du joueur
    private float _currentSpeed;
    private float _targetSpeed;

    //Saut en courant
    private bool _runJump = false;
    private bool _isRunning = false;

    //Crouch joueur
    private bool _crouch;

    //Rotation du joueur
    private float _turnSmoothVelocity;
    #endregion

    #region Properties
    #endregion

    #region BuildIn Methods
    void Start()
    {
        Initiate();
    }

    void Update()
    {
        Motion();
        UpdateAnims();
    }
    #endregion

    #region Customs Methods
    /// <summary>
    /// init method
    /// </summary>
    private void Initiate()
    {
        //motion
        _inputActions = GetComponent<PlayerInput>();
        _cc = GetComponent<CharacterController>();

        //anims
        _animator = GetComponent<Animator>();
        _animator.runtimeAnimatorController = playerAnimController;
        _animator.avatar = playerAvatar;
    }

    //Methods des déplcements du joueur
    #region Player Motion
    /// <summary>
    /// Player Motion Method
    /// </summary>
    private void Motion()
    {
        PlayerRunning();
        PlayerCrouching();
        VerticalMovement();

        //Mouvement joueur
        Vector2 inputs = Move.ReadValue<Vector2>();
        _direction.Set(inputs.x, 0f, inputs.y);

        Vector3 moveDir = new Vector3();

        if (_direction.normalized.magnitude >= 0.1f)
        {
            //calcul de l'angle vers lequel le joueur se dirige + rotation de la cam
            float targetAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg + _cam.eulerAngles.y;
            //smooth l'angle
            float dampedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, rotateSpeedPlayer);
            //rotation du joueur
            transform.rotation = Quaternion.Euler(0f, dampedAngle, 0f);

            //on fait aller le joueur vers la direction qu'il regarde
            moveDir = Quaternion.Euler(0f, dampedAngle, 0f) * Vector3.forward;
        }

        //set la vitesse du joueur
        SetPlayerSpeed();

        _motion = moveDir.normalized * (_currentSpeed * Time.deltaTime);

    }


    /// <summary>
    /// Vertical Velocity Movements
    /// </summary>
    private void VerticalMovement()
    {
        //si le player est grounded
        if (_cc.isGrounded)
        {
            //applique une gravité faible
            _verticalVelocity = -gravityValue * 0.3f;

            //si le player appuie sur Jump et qu'il est grounded
            if (Jump.triggered)
            {
                //si le joueur saute en courant sa speed, il garde sa vitesse de course
                if (_isRunning)
                    _runJump = true;

                //on regarde s'il est accroupi
                if(_crouch)
                    _verticalVelocity = jumpHeight * 0.75f;
                else
                    _verticalVelocity = jumpHeight;

                //anim
                _animator.SetTrigger("Jump");
            }
        }
        //sinon
        else
        {
            //si la valeur est proche de 0, on la set a 0
            if (Mathf.Approximately(_verticalVelocity, 0f))
                _verticalVelocity = 0f;

            _verticalVelocity -= gravityValue * Time.deltaTime;
        }

        //applique les deplacements au joueur
        _motion += _verticalVelocity * Vector3.up * Time.deltaTime;
        _cc.Move(_motion);

        //si le joueur courait au moment du saut 
        if (_runJump)
            //si le cc est grounded
            if (_cc.isGrounded)
                //on repasse le runJump en false
                _runJump = false;
    }

    /// <summary>
    /// Set la vitesse du joueur en fonction des déplacements, smooth les changements de vitesses
    /// </summary>
    private void SetPlayerSpeed()
    {
        //si le joueur court ou à sauté en courant
        if (_direction.magnitude >= 0.1f && _cc.isGrounded && _isRunning || _runJump)
        {
            //la vitesse de sprint est visée
            _targetSpeed = runningSpeed;
        }
        //si le joueur est accroupi
        else if (_direction.magnitude >= 0.1f && _cc.isGrounded && _crouch)
        {
            //la vitsse de marche est visée
            _targetSpeed = crouchSpeed;
        }
        //sinon s'il marche
        else if (_direction.magnitude >= 0.1f)
        {
            //la vitsse de marche est visée
            _targetSpeed = walkSpeed;
        }
        //sinon
        else
            //la vitesse visée est 0
            _targetSpeed = 0;

        //si la vitesse doit est differente
        if (_currentSpeed != _targetSpeed)
        {
            //smooth la valeur de _currentSpeed vers la _targetSpeed
            _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, smoothSpeedRatio);
        }
    }

    /// <summary>
    /// Check si le player court, modifie le bool en fonction du device connecté
    /// </summary>
    private void PlayerRunning()
    {
        //si c'est un GamePad
        if (_inputActions.currentControlScheme == "GamePad")
        {
            if (Run.triggered)
            {
                _isRunning = !_isRunning;
            }
        }
        //si c'est un clavier/souris
        else if(_inputActions.currentControlScheme == "Keyboard")
        {
            if (Run.ReadValue<float>() > 0.1f)
                _isRunning = true;
            else
                _isRunning = false;
        }
    }
    
    /// <summary>
    /// Check si le joueur est accroupi
    /// </summary>
    private void PlayerCrouching()
    {
        if (_isRunning)
        {
            _crouch = false;
            return;
        }

        if (Crouch.triggered)
            _crouch = !_crouch;            
    }
    #endregion

    #region Animation Methods
    /// <summary>
    /// Mettre a jours les anims
    /// </summary>
    private void UpdateAnims()
    {
        //set la vitesse
        _animator.SetFloat("Speed", _direction.magnitude);

        //Grounded ou non
        if (!_cc.isGrounded)
        {
            _animator.SetBool("IsGrounded", false);
            _animator.SetFloat("VerticalSpeed", _verticalVelocity);
        }
        else
            _animator.SetBool("IsGrounded", true);

        //Crouch anim
        if (!_crouch)
            _animator.SetBool("Crouching", false);
        else
            _animator.SetBool("Crouching", true);

        //Sprint anim
        if (!_isRunning || _direction.magnitude <= 0f || !_cc.isGrounded)
            _animator.SetBool("Running", false);
        else if (_isRunning && _cc.isGrounded)
            _animator.SetBool("Running", true);
    }
    #endregion
}
#endregion
