using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Seeding;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.MasterData.DataAccess.Seeders;

[Export(LifetimeType.Scoped, typeof(IDatabaseSeeder))]
public class MasterDataSeeder(MasterDataDbContext context) : IDatabaseSeeder
{
    public int Order => 10;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedIndustriesAsync(cancellationToken);
        await SeedWorkFormatsAsync(cancellationToken);
        await SeedCandidateSourcesAsync(cancellationToken);
        await SeedSkillCategoriesAndSkillsAsync(cancellationToken);
        await SeedStackItemsAsync(cancellationToken);
        await SeedPositionsAsync(cancellationToken);
    }

    private async Task SeedIndustriesAsync(CancellationToken ct)
    {
        string[] names =
        [
            "Information Technology",
            "Finance & Banking",
            "Healthcare",
            "E-Commerce & Retail",
            "Telecommunications",
            "Education & EdTech",
            "Gaming",
            "Media & Entertainment",
            "Logistics & Supply Chain",
            "Manufacturing",
            "Real Estate",
            "Insurance",
            "Energy & Utilities",
            "Automotive",
            "Government & Public Sector"
        ];

        var existing = await context.Industries.Select(x => x.Name).ToHashSetAsync(ct);

        foreach (var name in names.Where(n => !existing.Contains(n)))
            context.Industries.Add(Industry.Create(name));

        await context.SaveChangesAsync(ct);
    }

    private async Task SeedWorkFormatsAsync(CancellationToken ct)
    {
        string[] names = ["Remote", "Office", "Hybrid", "Flexible"];

        var existing = await context.WorkFormats.Select(x => x.Name).ToHashSetAsync(ct);

        foreach (var name in names.Where(n => !existing.Contains(n)))
            context.WorkFormats.Add(WorkFormat.Create(name));

        await context.SaveChangesAsync(ct);
    }

    private async Task SeedCandidateSourcesAsync(CancellationToken ct)
    {
        string[] names =
        [
            "LinkedIn",
            "HH.ru",
            "Referral",
            "Company Website",
            "GitHub",
            "Habr Career",
            "Recruitment Agency",
            "Cold Outreach",
            "Job Fair",
            "Indeed"
        ];

        var existing = await context.CandidateSources.Select(x => x.Name).ToHashSetAsync(ct);

        foreach (var name in names.Where(n => !existing.Contains(n)))
            context.CandidateSources.Add(CandidateSource.Create(name));

        await context.SaveChangesAsync(ct);
    }

    private async Task SeedSkillCategoriesAndSkillsAsync(CancellationToken ct)
    {
        var skillsByCategory = new Dictionary<string, string[]>
        {
            ["Backend"] = ["C#", "Java", "Python", "Go", "Kotlin", "Rust", "Node.js", "PHP", "Ruby", "Scala"],
            ["Frontend"] = ["TypeScript", "JavaScript", "React", "Angular", "Vue.js", "HTML", "CSS", "Next.js"],
            ["Mobile"] = ["Swift", "Objective-C", "Kotlin", "Flutter", "React Native", "Dart"],
            ["DevOps & Infrastructure"] = ["Docker", "Kubernetes", "Terraform", "Ansible", "CI/CD", "Linux", "Helm", "GitOps"],
            ["Databases"] = ["PostgreSQL", "MySQL", "MongoDB", "Redis", "Elasticsearch", "ClickHouse", "Cassandra", "MS SQL"],
            ["Cloud"] = ["AWS", "Azure", "GCP", "Yandex Cloud"],
            ["Testing"] = ["Unit Testing", "Integration Testing", "E2E Testing", "Load Testing", "Playwright", "Selenium", "k6"],
            ["Architecture"] = ["Microservices", "DDD", "CQRS", "Event Sourcing", "REST", "gRPC", "GraphQL", "Clean Architecture"],
            ["Data & ML"] = ["Python", "SQL", "Spark", "Airflow", "TensorFlow", "PyTorch", "Pandas", "NumPy"],
            ["Security"] = ["OWASP", "Penetration Testing", "OAuth2", "OpenID Connect", "Cryptography"]
        };

        var existingCategories = await context.SkillCategories
            .ToDictionaryAsync(x => x.Name, x => x.Id, ct);

        foreach (var (categoryName, _) in skillsByCategory.Where(kv => !existingCategories.ContainsKey(kv.Key)))
        {
            var category = SkillCategory.Create(categoryName);
            context.SkillCategories.Add(category);
            existingCategories[categoryName] = category.Id;
        }

        await context.SaveChangesAsync(ct);

        var existingSkills = await context.Skills.Select(x => x.Name).ToHashSetAsync(ct);

        foreach (var (categoryName, skills) in skillsByCategory)
        {
            var categoryId = existingCategories[categoryName];
            foreach (var skillName in skills.Where(s => !existingSkills.Contains(s)))
            {
                context.Skills.Add(Skill.Create(skillName, categoryId));
                existingSkills.Add(skillName);
            }
        }

        await context.SaveChangesAsync(ct);
    }

    private async Task SeedStackItemsAsync(CancellationToken ct)
    {
        string[] names =
        [
            ".NET", "Spring Boot", "Django", "FastAPI", "Laravel", "Ruby on Rails", "Express.js", "NestJS",
            "React", "Angular", "Vue.js", "Svelte", "Next.js", "Nuxt.js",
            "PostgreSQL", "MySQL", "MongoDB", "Redis", "Elasticsearch", "ClickHouse",
            "Docker", "Kubernetes", "Kafka", "RabbitMQ", "gRPC", "GraphQL",
            "AWS", "Azure", "GCP", "Terraform", "GitLab CI", "GitHub Actions"
        ];

        var existing = await context.StackItems.Select(x => x.Name).ToHashSetAsync(ct);

        foreach (var name in names.Where(n => !existing.Contains(n)))
            context.StackItems.Add(StackItem.Create(name));

        await context.SaveChangesAsync(ct);
    }

    private async Task SeedPositionsAsync(CancellationToken ct)
    {
        var positions = new (string Name, Grade Grade)[]
        {
            ("Software Engineer", Grade.Junior),
            ("Software Engineer", Grade.Middle),
            ("Software Engineer", Grade.Senior),
            ("Software Engineer", Grade.Lead),
            ("Software Engineer", Grade.Principal),
            ("Frontend Developer", Grade.Junior),
            ("Frontend Developer", Grade.Middle),
            ("Frontend Developer", Grade.Senior),
            ("Frontend Developer", Grade.Lead),
            ("Backend Developer", Grade.Junior),
            ("Backend Developer", Grade.Middle),
            ("Backend Developer", Grade.Senior),
            ("Backend Developer", Grade.Lead),
            ("Full Stack Developer", Grade.Middle),
            ("Full Stack Developer", Grade.Senior),
            ("Mobile Developer", Grade.Junior),
            ("Mobile Developer", Grade.Middle),
            ("Mobile Developer", Grade.Senior),
            ("DevOps Engineer", Grade.Junior),
            ("DevOps Engineer", Grade.Middle),
            ("DevOps Engineer", Grade.Senior),
            ("DevOps Engineer", Grade.Lead),
            ("QA Engineer", Grade.Junior),
            ("QA Engineer", Grade.Middle),
            ("QA Engineer", Grade.Senior),
            ("Data Engineer", Grade.Middle),
            ("Data Engineer", Grade.Senior),
            ("ML Engineer", Grade.Middle),
            ("ML Engineer", Grade.Senior),
            ("Solution Architect", Grade.Senior),
            ("Solution Architect", Grade.Principal),
            ("Team Lead", Grade.Lead),
            ("Engineering Manager", Grade.Lead),
            ("CTO", Grade.Principal),
            ("Product Manager", Grade.Middle),
            ("Product Manager", Grade.Senior),
            ("Product Manager", Grade.Lead),
            ("Recruiter", Grade.Junior),
            ("Recruiter", Grade.Middle),
            ("Recruiter", Grade.Senior),
            ("HR Manager", Grade.Middle),
            ("HR Manager", Grade.Senior),
        };

        var existing = await context.Positions
            .Select(x => new { x.Name, x.Grade })
            .ToListAsync(ct);

        var existingSet = existing.Select(x => (x.Name, x.Grade)).ToHashSet();

        foreach (var (name, grade) in positions.Where(p => !existingSet.Contains((p.Name, p.Grade))))
            context.Positions.Add(Position.Create(name, grade));

        await context.SaveChangesAsync(ct);
    }
}
