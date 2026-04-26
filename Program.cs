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

        var csvResult = CsvReader.ReadData(scenario.DataFile);
        var records = csvResult.Records;
        var csvWarnings = csvResult.Warnings;

        if (records.Count == 0)
        {
            throw new Exception("No records found in data.csv");
        }

        if (csvWarnings.Count > 0)
        {
            Console.WriteLine("CSV warnings:");
            foreach (var warning in csvWarnings)
            {
                Console.WriteLine($"- {warning}");
            }

            Console.WriteLine();
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
                bool success = ProcessRecord(driver, executor, tableHelper, screenshotsDir, record, scenario);

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
            Console.WriteLine($"CSV warnings: {csvWarnings.Count}");

            if (csvWarnings.Count > 0)
            {
                Console.WriteLine("CSV warning details:");
                foreach (var warning in csvWarnings)
                {
                    Console.WriteLine($"- {warning}");
                }
            }

            Console.WriteLine("Processing finished.");
            Console.WriteLine("Press ENTER to close browser...");
            Console.ReadLine();
        }
        finally
        {
            driver.Quit();
        }
    }

    static bool ExecuteScenarioStep(
        string action,
        IWebDriver driver,
        CommandExecutor executor,
        TableHelper tableHelper,
        string screenshotsDir,
        PersonRecord record)
    {
        switch (action)
        {
            case "addRecord":
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

                return true;

            case "waitForTableRowData":
                tableHelper.WaitForTableRowData(record.FirstName, record.LastName);
                ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"05_wait_row_data_{record.FirstName}_{record.LastName}");
                return true;

            case "verifyInsertedRow":
                if (!tableHelper.IsTableRowDataPresent(record.FirstName, record.LastName))
                {
                    throw new Exception($"Inserted row was not found: {record.FirstName} {record.LastName}");
                }

                Console.WriteLine($"Inserted row verified: {record.FirstName} {record.LastName}");
                return true;

            case "editSalary":
                Console.WriteLine($"Trying to click Edit for: {record.FirstName} {record.LastName}");

                executor.ClickEditButton(record.FirstName, record.LastName);
                Thread.Sleep(1500);
                ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"06_edit_clicked_{record.FirstName}_{record.LastName}");

                executor.FillRegistrationForm(new PersonRecord
                {
                    FirstName = record.FirstName,
                    LastName = record.LastName,
                    Email = record.Email,
                    Age = record.Age,
                    Salary = record.UpdatedSalary,
                    Department = record.Department
                });

                ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"07_salary_updated_{record.FirstName}_{record.LastName}");

                executor.SubmitRegistrationForm();
                Thread.Sleep(2000);
                ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"08_edit_submitted_{record.FirstName}_{record.LastName}");

                if (HasInvalidForm(executor))
                {
                    HandleInvalidForm(
                        executor,
                        driver,
                        screenshotsDir,
                        record,
                        "09_invalid_edit",
                        $"Updated values for {record.FirstName} {record.LastName} need to be reviewed."
                    );

                    CleanupAfterFailedEdit(driver, executor, tableHelper, screenshotsDir, record);
                    return false;
                }

                return true;

            case "verifyUpdatedSalary":
                if (!tableHelper.IsTableRowDataPresent(record.FirstName, record.LastName, record.UpdatedSalary))
                {
                    throw new Exception($"Updated salary was not found for {record.FirstName} {record.LastName}");
                }

                Console.WriteLine($"Updated salary verified for: {record.FirstName} {record.LastName}");
                return true;

            case "deleteRow":
                int previousCount = tableHelper.CountValueOccurrences(record.Email);

                executor.ClickDeleteButton(record.FirstName, record.LastName);
                Thread.Sleep(1500);

                record.PreviousEmailCount = previousCount;

                return true;

            case "waitForTableRowDeletion":
                tableHelper.WaitForTableRowDeletion(record.Email, record.PreviousEmailCount);
                Thread.Sleep(1500);
                ScreenshotService.SaveScreenshot(driver, screenshotsDir, $"10_row_deleted_{record.FirstName}_{record.LastName}");
                return true;

            case "verifyRowDeleted":
                if (tableHelper.CountValueOccurrences(record.Email) >= record.PreviousEmailCount)
                {
                    throw new Exception($"Row was not deleted: {record.Email}");
                }

                Console.WriteLine($"Row deleted verified: {record.FirstName} {record.LastName}");
                return true;

            default:
                throw new Exception($"Unknown JSON action: {action}");
        }
    }

    static bool ProcessRecord(
        IWebDriver driver,
        CommandExecutor executor,
        TableHelper tableHelper,
        string screenshotsDir,
        PersonRecord record,
        TestScenario scenario)
    {
        Console.WriteLine($"Processing record: {record.FirstName} {record.LastName}");

        foreach (var step in scenario.Steps)
        {
            bool shouldContinue = ExecuteScenarioStep(
                step.Action,
                driver,
                executor,
                tableHelper,
                screenshotsDir,
                record
            );

            if (!shouldContinue)
            {
                return false;
            }
        }

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

            int previousCount = tableHelper.CountValueOccurrences(record.Email);

            executor.ClickDeleteButton(record.FirstName, record.LastName);
            Thread.Sleep(1500);

            tableHelper.WaitForTableRowDeletion(record.Email, previousCount);
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