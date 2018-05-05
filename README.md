# A Simulator for Knuth's MIX Computer

## Introduction and Background

The MIX computer is an imaginary computer invented by Don Knuth in the 1960s in order to present his exposition on computer algorithms. As Knuth puts it, the use of an imaginary computer and machine language helps avoid distracting the reader with the technicalities of one particular computer system, and the focus remains on truths that have always been-and will always be-valid, independent of any kind of technological evolution or current trends.

There is no doubt about the truth of this statement. However, another kind of problem presents itself now. The MIX computer is, well, ...imaginary. A reader cannot experiment with it, or even have a go at solving the exercises by sitting in front of a real computer terminal and writing programs. Nor even can one be certain that a particular solution to an exercise is correct, unless one checks the answer to the exercise, and even then, there may be many different solutions to a problem, but the answers present only one. Unless of course the reader is inclined to simulate MIX in his or her head, or on a piece of paper. And these may very well have been Knuth's intentions for his readers. After all, there is no doubt that the reader that will get the most out of the books is the one who is patient and diligent enough to go through this kind of process. But it still is a daunting task, often too frustrating, and for many people in many ways, a distraction of the kind that Knuth wanted for his readers to avoid in the first place.

This is exactly the problem that the MIX simulator attempts to solve: a tool suite, capable of assembling and executing programs written in the MIXAL language, the MIX computer's assembly language. Features include a simplistic debugger, a complete implementation of the MIX computer's devices, symbol table generation, listing generation and typesetting for TeX, etc.

The full specification of the MIX computer and the MIXAL assembly language can be found in The Art of Computer Programming vol. 1. In fact, this software package has little use to anybody who has not read, or has no intention to read, this series of books, follow its examples, and solve its exercises. So, have fun using it, but first of all, enjoy reading the books!

## Building

Load the solution in Visual Studio and build.

## Using the Tools

The **MIXWare** distribution contains the complete documentation in the form of a *.pdf* file. In it, you may find the detailed instructions on how to execute the assembler and make the most out of the simulator. However, a (very) brief tutorial follows.

To assemble a MIXAL program:

```sh
MIXASM -f:card -o:primes.deck primes.mixal
```

This command assembles the file "*primes.mixal*" as a punched card deck and saves the output to "*primes.deck*".

The general syntax for MIXASM is:

```sh
MIXASM [arguments] [input-file]
```

where the optional `input-file` parameter is the name of a text file containing MIXAL code (if unspecified, input comes from the keyboard), and the `arguments` govern the compilation process. In particular, the `--output` (or `-o` for short) argument specifies the compiled output filename (if unspecified, it is sent to the standard output), for example *--output:primes.deck* sends the output to the file *primes.deck*. The `--format` (or `-f` for short) argument specifies whether the compiled output will be in the form of a punched card deck or a serialized binary file. For instance, `-f:card` specifies that the output is a card deck, while `-f:binary` specifies that the output is a binary serialized file. `--symtab` (or `-s` for short) outputs the program's symbol table, either to the standard output or to a specified file (works just like the `--output` argument). Similarly, `--list-file` (or `-lf` for short) produces a program listing. In this case, the filename is mandatory. In the case that you requested a listing or a symbol table, you may additionally specify `--pretty-print` (or `-pp` for short) to request that the output be a TeX file so that you may run it through TeX to be typeset nicely.

To start the simulator:

```sh
MIX --deck primes.deck or MIX --binary primes.bin
```

This command starts the simulator and loads the card deck found in the file "*primes.deck*" or "*primes.bin*", depending on whether the source has been compiled to a card deck or to a binary serialized file.

As soon as the simulator starts, it displays a prompt for you to input your commands. At this point, you may press `?` to get help on additional commands, the most important being "`run`", which begins execution of the program until it halts; "`step`", which executes only the next command pointed to by the Program Counter; and of course, "`quit`", if you've had enough of this! There exist a host of commands, all of which begin with "`show`", that echo the MIX computer's current state, as well as another set of commands, beginning with "`set`", that alter that state. Some examples are:

| Command | Description |
| ------- | ----------- |
| `show memory` | shows the contents of MIX's memory. You may specify a range, such as in `show memory 3000 3010` to show only 11 words. If you specify a range, you may also request a disassembly, such as in `show memory 3000 3010 with disassembly`. |
| `show state` | shows MIX's internal state, that is, the values of all the registers, flags, and the Program Counter. |
| `set <reg> <value>` | sets the value of a particular MIX register, for example, `set rA 100`. |

Chapters 1 and 2 of the documentation describe MIXASM and MIX in detail, including the simulator's command set and a full description of the assembler's command line arguments.

## Example Programs

The repository's `Examples` directory contains a few example MIXAL files to get you started. They have all been copied from The Art of Computer Programming book.