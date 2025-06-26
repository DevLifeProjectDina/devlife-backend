using DevLifeBackend.Models;
using MongoDB.Driver;

namespace DevLifeBackend.Data.Seed
{
    public static class SeedData
    {
        // The method now accepts MongoDbContext
        public static void Initialize(MongoDbContext context)
        {
            if (context.CodeSnippets.Find(_ => true).Any())
            {
                return;
            }

            var snippets = new CodeSnippet[]
            {
                new CodeSnippet
            {
                Language = "Unknown", // For new users from GitHub
                Difficulty = "Junior",
                Description = "This console log should print the number 5.",
                CorrectCode = "Console.WriteLine(5);",
                BuggyCode = "Console.WriteLine(\"5\");", // A subtle bug: prints string "5" not number 5
                Source = "Static (Hardcoded)"
            },
                new CodeSnippet { Language = ".NET", Difficulty = "Junior", Description = "This LINQ query should return only even numbers.", CorrectCode = "var evenNumbers = numbers.Where(n => n % 2 == 0);", BuggyCode = "var evenNumbers = numbers.Where(n => n % 2 = 0);", Source = "Static (Hardcoded)" },
                new CodeSnippet { Language = "React", Difficulty = "Junior", Description = "This component should increment the count on button click.", CorrectCode = "const [count, setCount] = useState(0);\n<button onClick={() => setCount(count + 1)}>Click</button>", BuggyCode = "const [count, setCount] = useState(0);\n<button onClick={setCount(count + 1)}>Click</button>", Source = "Static (Hardcoded)" },
                new CodeSnippet { Language = "Python", Difficulty = "Middle", Description = "This function should return a list of squared numbers.", CorrectCode = "def square_list(numbers):\n    return [n**2 for n in numbers]", BuggyCode = "def square_list(numbers):\n    return (n**2 for n in numbers)", Source = "Static (Hardcoded)" },
                new CodeSnippet { Language = ".NET", Difficulty = "Middle", Description = "This should correctly initialize a string.", CorrectCode = "string message = \"Hello, World!\";", BuggyCode = "string message = 'Hello, World!';", Source = "Static (Hardcoded)" },
                new CodeSnippet { Language = "Angular", Difficulty = "Junior", Description = "This should bind the 'title' property to the template.", CorrectCode = "<h1>{{ title }}</h1>", BuggyCode = "<h1>{ title }</h1>", Source = "Static (Hardcoded)" },
                new CodeSnippet { Language = "Angular", Difficulty = "Middle", Description = "This should correctly loop through an array using *ngFor.", CorrectCode = "<li *ngFor=\"let item of items\">{{ item }}</li>", BuggyCode = "<li *ngFor=\"let item in items\">{{ item }}</li>", Source = "Static (Hardcoded)" },
                new CodeSnippet { Language = "Angular", Difficulty = "Senior", Description = "This is two-way data binding for an input field.", CorrectCode = "<input [(ngModel)]=\"userName\">", BuggyCode = "<input (ngModel)=\"userName\">", Source = "Static (Hardcoded)" }
            };

            context.CodeSnippets.InsertMany(snippets);
        }
    }
}