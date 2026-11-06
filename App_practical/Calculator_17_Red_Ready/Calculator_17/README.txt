CALCULATOR_17 — ASP.NET Core MVC (.NET 9)

Open Calculator_17.sln in Visual Studio.
The project listens on http://localhost:5017.

Run locally:
1. Open Calculator_17.sln.
2. Select the Calculator_17 launch profile.
3. Press F5 or Ctrl+F5.

Run with Docker (from the project folder):
    docker build -t calculator-17 .
    docker run --rm -p 5017:5017 calculator-17

Then open:
    http://localhost:5017

The calculator supports addition, subtraction, multiplication and division.
Division by zero displays an error message. The calculator does not need a
database; the sample connection string is retained in appsettings.json as requested.
