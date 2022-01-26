using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerAim : MonoBehaviour
{
    #region Variables
    /// <summary>
    /// VcamAim
    /// </summary>
    [SerializeField]
    private CinemachineVirtualCamera _camAim;

    private PlayerInputs _inputs;
    #endregion

    #region Properties
    #endregion

    #region BuildIn Methods
    void Start()
    {
        Initialize();
    }

    void Update()
    {
       /* Aiming();*/
    }
    #endregion

    #region Customs Methods
    private void Initialize()
    {
        _inputs = GetComponent<PlayerInputs>();
    }

    private void Aiming()
    {
        if (_inputs.Aim)
        {

        }
        else
        {

        }
    }
    #endregion
}
