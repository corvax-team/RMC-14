using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Robust.Shared.Console;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Syntax;
using Robust.Shared.Toolshed.TypeParsers;
using Robust.Shared.Toolshed.TypeParsers.Math;

namespace Content.Shared._CCM.Toolshed;

public sealed class Int32TypeParser : TypeParser<int>
{
    public override bool TryParse(ParserContext ctx, [NotNullWhen(true)] out int result)
    {
        result = default;
        var maybeNumber = ctx.GetWord(ParserContext.IsNumeric);
        if (string.IsNullOrEmpty(maybeNumber))
        {
            ctx.Error = new ExpectedNumericError();
            return false;
        }

        if (int.TryParse(maybeNumber, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            return true;

        ctx.Error = new InvalidNumber<int>(maybeNumber);
        return false;
    }

    public override CompletionResult? TryAutocomplete(ParserContext parserContext, CommandArgument? arg)
    {
        return CompletionResult.FromHint(GetArgHint(arg));
    }
}
