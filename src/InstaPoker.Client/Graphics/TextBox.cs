using System.Numerics;
using InstaPoker.Client.Graphics.Styles;
using SubC.AllegroDotNet;
using SubC.AllegroDotNet.Enums;
using SubC.AllegroDotNet.Models;

namespace InstaPoker.Client.Graphics;

public class TextBox : SceneObject {
    
    public override bool UseMouse => true;
    public override bool UseKeyboard => true;

    public string Placeholder { get; set; } = string.Empty;
    public int MaxCharacters { get; set; }
    public TextboxKeyboard Keyboard { get; set; } = TextboxKeyboard.Any;
    public TextBoxStyle Style { get; set; }
    public VerticalAlign VerticalFontAlignment { get; set; }
    public HorizontalAlign HorizontalFontAlignment { get; set; }

    public event Action<string>? TextChanged;

    // text edition
    private const int BlockSize = 32;
    private char[] buffer = [];
    private string? cachedString;
    private int charCount;
    private int cursor;

    // handling
    private bool isHovering;
    private bool isPressed;
    private bool isSelected;


    public override void Initialize() {
        if (MaxCharacters > 0) {
            // pre allocate all space
            buffer = new char[MaxCharacters];
        }
        else {
            // allocate one block
            buffer = new char[BlockSize];
        }

        Clip = true;
        base.Initialize();
    }

    public override void Render(RenderContext ctx) {
        ctx.UpdateTransform();

        AllegroColor background = isPressed ? Style.BackgroundPressed :
            isHovering ? Style.BackgroundHover : Style.Background;

        // background

        Al.DrawFilledRectangle(0, 0, Size.X, Size.Y, background);

        AllegroFont font = FontManager.GetFont("ShareTech-Regular", Style.FontSize);
        AllegroColor color = charCount == 0 ? Style.PlaceholderForeground : Style.Foreground;
        string text = charCount == 0 && isSelected ? string.Empty : charCount == 0 ? Placeholder : cachedString!;

        int textWidth = Al.GetTextWidth(font, text);
        const float margin = 5;
        float xPosition = HorizontalFontAlignment switch {
            HorizontalAlign.Left => margin,
            HorizontalAlign.Center => Size.X * 0.5f - textWidth * 0.5f,
            HorizontalAlign.Right => Size.X - textWidth - margin,
            _ => throw new Exception("Unknown Horizontal Alignment")
        };
        float yPosition = VerticalFontAlignment switch {
            VerticalAlign.Top => margin,
            VerticalAlign.Center => Size.Y * 0.5f - Al.GetFontAscent(font) * 0.5f,
            VerticalAlign.Bottom => Size.Y - Al.GetFontAscent(font) - margin,
            _ => throw new Exception("Unknown Vertical Alignment")
        };

        if ((charCount == 0 && !isSelected) || charCount > 0) {
            Al.DrawText(font, color, (int)xPosition, (int)yPosition,
                FontAlignFlags.Left, text);
        }

        const double blinkPeriod = 0.75;
        // draw cursor
        if ((isSelected || (charCount > 0 && isSelected)) && Al.GetTime() % (blinkPeriod * 2) < blinkPeriod) {
            const float cursorThickness = 2;
            if (charCount == 0) {
                Al.DrawLine(xPosition, yPosition, xPosition, yPosition + Al.GetFontLineHeight(font),
                    Style.Foreground, cursorThickness);
            }
            else {
                int w = Al.GetTextWidth(font, text[..cursor]);
                float x = xPosition + w;
                Al.DrawLine(x, yPosition, x, yPosition + Al.GetFontLineHeight(font),
                    Style.Foreground, cursorThickness);
            }
        }

        // border
        if (Style.BorderSize > 0) {
            float borderhalf = Style.BorderSize * 0.5f;
            Al.DrawRectangle(borderhalf, borderhalf, Size.X - borderhalf, Size.Y - borderhalf,
                Style.BorderColor, Style.BorderSize);
        }

    }

