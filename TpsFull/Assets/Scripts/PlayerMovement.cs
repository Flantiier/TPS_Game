using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputs))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    #region Variables
    [Header("Speed Parameters")]
    /// <summary>
    /// vitesse de déplacements du player
    /// </summary>
    [SerializeField]
    private float characterSpeed;
    /// <summary>
    /// vitesse de rotation du player
    /// </summary>
    [SerializeField]
    private float turnSmoothTime;

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

    //Rotation du joueur
    private float turnSmoothVelocity;
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
            float dampedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            //rotation du joueur
            transform.rotation = Quaternion.Euler(0f, dampedAngle, 0f);

            //on fait aller le joueur vers la direction qu'il regarde
            moveDir = Quaternion.Euler(0f, dampedAngle, 0f) * Vector3.forward;
        }

        //mets a jours le deplacement du joueur
        _motion = moveDir.normalized * (characterSpeed * Time.deltaTime);
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
            Debug.Log("Grounded");
            //applique une gravité faible
            _ySpeed = -gravityValue * 0.3f;

            //si le player appuie sur Jump et qu'il est grounded
            if (_inputs.Jump)
            {
                //on lui aplique la force du saut
                _ySpeed = jumpHeight;
                _animator.SetTrigger("Jump");
            }
        }
        //sinon
        else
        {
            Debug.Log("Pas Grounded");
            //si la valeur est proche de 0, on la set a 0
            if (Mathf.Approximately(_ySpeed, 0f))
                _ySpeed = 0f;

            _ySpeed -= gravityValue * Time.deltaTime;
        }

        //applique les deplacements au joueur
        _motion += _ySpeed * Vector3.up * Time.deltaTime;
        _cc.Move(_motion);
    }
    #endregion

    #region Animation Methods
    private void UpdateAnims()
    {
        _animator.SetFloat("Speed", _direction.magnitude);

        if (!_cc.isGrounded)
        {
            _animator.SetBool("IsGrounded", false);
            _animator.SetFloat("VerticalSpeed", _ySpeed);
        }
        else
            _animator.SetBool("IsGrounded", true);
    }
    #endregion
}
#endregion
