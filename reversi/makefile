SOURCE=Program.cs Form1.Designer.cs Form1.cs \
Reversi.cs ReversiLogik.cs \
Properties/AssemblyInfo.cs Properties/Resources.Designer.cs Properties/Settings.Designer.cs

Debug: $(SOURCE)
	gmcs -debug \
	-r:System -r:System.Windows.Forms -r:System.Xml -r:System.Drawing -r:System.Data \
	-out:bin/Debug/reversi.exe \
	-resource:Form1.resources -resource:Properties/Resources.resources \
	-target:winexe $(SOURCE)