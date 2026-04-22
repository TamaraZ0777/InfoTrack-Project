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

        var record = records[0];

        //Creating a screenshot folder

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

        IWebDriver driver = new ChromeDriver();

        try
        {
            driver.Navigate().GoToUrl(scenario.Url);

            var executor = new CommandExecutor(driver);

            // Step 1: click Add
            //executor.ExecuteStep(new Step
            //{
                //Action = "click",
                //Selector = "#addNewRecordButton"
            //});
            executor.OpenAddRecordModal();

            Thread.Sleep(1500);
            SaveScreenshot(driver, screenshotsDir, "01_add_clicked");

            // Step 2: fill modal from first CSV row
            executor.FillRegistrationForm(record);

            Thread.Sleep(1000);
            SaveScreenshot(driver, screenshotsDir, "02_form_filled");

            // Step 3: submit
            executor.SubmitRegistrationForm();

            Thread.Sleep(1500);
            SaveScreenshot(driver, screenshotsDir, "03_form_submitted");

            //The part below is for testing one row. Update after testing finished

            if (executor.IsRegistrationModalOpen() && executor.HasInvalidFields())
            {
                SaveScreenshot(driver, screenshotsDir, "04_invalid_fields_review_needed");

                Console.WriteLine(
                    $"Record for {record.FirstName} {record.LastName} needs to be reviewed."
                );

                try
                {
                    executor.CloseRegistrationForm();
                    SaveScreenshot(driver, screenshotsDir, "05_modal_closed_after_invalid");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not close modal: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine(
                    $"Record for {record.FirstName} {record.LastName} submitted successfully."
                );
            }

            Console.WriteLine($"Submitted record: {record.FirstName} {record.LastName}");

            Console.WriteLine("Press ENTER to close browser...");
            Console.ReadLine();
        }
        finally
        {
            driver.Quit();
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