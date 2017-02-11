using System;
using System.IO;

using Frontend1;
using Frontend1.Parser;

public class ScriptProcessor {
    VendingMachineParser parser;
    SENG301VMAnalyzer analyzer;

    public ScriptProcessor(TextReader reader, IVendingMachineFactory factory) {
        this.analyzer = new SENG301VMAnalyzer();
        this.analyzer.RegisterVendingMachineFactory(factory);
        this.parser = new VendingMachineParser(reader, this.analyzer);
    }

    public ScriptProcessor(string pathToScript, IVendingMachineFactory factory) :
        this(new StreamReader(File.OpenRead(pathToScript)), factory) {
    }

    public void Parse() {
        this.parser.Parse();
    }
    
    public static void Main(string[] args) {
        int totalTests = 0;
        int passedTests = 0;
        var goodScripts = Directory.GetFiles("test-scripts", "T*");
        foreach(var script in goodScripts) {
            var pass = true;
            Console.Write(script + ":");
            try {
                var scriptParser = new ScriptProcessor(new StreamReader(File.OpenRead(script)), new seng301_asgn1.VendingMachineFactory());
                scriptParser.Parse();
            }
            catch {
                pass = false;
            }
            Console.WriteLine(pass ? " PASS=Good" : " FAIL=Bad");
            if (pass) {
                passedTests++;
            }
            totalTests++;
        }
        var badScripts = Directory.GetFiles("test-scripts", "U*");
        foreach(var script in badScripts) {
            var pass = true;
            Console.Write(script + ":");
            try {
                var scriptParser = new ScriptProcessor(new StreamReader(File.OpenRead(script)), new seng301_asgn1.VendingMachineFactory());
                scriptParser.Parse();
            }
            catch {
                pass = false;
            }
            Console.WriteLine(pass ? " PASS=Bad" : " FAIL=Good");
            if (!pass) {
                passedTests++;
            }
            totalTests++;
        }

        Console.WriteLine("{0}/{1} tests passed", passedTests, totalTests);
		Console.ReadLine();
    }
}
