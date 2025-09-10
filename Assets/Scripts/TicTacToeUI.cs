using UnityEngine;
using UnityEngine.UI;

public class TicTacToeUI : MonoBehaviour
{
	[Header("UI References")]
	public Button[] cells = new Button[9];
	public Text statusText;
	public Button resetButton;
	public Text humanMarkLabel;
	public Text aiMarkLabel;
	public GameObject startPanel;
	public GameObject gamePanel;
	public Button startButton,quitButton;
	public Dropdown difficultyDropdown;
	public Dropdown firstPlayerDropdown; // 0: Human, 1: AI
	public Button backToMenuButton;

	[Header("Appearance")]
	public string humanMark = "X";
	public string aiMark = "O";
	public Color winHighlightColor = new Color(0.3f, 0.8f, 0.3f);
	public Color normalColor = Color.white;

	[Header("Gameplay")]
	public TicTacToeCore.Difficulty difficulty = TicTacToeCore.Difficulty.Hard;

	private TicTacToeCore core = new TicTacToeCore();

	private void Awake()
	{
		// 绑定按钮事件
		for (int i = 0; i < cells.Length; i++)
		{
			int idx = i;
			if (cells[i] != null)
			{
				cells[i].onClick.AddListener(() => OnCellClicked(idx));
			}
		}
		if (resetButton != null)
		{
			resetButton.onClick.AddListener(ResetGame);
		}
		if (startButton != null)
		{
			startButton.onClick.AddListener(StartGame);
		}
		if (difficultyDropdown != null)
		{
			difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
		}
		if (firstPlayerDropdown != null)
		{
			firstPlayerDropdown.onValueChanged.AddListener(OnFirstPlayerChanged);
		}
		if (backToMenuButton != null)
		{
			backToMenuButton.onClick.AddListener(BackToMenu);
		}
		if( quitButton != null )
		{
			quitButton.onClick.AddListener(ExitGame);
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < cells.Length; i++)
		{
			int idx = i;
			if (cells[i] != null)
			{
				cells[i].onClick.RemoveAllListeners();
			}
		}
		if (resetButton != null)
		{
			resetButton.onClick.RemoveAllListeners();
		}
		if (startButton != null)
		{
			startButton.onClick.RemoveAllListeners();
		}
		if (difficultyDropdown != null)
		{
			difficultyDropdown.onValueChanged.RemoveAllListeners();
		}
		if (firstPlayerDropdown != null)
		{
			firstPlayerDropdown.onValueChanged.RemoveAllListeners();
		}
		if (backToMenuButton != null)
		{
			backToMenuButton.onClick.RemoveAllListeners();
		}
	}

	private void Start()
	{
		// 初始化面板显示：默认显示开始界面
		if (startPanel != null) startPanel.SetActive(true);
		if (gamePanel != null) gamePanel.SetActive(false);
		SetupDifficultyDropdown();
		SetupFirstPlayerDropdown();
		UpdateMarkLabels();
		// 预先重置棋盘状态，等开始时再显示
		ResetGame();
	}

	private void ResetGame()
	{
		core.Reset(SelectedFirstPlayer());

		for (int i = 0; i < cells.Length; i++)
		{
			if (cells[i] == null) continue;
			SetButtonText(cells[i], "");
			cells[i].interactable = true;
			SetButtonColor(cells[i], normalColor);
		}

		UpdateStatus("你的回合");
		UpdateMarkLabels();
	}

	private void StartGame()
	{
		ResetGame();
		if (startPanel != null) startPanel.SetActive(false);
		if (gamePanel != null) gamePanel.SetActive(true);
		// 若AI先手，则立即AI落子
		if (core.CurrentTurn == TicTacToeCore.Player.AI)
		{
			DoAIMove();
		}
	}
	
	public void ExitGame()
	{
		Application.Quit();
		#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
		#endif
	}

	private void OnCellClicked(int index)
	{
		if (!core.CanPlace(index) || core.CurrentTurn != TicTacToeCore.Player.Human) return;

		core.PlaceMove(index, TicTacToeCore.Player.Human);
		RenderCell(index);
		ResolveGameStateAndUI();

		if (!core.IsGameOver)
		{
			DoAIMove();
		}
	}

	private void RenderCell(int index)
	{
		if (cells[index] == null) return;
		var cellState = core.Board[index];
		string label = cellState == TicTacToeCore.Player.Human ? humanMark : cellState == TicTacToeCore.Player.AI ? aiMark : "";
		SetButtonText(cells[index], label);
		if (cellState != TicTacToeCore.Player.None)
		{
			cells[index].interactable = false;
		}
	}

