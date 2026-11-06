using NUnit.Framework;

namespace GameOfLife.Tests
{
    /// <summary>
    /// Hilfsfunktionen, um Boards lesbar als Text-Muster aufzubauen und zu prüfen.
    /// 'X' = lebende Zelle, '.' = tote Zelle. Zeile = y, Spalte = x.
    /// </summary>
    internal static class BoardTestHelper
    {
        public static Board FromPattern(params string[] rows)
        {
            int height = rows.Length;
            int width = rows[0].Length;
            var board = new Board(width, height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    board.SetAlive(x, y, rows[y][x] == 'X');

            return board;
        }

        public static void AssertMatches(Board board, params string[] expectedRows)
        {
            Assert.That(board.Height, Is.EqualTo(expectedRows.Length), "Höhe stimmt nicht");
            Assert.That(board.Width, Is.EqualTo(expectedRows[0].Length), "Breite stimmt nicht");

            for (int y = 0; y < board.Height; y++)
                for (int x = 0; x < board.Width; x++)
                {
                    bool expected = expectedRows[y][x] == 'X';
                    Assert.That(board.IsAlive(x, y), Is.EqualTo(expected),
                        $"Zelle ({x},{y}) sollte {(expected ? "leben" : "tot sein")}");
                }
        }
    }
}
