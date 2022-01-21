using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
	#region Variables
	//Inputs Déplacements
	private Vector2 _motion;
	private float _moveX;
	private float _moveY;

	//input du saut
	private bool _jump;
	#endregion

	#region Properties
	/// <summary>
	/// Déplacements du joueur
	/// </summary>
	public Vector2 Motion => _motion;
	/// <summary>
	/// Jump du joueur
	/// </summary>
	public bool Jump => _jump;
	#endregion

	#region BuildIn Methods
	void Update()
	{
		GetPlayerInputs();
	}
	#endregion

	#region Customs Methods
	/// <summary>
	/// recupère les inputs du players
	/// </summary>
	private void GetPlayerInputs()
	{
		//déplacements du joueur
		_moveX = Input.GetAxis("Horizontal");
		_moveY = Input.GetAxis("Vertical");
		_motion.Set(_moveX, _moveY);
		//jump du joueur
		_jump = Input.GetButtonDown("Jump");
	}
	#endregion
}
