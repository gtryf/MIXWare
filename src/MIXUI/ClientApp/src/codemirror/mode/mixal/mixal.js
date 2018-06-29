// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

/**
 * Author: George Tryfonas
 */

import CodeMirror from 'codemirror';

CodeMirror.defineMode("mixal", function () {
   const parseWords = (str) => {
        var obj = {}, words = str.split(" ");
        for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
        return obj;
    }

    const pseudos = parseWords("orig equ con alf end");
    const ops = parseWords("nop add sub mul div num char hlt sla sra slax srax slc src " +
                            "move " + 
                            "lda ld1 ld2 ld3 ld4 ld5 ld6 ldx " +
                            "ldan ld1n ld2n ld3n ld4n ld5n ld6n ldxn " +
                            "sta st1 st2 st3 st4 st5 st6 stx stj stz " +
                            "jbus ioc in out jred " +
                            "jmp jsj jov jnov jl je jg jge jne jle " +
                            "jan jaz jap jann janz janp " +
                            "j1n j1z j1p j1nn j1nz j1np " +
                            "j2n j2z j2p j2nn j2nz j2np " +
                            "j3n j3z j3p j3nn j3nz j3np " +
                            "j4n j4z j4p j4nn j4nz j4np " +
                            "j5n j5z j5p j5nn j5nz j5np " +
                            "j6n j6z j6p j6nn j6nz j6np " +
                            "jxn jxz jxp jxnn jxnz jxnp " +
                            "inca deca enta enna " +
                            "inc1 dec1 ent1 enn1 " +
                            "inc2 dec2 ent2 enn2 " +
                            "inc3 dec3 ent3 enn3 " +
                            "inc4 dec4 ent4 enn4 " +
                            "inc5 dec5 ent5 enn5 " +
                            "inc6 dec6 ent6 enn6 " +
                            "incx decx entx ennx " +
                            "cmpa cmp1 cmp2 cmp3 cmp4 cmp5 cmp6 cmpx");
    const operatorChar = /[+\-*/:,()=]/;
    const whiteSpaceChar = /[\t ]/;
    
    const tokenBase = (stream, state) => {
        if (stream.sol() && stream.peek() === '*') {
            stream.skipToEnd();
            return 'comment';
        }

        state.tokenize = loc;
        return state.tokenize(stream, state);
    }

    const loc = (stream, state) => {
        stream.eatWhile(/[0-9A-Za-z]/);
        state.tokenize = op;
        if (/\d[Hh]/.test(stream.current())) {
            return 'def';
        } else {
            return 'identifier';
        }
    }

    const op = (stream, state) => {
        if (stream.eatSpace()) return null;
        state.tokenize = operand;
        
        stream.eatWhile(/[^\t ]/);
        if (stream.eol()) {
            state.tokenize = tokenBase;
        }
        
        const s = stream.current().toLowerCase();
        
        if (pseudos && pseudos.propertyIsEnumerable(s)) {
            if (s === "alf") {
                state.tokenize = alf;
            }
            return "builtin";
        }
        if (ops && ops.propertyIsEnumerable(s)) {
            return "keyword";
        }
        return "keyword error";
    }

    const operand = (stream, state) => {
        if (stream.eatSpace()) return null;
        state.tokenize = parseOperand;
        if (stream.eol()) {
            state.tokenize = tokenBase;
        }
        
        return state.tokenize(stream, state);
    }

    const parseOperand = (stream, state) => {
        let style;

        if (stream.match(/^[0-9A-Za-z]+/)) {
            if (stream.eol()) {
                state.tokenize = tokenBase;
            }
            const s = stream.current();
            if (/^\d+$/.test(s)) {
                style = 'number';
            } else {
                style = 'identifier';
            }
        }
        else if (operatorChar.test(stream.peek())) {
            stream.next();
            if (stream.eol()) {
                state.tokenize = tokenBase;
            }
            style = 'operator';
        }
        else if (whiteSpaceChar.test(stream.peek())) {
            state.tokenize = comment;
        }

        return style;
    }

    const alf = (stream, state) => {
        state.tokenize = comment;
        if (stream.eat(' ')) {
            stream.eat(' ');
            stream.next();
            stream.next();
            stream.next();
            stream.next();
            stream.next();

            if (stream.eol()) {
                state.tokenize = tokenBase;
            }

            return 'string';
        }
        return 'error';
    }

    const comment = (stream, state) => {
        if (stream.eatSpace()) return null;
        
        stream.skipToEnd();
        state.tokenize = tokenBase;
        return 'comment';
    }

    return {
        startState: () => ({
            tokenize: tokenBase,
        }),
        token: (stream, state) => state.tokenize(stream, state),
    };
});

CodeMirror.defineMIME("text/x-mixal", "mixal");