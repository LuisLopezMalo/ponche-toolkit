Instructions to compile and build the PT.Builder project and how it must be called.

- Compile the PoncheToolkit.Builder project.
- In the PoncheToolkit main project, go to Properties -> BuildEvents
	And in the Pre-build event, call the PoncheToolkit.Builder.exe program with the correct parameters:
	"$(TargetDir)/PoncheToolkit.Builder.exe" -i "$(ProjectDir)/Content/Effects/PTColorEffect.fx" -in "$(TargetDir)/Content/Effects"