    public string GetString() {
        if (cachedString is null) {
            BuildString();
        }

        return cachedString!;
    }

    public void SetString(string value) {
        cachedString = value;
        buffer = new char[value.Length];
        for (int i = 0; i < value.Length; i++) {
            buffer[i] = value[i];
        }

        charCount = value.Length;
        cursor = charCount;
        TextChanged?.Invoke(cachedString);
    }

    private void IncreaseBuffer() {
        char[] newBuffer = new char[buffer.Length + BlockSize];
        buffer.CopyTo(newBuffer);
        buffer = newBuffer;
    }

    public override void OnMouseMove(Vector2 pos, Vector2 delta) {
        isHovering = pos.X >= 0
                     && pos.X <= Size.X
                     && pos.Y >= 0
                     && pos.Y <= Size.Y;
        if (isPressed && !isHovering) {
            isPressed = false;
        }
    }

    public override void OnMouseDown(MouseButton button) {
        if (button != MouseButton.Left) {
            return;
        }

        if (isHovering) {
            isPressed = true;
            isSelected = true;
        }
        else {
            isSelected = false;
        }
    }

    public override void OnMouseUp(MouseButton button) {
        if (button != MouseButton.Left) {
            return;
        }

        isPressed = false;
    }

    private bool IsKeyValid(char key) {
        switch (Keyboard) {
            case TextboxKeyboard.Numeric:
                return key is >= '0' and <= '9';
            case TextboxKeyboard.AlphaNumeric:
                return key is >= '0' and <= '9' or >= 'a' and <= 'z' or >= 'A' and <= 'Z';
            case TextboxKeyboard.Any:
                return true;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void OnKeyDown(KeyCode key, KeyModifiers modifiers) {
        if (!isSelected) return;
        if (key == KeyCode.KeyLeft && cursor > 0) {
            cursor--;
        }
        else if (key == KeyCode.KeyRight && cursor < charCount) {
            cursor++;
        }
        else if (key == KeyCode.KeyEnd) {
            cursor = charCount;
        }
        else if (key == KeyCode.KeyHome) {
            cursor = 0;
        }
    }

    private void Backspace() {
        if (!isSelected) return;
        if (charCount <= 0) return;
        if (cursor == 0) return;
        if (cursor == charCount) {
            // remove from end
            cursor--;
            charCount--;
        }
        else {
            // remove from middle
            for (int i = cursor; i < charCount; i++) {
                buffer[i - 1] = buffer[i];
            }

            cursor--;
            charCount--;
        }

        BuildString();
        TextChanged?.Invoke(cachedString!);
    }

    private void Delete() {
        if (!isSelected) return;
        if (charCount <= 0) return; // no chars
        if (cursor == charCount) return; // end
        for (int i = cursor + 1; i < charCount; i++) {
            buffer[i - 1] = buffer[i];
        }

        charCount--;
        BuildString();
        TextChanged?.Invoke(cachedString!);
    }

    public override void OnCharDown(char character) {
        if (!isSelected) {
            return;
        }

        if (character == '\b') {
            Backspace();
            return;
        }

        if ((int)character == 127) {
            Delete();
            return;
        }

        if (!IsKeyValid(character)) {
            return;
        }

        if (MaxCharacters > 0 && charCount >= MaxCharacters) {
            return;
        }

        if (buffer.Length == charCount) {
            IncreaseBuffer();
        }

        if (cursor == charCount) {
            // append
            buffer[charCount] = character;
        }
        else {
            // insert in middle
            for (int i = charCount; i > cursor; i--) {
                buffer[i] = buffer[i - 1];
            }

            buffer[cursor] = character;
        }

        charCount++;
        cursor++;
        BuildString();
        TextChanged?.Invoke(cachedString!);
    }

    private void BuildString() {
        cachedString = new string(buffer, 0, charCount);
    }
}

public enum TextboxKeyboard {
    Numeric,
    AlphaNumeric,
    Any
}