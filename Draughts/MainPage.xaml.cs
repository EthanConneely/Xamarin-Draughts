using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Draughts
{
    public partial class MainPage : ContentPage
    {
        private const int BOARD_ROWS = 8;
        private const int BOARD_COLS = 8;

        private int whiteScore;
        private int blackScore;

        private Image selectedPiece;
        readonly List<Image> pieces = new List<Image>();

        public MainPage()
        {
            InitializeComponent();
            InitialiseGameBoard();

            // This forces the board to be square found it on stack overflow and adapted it to work with mine
            GameBoardContainer.SizeChanged += (s, e) =>
            {
                GameBoard.HeightRequest = GameBoardContainer.Width;

                if (GameBoardContainer.Width > GameBoardContainer.Height)
                {
                    GameBoard.WidthRequest = GameBoardContainer.Height;
                }
                else
                {
                    GameBoard.WidthRequest = GameBoardContainer.Width;
                }
            };
        }

        private void InitialiseGameBoard()
        {
            // add rows and columns to the grid.
            for (int i = 0; i < BOARD_ROWS; i++)
            {
                GameBoard.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < BOARD_COLS; i++)
            {
                GameBoard.ColumnDefinitions.Add(new ColumnDefinition());
            }

            Image background = new Image();
            background.Source = "Board.png";
            background.SetValue(Grid.ColumnSpanProperty, 8);
            background.SetValue(Grid.RowSpanProperty, 8);
            background.SetValue(Grid.ColumnProperty, 0);
            background.SetValue(Grid.RowProperty, 0);
            GameBoard.Children.Add(background);

            AddSquaresToTheBoard();
            AddPiecesToBoard();

            selectedPiece = null;
        }

        private void AddPiecesToBoard()
        {
            int index = 0;

            TapGestureRecognizer t_sq = new TapGestureRecognizer();
            t_sq.NumberOfTapsRequired = 1;
            t_sq.Tapped += Piece_Tapped;

            for (int y = 0; y < BOARD_ROWS; y++)
            {
                for (int x = 0; x < BOARD_COLS; x++)
                {
                    if ((y + x) % 2 == 1 && y != 3 && y != 4)
                    {
                        Image piece = new Image();
                        pieces.Add(piece);

                        if (y < 4)
                        {
                            piece.Source = "black_piece.png";
                            piece.StyleId = "black";
                        }
                        else
                        {
                            piece.Source = "white_piece.png";
                            piece.StyleId = "white";
                        }

                        piece.HorizontalOptions = LayoutOptions.Center;
                        piece.VerticalOptions = LayoutOptions.Center;
                        piece.SetValue(Grid.RowProperty, y);
                        piece.SetValue(Grid.ColumnProperty, x);
                        piece.GestureRecognizers.Add(t_sq);

                        // add the boxview to the collection called Children on the Grid.
                        GameBoard.Children.Add(piece);
                        index++;
                    }
                }
            }
        }

        private void AddSquaresToTheBoard()
        {
            TapGestureRecognizer t_sq = new TapGestureRecognizer();
            t_sq.NumberOfTapsRequired = 1;
            t_sq.Tapped += Square_Tapped;

            for (int y = 0; y < BOARD_ROWS; y++)
            {
                for (int x = 0; x < BOARD_COLS; x++)
                {
                    if ((x + y) % 2 != 0)
                    {
                        // add one boxview.
                        BoxView sq = new BoxView();

                        sq.BackgroundColor = Color.Transparent;
                        sq.GestureRecognizers.Add(t_sq);
                        sq.SetValue(Grid.ColumnProperty, x);
                        sq.SetValue(Grid.RowProperty, y);
                        GameBoard.Children.Add(sq);
                    }
                }
            }
        }

        private void Square_Tapped(object sender, EventArgs e)
        {
            // is there a current piece selected? if not, return.
            if (selectedPiece == null) return;

            BoxView clickableCell = (BoxView)sender;
            int targetX = (int)clickableCell.GetValue(Grid.ColumnProperty);
            int targetY = (int)clickableCell.GetValue(Grid.RowProperty);

            int verticalDiretion = 0;

            if (selectedPiece.StyleId == "black")// Move Up
            {
                verticalDiretion = -1;
            }
            else if (selectedPiece.StyleId == "white")// Move Up
            {
                verticalDiretion = 1;
            }

            bool isValidNormalMove;
            bool isValidTakeMove;

            // We are crowned
            if (verticalDiretion == 0)
            {
                isValidNormalMove = IsValidMove(1, targetX, targetY) || IsValidMove(-1, targetX, targetY);
                isValidTakeMove = IsValidTake(1, targetX, targetY) || IsValidTake(-1, targetX, targetY);
            }
            else
            {
                isValidNormalMove = IsValidMove(verticalDiretion, targetX, targetY);
                isValidTakeMove = IsValidTake(verticalDiretion, targetX, targetY);
            }

            // Check for normal Move || Check for take piece
            if (isValidNormalMove || isValidTakeMove)
            {
                // Check for crowning
                if (targetY == 0 || targetY == 7)
                {
                    if (selectedPiece.StyleId.Contains("crowned") == false)
                    {
                        selectedPiece.Source = selectedPiece.StyleId + "_crowned_piece.png";
                        selectedPiece.StyleId += "_crowned";
                    }
                }

                // get this square row and col and set the row/col of the currentSelectedPiece
                selectedPiece.SetValue(Grid.RowProperty, clickableCell.GetValue(Grid.RowProperty));
                selectedPiece.SetValue(Grid.ColumnProperty, clickableCell.GetValue(Grid.ColumnProperty));
                selectedPiece.BackgroundColor = Color.Transparent;

                selectedPiece = null;
            }
        }

        private Image GetPieceAt(int targetX, int targetY)
        {
            foreach (Image piece in pieces)
            {
                if (piece.Parent != GameBoard)
                {
                    continue;
                }
                int x = (int)piece.GetValue(Grid.ColumnProperty);
                int y = (int)piece.GetValue(Grid.RowProperty);

                if (x == targetX && y == targetY)
                {
                    return piece;
                }
            }

            return null;
        }

        private void TakePiece(Image piece)
        {
            piece.SetValue(Grid.RowProperty, 0);
            piece.IsEnabled = false;

            if (piece.StyleId.Contains("white"))
            {
                TakenWhitePieces.Children.Add(piece);
                piece.SetValue(Grid.ColumnProperty, blackScore);
            }
            else
            {
                TakenBlackPieces.Children.Add(piece);
                piece.SetValue(Grid.ColumnProperty, whiteScore);
            }
        }

        private bool IsValidMove(int direction, int targetX, int targetY)
        {
            // move the current piece to the current location
            int pieceX = (int)selectedPiece.GetValue(Grid.ColumnProperty);
            int pieceY = (int)selectedPiece.GetValue(Grid.RowProperty);

            bool above = targetY + direction == pieceY;
            bool topLeft = targetX - direction == pieceX;
            bool topRight = targetX + direction == pieceX;

            return above && (topLeft || topRight);
        }

        private bool IsValidTake(int direction, int targetX, int targetY)
        {
            int xDiff = targetX - (int)selectedPiece.GetValue(Grid.ColumnProperty);
            int yDiff = targetY - (int)selectedPiece.GetValue(Grid.RowProperty);
            xDiff /= 2;
            yDiff /= 2;

            int x = targetX - xDiff;
            int y = targetY - yDiff;

            Image piece = GetPieceAt(x, y);

            if (piece != null && direction == -yDiff && selectedPiece.StyleId.Replace("_crowned", "") != piece.StyleId.Replace("_crowned", ""))
            {
                TakePiece(piece);

                if (selectedPiece.StyleId.Contains("white"))
                {
                    whiteScore++;
                }
                else
                {
                    blackScore++;
                }

                return true;
            }

            return false;
        }

        private void Piece_Tapped(object sender, EventArgs e)
        {
            if (selectedPiece != null)
            {
                selectedPiece.BackgroundColor = Color.Transparent;
            }

            Image piece = (Image)sender;
            piece.BackgroundColor = Color.Orange;
            selectedPiece = piece;
        }
    }
}