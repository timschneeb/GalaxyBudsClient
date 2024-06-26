using System.Text;

namespace GalaxyBudsClient.Generators.Utils;

public class CodeGenerator
{
    private const int IndentLength = 4;
    private readonly StringBuilder _sb = new();
    private int _currentIndentCount;

    public void EnterScope(string? header = null, string scopeToken = "{")
    {
        if (header != null)
        {
            AppendLines(header);
        }

        AppendLine(scopeToken);
        IncreaseIndentation();
    }

    public void LeaveScope(string suffix = "", string scopeToken = "}")
    {
        DecreaseIndentation();
        AppendLine($"{scopeToken}{suffix}");
    }

    private void IncreaseIndentation()
    {
        _currentIndentCount++;
    }

    private void DecreaseIndentation()
    {
        if (_currentIndentCount - 1 >= 0)
        {
            _currentIndentCount--;
        }
    }

    public void AppendLine()
    {
        _sb.AppendLine();
    }

    public void AppendLine(string text)
    {
        _sb.Append(' ', IndentLength * _currentIndentCount);
        _sb.AppendLine(text);
    }
    
    public void AppendLines(string text)
    {
        foreach (var line in text.Split('\n'))
        {
            _sb.Append(' ', IndentLength * _currentIndentCount);
            _sb.AppendLine(line);
        }
    }

    public override string ToString()
    {
        return _sb.ToString();
    }
}