public class TestScenario
{
    public string Url { get; set; } = string.Empty;
    public List<Step> Steps { get; set; } = new();
}

public class Step
{
    public string Action { get; set; } = string.Empty;
    public string Selector { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class PersonRecord
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Age { get; set; } = string.Empty;
    public string Salary { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string UpdatedSalary { get; set; } = string.Empty;
}