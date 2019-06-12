using UnityEngine;

public class MainMenu : MonoBehaviour {

	private LevelManager levelManager;


	private NetworkController networkController;

	private void Start()
	{
		GameController gameController = FindObjectOfType<GameController>();
		networkController = (NetworkController)gameController.GetController(typeof(NetworkController)) as NetworkController;
		levelManager = (LevelManager)gameController.GetController(typeof(LevelManager)) as LevelManager;
	}

	public void HostGameButton()
	{
		networkController.HostGame();
	}

	public void JoinGameButton()
	{
		networkController.JoinGame();
	}

	public void QuitButton()
	{
		levelManager.QuitRequest();
	}

}
