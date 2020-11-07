# BrassRay
Proof of concept software-based stochastic ray tracer in C#

My development environment is Windows.  I tested the CLI on Linux and it seems to be fine there.  It probably works on macOS too. 
I am working on a WPF frontend as well, which is not part of this repository yet.  WPF is Windows only unfortunately.

The project name is related to my last name.

## Quickstart
Go to the FrontendCli directory in your terminal shell

Type `dotnet build -c release`

Type `dotnet run -c release -- ../Examples/colorful.yaml ../Examples/out.png`

It will render a draft quality image to ../Examples/out.png.

Try the following for a high quality render: `dotnet run -c release -- ../Examples/colorful.yaml ../Examples/out2.png -s 400 -h 1080`

Type `dotnet run -c release` for a list of CLI options.

Take a look at the YAML files and get a feel for how the scene graphs are laid out.  Look at the RayTracer project for all of the types and properties you can use.
Feel free to make your own YAML scene files, but beware that the CLI has no error handling yet.  If you make a syntax error or logic error, it will crash with an unhelpful message.
