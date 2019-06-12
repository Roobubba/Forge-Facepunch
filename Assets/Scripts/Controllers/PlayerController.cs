using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;

public class PlayerController : PlayerControllerBehavior
{
	[SerializeField]
	private GameObject myCamera;

	private Text myNetStatusText;
	private bool isReady = false;
	private float speed = 10f;
	private float mouseSensitivity = 5f;

	protected override void NetworkStart()
	{
		base.NetworkStart();

		if (!networkObject.IsOwner)
		{
			Destroy(myCamera);
		}
		else
		{
			myCamera.SetActive(true);
			// We are the owner of this networkObject
			myNetStatusText = GameObject.FindGameObjectWithTag("NetStatus").GetComponent<Text>();
			if (!myNetStatusText)
			{
				BeardedManStudios.Forge.Logging.BMSLog.Log("Error, could not find the Net Status Text object");
			}
			string steamID = Steamworks.SteamClient.SteamId.Value.ToString();
			string netStatus = "host";
			if (!NetworkManager.Instance.IsServer)
			{
				netStatus = "client";
			}
			myNetStatusText.text = steamID + " is " + netStatus;
		}
		isReady = true;
	}

	private void Update()
	{
		if (!isReady) return;
		if (networkObject.IsOwner)
		{
			//Movement code
			Move();
			Rotate();

			//Then set network values to our client-auth values
			networkObject.position = transform.position;
			networkObject.rotation = transform.rotation;
		}
		else
		{
			transform.position = networkObject.position;
			transform.rotation = networkObject.rotation;
		}
	}

	/// <summary>
	/// Simple movement code for the x-z plane
	/// </summary>
	private void Move()
	{
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");
		Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
		Vector3 moveAmount = moveDirection * speed * Time.deltaTime;
		Vector3 move = transform.TransformDirection(moveAmount);
		transform.position += move;
	}

	/// <summary>
	/// Simple mouse rotation code for the y-axis
	/// </summary>
	private void Rotate()
	{
		float mouseTurn = Input.GetAxisRaw("Mouse X");
		transform.Rotate(Vector3.up * mouseTurn * mouseSensitivity);
	}
}
