using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private string path = Directory.GetCurrentDirectory() + "\\stockfish.exe";
    public enum State
    {
        Initialize,
        Turn,
        GameEnd
    }
    public enum SubState
    {
        WhiteTurnBeforeStart,
        WhiteTurnStart,
        WhiteTurnTransition,
        WhiteTurnMoveStart,
        WhiteTurnMoveMove,
        WhiteTurnMoveTransition,
        WhiteTurnMoveBeforeEnd,
        WhiteTurnMoveEnd
    }

    private static GameController instance;
    public GameObject whiteQueen;
    public GameObject blackQueen;
    public float speed;
    public Transform[] startPosition;
    public GameObject[] hookPivot;
    public GameObject[] robots;
    public System.Diagnostics.Process process;
    public ChessController chessController;
    public string FEN = "";
    public int currentPlayer;
    public int halfMoveClock;
    public int fullMoveClock;
    public bool gameEnd;
    public string move = "";
    List<Vector3> hookLocations = new List<Vector3>();
    Vector3[] robotLocations;
    public bool initDoneFlag = false;
    public State currentState = State.Initialize;
    public SubState currentSubState = SubState.WhiteTurnStart;
    void Start()
    {
        Cursor.visible = false;
        instance = this;
        speed = Time.deltaTime * 1.5f;
        startPosition = new Transform[2];
        hookPivot = new GameObject[2];
        robotLocations = new Vector3[2];
        robots = new GameObject[2];
        robots[0] = GameObject.FindGameObjectWithTag("RobotWhite");
        robots[1] = GameObject.FindGameObjectWithTag("RobotBlack");

        startPosition[0] = GameObject.FindGameObjectWithTag("StartPositionWhite").transform;
        startPosition[1] = GameObject.FindGameObjectWithTag("StartPositionBlack").transform;

        hookPivot[0] = GameObject.FindGameObjectWithTag("HookPivotWhite");
        hookPivot[1] = GameObject.FindGameObjectWithTag("HookPivotBlack");

        chessController = ChessController.GetSingleton();
        Stockfish();
    }

    void Update()
    {
        if (currentState == State.Initialize)
        {
            halfMoveClock = 0;
            currentPlayer = 0;
            fullMoveClock = 1;
            currentState = State.Turn;
            for (int i = 0; i < 2; i++)
            {
                hookPivot[i].transform.position = startPosition[i].position;
            }
        }
        if (currentState == State.Turn)
        {
            if (initDoneFlag == false)
            {
                ExportFEN(chessController);
                move = GetBestMove(FEN);
                CheckEndRound();
                if (currentState != State.GameEnd)
                {
                    currentSubState = SubState.WhiteTurnBeforeStart;
                    robotLocations = SetRobotPosition(currentPlayer);
                    initDoneFlag = true;
                }
            }
            if (currentSubState == SubState.WhiteTurnBeforeStart)
            {
                if (Vector3.Distance(robots[currentPlayer].transform.position, robotLocations[0]) > 0.0001f)
                {
                    robots[currentPlayer].transform.position = Vector3.MoveTowards(robots[currentPlayer].transform.position, robotLocations[0], speed);
                }
                else
                {
                    hookLocations = CalculateMovePositions(move, currentPlayer);
                    currentSubState = SubState.WhiteTurnStart;
                }
            }

            if (currentSubState == SubState.WhiteTurnStart)
            {
                if (Vector3.Distance(hookPivot[currentPlayer].transform.position, hookLocations[1]) > 0.0001f)
                {
                    hookPivot[currentPlayer].transform.position = Vector3.MoveTowards(hookPivot[currentPlayer].transform.position, hookLocations[1], speed);
                }
                else
                {
                    currentSubState = SubState.WhiteTurnTransition;
                }
            }
            if (currentSubState == SubState.WhiteTurnTransition)
            {
                if (Vector3.Distance(hookPivot[currentPlayer].transform.position, hookLocations[2]) > 0.0001f)
                {
                    hookPivot[currentPlayer].transform.position = Vector3.MoveTowards(hookPivot[currentPlayer].transform.position, hookLocations[2], speed);
                }
                else
                {
                    currentSubState = SubState.WhiteTurnMoveStart;
                    AttachFigureTransform();
                }
            }
            if (currentSubState == SubState.WhiteTurnMoveStart)
            {
                if (Vector3.Distance(hookPivot[currentPlayer].transform.position, hookLocations[1]) > 0.0001f)
                {
                    hookPivot[currentPlayer].transform.position = Vector3.MoveTowards(hookPivot[currentPlayer].transform.position, hookLocations[1], speed);
                }
                else
                {
                    currentSubState = SubState.WhiteTurnMoveMove;
                }
            }
            if (currentSubState == SubState.WhiteTurnMoveMove)
            {
                if (Vector3.Distance(robots[currentPlayer].transform.position, robotLocations[1]) > 0.0001f)
                {
                    robots[currentPlayer].transform.position = Vector3.MoveTowards(robots[currentPlayer].transform.position, robotLocations[1], speed);
                }
                else
                {
                    CalculateMoveAdjusment();
                    currentSubState = SubState.WhiteTurnMoveTransition;
                }
            }
            if (currentSubState == SubState.WhiteTurnMoveTransition)
            {
                if (Vector3.Distance(hookPivot[currentPlayer].transform.position, hookLocations[3]) > 0.0001f)
                {
                    hookPivot[currentPlayer].transform.position = Vector3.MoveTowards(hookPivot[currentPlayer].transform.position, hookLocations[3], speed);
                }
                else
                {
                    currentSubState = SubState.WhiteTurnMoveBeforeEnd;
                    DestroyFigure();
                    DetachFigureTransform();
                }
            }
            if (currentSubState == SubState.WhiteTurnMoveBeforeEnd)
            {
                if (Vector3.Distance(hookPivot[currentPlayer].transform.position, hookLocations[1]) > 0.0001f)
                {
                    hookPivot[currentPlayer].transform.position = Vector3.MoveTowards(hookPivot[currentPlayer].transform.position, hookLocations[1], speed);
                }
                else
                {
                    currentSubState = SubState.WhiteTurnMoveEnd;
                }
            }
            if (currentSubState == SubState.WhiteTurnMoveEnd)
            {
                if (Vector3.Distance(hookPivot[currentPlayer].transform.position, hookLocations[0]) > 0.0001f)
                {
                    hookPivot[currentPlayer].transform.position = Vector3.MoveTowards(hookPivot[currentPlayer].transform.position, hookLocations[0], speed);
                }
                else
                {
                    halfMoveClock = CalculateHalfMoveClock(halfMoveClock);
                    if (currentPlayer == 1)
                    {
                        fullMoveClock++;
                    }
                    MoveChessBoard(chessController);
                    CheckQueenRespawn();
                    currentPlayer = currentPlayer == 0 ? 1 : 0;
                    initDoneFlag = false;
                    ChessController.GetSingleton().PrintBoard();
                }
            }
        }
    }

    public void ExportFEN(ChessController chessController)
    {
        int freeCellCount = 0;
        FEN = "";
        for (int i = 7; i >= 0; i--)
        {
            for (int j = 0; j < 8; j++)
            {
                Figure figure = null;
                if (chessController.chessBoard[i][j].name != null)
                {
                    figure = chessController.chessBoard[i][j];
                }
                if (figure.name == "None")
                {
                    freeCellCount += 1;
                }
                else
                {
                    if (freeCellCount != 0)
                    {
                        FEN += freeCellCount.ToString();
                        freeCellCount = 0;
                    }
                    if (figure.figureType == FigureType.King)
                    {
                        if (figure.team == "White")
                            FEN += "K";
                        else
                            FEN += "k";
                    }
                    else if (figure.figureType == FigureType.Queen)
                    {
                        if (figure.team == "White")
                            FEN += "Q";
                        else
                            FEN += "q";
                    }
                    else if (figure.figureType == FigureType.Rook)
                    {
                        if (figure.team == "White")
                            FEN += "R";
                        else
                            FEN += "r";
                    }
                    else if (figure.figureType == FigureType.Bishop)
                    {
                        if (figure.team == "White")
                            FEN += "B";
                        else
                            FEN += "b";
                    }
                    else if (figure.figureType == FigureType.Knight)
                    {
                        if (figure.team == "White")
                            FEN += "N";
                        else
                            FEN += "n";
                    }
                    else if (figure.figureType == FigureType.Pawn)
                    {
                        if (figure.team == "White")
                            FEN += "P";
                        else
                            FEN += "p";
                    }
                }
            }
            if (freeCellCount != 0)
            {
                FEN += freeCellCount.ToString();
            }
            freeCellCount = 0;
            if (i != 0)
                FEN += '/';
        }
        FEN += " ";
        string turnIndicator = currentPlayer == 0 ? "w" : "b";
        FEN += turnIndicator + " ";

        bool[] castlingWhite = chessController.CheckCastling("White");
        bool[] castlingBlack = chessController.CheckCastling("Black");
        if (!castlingWhite[0] && !castlingWhite[1] && !castlingBlack[0] && !castlingBlack[1])
        {
            FEN += "-";
        }
        else
        {
            if (castlingWhite[1] == true)
            {
                FEN += "K";
            }
            if (castlingWhite[0] == true)
            {
                FEN += "Q";
            }
            if (castlingBlack[1] == true)
            {
                FEN += "k";
            }
            if (castlingBlack[0] == true)
            {
                FEN += "q";
            }
        }
        FEN += " ";

        FEN += chessController.CheckEnPassantSquare() + " ";

        FEN += halfMoveClock.ToString() + " " + fullMoveClock.ToString();
        Debug.Log(FEN);
    }

    public void Stockfish()
    {
        process = new System.Diagnostics.Process();
        process.StartInfo.FileName = path;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
    }
    public string GetBestMove(string FEN)
    {
        string bestMove = "";

        string setupString = "position fen " + FEN;
        process.StandardInput.WriteLine(setupString);

        float random = UnityEngine.Random.Range(100, 200);
        string processString = "go movetime " + random.ToString();
        process.StandardInput.WriteLine(processString);

        do
        {
            bestMove = process.StandardOutput.ReadLine();
        } while (CheckEndLine(bestMove));
        Debug.Log("REAL  " + bestMove);
        bestMove = bestMove.Substring(bestMove.IndexOf("bestmove") + 9, 4);
        return bestMove;
    }

    private bool CheckEndLine(string bestMove)
    {
        if (bestMove.IndexOf("bestmove") != -1)
        {
            return false;
        }
        return true;
    }

    private int CalculateHalfMoveClock(int halfMoveClock)
    {
        for (int i = 0; i < chessController.chessBoard.Count; i++)
        {
            Figure figure1 = chessController.chessBoard[i].Find(x => x.chessColumn == move.Substring(0, 1).ToUpper() && x.chessRow == int.Parse(move.Substring(1, 1)));
            if (figure1 != null && figure1.figureType == FigureType.Pawn)
            {
                return 0;
            }
            Figure figure2 = chessController.chessBoard[i].Find(x => x.chessColumn == move.Substring(2, 1).ToUpper() && x.chessRow == int.Parse(move.Substring(3, 1)));
            string team = currentPlayer == 0 ? "Black" : "White";
            if (figure2 != null && figure2.team == team)
            {
                return 0;
            }

        }
        halfMoveClock++;
        return halfMoveClock;
    }

    private void CheckQueenRespawn()
    {
        for (int i = 0; i < chessController.chessBoard.Count; i++)
        {
            Figure figure1 = chessController.chessBoard[i].Find(x => x.chessColumn == move.Substring(2, 1).ToUpper() && x.chessRow == int.Parse(move.Substring(3, 1)));
            if (figure1 != null && figure1.figureType == FigureType.Pawn && (figure1.chessRow == 8 || figure1.chessRow == 1))
            {
                GameObject newQueen = Instantiate(currentPlayer == 0 ? whiteQueen : blackQueen);
                newQueen.transform.parent = GameObject.FindGameObjectWithTag(currentPlayer == 0 ? "ChessFiguresWhite" : "ChessFiguresBlack").transform;
                newQueen.transform.position = figure1.figureObject.transform.position;
                newQueen.transform.position += new Vector3(0, 0.219054f, 0);
                newQueen.transform.localScale = new Vector3(8.65246f, 8.65246f, 8.65246f);
                int column = Figure.TransformChessColumn(figure1.chessColumn);
                chessController.chessBoard[i][column] = new Figure("Queen-respawn", FigureType.Queen, figure1.team, figure1.chessRow, column, false, false, newQueen.GetComponent<Collider>(), newQueen);
                Destroy(figure1.figureObject);
            }
        }
    }

    private void CheckEndRound()
    {
        if (GameObject.FindGameObjectWithTag("ChessFiguresWhite").transform.childCount == 1 && GameObject.FindGameObjectWithTag("ChessFiguresBlack").transform.childCount == 1)
        {
            currentState = State.GameEnd;
            Time.timeScale = 0;

        }
        if (move == "(non")
        {
            currentState = State.GameEnd;
            Time.timeScale = 0;

        }
        if (halfMoveClock > 99)
        {
            currentState = State.GameEnd;
            Time.timeScale = 0;
        }
    }

    private void AttachFigureTransform()
    {
        for (int i = 0; i < chessController.chessBoard.Count; i++)
        {
            Figure figure1 = chessController.chessBoard[i].Find(x => x.chessColumn == move.Substring(0, 1).ToUpper() && x.chessRow == int.Parse(move.Substring(1, 1)));
            if (figure1 != null)
            {
                figure1.figureObject.transform.parent = hookPivot[currentPlayer].transform;
            }
        }
    }
    private void DetachFigureTransform()
    {
        for (int i = 0; i < chessController.chessBoard.Count; i++)
        {
            Figure figure1 = chessController.chessBoard[i].Find(x => x.chessColumn == move.Substring(0, 1).ToUpper() && x.chessRow == int.Parse(move.Substring(1, 1)));
            if (figure1 != null)
            {
                figure1.figureObject.transform.parent = GameObject.FindGameObjectWithTag(currentPlayer == 0 ? "ChessFiguresWhite" : "ChessFiguresBlack").transform;
            }
        }
    }

    private void DestroyFigure()
    {
        for (int i = 0; i < chessController.chessBoard.Count; i++)
        {
            Figure figure1 = chessController.chessBoard[i].Find(x => x.chessColumn == move.Substring(2, 1).ToUpper() && x.chessRow == int.Parse(move.Substring(3, 1)));

            if (figure1 != null)
            {
                if (figure1.team != "None")
                {
                    int row = figure1.chessRow + 1;
                    int column = Figure.TransformChessColumn(figure1.chessColumn);
                    Destroy(figure1.figureObject);
                    figure1 = new Figure("None", FigureType.None, "None", row, column, false, false);
                }
            }
        }
    }
    private List<Vector3> CalculateMovePositions(string move, int currentPlayer)
    {
        List<Vector3> list = new List<Vector3>();
        string startString = move.Substring(0, 2);
        string endString = move.Substring(2, 2);

        Transform startPosition = hookPivot[currentPlayer].transform;
        Transform moveStartPosition = Array.Find<Transform>(ChessController.GetSingleton().figureLocations, x => x.name.ToLower() == startString);
        Transform moveEndPosition = Array.Find<Transform>(ChessController.GetSingleton().figureLocations, x => x.name.ToLower() == endString);
        Vector3 transitionPosition = CalculateMidpoint(moveStartPosition, moveEndPosition);
        transitionPosition.z = startPosition.position.z;

        list.Add(startPosition.position);
        list.Add(transitionPosition);
        list.Add(moveStartPosition.position);
        list.Add(moveEndPosition.position);
        /*foreach (Vector3 vector in list)
        {
            Debug.Log("  " + vector);
        }*/
        return list;
    }

    private void CalculateMoveAdjusment()
    {
        hookLocations[0] = new Vector3(hookLocations[0].x, hookLocations[0].y, hookPivot[currentPlayer].transform.position.z);
        hookLocations[3] = new Vector3(hookLocations[3].x, hookLocations[3].y, hookPivot[currentPlayer].transform.position.z);
        hookLocations[1] = new Vector3(hookLocations[1].x, hookLocations[1].y, hookPivot[currentPlayer].transform.position.z);
    }

    private Vector3 CalculateMidpoint(Transform pos1, Transform pos2)
    {
        Vector3 midpoint = (pos1.position + pos2.position) / 2f + new Vector3(0, 1f, 0);
        return midpoint;
    }

    private Vector3[] SetRobotPosition(int currentPlayer)
    {
        Transform robotPosition = robots[currentPlayer].transform;

        Transform[] locations = new Transform[8];
        Vector3[] robotLocations = new Vector3[2];
        int numberOfChildren = GameObject.FindGameObjectWithTag("RobotLocations").transform.GetChild(currentPlayer).childCount;
        for (int i = 0; i < numberOfChildren; i++)
        {
            locations[i] = GameObject.FindGameObjectWithTag("RobotLocations").transform.GetChild(currentPlayer).GetChild(i);
        }
        robotLocations[0] = Array.Find(locations, x => x.name.Substring(0, 1).ToLower() == move.Substring(0, 1).ToLower()).position;
        robotLocations[1] = Array.Find(locations, x => x.name.Substring(0, 1).ToLower() == move.Substring(2, 1).ToLower()).position;
        //Debug.Log(robotLocations[0] + "  " + robotLocations[1]);
        return robotLocations;
    }

    private void MoveChessBoard(ChessController chessController)
    {
        int startRow = 0;
        int startColumn = 0;
        int endRow = 0;
        int endColumn = 0;

        for (int i = 0; i < chessController.chessBoard.Count; i++)
        {
            int temp = chessController.chessBoard[i].FindIndex(x => x.chessColumn == move.Substring(0, 1).ToUpper() && x.chessRow == int.Parse(move.Substring(1, 1)));
            int temp2 = chessController.chessBoard[i].FindIndex(x => x.chessColumn == move.Substring(2, 1).ToUpper() && x.chessRow == int.Parse(move.Substring(3, 1)));
            if (temp != -1)
            {
                startRow = i;
                startColumn = temp;
            }
            if (temp2 != -1)
            {
                endRow = i;
                endColumn = temp2;
            }
        }
        CheckSetFigureMove(startRow, startColumn, endRow, endColumn);
        /*Debug.Log("----------------------");
        Debug.Log(chessController.chessBoard[startRow][startColumn].name);
        Debug.Log(chessController.chessBoard[endRow][endColumn].name);*/

        chessController.chessBoard[endRow][endColumn] = chessController.chessBoard[startRow][startColumn];
        chessController.chessBoard[startRow][startColumn].chessRow = endRow + 1;
        chessController.chessBoard[startRow][startColumn].chessColumn = Figure.TransformColumn(endColumn);
        chessController.chessBoard[startRow][startColumn] = new Figure("None", FigureType.None, "None", startRow + 1, startColumn, false, false);

        /*Debug.Log("----------------------");
        Debug.Log(chessController.chessBoard[startRow][startColumn].name);
        Debug.Log(chessController.chessBoard[endRow][endColumn].name);*/
    }

    private void CheckSetFigureMove(int startRow, int startColumn, int endRow, int endColumn)
    {
        chessController.chessBoard[startRow][startColumn].hasMoved = true;
        if (chessController.chessBoard[startRow][startColumn].figureType == FigureType.Pawn)
        {
            if (Math.Abs(endRow - startRow) == 2)
            {
                chessController.chessBoard[startRow][startColumn].doubleMove = true;
            }
        }
    }

    public static GameController GetSingleton() => instance;
}