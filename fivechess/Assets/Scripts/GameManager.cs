using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum WhoTurn                //先手顺序
{
    PlayerGo=0,
    AiGo=1,
}


public enum ChessState          //落子情况
{
    None=0,
    BlackChess,
    WhiteChess,
}


public class GameManager : MonoBehaviour {


    [SerializeField]
    private Camera _mainCamera;
    [SerializeField]
    private GameObject _panel;
    [SerializeField]
    private GameObject _showPanel;
    [SerializeField]
    private Text _blackText;
    [SerializeField]
    private Text _whiteText;

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

    private Chess[,] _chess;                                                                         //棋盘
    private Chess _curChess = new Chess();


    private int _chessX;
    private int _chessY;
    private int _playerX;                                                                             //玩家走的位置
    private int _playerY;
    private WhoTurn _whoTurn;                                                      //该谁下棋
    private float _gridWidth;                                                                         //棋盘格宽度
    private float _gridHeight;                                                                        //棋盘格高度
    private float _minGridDis;

    private AI _ai = new AI();
    private bool _isWin = false;
    private bool _isPlayerFirst;

    private bool _isStartGame = false;
    private bool _canUndo = false;

    private GameObject _lastObj=null;
    private GameObject _curObj=null;
    private GameObject _curInstantiationObj = null;




    private void Awake()
    {
        _chess = new Chess[15, 15];
        _uiAbs = AssetBundle.LoadFromFile(ConstKey.UI_AssetBundle_Path);
        GameObject chessBoard = _uiAbs.LoadAsset<GameObject>(ConstKey.ChessBoard_AssetBundle_Path);
        _blackPrefab = _uiAbs.LoadAsset<GameObject>(ConstKey.Black_AssetBundle_Path);
        _whiteParefab = _uiAbs.LoadAsset<GameObject>(ConstKey.White_AssetBundle_Path);
        _whiteWinPrefab = _uiAbs.LoadAsset<GameObject>(ConstKey.WhiteWin_AssetBundle_Path);
        _blackWinPrefab = _uiAbs.LoadAsset<GameObject>(ConstKey.BlackWin_AssetBundle_Path);
        GameObject obj=Instantiate(chessBoard);
        GetChessBoardVertexs(obj);
        _gridWidth = (_rightTopPos.x - _leftTopPos.x) / ConstKey.ChessBoard_Grid_Num;
        _gridHeight = (_leftTopPos.y - _leftBottomPos.y) /  ConstKey.ChessBoard_Grid_Num;
        _minGridDis = _gridWidth < _gridHeight ? _gridWidth : _gridHeight;
        GetChessPos();


    }

    private void FixedUpdate()
    {
        if(_isStartGame)
        {
            if (Input.GetMouseButton(0) && _isWin == false && _whoTurn == WhoTurn.PlayerGo)
            {
                _pointerPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                PlayerGo(_pointerPos);
            }
        }

        if (Input.GetKey(KeyCode.R)&&_isWin)
        {
            RestartGame();
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
        for(int i=0;i<ConstKey.BoardCrossCount;i++)
        {
            for(int j=0;j< ConstKey.BoardCrossCount; j++)
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
    /// AI下棋
    /// </summary>
    private void AiGo(int x,int y)
    {
        _playerX = x;
        _playerY = y;
        _ai.ComputerDo(x, y, out _chessX, out _chessY);
        _chess[_chessX, _chessY].CurChessState =_isPlayerFirst? ChessState.WhiteChess:ChessState.BlackChess;
        _curInstantiationObj = _isPlayerFirst ? _whiteParefab : _blackPrefab;
        _curChess = _chess[_chessX, _chessY];
        _curObj=Instantiate(_curInstantiationObj, _curChess.Position, Quaternion.identity);
        var isWin=IsWin(_chessX, _chessY, _curChess.CurChessState);
        if(isWin)
        {
            ShowResultPanel(_curChess.CurChessState);
            _isWin = true;
        }
        else
        {
            _whoTurn = WhoTurn.PlayerGo;
            _canUndo = true;
        }
        

    }

    /// <summary>
    /// 玩家下棋
    /// </summary>
    private void PlayerGo(Vector3 pointerPos)
    {
        for (int i = 0; i < ConstKey.BoardCrossCount; i++)
        {
            for (int j = 0; j < ConstKey.BoardCrossCount; j++)
            {
                _curChess = _chess[i, j];
                if (_curChess.CurChessState==ChessState.None && Dis(_pointerPos, _curChess.Position) < _minGridDis / 2)                          //找到最接近鼠标点击位置的点，如果空,则落子
                {
                    _chess[i, j].CurChessState =_isPlayerFirst? ChessState.BlackChess:ChessState.WhiteChess;                                                                          
                    _curInstantiationObj = _isPlayerFirst ? _blackPrefab : _whiteParefab;
                    _lastObj=Instantiate(_curInstantiationObj, _curChess.Position, Quaternion.identity);
                    _curChess = _chess[i, j];
                    //落子成功，更换下棋顺序
                    _whoTurn = WhoTurn.AiGo;
                    _isWin = IsWin(i, j,_curChess.CurChessState);
                    if (_isWin)
                    {
                        ShowResultPanel(_curChess.CurChessState);
                    }
                    else
                    {
                        AiGo(i, j);
                    }
                    return;

                }
            }
        }

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
        if (count >= ConstKey.WinNum)
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
        if (count >= ConstKey.WinNum)
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
        if (count >= ConstKey.WinNum)
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
        if (count >= ConstKey.WinNum)
            return true; //获胜

        return false; //该步没有取胜

    }


    private void ShowResultPanel(ChessState curState)
    {
        if (curState == ChessState.BlackChess)
        {
            Instantiate(_blackWinPrefab);
            Debug.Log("Player Win");
        }
        else if(curState==ChessState.WhiteChess)
        {
            Instantiate(_whiteWinPrefab);
            Debug.Log("AI Win");
        }

    }

    public void RestartGame()
    {
        _uiAbs.Unload(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void ComputerFirst()
    {
        _isPlayerFirst = false;
        _whoTurn = WhoTurn.AiGo;
        _ai.ComputerFirst(out _chessX,out _chessY);
        StartPlaying();
        _chess[_chessX, _chessY].CurChessState = ChessState.BlackChess;                                                                          //默认先手为黑子
        _lastObj = Instantiate(_blackPrefab, _curChess.Position, Quaternion.identity);
        _whoTurn = WhoTurn.PlayerGo;
    }

    public void PlayerFirst()
    {
        _isPlayerFirst = true;
        _whoTurn = WhoTurn.PlayerGo;
        StartPlaying();
    }

    private void StartPlaying()
    {
        _isStartGame = true;
        _panel.SetActive(false);
        _showPanel.SetActive(true);
        if(_isPlayerFirst)
        {
            _blackText.text = string.Format("<color=red>我方</color>");
            _whiteText.text = string.Format("电脑");
        }
        else
        {
            _blackText.text = string.Format("电脑");
            _whiteText.text = string.Format("<color=red>我方</color>");
        }
        
    }

    public void OnClickOnDo()
    {
        if(_whoTurn==WhoTurn.PlayerGo&&_canUndo==true&&_lastObj!=null)
        {
            _canUndo = false;
            _chess[_chessX, _chessY].CurChessState = ChessState.None;
            _chess[_playerX, _playerY].CurChessState = ChessState.None;
            if(_lastObj!=null)
            {
                Destroy(_lastObj);
                _lastObj = null;
            }
            if(_curObj!=null)
            {
                Destroy(_curObj);
                _curObj = null;
            }


        }

    }



}
