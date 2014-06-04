mdbg> mo nc on
mdbg> run "E:\Galos\Projects\MDbg\Version_4\MDbg Sample\bin\Debug\test\ConsoleApplication12.exe" 
STOP: Breakpoint Hit
12:        {
[t#:0] mdbg> next
13:            MessageBox.Show("Hello World!");
[t#:0] mdbg> go
STOP: Process Exited
mdbg> mo nc on
mdbg> run "E:\Galos\Projects\MDbg\Version_4\MDbg Sample\bin\Debug\test\ConsoleApplication12.exe" 
STOP: Breakpoint Hit
12:        {


go      F5
next    F10         (step over)
step    F11         (step in)
out     Shift+F10   (step out)

F9 public MDbgBreakpoint CreateBreakpoint(string fileName, int lineNumber)
and MDbgBreakpoint.Delete();


------------------------------
Following commands are available:
q[uit]        Quits the program
ex[it]        Quits the program
h[elp]        Prints this help screen.
?             Prints this help screen.
r[un]         Runs a program under the debugger
g[o]          Continues program execution
k[ill]        Kills the active process
setip         Sets an ip into new position in the current function
w[here]       Prints a stack trace
n[ext]        Step Over
s[tep]        Step Into
o[ut]         Steps Out of function
sh[ow]        Show sources around the current location
b[reak]       Sets or displays breakpoints
del[ete]      Deletes a breakpoint
t[hread]      Displays active threads or switches to a specified thread
su[spend]     Prevents thread from running
re[sume]      Resumes suspended thread
int[ercept]   Intercepts the current exception at the given frame on the stack
ca[tch]       Set or display what events will be stopped on
ig[nore]      Set or display what events will be ignored
log           Set or display what events will be logged
enableNotif[ication]
              Enables or disables custom notifications for a given type
mo[de]        Set/Query different debugger options
lo[ad]        Loads an extension from some assembly
unload        Unloads an extension
p[rint]       prints local or debug variables
f[unceval]    Evaluates a given function outside normal program flow
newo[bj]      Creates new object of type typeName
set           Sets a variable to a new value
l[ist]        Displays loaded modules appdomains or assemblies
sy[mbol]      Sets/Displays path or Reloads/Lists symbols
pro[cessenum] Displays active processes
cl[earException]
              Clears the current exception
opendump      Opens the specified dump file for debugging.
a[ttach]      Attaches to a process or prints available processes
de[tach]      Detaches from debugged process
u[p]          Moves the active stack frame up
d[own]        Moves the active stack frame down
pa[th]        Sets or displays current source path
x             Displays functions in a module
ap[rocess]    Switches to another debugged process or prints available ones
fo[reach]     Executes other command on all threads
when          Execute commands based on debugger event
echo          Echoes a message to the console
uwgc[handle]  Prints the object tracked by a GC handle
conf[ig]      Sets or Displays debugger configurable options
printe[xception]
              Prints the last exception on the current thread
mon[itorInfo] Displays object monitor lock information
block[ingObjects]
              Displays any monitor locks blocking threads

Extension: npp
il_n[ext]     Step over the next IL instruction
il_s[tep]     Step into the next IL instruction
.m[enu]       Execute gui menu command
gui           gui [close] - starts/closes a gui interface
