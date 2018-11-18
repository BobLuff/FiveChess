using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private GameObject _whiteWinPrefab;
    private GameObject _blackWinPrefab;

    private AssetBundle _uiAbs;
    private Vector3 _leftTopPos;
    private Vector3 _rightTopPos;
    private Vector3 _leftBottomPos;
    private Vector3 _rightBottomPos;
    private Vector3 _pointerPos;

    private Chess[,] _chess;
    private Chess _curChess = new Chess();


    private Turn _chessTurn = Turn.black; 
    private float _gridWidth;                                                                         //棋盘格宽度
    private float _gridHeight;                                                                        //棋盘格高度
    private float _minGridDis;

    private bool _isWin = false;

    private const string UI_AssetBundle_Path = "Assets/StreamingAssets/ui.unity3d";
    private const string ChessBoard_Path = "ChessBoard";
    private const string Black_Path = "Black";
    private const string White_Path = "White";
    private const string BlackWin_Path = "BlackWin";
    private const string WhiteWin_Path = "WhiteWin";
    private const int ChessBoard_Grid_Num = 14;                                                       //棋盘网格总数
    private const int Win_Num = 5;                                                                    //连续五个相同颜色的棋子即可获胜





    private void Awake()
    {
        _chess = new Chess[15, 15];
        _uiAbs = AssetBundle.LoadFromFile(UI_AssetBundle_Path);
        GameObject chessBoard = _uiAbs.LoadAsset<GameObject>(ChessBoard_Path);
        _blackPrefab = _uiAbs.LoadAsset<GameObject>(Black_Path);
        _whiteParefab = _uiAbs.LoadAsset<GameObject>(White_Path);
        _whiteWinPrefab = _uiAbs.LoadAsset<GameObject>(WhiteWin_Path);
        _blackWinPrefab = _uiAbs.LoadAsset<GameObject>(BlackWin_Path);
        GameObject obj=Instantiate(chessBoard);
        GetChessBoardVertexs(obj);
        _gridWidth = (_rightTopPos.x - _leftTopPos.x) / ChessBoard_Grid_Num;
        _gridHeight = (_leftTopPos.y - _leftBottomPos.y) / ChessBoard_Grid_Num;
        _minGridDis = _gridWidth < _gridHeight ? _gridWidth : _gridHeight;
        GetChessPos();


    }

    private void FixedUpdate()
    {
        if(Input.GetMouseButton(0)&&_isWin==false)
        {
            _pointerPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            for(int i=0;i<=ChessBoard_Grid_Num;i++)
            {
                for(int j=0;j<=ChessBoard_Grid_Num;j++)
                {
                    _curChess = _chess[i, j];
                    //找到最接近鼠标点击位置的落子点，如果空,则落子
                    if (Dis(_pointerPos, _curChess.Position) < _minGridDis / 2 && _curChess.CurChessState == ChessState.None)
                    {
                        //根据下棋顺序确定落子颜色
                        _chess[i,j].CurChessState = _chessTurn == Turn.black ? ChessState.BlackChess:ChessState.WhiteChess;
                        //落子成功，更换下棋顺序
                        _chessTurn = _chessTurn == Turn.black ? Turn.white : Turn.black;
                        _curChess = _chess[i, j];
                        switch (_curChess.CurChessState)
                        {
                            case ChessState.BlackChess:
                                Instantiate(_blackPrefab, _curChess.Position,Quaternion.identity);
                                break;
                            case ChessState.WhiteChess:
                                Instantiate(_whiteParefab, _curChess.Position, Quaternion.identity);
                                break;
                            default:
                                break;
                        }
                        _isWin=IsWin(i, j,_curChess.CurChessState);
                        if(_isWin)
                        {
                            ShowResultPanel(_curChess.CurChessState);
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
        for(int i=0;i<=ChessBoard_Grid_Num;i++)
        {
            for(int j=0;j<= ChessBoard_Grid_Num; j++)
            {
                _chess[i, j] = new Chess();
                pos.x = _leftBottomPos.x + _gridWidth * i;
                pos.y = _leftBottomPos.y + _gridHeight * j;
                _chess[i, j].Position = pos;
                _chess[i, j].CurChessState = ChessState.None;
               
            }
        }                                                                                                            
    }

    //计算平面距离函数
    private float Dis(Vector3 mPos, Vector3 gridPos)
    {
        return Mathf.Sqrt(Mathf.Pow(mPos.x - gridPos.x, 2) + Mathf.Pow(mPos.y - gridPos.y, 2));
    }

   
    /// <summary>
    /// 判断当前棋手是否获胜
    /// </summary>
    private bool IsWin(int x, int y, ChessState chess)
    {
        int i = x, j = y;
        int count = 0; //棋子计数器
                       /*计算水平方向连续棋子个数*/
        while (i > -1 && _chess[i,j].CurChessState ==chess )
        {
            i--;
            count++; //累加左侧
        }
        i = x + 1;
        while (i < 15 && _chess[i,j].CurChessState == chess)
        {
            i++;
            count++; //累加右侧
        }
        if (count >= Win_Num)
            return true; //获胜

        /*计算竖直方向连续棋子个数*/
        i = x;
        count = 0;
        while (j > -1 && _chess[i,j].CurChessState == chess)
        {
            j--;
            count++; //累加上方
        }
        j = y + 1;
        while (j < 15 && _chess[i, j].CurChessState == chess)
        {
            j++;
            count++; //累加下方
        }
        if (count >= Win_Num)
            return true; //获胜

        /*计算左上右下方向连续棋子个数*/
        j = y;
        count = 0;
        while (i > -1 && j > -1 && _chess[i, j].CurChessState == chess)
        {
            i--;
            j--;
            count++; //累加左上
        }
        i = x + 1;
        j = y + 1;
        while (i < 15 && j < 15 && _chess[i, j].CurChessState == chess)
        {
            i++;
            j++;
            count++; //累加右下
        }
        if (count >= Win_Num)
            return true; //获胜

        /*计算右上左下方向连续棋子个数*/
        i = x;
        j = y;
        count = 0;
        while (i < 15 && j > -1 && _chess[i, j].CurChessState == chess)
        {
            i++;
            j--;
            count++; //累加右上
        }
        i = x - 1;
        j = y + 1;
        while (i > -1 && j < 15 && _chess[i, j].CurChessState == chess)
        {
            i--;
            j++;
            count++; //累加左下
        }
        if (count >= Win_Num)
            return true; //获胜

        return false; //该步没有取胜

    }


    private void ShowResultPanel(ChessState curState)
    {
        if (curState == ChessState.BlackChess)
        {
            Instantiate(_blackWinPrefab);
            Debug.Log("Black Win");


        }
        else if(curState==ChessState.WhiteChess)
        {
            Instantiate(_whiteWinPrefab);
            Debug.Log("White Win");
        }
        if(Input.GetKey(KeyCode.R))
        {
            RestartGame();
        }
    }

    private void RestartGame()
    {
        _uiAbs.Unload(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }


}
