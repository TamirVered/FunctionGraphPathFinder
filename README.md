# FunctionGraphPathFinder
A utility for finding function call path in an exported radare2/Cutter global call-graph JSON.

## Motivation
The global call-graph feature in Cutter is useful for having a bird's-eye view over the flow of an application.
An issue arrises when an application is large enough so finding a call-path between two desired functions (starting function, and a target function) becomes difficult to do "by eye".

FunctionGraphPathFinder (fgpf.exe) is a utility which receives an exported radare2/Cutter global call-graph JSON file along with a starting function and a target function and outputs all of the possible paths in the graph.

## Usage
`FunctionGraphPathFinder.exe [path] [source function] [target function]`

## Example
`FunctionGraphPathFinder.exe "C:\re\call-graph.json" fcn.00114818 fcn.00115e4d`

Output, when there are three relevant paths:

<span style="color: red">fnc.00114818</span> -> fnc.00124ee6 -> fnc.00115e4d

`fnc.00114818 -> FunctionWithCutomName -> fnc.00124ee6 -> fnc.00115e4d`

`fnc.00114818 -> FunctionWithCutomName -> fnc.00115e4d`
