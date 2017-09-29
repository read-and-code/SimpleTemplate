PROJECT_NAME = SimpleTemplate
PROJECT_FILE = ./$(PROJECT_NAME)/$(PROJECT_NAME).csproj
TEST_PROJECT_FILE = ./$(PROJECT_NAME).Tests/$(PROJECT_NAME).Tests.csproj

restore:
	dotnet restore $(PROJECT_FILE)
	dotnet restore $(TEST_PROJECT_FILE)

build:
	dotnet build $(PROJECT_FILE)
	dotnet build $(TEST_PROJECT_FILE)

clean:
	dotnet clean $(PROJECT_FILE)
	dotnet clean $(TEST_PROJECT_FILE)

test:
	dotnet test $(TEST_PROJECT_FILE)