using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

public class TableHelper
{
    private readonly IWebDriver _driver;

    public TableHelper(IWebDriver driver)
    {
        _driver = driver;
    }

    private string GetPageText()
    {
        return _driver.FindElement(By.TagName("body")).Text;
    }

    private bool IsRowPresent(string firstName, string lastName)
    {
        string pageText = GetPageText();

        return pageText.Contains(firstName) && pageText.Contains(lastName);
    }

    private bool IsRowPresent(string firstName, string lastName, string value)
    {
        string pageText = GetPageText();

        return pageText.Contains(firstName)
            && pageText.Contains(lastName)
            && pageText.Contains(value);
    }

    public int CountValueOccurrences(string value)
    {
        string pageText = GetPageText();

        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        return pageText.Split(value).Length - 1;
    }

    public void WaitForTableRowData(string firstName, string lastName, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));

        wait.Until(driver => IsRowPresent(firstName, lastName));
    }

    public void WaitForTableRowData(string firstName, string lastName, string value, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));

        wait.Until(driver => IsRowPresent(firstName, lastName, value));
    }

    public void WaitForTableRowDeletion(string uniqueValue, int previousCount, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));

        wait.Until(driver => CountValueOccurrences(uniqueValue) < previousCount);
    }

    public bool IsTableRowDataPresent(string firstName, string lastName)
    {
        return IsRowPresent(firstName, lastName);
    }

    public bool IsTableRowDataPresent(string firstName, string lastName, string value)
    {
        return IsRowPresent(firstName, lastName, value);
    }

    public void PrintCurrentPageText()
    {
        Console.WriteLine("Current page text:");
        Console.WriteLine(GetPageText());
    }
}