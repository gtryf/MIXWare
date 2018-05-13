using System;
using System.Text;
using MIXLib;

namespace MIXAsm
{
    public interface IPrettyPrinter
    {
        string Preamble { get; }
        string PostDocText { get; }
        string EmptyLine { get; }

        string FormatHeading(string headingText);
        string FormatInstruction(int location, MIXWord instruction, int lineNo, string line);
        string FormatPseudo(int lineNo, string line);
        string FormatSymbol(string name, MIXWord value);
    }

    public class PlainTextPrinter
        : IPrettyPrinter
    {
        public string Preamble { get { return string.Empty; } }
        public string PostDocText { get { return string.Empty; } }
        public string EmptyLine { get { return Environment.NewLine; } }

        public string FormatHeading(string headingText)
		    => "============== " + headingText + " ==============";

        public string FormatInstruction(int location, MIXWord instruction, int lineNo, string line)
        {
            string strLine = lineNo.ToString().PadLeft(4) + " " + line;
            string firstPart = string.Format("{0:0000}: {1} ", location, instruction.ToInstructionString());

            strLine = firstPart += strLine;

            return strLine;
        }

        public string FormatPseudo(int lineNo, string line)
		    => new string(' ', 25) + lineNo.ToString().PadLeft(4) + " " + line;

        public string FormatSymbol(string name, MIXWord value)
		    => name + "\t" + value + " = " + value.Value;
    }

