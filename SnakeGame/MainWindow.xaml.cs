using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;




// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SnakeGame
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private const int TileSize = 40;
        private const int GridWidth = 40;
        private const int GridHeight = 30;
        private const int IntialSnakeLength = 3;
        private int score = 0;
        private List<Windows.Foundation.Point> snake = new List<Windows.Foundation.Point>();
        private Windows.Foundation.Point food;
        private int speed = 500;

        public enum Direction
        {
            Up,
            Down,
            Left, 
            Right
        }

        private Direction currentDirection = Direction.Right;
        private DispatcherTimer gameTimer = new DispatcherTimer();
        private bool isGameInProgress = false;
        private TextBlock textBlock = new TextBlock();

        // colors for the game

        Color grayColor = Color.FromArgb(255, 128, 128, 128);
        Color grayWithTransparency = Color.FromArgb(128, 128, 128, 128);
        Color greenColor = Color.FromArgb(255, 0, 255, 0);
        Color yellowColor = Color.FromArgb(255, 255, 255, 0);
        Color blackColor = Color.FromArgb(255, 0, 0, 0);
        Color redColor = Color.FromArgb(255, 255, 0, 0);



        public MainWindow()
        {
            this.InitializeComponent();
            GameCanvas.Focus(FocusState.Keyboard);
            gameTimer.Interval = TimeSpan.FromMilliseconds(speed);
            gameTimer.Tick += GameLoop;

            InitializeGame();
        }

        private void InitializeGame()
        {
            for (int i = 0; i < IntialSnakeLength; i++)
                snake.Add(new Windows.Foundation.Point(GridWidth / 4, GridHeight / 4));
            food = GenerateFoodPosition();

            StartGame();
        }
        
        private void DrawGrid()
        {
            for (int x = 0;  x < GridWidth / 2; x++) 
            {
                for(int y = 0; y < GridHeight / 2; y++)
                {
                    Rectangle rect = new Rectangle
                    {
                        Width = TileSize,
                        Height = TileSize,
                        Stroke = new SolidColorBrush(grayColor),
                        StrokeThickness = 1,
                        Fill = new SolidColorBrush(grayWithTransparency)
                    };

                    Canvas.SetLeft(rect, x * TileSize);
                    Canvas.SetTop(rect, y * TileSize);

                    GameCanvas.Children.Add(rect);

                }
            }
        }

        private Windows.Foundation.Point GenerateFoodPosition()
        {
            Random random = new Random();
            Windows.Foundation.Point foodPosition;

            do
            {
                int x = random.Next(GridWidth / 2);
                int y = random.Next(GridHeight / 2);
                foodPosition = new Windows.Foundation.Point(x, y);
            }
            while (snake.Any(segment => segment == foodPosition)); 
            return foodPosition;
        }

        private void DrawSnake()
        {
            foreach(var segment in snake)
            {
                Rectangle rect = new Rectangle
                {
                    Width = TileSize,
                    Height = TileSize,
                    Fill = new SolidColorBrush(greenColor)
                };
                Rectangle innerRect = new Rectangle
                {
                    Width = TileSize - 12,
                    Height = TileSize - 12,
                    Fill = new SolidColorBrush(yellowColor)
                };

                Canvas.SetLeft(rect, segment.X * TileSize);
                Canvas.SetTop(rect, segment.Y * TileSize); 

                Canvas.SetLeft(innerRect, segment.X * TileSize + 6);
                Canvas.SetTop(innerRect, segment.Y * TileSize + 6);

                GameCanvas.Children.Add(rect);
                GameCanvas.Children.Add(innerRect);

                if (segment.Equals(snake.First()))
                {
                    Ellipse leftEye = new Ellipse
                    {
                        Width = 6,
                        Height = 6,
                        Fill = new SolidColorBrush(blackColor)
                    };

                    Ellipse rightEye = new Ellipse
                    {
                        Width = 6,
                        Height = 6,
                        Fill = new SolidColorBrush(blackColor)
                    };

                    switch (currentDirection)
                    {
                        case Direction.Up:
                            Canvas.SetLeft(leftEye, segment.X * TileSize + 10);
                            Canvas.SetTop(leftEye, segment.Y * TileSize + 10);
                            Canvas.SetLeft(rightEye, segment.X * TileSize + 25);
                            Canvas.SetTop(rightEye, segment.Y * TileSize + 10);
                            break;
                        case Direction.Down:
                            Canvas.SetLeft(leftEye, segment.X * TileSize + 10);
                            Canvas.SetTop(leftEye, segment.Y * TileSize + 25);
                            Canvas.SetLeft(rightEye, segment.X * TileSize + 25);
                            Canvas.SetTop(rightEye, segment.Y * TileSize + 25);
                            break;
                        case Direction.Left:
                            Canvas.SetLeft(leftEye, segment.X * TileSize + 10);
                            Canvas.SetTop(leftEye, segment.Y * TileSize + 10);
                            Canvas.SetLeft(rightEye, segment.X * TileSize + 10);
                            Canvas.SetTop(rightEye, segment.Y * TileSize + 25);
                            break;
                        case Direction.Right:
                            Canvas.SetLeft(leftEye, segment.X * TileSize + 25);
                            Canvas.SetTop(leftEye, segment.Y * TileSize + 10);
                            Canvas.SetLeft(rightEye, segment.X * TileSize + 25);
                            Canvas.SetTop(rightEye, segment.Y * TileSize + 25);
                            break;
                    }

                    GameCanvas.Children.Add(leftEye);
                    GameCanvas.Children.Add(rightEye);

                }

            }
        }

        private void DrawFood()
        {
            Ellipse ellipse = new Ellipse
            {
                Width = TileSize - 6,
                Height = TileSize - 6,
                Fill = new SolidColorBrush(redColor)
            };
            Canvas.SetLeft(ellipse, food.X * TileSize + 3);
            Canvas.SetTop(ellipse, food.Y * TileSize + 3);
            GameCanvas.Children.Add(ellipse);
        }

        private void GameLoop(object sender, object e)
        {
            MoveSnake();
            if(IsGameOver())
            {
                EndGame();
            }
            else
            {
                DrawGame();
            }
        }

        private void MoveSnake()
        {
            Windows.Foundation.Point newHead = snake.First();
            switch (currentDirection) 
            {
                case Direction.Up:
                    newHead.Y--;
                    break;
                case Direction.Down: 
                    newHead.Y++; 
                    break;
                case Direction.Left:
                    newHead.X--;
                    break;
                case Direction.Right:
                    newHead.X++;
                    break;

            }
            if (newHead == food) 
            {
                snake.Insert(0, newHead);
                food = GenerateFoodPosition();
                score++;
                Score.Text = "Score: " + score.ToString();
                ChangeSpeed(score);
            }
            else
            {
                snake.Insert(0, newHead);
                snake.RemoveAt(snake.Count - 1);
            }
        }

        private void ChangeSpeed(int score)
        {
            if (score >= 5)
            {
                speed = 400;
            }
            else if (score >= 10)
            {
                speed = 300;
            }
            else if (score >= 15)
            {
                speed = 200;
            }
            else if (score >= 20)
            {
                speed = 100;
            }
            else
            {
                speed = 500;
            }
        }

        private bool IsGameOver()
        {
            Windows.Foundation.Point head = snake.First(); 
            if (head.X < 0 || head.X >= GridWidth / 2 || head.Y < 0 || head.Y >= GridHeight / 2) 
            {
                return true;
            }

            if (snake.Skip(1).Any(segment => segment == head)) 
            {
                return true;
            }
            
            return false; 
        }

        private void StartGame()
        {
            isGameInProgress = true;
            gameTimer.Start();
            DrawGame();
        }

        private async void EndGame()
        {
            isGameInProgress = false;
            gameTimer.Stop();
            GameCanvas.Children.Clear();
            DrawGameOverText();
            await BlinkSnake();
            snake.Clear();
            score = 0;
            Score.Text = "Score: 0";
            InitializeGame();
        }

        private void DrawGame()
        {
            GameCanvas.Children.Clear();
            DrawFood();
            DrawGrid();
            DrawSnake();
        }



        public void GameCanvas_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (isGameInProgress)
            {
                switch (e.Key)
                {
                    case Windows.System.VirtualKey.W:
                        if (currentDirection != Direction.Down)
                        {
                            currentDirection = Direction.Up;

                        }
                        break;
                    case Windows.System.VirtualKey.S:
                        if(currentDirection != Direction.Up)
                        {
                            currentDirection = Direction.Down;
                        }
                        break;
                    case Windows.System.VirtualKey.A:
                        if (currentDirection != Direction.Right)
                        {
                            currentDirection = Direction.Left;
                        }
                        break;
                    case Windows.System.VirtualKey.D:
                        if(currentDirection != Direction.Left)
                        {
                            currentDirection = Direction.Right;
                        }
                        break;



                }
            }

        }

        private void DrawGameOverText()
        {
            textBlock.Text = "Game Over";
            textBlock.FontSize = 40;
            GameCanvas.Children.Add(textBlock);
            GameCanvas.UpdateLayout();
            Canvas.SetLeft(textBlock, (GameCanvas.ActualWidth - textBlock.ActualWidth) / 2);
            Canvas.SetTop(textBlock, (GameCanvas.ActualHeight - textBlock.ActualHeight) / 2);
        }

        private void ClearSnake()
        {
            foreach (var child in GameCanvas.Children.OfType<Rectangle>().ToList())
            {
                GameCanvas.Children.Remove(child);
            }
        }

        private async Task BlinkSnake()
        {
            for (int i = 0; i < 3; i++)
            {
                DrawSnake();
                await Task.Delay(TimeSpan.FromSeconds(1));
                ClearSnake();
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        public void GameCanvas_LostFocus(object sender, RoutedEventArgs e)
        {
            GameCanvas.Focus(FocusState.Pointer);

        }



    }
}
