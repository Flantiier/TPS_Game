using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputs))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    #region Variables
    [Header("Motion Parameters")]
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
    /// smooth ratio pour les changements de speed
    /// </summary>
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
    private PlayerInputs _inputs;
    private CharacterController _cc;
    private Transform _cam;
    private Animator _animator;

    //Déplacements du joueur
    private Vector3 _motion;
    private Vector3 _direction;
    private float _xValue;
    private float _zValue;
    private float _ySpeed;

    //Vitesse du joueur
    private float _currentSpeed;
    private float _targetSpeed;

    //Saut en courant
    private bool _runJump = false;

    //Crouch joueur
    private bool _crouch => _inputs.Crounch;

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
        VerticalMovevement();
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
        _inputs = GetComponent<PlayerInputs>();
        _cc = GetComponent<CharacterController>();

        //cam
        _cam = Camera.main.transform;

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
        //si il n'y a pas d'inputs
        if (!_inputs)
            return;

        //Mouvement joueur
        _xValue = _inputs.Motion.x;
        _zValue = _inputs.Motion.y;
        _direction.Set(_xValue, 0f, _zValue);

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
    private void VerticalMovevement()
    {
        //calcul du mouvement vertical du joueur sur l'axe y
        //si le player est grounded
        if (_cc.isGrounded)
        {
            //applique une gravité faible
            _ySpeed = -gravityValue * 0.3f;

            //si le player appuie sur Jump et qu'il est grounded
            if (_inputs.Jump)
            {
                //si le joueur saute en courant sa speed, il garde sa vitesse de course
                if (_inputs.Run)
                    _runJump = true;

                if(_inputs.Crounch)
                    //on lui aplique la force du saut
                    _ySpeed = jumpHeight * 0.75f;
                else
                    _ySpeed = jumpHeight;

                //anim
                _animator.SetTrigger("Jump");
            }
        }
        //sinon
        else
        {
            //si la valeur est proche de 0, on la set a 0
            if (Mathf.Approximately(_ySpeed, 0f))
                _ySpeed = 0f;

            _ySpeed -= gravityValue * Time.deltaTime;
        }

        //applique les deplacements au joueur
        _motion += _ySpeed * Vector3.up * Time.deltaTime;
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
        //smooth la speed du joueur
        //si le joueur court ou à sauté en courant
        if (_direction.magnitude >= 0.1f && _cc.isGrounded && _inputs.Run || _runJump)
        {
            //la vitesse de sprint est visée
            _targetSpeed = runningSpeed;
        }
        //si le joueur est accroupi
        else if (_direction.magnitude >= 0.1f && _cc.isGrounded && _inputs.Crounch)
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
            _animator.SetFloat("VerticalSpeed", _ySpeed);
        }
        else
            _animator.SetBool("IsGrounded", true);

        //Crouch anim
        if (!_crouch)
            _animator.SetBool("Crouching", false);
        else
            _animator.SetBool("Crouching", true);

        //Sprint anim
        if (!_inputs.Run || _direction.magnitude <= 0f || !_cc.isGrounded)
            _animator.SetBool("Running", false);
        else if (_inputs.Run && _cc.isGrounded)
            _animator.SetBool("Running", true);
    }
    #endregion
}
#endregion
