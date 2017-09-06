using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Windows.Threading;
using System.Media;

namespace TheDragonsTreasury
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //images on the board
        Dictionary<Point, Image> ImageBoard;

        ElementClass[,] Board;


        ElementClass[] El; //falling elements

        int row = 13;
        int column = 6;

        DispatcherTimer Timer;

        int score;

        #region onceCalled

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ImageBoard = new Dictionary<Point, Image>();
            BindImageBoard();

            Board = new ElementClass[row, column];

            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(0.5);
            Timer.Tick += Timer_Tick;
            GenerateStack();
            
            Timer.Start();

        }

        //create images, bind to grid and dictionary
        private void BindImageBoard()
        {
            Image I;
            for (int i = 0; i < row; ++i)
                for (int j = 0; j < column; ++j)
                {
                    I = new Image(); //image object
                    I.Name = "node" + i + j;
                    I.HorizontalAlignment = HorizontalAlignment.Stretch;
                    I.VerticalAlignment = VerticalAlignment.Stretch;
                    I.Source = none.Source;
                    Grid.SetRow(I, i);
                    Grid.SetColumn(I, j);
                    workGrid.Children.Add(I); //add to the board
                    ImageBoard.Add(new Point(i, j), I); //add to the dictionary
                }
        }

        #endregion

        #region GameLoopFuncs

        bool beDeleted = false;
        private void Timer_Tick(object sender, EventArgs e) //game loop
        {
            if (!IsDoneFalling())
                StackFalling();
            else
            {
                StackDropped();
                
                while (CheckItemsToBeDeleted())
                {
                    this.Refresh();
                    System.Threading.Thread.Sleep(1000);
                    
                    DropBoardElements();
                    beDeleted = false;
                }
                GenerateStack();
                label.Content = "Jewelry:" + score;
            }
            if (Board[2,3]!=null)
            {
                MessageBoxResult M = MessageBox.Show("Game over! Your jewelry score is"+score, "Game over", MessageBoxButton.OK);
                if(M==MessageBoxResult.OK)
                {
                    Timer.Stop();
                    Environment.Exit(0);
                }
            }

        }

        private void GenerateStack()
        {
            El = new ElementClass[3];
            Random R = new Random();
            Stone St;

            for(int i=0;i<3;++i)
            {
                int r = R.Next(0, 6);
                St = (Stone)r;
                El[i] = new ElementClass(St, new Point(i, 3));
                ImageBoard[El[i].Loc].Source = ChosePicture(El[i].elType);
            }
        }

        
        private void StackFalling()
        {
            Point P;
            for (int i = 2; i >= 0; --i)
            {
                ImageBoard[El[i].Loc].Source = ChosePicture(Stone.none);
                P = new Point(El[i].Loc.X + 1, El[i].Loc.Y);
                El[i].Loc = P;
                ImageBoard[El[i].Loc].Source = ChosePicture(El[i].elType);
            }
        }

        
        private bool IsDoneFalling()
        {
            if (((El[2].Loc.X + 1) >= row)|| (Board[El[2].Loc.X + 1, El[2].Loc.Y] != null))
                return true;
            return false;
        }

        //refresh the game board
        private void StackDropped()  
        {
            for (int i = 0; i < 3; ++i)
            {
                Board[El[i].Loc.X, El[i].Loc.Y] = El[i];
                ImageBoard[El[i].Loc].Source = ChosePicture(El[i].elType);
            }
        }

        
        private bool CanBeMovedLeft()
        {
            if ((El[2].Loc.Y == 0)||(Board[El[2].Loc.X, El[2].Loc.Y - 1] != null))
                return false;
            return true;
        }

        private bool CanBeMovedRight()
        {
            if ((El[2].Loc.Y == column - 1)||(Board[El[2].Loc.X, El[2].Loc.Y + 1] != null))
                return false;
            return true;
        }

        //moves the stack
        private void MoveStack(bool left)
        {
            Point P;
            for(int i=0;i<3;++i)
            {
                ImageBoard[El[i].Loc].Source = ChosePicture(Stone.none);
                if (left)
                    P = new Point(El[i].Loc.X, El[i].Loc.Y - 1);
                else
                    P = new Point(El[i].Loc.X, El[i].Loc.Y + 1);
                El[i].Loc = P;
                ImageBoard[El[i].Loc].Source = ChosePicture(El[i].elType);
            }
        }

        //swap elements inside th stack
        private void ChangePositions()
        {
            for(int i=2;i>0;--i)
            {
                Swap(ref El[i],ref El[i - 1]);
            }
        }

        //helper
        private void Swap(ref ElementClass El1,ref ElementClass El2)
        {
            ImageSource I;
            I = ImageBoard[El1.Loc].Source;
            ImageBoard[El1.Loc].Source = ImageBoard[El2.Loc].Source;
            ImageBoard[El2.Loc].Source = I;

            Point temp = El1.Loc;
            El1.Loc = El2.Loc;
            El2.Loc = temp;
            ElementClass El = El1;
            El1 = El2;
            El2 = El; 
        }
        #endregion

        #region CheckingElements

        //checks all the field for deleting something
        private bool CheckItemsToBeDeleted()
        {
            bool somethingIsToBeDeleted = false;
            for(int i=0;i<row;++i)
                for(int j=0;j<column;++j)
                {
                    if ((Board[i, j] != null))
                        somethingIsToBeDeleted |= ElementCheck(Board[i, j]);
                }

            if(somethingIsToBeDeleted)
            foreach(ElementClass El1 in Board)
            {
                    if ((El1 != null) && (El1.toBeDeleted))
                    {
                        ImageBoard[El1.Loc].Source = clear.Source;
                        score++;
                    }
            }
            
            return somethingIsToBeDeleted;
        }

        //check one element and find same elements around
        //finds the direction to move in
        private bool ElementCheck(ElementClass El) 
        {
            bool theReturnState = false;
            int Xind = 0;
            int Yind = 0;
                for(int i=0;i<3;++i)
                {
                if ((El.Loc.X != row - 1) && ((El.Loc.Y-1+ i)<column) && ((El.Loc.Y - 1 + i)>=0))
                //not the bottom
                    if (IsSameStone(Board[El.Loc.X + 1, El.Loc.Y - 1 + i], El))
                    {
                        Xind = 1;
                        Yind = i - 1;
                        theReturnState |= CheckHelperMethod(Board[El.Loc.X, El.Loc.Y], Xind, Yind);
                    }

                if ((El.Loc.X != 0) && ((El.Loc.Y - 1 + i) < column) && ((El.Loc.Y - 1 + i)>=0)) 
                //not the top
                    if (IsSameStone(Board[El.Loc.X - 1, El.Loc.Y - 1 + i], El))
                    {
                        Xind = -1;
                        Yind = i - 1;
                        theReturnState |= CheckHelperMethod(Board[El.Loc.X, El.Loc.Y], Xind, Yind);
                    }
                }
                // not to the left
                if((El.Loc.Y!=0)&&(El.Loc.Y-1>=0)) 
                    if(IsSameStone(Board[El.Loc.X, El.Loc.Y - 1],El))
                    {
                        Xind = 0;
                        Yind = -1;
                        theReturnState |= CheckHelperMethod(Board[El.Loc.X, El.Loc.Y], Xind, Yind);
                    }
                //not to the right
                if ((El.Loc.Y != column - 1)&&(El.Loc.Y+1>=0)) 
                    if (IsSameStone(Board[El.Loc.X, El.Loc.Y + 1], El))
                    {
                        Xind = 0;
                        Yind = 1;
                        theReturnState |= CheckHelperMethod(Board[El.Loc.X, El.Loc.Y], Xind, Yind);
                    }

            return theReturnState;             
        }

        private bool CheckHelperMethod(ElementClass El, int Xind, int Yind)
        {
            bool theReturnState = false;
            int Xplus = Xind;
            int Yplus = Yind;
            if((Xind!=0)||(Yind!=0))
                while ((CheckStraight(Board[El.Loc.X + Xplus, El.Loc.Y + Yplus], Xind, Yind))&&(El!= null))
                {
                    El.toBeDeleted = true;
                    Xplus += Xind;
                    Yplus += Yind;
                    theReturnState = true;
                }
            //if (theReturnState) score+=1;
            return theReturnState;
        }

        //checks all the elements in one direction
        //i,j parameters set the direction 
        private bool CheckStraight(ElementClass El, int i, int j)
        {
            if (((El.Loc.X + i) >= row) || ((El.Loc.Y + j) >= column)
                       || ((El.Loc.X + i) < 0) || ((El.Loc.Y + j) < 0))
                return false;
            if ((Board[El.Loc.X+i, El.Loc.Y+j]!=null)&&(IsSameStone(El,Board[El.Loc.X + i, El.Loc.Y + j])))
            {
                El.toBeDeleted = true;
                Board[El.Loc.X + i, El.Loc.Y + j].toBeDeleted = true;
                return true;
            }
            return false;
        }

        private bool IsSameStone(ElementClass El1, ElementClass El2)
        {
            if ((El1!= null) && (El1.elType == El2.elType))
                return true;
            else return false;
        }

        private void DropBoardElements()
        {
            for(int j = 0;j<column;++j)
            {
                int throwInd = row-1;
                for(int i=row-1;i>=0;--i)
                {
                    if ((Board[i, j] != null) && (Board[i, j].toBeDeleted)) //deleted
                    {
                        Board[i, j] = null;
                    }
                    else if ((Board[i,j]!=null) && (i==throwInd)) //dont move
                    {
                        throwInd--;
                    }
                    else if(Board[i,j]!=null)   //move down
                    {
                        Board[throwInd, j] = Board[i, j];
                        Board[throwInd, j].Loc = new Point(throwInd, j);
                        ImageBoard[Board[throwInd, j].Loc].Source = ChosePicture(Board[throwInd, j].elType);
                        Board[i, j] = null;
                        throwInd--;
                    }
                }

                for(int i = throwInd;i>=0;--i)
                {
                    ImageBoard[new Point(i, j)].Source = ChosePicture(Stone.none);
                    Board[i, j] = null;
                }
            }
        }


        //return the element picture
        private ImageSource ChosePicture(Stone St)
        {
            switch (St)
            {
                case Stone.Ruby:
                    {
                        return Ruby.Source;
                    }
                case Stone.Amber:
                    {
                        return Amber.Source;
                    }
                case Stone.Citrine:
                    {
                        return Citrine.Source;
                    }
                case Stone.Emerald:
                    {
                        return Emerald.Source;
                    }
                case Stone.Sapphire:
                    {
                        return Sapphire.Source;
                    }
                case Stone.Amethyst:
                    {
                        return Amethyst.Source;
                    }
            }
            return none.Source;
        } 
        #endregion


        #region Buttons
        private void generateButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateStack();
            Timer.Start();
        }

        private void dropButton_Click(object sender, RoutedEventArgs e)
        {
            if(!IsDoneFalling())
            StackFalling();
        }

        private void moveLeft_Click(object sender, RoutedEventArgs e)
        {
            if (CanBeMovedLeft())
                MoveStack(true);
        }

        private void moveRight_Click(object sender, RoutedEventArgs e)
        {
            if (CanBeMovedRight())
                MoveStack(false);
        }

        private void change_Click(object sender, RoutedEventArgs e)
        {
            ChangePositions();
        }

        #endregion

        private void checkDel_Click(object sender, RoutedEventArgs e)
        {
            CheckItemsToBeDeleted();
            DropBoardElements();
        }

        bool stopped = false;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.A)
            {
                if (CanBeMovedLeft())
                    MoveStack(true);
            }
            if(e.Key == Key.D)
            {
                if (CanBeMovedRight())
                    MoveStack(false);
            }
            if(e.Key == Key.S)
            {
                if (!IsDoneFalling())
                    StackFalling();
            }
            if(e.Key == Key.J)
            {
                ChangePositions();
            }
            if(e.Key == Key.Enter)
            {
                if (!stopped)
                {
                    stopped = true;
                    Timer.IsEnabled = false;
                }
                else
                {
                    stopped = false;
                    Timer.IsEnabled = true;
                }
            }
        }
    }
}
