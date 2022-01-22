using BuildingBlocks.Core.Domain.Model;
using Catalog.Categories.Exceptions;
using Catalog.Categories.Exceptions.Domain;

namespace Catalog.Categories;

// https://stackoverflow.com/a/32354885/581476
public class Category : AggregateRoot<long>
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Code { get; private set; }

    public static Category Create(long id, string name, string code, string description = "")
    {
        var category = new Category { Id = id };

        category.ChangeName(name);
        category.ChangeDescription(description);
        category.ChangeCode(code);

        return category;
    }

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new CategoryDomainException("Name can't be white space or null.");

        Name = name;
    }

    public void ChangeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new CategoryDomainException("Code can't be white space or null.");

        Code = code;
    }

    public void ChangeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new CategoryDomainException("Description can't be white space or null.");

        Description = description;
    }

    public override string ToString()
    {
        return $"{Name} - {Code}";
    }
}