	private void DoAIMove()
	{
		UpdateStatus("AI 思考中...");
		int best = core.AIMove(difficulty);
		if (best >= 0)
		{
			RenderCell(best);
			ResolveGameStateAndUI();
		}
	}

	private void SetupDifficultyDropdown()
	{
		if (difficultyDropdown == null) return;
		difficultyDropdown.ClearOptions();
		var options = new System.Collections.Generic.List<string>() { "简单", "普通", "困难" };
		difficultyDropdown.AddOptions(options);
		difficultyDropdown.value = DifficultyToIndex(difficulty);
		difficultyDropdown.RefreshShownValue();
	}

	private void SetupFirstPlayerDropdown()
	{
		if (firstPlayerDropdown == null) return;
		firstPlayerDropdown.ClearOptions();
		var options = new System.Collections.Generic.List<string>() { "玩家先手", "AI先手" };
		firstPlayerDropdown.AddOptions(options);
		firstPlayerDropdown.value = core.CurrentTurn == TicTacToeCore.Player.AI ? 1 : 0;
		firstPlayerDropdown.RefreshShownValue();
	}

	private void OnDifficultyChanged(int index)
	{
		difficulty = IndexToDifficulty(index);
	}

	private void OnFirstPlayerChanged(int index)
	{
		// 仅更新下次开始时的先手，不立即重置
	}

	private int DifficultyToIndex(TicTacToeCore.Difficulty diff)
	{
		switch (diff)
		{
			case TicTacToeCore.Difficulty.Easy: return 0;
			case TicTacToeCore.Difficulty.Medium: return 1;
			default: return 2;
		}
	}

	private TicTacToeCore.Difficulty IndexToDifficulty(int index)
	{
		switch (index)
		{
			case 0: return TicTacToeCore.Difficulty.Easy;
			case 1: return TicTacToeCore.Difficulty.Medium;
			default: return TicTacToeCore.Difficulty.Hard;
		}
	}

	private void UpdateMarkLabels()
	{
		if (humanMarkLabel != null) humanMarkLabel.text = "玩家: " + humanMark;
		if (aiMarkLabel != null) aiMarkLabel.text = "AI: " + aiMark;
	}

	private TicTacToeCore.Player SelectedFirstPlayer()
	{
		if (firstPlayerDropdown == null) return TicTacToeCore.Player.Human;
		return firstPlayerDropdown.value == 1 ? TicTacToeCore.Player.AI : TicTacToeCore.Player.Human;
	}

	private void BackToMenu()
	{
		if (gamePanel != null) gamePanel.SetActive(false);
		if (startPanel != null) startPanel.SetActive(true);
	}

	private void ResolveGameStateAndUI()
	{
		if (core.IsGameOver)
		{
			LockAllCells();
			UpdateStatus(core.Winner == TicTacToeCore.Player.None ? "平局" : (core.Winner == TicTacToeCore.Player.Human ? "玩家胜利！" : "AI 胜利"));
			HighlightWinningLine();
			return;
		}
		UpdateStatus(core.CurrentTurn == TicTacToeCore.Player.Human ? "你的回合" : "AI 思考中...");
	}

	private void LockAllCells()
	{
		for (int i = 0; i < cells.Length; i++)
		{
			if (cells[i] == null) continue;
			cells[i].interactable = false;
		}
	}

	private void UpdateStatus(string text)
	{
		if (statusText != null)
		{
			statusText.text = text;
		}
	}

	private void SetButtonText(Button btn, string text)
	{
		if (btn == null) return;
		Text label = btn.GetComponentInChildren<Text>();
		if (label != null)
		{
			label.text = text;
		}
	}

	private void SetButtonColor(Button btn, Color color)
	{
		if (btn == null) return;
		Image img = btn.GetComponent<Image>();
		if (img != null)
		{
			img.color = color;
		}
	}

	private void HighlightWinningLine()
	{
		if (core.Winner == TicTacToeCore.Player.None) return;
		if (core.WinningLine == null) return;
		for (int i = 0; i < core.WinningLine.Length; i++)
		{
			int idx = core.WinningLine[i];
			if (idx < 0 || idx >= cells.Length) continue;
			SetButtonColor(cells[idx], winHighlightColor);
		}
	}


}

