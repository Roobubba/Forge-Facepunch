using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
	[SerializeField]
	private GameObject quitButton;
	
	private NetworkController networkController;

    // Start is called before the first frame update
    void Start()
    {
		GameController gameController = FindObjectOfType<GameController>();
		networkController = (NetworkController)gameController.GetController(typeof(NetworkController)) as NetworkController;
	}

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			quitButton.SetActive(!quitButton.activeSelf);
		}      
    }

	public void QuitButtonPressed()
	{
		networkController.CancelGame();
	}
}
