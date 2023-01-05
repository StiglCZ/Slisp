﻿using System;
using System.Text;

namespace Slisp{
    class Program{        
        public static readonly int ERROR = 1;
        public static readonly int WARN = 2;
        public static readonly int INFO = 3;
        public static string Compile(string path){
            string output = "";
            string code = File.ReadAllText(path);
            code = code.Replace("\n","")
                       .Replace("\t","")
                       .Replace(" " ,"");
            string[] functions = SplitParts(code);
            List<Function> funcs = new List<Function>();
            List<string> includes = new List<string>();
            for(int i = 0; i < functions.Length;i++){
                funcs.Add(new Function(functions[i]));
                output +=  funcs[funcs.Count-1].ccode;
                foreach(string include in funcs[funcs.Count-1].includes){
                    for(int j = 0; j < includes.Count;j++){
                        if(includes[j] == include){
                            goto completed;
                        }
                    }
                    includes.Add(include);
                    completed:
                    i=i;
                }
            }
            foreach(string include in includes){
                output = Compile(include) + output;
            }
            return output;
        }
        public static void Info(int level, string message){
            if(level == ERROR){
                Console.WriteLine("Error: " + message);
                Console.ForegroundColor = ConsoleColor.DarkRed;
            }else if(level == WARN){
                Console.WriteLine("Warning: " + message);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            }else if(level == INFO){
                Console.WriteLine("Info: " + message);
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static string[] SplitParts(string src){
            StringBuilder code = new StringBuilder(src);
            int opened = 0;
            bool already1 = false;
            for (int i = 0; i < code.Length; i++) {
                if (code[i] == '{') {
                    opened++;
                    if(!already1){
                        already1 = true;
                        code[i] = ' ';
                    }
                    
                }
                else if (code[i] == '}') {
                    opened--;
                }
                if (already1 && opened == 0) {
                    code[i] = '\n';
                    already1 = false;
                    opened = 0;
                }
            }
            if(opened > 1){
                Info(WARN,"'}' Expected!");
            }
            return code.ToString().Replace(" ","").Split('\n');
        }
        public static void Main(string[] args){
            string path = "";
            for(int i = 0; i < args.Length;i++){
                if(args[i].Length > 0){
                    path = args[i];
                }
            }
            File.WriteAllText("output.c",Compile(path));
            
        }
        public static string[] SplitPartsAndGetStart(string src){
            StringBuilder source = new(src);
            string start = "";
            for(int i = 0; i < source.Length;i++){
                if(source[i]== '{'){
                    break;
                }else{
                    start += source[i].ToString();
                    source[i] = ' ';
                }
                if(i == source.Length-1){
                    return new string[] {start};
                }
            }
            source = source.Replace(" ","");
            List<string> output = new();
            output.Add(start);
            foreach(string input in SplitParts(source.ToString())){
                output.Add(input);
            }
            return output.ToArray();
        }
    }
    class Function{
        public Function(string code){
            this.code =code;
            parse();
        }
        string code = "";
        public string ccode = "";
        string name = "";
        public List<string> includes;
        List<string> globInts;
        List<string> globStrings;
        List<string> arguments;
        public void parse(){
            string[] instructions = Program.SplitPartsAndGetStart(code);   
            name = instructions[0];
            for(int i = 1; i < instructions.Length;i++){
                string[] arg = Program.SplitPartsAndGetStart(instructions[i]);
                if(arg[0].Length < 1 && i == 1){
                    for(int j = 1; j < arg.Length;j++){
                        arguments.Add(arg[j]);
                    }
                }
                if(arg.Length == 1 && arg[0].Length < 1){
                    Program.Info(Program.ERROR,"Invalid code! instruction:" + name);
                }
                string cmd = arg[0];
                switch(cmd){
                    case "incl":
                        includes.Add(arg[1]);
                        break;
                    case "var":
                        if(arg[1] == "local"){
                            if(arg[2] == "int"){
                                ccode += "int " + arg[3] + ";";
                            }else if(arg[2] == "string"){
                                ccode += "char[256] " + arg[3] + ";";
                            }
                        }else if(arg[1] == "global"){
                            if(arg[2] == "int"){
                                globInts.Add(arg[3]);
                            }else if(arg[2] == "string"){
                                globStrings.Add(arg[3]);
                            }
                        }
                        break;
                    case "set":
                        if(arg[1] == "int"){    
                            ccode += arg[2] + "=" + arg[3];
                        }else if(arg[1] == "string"){
                            ccode += arg[2]+ " = " + arg[3].Replace("\\s"," ").Replace("\\n","\n")  +";";
                        }
                        break;
                    case "io":
                        //Method && Type
                        if(arg[2] == "string"){
                            ccode += arg[1] + "(\"%s\"," + arg[3] +");";
                        } else if(arg[2] == "int"){
                            ccode += arg[1] + "(\"%d\"," + arg[3] +");";
                        }
                        break;
                    default:
                        ccode += arg[0] + "(";
                        for(int j = 1; j < arg.Length;j++){
                            if(j > 1){
                                ccode += ",";
                            }
                            ccode += ");";
                        }
                        break;
                }
            }
            string argtext = "int " + name + "(";
            for(int i = 0; i < arguments.Count;i++){
                if(i > 0){
                    argtext += ",";
                }
                argtext +="int"+ arguments[i];
            }
            argtext += "){";
            ccode = argtext + ccode;
            ccode += "return 0;}";
        }
    }
}