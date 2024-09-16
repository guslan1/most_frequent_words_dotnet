# Most Frequent Words Web-API

REST-API for code assignment in .NET

## The Assignment

The assignment is to create a web-API that returns the count of the 10 most frequent words together with their frequencies.

### Language of Implementation

[.NET](https://dotnet.microsoft.com/)

### Framework for REST-API

[ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)

## Scripts

### Installing Dependencies
Within the `api` folder, run `dotnet restore` to install dependencies.

### The Application
Within the `api` folder, run `dotnet run` to start the app.

### Developer Mode
Within the `api` folder, run `dotnet watch run` to start the app in dev mode.

### Running Tests
Within the `api.tests` folder, run `dotnet test` to execute the unit tests.

## How to Use the REST-API
### Sending a GET request to /api of the application returns an explanation of how to use the API.
```json
{
    "self": {
        "href": "http://localhost:5116/api",
        "method": "GET",
        "desc": "Overview of most frequent words API"
    },
    "count": {
        "href": "http://localhost:5116/count",
        "method": "POST",
        "desc": "Returns the count of the ten most frequent words in the input string",
        "params": "{input_string}",
        "header": "Content-Type: text/plain"
    }
}