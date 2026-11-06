using NUnit.Framework;

namespace GameOfLife.Tests
{
    /// <summary>
    /// 2. UI-Tests: Nach einem Spielzug über den GameController muss das
    /// angezeigte Grid (GridView) den neuen Board-Zustand widerspiegeln.
    /// </summary>
    [TestFixture]
    public class GridViewTests
    {
        private const char Alive = 'X';
        private const char Dead = '.';

        private static (Board board, GridView view, GameController controller) CreateBlinkerSetup()
        {
            var board = BoardTestHelper.FromPattern(
                ".....",
                ".....",
                ".XXX.",
                ".....",
                ".....");

            var view = new GridView(Alive, Dead);
            view.Initialize(board);
            var controller = new GameController(view);

            return (board, view, controller);
        }

        [Test]
        public void Initialize_ShowsStartState()
        {
            var (_, view, _) = CreateBlinkerSetup();

            Assert.That(view.Render(), Is.EqualTo(
                ".....\n" +
                ".....\n" +
                ".XXX.\n" +
                ".....\n" +
                "....."));
        }

        [Test]
        public void Initialize_GridHasSameDimensionsAsBoard()
        {
            var board = new Board(6, 3);
            var view = new GridView(Alive, Dead);

            view.Initialize(board);

            string[] rows = view.Render().Split('\n');
            Assert.That(rows.Length, Is.EqualTo(3));
            Assert.That(rows, Has.All.Length.EqualTo(6));
        }

        [Test]
        public void NextGeneration_UpdatesGrid()
        {
            var (_, view, controller) = CreateBlinkerSetup();
            string before = view.Render();

            controller.NextGeneration();

            Assert.That(view.Render(), Is.Not.EqualTo(before));
            Assert.That(view.Render(), Is.EqualTo(
                ".....\n" +
                "..X..\n" +
                "..X..\n" +
                "..X..\n" +
                "....."));
        }

        [Test]
        public void NextGeneration_ChangedCellsShowNewSymbol()
        {
            var (_, view, controller) = CreateBlinkerSetup();

            controller.NextGeneration();

            Assert.That(view.GetCellSymbol(1, 2), Is.EqualTo(Dead), "gestorbene Zelle");
            Assert.That(view.GetCellSymbol(2, 1), Is.EqualTo(Alive), "geborene Zelle");
            Assert.That(view.GetCellSymbol(2, 2), Is.EqualTo(Alive), "überlebende Zelle");
        }

        [Test]
        public void NextGeneration_GridMatchesBoardInEveryCell()
        {
            var (board, view, controller) = CreateBlinkerSetup();

            for (int generation = 1; generation <= 3; generation++)
            {
                controller.NextGeneration();

                for (int x = 0; x < board.Width; x++)
                    for (int y = 0; y < board.Height; y++)
                    {
                        char expected = board.IsAlive(x, y) ? Alive : Dead;
                        Assert.That(view.GetCellSymbol(x, y), Is.EqualTo(expected),
                            $"Generation {generation}, Zelle ({x},{y})");
                    }
            }
        }

        [Test]
        public void NextGeneration_AdvancesBoardExactlyOneStep()
        {
            var (board, _, controller) = CreateBlinkerSetup();

            controller.NextGeneration();

            BoardTestHelper.AssertMatches(board,
                ".....",
                "..X..",
                "..X..",
                "..X..",
                ".....");
        }

        [Test]
        public void NextGeneration_StableBoard_GridStaysTheSame()
        {
            var board = BoardTestHelper.FromPattern(
                "....",
                ".XX.",
                ".XX.",
                "....");
            var view = new GridView(Alive, Dead);
            view.Initialize(board);
            var controller = new GameController(view);
            string before = view.Render();

            controller.NextGeneration();

            Assert.That(view.Render(), Is.EqualTo(before));
        }

        [Test]
        public void Refresh_ReflectsManualBoardChanges()
        {
            var board = new Board(3, 3);
            var view = new GridView(Alive, Dead);
            view.Initialize(board);

            board.SetAlive(1, 1, true);
            view.Refresh();

            Assert.That(view.GetCellSymbol(1, 1), Is.EqualTo(Alive));
        }

        [Test]
        public void Controller_ExposesBoardOfView()
        {
            var (board, _, controller) = CreateBlinkerSetup();

            Assert.That(controller.Board, Is.SameAs(board));
        }
    }
}
