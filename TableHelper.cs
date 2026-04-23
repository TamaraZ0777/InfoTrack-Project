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

    private bool IsRowPresent(string firstName, string lastName, string salary)
    {
        string pageText = GetPageText();

        return pageText.Contains(firstName)
            && pageText.Contains(lastName)
            && pageText.Contains(salary);
    }

    public void WaitForTableRowData(string firstName, string lastName, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));

        wait.Until(driver => IsRowPresent(firstName, lastName));
    }

    public void WaitForTableRowData(string firstName, string lastName, string salary, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));

        wait.Until(driver => IsRowPresent(firstName, lastName, salary));
    }

    public void WaitForTableRowDeletion(string firstName, string lastName, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));

        wait.Until(driver => !IsRowPresent(firstName, lastName));
    }

    public void PrintCurrentPageText()
    {
        Console.WriteLine("Current page text:");
        Console.WriteLine(GetPageText());
    }
}