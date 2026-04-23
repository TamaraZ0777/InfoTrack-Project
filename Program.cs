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

        string projectRoot = AppDomain.CurrentDomain.BaseDirectory;
        string screenshotsRoot = Path.GetFullPath(
            Path.Combine(projectRoot, @"..\..\..\Screenshots")
        );

        string runFolderName = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string screenshotsDir = Path.Combine(screenshotsRoot, runFolderName);

        if (!Directory.Exists(screenshotsDir))
        {
            Directory.CreateDirectory(screenshotsDir);
        }

        Console.WriteLine($"Screenshots folder: {screenshotsDir}");

        IWebDriver driver = new ChromeDriver();

        try
        {
            driver.Navigate().GoToUrl(scenario.Url);

            var executor = new CommandExecutor(driver);
            var tableHelper = new TableHelper(driver);

            foreach (var record in records)
            {
                Console.WriteLine($"Processing record: {record.FirstName} {record.LastName}");

                executor.OpenAddRecordModal();
                SaveScreenshot(driver, screenshotsDir, $"01_add_clicked_{record.FirstName}_{record.LastName}");

                executor.FillRegistrationForm(record);
                SaveScreenshot(driver, screenshotsDir, $"02_form_filled_{record.FirstName}_{record.LastName}");

                executor.SubmitRegistrationForm();
                Console.WriteLine("Current URL after submit: " + driver.Url);
                Console.WriteLine("Current page title after submit: " + driver.Title);
                Thread.Sleep(2000);
                SaveScreenshot(driver, screenshotsDir, $"03_form_submitted_{record.FirstName}_{record.LastName}");

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

                    continue;
                }

                Console.WriteLine($"Looking for row: {record.FirstName} {record.LastName}");
                Thread.Sleep(3000);

                tableHelper.PrintCurrentPageText();
                tableHelper.WaitForTableRowData(record.FirstName, record.LastName);

                SaveScreenshot(driver, screenshotsDir, $"06_row_added_{record.FirstName}_{record.LastName}");

                Console.WriteLine(
                    $"Row for {record.FirstName} {record.LastName} was added successfully."
                );

                // EDIT FLOW
                executor.ClickEditButton(record.FirstName, record.LastName);
                Thread.Sleep(1500);
                SaveScreenshot(driver, screenshotsDir, $"07_edit_clicked_{record.FirstName}_{record.LastName}");

                executor.FillRegistrationForm(new PersonRecord
                {
                    FirstName = record.FirstName,
                    LastName = record.LastName,
                    Email = record.Email,
                    Age = record.Age,
                    Salary = record.UpdatedSalary,
                    Department = record.Department
                });

                SaveScreenshot(driver, screenshotsDir, $"08_salary_updated_{record.FirstName}_{record.LastName}");

                executor.SubmitRegistrationForm();
                Thread.Sleep(2000);
                SaveScreenshot(driver, screenshotsDir, $"09_edit_submitted_{record.FirstName}_{record.LastName}");

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

                    // cleanup: delete the row that was added before the failed edit
                    try
                    {
                        Console.WriteLine($"Cleaning up added row after failed edit: {record.FirstName} {record.LastName}");

                        executor.ClickDeleteButton(record.FirstName, record.LastName);
                        Thread.Sleep(1500);

                        tableHelper.WaitForTableRowDeletion(record.FirstName, record.LastName);
                        Thread.Sleep(1500);

                        SaveScreenshot(driver, screenshotsDir, $"12_cleanup_deleted_after_invalid_edit_{record.FirstName}_{record.LastName}");

                        Console.WriteLine(
                            $"Cleanup completed for {record.FirstName} {record.LastName}."
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not clean up row after invalid edit: {ex.Message}");
                    }

                    continue;
                }

                tableHelper.WaitForTableRowData(record.FirstName, record.LastName, record.UpdatedSalary);

                SaveScreenshot(driver, screenshotsDir, $"12_row_updated_{record.FirstName}_{record.LastName}");

                Console.WriteLine(
                    $"Row for {record.FirstName} {record.LastName} was updated successfully."
                );

                // DELETE FLOW
                executor.ClickDeleteButton(record.FirstName, record.LastName);
                Thread.Sleep(1500);

                tableHelper.WaitForTableRowDeletion(record.FirstName, record.LastName);

                // small extra delay so UI fully refreshes before screenshot
                Thread.Sleep(1500);

                tableHelper.PrintCurrentPageText();
                SaveScreenshot(driver, screenshotsDir, $"10_row_deleted_{record.FirstName}_{record.LastName}");

                Console.WriteLine(
                    $"Row for {record.FirstName} {record.LastName} was deleted successfully."
                );
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
        SaveScreenshot(driver, screenshotsDir, $"{screenshotPrefix}_review_needed_{record.FirstName}_{record.LastName}");

        Console.WriteLine(message);

        try
        {
            executor.CloseRegistrationForm();
            Thread.Sleep(1500);
            SaveScreenshot(driver, screenshotsDir, $"{screenshotPrefix}_modal_closed_{record.FirstName}_{record.LastName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not close modal: {ex.Message}");
        }
    }

    static void SaveScreenshot(IWebDriver driver, string folderPath, string fileName)
    {
        Thread.Sleep(1500);

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        string fullPath = Path.Combine(folderPath, $"{fileName}_{timestamp}.png");

        var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
        screenshot.SaveAsFile(fullPath);

        Console.WriteLine($"Screenshot saved: {fullPath}");
    }
}