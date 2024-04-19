﻿using System.Diagnostics;
using System.Text;
using FiniteAutomatas.Domain.Automatas;
using FiniteAutomatas.Domain.ValueObjects;

namespace FiniteAutomatas.Visualizations;

public class Visualizer
{
    private readonly string _graphvizPath = "./Graphviz/bin/dot.exe";
    private readonly string _graphName = "graphName";

    private readonly FiniteAutomata _automata;

    public Visualizer( FiniteAutomata automata )
    {
        _automata = automata;
    }

    public void ToImage( string path )
    {
        var nodes = _automata.AllStates.Select( x => $"{x.Name} [{BuildNodeStyles( x )}];" );
        var transitions = _automata.Transitions.Select( x => $"{x.From.Name} -> {x.To.Name} [label=\"{BuildTransitionLabel( x )}\"];" );

        string data = $@"
            digraph {_graphName} {{
	            rankdir=LR; 
                {{ {String.Join( "", nodes )} }}
                {{ {String.Join( "", transitions )} }}
            }}
        ";

        var tempDataPath = $"{path}.dot";
        File.WriteAllText( tempDataPath, data );

        var startInfo = new ProcessStartInfo
        {
            FileName = _graphvizPath,
            Arguments = $"-Tpng {tempDataPath} -o {path}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = new Process
        {
            StartInfo = startInfo,
        };

        process.Start();
        process.WaitForExit();

        File.Delete( tempDataPath );
    }

    private static string BuildTransitionLabel( Transition transition )
    {
        return transition.Argument == Argument.Epsilon
            ? "Eps"
            : transition.Argument.Value;
    }

    private static string BuildNodeStyles( State x )
    {
        string style = "filled";
        string label = x.Name;
        string fillcolor = BuildFillColor( x );

        return $"style=\"{style}\" fillcolor=\"{fillcolor}\" label=\"{label}\"";
    }

    private static string BuildFillColor( State x )
    {
        if ( x.IsEnd && x.IsStart )
        {
            return "purple";
        }
        
        if ( x.IsStart )
        {
            // Blue
            return "#40b0f0";
        }

        if ( x.IsEnd )
        {
            return "red";
        }

        return "white";
    }
}