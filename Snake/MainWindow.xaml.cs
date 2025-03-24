// Game inspired by the tutorial from OttoBotCode - Programming a Snake Game in C# - Full Guide
// https://www.youtube.com/watch?v=uzAXxFBbVoE


using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Snake;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
    {
        {GridValue.Empty, Images.Empty },
        {GridValue.Snake, Images.Body },
        {GridValue.Food, Images.Food },
    };

    private readonly Dictionary<Direction, int> dirToRotation = new()
    {
        {Direction.Up, 0 },
        {Direction.Right, 90},
        {Direction.Down, 180},
        {Direction.Left, 270},
    };

    private readonly int rows = 15, cols = 30;//can increase the the rows and colums but they have to be equal
    private readonly Image[,] gridImages;
    private GameState gameState;
    private bool gameRunning;
    public MainWindow()
    {
        InitializeComponent();
        gridImages = SetupGrid();
        gameState = new GameState(rows, cols);
    }

    private async Task RunGame()
    {
        this.Focus(); // Ensure the window has focus
        Draw();
        await ShowCountDown();
        Overlay.Visibility = Visibility.Hidden;
        await GameLoop();
        await ShowGameOver();
        gameState = new GameState(rows, cols);
    }

    private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if(Overlay.Visibility == Visibility.Visible)
        {
            e.Handled = true;
        }
        if (!gameRunning)
        {
            gameRunning = true;
            await RunGame();
            gameRunning = false;
        }
    }


    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (gameState.GameOver)
        {
            return;
        }

        //key coralation to the direction
        Direction newDirection = gameState.Dir; // Current direction
        switch (e.Key)
        {
            case Key.A:
                newDirection = Direction.Left;
                break;
            case Key.D:
                newDirection = Direction.Right;
                break;
            case Key.W:
                newDirection = Direction.Up;
                break;
            case Key.S:
                newDirection = Direction.Down;
                break;
        }

        // Prevent reversing direction directly
        if (newDirection != gameState.Dir.Opposite())
        {
            gameState.ChangeDirection(newDirection);
        }
    }


    private async Task GameLoop()
    {
        while (!gameState.GameOver)
        {
            await Task.Delay(500);//Can change to make the game slower or faster Defualt set 100ms.
            gameState.Move();
            Draw();
        }
    }

    private Image[,] SetupGrid()
    {
        Image[,] images = new Image[rows, cols];
        GameGrid.Rows = rows;
        GameGrid.Columns = cols;
        GameGrid.Width = GameGrid.Height * (cols / (double)rows);//incase the rows and cols are not even.

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Image image = new Image
                {
                    Source = Images.Empty,
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };

                images[r, c] = image; 
                GameGrid.Children.Add(image); 
            }
        }

        return images;
    }

    private void Draw()
    {
        DrawGrid();
        DrawSnakeHead();
        ScoreText.Text = $"SCORE {gameState.Score}";
    } 

    private void DrawGrid()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c= 0; c< cols; c++)
            {
                GridValue gridVal = gameState.Grid[r,c];
                gridImages[r, c].Source = gridValToImage[gridVal];
                gridImages[r, c].RenderTransform = Transform.Identity;
            }
        }
    }
    
    private void DrawSnakeHead()
    {
        Position headPos = gameState.HeadPosition();
        Image image = gridImages[headPos.Row, headPos.Col];
        image.Source = Images.Head;

        int rotation = dirToRotation[gameState.Dir];
        image.RenderTransform = new RotateTransform(rotation);
    }

    private async Task DrawDeadSnake()
    {
        List<Position> position = new List<Position>(gameState.SnakePositions());
        for (int i = 0; i < position.Count; i++)
        {
            Position pos = position[i];
            ImageSource source  = ( i == 0) ? Images.DeadHead : Images.DeadBody;
            gridImages[pos.Row, pos.Col].Source = source;
            await Task.Delay(50);
        }
    }

     private async Task ShowCountDown()
     {
        for (int i = 3; i >= 1; i--)
        {
            OverlayText.Text = i.ToString();
            await Task.Delay(500);
        }
     }

    private async Task ShowGameOver()
    {
        await DrawDeadSnake();
        await Task.Delay(1000);
        Overlay.Visibility = Visibility.Visible;
        OverlayText.Text = "PRESS ANY KEY TO START";
    }

}
