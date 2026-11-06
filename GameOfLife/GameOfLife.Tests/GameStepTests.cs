using NUnit.Framework;

namespace GameOfLife.Tests
{
    /// <summary>
    /// 1. Spielzug-Tests: Board.Step() wendet die vier Regeln auf das ganze Feld an,
    /// und zwar gleichzeitig auf Basis des alten Zustands.
    /// </summary>
    [TestFixture]
    public class GameStepTests
    {
        // --- Jede Regel im Kontext eines echten Spielzugs ---

        [Test]
        public void Step_Rule1_IsolatedCellsDieOfUnderpopulation()
        {
            var board = BoardTestHelper.FromPattern(
                ".....",
                ".X...",
                ".....",
                "...X.",
                "...X.");

            board.Step();

            BoardTestHelper.AssertMatches(board,
                ".....",
                ".....",
                ".....",
                ".....",
                ".....");
        }

        [Test]
        public void Step_Rule2_CellsWithTwoOrThreeNeighboursSurvive()
        {
            // Block: jede Zelle hat genau 3 Nachbarn
            var board = BoardTestHelper.FromPattern(
                "....",
                ".XX.",
                ".XX.",
                "....");

            board.Step();

            BoardTestHelper.AssertMatches(board,
                "....",
                ".XX.",
                ".XX.",
                "....");
        }

        [Test]
        public void Step_Rule3_CellWithMoreThanThreeNeighboursDies()
        {
            // Mitte hat 4 Nachbarn -> stirbt
            var board = BoardTestHelper.FromPattern(
                ".....",
                "..X..",
                ".XXX.",
                "..X..",
                ".....");

            board.Step();

            Assert.That(board.IsAlive(2, 2), Is.False);
        }

        [Test]
        public void Step_Rule4_DeadCellWithThreeNeighboursIsBorn()
        {
            var board = BoardTestHelper.FromPattern(
                "XX.",
                "X..",
                "...");

            board.Step();

            BoardTestHelper.AssertMatches(board,
                "XX.",
                "XX.",
                "...");
        }

        // --- Gesamtverhalten über bekannte Muster ---

        [Test]
        public void Step_EmptyBoard_StaysEmpty()
        {
            var board = new Board(5, 5);

            board.Step();

            BoardTestHelper.AssertMatches(board,
                ".....",
                ".....",
                ".....",
                ".....",
                ".....");
        }

        [Test]
        public void Step_Blinker_OscillatesBetweenHorizontalAndVertical()
        {
            var board = BoardTestHelper.FromPattern(
                ".....",
                ".....",
                ".XXX.",
                ".....",
                ".....");

            board.Step();
            BoardTestHelper.AssertMatches(board,
                ".....",
                "..X..",
                "..X..",
                "..X..",
                ".....");

            board.Step();
            BoardTestHelper.AssertMatches(board,
                ".....",
                ".....",
                ".XXX.",
                ".....",
                ".....");
        }

        [Test]
        public void Step_UpdatesAllCellsSimultaneously()
        {
            // Würde Step() Zellen der Reihe nach "in place" ändern, entstünde
            // beim Blinker kein sauberes senkrechtes Muster. Hier zusätzlich
            // explizit: die Endzellen sterben, obwohl ihre Nachbarn im selben
            // Zug neu geboren werden.
            var board = BoardTestHelper.FromPattern(
                "...",
                "XXX",
                "...");

            board.Step();

            Assert.That(board.IsAlive(0, 1), Is.False, "linkes Ende");
            Assert.That(board.IsAlive(2, 1), Is.False, "rechtes Ende");
            Assert.That(board.IsAlive(1, 0), Is.True, "oben geboren");
            Assert.That(board.IsAlive(1, 2), Is.True, "unten geboren");
            Assert.That(board.IsAlive(1, 1), Is.True, "Mitte überlebt");
        }

        [Test]
        public void Step_Glider_MovesOneCellDiagonallyAfterFourGenerations()
        {
            var board = BoardTestHelper.FromPattern(
                ".X....",
                "..X...",
                "XXX...",
                "......",
                "......",
                "......");

            for (int i = 0; i < 4; i++)
                board.Step();

            BoardTestHelper.AssertMatches(board,
                "......",
                "..X...",
                "...X..",
                ".XXX..",
                "......",
                "......");
        }

        [Test]
        public void Step_DoesNotChangeBoardDimensions()
        {
            var board = new Board(7, 4);

            board.Step();

            Assert.That(board.Width, Is.EqualTo(7));
            Assert.That(board.Height, Is.EqualTo(4));
        }

        [Test]
        public void Step_CellsAtEdge_DoNotWrapAround()
        {
            // Bei Wrap-around hätte (0,1) links-außen 3 Nachbarn und würde geboren
            var board = BoardTestHelper.FromPattern(
                "...X",
                "...X",
                "...X");

            board.Step();

            Assert.That(board.IsAlive(0, 1), Is.False);
        }
    }
}
