using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

public class CommandExecutor
{
    private readonly IWebDriver _driver;

    public CommandExecutor(IWebDriver driver)
    {
        _driver = driver;
    }

    public void ExecuteStep(Step step)
    {
        Console.WriteLine($"Executing action: {step.Action}");

        switch (step.Action.ToLower())
        {
            case "click":
                Click(step.Selector);
                break;

            case "type":
                Type(step.Selector, step.Value);
                break;

            default:
                throw new Exception($"Unknown action: {step.Action}");
        }
    }

    public void OpenAddRecordModal()
    {
        Click("#addNewRecordButton");
    }
    public void FillRegistrationForm(PersonRecord record)
    {
        TypeById("firstName", record.FirstName);
        TypeById("lastName", record.LastName);
        TypeById("userEmail", record.Email);
        TypeById("age", record.Age);
        TypeById("salary", record.Salary);
        TypeById("department", record.Department);
    }

    public void SubmitRegistrationForm()
    {
        Click("#submit");
    }

    private void Click(string selector)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        var element = wait.Until(driver => driver.FindElement(By.CssSelector(selector)));
        element.Click();
    }

    private void Type(string selector, string value)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        var element = wait.Until(driver => driver.FindElement(By.CssSelector(selector)));
        element.Clear();
        element.SendKeys(value);
    }

    private void TypeById(string id, string value)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        var element = wait.Until(driver => driver.FindElement(By.Id(id)));
        element.Clear();
        element.SendKeys(value);
    }

    public bool HasInvalidFields()
    {
        var invalidElements = _driver.FindElements(
            By.CssSelector("input:invalid")
        );

        return invalidElements.Count > 0;
    }

    public bool IsRegistrationModalOpen()
    {
        var modalElements = _driver.FindElements(By.CssSelector(".modal-content"));

        return modalElements.Count > 0 && modalElements[0].Displayed;
    }
    public void CloseRegistrationForm()
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));

        // Try likely close button selectors first
        var possibleSelectors = new[]
        {
            "#closeLargeModal",
            ".modal-content button.close",
            ".modal-header button.close",
            ".close"
        };

        foreach (var selector in possibleSelectors)
        {
            var elements = _driver.FindElements(By.CssSelector(selector));

            if (elements.Count > 0 && elements[0].Displayed && elements[0].Enabled)
            {
                elements[0].Click();
                return;
            }
        }

        // Fallback: press Escape
        _driver.FindElement(By.TagName("body")).SendKeys(Keys.Escape);

        // Optional: wait until modal disappears
        wait.Until(driver =>
        {
            var modals = driver.FindElements(By.CssSelector(".modal-content"));
            return modals.Count == 0 || !modals[0].Displayed;
        });
    }

    public void ClickEditButton(string firstName, string lastName)
    {
        object? result = ((IJavaScriptExecutor)_driver).ExecuteScript(@"
            const buttons = Array.from(document.querySelectorAll('[id^=""edit-record-""]'))
                .filter(btn => btn.offsetParent !== null);

            if (buttons.length === 0) {
                return false;
            }

            buttons[buttons.length - 1].click();
            return true;
        ");

        bool clicked = result is bool b && b;

        if (!clicked)
        {
            throw new Exception($"Edit button not found for row: {firstName} {lastName}");
        }
    }
    public void ClickDeleteButton(string firstName, string lastName)
    {
        object? result = ((IJavaScriptExecutor)_driver).ExecuteScript(@"
            const buttons = Array.from(document.querySelectorAll('[id^=""delete-record-""]'))
                .filter(btn => btn.offsetParent !== null);

            if (buttons.length === 0) {
                return false;
            }

            buttons[buttons.length - 1].click();
            return true;
        ");

        bool clicked = result is bool b && b;

        if (!clicked)
        {
            throw new Exception($"Delete button not found for row: {firstName} {lastName}");
        }
    }
}