using NUnit.Framework;

namespace GameOfLife.Tests
{
    /// <summary>
    /// 3. Single-Cell-Tests: jede der vier Regeln isoliert, inkl. Grenzwerte,
    /// sowie die Nachbarzählung, auf der alle Regeln aufbauen.
    /// </summary>
    [TestFixture]
    public class CellRuleTests
    {
        // Regel 1: Unterbevölkerung – lebende Zelle mit < 2 Nachbarn stirbt
        [TestCase(0)]
        [TestCase(1)]
        public void LiveCell_WithFewerThanTwoNeighbours_Dies(int neighbours)
        {
            Assert.That(GameRules.NextState(true, neighbours), Is.False);
        }

        // Regel 2: Überleben – lebende Zelle mit 2 oder 3 Nachbarn lebt weiter
        [TestCase(2)]
        [TestCase(3)]
        public void LiveCell_WithTwoOrThreeNeighbours_Survives(int neighbours)
        {
            Assert.That(GameRules.NextState(true, neighbours), Is.True);
        }

        // Regel 3: Überbevölkerung – lebende Zelle mit > 3 Nachbarn stirbt
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public void LiveCell_WithMoreThanThreeNeighbours_Dies(int neighbours)
        {
            Assert.That(GameRules.NextState(true, neighbours), Is.False);
        }

        // Regel 4: Reproduktion – tote Zelle mit genau 3 Nachbarn wird lebendig
        [Test]
        public void DeadCell_WithExactlyThreeNeighbours_BecomesAlive()
        {
            Assert.That(GameRules.NextState(false, 3), Is.True);
        }

        // Gegenprobe zu Regel 4: sonst bleibt eine tote Zelle tot
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public void DeadCell_WithoutExactlyThreeNeighbours_StaysDead(int neighbours)
        {
            Assert.That(GameRules.NextState(false, neighbours), Is.False);
        }

        // --- Zellzustand am Board ---

        [Test]
        public void NewBoard_AllCellsAreDead()
        {
            var board = new Board(4, 3);

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 3; y++)
                    Assert.That(board.IsAlive(x, y), Is.False, $"Zelle ({x},{y})");
        }

        [Test]
        public void SetAlive_ChangesOnlyTheTargetCell()
        {
            var board = new Board(3, 3);

            board.SetAlive(2, 1, true);

            Assert.That(board.IsAlive(2, 1), Is.True);
            Assert.That(board.IsAlive(1, 2), Is.False, "x/y dürfen nicht vertauscht sein");
            Assert.That(board.CountLiveNeighbours(1, 1), Is.EqualTo(1));
        }

        [Test]
        public void SetAlive_False_KillsCell()
        {
            var board = new Board(3, 3);
            board.SetAlive(1, 1, true);

            board.SetAlive(1, 1, false);

            Assert.That(board.IsAlive(1, 1), Is.False);
        }

        // --- Nachbarzählung ---

        [Test]
        public void CountLiveNeighbours_CountsAllEightDirections()
        {
            var board = BoardTestHelper.FromPattern(
                "XXX",
                "XXX",
                "XXX");

            Assert.That(board.CountLiveNeighbours(1, 1), Is.EqualTo(8));
        }

        [Test]
        public void CountLiveNeighbours_DoesNotCountCellItself()
        {
            var board = BoardTestHelper.FromPattern(
                "...",
                ".X.",
                "...");

            Assert.That(board.CountLiveNeighbours(1, 1), Is.EqualTo(0));
        }

        [Test]
        public void CountLiveNeighbours_IgnoresCellsFurtherAway()
        {
            var board = BoardTestHelper.FromPattern(
                "X...X",
                ".....",
                "..X..",
                ".....",
                "X...X");

            Assert.That(board.CountLiveNeighbours(2, 2), Is.EqualTo(0));
        }

        [Test]
        public void CountLiveNeighbours_TreatsCellsOutsideBoardAsDead()
        {
            var board = BoardTestHelper.FromPattern(
                ".X",
                "XX");

            Assert.That(board.CountLiveNeighbours(0, 0), Is.EqualTo(3));
        }

        [Test]
        public void CountLiveNeighbours_DoesNotWrapAroundEdges()
        {
            var board = BoardTestHelper.FromPattern(
                "X..X",
                "....",
                "....",
                "X..X");

            Assert.That(board.CountLiveNeighbours(0, 0), Is.EqualTo(0));
        }
    }
}
