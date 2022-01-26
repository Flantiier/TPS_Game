using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputs : MonoBehaviour
{
    #region Variables
    //new input system component
    private PlayerInput _inputs;

    //motion inputs
    private Vector2 _motion;
    //jump input
    private bool _jump;

    //run input
    private bool _run;
    //crouch input
    private bool _crouch;
    //aim input
    private bool _aim;
    #endregion

    #region Properties
    /// <summary>
    /// motion inputs 
    /// </summary>
    public Vector2 Motion => _motion;
    /// <summary>
    /// jump input
    /// </summary>
    public bool Jump => _jump;
    /// <summary>
    /// run input
    /// </summary>
    public bool Run => _run;
    /// <summary>
    /// crouch input
    /// </summary>
    public bool Crouch => _crouch;
    /// <summary>
    /// aim input
    /// </summary>
    public bool Aim => _aim;
    #endregion

    #region BuildIn Methods
    void Start()
    {
        Initialize();
    }

    void Update()
    {
        GetInputs();
    }
    #endregion

    #region Customs Methods
    /// <summary>
    /// initiate method
    /// </summary>
    private void Initialize()
    {
        _inputs = GetComponent<PlayerInput>();
    }
    
    /// <summary>
    /// get inputs of the player
    /// </summary>
    private void GetInputs()
    {
        //motion inputs
        _motion = _inputs.actions["Move"].ReadValue<Vector2>();
        //jump inputs
        _jump = _inputs.actions["Jump"].triggered;

        PlayerAim();
        PlayerRun();
        PlayerCrouching();
    }

    /// <summary>
    /// set the run boolean 
    /// </summary>
    private void PlayerRun()
    {
        //Run inputs
        //si c'est un GamePad
        if (_inputs.currentControlScheme == "GamePad")
        {
            if(_inputs.actions["Move"].ReadValue<Vector2>().magnitude <= 0.7f) 
                _run = false;

            if (_inputs.actions["Run"].triggered)
                _run = !_run;
        }
        //si c'est un Keyboard
        else if (_inputs.currentControlScheme == "Keyboard")
        {
            if (_inputs.actions["Run"].ReadValue<float>() >= 0.1f)
                _run = true;
            else
                _run = false;
        }
    }

    /// <summary>
    /// set the crouxh boolean
    /// </summary>
    private void PlayerCrouching()
    {
        //crouch inputs
        //si le joueur run, on empeche le crouch
        if (_run)
        {
            _crouch = false;
            return;
        }

        //sinon le joueur peut s'accroupir
        if (_inputs.actions["Crouch"].triggered)
            _crouch = !_crouch;
    }

    private void PlayerAim()
    {
        if (_inputs.actions["Aim"].ReadValue<float>() >= 0.1f)
            _aim = true;
        else
            _aim = false;
    }
    #endregion
}
