using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

class Program
{
    static void Main(string[] args)
    {
        var json = File.ReadAllText("test.json");

        var scenario = JsonSerializer.Deserialize<TestScenario>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (scenario == null)
        {
            throw new Exception("Failed to deserialize test.json");
        }

        var records = CsvReader.ReadData("data.csv");

        if (records.Count == 0)
        {
            throw new Exception("No records found in data.csv");
        }

        //Screenshot Folder creation
        string screenshotsDir = ScreenshotService.CreateRunFolder();
        Console.WriteLine($"Screenshots folder: {screenshotsDir}");


        IWebDriver driver = new ChromeDriver();

        try
        {
            driver.Navigate().GoToUrl(scenario.Url);

            var executor = new CommandExecutor(driver);
            var tableHelper = new TableHelper(driver);

            int successfulRecords = 0;
            int reviewRecords = 0;

            foreach (var record in records)
            {
                bool success = ProcessRecord(driver, executor, tableHelper, screenshotsDir, record);

                if (success)
                {
                    successfulRecords++;
                }
                else
                {
                    reviewRecords++;
                }
            }
            //Execution Summary Creation
            Console.WriteLine();
            Console.WriteLine("Run summary:");
            Console.WriteLine($"Total records: {records.Count}");
            Console.WriteLine($"Successful records: {successfulRecords}");
            Console.WriteLine($"Records needing review: {reviewRecords}");
            Console.WriteLine($"Screenshots folder: {screenshotsDir}");

            Console.WriteLine("Processing finished.");
            Console.WriteLine("Press ENTER to close browser...");
            Console.ReadLine();
        }
        finally
        {
            driver.Quit();
        }
    }

    static bool ProcessRecord(
        IWebDriver driver,
        CommandExecutor executor,
        TableHelper tableHelper,
        string screenshotsDir,
        PersonRecord record)
    {
        Console.WriteLine($"Processing record: {record.FirstName} {record.LastName}");

        bool addSuccess = AddRecordFlow(driver, executor, tableHelper, screenshotsDir, record);

        if (!addSuccess)
        {
            return false;
        }

        return EditAndDeleteFlow(driver, executor, tableHelper, screenshotsDir, record);
    }

    static bool AddRecordFlow(
        IWebDriver driver,
        CommandExecutor executor,
        TableHelper tableHelper,
        string screenshotsDir,
        PersonRecord record)
    {
        executor.OpenAddRecordModal();
        ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"01_add_clicked_{record.FirstName}_{record.LastName}");

        executor.FillRegistrationForm(record);
        ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"02_form_filled_{record.FirstName}_{record.LastName}");

        executor.SubmitRegistrationForm();
        Thread.Sleep(2000);

        ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"03_form_submitted_{record.FirstName}_{record.LastName}");

        if (HasInvalidForm(executor))
        {
            HandleInvalidForm(
                executor,
                driver,
                screenshotsDir,
                record,
                "04_invalid_add",
                $"Record for {record.FirstName} {record.LastName} needs to be reviewed."
            );

            return false;
        }

        tableHelper.WaitForTableRowData(record.FirstName, record.LastName);

        ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"06_row_added_{record.FirstName}_{record.LastName}");

        Console.WriteLine($"Row for {record.FirstName} {record.LastName} was added successfully.");

        return true;
    }

    static bool EditAndDeleteFlow(
        IWebDriver driver,
        CommandExecutor executor,
        TableHelper tableHelper,
        string screenshotsDir,
        PersonRecord record)
    {
        Console.WriteLine($"Trying to click Edit for: {record.FirstName} {record.LastName}");

        executor.ClickEditButton(record.FirstName, record.LastName);
        Thread.Sleep(1500);

        ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"07_edit_clicked_{record.FirstName}_{record.LastName}");

        executor.FillRegistrationForm(new PersonRecord
        {
            FirstName = record.FirstName,
            LastName = record.LastName,
            Email = record.Email,
            Age = record.Age,
            Salary = record.UpdatedSalary,
            Department = record.Department
        });

        ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"08_salary_updated_{record.FirstName}_{record.LastName}");

        executor.SubmitRegistrationForm();
        Thread.Sleep(2000);

        ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"09_edit_submitted_{record.FirstName}_{record.LastName}");

        if (HasInvalidForm(executor))
        {
            HandleInvalidForm(
                executor,
                driver,
                screenshotsDir,
                record,
                "10_invalid_edit",
                $"Updated values for {record.FirstName} {record.LastName} need to be reviewed."
            );

            CleanupAfterFailedEdit(driver, executor, tableHelper, screenshotsDir, record);
            return false;
        }

        tableHelper.WaitForTableRowData(record.FirstName, record.LastName, record.UpdatedSalary);

        ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"12_row_updated_{record.FirstName}_{record.LastName}");

        Console.WriteLine($"Row for {record.FirstName} {record.LastName} was updated successfully.");

        // DELETE
        executor.ClickDeleteButton(record.FirstName, record.LastName);
        Thread.Sleep(1500);

        tableHelper.WaitForTableRowDeletion(record.FirstName, record.LastName);
        Thread.Sleep(1500);

        ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"13_row_deleted_{record.FirstName}_{record.LastName}");

        Console.WriteLine($"Row for {record.FirstName} {record.LastName} was deleted successfully.");

        return true;
    }

    static bool HasInvalidForm(CommandExecutor executor)
    {
        return executor.IsRegistrationModalOpen() && executor.HasInvalidFields();
    }

    static void HandleInvalidForm(
        CommandExecutor executor,
        IWebDriver driver,
        string screenshotsDir,
        PersonRecord record,
        string screenshotPrefix,
        string message)
    {
        ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"{screenshotPrefix}_review_needed_{record.FirstName}_{record.LastName}");

        Console.WriteLine(message);

        try
        {
            executor.CloseRegistrationForm();
            Thread.Sleep(1500);
            ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"{screenshotPrefix}_modal_closed_{record.FirstName}_{record.LastName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not close modal: {ex.Message}");
        }
    }

    static void CleanupAfterFailedEdit(
        IWebDriver driver,
        CommandExecutor executor,
        TableHelper tableHelper,
        string screenshotsDir,
        PersonRecord record)
    {
        try
        {
            Console.WriteLine($"Cleaning up row after failed edit: {record.FirstName} {record.LastName}");

            executor.ClickDeleteButton(record.FirstName, record.LastName);
            Thread.Sleep(1500);

            tableHelper.WaitForTableRowDeletion(record.FirstName, record.LastName);
            Thread.Sleep(1500);

            ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"11_cleanup_deleted_{record.FirstName}_{record.LastName}");

            Console.WriteLine($"Cleanup completed for {record.FirstName} {record.LastName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cleanup failed: {ex.Message}");
        }
    }
}