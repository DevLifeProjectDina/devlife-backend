using DevLifeBackend.Models;
using MongoDB.Driver;
using DevLifeBackend.Enums;
namespace DevLifeBackend.Data.Seed
{
    public static class SeedData
    {
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
                Language = TechStack.Unknown,
                Difficulty = ExperienceLevel.Unknown,
                Description = "This console log should print the number 5.",
                CorrectCode = "Console.WriteLine(5);",
                BuggyCode = "Console.WriteLine(\"5\");",
                Source = "Static (Hardcoded)"
            },
                new CodeSnippet { Language = TechStack.NET, Difficulty = ExperienceLevel.Junior, Description = "This LINQ query should return only even numbers.", CorrectCode = "var evenNumbers = numbers.Where(n => n % 2 == 0);", BuggyCode = "var evenNumbers = numbers.Where(n => n % 2 = 0);", Source = "Static (Hardcoded)" },
                new CodeSnippet { Language = TechStack.React, Difficulty = ExperienceLevel.Junior, Description = "This component should increment the count on button click.", CorrectCode = "const [count, setCount] = useState(0);\n<button onClick={() => setCount(count + 1)}>Click</button>", BuggyCode = "const [count, setCount] = useState(0);\n<button onClick={setCount(count + 1)}>Click</button>", Source = "Static (Hardcoded)" },
                new CodeSnippet { Language = TechStack.Python, Difficulty = ExperienceLevel.Middle, Description = "This function should return a list of squared numbers.", CorrectCode = "def square_list(numbers):\n    return [n**2 for n in numbers]", BuggyCode = "def square_list(numbers):\n    return (n**2 for n in numbers)", Source = "Static (Hardcoded)" },
                new CodeSnippet { Language = TechStack.NET, Difficulty = ExperienceLevel.Middle, Description = "This should correctly initialize a string.", CorrectCode = "string message = \"Hello, World!\";", BuggyCode = "string message = 'Hello, World!';", Source = "Static (Hardcoded)" },
                new CodeSnippet { Language = TechStack.Angular, Difficulty = ExperienceLevel.Junior, Description = "This should bind the 'title' property to the template.", CorrectCode = "<h1>{{ title }}</h1>", BuggyCode = "<h1>{ title }</h1>", Source = "Static (Hardcoded)" },
                new CodeSnippet { Language = TechStack.Angular, Difficulty = ExperienceLevel.Middle, Description = "This should correctly loop through an array using *ngFor.", CorrectCode = "<li *ngFor=\"let item of items\">{{ item }}</li>", BuggyCode = "<li *ngFor=\"let item in items\">{{ item }}</li>", Source = "Static (Hardcoded)" },
                new CodeSnippet { Language = TechStack.Angular, Difficulty = ExperienceLevel.Senior, Description = "This is two-way data binding for an input field.", CorrectCode = "<input [(ngModel)]=\"userName\">", BuggyCode = "<input (ngModel)=\"userName\">", Source = "Static (Hardcoded)" }
            };

            context.CodeSnippets.InsertMany(snippets);

            if (!context.DatingProfiles.Find(_ => true).Any())
            {
                var profiles = new DatingProfile[]
                {
            new DatingProfile { Name = "Anna", Age = 28, Stacks = TechStack.Python | TechStack.React, Bio = "I like clean architecture and long walks on the beach... to think about clean architecture.", CharacterPrompt = "You are Anna, a sharp and witty .NET developer. You are confident and a bit sarcastic." },
            new DatingProfile { Name = "Leo", Age = 25, Stacks =  TechStack.Python | TechStack.React, Bio = "My life is like a hook, it only re-renders when the state changes. Looking for my dependency.", CharacterPrompt = "You are Leo, a cool and creative React developer. You are very passionate about UI/UX and modern design." },
            new DatingProfile { Name = "Eva", Age = 31, Stacks =  TechStack.Python | TechStack.React, Bio = "My love for you is like a Jupyter Notebook: messy, but full of beautiful results. Let's analyze our compatibility.", CharacterPrompt = "You are Eva, a smart and thoughtful Python data scientist. You are very analytical and use data-related metaphors." }
                };
                context.DatingProfiles.InsertMany(profiles);
            }
        }
    }
}