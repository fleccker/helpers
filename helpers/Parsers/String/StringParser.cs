using helpers.Pooling.Pools;
using helpers.Results;

namespace helpers.Parsers.String
{
    public static class StringParser
    {
        public static bool TryParse(string input, out string[] args)
        {
            var result = Parse(input);
            if (!result.IsSuccess)
            {
                args = null;
                return false;
            }

            args = result.Result;
            return true;
        }

        public static IResult<string[]> Parse(string input)
        {
            var results = ListPool<string>.Pool.Get();
            var builder = StringBuilderPool.Pool.Get();

            var endPos = input.Length;
            var lastArgEndPos = int.MinValue;

            var curPart = StringParserPart.None;

            var isEscaping = false;

            var matchQuote = '\0';
            var c = '\0';

            for (int curPos = 0; curPos <= endPos; curPos++)
            {
                if (curPos < endPos) 
                    c = input[curPos];
                else 
                    c = '\0';

                if (isEscaping)
                {
                    if (curPos != endPos)
                    {
                        if (c != matchQuote) 
                            builder.Append('\\');

                        builder.Append(c);
                        isEscaping = false;

                        continue;
                    }
                }

                if (c is '\\')
                {
                    isEscaping = true;
                    continue;
                }

                if (curPart is StringParserPart.None)
                {
                    if (char.IsWhiteSpace(c) || curPos == endPos) 
                        continue;
                    else
                    {
                        if (StringParserUtils.IsOpen(c))
                        {
                            curPart = StringParserPart.QuotedParameter;
                            matchQuote = StringParserUtils.GetMatch(c);
                            continue;
                        }

                        curPart = StringParserPart.Parameter;
                    }
                }

                var argString = "";

                if (curPart is StringParserPart.Parameter)
                {
                    if (curPos == endPos || char.IsWhiteSpace(c))
                    {
                        argString = builder.ToString();
                        lastArgEndPos = curPos;
                    }
                    else 
                        builder.Append(c);
                }
                else if (curPart is StringParserPart.QuotedParameter)
                {
                    if (c == matchQuote)
                    {
                        argString = builder.ToString();
                        lastArgEndPos = curPos + 1;
                    }
                    else 
                        builder.Append(c);
                }

                if (argString != "")
                {
                    results.Add(argString);
                    curPart = StringParserPart.None;
                    builder.Clear();
                }
            }

            if (isEscaping) 
                return new ErrorResult<string[]>("Input text may not end on an incomplete escape.");

            var resultArray = results.ToArray(); 

            ListPool<string>.Pool.Push(results);
            StringBuilderPool.Pool.Push(builder);

            return new SuccessResult<string[]>(resultArray);
        }
    }
}