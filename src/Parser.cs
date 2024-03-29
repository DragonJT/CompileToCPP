

using System.Text.Json.Nodes;

static class Parser{
    static List<List<Token>> SplitByComma(List<Token> tokens){
        List<List<Token>> output = [];
        var start = 0;
        for(var i=0;i<tokens.Count;i++){
            if(tokens[i].type == TokenType.Comma){
                output.Add(tokens.GetRange(start, i-start));
                start = i+1;
            }
        }
        if(start<tokens.Count){
            output.Add(tokens.GetRange(start, tokens.Count-start));
        }
        return output;
    }

    static IExpression ParseExpressionInParens(Token token){
        return ParseExpression(Tokenizer.Tokenize(token.value));
    }

    static IExpression[] ParseArgs(Token token){
        return SplitByComma(Tokenizer.Tokenize(token.value)).Select(ParseExpression).ToArray();
    }

    static bool CheckIfNextTokensAreValidForBinaryOp(List<Token> tokens, int index, string[] prefixOps){
        for(var i=index+1;i<tokens.Count;i++){
            var t = tokens[i];
            if(t.type == TokenType.Operator){
                if(prefixOps.Contains(t.value)){
                    continue;
                }
                else{
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    static bool CheckIfPrevTokensAreValidForBinaryOp(List<Token> tokens, int index, string[] postfixOps){
        for(var i=index-1;i>=0;i--){
            var t = tokens[i];
            if(t.type == TokenType.Operator){
                if(postfixOps.Contains(t.value)){
                    continue;
                }
                else{
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    static int FindBinaryOp(List<Token> tokens, string[] ops, string[] prefixOps, string[] postfixOps){
        for(var i=tokens.Count-1;i>=0;i--){
            if(tokens[i].type == TokenType.Operator && ops.Contains(tokens[i].value)){
                if(CheckIfPrevTokensAreValidForBinaryOp(tokens, i, postfixOps) && CheckIfNextTokensAreValidForBinaryOp(tokens, i, prefixOps)){
                    return i;
                }
            }
        }
        return -1;
    }

    static IExpression ParseExpression(List<Token> tokens){
        string[][] binaryOps = [["=", "+=", "-=", "*=", "/="], ["&&", "||"], [">=", "<=", "==", "!="],
            ["<", ">"], ["+", "-"], ["/", "*"]];
        string[] postfixOps = ["++", "--"];
        string[] prefixOps = ["!", "+", "-", "*", "&", "++", "--"];

        if(tokens.Count == 0){
            throw new Exception("No tokens");
        }
        else if(tokens.Count == 1){
            if(tokens[0].type == TokenType.Parens){
                return ParseExpressionInParens(tokens[0]);
            }
            else if(tokens[0].type == TokenType.Float){
                return new Literal(LiteralType.Float, tokens[0]);
            }
            else if(tokens[0].type == TokenType.Int){
                return new Literal(LiteralType.Int, tokens[0]);
            }
            else if(tokens[0].type == TokenType.DoubleQuote){
                return new Literal(LiteralType.String, tokens[0]);
            }
            else if(tokens[0].type == TokenType.SingleQuote){
                return new Literal(LiteralType.Char, tokens[0]);
            }
            else if(tokens[0].type == TokenType.Varname){
                return new Literal(LiteralType.Varname, tokens[0]);
            }
            else if(tokens[0].type == TokenType.True){
                return new Literal(LiteralType.True, tokens[0]);
            }
            else if(tokens[0].type == TokenType.False){
                return new Literal(LiteralType.False, tokens[0]);
            }
            else{
                throw new Exception("Unexpected token: "+tokens[0]);
            }
        }
        else if(tokens.Count == 2){
            if(tokens[0].type == TokenType.Varname && tokens[1].type == TokenType.Varname){
                return new Var(tokens[0], tokens[1]);
            }
            else if(tokens[0].type == TokenType.Varname && tokens[1].type == TokenType.Parens){
                return new Call(tokens[0], ParseArgs(tokens[1]));
            }
            else if(tokens[0].type == TokenType.Varname && tokens[1].type == TokenType.Square){
                return new Indexor(tokens[0], ParseExpression(Tokenizer.Tokenize(tokens[1].value)));
            }
        }
        foreach(var ops in binaryOps){
            var index = FindBinaryOp(tokens, ops, prefixOps, postfixOps);
            if(index >= 0){
                var left = ParseExpression(tokens.GetRange(0, index));
                var right = ParseExpression(tokens.GetRange(index+1, tokens.Count - (index+1)));
                return new BinaryOp(left, right, tokens[index]);
            }
        }
        if(tokens[0].type == TokenType.Operator && prefixOps.Contains(tokens[0].value)){
            return new PrefixUnaryOp(ParseExpression(tokens.GetRange(1, tokens.Count-1)), tokens[0]);
        }
        var lastToken = tokens[^1];
        if(lastToken.type == TokenType.Operator && postfixOps.Contains(lastToken.value)){
            return new PostfixUnaryOp(ParseExpression(tokens.GetRange(0, tokens.Count-1)), lastToken);
        }
        var dotIndex = tokens.FindLastIndex(t=>t.type == TokenType.Dot);
        if(dotIndex>0){
            var left = ParseExpression(tokens.GetRange(0, dotIndex));
            var right = ParseExpression(tokens.GetRange(dotIndex+1, tokens.Count - (dotIndex+1)));
            return new BinaryOp(left, right, tokens[dotIndex]);
        }
        throw new Exception("Unexpected tokens");
    }

    static List<List<Token>> SplitIntoGroups(List<Token> tokens){
        var output = new List<List<Token>>();
        var start = 0;
        for(var i=0;i<tokens.Count;i++){
            if(tokens[i].type == TokenType.SemiColon){
                output.Add(tokens.GetRange(start, i-start));
                start=i+1;
            }
            else if(tokens[i].type == TokenType.Curly){
                output.Add(tokens.GetRange(start, i+1-start));
                start=i+1;
            }
        }
        return output;
    }

    static Body ParseBody(string code){
        var statementTokens = SplitIntoGroups(Tokenizer.Tokenize(code));
        List<IStatement> statements = [];
        foreach(var s in statementTokens){
            if(s.Count == 0){
                continue;
            }
            else if(s[0].type == TokenType.While){
                statements.Add(new While(ParseExpression(Tokenizer.Tokenize(s[1].value)), ParseBody(s[2].value)));
            }
            else if(s[0].type == TokenType.If){
                statements.Add(new If(ParseExpression(Tokenizer.Tokenize(s[1].value)), ParseBody(s[2].value)));
            }
            else if(s[0].type == TokenType.Break){
                statements.Add(new Break());
            }
            else if(s[0].type == TokenType.For){
                var args = SplitByComma(Tokenizer.Tokenize(s[1].value));
                statements.Add(new For(args[0][0], ParseExpression(args[1]), ParseExpression(args[2]), ParseBody(s[2].value)));
            }
            else if(s[0].type == TokenType.Return){
                if(s.Count == 1){
                    statements.Add(new Return(null));
                }
                else{
                    statements.Add(new Return(ParseExpression(s.GetRange(1, s.Count-1))));
                }
            }
            else{
                statements.Add(new Expression(ParseExpression(s)));
            }
        }
        return new Body(statements);
    }

    static Variable[] ParseParameters(string code){
        var parameterTokens = SplitByComma(Tokenizer.Tokenize(code));
        return parameterTokens.Select(t=>new Variable(t[0], t[1])).ToArray();
    }

    public static Root ParseRoot(string code){
        var declarationTokens = SplitIntoGroups(Tokenizer.Tokenize(code));
        List<IDeclaration> declarations = [];
        foreach(var d in declarationTokens){
            if(d.Count == 4 && d[2].type == TokenType.Parens && d[3].type == TokenType.Curly){
                var returnType = d[0];
                var name = d[1];
                var parameters = ParseParameters(d[2].value);
                declarations.Add(new Function(returnType, name, parameters, ParseBody(d[3].value)));
            }
            else{
                var index = d.FindIndex(t=>t.type == TokenType.Operator && t.value == "=");
                if(index >= 0){
                    var left = ParseExpression(d.GetRange(0, index));
                    var right = ParseExpression(d.GetRange(index+1, d.Count - (index+1)));
                    declarations.Add(new Global(left, right));
                }
                else{
                    throw new Exception("Unexpected declaration.");
                }
            }
        }
        return new Root(declarations);
    }    
}
