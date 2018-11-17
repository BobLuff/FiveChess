using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Turn                //先手顺序
{
    black=0,
    white=1,
}


public enum ChessState         //是否已经落子 
{
    None=0,
    BlackChess,
    WhiteChess,
}

public class GameManager : MonoBehaviour {


    [SerializeField]
    private Camera _mainCamera;

    private GameObject _blackPrefab;
    private GameObject _whiteParefab;

    private AssetBundle _uiAbs;
    private Vector3 _leftTopPos;
    private Vector3 _rightTopPos;
    private Vector3 _leftBottomPos;
    private Vector3 _rightBottomPos;
    private Vector3 _pointerPos;

    private Vector3[,] _chessPos;                                                                     //棋盘落子的坐标
    private ChessState[,] _chessState;

    private Turn _chessTurn = Turn.black; 
    private float _gridWidth;                                                                         //棋盘格宽度
    private float _gridHeight;                                                                        //棋盘格高度
    private float _minGridDis;

    private const string UI_AssetBundle_Path = "Assets/StreamingAssets/ui.unity3d";
    private const string ChessBoard_Path = "ChessBoard";
    private const string Black_Path = "Black";
    private const string White_Path = "White";
    private const int ChessBoard_Grid_Num = 14;                                                       //棋盘网格总数





    private void Awake()
    {
        _chessState = new ChessState[ChessBoard_Grid_Num+1, ChessBoard_Grid_Num+1];

        _uiAbs = AssetBundle.LoadFromFile(UI_AssetBundle_Path);
        GameObject chessBoard = _uiAbs.LoadAsset<GameObject>(ChessBoard_Path);
        _blackPrefab = _uiAbs.LoadAsset<GameObject>(Black_Path);
        _whiteParefab = _uiAbs.LoadAsset<GameObject>(White_Path);
        GameObject obj=Instantiate(chessBoard);
        GetChessBoardVertexs(obj);
        _gridWidth = (_rightTopPos.x - _leftTopPos.x) / ChessBoard_Grid_Num;
        _gridHeight = (_leftTopPos.y - _leftBottomPos.y) / ChessBoard_Grid_Num;
        _minGridDis = _gridWidth < _gridHeight ? _gridWidth : _gridHeight;
        GetChessPos();


    }

    private void FixedUpdate()
    {
        if(Input.GetMouseButton(0))
        {
            _pointerPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            for(int i=0;i<=ChessBoard_Grid_Num;i++)
            {
                for(int j=0;j<=ChessBoard_Grid_Num;j++)
                {
                    var chess = _chessState[i, j];
                    Vector3 chessPos =_chessPos[i,j];
                    //找到最接近鼠标点击位置的落子点，如果空则落子
                    if (Dis(_pointerPos, chessPos) < _minGridDis / 2 && chess == ChessState.None)
                    {
                        //根据下棋顺序确定落子颜色
                        chess = _chessTurn == Turn.black ? ChessState.BlackChess:ChessState.WhiteChess;
                        //落子成功，更换下棋顺序
                        _chessTurn = _chessTurn == Turn.black ? Turn.white : Turn.black;
                        switch (chess)
                        {
                            //todo
                            case ChessState.BlackChess:
                                Instantiate(_blackPrefab, chessPos,Quaternion.identity);
                                break;
                            case ChessState.WhiteChess:
                                Instantiate(_whiteParefab, chessPos, Quaternion.identity);
                                break;
                            default:
                                break;
                        }
                    }

                }
            }

        }
    }


    private void GetChessBoardVertexs(GameObject chessBoard)
    {
        var vertexObjs = chessBoard.GetComponentsInChildren<Transform>();        //GetComponentsInChildren回读取父物体中的Transform
        _leftTopPos = vertexObjs[1].position;
        _rightTopPos = vertexObjs[2].position;
        _leftBottomPos = vertexObjs[3].position;
        _rightBottomPos = vertexObjs[4].position;
    }

    /// <summary>
    /// 获取棋盘上所有可以落子的位置坐标
    /// </summary>
    private void GetChessPos()
    {
        Vector3 pos = Vector3.zero;
        _chessPos = new Vector3[ChessBoard_Grid_Num+1, ChessBoard_Grid_Num+1];
        for(int i=0;i<=ChessBoard_Grid_Num;i++)
        {
            for(int j=0;j<= ChessBoard_Grid_Num; j++)
            {
                pos.x = _leftBottomPos.x + _gridWidth * i;
                pos.y = _leftBottomPos.y + _gridHeight * j;
                _chessPos[i, j] = pos;
                _chessState[i, j] = ChessState.None;
            }
        }                                                                                                            
    }

    //计算平面距离函数
    private float Dis(Vector3 mPos, Vector3 gridPos)
    {
        return Mathf.Sqrt(Mathf.Pow(mPos.x - gridPos.x, 2) + Mathf.Pow(mPos.y - gridPos.y, 2));
    }


}
