all: project

project:
	rm project_*.gv project_*.gv.png
	dotnet build . -o ../build/helveg -r linux-x64
	dotnet run ~/dev/riganti/dotvvm/src/DotVVM.Core/DotVVM.Core.csproj
	for f in project_*.gv; do neato -n -Tpng $$f > $$f.png; done
	feh project_*.png &

.PHONY: all project
