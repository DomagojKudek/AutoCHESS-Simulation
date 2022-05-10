using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessController : MonoBehaviour
{
    private static ChessController instance;
    public List<List<Figure>> chessBoard = new List<List<Figure>>();
    public Transform[] figureLocations = new Transform[64];

    void Start()
    {
        instance = this;
        InitializeChessboard();
        Debug.Log("init done");
    }

    private void InitializeChessboard()
    {
        GameObject locations = GameObject.FindGameObjectWithTag("FigureLocations");
        for (int i = 0; i < locations.transform.childCount; i++)
        {
            figureLocations[i] = locations.transform.GetChild(i);
        }
        GameObject whiteFigures = GameObject.FindGameObjectWithTag("ChessFiguresWhite");
        GameObject blackFigures = GameObject.FindGameObjectWithTag("ChessFiguresBlack");
        Transform[] figures = new Transform[32];

        for (int i = 0; i < whiteFigures.transform.childCount; i++)
        {
            figures[i] = whiteFigures.transform.GetChild(i);
            figures[i + 16] = blackFigures.transform.GetChild(i);
        }

        int k = 0;
        for (int i = 1; i < 9; i++)
        {
            List<Figure> temp = new List<Figure>();
            for (int j = 0; j < 8; j++)
            {
                if (i < 3 || i > 6)
                {
                    string team = "White";
                    if (i > 6)
                    {
                        team = "Black";
                    }
                    string name = figures[k].name;
                    FigureType type = (FigureType)Enum.Parse(typeof(FigureType), name.Split(' ')[0]);
                    temp.Add(new Figure(name, type, team, i, j, false, false, figures[k].gameObject.GetComponent<Collider>(), figures[k].gameObject));
                    k++;
                }
                else
                {
                    string name = "None";
                    string team = "None";
                    FigureType type = FigureType.None;
                    temp.Add(new Figure(name, type, team, i, j, false, false));
                }
            }
            chessBoard.Add(temp);
        }
    }

    public Figure GetKing(string team)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (chessBoard[i][j] != null)
                {
                    if (chessBoard[i][j].figureType == FigureType.King && chessBoard[i][j].team == team)
                    {
                        return chessBoard[i][j];
                    }
                }
            }
        }
        return null;
    }

    public bool[] CheckCastling(string team)
    {
        bool[] result = { false, false };
        int row = 7;
        if (team == "White")
        {
            row = 0;
        }
        //left castling chek for bool[0]
        if (GetKing(team).hasMoved == false && chessBoard[row][Figure.TransformChessColumn("A")].hasMoved == false)
        {
            //if (chessBoard[Figure.TransformChessRow(row), Figure.TransformChessColumn("B")] == null && chessBoard[Figure.TransformChessRow(row), Figure.TransformChessColumn("C")] == null)
            //{
            //    if (chessBoard[Figure.TransformChessRow(row), Figure.TransformChessColumn("D")] == null)
            //    {
            result[0] = true;
            //    }
            //}
        }
        //right castling chek for bool[1]
        if (GetKing(team).hasMoved == false && chessBoard[row][Figure.TransformChessColumn("H")].hasMoved == false)
        {
            //if (chessBoard[Figure.TransformChessRow(row), Figure.TransformChessColumn("F")] == null && chessBoard[Figure.TransformChessRow(row), Figure.TransformChessColumn("G")] == null)
            //{
            result[1] = true;
            //}
        }
        return result;
    }

    public string CheckEnPassantSquare()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Figure figure = chessBoard[i][j];
                if (chessBoard[i][j] != null)
                {
                    if (figure.figureType == FigureType.Pawn && figure.doubleMove == true)
                    {
                        if (figure.team == "Black")
                        {
                            return (figure.chessColumn.ToLower() + "5");
                        }
                        else
                        {
                            return (figure.chessColumn.ToLower() + "4");
                        }
                    }
                }
            }
        }
        return "-";
    }

    public void PrintBoard()
    {
        string export = "";
        for (int i = 7; i >= 0; i--)
        {
            for (int j = 0; j < chessBoard[i].Count; j++)
            {
                FigureType part = chessBoard[i][j].figureType;
                if (part != FigureType.None)
                {
                    export += part.ToString().ToUpper();
                }
                else
                {
                    export += part.ToString();
                }
                export += "  ";
            }
            export += "\n";
        }
        Debug.Log(export);
    }

    public static ChessController GetSingleton() => instance;
}
