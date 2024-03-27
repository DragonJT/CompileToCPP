

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

    static IExpression ParseSubExpression(List<Token> tokens){
        var operators = new string[][]{ ["="], ["&&", "||"], [">=", "<=", "==", "!="],
            ["<", ">"], ["+", "-"], ["/", "*"], ["."]};
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
            if(tokens[0].type == TokenType.Varname && tokens[1].type == TokenType.Parens){
                return new Call(tokens[0], ParseArgs(tokens[1]));
            }
            else if(tokens[0].type == TokenType.Varname && tokens[1].type == TokenType.Square){
                return new Indexor(tokens[0], ParseSubExpression(Tokenizer.Tokenize(tokens[1].value)));
            }
        }
        else if(tokens.Count == 3){
            if(tokens[0].type == TokenType.New && tokens[1].type == TokenType.Varname && tokens[2].type == TokenType.Parens){
                return new New(tokens[1], ParseArgs(tokens[2]));
            }
        }
        foreach(var ops in operators){
            var index = tokens.FindLastIndex(t=>t.type == TokenType.Operator && ops.Contains(t.value));
            if(index>=0){
                var left = ParseSubExpression(tokens.GetRange(0, index));
                var right = ParseSubExpression(tokens.GetRange(index+1, tokens.Count - (index+1)));
                return new BinaryOp(left, right, tokens[index]);
            }
        }
        if(tokens[0].type == TokenType.Minus){
            return new UnaryOp(ParseSubExpression(tokens.GetRange(1, tokens.Count-1)), tokens[0]);
        }
        if(tokens[0].type == TokenType.Operator && tokens[0].value == "!"){
            return new UnaryOp(ParseSubExpression(tokens.GetRange(1, tokens.Count-1)), tokens[0]);
        }
        throw new Exception("Unexpected tokens");
    }

    static IExpression ParseExpression(List<Token> tokens){
        var subtractsSurroundingTokens = new List<TokenType>{TokenType.Varname, TokenType.Int, TokenType.Float};
        for(var i=1;i<tokens.Count-1;i++){
            bool isSubtract = tokens[i].type == TokenType.Minus 
                && subtractsSurroundingTokens.Contains(tokens[i-1].type) 
                && subtractsSurroundingTokens.Contains(tokens[i+1].type);
            if(isSubtract){
                tokens[i].type = TokenType.Operator;
            }
        }
        return ParseSubExpression(tokens);
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

    static Variable[] ParseParameters(string code){
        var parameterTokens = SplitByComma(Tokenizer.Tokenize(code));
        return parameterTokens.Select(t=>new Variable(t[0], t[1])).ToArray();
    }

    public static Body ParseBody(string code){
        var statementTokens = SplitIntoGroups(Tokenizer.Tokenize(code));
        List<IStatement> statements = [];
        for(var i=0;i<statementTokens.Count;i++){
            var s = statementTokens[i];
            if(s[0].type == TokenType.Import){
                var returnType = s[1];
                var name = s[2];
                var parameters = ParseParameters(s[3].value);
                statements.Add(new ImportFunction(returnType, name, parameters, s[4]));
            }
            else if(s.Count == 4 && s[3].type == TokenType.Curly){
                var returnType = s[0];
                var name = s[1];
                var parameters = ParseParameters(s[2].value);
                statements.Add(new Function(returnType, name, parameters, ParseBody(s[3].value)));
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
}
