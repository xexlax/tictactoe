using UnityEngine;

public class TicTacToeCore
{
	public enum Difficulty
	{
		Easy,
		Medium,
		Hard
	}

	public enum Player
	{
		None = 0,
		Human = 1,
		AI = 2
	}

	private readonly int[,] winningLines = new int[,]
	{
		{ 0, 1, 2 },
		{ 3, 4, 5 },
		{ 6, 7, 8 },
		{ 0, 3, 6 },
		{ 1, 4, 7 },
		{ 2, 5, 8 },
		{ 0, 4, 8 },
		{ 2, 4, 6 }
	};

	public Player[] Board { get; private set; } = new Player[9];
	public Player CurrentTurn { get; private set; } = Player.Human;
	public Player Winner { get; private set; } = Player.None;
	public bool IsGameOver { get; private set; } = false;
	public int[] WinningLine { get; private set; } = new int[3] { -1, -1, -1 };

	public void Reset()
	{
		Reset(Player.Human);
	}

	public void Reset(Player startingPlayer)
	{
		for (int i = 0; i < Board.Length; i++)
		{
			Board[i] = Player.None;
		}
		CurrentTurn = startingPlayer;
		Winner = Player.None;
		IsGameOver = false;
		WinningLine[0] = WinningLine[1] = WinningLine[2] = -1;
	}

	public bool CanPlace(int index)
	{
		return !IsGameOver && index >= 0 && index < Board.Length && Board[index] == Player.None;
	}

	public bool PlaceMove(int index, Player player)
	{
		if (!CanPlace(index))
		{
			return false;
		}

		Board[index] = player;
		ResolveGameStateAfterMove();
		if (!IsGameOver)
		{
			CurrentTurn = (player == Player.Human) ? Player.AI : Player.Human;
		}
		return true;
	}

	public int AIMove()
	{
		if (IsGameOver || CurrentTurn != Player.AI)
		{
			return -1;
		}
		int best = FindBestMove(Board);
		if (best >= 0)
		{
			PlaceMove(best, Player.AI);
		}
		return best;
	}

	public int AIMove(Difficulty difficulty)
	{
		if (IsGameOver || CurrentTurn != Player.AI)
		{
			return -1;
		}

		float optimalChance = 1f;
		switch (difficulty)
		{
			case Difficulty.Easy:
				optimalChance = 0.3f; // 容易失误
				break;
			case Difficulty.Medium:
				optimalChance = 0.75f; // 偶有失误
				break;
			case Difficulty.Hard:
				optimalChance = 1f; // 始终最佳
				break;
		}

		int moveIndex;
		if (UnityEngine.Random.value <= optimalChance)
		{
			moveIndex = FindBestMove(Board);
		}
		else
		{
			moveIndex = FindRandomMove(Board);
		}

		if (moveIndex >= 0)
		{
			PlaceMove(moveIndex, Player.AI);
		}
		return moveIndex;
	}

	private void ResolveGameStateAfterMove()
	{
		Player detected = DetectWinner(Board);
		if (detected != Player.None)
		{
			Winner = detected;
			IsGameOver = true;
			// 记录胜利连线
			for (int i = 0; i < winningLines.GetLength(0); i++)
			{
				int a = winningLines[i, 0];
				int b = winningLines[i, 1];
				int c = winningLines[i, 2];
				if (Board[a] != Player.None && Board[a] == Board[b] && Board[b] == Board[c])
				{
					WinningLine[0] = a; WinningLine[1] = b; WinningLine[2] = c;
					break;
				}
			}
			return;
		}
		if (!HasMovesLeft(Board))
		{
			Winner = Player.None;
			IsGameOver = true;
		}
	}

	public Player DetectWinner(Player[] state)
	{
		for (int i = 0; i < winningLines.GetLength(0); i++)
		{
			int a = winningLines[i, 0];
			int b = winningLines[i, 1];
			int c = winningLines[i, 2];
			if (state[a] != Player.None && state[a] == state[b] && state[b] == state[c])
			{
				return state[a];
			}
		}
		return Player.None;
	}

	public bool HasMovesLeft(Player[] state)
	{
		for (int i = 0; i < state.Length; i++)
		{
			if (state[i] == Player.None) return true;
		}
		return false;
	}

	private int Evaluate(Player[] state, int depth)
	{
		Player w = DetectWinner(state);
		if (w == Player.AI) return 10 - depth;
		if (w == Player.Human) return depth - 10;
		return 0;
	}

	public int FindBestMove(Player[] state)
	{
		int bestValue = int.MinValue;
		int bestIndex = -1;
		for (int i = 0; i < state.Length; i++)
		{
			if (state[i] != Player.None) continue;
			state[i] = Player.AI;
			int moveValue = Minimax(state, 0, false, int.MinValue + 1, int.MaxValue - 1);
			state[i] = Player.None;
			if (moveValue > bestValue)
			{
				bestValue = moveValue;
				bestIndex = i;
			}
		}
		return bestIndex;
	}

	private int FindRandomMove(Player[] state)
	{
		int availableCount = 0;
		for (int i = 0; i < state.Length; i++)
		{
			if (state[i] == Player.None) availableCount++;
		}
		if (availableCount == 0) return -1;
		int pick = UnityEngine.Random.Range(0, availableCount);
		for (int i = 0; i < state.Length; i++)
		{
			if (state[i] != Player.None) continue;
			if (pick == 0) return i;
			pick--;
		}
		return -1;
	}

	private int Minimax(Player[] state, int depth, bool isMaximizing, int alpha, int beta)
	{
		int score = Evaluate(state, depth);
		if (score != 0) return score;
		if (!HasMovesLeft(state)) return 0;

		if (isMaximizing)
		{
			int best = int.MinValue;
			for (int i = 0; i < state.Length; i++)
			{
				if (state[i] != Player.None) continue;
				state[i] = Player.AI;
				int value = Minimax(state, depth + 1, false, alpha, beta);
				state[i] = Player.None;
				best = Mathf.Max(best, value);
				alpha = Mathf.Max(alpha, best);
				if (beta <= alpha) break;
			}
			return best;
		}
		else
		{
			int best = int.MaxValue;
			for (int i = 0; i < state.Length; i++)
			{
				if (state[i] != Player.None) continue;
				state[i] = Player.Human;
				int value = Minimax(state, depth + 1, true, alpha, beta);
				state[i] = Player.None;
				best = Mathf.Min(best, value);
				beta = Mathf.Min(beta, best);
				if (beta <= alpha) break;
			}
			return best;
		}
	}
}

