run-clean: cleanup restore build run 

all : cleanup restore build

cleanup:
	dotnet clean TP1.csproj

restore:
	dotnet restore TP1.csproj

build:
	dotnet build TP1.csproj

run:
	dotnet run -p TP1.csproj