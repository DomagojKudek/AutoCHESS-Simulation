using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FigureType
{
    Rook,
    Knight,
    Bishop,
    Queen,
    King,
    Pawn,
    None
}

public class Figure
{
    public string name;
    public FigureType figureType;
    public string team;
    public string chessColumn;
    public int chessRow;
    public bool doubleMove;
    public bool isAlive;
    public bool hasMoved;
    public Collider collider;
    public GameObject figureObject;

    public Figure(string name, FigureType figureType, string team, int chessRow, int chessColumn, bool doubleMove, bool hasMoved, Collider collider, GameObject figureObject)
    {
        this.name = name;
        this.figureType = figureType;
        this.team = team;
        this.chessRow = chessRow;
        this.chessColumn = TransformColumn(chessColumn);
        this.doubleMove = doubleMove;
        this.hasMoved = hasMoved;
        this.collider = collider;
        this.figureObject = figureObject;
    }

    public Figure(string name, FigureType figureType, string team, int chessRow, int chessColumn, bool doubleMove, bool hasMoved)
    {
        this.name = name;
        this.figureType = figureType;
        this.team = team;
        this.chessRow = chessRow;
        this.chessColumn = TransformColumn(chessColumn);
        this.doubleMove = doubleMove;
        this.hasMoved = hasMoved;
    }

    public static string TransformColumn(int column)
    {
        switch (column)
        {
            case 0:
                return "A";
            case 1:
                return "B";
            case 2:
                return "C";
            case 3:
                return "D";
            case 4:
                return "E";
            case 5:
                return "F";
            case 6:
                return "G";
            case 7:
                return "H";
        }
        return "Wrong Position";
    }

    public static int TransformChessColumn(string chessColumn)
    {
        switch (chessColumn)
        {
            case "A":
                return 0;
            case "B":
                return 1;
            case "C":
                return 2;
            case "D":
                return 3;
            case "E":
                return 4;
            case "F":
                return 5;
            case "G":
                return 6;
            case "H":
                return 7;
        }
        return 999;
    }
}