// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

/**
 * Author: George Tryfonas
 */

import CodeMirror from 'codemirror';

CodeMirror.defineMode("mixal", function () {
   function makeKeywords(str) {
        var obj = {}, words = str.split(" ");
        for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
        return obj;
    }

    const pseudos = makeKeywords("ORIG EQU CON ALF END");
    const tests = {
        local: (input) => /[0-9]H/.test(input),
        symbol: (input) => /[0-9A-Z]{1,10}/.test(input),
        number: (input) => /\d{1,10}/.test(input),
        keyword: (list, input) => list[input]
    };

    return {
        startState: function () {
            return {
                inString: false,
                segment: 0, // 0 = LOC, 1 = OP, 2 = OPERAND
            }
        },
        token: function (stream, state) {
            if (stream.column() === 0 && stream.peek() === "*") {
                stream.skipToEnd();
                state.segment = 0;
                return 'comment';
            }

            switch (state.segment) {
                case 0:
                    stream.eatWhile(ch => ch !== ' ' && ch !== '\t' && stream.column() < 10);
                    stream.eatSpace();
                    state.segment = 1;
                    if (tests.local(stream.current().toUpperCase()))
                        return 'def';
                    return 'identifier';
                case 1:
                    stream.eatWhile(ch => ch !== ' ' && ch !== '\t');
                    stream.eatSpace();
                    state.segment = 2;
                    if (tests.keyword(pseudos, stream.current().toUpperCase()))
                        return 'builtin';
                    return 'keyword';
                case 2:
                    if (stream.eat('/')) {
                        return 'operator'
                    } else if (stream.eat('+')) {
                        return 'operator'
                    } else if (stream.eat('-')) {
                        return 'operator'
                    } else if (stream.eat('*')) {
                        return 'operator'
                    } else if (stream.eat(/\d/)) {
                        return 'number';
                    } else if (stream.eat(/[a-zA-Z]/)) {
                        return 'identifier';
                    } else if (stream.eatSpace()) {
                        state.segment = 3;
                        return null;
                    }

                    stream.eat(/./);
                    return null;
                case 3:
                    stream.skipToEnd();
                    state.segment = 0;
                    return 'comment';
                default:
                    stream.skipToEnd();
                    state.segment = 0;
                    return null;
            }
        }
    };
});

CodeMirror.defineMIME("text/x-mixal", "mixal");