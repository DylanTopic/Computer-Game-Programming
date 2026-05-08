using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceRace.Gameplay;

namespace SpaceRace.UI
{
    public class Hud
    {
        // ---- Tunable scoring constants ----
        public int   BaseScore             { get; set; } = 10000;
        public float TimePenaltyPerSecond  { get; set; } = 10f;
        public int   MissPenalty           { get; set; } = 500;
        public int   CleanRunBonus         { get; set; } = 1000;

        private readonly SpriteFont _font;
        private readonly SpriteBatch _spriteBatch;
        private readonly GraphicsDevice _graphicsDevice;

        public Hud(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _font = font;
        }

        public int ComputeScore(CourseManager course)
        {
            float raw = BaseScore - (course.TotalElapsedSeconds * TimePenaltyPerSecond) - (course.RingsMissed * MissPenalty);
            int score = (int)MathF.Max(0f, raw);
            if (course.RingsMissed == 0 && course.IsComplete)
                score += CleanRunBonus;
            return score;
        }

        public void Draw(CourseManager course, Ship ship)
        {
            _spriteBatch.Begin();

            // Top-left: clock + speed
            string time = FormatTime(course.TotalElapsedSeconds);
            _spriteBatch.DrawString(_font, $"TIME   {time}",            new Vector2(20, 16), Color.White);
            _spriteBatch.DrawString(_font, $"SPEED  {ship.Speed,5:F1} m/s", new Vector2(20, 44), Color.LightGray);

            // Top-right: ring progress
            string ringLine   = $"RING {course.TargetIndex} / {course.Rings.Count}";
            string countsLine = $"PASSED {course.RingsPassed}    MISSED {course.RingsMissed}";

            Vector2 ringSize   = _font.MeasureString(ringLine);
            Vector2 countsSize = _font.MeasureString(countsLine);

            int rightEdge = _graphicsDevice.Viewport.Width - 20;
            _spriteBatch.DrawString(_font, ringLine,   new Vector2(rightEdge - ringSize.X,   16), Color.Cyan);
            _spriteBatch.DrawString(_font, countsLine, new Vector2(rightEdge - countsSize.X, 16 + ringSize.Y + 4),
                course.RingsMissed > 0 ? Color.Salmon : Color.LightGreen);

            if (course.IsComplete)
                DrawCompleteOverlay(course);

            _spriteBatch.End();
        }

        private void DrawCompleteOverlay(CourseManager course)
        {
            int score = ComputeScore(course);

            string title    = "COURSE COMPLETE";
            string scoreStr = $"FINAL SCORE: {score}";
            string detailStr = $"Time {FormatTime(course.TotalElapsedSeconds)}    Passed {course.RingsPassed}    Missed {course.RingsMissed}";
            string hint = "Press R to restart    ESC to exit";

            Vector2 titleSize    = _font.MeasureString(title);
            Vector2 scoreSize    = _font.MeasureString(scoreStr);
            Vector2 detailSize   = _font.MeasureString(detailStr);
            Vector2 hintSize     = _font.MeasureString(hint);

            int cx = _graphicsDevice.Viewport.Width  / 2;
            int cy = _graphicsDevice.Viewport.Height / 2;

            // Semi-shadow effect: draw black offset, then the real color, for legibility on starfield.
            DrawCentered(title,    cx, cy - 60, Color.Yellow);
            DrawCentered(scoreStr, cx, cy - 20, Color.White);
            DrawCentered(detailStr, cx, cy + 16, Color.LightGray);
            DrawCentered(hint,     cx, cy + 56, Color.DimGray);
        }

        private void DrawCentered(string text, int cx, int y, Color color)
        {
            Vector2 size = _font.MeasureString(text);
            Vector2 origin = new Vector2(cx - size.X / 2f, y);
            _spriteBatch.DrawString(_font, text, origin + new Vector2(2, 2), Color.Black); // shadow
            _spriteBatch.DrawString(_font, text, origin, color);
        }

        private static string FormatTime(float seconds)
        {
            int min = (int)(seconds / 60f);
            float secs = seconds - min * 60;
            return $"{min:00}:{secs:00.0}";
        }
    }
}