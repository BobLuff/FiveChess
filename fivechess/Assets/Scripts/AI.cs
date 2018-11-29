using System.Collections;
using System.Collections.Generic;

class AI
{
    // 15*15共有572种五子连珠的可能性
    const int MaxFiveChainCount = 572;

    //玩家的可能性
    bool[,,] _playerTable = new bool[ConstKey.BoardCrossCount, ConstKey.BoardCrossCount, MaxFiveChainCount];

    //电脑的可能性
    bool[,,] _aiTable = new bool[ConstKey.BoardCrossCount, ConstKey.BoardCrossCount, MaxFiveChainCount];

    //记录2位玩家所有可能的连珠数，-1则为永远无法5连珠
    int[,] _win = new int[2, MaxFiveChainCount];

    //记录每格的分值
    int[,] _aiGrades = new int[ConstKey.BoardCrossCount, ConstKey.BoardCrossCount];
    int[,] _playerGrades = new int[ConstKey.BoardCrossCount, ConstKey.BoardCrossCount];

    //记录棋盘
    int[,] _board = new int[ConstKey.BoardCrossCount, ConstKey.BoardCrossCount];

    int _cgrade, _pgrade;
    int _icount, _m, _n;
    int _mat, _nat, _mde, _nde;

    public AI()
    {
        for (int i = 0; i < ConstKey.BoardCrossCount; i++)
        {
            for (int j = 0; j < ConstKey.BoardCrossCount; j++)
            {
                _playerGrades[i, j] = 0;
                _aiGrades[i, j] = 0;
                _board[i, j] = 0;
            }
        }

        //遍历所有的五连子可能情况的权值  
        //横  
        for (int i = 0; i < ConstKey.BoardCrossCount; i++)
        {
            for (int j = 0; j < ConstKey.BoardCrossCount - 4; j++)
            {
                for (int k = 0; k < ConstKey.WinNum; k++)
                {
                    _playerTable[j + k, i, _icount] = true;
                    _aiTable[j + k, i, _icount] = true;
                }

                _icount++;
            }
        }

        //竖
        for (int i = 0; i < ConstKey.BoardCrossCount; i++)
        {
            for (int j = 0; j < ConstKey.BoardCrossCount - 4; j++)
            {
                for (int k = 0; k < ConstKey.WinNum; k++)
                {
                    _playerTable[i, j + k, _icount] = true;
                    _aiTable[i, j + k, _icount] = true;
                }

                _icount++;
            }
        }

        // 右斜
        for (int i = 0; i < ConstKey.BoardCrossCount - 4; i++)
        {
            for (int j = 0; j < ConstKey.BoardCrossCount - 4; j++)
            {
                for (int k = 0; k < ConstKey.WinNum; k++)
                {
                    _playerTable[j + k, i + k, _icount] = true;
                    _aiTable[j + k, i + k, _icount] = true;
                }

                _icount++;
            }
        }

        // 左斜
        for (int i = 0; i < ConstKey.BoardCrossCount - 4; i++)
        {
            for (int j = ConstKey.BoardCrossCount - 1; j >= 4; j--)
            {
                for (int k = 0; k < ConstKey.WinNum; k++)
                {
                    _playerTable[j - k, i + k, _icount] = true;
                    _aiTable[j - k, i + k, _icount] = true;
                }

                _icount++;
            }
        }

        //初始化玩家和电脑连珠数
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < MaxFiveChainCount; j++)
            {
                _win[i, j] = 0;
            }
        }

        _icount = 0;
    }




    /// <summary>
    /// 计算得分
    /// </summary>
    void CalcScore()
    {
        _cgrade = 0;
        _pgrade = 0;
        _board[_m, _n] = 1;//电脑下子位置     

        for (int i = 0; i < MaxFiveChainCount; i++)
        {
            if (_aiTable[_m, _n, i] && _win[0, i] != -1)
            {
                _win[0, i]++;//给白子的所有五连子可能的加载当前连子数  
            }

            if (_playerTable[_m, _n, i])
            {
                _playerTable[_m, _n, i] = false;
                _win[1, i] = -1;
            }
        }
    }

    void CalcCore()
    {
        //遍历棋盘上的所有坐标  
        for (int i = 0; i < ConstKey.BoardCrossCount; i++)
        {
            for (int j = 0; j < ConstKey.BoardCrossCount; j++)
            {
                //该坐标的黑子奖励积分清零  
                _playerGrades[i, j] = 0;

                //在还没下棋子的地方遍历  
                if (_board[i, j] == 0)
                {
                    //遍历该棋盘可落子点上的黑子所有权值的连子情况，并给该落子点加上相应奖励分  
                    for (int k = 0; k < MaxFiveChainCount; k++)
                    {
                        if (_playerTable[i, j, k])
                        {
                            switch (_win[1, k])
                            {
                                case 1://一连子  
                                    _playerGrades[i, j] += 5;
                                    break;
                                case 2://两连子  
                                    _playerGrades[i, j] += 50;
                                    break;
                                case 3://三连子  
                                    _playerGrades[i, j] += 180;
                                    break;
                                case 4://四连子  
                                    _playerGrades[i, j] += 400;
                                    break;
                            }
                        }
                    }

                    _aiGrades[i, j] = 0;//该坐标的白子的奖励积分清零  
                    if (_board[i, j] == 0)//在还没下棋子的地方遍历  
                    {
                        //遍历该棋盘可落子点上的白子所有权值的连子情况，并给该落子点加上相应奖励分  
                        for (int k = 0; k < MaxFiveChainCount; k++)
                        {
                            if (_aiTable[i, j, k])
                            {
                                switch (_win[0, k])
                                {
                                    case 1://一连子  
                                        _aiGrades[i, j] += 5;
                                        break;
                                    case 2: //两连子  
                                        _aiGrades[i, j] += 52;
                                        break;
                                    case 3://三连子  
                                        _aiGrades[i, j] += 130;
                                        break;
                                    case 4: //四连子  
                                        _aiGrades[i, j] += 10000;
                                        break;
                                }
                            }
                        }

                    }


                }
            }
        }

    }

    public void ComputerFirst(out int finalX,out int finalY)
    {
        finalX = 7;
        finalY = 7;
        _board[7, 7] = 1;//电脑下子位置   
    }

    /// <summary>
    /// 悔棋
    /// </summary>
    public void ComputerUndo(int x,int y)
    {
        _board[x, y] = 0;
    }

    // AI计算输出, 需要玩家走过的点
    public void ComputerDo(int playerX, int playerY, out int finalX, out int finalY)
    {
        SetPlayerChess(playerX, playerY);

        CalcCore();

        for (int i = 0; i < ConstKey.BoardCrossCount; i++)
        {
            for (int j = 0; j < ConstKey.BoardCrossCount; j++)
            {
                //找出棋盘上可落子点的黑子白子的各自最大权值，找出各自的最佳落子点 
                if (_board[i, j] == 0)
                {
                    if (_aiGrades[i, j] >= _cgrade)
                    {
                        _cgrade = _aiGrades[i, j];
                        _mat = i;
                        _nat = j;
                    }

                    if (_playerGrades[i, j] >= _pgrade)
                    {
                        _pgrade = _playerGrades[i, j];
                        _mde = i;
                        _nde = j;
                    }

                }
            }
        }

        //如果白子的最佳落子点的权值比黑子的最佳落子点权值大，则电脑的最佳落子点为白子的最佳落子点，否则相反  
        if (_cgrade >= _pgrade)
        {
            _m = _mat;
            _n = _nat;
        }
        else
        {
            _m = _mde;
            _n = _nde;
        }


        CalcScore();

        finalX = _m;
        finalY = _n;
    }

    /// <summary>
    /// 设置player下棋位置
    /// </summary>
    void SetPlayerChess(int playerX, int playerY)
    {
        int m = playerX;
        int n = playerY;

        if (_board[m, n] == 0)
        {
            _board[m, n] = 2;

            for (int i = 0; i < MaxFiveChainCount; i++)
            {
                if (_playerTable[m, n, i] && _win[1, i] != -1)
                {
                    _win[1, i]++;
                }
                if (_aiTable[m, n, i])
                {
                    _aiTable[m, n, i] = false;
                    _win[0, i] = -1;
                }
            }
        }
    }

}