    public class TeXPrinter
        : IPrettyPrinter
    {
        private string SourceFile { get; set; }
        public TeXPrinter(string sourceFile)
        {
            SourceFile = sourceFile;
        }

        public string Preamble
        {
            get
            {
                #region TeX Preamble
                return @"\newif\ifusehrule
\newcount\utilmix

\newskip\ttglue
\def\tenpoint{\def\rm{\fam0\tenrm}%
  \textfont0=\tenrm \scriptfont0=\sevenrm \scriptscriptfont0=\fiverm
  \textfont1=\teni \scriptfont1=\seveni \scriptscriptfont1=\fivei
  \textfont2=\tensy \scriptfont2=\sevensy \scriptscriptfont2=\fivesy
  \textfont3=\tenex \scriptfont3=\tenex \scriptscriptfont3=\tenex
  \def\it{\fam\itfam\tenit}%
  \textfont\itfam=\tenit
  \def\sl{\fam\slfam\tensl}%
  \textfont\slfam=\tensl
  \def\bf{\fam\bffam\tenbf}%
  \textfont\bffam=\tenbf \scriptfont\bffam=\sevenbf
   \scriptscriptfont\bffam=\fivebf
  \def\tt{\fam\ttfam\tentt}%
  \textfont\ttfam=\tentt
  \tt \ttglue=.5em plus.25em minus.15em
  \normalbaselineskip=12pt
  \def\MF{{\manual META}\-{\manual FONT}}%
  \let\sc=\eightrm
  \let\big=\tenbig
  \setbox\strutbox=\hbox{\vrule height8.5pt depth3.5pt width\z@}%
  \normalbaselines\rm}

\def\ninepoint{\def\rm{\fam0\ninerm}%
  \textfont0=\ninerm \scriptfont0=\sixrm \scriptscriptfont0=\fiverm
  \textfont1=\ninei \scriptfont1=\sixi \scriptscriptfont1=\fivei
  \textfont2=\ninesy \scriptfont2=\sixsy \scriptscriptfont2=\fivesy
  \textfont3=\tenex \scriptfont3=\tenex \scriptscriptfont3=\tenex
  \def\it{\fam\itfam\nineit}%
  \textfont\itfam=\nineit
  \def\sl{\fam\slfam\ninesl}%
  \textfont\slfam=\ninesl
  \def\bf{\fam\bffam\ninebf}%
  \textfont\bffam=\ninebf \scriptfont\bffam=\sixbf
   \scriptscriptfont\bffam=\fivebf
  \def\tt{\fam\ttfam\ninett}%
  \textfont\ttfam=\ninett
  \tt \ttglue=.5em plus.25em minus.15em
  \normalbaselineskip=11pt
  \def\MF{{\manual hijk}\-{\manual lmnj}}%
  \let\sc=\sevenrm
  \let\big=\ninebig
  \setbox\strutbox=\hbox{\vrule height8pt depth3pt width\z@}%
  \normalbaselines\rm}

\def\eightpoint{\def\rm{\fam0\eightrm}%
  \textfont0=\eightrm \scriptfont0=\sixrm \scriptscriptfont0=\fiverm
  \textfont1=\eighti \scriptfont1=\sixi \scriptscriptfont1=\fivei
  \textfont2=\eightsy \scriptfont2=\sixsy \scriptscriptfont2=\fivesy
  \textfont3=\tenex \scriptfont3=\tenex \scriptscriptfont3=\tenex
  \def\it{\fam\itfam\eightit}%
  \textfont\itfam=\eightit
  \def\sl{\fam\slfam\eightsl}%
  \textfont\slfam=\eightsl
  \def\bf{\fam\bffam\eightbf}%
  \textfont\bffam=\eightbf \scriptfont\bffam=\sixbf
   \scriptscriptfont\bffam=\fivebf
  \def\tt{\fam\ttfam\eighttt}%
  \textfont\ttfam=\eighttt
  \tt \ttglue=.5em plus.25em minus.15em
  \normalbaselineskip=9pt
  \def\MF{{\manual opqr}\-{\manual stuq}}%
  \let\sc=\sixrm
  \let\big=\eightbig
  \setbox\strutbox=\hbox{\vrule height7pt depth2pt width\z@}%
  \normalbaselines\rm}

\def\tenmath{\tenpoint\fam-1 } % use after $ in ninepoint sections
\def\tenbig#1{{\hbox{$\left#1\vbox to8.5pt{}\right.\n@space$}}}
\def\ninebig#1{{\hbox{$\textfont0=\tenrm\textfont2=\tensy
  \left#1\vbox to7.25pt{}\right.\n@space$}}}
\def\eightbig#1{{\hbox{$\textfont0=\ninerm\textfont2=\ninesy
  \left#1\vbox to6.5pt{}\right.\n@space$}}}
    
\chardef\other=12
\def\ttverbatim{\begingroup
  \catcode`\\=\other
  \catcode`\{=\other
  \catcode`\}=\other
  \catcode`\$=\other
  \catcode`\&=\other
  \catcode`\#=\other
  \catcode`\%=\other
  \catcode`\~=\other
  \catcode`\_=\other
  \catcode`\^=\other
  \obeyspaces \obeylines \tt}
  
\catcode`\|=\active
{\obeylines \gdef|{\ttverbatim \spaceskip\ttglue \let^^M=\  \let|=\endgroup}}

\def\|{\leavevmode\hbox{\tt\char`\|}}

\def\beginlisting{
\bgroup\offinterlineskip\halign\bgroup\offinterlineskip
\vbox{\offinterlineskip\hbox{\vrule height2pt width0pt}\hbox{\strut\tt##:\hfil}\hbox{\vrule height2pt width0pt}}\quad&% memory location
##&% instruction word
\quad\hfil\vbox{\offinterlineskip\hbox{\vrule height2pt width0pt}\hbox{\strut\sevenrm##}\hbox{\vrule height2pt width0pt}}&% line number
\ \vbox{\offinterlineskip\hbox{\vrule height2pt width0pt}\hbox to 20pc{\strut##\hfil}\hbox{\vrule height2pt width0pt}}\cr% source code
}

\def\endlisting{\egroup\egroup}

\def\mixword#1.#2.#3.#4.#5.#6.{%
  \vbox{\offinterlineskip%
	\ifnum\count0>\utilmix
	  \global\utilmix=\count0\hrule
	\else
	  \ifusehrule\hrule\fi
	\fi%
	\global\usehrulefalse%
  	\hbox{\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt}%
  	\hbox{%
  		\vrule%
  		\hbox to 15pt{\strut\hfil#1\hfil}%
  		\vrule%
  		\hbox to 15pt{\hfil\tt#2\hfil}%
  		\vrule%
  		\hbox to 15pt{\hfil\tt#3\hfil}%
  		\vrule%
  		\hbox to 15pt{\hfil\tt#4\hfil}%
  		\vrule%
  		\hbox to 15pt{\hfil\tt#5\hfil}%
  		\vrule%
  		\hbox to 15pt{\hfil\tt#6\hfil}%
  		\vrule%
  	}%
  	\hbox{\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt}%
    \hrule%
  }%
}

\def\mixinst#1.#2.#3.#4.#5.{%
  \offinterlineskip\vbox{\offinterlineskip%
	\ifnum\count0>\utilmix
	  \global\utilmix=\count0\hrule
	\else
	  \ifusehrule\hrule\fi
	\fi%
	\global\usehrulefalse%
  	\hbox{\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt}%
  	\hbox{%
  		\vrule%
  		\hbox to 15pt{\strut\hfil#1\hfil}%
  		\vrule%
  		\hbox to 30.4pt{\hfil\tt#2\thinspace}%
  		\vrule%
  		\hbox to 15pt{\hfil\tt#3\hfil}%
  		\vrule%
  		\hbox to 15pt{\hfil\tt#4\hfil}%
  		\vrule%
  		\hbox to 15pt{\hfil\tt#5\hfil}%
  		\vrule%
  	}%
  	\hbox{\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt\hskip15pt\vrule height2pt}%
    \hrule%
  }%
}

\font\fnamefont=cmssdc10 at 40pt
\font\titlefont=cmssdc10 at 30pt

\centerline{\fnamefont " + SourceFile.ToUpper() + @"}
\vskip1em
\centerline{\titlefont Program Listing}
\vfill\eject

\beginlisting";
                #endregion
            }
        }
        public string PostDocText { get { return @"\endlisting\bye"; } }
        public string EmptyLine { get { return @"\omit&\omit&\omit&\omit\strut\cr" + Environment.NewLine; } }

        public string FormatHeading(string headingText)
        {
            return "\\multispan4\\global\\usehruletrue\\leaders\\hrule\\hfil{\\it " 
                + headingText 
                + "}\\leaders\\hrule\\hfil\\cr\n\\omit&\\omit&\\omit&\\omit\\strut\\cr";
        }

        public string FormatInstruction(int location, MIXWord instruction, int lineNo, string line)
        {
            string strLine = string.Format("{0:0000}&\\mixinst${1}$.{2}.{3}.{4}.{5}.&{6}&|{7}|\\cr",
                location, instruction[0] == 1 ? "-" : "+", instruction[1, 2], instruction[3], instruction[4], instruction[5],
                lineNo, line);

            return strLine;
        }

		public string FormatPseudo(int lineNo, string line)
		    => $"\\omit\\global\\usehruletrue&\\omit&{lineNo}&|{line}|\\cr";

        public string FormatSymbol(string name, MIXWord value)
		    => string.Format("{0}&\\mixword${1}$.{2}.{3}.{4}.{5}.{6}.&=&{7}\\cr", name.Replace("|", @"\|"),
                value[0] == 1 ? "-" : "+", value[1], value[2], value[3], value[4], value[5], value.Value);
    }
}