using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CheckersGame
{
    // .NET Framework
    public partial class MainForm : Form
    {
        const int MapSize = 15;
        const int CellSize = 50;

        int _currentPlayer;

        List<Button> _simpleSteps = new List<Button>();

        int _countEatSteps = 0;
        Button _prevButton;
        Button _pressedButton;
        bool _isContinue = false;

        bool _isMoving;

        int[,] _map = new int[MapSize, MapSize];

        Button[,] _buttons = new Button[MapSize, MapSize];

        Image _whiteFigure;
        Image _blackFigure;

        public MainForm()
        {
            InitializeComponent();

            _whiteFigure = new Bitmap(new Bitmap(@"w.png"), new Size(CellSize - 10, CellSize - 10));
            _blackFigure = new Bitmap(new Bitmap(@"b.png"), new Size(CellSize - 10, CellSize - 10));

            this.Text = "Checkers";

            Init();
        }

        public void Init()
        {
            _currentPlayer = 1;
            _isMoving = false;
            _prevButton = null;

            _map = new int[MapSize, MapSize] {
                { 0,1,0,1,0,1,0,1,0,1,0,1,0,1,0  },
                { 1,0,1,0,1,0,1,0,1,0,1,0,1,0,1 },
                { 0,1,0,1,0,1,0,1,0,1,0,1,0,1,0 },
                { 1,0,1,0,1,0,1,0,1,0,1,0,1,0,1 },
                { 0,1,0,1,0,1,0,1,0,1,0,1,0,1,0  },
                { 1,0,1,0,1,0,1,0,1,0,1,0,1,0,1 },
                { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
                { 2,0,2,0,2,0,2,0,2,0,2,0,2,0,2 },
                { 0,2,0,2,0,2,0,2,0,2,0,2,0,2,0 },
                { 2,0,2,0,2,0,2,0,2,0,2,0,2,0,2 },
                { 0,2,0,2,0,2,0,2,0,2,0,2,0,2,0 },
                { 2,0,2,0,2,0,2,0,2,0,2,0,2,0,2 },
                { 0,2,0,2,0,2,0,2,0,2,0,2,0,2,0 },
            };

            CreateMap();
        }

        public void ResetGame()
        {
            bool player1 = false;
            bool player2 = false;

            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                {
                    if (_map[i, j] == 1)
                        player1 = true;
                    if (_map[i, j] == 2)
                        player2 = true;
                }
            }
            if (!player1 || !player2)
            {
                this.Controls.Clear();
                Init();
            }
        }

        public void CreateMap()
        {
            this.Width = (MapSize + 1) * CellSize;
            this.Height = (MapSize + 1) * CellSize;

            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                {
                    Button button = new Button();
                    button.Location = new Point(j * CellSize, i * CellSize);
                    button.Size = new Size(CellSize, CellSize);
                    button.Click += new EventHandler(OnFigurePress);
                    if (_map[i, j] == 1)
                        button.Image = _whiteFigure;
                    else if (_map[i, j] == 2) button.Image = _blackFigure;

                    button.BackColor = GetPrevButtonColor(button);
                    button.ForeColor = Color.Red;

                    _buttons[i, j] = button;

                    this.Controls.Add(button);
                }
            }
        }

        public void SwitchPlayer()
        {
            _currentPlayer = _currentPlayer == 1 ? 2 : 1;
            ResetGame();
        }

        public Color GetPrevButtonColor(Button prevButton)
        {
            if ((prevButton.Location.Y / CellSize % 2) != 0)
            {
                if ((prevButton.Location.X / CellSize % 2) == 0)
                {
                    return Color.Gray;
                }
            }
            if ((prevButton.Location.Y / CellSize) % 2 == 0)
            {
                if ((prevButton.Location.X / CellSize) % 2 != 0)
                {
                    return Color.Gray;
                }
            }
            return Color.WhiteSmoke;
        }

        public void OnFigurePress(object sender, EventArgs e)
        {
            if (_prevButton != null)
                _prevButton.BackColor = GetPrevButtonColor(_prevButton);

            _pressedButton = sender as Button;

            if (_map[_pressedButton.Location.Y / CellSize, _pressedButton.Location.X / CellSize] != 0 && _map[_pressedButton.Location.Y / CellSize, _pressedButton.Location.X / CellSize] == _currentPlayer)
            {
                CloseSteps();
                _pressedButton.BackColor = Color.Red;
                DeactivateAllButtons();
                _pressedButton.Enabled = true;
                _countEatSteps = 0;
                if (_pressedButton.Text == "D")
                    ShowSteps(_pressedButton.Location.Y / CellSize, _pressedButton.Location.X / CellSize, false);
                else ShowSteps(_pressedButton.Location.Y / CellSize, _pressedButton.Location.X / CellSize);

                if (_isMoving)
                {
                    CloseSteps();
                    _pressedButton.BackColor = GetPrevButtonColor(_pressedButton);
                    ShowPossibleSteps();
                    _isMoving = false;
                }
                else
                    _isMoving = true;
            }
            else
            {
                if (_isMoving)
                {
                    _isContinue = false;
                    if (Math.Abs(_pressedButton.Location.X / CellSize - _prevButton.Location.X / CellSize) > 1)
                    {
                        _isContinue = true;
                        DeleteEaten(_pressedButton, _prevButton);
                    }
                    int temp = _map[_pressedButton.Location.Y / CellSize, _pressedButton.Location.X / CellSize];
                    _map[_pressedButton.Location.Y / CellSize, _pressedButton.Location.X / CellSize] =
                        _map[_prevButton.Location.Y / CellSize, _prevButton.Location.X / CellSize];
                    _map[_prevButton.Location.Y / CellSize, _prevButton.Location.X / CellSize] = temp;
                    _pressedButton.Image = _prevButton.Image;
                    _prevButton.Image = null;
                    _pressedButton.Text = _prevButton.Text;
                    _prevButton.Text = "";
                    SwitchButtonToCheat(_pressedButton);
                    _countEatSteps = 0;
                    _isMoving = false;
                    CloseSteps();
                    DeactivateAllButtons();
                    if (_pressedButton.Text == "D")
                        ShowSteps(_pressedButton.Location.Y / CellSize, _pressedButton.Location.X / CellSize, false);
                    else ShowSteps(_pressedButton.Location.Y / CellSize, _pressedButton.Location.X / CellSize);
                    if (_countEatSteps == 0 || !_isContinue)
                    {
                        CloseSteps();
                        SwitchPlayer();
                        ShowPossibleSteps();
                        _isContinue = false;
                    }
                    else if (_isContinue)
                    {
                        _pressedButton.BackColor = Color.Yellow;
                        _pressedButton.Enabled = true;
                        _isMoving = true;
                    }
                }
            }

            _prevButton = _pressedButton;
        }

        public void ShowPossibleSteps()
        {
            bool isEatStep = false;
            DeactivateAllButtons();
            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                {
                    if (_map[i, j] == _currentPlayer)
                    {
                        var isOneStep = true;
                        if (_buttons[i, j].Text == "D")
                            isOneStep = false;
                        else isOneStep = true;
                        if (IsButtonHasEatStep(i, j, isOneStep, new int[2] { 0, 0 }))
                        {
                            isEatStep = true;
                            _buttons[i, j].Enabled = true;
                        }
                    }
                }
            }
            if (!isEatStep)
                ActivateAllButtons();
        }

        public void SwitchButtonToCheat(Button button)
        {
            if (_map[button.Location.Y / CellSize, button.Location.X / CellSize] == 1 && button.Location.Y / CellSize == MapSize - 1)
            {
                button.Text = "D";

            }
            if (_map[button.Location.Y / CellSize, button.Location.X / CellSize] == 2 && button.Location.Y / CellSize == 0)
            {
                button.Text = "D";
            }
        }

        public void DeleteEaten(Button endButton, Button startButton)
        {
            int count = Math.Abs(endButton.Location.Y / CellSize - startButton.Location.Y / CellSize);
            int startIndexX = endButton.Location.Y / CellSize - startButton.Location.Y / CellSize;
            int startIndexY = endButton.Location.X / CellSize - startButton.Location.X / CellSize;
            startIndexX = startIndexX < 0 ? -1 : 1;
            startIndexY = startIndexY < 0 ? -1 : 1;
            int currCount = 0;
            int i = startButton.Location.Y / CellSize + startIndexX;
            int j = startButton.Location.X / CellSize + startIndexY;
            while (currCount < count - 1)
            {
                _map[i, j] = 0;
                _buttons[i, j].Image = null;
                _buttons[i, j].Text = "";
                i += startIndexX;
                j += startIndexY;
                currCount++;
            }

        }

        public void ShowSteps(int iCurrFigure, int jCurrFigure, bool isOnestep = true)
        {
            _simpleSteps.Clear();
            ShowDiagonal(iCurrFigure, jCurrFigure, isOnestep);
            if (_countEatSteps > 0)
                CloseSimpleSteps(_simpleSteps);
        }

        public void ShowDiagonal(int icurrFigure, int jcurrFigure, bool isOneStep = false)
        {
            int j = jcurrFigure + 1;
            for (int i = icurrFigure - 1; i >= 0; i--)
            {
                if (_currentPlayer == 1 && isOneStep && !_isContinue) break;
                if (IsInsideBorders(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j < 7)
                    j++;
                else break;

                if (isOneStep)
                    break;
            }

            j = jcurrFigure - 1;
            for (int i = icurrFigure - 1; i >= 0; i--)
            {
                if (_currentPlayer == 1 && isOneStep && !_isContinue) break;
                if (IsInsideBorders(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j > 0)
                    j--;
                else break;

                if (isOneStep)
                    break;
            }

            j = jcurrFigure - 1;
            for (int i = icurrFigure + 1; i < 8; i++)
            {
                if (_currentPlayer == 2 && isOneStep && !_isContinue) break;
                if (IsInsideBorders(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j > 0)
                    j--;
                else break;

                if (isOneStep)
                    break;
            }

            j = jcurrFigure + 1;
            for (int i = icurrFigure + 1; i < 8; i++)
            {
                if (_currentPlayer == 2 && isOneStep && !_isContinue) break;
                if (IsInsideBorders(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j < 7)
                    j++;
                else break;

                if (isOneStep)
                    break;
            }
        }

        public bool DeterminePath(int ti, int tj)
        {

            if (_map[ti, tj] == 0 && !_isContinue)
            {
                _buttons[ti, tj].BackColor = Color.ForestGreen;
                _buttons[ti, tj].Enabled = true;
                _simpleSteps.Add(_buttons[ti, tj]);
            }
            else
            {

                if (_map[ti, tj] != _currentPlayer)
                {
                    if (_pressedButton.Text == "D")
                        ShowProceduralEat(ti, tj, false);
                    else ShowProceduralEat(ti, tj);
                }

                return false;
            }
            return true;
        }

        public void CloseSimpleSteps(List<Button> simpleSteps)
        {
            if (simpleSteps.Count > 0)
            {
                for (int i = 0; i < simpleSteps.Count; i++)
                {
                    simpleSteps[i].BackColor = GetPrevButtonColor(simpleSteps[i]);
                    simpleSteps[i].Enabled = false;
                }
            }
        }
        public void ShowProceduralEat(int i, int j, bool isOneStep = true)
        {
            int dirX = i - _pressedButton.Location.Y / CellSize;
            int dirY = j - _pressedButton.Location.X / CellSize;
            dirX = dirX < 0 ? -1 : 1;
            dirY = dirY < 0 ? -1 : 1;
            int il = i;
            int jl = j;
            bool isEmpty = true;
            while (IsInsideBorders(il, jl))
            {
                if (_map[il, jl] != 0 && _map[il, jl] != _currentPlayer)
                {
                    isEmpty = false;
                    break;
                }
                il += dirX;
                jl += dirY;

                if (isOneStep)
                    break;
            }
            if (isEmpty)
                return;
            List<Button> toClose = new List<Button>();
            bool closeSimple = false;
            int ik = il + dirX;
            int jk = jl + dirY;
            while (IsInsideBorders(ik, jk))
            {
                if (_map[ik, jk] == 0)
                {
                    if (IsButtonHasEatStep(ik, jk, isOneStep, new int[2] { dirX, dirY }))
                    {
                        closeSimple = true;
                    }
                    else
                    {
                        toClose.Add(_buttons[ik, jk]);
                    }
                    _buttons[ik, jk].BackColor = Color.Yellow;
                    _buttons[ik, jk].Enabled = true;
                    _countEatSteps++;
                }
                else break;
                if (isOneStep)
                    break;
                jk += dirY;
                ik += dirX;
            }
            if (closeSimple && toClose.Count > 0)
            {
                CloseSimpleSteps(toClose);
            }

        }

        public bool IsButtonHasEatStep(int icurrFigure, int jcurrFigure, bool isOneStep, int[] dir)
        {
            bool eatStep = false;
            int j = jcurrFigure + 1;
            for (int i = icurrFigure - 1; i >= 0; i--)
            {
                if (_currentPlayer == 1 && isOneStep && !_isContinue) break;
                if (dir[0] == 1 && dir[1] == -1 && !isOneStep) break;
                if (IsInsideBorders(i, j))
                {
                    if (_map[i, j] != 0 && _map[i, j] != _currentPlayer)
                    {
                        eatStep = true;
                        if (!IsInsideBorders(i - 1, j + 1))
                            eatStep = false;
                        else if (_map[i - 1, j + 1] != 0)
                            eatStep = false;
                        else return eatStep;
                    }
                }
                if (j < 7)
                    j++;
                else break;

                if (isOneStep)
                    break;
            }

            j = jcurrFigure - 1;
            for (int i = icurrFigure - 1; i >= 0; i--)
            {
                if (_currentPlayer == 1 && isOneStep && !_isContinue) break;
                if (dir[0] == 1 && dir[1] == 1 && !isOneStep) break;
                if (IsInsideBorders(i, j))
                {
                    if (_map[i, j] != 0 && _map[i, j] != _currentPlayer)
                    {
                        eatStep = true;
                        if (!IsInsideBorders(i - 1, j - 1))
                            eatStep = false;
                        else if (_map[i - 1, j - 1] != 0)
                            eatStep = false;
                        else return eatStep;
                    }
                }
                if (j > 0)
                    j--;
                else break;

                if (isOneStep)
                    break;
            }

            j = jcurrFigure - 1;
            for (int i = icurrFigure + 1; i < 8; i++)
            {
                if (_currentPlayer == 2 && isOneStep && !_isContinue) break;
                if (dir[0] == -1 && dir[1] == 1 && !isOneStep) break;
                if (IsInsideBorders(i, j))
                {
                    if (_map[i, j] != 0 && _map[i, j] != _currentPlayer)
                    {
                        eatStep = true;
                        if (!IsInsideBorders(i + 1, j - 1))
                            eatStep = false;
                        else if (_map[i + 1, j - 1] != 0)
                            eatStep = false;
                        else return eatStep;
                    }
                }
                if (j > 0)
                    j--;
                else break;

                if (isOneStep)
                    break;
            }

            j = jcurrFigure + 1;
            for (int i = icurrFigure + 1; i < 8; i++)
            {
                if (_currentPlayer == 2 && isOneStep && !_isContinue) break;
                if (dir[0] == -1 && dir[1] == -1 && !isOneStep) break;
                if (IsInsideBorders(i, j))
                {
                    if (_map[i, j] != 0 && _map[i, j] != _currentPlayer)
                    {
                        eatStep = true;
                        if (!IsInsideBorders(i + 1, j + 1))
                            eatStep = false;
                        else if (_map[i + 1, j + 1] != 0)
                            eatStep = false;
                        else return eatStep;
                    }
                }
                if (j < 7)
                    j++;
                else break;

                if (isOneStep)
                    break;
            }
            return eatStep;
        }

        public void CloseSteps()
        {
            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                {
                    _buttons[i, j].BackColor = GetPrevButtonColor(_buttons[i, j]);
                }
            }
        }

        public bool IsInsideBorders(int ti, int tj)
        {
            if (ti >= MapSize || tj >= MapSize || ti < 0 || tj < 0)
            {
                return false;
            }
            return true;
        }

        public void ActivateAllButtons()
        {
            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                {
                    _buttons[i, j].Enabled = true;
                }
            }
        }

        public void DeactivateAllButtons()
        {
            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                {
                    _buttons[i, j].Enabled = false;
                }
            }
        }

    }
}
